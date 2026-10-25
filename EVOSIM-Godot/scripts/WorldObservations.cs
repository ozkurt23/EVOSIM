using Godot;
using EvoSim.Core;
using State=Godot.Collections.Dictionary;
using Rows=Godot.Collections.Array;

public partial class SimulationBridge
{
    // Read only, reconstructed from the displayed checkpoint. No future-state cache.
    public State FounderDistribution(int gene)
    {
        gene=Math.Clamp(gene,0,5);int[] h=new int[10],c=new int[10];
        foreach(var a in world.Creatures.Where(a=>a.ParentA==0 && a.ParentB==0))
            (a.Genes[2]<.5f?h:c)[Math.Clamp((int)(a.Genes[gene]*10),0,9)]++;
        return new State{{"herb",h},{"carn",c}};
    }
    public int[] DescendantIds(int ancestor)
    {
        var members=new HashSet<int>{ancestor};
        // IDs are topological in saved worlds; handle both parents without duplicate descendants.
        foreach(var a in world.Creatures.OrderBy(a=>a.Id))
            if(members.Contains(a.ParentA)||members.Contains(a.ParentB))members.Add(a.Id);
        return world.Creatures.Where(a=>a.Alive && members.Contains(a.Id)).Select(a=>a.Id).ToArray();
    }
    public State RegionObservation(float x,float z,float radius)
    {
        radius=Math.Clamp(radius,2,24);double rr=radius*radius;
        bool Inside(float px,float pz)=>(px-x)*(px-x)+(pz-z)*(pz-z)<=rr;
        var live=world.Creatures.Where(a=>a.Alive&&Inside(a.X,a.Y)).ToArray();
        int trees=0,fruit=0,water=0,cells=0;double food=0;
        foreach(var tree in world.Map.Trees)if(Inside(tree.X,tree.Y)){trees++;fruit+=tree.Fruit.Count(e=>e>.01f);}
        for(int cy=Math.Max(0,(int)(z-radius));cy<=Math.Min(world.Map.Size-1,(int)(z+radius));cy++)
        for(int cx=Math.Max(0,(int)(x-radius));cx<=Math.Min(world.Map.Size-1,(int)(x+radius));cx++)
            if(Inside(cx+.5f,cy+.5f)){int i=cx+cy*world.Map.Size;cells++;food+=world.Map.Food[i];if(world.Map.Tiles[i]==1)water++;}
        var causes=new State();
        foreach(var a in world.Creatures.Where(a=>!a.Alive&&a.DeathTime>=Math.Max(0,world.Time-30)&&Inside(a.X,a.Y)))
            causes[a.DeathCause]=(causes.ContainsKey(a.DeathCause)?causes[a.DeathCause].AsInt32():0)+1;
        return new State{{"x",x},{"z",z},{"radius",radius},{"day",world.Time},{"herbivores",live.Count(a=>a.Genes[2]<.5f)},
            {"carnivores",live.Count(a=>a.Genes[2]>=.5f)},{"trees",trees},{"fruit",fruit},{"grass_energy",food},
            {"water_cells",water},{"cells",cells},{"deaths",causes}};
    }
    public Rows ObservationNotes()
    {
        var rows=new Rows();
        foreach(var e in world.Events.Where(e=>e.Kind is "birth" or "death" or "mutation").TakeLast(8).Reverse())
        {
            int id=e.ChildId;
            if(e.Kind=="death") {int end=e.Text.IndexOf(' ');if(end>1)int.TryParse(e.Text.AsSpan(1,end-1),out id);}
            var c=world.Find(id);
            rows.Add(new State{{"day",e.Time},{"text",DescribeEvent(e)},{"id",id},{"located",c!=null},
                {"x",c?.X??0},{"z",c?.Y??0},{"kind",e.Kind}});
        }
        var past=world.History.LastOrDefault(h=>h.Time<=world.Time-30);
        if(past!=null) {
            int then=past.Herbivores+past.Carnivores,now=world.Living;
            if(then>0 && Math.Abs(now-then)>=Math.Max(5,then*.15))
                rows.Insert(0,new State{{"day",world.Time},{"text",$"Son {world.Time-past.Time:0} günde nüfus {then} → {now}. Doğum ve ölümleri incele."},
                    {"id",-1},{"located",false},{"x",0},{"z",0},{"kind","population"}});
        }
        return rows;
    }
    public Rows ObservationMap(int mode)
    {
        var cells=new Dictionary<(int,int),double>();
        void Add(float x,float z,double amount){var key=((int)x/8,(int)z/8);cells[key]=cells.GetValueOrDefault(key)+amount;}
        if(mode==1)foreach(var a in world.Creatures.Where(a=>a.Alive))Add(a.X,a.Y,1);
        if(mode==2) {
            for(int i=0;i<world.Map.Food.Length;i++)if(world.Map.Food[i]>.01)Add(i%world.Map.Size,i/world.Map.Size,world.Map.Food[i]);
            foreach(var t in world.Map.Trees)Add(t.X,t.Y,t.Fruit.Sum());
        }
        if(mode==3)foreach(var a in world.Creatures.Where(a=>!a.Alive && a.DeathTime>=Math.Max(0,world.Time-30)))Add(a.X,a.Y,1);
        var result=new Rows();foreach(var cell in cells)result.Add(new State{{"x",cell.Key.Item1*8+4},{"z",cell.Key.Item2*8+4},{"value",cell.Value}});
        return result;
    }
    public Rows ReproductiveSuccess(int gene)
    {
        gene=Math.Clamp(gene,0,5);var counts=new Dictionary<int,int>();
        foreach(var child in world.Creatures.Where(c=>c.Age>=world.Config.MaturityAge))
            foreach(int parent in new[]{child.ParentA,child.ParentB}.Where(i=>i>0).Distinct())counts[parent]=counts.GetValueOrDefault(parent)+1;
        var rows=new Rows();foreach(var c in world.Creatures.Where(c=>c.Age>=world.Config.MaturityAge))
            rows.Add(new State{{"id",c.Id},{"gene",c.Genes[gene]},{"offspring",counts.GetValueOrDefault(c.Id)},{"age",c.Age},{"predator",c.Genes[2]>=.5f}});
        return rows;
    }
    public State PopulationOutcomes()
    {
        var causes=new State();var actions=new State();
        foreach(var c in world.Creatures) {
            if(c.Alive)actions[c.Action]=(actions.ContainsKey(c.Action)?actions[c.Action].AsInt32():0)+1;
            else if(c.DeathTime>=Math.Max(0,world.Time-30))causes[c.DeathCause]=(causes.ContainsKey(c.DeathCause)?causes[c.DeathCause].AsInt32():0)+1;
        }
        return new State{{"deaths",causes},{"actions",actions}};
    }
    public State ValidateObservations()
    {
        string before=world.StateHash();var region=RegionObservation(64,64,24);var founders=FounderDistribution(0);
        var expected=world.Creatures.Count(a=>a.ParentA==0&&a.ParentB==0);
        var result=new State{{"founder_count",founders["herb"].AsInt32Array().Sum()+founders["carn"].AsInt32Array().Sum()==expected},
          {"missing_ancestor_empty",DescendantIds(int.MaxValue).Length==0},
          {"region_counts",region["herbivores"].AsInt32()+region["carnivores"].AsInt32()<=world.Living}};
        ObservationNotes();result["read_only"]=before==world.StateHash();result["passed"]=result.Values.All(v=>v.AsBool());return result;
    }
}
