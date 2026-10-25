using Godot;
using EvoSim.Core;
using System;
using System.Diagnostics;
using State = Godot.Collections.Dictionary;
using Rows = Godot.Collections.Array;

/// <summary>
/// Owns the unchanged, single-threaded simulation. Rendering only reads snapshots.
/// Wall-clock debt is retained when the per-frame CPU budget is exhausted.
/// </summary>
public partial class SimulationBridge : Node
{
    private World world;
    private readonly GroundFoodReference groundFoodReference = new();
    private double debt;
    private int seed;
    private double speedWindow, steppedWindow;
    private double effectiveSpeed=1;
    private readonly System.Collections.Generic.List<(long Tick, bool Dry)> climateCommands = new();
    public bool ClimateCycle { get; set; } = true;
    public string Scenario { get; private set; } = "normal";
    public bool Rain => !world.Drought && (Scenario == "rain" || (ClimateCycle && world.Time % 240 >= 160 && world.Time % 240 < 190));
    public void SetDry(bool dry)
    {
        if (world.Drought == dry) return;
        climateCommands.Add((world.Tick, dry));
        if (timeline != null) timeline.ExecuteDrought(dry); else world.SetDrought(dry);
    }
    public bool ConfigureScenario(string scenario)
    {
        try
        {
        if (scenario != "normal" && scenario != "cycle" && scenario != "empty" && scenario != "drought" && scenario != "rain") throw new ArgumentException("Unknown visual scenario: " + scenario);
        Scenario = scenario;
        if (scenario == "normal") return true;
        ClimateCycle = false;
        if (scenario == "cycle")
        {
            ClimateCycle = true;
            for (int i = 0; i < 4801; i++) { ClimateInput(); StepWorld(); }
        }
        if (scenario == "empty")
        {
            var config = Configuration(); config.Herbivores = 1; config.Carnivores = 0;
            config.FounderAgeMin = 0; config.FounderAgeMax = 0; config.HerbLifespan = .5f;
            config.CorpseLifetime = 5;
            world = CreateSimulationWorld(seed, config); timeline = EvoSim.Observer.TimelineSession.Create(world);
            for (int i = 0; i < 160; i++) StepWorld();
        }
        if (scenario == "drought" || scenario == "rain")
        {
            SetDry(true);
            for (int i = 0; i < 2400; i++) StepWorld();
            if (scenario == "rain") SetDry(false);
        }
        return true;
        }
        catch (Exception error) { GD.PushError(error.Message); return false; }
    }
    private void ClimateInput()
    {
        if (ClimateCycle && timeline?.IsPreviewingHistory != true) SetDry(world.Time % 240 >= 80 && world.Time % 240 < 160);
    }
    public bool Paused { get; set; }
    public int Speed { get; set; } = 1;
    public double FrameBudgetMs { get; set; } = 7;
    public long CurrentTick => world?.Tick ?? 0;

    public static WorldConfig Configuration() => new WorldConfig
    {
        Herbivores = 70, Carnivores = 8, TreeCount = 300,
        LandRegions = 3, RockClusters = 9
    };

    public void Initialize(int worldSeed = 42)
    {
        seed = worldSeed;
        world = CreateSimulationWorld(seed, Configuration());
        debt = 0;
        climateCommands.Clear();
        EnsureStore(); timeline = EvoSim.Observer.TimelineSession.Create(world);
    }

    public int Advance(double delta)
    {
        if (timeline?.IsSeeking == true) { timeline.PumpSeek(); world=timeline.DisplayWorld; return 0; }
        if (Paused) return 0;
        // Ignore long OS suspends; normal rendering jitter does not lose tick debt.
        debt += Math.Clamp(delta, 0, .25) * Math.Clamp(Speed, 1, 20);
        int count = 0;
        var timer = Stopwatch.StartNew();
        while (debt + 1e-10 >= World.Dt && count < 160 && timer.Elapsed.TotalMilliseconds < Math.Min(Math.Clamp(FrameBudgetMs,2,12),3+Speed*.3))
        {
            ClimateInput();
            StepWorld();
            debt -= World.Dt;
            count++;
        }
        if(delta>0) { speedWindow+=delta;steppedWindow+=count*World.Dt;
            if(speedWindow>=1){effectiveSpeed=steppedWindow/speedWindow;speedWindow=0;steppedWindow=0;}}
        return count;
    }

    public State GetMap()
    {
        var trees = new Rows();
        for(int index=0;index<world.Map.Trees.Count;index++)
        {
            var t=world.Map.Trees[index];
            var row=new State { { "id",TreeId(index) }, { "x", t.X }, { "z", t.Y }, { "radius", t.Radius } };
            AddTreeEcology(row,index);trees.Add(row);
        }
        return new State
        {
            { "size", world.Map.Size }, { "tiles", world.Map.Tiles },
            { "food", world.Map.Food }, { "food_reference", groundFoodReference.For(world) }, { "moisture", world.Map.Moisture }, { "trees", trees }
        };
    }

    public Vector3[] GetMapMarkers() => world.Creatures.Where(c=>c.Alive)
        .Select(c=>new Vector3(c.X,c.Y,c.Genes[2]>=.5f?1:0)).ToArray();

    public Rows GetAgents() => GetAgentsRegion(float.NegativeInfinity,float.NegativeInfinity,float.PositiveInfinity,float.PositiveInfinity);

    public Rows GetAgentsRegion(float minX,float minY,float maxX,float maxY)
    {
        var rows = new Rows();
        foreach (var c in world.Creatures)
        {
            if (!c.Alive || c.X<minX || c.Y<minY || c.X>maxX || c.Y>maxY) continue;
            rows.Add(new State
            {
                { "id", c.Id }, { "x", c.X }, { "z", c.Y }, { "angle", c.Angle },
                { "radius", c.Radius }, { "predator", c.Genes[2] >= .5f },
                { "age", c.Age }, { "aging", world.Aging(c) }, { "energy", c.Energy },
                { "max_energy", c.MaxEnergy }, { "water", c.Hydration }, { "boldness", c.Genes[4] },
                { "action", c.Action }, { "distance", c.DistanceTravelled }, { "damage", c.RecentDamage },
                { "health", c.Health }, { "maturity", c.Maturity }, { "swimming", world.Map.IsWater(c.X,c.Y) },
                { "parent_a", c.ParentA }, { "parent_b", c.ParentB }, { "generation", c.Generation }, { "speed", MathF.Sqrt(c.Vx*c.Vx + c.Vy*c.Vy) }
            });
        }
        return rows;
    }

    public State Stats()
    {
        double energy = 0, water = 0;
        int living = 0;
        foreach (var creature in world.Creatures)
        {
            if (!creature.Alive) continue;
            energy += Math.Clamp(100.0 * creature.Energy / Math.Max(1, creature.MaxEnergy), 0, 100);
            water += Math.Clamp(creature.Hydration, 0, 100);
            living++;
        }
        return new State
        {
            { "effective_speed", effectiveSpeed }, { "tick", world.Tick }, { "days", world.Time },
            { "herbivores", world.Herbivores }, { "carnivores", world.Carnivores }, { "pending", debt },
            { "drought", world.Drought }, { "rain", Rain }, { "water_level", world.Map.WaterLevel },
            { "climate_commands", climateCommands.Count }, { "births", world.Births }, { "deaths", world.Deaths }, { "scenario", Scenario },
            { "mean_energy", living == 0 ? 0 : energy / living },
            { "mean_water", living == 0 ? 0 : water / living }
        };
    }

    public Rows GetCorpses()
    {
        var rows = new Rows();
        foreach (var corpse in world.Corpses)
        {
            var original = world.Creatures.Find(c => c.Id == corpse.CreatureId);
            rows.Add(new State { { "id", corpse.CreatureId }, { "x", corpse.X }, { "z", corpse.Y },
                { "predator", original != null && original.Genes[2] >= .5f },
                { "radius", original?.Radius ?? .3f },
                { "decay", Math.Clamp((world.Tick-corpse.CreatedTick)*World.Dt/Math.Max(.01f,world.Config.CorpseLifetime),0,1) } });
        }
        return rows;
    }

    public float[] GetGroundFood() => world.Map.Food;

    public Rows GetFruit() => GetFruitRegion(float.NegativeInfinity,float.NegativeInfinity,float.PositiveInfinity,float.PositiveInfinity);

    public Rows GetFruitRegion(float minX,float minY,float maxX,float maxY)
    {
        var rows = new Rows();
        foreach (var t in world.Map.Trees)
        {
            if(t.X<minX||t.Y<minY||t.X>maxX||t.Y>maxY)continue;
            for (int slot = 0; slot < t.Fruit.Length; slot++)
            {
                var p = t.FruitPosition(slot);
                rows.Add(new State
                {
                    { "x", p.X }, { "z", p.Y }, { "tree_x", t.X }, { "tree_z", t.Y },
                    { "slot", slot }, { "available", t.Fruit[slot] > .01f }, { "radius", t.Radius }
                });
            }
        }
        return rows;
    }

    /// <summary>Replay without any rendering; equal hashes detect adapter side effects.</summary>
    public State Validate()
    {
#if EVOSIM_FOREST
        var reference = new World(seed,world.Config,world.Forest.Settings);
#else
        var reference = CreateSimulationWorld(seed, world.Config);
#endif
        int command = 0;
        for (long t = 0; t <= world.Tick; t++)
        {
            while (command < climateCommands.Count && climateCommands[command].Tick == t)
                reference.SetDrought(climateCommands[command++].Dry);
            if (t < world.Tick) reference.Step();
        }
        return new State
        {
            { "passed", reference.StateHash() == world.StateHash() },
            { "tick", world.Tick }, { "hash", world.StateHash() }, { "live", world.Living }
        };
    }
    /// <summary>Exercise the actual adapter accumulator at different render cadences.</summary>
    public State ValidateClocks()
    {
        var cases = new Rows();
        bool passed = true;
        foreach (int speed in new[] { 1, 20 })
        foreach (int fps in new[] { 30, 60, 144 })
        {
            var adapter = new SimulationBridge();
            adapter.Initialize(seed);
            adapter.Speed = speed;
            for (int f = 0; f < fps * 2; f++) adapter.Advance(1.0 / fps);
            for (int drain = 0; drain < 1000 && adapter.debt + 1e-10 >= World.Dt; drain++)
                adapter.Advance(0);
            long expected = (long)Math.Floor(2.0 * speed / World.Dt);
            var direct = CreateSimulationWorld(seed, Configuration());
            for (long t = 0; t < expected; t++) direct.Step();
            bool equal = adapter.CurrentTick == expected && adapter.world.StateHash() == direct.StateHash();
            cases.Add(new State { { "fps", fps }, { "speed", speed },
                { "tick", adapter.CurrentTick }, { "expected", expected }, { "passed", equal } });
            passed &= equal;
            adapter.Free();
        }
        return new State { { "passed", passed }, { "cases", cases } };
    }

}
