using EvoSim.Observer;
using State = Godot.Collections.Dictionary;
using Rows = Godot.Collections.Array;

public partial class SimulationBridge
{
    private readonly Dictionary<int,(TimelineSession Source,BackgroundLifeTrace Job)> traces=new();
    private readonly Dictionary<int,Rows> traceRows=new();
    public bool StartLifeTrace(int id) => Attempt(()=> {
        foreach(int stale in traces.Where(p=>!ReferenceEquals(p.Value.Source,timeline)).Select(p=>p.Key).ToArray())CancelLifeTrace(stale);
        if(traces.ContainsKey(id))return;
        if(world.Find(id)==null)throw new ArgumentException("Bu anda canlı bulunamadı.");
        if(traces.Count>=4)throw new InvalidOperationException("Önce açık yaşam izi panellerinden birini kapatın.");
        traces[id]=(timeline,new BackgroundLifeTrace(timeline,id,CurrentTick));
    });
    public State ReadLifeTrace(int id)
    {
        if(!traces.TryGetValue(id,out var entry))return new State{{"missing",true}};
        if(!ReferenceEquals(entry.Source,timeline)){CancelLifeTrace(id);return new State{{"missing",true}};}
        var job=entry.Job;job.Pump();
        if(job.Complete && !traceRows.ContainsKey(id))
        {
            var points=new Rows();
            foreach(var p in job.Points)
                points.Add(new State{{"day",p.Tick*.05},{"exists",p.Exists},{"alive",p.Alive},
                    {"energy",p.Energy.HasValue && p.MaxEnergy>0 ? Math.Clamp(p.Energy.Value/p.MaxEnergy.Value*100,0,100):0},
                    {"water",p.Water??0},{"health",p.Health??0}});
            traceRows[id]=points;
        }
        return new State{{"missing",false},{"complete",job.Complete},{"error",job.Error??""},
            {"fraction",job.Fraction},{"from_day",job.FromTick*.05},{"to_day",job.ToTick*.05},
            {"points",traceRows.TryGetValue(id,out var rows)?rows:new Rows()}};
    }
    public void CancelLifeTrace(int id)
    {
        if(traces.Remove(id,out var entry))entry.Job.Cancel();
        traceRows.Remove(id);
    }
    public override void _ExitTree()
    {
        foreach(var entry in traces.Values)entry.Job.Cancel();
        traces.Clear();traceRows.Clear();
    }
}
