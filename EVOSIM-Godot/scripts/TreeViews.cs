using EvoSim.Core;
using State = Godot.Collections.Dictionary;

public partial class SimulationBridge
{
    public State TreeDetails(int id)=>InspectTree(world,id);
    private static State InspectTree(World source,int id)
    {
#if EVOSIM_FOREST
        int index=-1;
        for(int i=0;i<source.Forest.Trees.Count;i++)if(source.TreeIdentity(i)==id){index=i;break;}
        if(index<0)return new State();
#else
        int index=id;
#endif
        if(index<0||index>=source.Map.Trees.Count)return new State();
        var tree=source.Map.Trees[index];
        var result=new State{{"id",id},{"x",tree.X},{"y",tree.Y},{"radius",tree.Radius},
            {"available",tree.Fruit.Count(f=>f>.01f)},{"slots",tree.Fruit.Length},
            {"energy",tree.Fruit.Sum(f=>(double)f)},{"fruit_energy",source.Config.FruitEnergy},
            {"water_access",source.Map.WaterLevel},{"drought",source.Drought}};
#if EVOSIM_FOREST
        var state=source.Forest.Trees[index];
        result["death_cause"]=state.DeadDay<0?"":state.Health<=0?"Susuzluk":"Yaşlılık";
        result["age_days"]=state.Age;result["health"]=state.Health;result["dead"]=state.DeadDay>=0;
        result["growth"]=source.Forest.Settings.MaturityDays<=0?1:Math.Clamp(state.Growth/source.Forest.Settings.MaturityDays,0,1);
        result["soil"]=source.Forest.SoilAt(state.Cell);
#endif
        return result;
    }
    public State ValidateTreeViews()
    {
        var before=world.StateHash();var state=TreeDetails(world.Map.Trees.Count>0?TreeId(0):-1);
        var checks=new State{{"read_only",before==world.StateHash()},
            {"invalid_ids_empty",TreeDetails(-1).Count==0 && TreeDetails(int.MaxValue).Count==0}};
        var config=Configuration();config.TreeCount=1;config.Herbivores=1;config.Carnivores=0;
        var fixture=new World(42,config);var tree=fixture.Map.Trees[0];
#if EVOSIM_FOREST
        int fixtureId=checked((int)fixture.TreeIdentity(0));
#else
        int fixtureId=0;
#endif
        var full=InspectTree(fixture,fixtureId);
        checks["slots_and_initial_stock"]=full["available"].AsInt32()==tree.Fruit.Length && Math.Abs(full["energy"].AsDouble()-tree.Fruit.Sum(f=>(double)f))<1e-5;
        tree.Fruit[0]=0;tree.Regrow[0]=1;
        checks["depleted_slot_visible"]=InspectTree(fixture,fixtureId)["available"].AsInt32()==tree.Fruit.Length-1;
        // Forest fertility scales the real regrowth rate; one day is not guaranteed.
        for(int day=0;day<20 && tree.Fruit[0]<=.01f;day++)fixture.Map.EcologyStep(false);
        checks["real_regrowth_visible"]=InspectTree(fixture,fixtureId)["available"].AsInt32()==tree.Fruit.Length;
        checks["position_matches"]=full["x"].AsSingle()==tree.X && full["y"].AsSingle()==tree.Y;
        fixture.Map.Trees.Clear();checks["world_without_tree_empty"]=InspectTree(fixture,fixtureId).Count==0;
        checks["passed"]=checks.Values.All(v=>v.AsBool());return checks;
    }
}
