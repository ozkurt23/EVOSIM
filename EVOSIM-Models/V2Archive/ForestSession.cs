using System.Security.Cryptography;
using System.Text;
using EvoSim.Core;
using EvoSim.Ecology;
using EvoSim.Observer;

namespace EvoSim.ForestArchive;

public static class ModelIdentity
{
    public static string Combine(byte[] core,byte[] ecology,byte[] archive)
    {
        string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
        string canonical="EVOSIM-MODEL-SET-1\nCoreV2="+Hash(core)+"\nForestEcology="+Hash(ecology)+"\nV2Archive="+Hash(archive)+"\n";
        return Hash(Encoding.UTF8.GetBytes(canonical)).ToLowerInvariant();
    }
    static readonly Lazy<string> value=new(()=>Combine(File.ReadAllBytes(typeof(World).Assembly.Location),
        File.ReadAllBytes(typeof(Forest).Assembly.Location),File.ReadAllBytes(typeof(ModelIdentity).Assembly.Location)));
    static string? configured;
    public static void Configure(byte[] core,byte[] ecology,byte[] archive)
    {
        string identity=Combine(core,ecology,archive);
        if(configured!=null && configured!=identity)throw new InvalidOperationException("Model identity already configured differently.");
        if(value.IsValueCreated && value.Value!=identity)throw new InvalidOperationException("Loaded model identity differs.");
        configured=identity;
    }
    public static string Value=>configured??value.Value;
}

/// <summary>Explicit v2 timeline boundary. This API does not open v1 libraries.</summary>
public sealed class ForestSession
{
    readonly TimelineSession session;
    static ForestSession()=>TimelineArchive.ConfigureCoreHash(ModelIdentity.Value);
    ForestSession(TimelineSession value){session=value;}
    public World Display=>session.DisplayWorld;
    public long HeadTick=>session.HeadTick;
    public long EarliestTick=>session.EarliestTick;
    public bool Seeking=>session.IsSeeking;
    public string? Error=>session.Progress.Error;
    public static ForestSession Create(World world)=>new(TimelineSession.Create(world));
    public static ForestSession Open(string directory,string id)
    {
        var store=new WorldStore(directory,ModelIdentity.Value);
        var slot=store.List().Single(s=>s.Id==id);
        return new(TimelineSession.Open(store.Load(slot),store.ReadTimeline(slot)));
    }
    public void Advance()=>session.AdvancePlaybackOneTick();
    public void SetDrought(bool value)=>session.ExecuteDrought(value);
    public void Seek(long tick)=>session.BeginSeek(tick);
    public void Pump()=>session.PumpSeek();
    public void ReturnToHead()=>session.ReturnToHead();
    public ForestSession Branch()=>new(session.CreateBranch());
    public string Save(string directory,string name,string? existingId=null)
    {
        var store=new WorldStore(directory,ModelIdentity.Value);
        var existing=existingId==null?null:store.List().Single(s=>s.Id==existingId);
        var slot=store.Save(session.HeadWorld,name,existing,1,64,64,1,timelineArchive:session.SerializeArchive());
        return slot.Id;
    }
    public byte[] ExportTimeline()=>session.SerializeArchive();
    public static void ValidateTimeline(byte[] bytes)=>TimelineArchive.ValidateModel(bytes);
}
