namespace EvoSim.Observer;

// The UI captures the source once. Only the worker owns and steps the replay engine.
internal sealed class BackgroundLifeTrace
{
    sealed record Progress(bool Preparing,long Tick,double Fraction);
    sealed record Result(IReadOnlyList<LifeTracePoint> Points,string? Error);
    readonly Task<Result> worker;
    int cancelRequested;
    internal int WorkerThreadId;
    Progress progress;
    public long FromTick{get;} public long ToTick{get;} public int CreatureId{get;}
    public bool Complete{get;private set;} public bool Cancelled{get;private set;}
    public string? Error{get;private set;}
    public IReadOnlyList<LifeTracePoint> Points{get;private set;}=Array.Empty<LifeTracePoint>();
    public bool Preparing=>Volatile.Read(ref progress).Preparing;
    public long CurrentTick=>Complete?ToTick:Volatile.Read(ref progress).Tick;
    public double Fraction=>Complete?1:Volatile.Read(ref progress).Fraction;
    internal Task Completion=>worker;
    public BackgroundLifeTrace(TimelineSession source,int id,long until)
    {
        var engine=new LifeTraceJob(source,id,until);
        FromTick=engine.FromTick;ToTick=engine.ToTick;CreatureId=id;
        progress=new(true,FromTick,0);
        worker=Task.Run(()=>
        {
            Volatile.Write(ref WorkerThreadId,Environment.CurrentManagedThreadId);
            try
            {
                while(!engine.Complete&&engine.Error==null&&Volatile.Read(ref cancelRequested)==0)
                {
                    engine.Pump(6,100);
                    Volatile.Write(ref progress,new(engine.Preparing,engine.CurrentTick,engine.Fraction));
                }
                if(Volatile.Read(ref cancelRequested)!=0)return new Result(Array.Empty<LifeTracePoint>(),null);
                return new Result(Array.AsReadOnly(engine.Points.ToArray()),engine.Error);
            }
            catch(Exception ex){return new Result(Array.Empty<LifeTracePoint>(),ex.Message);}
            finally{engine.Cancel();}
        });
    }
    // Never wait for replay or decoding from the render/event loop.
    public void Pump()
    {
        if(Cancelled||Complete||Error!=null||!worker.IsCompleted)return;
        var result=worker.GetAwaiter().GetResult();Points=result.Points;Error=result.Error;Complete=Error==null;
    }
    public void Cancel(){Interlocked.Exchange(ref cancelRequested,1);Cancelled=true;Points=Array.Empty<LifeTracePoint>();}
}
