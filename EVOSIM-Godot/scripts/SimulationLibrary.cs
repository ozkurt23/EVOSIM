using Godot;
using EvoSim.Core;
using EvoSim.Observer;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using State = Godot.Collections.Dictionary;
using Rows = Godot.Collections.Array;

public partial class SimulationBridge
{
    private WorldStore store;
    private TimelineSession timeline;
    private WorldSlot currentSlot;
    private bool currentDeleted;
    public string LastError { get; private set; } = "";
    public string WorldName { get; private set; } = "Yeni vadi";
    public bool ModelReady => store!=null && timeline!=null;
    public bool CanResume => !currentDeleted;
    public bool IsHistorical => timeline?.IsPreviewingHistory ?? false;
    public long HeadTick => timeline?.HeadTick ?? CurrentTick;
    public long EarliestTick => timeline?.EarliestTick ?? 0;
    public bool Seeking => timeline?.IsSeeking ?? false;
    private void EnsureStore()
    {
        if(store!=null)return;
#if EVOSIM_FOREST
        byte[] ModelBytes(string name)
        {
            using var stream=typeof(SimulationBridge).Assembly.GetManifestResourceStream(name+".fingerprint.dll")??throw new InvalidOperationException("Model fingerprint resource missing: "+name);
            using var buffer=new System.IO.MemoryStream();stream.CopyTo(buffer);byte[] bytes=buffer.ToArray();
            var assembly=name switch {"EvoSim.Core.V2"=>typeof(World).Assembly,"ForestEcology"=>typeof(EvoSim.Ecology.Forest).Assembly,_=>typeof(EvoSim.ForestArchive.ModelIdentity).Assembly};
            using var reader=new PEReader(new System.IO.MemoryStream(bytes,false));
            var metadata=reader.GetMetadataReader();
            if(metadata.GetGuid(metadata.GetModuleDefinition().Mvid)!=assembly.ManifestModule.ModuleVersionId)
                throw new InvalidOperationException("Çalışan model ile kayıt kimliği farklı: "+name);
            return bytes;
        }
        EvoSim.ForestArchive.ModelIdentity.Configure(ModelBytes("EvoSim.Core.V2"),ModelBytes("ForestEcology"),ModelBytes("V2Archive"));
        string hash=EvoSim.ForestArchive.ModelIdentity.Value;
        string directory="user://worlds";
#else
        using var core=typeof(SimulationBridge).Assembly.GetManifestResourceStream("EvoSim.Core.fingerprint.dll") ?? throw new InvalidOperationException("Core fingerprint resource missing");
        string hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(core));
        string directory="user://worlds";
#endif
        foreach(var arg in OS.GetCmdlineUserArgs())if(arg.StartsWith("--test-library-dir="))directory=arg.Substring(19);
        store=new WorldStore(ProjectSettings.GlobalizePath(directory),hash);
    }
    private void StepWorld()
    {
        if(timeline == null) world.Step();
        else { timeline.AdvancePlaybackOneTick(); world=timeline.DisplayWorld; }
    }
    private bool Attempt(Action operation)
    {
        try { operation(); LastError=""; return true; }
        catch(Exception ex) { LastError=ex.Message; GD.PushWarning(ex.Message); return false; }
    }
    private WorldStore LegacyStore()
    {
        EnsureStore();
        string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(ProjectSettings.GlobalizePath("res://"),"..","EVOSIM","worlds"));
        return new WorldStore(path,TimelineArchive.CoreHash);
    }
    public Rows ListLegacyWorlds()
    {
        var result=new Rows();
#if EVOSIM_FOREST
        return result; // v1 archives stay available through the original launcher.
#else
        foreach(var slot in LegacyStore().List())result.Add(new State{{"id",slot.Id},{"name",slot.Name},{"days",slot.Tick*World.Dt},{"living",slot.Living}});
        return result;
#endif
    }
    public bool ImportLegacyWorld(string id) => Attempt(()=> {
#if EVOSIM_FOREST
        throw new InvalidOperationException("Eski dünyayı özgün EVOSIM başlatıcısında açın. Orman sürümü eski geçmişi değiştirmez.");
#else
        var source=LegacyStore();var original=source.List().Single(s=>s.Id==id);var loaded=source.Load(original);
        byte[] archive;
        try { archive=source.ReadTimeline(original); }
        catch(InvalidOperationException) { archive=TimelineSession.Create(loaded).SerializeArchive(); }
        var imported=store.Save(loaded,original.Name,null,1,64,64,original.Speed,original.Id,loaded.LoadedCheckpointFormat,archive,false);
        timeline=TimelineSession.Open(loaded,archive);world=loaded;currentSlot=imported;currentDeleted=false;WorldName=imported.Name;seed=world.Seed;
        ClimateCycle=false;Paused=true;debt=0;Scenario="normal";climateCommands.Clear();
#endif
    });
    public Rows ListWorlds(bool trash=false)
    {
        EnsureStore(); var result=new Rows();
        foreach(var slot in store.List(trash)) result.Add(new State {{"id",slot.Id},{"name",slot.Name},{"days",slot.Tick*World.Dt},{"living",slot.Living},{"saved",slot.SavedUtc},{"seed",slot.Seed}});
        return result;
    }
    private WorldSlot Slot(string id,bool trash=false) => store.List(trash).Single(s=>s.Id==id);
    public bool SaveCurrent(string name) => Attempt(()=> {
        EnsureStore(); if(currentDeleted)throw new InvalidOperationException("Bu dünya silindi. Silinenlerden geri alın veya yeni bir dünya oluşturun."); WorldName=string.IsNullOrWhiteSpace(name)?WorldName:name.Trim();
        currentSlot=store.Save(timeline.HeadWorld,WorldName,currentSlot,1,64,64,Speed,timelineArchive:timeline.SerializeArchive(),autoClimate:ClimateCycle);
    });
    public bool OpenWorld(string id) => Attempt(()=> {
        EnsureStore(); var opened=store.OpenForCurrentVersion(Slot(id));
        var session=TimelineSession.Open(opened.World,opened.TimelineArchive);
        timeline=session;world=session.DisplayWorld;currentSlot=opened.Slot;currentDeleted=false;
        WorldName=currentSlot.Name;ClimateCycle=currentSlot.AutoClimate;seed=world.Seed;debt=0;Paused=true;Speed=currentSlot.Speed;
        climateCommands.Clear();Scenario="normal";
        // Existing archives preserve every intervention. Future automatic weather remains optional.
    });
    public bool RenameWorld(string id,string name) => Attempt(()=> {
        var updated=store.Rename(Slot(id),name);
        if(currentSlot?.Id==id){currentSlot=updated;WorldName=updated.Name;}
    });
    public bool DeleteWorld(string id) => Attempt(()=> {
        store.Trash(Slot(id));if(currentSlot?.Id==id)currentDeleted=true;
    });
    public bool RestoreWorld(string id) => Attempt(()=>{store.Restore(Slot(id,true));if(currentSlot?.Id==id)currentDeleted=false;});
    public bool SeekDay(double day) => Attempt(()=> {
        Paused=true;debt=0;timeline.BeginSeek(Math.Clamp((long)Math.Round(day/World.Dt),EarliestTick,HeadTick));world=timeline.DisplayWorld;
    });
    public void ReturnToPresent(){timeline.ReturnToHead();world=timeline.DisplayWorld;debt=0;Paused=true;}
    public bool BranchHistory(string name) => Attempt(()=> {
        var branch=timeline.CreateBranch();
        var slot=store.Save(branch.HeadWorld,name,null,1,64,64,Speed,currentSlot?.Id,timelineArchive:branch.SerializeArchive(),autoClimate:ClimateCycle);
        timeline=branch;world=branch.DisplayWorld;currentSlot=slot;currentDeleted=false;WorldName=slot.Name;debt=0;
    });
    public Rows Settings()
    {
        var rows=new Rows();var config=Configuration();
        foreach(var field in typeof(WorldConfig).GetFields())
        {
            var attribute=field.GetCustomAttribute<SettingAttribute>();if(attribute==null)continue;
            double scale=WorldCalendar.UsesYears(field.Name)?365:1;
            rows.Add(new State{{"key",field.Name},{"group",attribute.Group},{"label",WorldCalendar.SettingLabel(field.Name,attribute.Label)},
                {"value",Convert.ToDouble(field.GetValue(config))/scale},{"min",attribute.Min/scale},{"max",attribute.Max/scale},
                {"integer",field.FieldType==typeof(int)},{"normalized",attribute.Min>=0 && attribute.Max<=1 && field.FieldType==typeof(float)},
                {"help",SettingHelp.For(field.Name)}});
        }
        string[] genes={"Beden","Algı","Beslenme","Su uyumu","Atılganlık","Sosyal bağ"};
        foreach(string group in new[]{"Herb","Carn"})foreach(string edge in new[]{"Min","Max"})for(int i=0;i<6;i++)
        {
            var values=(float[])typeof(WorldConfig).GetField(group+edge).GetValue(config);
            rows.Add(new State{{"key",$"{group}{edge}:{i}"},{"group",group=="Herb"?"Otçul genleri":"Etçil genleri"},
                {"label",genes[i]+(edge=="Min"?" · alt":" · üst")},{"value",values[i]},{"min",0.0},{"max",1.0},{"integer",false},{"normalized",true},{"help",SettingHelp.For("Gene"+i)}});
        }
        AddForestSettings(rows);
        return rows;
    }
    public bool CreateWorld(string name,int worldSeed,State values) => Attempt(()=> {
        var config=Configuration();
        foreach(var key in values.Keys)
        {
            string nameKey=key.AsString();double value=values[key].AsDouble();
            if(!double.IsFinite(value))throw new ArgumentException("Ayar sonlu bir sayı olmalı.");
            if(nameKey.StartsWith("Forest"))
            {
                if(!LivingForest || !new[]{"ForestLifespan","ForestMaturity","ForestDroughtTolerance","ForestSeedSpread","ForestDensity","ForestCorpseContribution","ForestSoilHalfLife"}.Contains(nameKey))
                    throw new ArgumentException("Bilinmeyen orman ayarı: "+nameKey);
                continue;
            }
            if(nameKey.Contains(':')) {var parts=nameKey.Split(':');var field=typeof(WorldConfig).GetField(parts[0]);if(field?.FieldType!=typeof(float[]))throw new ArgumentException("Bilinmeyen gen aralığı");((float[])field.GetValue(config))[int.Parse(parts[1])]=(float)value;continue;}
            var setting=typeof(WorldConfig).GetField(nameKey);
            if(setting?.GetCustomAttribute<SettingAttribute>()==null)throw new ArgumentException("Bilinmeyen ayar: "+nameKey);
            if(WorldCalendar.UsesYears(nameKey))value*=365;
            setting.SetValue(config,setting.FieldType==typeof(int)?(object)checked((int)Math.Round(value)):(float)value);
        }
        config.Validate();var candidate=CreateSimulationWorld(worldSeed,config,values);var session=TimelineSession.Create(candidate);
        world=candidate;timeline=session;seed=worldSeed;currentSlot=null;currentDeleted=false;WorldName=string.IsNullOrWhiteSpace(name)?"Yeni vadi":name.Trim();
        debt=0;Paused=true;climateCommands.Clear();Scenario="normal";
    });
    public string ExportReport()
    {
        string destination="";
        Attempt(()=>destination=ObservationExporter.Write(world,ProjectSettings.GlobalizePath("user://reports"),HeadTick,EarliestTick,currentSlot?.Id,WorldName,TimelineArchive.CoreHash.ToLowerInvariant()));
        return destination;
    }
    public Rows PopulationHistory()
    {
        var result=new Rows();foreach(var h in world.History)result.Add(new State{{"day",h.Time},{"herbivores",h.Herbivores},{"carnivores",h.Carnivores},{"births",h.Births},{"deaths",h.Deaths}});return result;
    }
    public Rows Journal()
    {
        var result=new Rows();foreach(var e in world.Events.TakeLast(100))result.Add(new State{{"day",e.Time},{"text",DescribeEvent(e)},{"kind",e.Kind}});return result;
    }
    private string DescribeEvent(SimEvent entry)
    {
        int id=entry.ChildId;
        if(entry.Kind=="death") { int end=entry.Text.IndexOf(' '); if(end>1 && entry.Text.StartsWith('#'))int.TryParse(entry.Text.AsSpan(1,end-1),out id); }
        var creature=world.Find(id);
        return creature!=null && (entry.Kind=="birth"||entry.Kind=="death"||entry.Kind=="mutation") ? (creature.Genes[2]<.5f?"Otçul":"Etçil")+" · "+entry.Text : entry.Text;
    }
    public string StateDigest() => world.StateHash();
    public State CreatureDetails(int id)
    {
        var c=world.Creatures.Find(c=>c.Id==id);if(c==null)return new State();
        var children=world.Creatures.Where(x=>x.ParentA==id||x.ParentB==id).Select(x=>x.Id).ToHashSet();
        int grandchildren=world.Creatures.Count(x=>children.Contains(x.ParentA)||children.Contains(x.ParentB));
        return new State{{"id",c.Id},{"alive",c.Alive},{"predator",c.Genes[2]>=.5f},{"age",c.Age},{"generation",c.Generation},
            {"parent_a",c.ParentA},{"parent_b",c.ParentB},{"children",children.Count},{"grandchildren",grandchildren},
            {"energy",c.Energy},{"max_energy",c.MaxEnergy},{"water",c.Hydration},{"health",c.Health},{"action",c.Action},{"genes",c.Genes}};
    }
}
