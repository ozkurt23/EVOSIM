using EvoSim.Core;
using EvoSim.Observer;
using State = Godot.Collections.Dictionary;
using Rows = Godot.Collections.Array;

public partial class SimulationBridge
{
    private readonly PopulationInsightsCache insightsCache=new();
    private World? resourcesSource;
    private long resourcesTick=-1;
    private State? resourcesSnapshot;

    public State GeneDistributions()
    {
        var snapshot=insightsCache.Get(world);
        static State Diet(PopulationDietInsights diet)
        {
            var genes=new Rows();
            foreach(var g in diet.Genes)
                genes.Add(new State{{"bins",g.Bins.ToArray()},{"count",g.Count},
                    {"has_samples",g.Median.HasValue},{"minimum",g.Minimum??0},
                    {"median",g.Median??0},{"maximum",g.Maximum??0}});
            return new State{{"count",diet.Count},{"swimmers",diet.Swimmers},
                {"mature",diet.Mature},{"aging",diet.Aging},{"genes",genes}};
        }
        return new State{{"tick",snapshot.Tick},{"herbivores",Diet(snapshot.Herbivorous)},
            {"carnivores",Diet(snapshot.Carnivorous)}};
    }

    // Inventory is descriptive, never a sensory query, steering rule or resource mutation.
    public State ResourceInventory()
    {
        if(ReferenceEquals(resourcesSource,world) && resourcesSnapshot!=null &&
           world.Tick>=resourcesTick && world.Tick-resourcesTick<20)
            return resourcesSnapshot.Duplicate(true);
        resourcesSource=world;resourcesTick=world.Tick;
        int slots=0,available=0;double fruitEnergy=0,groundEnergy=0;
        foreach(var tree in world.Map.Trees)
            foreach(float energy in tree.Fruit){slots++;if(energy>.01f)available++;fruitEnergy+=energy;}
        foreach(float energy in world.Map.Food)groundEnergy+=energy;
        resourcesSnapshot=new State{{"tick",world.Tick},{"trees",world.Map.Trees.Count},
            {"fruit_available",available},{"fruit_slots",slots},{"fruit_energy",fruitEnergy},
            {"ground_energy",groundEnergy},{"water_access",world.Map.WaterLevel},
            {"drought",world.Drought},{"corpses",world.Corpses.Count}};
#if EVOSIM_FOREST
        resourcesSnapshot["living_trees"]=world.Forest.Trees.Count(t=>t.DeadDay<0);
        resourcesSnapshot["seedlings"]=world.Forest.Trees.Count(t=>t.DeadDay<0 && t.Growth<world.Forest.Settings.MaturityDays);
        resourcesSnapshot["dead_trunks"]=world.Forest.Trees.Count(t=>t.DeadDay>=0);
        resourcesSnapshot["seeds"]=world.Forest.SeedCount;
#endif
        return resourcesSnapshot.Duplicate(true);
    }

    public State ValidateResearchViews()
    {
        var checks=new State();string before=world.StateHash();
        var genes=GeneDistributions();var resources=ResourceInventory();
        var markers=GetMapMarkers();
        checks["map_markers_match_population"]=markers.Length==world.Herbivores+world.Carnivores && markers.Count(p=>p.Z>.5f)==world.Carnivores;
        checks["queries_preserve_world"]=before==world.StateHash();
        var h=genes["herbivores"].AsGodotDictionary();var c=genes["carnivores"].AsGodotDictionary();
        checks["living_counts_match"]=h["count"].AsInt32()==world.Herbivores && c["count"].AsInt32()==world.Carnivores;
        checks["fruit_matches_world_objects"]=resources["fruit_available"].AsInt32()==GetFruit().Count(row=>row.AsGodotDictionary()["available"].AsBool());
        resources["trees"]=-1;
        checks["snapshot_cannot_mutate_cache"]=ResourceInventory()["trees"].AsInt32()==world.Map.Trees.Count;
        var config=Configuration();config.Herbivores=3;config.Carnivores=0;
        var fixture=new World(73,config);
        for(int i=0;i<3;i++)for(int g=0;g<Genetics.Count;g++)fixture.Creatures[i].Genes[g]=i*.5f;
        fixture.Creatures[2].Alive=false;
        var sample=PopulationInsights.Inspect(fixture);
        checks["diet_boundary_and_dead_exclusion"]=sample.Herbivorous.Count==1 && sample.Carnivorous.Count==1;
        checks["known_bins"]=sample.Herbivorous.Genes[0].Bins[0]==1 && sample.Carnivorous.Genes[0].Bins[5]==1;
        fixture.Creatures[1].Genes[0]=1;
        sample=PopulationInsights.Inspect(fixture);
        checks["upper_endpoint_in_last_bin"]=sample.Carnivorous.Genes[0].Bins[9]==1 && sample.Carnivorous.Genes[0].Median==1;
        fixture.Creatures[0].Alive=false;fixture.Creatures[1].Alive=false;
        sample=PopulationInsights.Inspect(fixture);
        checks["empty_population_has_no_median"]=sample.Living==0 && sample.Herbivorous.Genes.All(g=>g.Median==null && g.Bins.Sum()==0);
        checks["passed"]=checks.Values.All(v=>v.AsBool());return checks;
    }
}
