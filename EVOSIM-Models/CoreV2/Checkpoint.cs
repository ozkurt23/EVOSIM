using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
namespace EvoSim.Core
{
    public sealed partial class World
    {
        public const string CurrentCheckpointFormat = "EVOSIM-FOREST-WORLD-1";
        public const string V1CheckpointFormat = "EVOSIM-0.4-WORLD-2";
        public const string FixedMapCheckpointFormat = "EVOSIM-0.4-WORLD-1";
        public const string LegacyCheckpointFormat = "EVOSIM-0.2-WORLD-2";
        public string LoadedCheckpointFormat { get; private set; } = CurrentCheckpointFormat;
        private const int MaximumCheckpointBytes = 512 * 1024 * 1024;
        private const int MaximumRecords = 1000000;
        private const int MaximumStringBytes = 65536;
        private static readonly UTF8Encoding CheckpointEncoding = new UTF8Encoding(false, true);
        private static readonly string[] RngNames = { "founders", "exploration", "inheritance", "mutation", "brainFounders", "brainInheritance" };

        // A new field is not silently included by reflection. Its explicit name, wire type,
        // and migration default must be added to the schema and checkpoint version policy.
        public void Save(Stream stream)
        {
            if (stream == null || !stream.CanWrite) throw new ArgumentException("Yazılabilir kayıt akışı gerekli.", nameof(stream));
            ValidateCheckpointState(this);ValidateForestClock();
            using (var payload = new MemoryStream())
            {
                using (var w = new BinaryWriter(payload, CheckpointEncoding, true))
                {
                    WriteRecords(w, new List<WorldConfig> { Config });
                    WriteHeaderState(w);
                    var rngs = Rngs(); w.Write(rngs.Length);
                    for (int i = 0; i < rngs.Length; i++) { w.Write(RngNames[i]); w.Write(rngs[i].State); w.Write(rngs[i].Increment); }
                    w.Write(Map.Size); w.Write(Map.WaterLevel); WriteArray(w, Map.Food); WriteArray(w, Map.Moisture);
                    WriteRecords(w, Map.Trees);
                    WriteRecords(w, Creatures); WriteRecords(w, Corpses); WriteRecords(w, Events); WriteRecords(w, History);
                    byte[] forest=Forest.Save();w.Write(ForestOriginDay);w.Write(forest.Length);w.Write(forest);
                }
                if (payload.Length > MaximumCheckpointBytes) throw new InvalidDataException("Dünya kaydı bu sürümün 512 MiB sınırını aşıyor.");
                byte[] data = payload.ToArray(), digest;
                using (var sha = SHA256.Create()) digest = sha.ComputeHash(data);
                using (var w = new BinaryWriter(stream, CheckpointEncoding, true))
                {
                    w.Write(CurrentCheckpointFormat); w.Write(data.Length); w.Write(digest); w.Write(data);
                }
            }
        }

        public static World Load(Stream stream)=>LoadVersion(stream,false);
        public static World ImportV1Origin(Stream stream)=>LoadVersion(stream,true);
        private static World LoadVersion(Stream stream,bool importV1)
        {
            if (stream == null || !stream.CanRead) throw new ArgumentException("Okunabilir kayıt akışı gerekli.", nameof(stream));
            try
            {
                using (var r = new BinaryReader(stream, CheckpointEncoding, true))
                {
                    string format = ReadString(r, 128);
                    World world;
                    if ((!importV1 && format == CurrentCheckpointFormat) || (importV1 && format == V1CheckpointFormat))
                    {
                        int length = ReadCount(r, MaximumCheckpointBytes, "kayıt gövdesi");
                        if (length < 64) throw new InvalidDataException("Kayıt gövdesi eksik.");
                        EnsureAvailable(r, (long)length + 32);
                        byte[] digest = ReadExact(r, 32), data = ReadExact(r, length), actual;
                        using (var sha = SHA256.Create()) actual = sha.ComputeHash(data);
                        if (!digest.SequenceEqual(actual)) throw new InvalidDataException("Dünya kaydının bütünlük kontrolü başarısız (SHA-256).");
                        using (var payload = new MemoryStream(data, false))
                        using (var body = new BinaryReader(payload, CheckpointEncoding, true))
                        {
                            world = ReadCurrentWorld(body, false);
                            if(importV1)world.InitializeForest();
                            else { long originDay=body.ReadInt64();world.RestoreForest(ReadExact(body,ReadCount(body,EvoSim.Ecology.Forest.MaximumBytes,"orman kaydı")),originDay); }
                            if (payload.Position != payload.Length) throw new InvalidDataException("Dünya kaydında beklenmeyen ek veri var.");
                        }
                    }
                    
                    else throw new InvalidDataException("Desteklenmeyen EVOSIM dünya kayıt sürümü: " + format);
                    if (stream.CanSeek && stream.Position != stream.Length) throw new InvalidDataException("Dünya kaydının sonunda beklenmeyen ek veri var.");
                    ValidateCheckpointState(world); world.LoadedCheckpointFormat = format; return world;
                }
            }
            catch (EndOfStreamException ex) { throw new InvalidDataException("Dünya kaydı tamamlanmamış veya kesilmiş.", ex); }
            catch (DecoderFallbackException ex) { throw new InvalidDataException("Dünya kaydındaki metin UTF-8 biçiminde değil.", ex); }
            catch (ArgumentException ex) { throw new InvalidDataException("Dünya kaydındaki ayar veya alan geçersiz: " + ex.Message, ex); }
            catch (OverflowException ex) { throw new InvalidDataException("Dünya kaydındaki boyut sınırı aşılıyor.", ex); }
        }

        private void WriteHeaderState(BinaryWriter w)
        {
            w.Write(Seed); w.Write(Tick); w.Write(Drought); w.Write(Births); w.Write(Deaths); w.Write(Mutations); w.Write(_nextId);
        }
        private static World ReadHeaderState(BinaryReader r, WorldConfig config)
        {
            config.Validate(); var world = new World(r.ReadInt32(), config, true);
            world.Tick = r.ReadInt64(); world.Drought = ReadBoolean(r); world.Births = r.ReadInt32(); world.Deaths = r.ReadInt32(); world.Mutations = r.ReadInt32(); world._nextId = r.ReadInt32();
            return world;
        }
        private static World ReadCurrentWorld(BinaryReader r, bool fixedMap)
        {
            var configs = ReadRecords<WorldConfig>(r, false, 1, fixedMap);
            if (configs.Count != 1) throw new InvalidDataException("Dünya tam bir ayar kaydı içermeli.");
            if (fixedMap) ApplyLegacyMapDefaults(configs[0]);
            var world = ReadHeaderState(r, configs[0]);
            var rngs = world.Rngs();
            if (ReadCount(r, 32, "rastgelelik akışı") != rngs.Length) throw new InvalidDataException("Rastgelelik akışlarının sayısı eşleşmiyor.");
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < rngs.Length; i++)
            {
                string name = ReadString(r, 128); int index = Array.IndexOf(RngNames, name);
                if (index < 0 || !seen.Add(name)) throw new InvalidDataException("Bilinmeyen veya yinelenmiş rastgelelik akışı: " + name);
                rngs[index].Restore(r.ReadUInt64(), r.ReadUInt64());
            }
            if (r.ReadInt32() != world.Config.MapSize) throw new InvalidDataException("Kaydın harita boyutu dünya ayarlarıyla eşleşmiyor.");
            world.Map.WaterLevel = ReadFiniteSingle(r);
            world.Map.Food = ReadArray(r, world.Map.CellCount); world.Map.Moisture = ReadArray(r, world.Map.CellCount);
            // Tree positions, dimensions and fruit placement are stored, never inferred from
            // today's constructor defaults. The remaining terrain is versioned procedural state.
            world.Map.Trees = ReadRecords<Tree>(r, false, 100000);
            world.RebuildResourceIndex();
            ReadLifeRecords(r, world, false); return world;
        }
        private static World ReadLegacyWorld(BinaryReader r)
        {
            // WORLD-2 wrote untyped names followed by values. All original fields remain
            // mandatory; only documented v0.4 and v0.4.1 additions get defaults.
            var configs = ReadRecords<WorldConfig>(r, true, 1);
            if (configs.Count != 1) throw new InvalidDataException("Eski dünya tam bir ayar kaydı içermeli.");
            ApplyLegacyConfigDefaults(configs[0]);
            var world = ReadHeaderState(r, configs[0]);
            foreach (var rng in world.Rngs()) rng.Restore(r.ReadUInt64(), r.ReadUInt64());
            world.Map.WaterLevel = ReadFiniteSingle(r);
            world.Map.Food = ReadArray(r, world.Map.CellCount); world.Map.Moisture = ReadArray(r, world.Map.CellCount);
            if (ReadCount(r, 100000, "ağaç") != world.Map.Trees.Count) throw new InvalidDataException("Eski kaydın ağaç yerleşimi yeniden üretilemedi.");
            foreach (var tree in world.Map.Trees)
            {
                tree.FruitRadius = .8f; tree.Fruit = ReadArray(r, world.Config.FruitSlots); tree.Regrow = ReadArray(r, world.Config.FruitSlots);
            }
            world.RebuildResourceIndex();
            ReadLifeRecords(r, world, true); return world;
        }
        private static void ApplyLegacyMapDefaults(WorldConfig c)
        {
            c.MapSize = 128; c.GeneratorVersion = 1;
        }
        private static void ApplyLegacyConfigDefaults(WorldConfig c)
        {
            ApplyLegacyMapDefaults(c);
            c.TreeRadiusMin = 1.35f; c.TreeRadiusMax = 2.45f; c.TreeSpacing = 3f; c.TreeMoistureAffinity = .66f; c.TreeFruitRadius = .8f; c.FruitDroughtTolerance = .2f;
            c.HerbBreedingMode = 1; c.CarnBreedingMode = 1; c.HerbReproductiveDrive = .5f; c.CarnReproductiveDrive = .5f; c.BreedContactRange = .8f;
        }
        private static void ReadLifeRecords(BinaryReader r, World world, bool legacy)
        {
            world.Creatures = ReadRecords<Creature>(r, legacy); world.Corpses = ReadRecords<Corpse>(r, legacy);
            world.Events = ReadRecords<SimEvent>(r, legacy); world.History = ReadRecords<HistorySample>(r, legacy);
        }
        private PcgRandom[] Rngs() { return new[] { _founders, _exploration, _inheritance, _mutation, _brainFounders, _brainInheritance }; }

        private enum WireKind : byte { Int32 = 1, Int64 = 2, Float32 = 3, Float64 = 4, Boolean = 5, String = 6, FloatArray = 7, VectorList = 8 }
        private sealed class CheckpointField
        {
            public readonly string Name; public readonly FieldInfo Field; public readonly WireKind Kind; public readonly int ArrayLength;
            public CheckpointField(Type owner, string name, WireKind kind, int arrayLength = -1)
            {
                Name = name; Kind = kind; ArrayLength = arrayLength;
                Field = owner.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Type expected = kind == WireKind.Int32 ? typeof(int) : kind == WireKind.Int64 ? typeof(long) : kind == WireKind.Float32 ? typeof(float) : kind == WireKind.Float64 ? typeof(double) : kind == WireKind.Boolean ? typeof(bool) : kind == WireKind.String ? typeof(string) : kind == WireKind.FloatArray ? typeof(float[]) : typeof(List<Vector2>);
                if (Field == null || Field.FieldType != expected) throw new InvalidDataException("Uygulamanın kayıt şeması eksik veya türü uyumsuz: " + owner.Name + "." + name);
            }
            public int MinimumBytes { get { return Kind == WireKind.Int64 || Kind == WireKind.Float64 ? 8 : Kind == WireKind.Boolean || Kind == WireKind.String ? 1 : 4; } }
        }
        private static class CheckpointSchema<T>
        {
            public static readonly CheckpointField[] Fields = ValidateSchema(typeof(T), MakeSchema(typeof(T)));
        }
        private static CheckpointField[] ValidateSchema(Type type, CheckpointField[] fields)
        {
            var actual = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (actual.Length != fields.Length || actual.Any(f => !fields.Any(s => s.Name == f.Name))) throw new InvalidDataException("Modelde kayda alınmamış yeni alan var: " + type.Name + ". Kayıt şeması açıkça güncellenmeli.");
            return fields;
        }
        private static CheckpointField[] MakeSchema(Type type)
        {
            switch (type.Name)
            {
                case "WorldConfig": return new[]{
                    new CheckpointField(type,"Acceleration", WireKind.Float32),
                    new CheckpointField(type,"AquaticCost", WireKind.Float32),
                    new CheckpointField(type,"BasalCost", WireKind.Float32),
                    new CheckpointField(type,"BaseSpeed", WireKind.Float32),
                    new CheckpointField(type,"BirthCooldown", WireKind.Float32),
                    new CheckpointField(type,"BirthEfficiency", WireKind.Float32),
                    new CheckpointField(type,"BiteCooldown", WireKind.Float32),
                    new CheckpointField(type,"BiteCost", WireKind.Float32),
                    new CheckpointField(type,"BiteDamage", WireKind.Float32),
                    new CheckpointField(type,"BrainMutationScale", WireKind.Float32),
                    new CheckpointField(type,"BrainRange", WireKind.Float32),
                    new CheckpointField(type,"BreedContactRange", WireKind.Float32),
                    new CheckpointField(type,"BreedEnergy", WireKind.Float32),
                    new CheckpointField(type,"BreedHealth", WireKind.Float32),
                    new CheckpointField(type,"BreedWater", WireKind.Float32),
                    new CheckpointField(type,"CarnAging", WireKind.Int32),
                    new CheckpointField(type,"CarnBreedingMode", WireKind.Int32),
                    new CheckpointField(type,"CarnLifespan", WireKind.Float32),
                    new CheckpointField(type,"CarnMax", WireKind.FloatArray, 6),
                    new CheckpointField(type,"CarnMin", WireKind.FloatArray, 6),
                    new CheckpointField(type,"CarnReproductiveDrive", WireKind.Float32),
                    new CheckpointField(type,"Carnivores", WireKind.Int32),
                    new CheckpointField(type,"ChildEnergy", WireKind.Float32),
                    new CheckpointField(type,"ChildWater", WireKind.Float32),
                    new CheckpointField(type,"CorpseDecay", WireKind.Float32),
                    new CheckpointField(type,"CorpseLifetime", WireKind.Float32),
                    new CheckpointField(type,"CriticalBoost", WireKind.Float32),
                    new CheckpointField(type,"CriticalNeed", WireKind.Float32),
                    new CheckpointField(type,"DeclineAge", WireKind.Float32),
                    new CheckpointField(type,"DrinkRate", WireKind.Float32),
                    new CheckpointField(type,"DryWater", WireKind.Float32),
                    new CheckpointField(type,"FounderAgeMax", WireKind.Float32),
                    new CheckpointField(type,"FounderAgeMin", WireKind.Float32),
                    new CheckpointField(type,"FruitBiteRate", WireKind.Float32),
                    new CheckpointField(type,"FruitDroughtTolerance", WireKind.Float32),
                    new CheckpointField(type,"FruitEnergy", WireKind.Float32),
                    new CheckpointField(type,"FruitRegrow", WireKind.Float32),
                    new CheckpointField(type,"FruitRegrowStep", WireKind.Float32),
                    new CheckpointField(type,"FruitSlots", WireKind.Int32),
                    new CheckpointField(type,"GeneratorVersion", WireKind.Int32),
                    new CheckpointField(type,"GrassBiteRate", WireKind.Float32),
                    new CheckpointField(type,"GrassCapacity", WireKind.Float32),
                    new CheckpointField(type,"GrassRegrowth", WireKind.Float32),
                    new CheckpointField(type,"GrowthEfficiency", WireKind.Float32),
                    new CheckpointField(type,"GrowthRate", WireKind.Float32),
                    new CheckpointField(type,"HealingRate", WireKind.Float32),
                    new CheckpointField(type,"HerbAging", WireKind.Int32),
                    new CheckpointField(type,"HerbBreedingMode", WireKind.Int32),
                    new CheckpointField(type,"HerbLifespan", WireKind.Float32),
                    new CheckpointField(type,"HerbMax", WireKind.FloatArray, 6),
                    new CheckpointField(type,"HerbMin", WireKind.FloatArray, 6),
                    new CheckpointField(type,"HerbReproductiveDrive", WireKind.Float32),
                    new CheckpointField(type,"Herbivores", WireKind.Int32),
                    new CheckpointField(type,"HungerOnset", WireKind.Float32),
                    new CheckpointField(type,"InitialCooldownMax", WireKind.Float32),
                    new CheckpointField(type,"InitialCooldownMin", WireKind.Float32),
                    new CheckpointField(type,"InitialEnergy", WireKind.Float32),
                    new CheckpointField(type,"InitialWaterMax", WireKind.Float32),
                    new CheckpointField(type,"InitialWaterMin", WireKind.Float32),
                    new CheckpointField(type,"LandRegions", WireKind.Int32),
                    new CheckpointField(type,"LargeChance", WireKind.Float32),
                    new CheckpointField(type,"LargeSigma", WireKind.Float32),
                    new CheckpointField(type,"MapSize", WireKind.Int32),
                    new CheckpointField(type,"MassCost", WireKind.Float32),
                    new CheckpointField(type,"MaturityAge", WireKind.Float32),
                    new CheckpointField(type,"MaxAge", WireKind.Float32),
                    new CheckpointField(type,"MeadowCount", WireKind.Int32),
                    new CheckpointField(type,"MeadowX", WireKind.Float32),
                    new CheckpointField(type,"MeadowY", WireKind.Float32),
                    new CheckpointField(type,"MoveCost", WireKind.Float32),
                    new CheckpointField(type,"MoveWater", WireKind.Float32),
                    new CheckpointField(type,"MutationRate", WireKind.Float32),
                    new CheckpointField(type,"OldAgility", WireKind.Float32),
                    new CheckpointField(type,"OldHealing", WireKind.Float32),
                    new CheckpointField(type,"OldSpeed", WireKind.Float32),
                    new CheckpointField(type,"PerceptionCost", WireKind.Float32),
                    new CheckpointField(type,"RiverWidth", WireKind.Float32),
                    new CheckpointField(type,"RockClusters", WireKind.Int32),
                    new CheckpointField(type,"SmallSigma", WireKind.Float32),
                    new CheckpointField(type,"SwimCost", WireKind.Float32),
                    new CheckpointField(type,"SwimmerChance", WireKind.Float32),
                    new CheckpointField(type,"ThirstOnset", WireKind.Float32),
                    new CheckpointField(type,"TreeCount", WireKind.Int32),
                    new CheckpointField(type,"TreeFruitRadius", WireKind.Float32),
                    new CheckpointField(type,"TreeMoistureAffinity", WireKind.Float32),
                    new CheckpointField(type,"TreeRadiusMax", WireKind.Float32),
                    new CheckpointField(type,"TreeRadiusMin", WireKind.Float32),
                    new CheckpointField(type,"TreeSpacing", WireKind.Float32),
                    new CheckpointField(type,"TurnRate", WireKind.Float32),
                    new CheckpointField(type,"WaterDrain", WireKind.Float32)
                };
                case "Creature": return new[]{
                    new CheckpointField(type,"Action", WireKind.String),
                    new CheckpointField(type,"ActionAge", WireKind.Float32),
                    new CheckpointField(type,"ActionScore", WireKind.Float32),
                    new CheckpointField(type,"AdultTissue", WireKind.Float32),
                    new CheckpointField(type,"Age", WireKind.Float32),
                    new CheckpointField(type,"Alive", WireKind.Boolean),
                    new CheckpointField(type,"Angle", WireKind.Float32),
                    new CheckpointField(type,"AttackCooldown", WireKind.Float32),
                    new CheckpointField(type,"BlockedAge", WireKind.Float32),
                    new CheckpointField(type,"BlockedSeconds", WireKind.Float32),
                    new CheckpointField(type,"BrainMemory", WireKind.FloatArray, 8),
                    new CheckpointField(type,"BrainWeights", WireKind.FloatArray, 357),
                    new CheckpointField(type,"CorpseIndex", WireKind.Int32),
                    new CheckpointField(type,"DeathCause", WireKind.String),
                    new CheckpointField(type,"DeathTime", WireKind.Float64),
                    new CheckpointField(type,"DistanceTravelled", WireKind.Float32),
                    new CheckpointField(type,"Energy", WireKind.Float32),
                    new CheckpointField(type,"FoodCell", WireKind.Int32),
                    new CheckpointField(type,"Generation", WireKind.Int32),
                    new CheckpointField(type,"Genes", WireKind.FloatArray, 6),
                    new CheckpointField(type,"Group", WireKind.Int32),
                    new CheckpointField(type,"Health", WireKind.Float32),
                    new CheckpointField(type,"Hydration", WireKind.Float32),
                    new CheckpointField(type,"Id", WireKind.Int32),
                    new CheckpointField(type,"LastBirthCost", WireKind.Float32),
                    new CheckpointField(type,"LastSeenAge", WireKind.Float32),
                    new CheckpointField(type,"Maturity", WireKind.Float32),
                    new CheckpointField(type,"Motors", WireKind.FloatArray, 5),
                    new CheckpointField(type,"ParentA", WireKind.Int32),
                    new CheckpointField(type,"ParentB", WireKind.Int32),
                    new CheckpointField(type,"Path", WireKind.VectorList),
                    new CheckpointField(type,"PathAge", WireKind.Float32),
                    new CheckpointField(type,"PathIndex", WireKind.Int32),
                    new CheckpointField(type,"PreviousX", WireKind.Float32),
                    new CheckpointField(type,"PreviousY", WireKind.Float32),
                    new CheckpointField(type,"Radius", WireKind.Float32),
                    new CheckpointField(type,"Reason", WireKind.String),
                    new CheckpointField(type,"RecentDamage", WireKind.Float32),
                    new CheckpointField(type,"RepathX", WireKind.Float32),
                    new CheckpointField(type,"RepathY", WireKind.Float32),
                    new CheckpointField(type,"ReproductionCooldown", WireKind.Float32),
                    new CheckpointField(type,"Senses", WireKind.FloatArray, 30),
                    new CheckpointField(type,"Speed", WireKind.Float32),
                    new CheckpointField(type,"TargetId", WireKind.Int32),
                    new CheckpointField(type,"TargetX", WireKind.Float32),
                    new CheckpointField(type,"TargetY", WireKind.Float32),
                    new CheckpointField(type,"Tissue", WireKind.Float32),
                    new CheckpointField(type,"Vx", WireKind.Float32),
                    new CheckpointField(type,"Vy", WireKind.Float32),
                    new CheckpointField(type,"X", WireKind.Float32),
                    new CheckpointField(type,"Y", WireKind.Float32)
                };
                case "Corpse": return new[]{
                    new CheckpointField(type,"CreatedTick", WireKind.Int64),
                    new CheckpointField(type,"CreatureId", WireKind.Int32),
                    new CheckpointField(type,"Food", WireKind.Float32),
                    new CheckpointField(type,"X", WireKind.Float32),
                    new CheckpointField(type,"Y", WireKind.Float32)
                };
                case "SimEvent": return new[]{
                    new CheckpointField(type,"ChildEnergy", WireKind.Float32),
                    new CheckpointField(type,"ChildId", WireKind.Int32),
                    new CheckpointField(type,"EnergySpent", WireKind.Float32),
                    new CheckpointField(type,"Kind", WireKind.String),
                    new CheckpointField(type,"ParentA", WireKind.Int32),
                    new CheckpointField(type,"ParentB", WireKind.Int32),
                    new CheckpointField(type,"Text", WireKind.String),
                    new CheckpointField(type,"Time", WireKind.Float64)
                };
                case "HistorySample": return new[]{
                    new CheckpointField(type,"Births", WireKind.Int32),
                    new CheckpointField(type,"Carnivores", WireKind.Int32),
                    new CheckpointField(type,"Deaths", WireKind.Int32),
                    new CheckpointField(type,"Herbivores", WireKind.Int32),
                    new CheckpointField(type,"Time", WireKind.Float64)
                };
                case "Tree": return new[]{
                    new CheckpointField(type,"Fruit", WireKind.FloatArray),
                    new CheckpointField(type,"FruitRadius", WireKind.Float32),
                    new CheckpointField(type,"Radius", WireKind.Float32),
                    new CheckpointField(type,"Regrow", WireKind.FloatArray),
                    new CheckpointField(type,"X", WireKind.Float32),
                    new CheckpointField(type,"Y", WireKind.Float32)
                };
                default: throw new InvalidDataException("Desteklenmeyen kayıt modeli: " + type.Name);
            }
        }
        private static readonly HashSet<string> AddedMapConfigFields = new HashSet<string>(new[] { "MapSize", "GeneratorVersion" }, StringComparer.Ordinal);
        private static readonly HashSet<string> LegacyAddedConfigFields = new HashSet<string>(new[] {
            "TreeRadiusMin", "TreeRadiusMax", "TreeSpacing", "TreeMoistureAffinity", "TreeFruitRadius", "FruitDroughtTolerance", "HerbBreedingMode", "CarnBreedingMode", "HerbReproductiveDrive", "CarnReproductiveDrive", "BreedContactRange"
        }, StringComparer.Ordinal);

        private static void WriteRecords<T>(BinaryWriter w, List<T> records)
        {
            var fields = CheckpointSchema<T>.Fields;
            if (records == null || records.Count > MaximumRecords) throw new InvalidDataException("Geçersiz kayıt sayısı: " + typeof(T).Name);
            w.Write(typeof(T).Name); w.Write(fields.Length);
            foreach (var field in fields) { w.Write(field.Name); w.Write((byte)field.Kind); }
            w.Write(records.Count);
            foreach (var item in records)
            {
                if (item == null) throw new InvalidDataException("Boş model kaydı: " + typeof(T).Name);
                foreach (var field in fields) WriteValue(w, field, field.Field.GetValue(item));
            }
        }
        private static List<T> ReadRecords<T>(BinaryReader r, bool legacy, int maximum = MaximumRecords, bool fixedMap = false) where T : new()
        {
            var allFields = CheckpointSchema<T>.Fields;
            var required = allFields;
            if (typeof(T) == typeof(WorldConfig) && (legacy || fixedMap))
                required = allFields.Where(f => !AddedMapConfigFields.Contains(f.Name) && (!legacy || !LegacyAddedConfigFields.Contains(f.Name))).ToArray();
            if (!legacy && ReadString(r, 128) != typeof(T).Name) throw new InvalidDataException("Beklenen kayıt türü: " + typeof(T).Name);
            int countFields = ReadCount(r, 512, "alan");
            if (countFields != required.Length) throw new InvalidDataException(typeof(T).Name + " kayıt alan sayısı desteklenen şemayla eşleşmiyor.");
            var fields = new CheckpointField[countFields]; var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < countFields; i++)
            {
                string name = ReadString(r, 128); var field = required.FirstOrDefault(f => f.Name == name);
                if (field == null || !seen.Add(name)) throw new InvalidDataException("Bilinmeyen veya yinelenmiş alan: " + typeof(T).Name + "." + name);
                if (!legacy && r.ReadByte() != (byte)field.Kind) throw new InvalidDataException("Kayıt alan türü eşleşmiyor: " + typeof(T).Name + "." + name);
                fields[i] = field;
            }
            int count = ReadCount(r, maximum, typeof(T).Name);
            EnsureAvailable(r, (long)count * fields.Sum(f => f.MinimumBytes));
            var records = new List<T>(Math.Min(count, 8192));
            for (int i = 0; i < count; i++)
            {
                var item = new T(); foreach (var field in fields) field.Field.SetValue(item, ReadValue(r, field)); records.Add(item);
            }
            return records;
        }
        private static void WriteValue(BinaryWriter w, CheckpointField field, object value)
        {
            switch (field.Kind)
            {
                case WireKind.Int32: w.Write((int)value); break;
                case WireKind.Int64: w.Write((long)value); break;
                case WireKind.Float32: ValidateFloat((float)value, field); w.Write((float)value); break;
                case WireKind.Float64: if (!Finite((double)value)) throw new InvalidDataException("Geçersiz sayı: " + field.Name); w.Write((double)value); break;
                case WireKind.Boolean: w.Write((bool)value); break;
                case WireKind.String:
                    string text = (string)value ?? ""; if (CheckpointEncoding.GetByteCount(text) > MaximumStringBytes) throw new InvalidDataException("Kayıt metni çok uzun: " + field.Name); w.Write(text); break;
                case WireKind.FloatArray:
                    var array = (float[])value; if (array == null || field.ArrayLength >= 0 && array.Length != field.ArrayLength) throw new InvalidDataException("Kayıt dizisi eşleşmiyor: " + field.Name); WriteArray(w, array); break;
                case WireKind.VectorList:
                    var path = (List<Vector2>)value; if (path == null || path.Count > 100000) throw new InvalidDataException("Geçersiz yol uzunluğu."); w.Write(path.Count);
                    foreach (var point in path) { if (!Finite(point.X) || !Finite(point.Y)) throw new InvalidDataException("Geçersiz yol noktası."); w.Write(point.X); w.Write(point.Y); } break;
                default: throw new InvalidDataException("Desteklenmeyen alan türü: " + field.Name);
            }
        }
        private static object ReadValue(BinaryReader r, CheckpointField field)
        {
            switch (field.Kind)
            {
                case WireKind.Int32: return r.ReadInt32();
                case WireKind.Int64: return r.ReadInt64();
                case WireKind.Float32: float value = r.ReadSingle(); ValidateFloat(value, field); return value;
                case WireKind.Float64: double precise = r.ReadDouble(); if (!Finite(precise)) throw new InvalidDataException("Geçersiz sayı: " + field.Name); return precise;
                case WireKind.Boolean: return ReadBoolean(r);
                case WireKind.String: return ReadString(r, MaximumStringBytes);
                case WireKind.FloatArray: return ReadArray(r, field.ArrayLength);
                case WireKind.VectorList:
                    int count = ReadCount(r, 100000, "yol"); EnsureAvailable(r, (long)count * 8); var path = new List<Vector2>(Math.Min(count, 4096));
                    for (int i = 0; i < count; i++) path.Add(new Vector2(ReadFiniteSingle(r), ReadFiniteSingle(r))); return path;
                default: throw new InvalidDataException("Desteklenmeyen alan türü: " + field.Name);
            }
        }
        private static void ValidateFloat(float value, CheckpointField field)
        {
            // This retired route-cache field intentionally uses NaN as the never-planned sentinel.
            if (float.IsNaN(value) && field.Field.DeclaringType == typeof(Creature) && field.Name == "RepathX") return;
            if (!Finite(value)) throw new InvalidDataException("Geçersiz sayı: " + field.Name);
        }
        private static bool Finite(double value) { return !double.IsNaN(value) && !double.IsInfinity(value); }
        private static float ReadFiniteSingle(BinaryReader r) { float value = r.ReadSingle(); if (!Finite(value)) throw new InvalidDataException("Kayıtta sonlu olmayan sayı."); return value; }
        private static bool ReadBoolean(BinaryReader r) { byte value = r.ReadByte(); if (value > 1) throw new InvalidDataException("Kayıtta geçersiz açık/kapalı değeri."); return value == 1; }
        private static void WriteArray(BinaryWriter w, float[] array)
        {
            if (array == null || array.Length > 1000000) throw new InvalidDataException("Geçersiz dizi boyutu."); w.Write(array.Length);
            foreach (float value in array) { if (!Finite(value)) throw new InvalidDataException("Kayıt dizisinde sonlu olmayan sayı."); w.Write(value); }
        }
        private static float[] ReadArray(BinaryReader r, int expected = -1)
        {
            int count = ReadCount(r, 1000000, "dizi"); if (expected >= 0 && count != expected) throw new InvalidDataException("Kayıt dizisinin boyutu modelle eşleşmiyor.");
            EnsureAvailable(r, (long)count * 4); var result = new float[count]; for (int i = 0; i < count; i++) result[i] = ReadFiniteSingle(r); return result;
        }
        private static int ReadCount(BinaryReader r, int maximum, string label)
        {
            int count = r.ReadInt32(); if (count < 0 || count > maximum) throw new InvalidDataException("Geçersiz " + label + " sayısı/boyutu."); return count;
        }
        private static void EnsureAvailable(BinaryReader r, long bytes)
        {
            if (bytes < 0 || r.BaseStream.CanSeek && bytes > r.BaseStream.Length - r.BaseStream.Position) throw new InvalidDataException("Kayıt boyutu kalan veriyi aşıyor.");
        }
        private static byte[] ReadExact(BinaryReader r, int bytes)
        {
            var result = r.ReadBytes(bytes); if (result.Length != bytes) throw new EndOfStreamException(); return result;
        }
        private static string ReadString(BinaryReader r, int maximumBytes)
        {
            uint length = 0; int shift = 0;
            for (int i = 0; i < 5; i++)
            {
                byte value = r.ReadByte(); if (i == 4 && (value & 0xF8) != 0) throw new InvalidDataException("Geçersiz metin uzunluğu kodlaması.");
                length |= (uint)(value & 0x7F) << shift;
                if ((value & 0x80) == 0)
                {
                    if (length > maximumBytes) throw new InvalidDataException("Kayıt metni izin verilen boyutu aşıyor.");
                    EnsureAvailable(r, length); return CheckpointEncoding.GetString(ReadExact(r, (int)length));
                }
                shift += 7;
            }
            throw new InvalidDataException("Geçersiz metin uzunluğu.");
        }
        private static void ValidateCheckpointState(World world)
        {
            world.Config.Validate();
            if (world.Tick < 0 || world.Births < 0 || world.Deaths < 0 || world.Mutations < 0 || world._nextId < 1) throw new InvalidDataException("Geçersiz dünya zamanı veya sayacı.");
            if (world.Map.Size != world.Config.MapSize || world.Map.Food.Length != world.Map.CellCount || world.Map.Moisture.Length != world.Map.CellCount || !Finite(world.Map.WaterLevel) || world.Map.WaterLevel < 0) throw new InvalidDataException("Geçersiz arazi durumu.");
            if (world.Map.Food.Any(v => !Finite(v) || v < 0) || world.Map.Moisture.Any(v => !Finite(v) || v < 0 || v > 1)) throw new InvalidDataException("Geçersiz kaynak veya nem alanı.");
            var ids = new HashSet<int>();
            foreach (var c in world.Creatures)
            {
                if (c.Id <= 0 || c.Id >= world._nextId || !ids.Add(c.Id) || c.Group < 0 || c.Group > 1 || c.Generation < 0) throw new InvalidDataException("Geçersiz veya yinelenmiş canlı kimliği.");
                if (c.Genes.Length != 6 || c.BrainWeights.Length != Brain.Count || c.BrainMemory.Length != Brain.Hidden || c.Senses.Length != Brain.Inputs || c.Motors.Length != Brain.Outputs) throw new InvalidDataException("Geçersiz beden/beyin biçimi.");
                if (c.Genes.Any(g => !Finite(g) || g < 0 || g > 1) || !Finite(c.X) || !Finite(c.Y) || c.X < 0 || c.Y < 0 || c.X >= world.Map.Size || c.Y >= world.Map.Size || c.Age < 0 || c.Energy < 0 || c.Hydration < 0 || c.Tissue < 0) throw new InvalidDataException("Geçersiz canlı bedeni veya konumu.");
                if (c.PathIndex < 0 || c.PathIndex > c.Path.Count) throw new InvalidDataException("Geçersiz yol imleci.");
            }
            foreach (var c in world.Creatures) if (c.ParentA < 0 || c.ParentB < 0 || c.ParentA >= c.Id || c.ParentB >= c.Id || c.ParentA != 0 && !ids.Contains(c.ParentA) || c.ParentB != 0 && !ids.Contains(c.ParentB)) throw new InvalidDataException("Geçersiz soy bağlantısı.");
            foreach (var corpse in world.Corpses) if (!ids.Contains(corpse.CreatureId) || corpse.CreatedTick < 0 || corpse.CreatedTick > world.Tick || corpse.Food < 0) throw new InvalidDataException("Geçersiz ceset kaydı.");
            foreach (var tree in world.Map.Trees)
            {
                if (!Finite(tree.X) || !Finite(tree.Y) || !Finite(tree.Radius) || !Finite(tree.FruitRadius) || tree.X < 0 || tree.Y < 0 || tree.X >= world.Map.Size || tree.Y >= world.Map.Size || tree.Radius <= 0 || tree.FruitRadius < 0 || tree.FruitRadius > 1 || tree.Fruit == null || tree.Regrow == null || tree.Fruit.Length != world.Config.FruitSlots || tree.Regrow.Length != tree.Fruit.Length) throw new InvalidDataException("Geçersiz ağaç geometrisi veya meyve biçimi.");
                if (tree.Fruit.Any(v => !Finite(v) || v < 0 || v > world.Config.FruitEnergy + .0001f) || tree.Regrow.Any(v => !Finite(v) || v < 0)) throw new InvalidDataException("Geçersiz meyve stoğu veya yenilenme sayacı.");
            }
            foreach (var rng in world.Rngs()) if ((rng.Increment & 1) == 0) throw new InvalidDataException("Geçersiz rastgelelik akışı.");
        }
    }
}
