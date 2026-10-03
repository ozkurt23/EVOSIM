using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EvoSim.Core;

namespace EvoSim.Observer;

public static class ObservationExporter
{
    public static string Write(World world, string directory, long headTick, long earliestTick,
        string? sourceWorldId = null, string? worldName = null)
    {
        if (earliestTick > world.Tick || headTick < world.Tick || earliestTick < 0)
            throw new ArgumentException("Görüntülenen an geçmiş aralığında olmalı.");
        Directory.CreateDirectory(directory);
        string name = $"evosim-{world.Seed}-t{world.Tick}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}";
        string destination = Path.Combine(directory, name), staging = destination + ".partial";
        Directory.CreateDirectory(staging);
        try
        {
            var health = PopulationHealth.Read(world);
            var report = new {
                schema = "evosim.observation.v2", version = "0.5.10",
                displayCalendar = new { daysPerYear = 365, daysPerSimulationSecond = 1 },
                note = "Gözlem anı raporu; devam ettirilebilir dünya kaydı veya sürekli birey geçmişi değildir.",
                sourceWorldId, worldName, createdAtUtc = DateTime.UtcNow,
                coreSha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(typeof(World).Assembly.Location))),
                viewTick = world.Tick, headTick, earliestTick, historyPreview = world.Tick < headTick,
                seed = world.Seed, config = world.Config, tick = world.Tick, time = world.Time,
                drought = world.Drought, living = world.Living, births = world.Births, deaths = world.Deaths,
                mutations = world.Mutations, stateHash = world.StateHash(), trees = world.Map.Trees,
                corpses = world.Corpses,
                creatures = world.Creatures.Select(c => new {
                    c.Id, c.Group, c.Generation, c.ParentA, c.ParentB, c.Alive, c.Age, c.X, c.Y,
                    c.Energy, c.MaxEnergy, c.Hydration, c.Health, c.Genes, c.BrainWeights, c.BrainMemory,
                    c.Motors, c.BlockedSeconds, c.DistanceTravelled, c.Action, c.Reason,
                    deathTime = c.Alive ? (double?)null : c.DeathTime,
                    deathCause = c.Alive ? null : c.DeathCause,
                    currentDiet = c.Genes[2] < .5f ? "herbivorous" : "carnivorous",
                    c.Maturity, c.ReproductionCooldown, c.Speed, c.Radius }),
                breeding = health.Breeding, populationHealth = health,
                history = world.History, events = world.Events,
                semantics = new {
                    groups = "Group is founder category; currentDiet uses gene 2 at this view tick.",
                    deaths = "Dead bodies keep their final state. deathTime is simulation seconds; living death fields are null.",
                    genealogy = "Parent ID 0 means absent; asexual births have ParentB=0. All recorded creatures are included, not only living ones.",
                    breeding = "Direct condition counts overlap; ExclusiveFirstBlocker partitions current living creatures. Ready pairs are not births.",
                    history = "Population samples and retained events belong to the displayed world. Older event rows may have been trimmed by Core.",
                    timeline = "headTick and earliestTick describe the source archive range; body data describe viewTick only. No future organism states are exported."
                }
            };
            File.WriteAllText(Path.Combine(staging, "report.json"), JsonSerializer.Serialize(report,
                new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));
            var population = new StringBuilder("sim_seconds,herbivores,carnivores,births,deaths\r\n");
            foreach (var h in world.History) Row(population, h.Time, h.Herbivores, h.Carnivores, h.Births, h.Deaths);
            File.WriteAllText(Path.Combine(staging, "population.csv"), population.ToString());
            var creatures = new StringBuilder("view_tick,id,founder_group,current_diet,generation,parent_a,parent_b,alive,age_seconds,x,y,energy,max_energy,hydration,health,death_seconds,death_cause,body,perception,diet,aquatic,boldness,bond,maturity,reproduction_cooldown,speed_capacity,radius,distance_travelled,blocked_seconds,action\r\n");
            foreach (var c in world.Creatures)
                Row(creatures, world.Tick, c.Id, c.Group, c.Genes[2] < .5f ? "herbivorous" : "carnivorous",
                    c.Generation, c.ParentA, c.ParentB, c.Alive, c.Age, c.X, c.Y, c.Energy, c.MaxEnergy,
                    c.Hydration, c.Health, c.Alive ? null : c.DeathTime, c.Alive ? null : c.DeathCause,
                    c.Genes[0], c.Genes[1], c.Genes[2], c.Genes[3], c.Genes[4], c.Genes[5], c.Maturity,
                    c.ReproductionCooldown, c.Speed, c.Radius, c.DistanceTravelled, c.BlockedSeconds, c.Action);
            File.WriteAllText(Path.Combine(staging, "creatures.csv"), creatures.ToString());
            var hashes = Directory.GetFiles(staging).ToDictionary(path => Path.GetFileName(path)!,
                path => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))));
            File.WriteAllText(Path.Combine(staging, "integrity.json"), JsonSerializer.Serialize(hashes, new JsonSerializerOptions { WriteIndented = true }));
            Directory.Move(staging, destination);
            return destination;
        }
        catch { if (Directory.Exists(staging)) Directory.Delete(staging, true); throw; }
    }

    static void Row(StringBuilder buffer, params object?[] cells)
    {
        buffer.AppendJoin(',', cells.Select(value => {
            string s = value switch { null => "", bool b => b ? "true" : "false",
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture), _ => value.ToString() ?? "" };
            return s.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
        }));
        buffer.Append("\r\n");
    }
}
