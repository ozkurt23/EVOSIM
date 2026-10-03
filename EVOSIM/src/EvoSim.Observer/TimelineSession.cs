using System.Diagnostics;
using EvoSim.Core;

namespace EvoSim.Observer;

internal readonly record struct TimelineProgress(long StartTick,long TargetTick,long CurrentTick,bool IsSeeking,double Fraction,string? Error=null);

// HeadWorld is authoritative. Historical playback and seek candidates never replace it.
// Callers must route biological stepping/interventions through this session.
internal sealed class TimelineSession
{
    readonly TimelineArchive archive;
    World display;int displayNextCommandIndex;
    SeekWork? seek;TimelineProgress progress;
    public World HeadWorld { get; }
    public World DisplayWorld=>display;
    public long EarliestTick=>archive.Origin.Tick;
    public long HeadTick=>archive.HeadTick;
    public long ViewTick=>display.Tick;
    public bool IsSeeking=>seek!=null;
    public bool IsPreviewingHistory=>display.Tick<HeadTick;
    public TimelineProgress Progress=>progress;
    internal int CachedSnapshotCount=>archive.Snapshots.Count;
    internal long CachedSnapshotBytes=>archive.Snapshots.Sum(s=>(long)s.Compressed.Length);
    internal int CommandCount=>archive.Commands.Count;
    internal int CacheBudgetBytes=>archive.CacheBudgetBytes;
    internal int SnapshotIntervalTicks=>archive.SnapshotIntervalTicks;
    sealed class SeekWork
    {
        internal readonly World Candidate;internal readonly long Start,Target;internal int NextCommandIndex;
        internal SeekWork(World candidate,long target,int next){Candidate=candidate;Start=candidate.Tick;Target=target;NextCommandIndex=next;}
    }
    TimelineSession(World head,TimelineArchive archive)
    {HeadWorld=head;display=head;this.archive=archive;displayNextCommandIndex=archive.Commands.Count;progress=new(head.Tick,head.Tick,head.Tick,false,1);}
    public static TimelineSession Create(World head)=>Create(head,TimelineArchive.DefaultCacheBudgetBytes,TimelineArchive.DefaultSnapshotIntervalTicks);
    internal static TimelineSession Create(World head,int cacheBudgetBytes,int snapshotIntervalTicks)
    {
        ArgumentNullException.ThrowIfNull(head);_=TimelineArchive.CoreHash;
        if(cacheBudgetBytes<0||cacheBudgetBytes>TimelineArchive.DefaultCacheBudgetBytes)throw new ArgumentOutOfRangeException(nameof(cacheBudgetBytes));
        if(snapshotIntervalTicks<1)throw new ArgumentOutOfRangeException(nameof(snapshotIntervalTicks));
        var origin=TimelineSnapshot.Capture(head,0);
        return new TimelineSession(head,new TimelineArchive(origin,new(),new(),head.Tick,origin.RawHash,cacheBudgetBytes,snapshotIntervalTicks));
    }
    public static TimelineSession Open(World head,byte[] bytes)
    {ArgumentNullException.ThrowIfNull(head);return new TimelineSession(head,TimelineArchive.Deserialize(bytes,head));}
    void CheckHead(){if(HeadWorld.Tick!=HeadTick)throw new InvalidOperationException("Dünya geçmiş sistemi dışında ilerletilmiş; kayıt üzerine yazılmadı.");}
    public void AdvancePlaybackOneTick()
    {
        CheckHead();if(IsSeeking)throw new InvalidOperationException("Geçmiş anı aranırken oynatma beklemeli.");
        if(!ReferenceEquals(display,HeadWorld))
        {
            Drain(display,ref displayNextCommandIndex);
            if(display.Tick<HeadTick){display.Step();Drain(display,ref displayNextCommandIndex);}
            if(display.Tick==HeadTick)
            {
                if(TimelineArchive.Hash(TimelineArchive.SaveWorld(display))!=TimelineArchive.Hash(TimelineArchive.SaveWorld(HeadWorld)))throw new InvalidDataException("Yeniden oynatılan gelecek kayıtlı son durumla eşleşmedi.");
                display=HeadWorld;displayNextCommandIndex=archive.Commands.Count;
            }
            progress=new(ViewTick,ViewTick,ViewTick,false,1);return;
        }
        HeadWorld.Step();archive.HeadTick=HeadWorld.Tick;displayNextCommandIndex=archive.Commands.Count;
        if((HeadTick-EarliestTick)%archive.SnapshotIntervalTicks==0)AddSnapshot(TimelineSnapshot.Capture(HeadWorld,archive.Commands.Count));
        progress=new(HeadTick,HeadTick,HeadTick,false,1);
    }
    public void ExecuteDrought(bool enabled)
    {
        CheckHead();if(IsSeeking||!ReferenceEquals(display,HeadWorld))throw new InvalidOperationException("Geçmişe müdahale etmek için önce ayrı bir dal oluştur.");
        if(archive.Commands.Count>=TimelineArchive.MaximumCommands)throw new InvalidOperationException("Geçmiş komut kapasitesine ulaşıldı; hiçbir kayıt silinmedi.");
        archive.Commands.Add(new TimelineCommand(HeadTick,enabled));HeadWorld.SetDrought(enabled);displayNextCommandIndex=archive.Commands.Count;
    }
    void Drain(World world,ref int next)
    {while(next<archive.Commands.Count&&archive.Commands[next].Tick<=world.Tick){world.SetDrought(archive.Commands[next].Drought);next++;}}
    void AddSnapshot(TimelineSnapshot snapshot)
    {
        if(snapshot.Compressed.Length>archive.CacheBudgetBytes)return;
        archive.Snapshots.RemoveAll(s=>s.Tick==snapshot.Tick&&s.NextCommandIndex==snapshot.NextCommandIndex);
        archive.Snapshots.Add(snapshot);archive.Snapshots.Sort((a,b)=>a.Tick!=b.Tick?a.Tick.CompareTo(b.Tick):a.NextCommandIndex.CompareTo(b.NextCommandIndex));
        long bytes=CachedSnapshotBytes;while(bytes>archive.CacheBudgetBytes&&archive.Snapshots.Count>0){bytes-=archive.Snapshots[0].Compressed.Length;archive.Snapshots.RemoveAt(0);}
    }
    public void BeginSeek(long targetTick)
    {
        CheckHead();if(targetTick<EarliestTick||targetTick>HeadTick)throw new ArgumentOutOfRangeException(nameof(targetTick),"Seçilen an kayıtlı geçmiş aralığının dışında.");
        if(targetTick==HeadTick){ReturnToHead();return;}
        if(targetTick==ViewTick){CancelSeek();return;}
        var snapshot=archive.Snapshots.Where(s=>s.Tick<=targetTick).OrderByDescending(s=>s.Tick).ThenByDescending(s=>s.NextCommandIndex).FirstOrDefault()??archive.Origin;
        // Decode into an unpublished candidate. A cancelled or replaced seek never mutates display.
        var candidate=snapshot.Restore();seek=new SeekWork(candidate,targetTick,snapshot.NextCommandIndex);
        progress=new(seek.Start,targetTick,candidate.Tick,true,0);
    }
    public void PumpSeek(double budgetMs=6,int maxTicks=100)
    {
        if(!double.IsFinite(budgetMs)||budgetMs<=0)throw new ArgumentOutOfRangeException(nameof(budgetMs));if(maxTicks<1)throw new ArgumentOutOfRangeException(nameof(maxTicks));
        if(seek==null)return;CheckHead();var job=seek;var timer=Stopwatch.StartNew();int ticks=0,commands=0;
        try
        {
            while(true)
            {
                while(job.NextCommandIndex<archive.Commands.Count&&archive.Commands[job.NextCommandIndex].Tick<=job.Candidate.Tick)
                {
                    job.Candidate.SetDrought(archive.Commands[job.NextCommandIndex].Drought);job.NextCommandIndex++;commands++;
                    if(timer.Elapsed.TotalMilliseconds>=budgetMs||commands>=4096){UpdateProgress(job);return;}
                }
                if(job.Candidate.Tick==job.Target)
                {
                    display=job.Candidate;displayNextCommandIndex=job.NextCommandIndex;seek=null;progress=new(job.Start,job.Target,job.Target,false,1);return;
                }
                if(ticks>=maxTicks||timer.Elapsed.TotalMilliseconds>=budgetMs){UpdateProgress(job);return;}
                job.Candidate.Step();ticks++;
            }
        }
        catch(Exception ex){seek=null;progress=new(job.Start,job.Target,job.Candidate.Tick,false,0,ex.Message);throw;}
    }
    void UpdateProgress(SeekWork job)
    {double fraction=job.Target==job.Start?0:(double)(job.Candidate.Tick-job.Start)/(job.Target-job.Start);progress=new(job.Start,job.Target,job.Candidate.Tick,true,Math.Clamp(fraction,0,1));}
    public void CancelSeek(){seek=null;progress=new(ViewTick,ViewTick,ViewTick,false,1);}
    public void ReturnToHead(){seek=null;display=HeadWorld;displayNextCommandIndex=archive.Commands.Count;progress=new(HeadTick,HeadTick,HeadTick,false,1);}
    public TimelineSession CreateBranch()
    {
        CheckHead();if(IsSeeking)throw new InvalidOperationException("Dal oluşturmadan önce geçmiş aramasının bitmesini bekle.");
        byte[] state=TimelineArchive.SaveWorld(display);using var memory=new MemoryStream(state,false);var child=World.Load(memory);
        int prefix=archive.Commands.FindIndex(c=>c.Tick>ViewTick);if(prefix<0)prefix=archive.Commands.Count;
        var commands=archive.Commands.Take(prefix).ToList();var snapshots=archive.Snapshots.Where(s=>s.Tick<=ViewTick&&s.NextCommandIndex<=prefix).ToList();
        var branch=new TimelineSession(child,new TimelineArchive(archive.Origin,commands,snapshots,ViewTick,TimelineArchive.Hash(state),archive.CacheBudgetBytes,archive.SnapshotIntervalTicks));
        branch.AddSnapshot(TimelineSnapshot.Capture(child,prefix));return branch;
    }
    public byte[] SerializeArchive()
    {
        CheckHead();archive.HeadHash=TimelineArchive.Hash(TimelineArchive.SaveWorld(HeadWorld));archive.HeadTick=HeadWorld.Tick;return archive.Serialize();
    }
}
