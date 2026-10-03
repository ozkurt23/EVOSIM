using System.Diagnostics;
using EvoSim.Core;
namespace EvoSim.Observer;

internal sealed record LifeTracePoint(long Tick,bool Exists,bool Alive,float? Energy,float? MaxEnergy,
    float? Water,float? Health,float? X,float? Y,double? DeathTime,string? DeathCause);
internal sealed class LifeTraceJob
{
    byte[]? headBytes,archiveBytes;TimelineSession? replay;
    readonly List<LifeTracePoint> points=new();bool? previousExists,previousAlive;
    public long FromTick{get;} public long ToTick{get;} public int CreatureId{get;}
    public int IntervalTicks{get;} public bool Complete{get;private set;} public bool Cancelled{get;private set;}
    public string? Error{get;private set;} public bool Preparing=>replay==null||replay.IsSeeking;
    public IReadOnlyList<LifeTracePoint> Points=>points;
    public double Fraction=>Complete?1:replay==null||replay.IsSeeking?0:Math.Clamp((replay.ViewTick-FromTick)/(double)Math.Max(1,ToTick-FromTick),0,1);
    public long CurrentTick=>Complete?ToTick:replay?.ViewTick??FromTick;
    public LifeTraceJob(TimelineSession source,int creatureId,long endTick,int intervalTicks=20)
    {
        if(endTick<source.EarliestTick||endTick>source.HeadTick||intervalTicks<1)throw new ArgumentOutOfRangeException(nameof(endTick));
        CreatureId=creatureId;ToTick=endTick;FromTick=Math.Max(source.EarliestTick,endTick-24000);IntervalTicks=intervalTicks;
        using var stream=new MemoryStream();source.HeadWorld.Save(stream);headBytes=stream.ToArray();archiveBytes=source.SerializeArchive();
    }
    public void Cancel(){Cancelled=true;headBytes=archiveBytes=null;replay=null;points.Clear();}
    public void Pump(double budgetMs=6,int maxTicks=100)
    {
        if(Complete||Cancelled||Error!=null)return;
        if(!double.IsFinite(budgetMs)||budgetMs<=0||maxTicks<1)throw new ArgumentOutOfRangeException(nameof(budgetMs));
        var watch=Stopwatch.StartNew();
        try
        {
            // Snapshot decoding is indivisible; stepping after initialization is budgeted.
            if(replay==null){using var stream=new MemoryStream(headBytes!,false);replay=TimelineSession.Open(World.Load(stream),archiveBytes!);headBytes=archiveBytes=null;replay.BeginSeek(FromTick);return;}
            if(replay.IsSeeking){replay.PumpSeek(budgetMs,maxTicks);return;}
            int steps=0;
            while(true)
            {
                var world=replay.DisplayWorld;var c=world.Find(CreatureId);bool exists=c!=null,lives=c?.Alive??false;
                bool sample=(world.Tick-FromTick)%IntervalTicks==0||world.Tick==ToTick||previousExists!=exists||previousAlive!=lives;
                if(sample&&(points.Count==0||points[^1].Tick!=world.Tick))points.Add(new(world.Tick,exists,lives,c?.Energy,c?.MaxEnergy,c?.Hydration,c?.Health,c?.X,c?.Y,c is {Alive:false}?c.DeathTime:null,c is {Alive:false}?c.DeathCause:null));
                previousExists=exists;previousAlive=lives;
                if(world.Tick==ToTick){Complete=true;replay=null;return;}
                if(steps>=maxTicks||watch.Elapsed.TotalMilliseconds>=budgetMs)return;
                replay.AdvancePlaybackOneTick();steps++;
            }
        }
        catch(Exception ex){Error=ex.Message;replay=null;headBytes=archiveBytes=null;points.Clear();}
    }
}
