using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using EvoSim.Core;

// No external test framework is needed: this runner works with the same Core
// assembly as the observer and returns a failing process status on an invariant failure.
internal static partial class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static int Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            if (options.Help)
            {
                Console.WriteLine("EVOSIM checks and calibration\n" +
                    "  --selftest                 Run deterministic Core checks (the default).\n" +
                    "  --ticks 12000 --seed 42     Run a bounded headless benchmark.\n" +
                    "  --map-size 256             Use a 128, 192 or 256 cell square map.\n" +
                    "  --herbivores 450            Set the literal founder herbivore count.\n" +
                    "  --carnivores 50             Set the literal founder carnivore count.\n" +
                    "  --trees 250                 Set the literal tree count; no density scaling.\n" +
                    "  --drought-at 4000          Enable drought at that simulation tick.\n" +
                    "  --drought                  Enable drought from the beginning.\n" +
                    "  --help                     Show this help.\n\n" +
                    "Results are JSON. Extinction is a reported ecological outcome, not a test failure.\n" +
                    "Measurements describe this machine, runtime and initial preview model.");
                return 0;
            }
            return options.SelfTest ? RunSelfTests() : RunBenchmark(options);
        }
        catch (Exception e)
        {
            Console.WriteLine(JsonSerializer.Serialize(new { mode = "error", error = e.Message }, JsonOptions));
            return 2;
        }
    }

    private static int RunSelfTests()
    {
        var results = new List<object>();
        var failures = 0;
        void Check(string name, Func<object> run)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                var details = run();
                results.Add(new { name, passed = true, elapsedMs = watch.Elapsed.TotalMilliseconds, details });
            }
            catch (Exception e)
            {
                failures++;
                results.Add(new { name, passed = false, elapsedMs = watch.Elapsed.TotalMilliseconds, error = e.Message });
            }
        }

        Check("checkpoint preserves exact future and custom settings", CheckCheckpoint);
        Check("procedural region count and custom founders", CheckConfiguration);
        Check("neural motor authority and inherited controller", CheckNeuralControl);
        Check("clustered grass, edible fruit and delayed regrowth", CheckFruit);
        Check("corpse expiry and early physiological needs", CheckCorpseAndNeeds);
        Check("aging slows the same body", CheckAge);
        Check("independent lifespans and ageless bodies", CheckImmortality);
        Check("breeding diagnostics gates, contacts and read-only future", CheckBreedingDiagnostics);
        Check("breeding modes preserve neural authority and full budgets", CheckBreedingModes);
        Check("reproductive drive changes the input, not forced births", CheckReproductiveDrive);
        Check("tree profile defaults, geometry and resource effects", CheckTreeProfiles);
        Check("benchmark scale options preserve defaults and literal counts", CheckScaleOptions);
        Check("large terrain regions, independent bounds and southeast collision", CheckLargeTerrain);
        Check("large grass, corpse and fruit resource identities remain distinct", CheckLargeResourceIdentity);
        Check("spatial index queries match brute force across large coordinates", CheckLargeSpatialQueries);
        Check("interleaved world sizes and large checkpoint futures", CheckLargeWorldIndependence);
        Check("parent interval inheritance", CheckInheritance);
        Check("birth level mutation rate and equal parent variation", CheckMutation);
        Check("fixed seed, restart and render cadence independence", CheckDeterminism);
        Check("river barriers, swimmers, swept paths and reachable shores", CheckTerrain);
        Check("unique descendants, exact grandchildren and living filters", CheckDescendants);
        Check("drought changes resource ecology without rewriting genomes", CheckDrought);
        Check("state, movement, lifecycle and genealogy invariants", CheckLifecycle);

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            mode = "selftest", passed = failures == 0, failures,
            environment = RunEnvironment(),
            scope = "v0.4.1 dynamic world sizes, resource identities, spatial queries, checkpoints, breeding and inherited neural controller checks. No claim that escape has already evolved.",
            results
        }, JsonOptions));
        return failures == 0 ? 0 : 1;
    }

    private static object CheckImmortality()
    {
        var cfg=new WorldConfig { Herbivores=1,Carnivores=1,HerbLifespan=0,CarnLifespan=500,HerbAging=0 };
        var w=new World(42,cfg);var herb=w.Creatures[0];var carn=w.Creatures[1];
        herb.Age=10000;carn.Age=501;herb.Energy=herb.MaxEnergy;carn.Energy=carn.MaxEnergy;
        herb.Hydration=carn.Hydration=100;herb.Health=carn.Health=100;
        w.Step();Require(herb.Alive&&!carn.Alive&&carn.DeathCause=="yaşlılık","Per-group lifespan ignored.");
        Require(w.Aging(herb)==0,"Ageless creature still senesces.");
        using var bytes=new MemoryStream();w.Save(bytes);bytes.Position=0;var loaded=World.Load(bytes);
        Require(loaded.StateHash()==w.StateHash()&&loaded.Config.HerbLifespan==0,"Immortal setting lost in save.");
        herb.Health=0;w.Step();Require(!herb.Alive,"Immortality prevented non-age death.");
        return new { immortalSurvivesAge=true,mortalDiesAtLimit=true,ageless=true,otherDeathStillWorks=true };
    }

    private static object CheckCheckpoint(){
        var cfg=new WorldConfig{Herbivores=30,Carnivores=8,LandRegions=4,FruitSlots=9,FruitEnergy=22,MutationRate=.15f};
        var original=new World(891,cfg);for(int i=0;i<411;i++)original.Step();original.SetDrought(true);
        using var bytes=new MemoryStream();original.Save(bytes);bytes.Position=0;var loaded=World.Load(bytes);
        Require(original.StateHash()==loaded.StateHash(),"Checkpoint lost current state.");
        Require(loaded.Config.MutationRate==cfg.MutationRate&&loaded.Map.Trees[0].Fruit.Length==9&&loaded.Config.LandRegions==4,"World config was not restored.");
        for(int i=0;i<400;i++){original.Step();loaded.Step();}
        Require(original.StateHash()==loaded.StateHash(),"Checkpoint continuation diverged, including RNG/brain/fruit timers.");
        return new{savedAtTick=411,continuedTicks=400,bytes=bytes.Length,hash=loaded.StateHash()};
    }
    private static object CheckConfiguration(){
        int checkedMaps=0;
        foreach(int seed in new[]{7,42,135})for(int regions=1;regions<=5;regions++){
            var cfg=new WorldConfig{LandRegions=regions,Herbivores=12,Carnivores=4,TreeCount=30,MeadowCount=3};
            var w=new World(seed,cfg);Require(w.Map.LandRegionCount()==regions,$"Seed {seed}: expected {regions} land regions, got {w.Map.LandRegionCount()}.");Require(w.Living==16&&w.Map.Trees.Count==30,"Custom counts ignored.");checkedMaps++;
        }
        var exact=new WorldConfig{Herbivores=5,Carnivores=3,SwimmerChance=0};exact.HerbMin=exact.HerbMax=new[]{.4f,.5f,.1f,.2f,.3f,.6f};exact.CarnMin=exact.CarnMax=new[]{.7f,.6f,.9f,.1f,.8f,.2f};
        var world=new World(42,exact);Require(world.Creatures.Take(5).All(c=>c.Genes.SequenceEqual(exact.HerbMin))&&world.Creatures.Skip(5).All(c=>c.Genes.SequenceEqual(exact.CarnMin)),"Founder trait ranges were ignored.");
        bool rejected=false;try{new World(1,new WorldConfig{MutationRate=2});}catch(ArgumentException){rejected=true;}Require(rejected,"Invalid probability accepted.");
        return new{checkedMaps,independentGroupRanges=true,invalidConfigRejected=true};
    }

    private static void ConstantBrain(Creature c,float drive=0,float turn=0,float interact=0,float bite=0,float mate=0)
    {
        Array.Clear(c.BrainWeights);Array.Clear(c.BrainMemory);Array.Clear(c.Motors);
        int offset=Brain.Hidden*(Brain.Inputs+Brain.Hidden+1);
        float[] values={drive,turn,interact,bite,mate};
        for(int i=0;i<5;i++)c.BrainWeights[offset+i*(Brain.Hidden+1)]=values[i];
    }
    private static object CheckNeuralControl()
    {
        var world=new World(42);var a=world.Creatures[0];var b=world.Creatures[150];
        var point=world.Map.FindSpawn(new PcgRandom(17,3),3,0);
        a.X=point.X;a.Y=point.Y;b.X=point.X+1.2f;b.Y=point.Y;
        world.Creatures.Clear();world.Creatures.Add(a);world.Creatures.Add(b);
        a.Genes[2]=0;b.Genes[2]=1;ConstantBrain(a);ConstantBrain(b);
        var weights=a.BrainWeights.ToArray();float x=a.X,y=a.Y;
        for(int i=0;i<40;i++)world.Step();
        Require(Math.Abs(a.X-x)+Math.Abs(a.Y-y)<.001f,"Zero controller was overridden by automatic predator escape.");
        Require(a.Health>99&&b.Health>99,"A bite happened without a neural bite command.");
        world.Creatures.Remove(b);ConstantBrain(a,.8f,.08f);a.Angle=0;
        for(int i=0;i<40;i++)world.Step();
        Require(Vector2.Distance(new(x,y),new(a.X,a.Y))>.4f,"Motor output did not move body.");
        var after=a.BrainWeights.ToArray();for(int i=0;i<40;i++)world.Step();
        Require(after.SequenceEqual(a.BrainWeights),"Controller weights silently trained during life.");
        float[] pa=Enumerable.Repeat(-.5f,Brain.Count).ToArray(),pb=Enumerable.Repeat(.8f,Brain.Count).ToArray();
        var child=Brain.Inherit(pa,pb,new PcgRandom(55,7));
        Require(child.All(v=>v>=-.5f&&v<=.8f)&&child.Distinct().Count()>100,"Controller inheritance is not continuous within parent values.");
        return new{noAutomaticEscape=true,noAutomaticRetaliation=true,motorMovement=true,inheritedWeights=Brain.Count,lifetimeWeightUpdates=false};
    }
    private static object CheckFruit()
    {
        var world=new World(42);int active=world.Map.Food.Count(f=>f>0);
        Require(active>30&&active<world.Map.Food.Length*.15,"Grass no longer occupies small finite meadows.");
        var tree=world.Map.Trees.First(t=>world.Map.CanOccupy(t.FruitPosition(0).X,t.FruitPosition(0).Y,.5f,0));
        var c=world.Creatures[0];world.Creatures.Clear();world.Creatures.Add(c);ConstantBrain(c);
        var point=tree.FruitPosition(0);c.X=point.X;c.Y=point.Y;c.Energy=10;c.Hydration=100;c.Genes[2]=0;
        float initial=tree.Fruit[0],energy=c.Energy;
        for(int i=0;i<20;i++)world.Step();
        Require(tree.Fruit[0]<initial&&c.Energy>energy,"Hungry mouth in reach did not actually eat fruit.");
        tree.Fruit[0]=0;tree.Regrow[0]=45;
        for(int i=0;i<44;i++)world.Map.EcologyStep(false);
        Require(tree.Fruit[0]==0,"Fruit respawned before its cooldown.");
        world.Map.EcologyStep(false);Require(tree.Fruit[0]==18,"Fruit failed to regrow after cooldown.");
        int dryDelay=0;tree.Fruit[0]=0;tree.Regrow[0]=45;world.Map.WaterLevel=.12f;
        while(tree.Fruit[0]==0&&dryDelay<250){world.Map.EcologyStep(true);dryDelay++;}
        Require(dryDelay>45,"Drought did not delay fruit regrowth.");
        return new{grassCells=active,totalCells=world.Map.CellCount,fruitSlotsPerTree=Tree.FruitSlots,fruitEnergy=18,normalRegrowSeconds=45,dryRegrowSeconds=dryDelay};
    }
    private static object CheckCorpseAndNeeds()
    {
        var world=new World(42);world.Creatures.Clear();world.Corpses.Clear();
        world.Corpses.Add(new Corpse{X=10,Y=10,Food=500,CreatedTick=0,CreatureId=999});
        for(int i=0;i<1799;i++)world.Step();Require(world.Corpses.Count==1,"Corpse TTL was shorter than configured.");
        world.Step();Require(world.Corpses.Count==0,"Corpse survived its 90 second TTL.");
        Require(World.NeedPressure(.8f,.6f)==0,"Satiated animal has hunger pressure.");
        Require(World.NeedPressure(.4f,.6f)>World.NeedPressure(.55f,.6f),"Hunger does not grow below onset.");
        Require(World.NeedPressure(.15f,.6f)>1,"Critical deficit does not amplify pressure.");
        return new{corpseExpirySeconds=90,hungerOnset=.60,thirstOnset=.65,criticalThreshold=.25};
    }
    private static object CheckAge()
    {
        var young=new World(42);var old=new World(42);var a=young.Creatures[0];var b=old.Creatures[0];
        young.Creatures.Clear();old.Creatures.Clear();young.Creatures.Add(a);old.Creatures.Add(b);
        a.Age=200;b.Age=1100;ConstantBrain(a);ConstantBrain(b);young.Step();old.Step();
        Require(b.Speed<a.Speed*.6f,"Old identical body did not lose speed.");
        return new{youngSpeed=a.Speed,oldSpeed=b.Speed};
    }

    private static object CheckInheritance()
    {
        float[] a = { .12f, .82f, .20f, .33f, .98f, .01f };
        float[] b = { .71f, .21f, .85f, .33f, .99f, .90f };
        var inherit = new PcgRandom(1801, 1);
        var mutation = new PcgRandom(1802, 2);
        var aBefore = a.ToArray();
        var bBefore = b.ToArray();
        const int samples = 10000;
        var lowHalf = 0;
        var highHalf = 0;
        var uniqueValues = new HashSet<float>();
        for (var n = 0; n < samples; n++)
        {
            var child = Genetics.Inherit(a, b, inherit, mutation, out var mutated, out _, 0);
            Require(!mutated, "Mutation was recorded with zero mutation probability.");
            Require(child.Length == 6, "The preview genome must contain six loci.");
            for (var i = 0; i < child.Length; i++)
                Require(child[i] >= Math.Min(a[i], b[i]) && child[i] <= Math.Max(a[i], b[i]),
                    $"Unmutated locus {i} lies outside its parent interval.");
            if (child[0] < .3f) lowHalf++;
            if (child[0] > .5f) highHalf++;
            uniqueValues.Add(child[0]);
        }
        Require(a.SequenceEqual(aBefore) && b.SequenceEqual(bBefore), "Inheritance changed a parent genome.");
        Require(lowHalf > 1500 && highHalf > 1500,
            "Inheritance did not sample the parental interval broadly; check for averaging or binary selection.");
        Require(uniqueValues.Count > 9000, "Inheritance collapsed to discrete parent/average values.");
        return new { samples, lowerIntervalSamples = lowHalf, upperIntervalSamples = highHalf, uniqueValues = uniqueValues.Count };
    }

    private static object CheckMutation()
    {
        var a = Enumerable.Repeat(.5f, 6).ToArray();
        var inherit = new PcgRandom(2201, 1);
        var mutation = new PcgRandom(2202, 2);
        const int samples = 100000;
        var events = 0;
        var changed = 0;
        var positive = 0;
        var negative = 0;
        for (var n = 0; n < samples; n++)
        {
            var child = Genetics.Inherit(a, a, inherit, mutation, out var mutated, out var locus);
            foreach (var gene in child)
                Require(float.IsFinite(gene) && gene >= 0 && gene <= 1, "Mutation produced an invalid gene.");
            if (!mutated)
            {
                Require(child.All(x => x == .5f), "Equal parents changed without a mutation event.");
                continue;
            }
            events++;
            Require(locus >= 0 && locus < 6, "Mutation event has an invalid locus.");
            for (var i = 0; i < child.Length; i++)
                if (i != locus) Require(child[i] == .5f, "One birth mutation changed multiple loci.");
            if (child[locus] != .5f) changed++;
            if (child[locus] > .5f) positive++;
            if (child[locus] < .5f) negative++;
        }
        var expected = samples * .001;
        var sixSigma = 6 * Math.Sqrt(samples * .001 * .999);
        Require(events >= Math.Floor(expected - sixSigma) && events <= Math.Ceiling(expected + sixSigma),
            $"Observed {events}/{samples} birth mutations; outside the predeclared six-sigma binomial band.");
        Require(changed > 0 && positive > 0 && negative > 0,
            "Equal-parent mutation must be able to produce variation in both directions.");

        // Forced events exercise boundary handling without waiting for a rare event near a bound.
        var nearBounds = new[] { 0f, 1f, .001f, .999f, 0f, 1f };
        for (var n = 0; n < 20000; n++)
        {
            var child = Genetics.Inherit(nearBounds, nearBounds, inherit, mutation, out var mutated, out _, 1);
            Require(mutated, "A certain mutation event was skipped.");
            Require(child.All(x => float.IsFinite(x) && x >= 0 && x <= 1), "Boundary mutation escaped [0,1].");
        }
        return new { samples, events, changed, positive, negative, expectedProbabilityPerBirth = .001,
            predeclaredAcceptanceBand = new[] { (int)Math.Floor(expected - sixSigma), (int)Math.Ceiling(expected + sixSigma) } };
    }

    private static object CheckDeterminism()
    {
        var sequential = new World(42);
        var batched = new World(42);
        var other = new World(43);
        var initialHash = sequential.StateHash();
        Require(initialHash.Equals(batched.StateHash()), "Equal initial seeds differ.");
        Require(!initialHash.Equals(other.StateHash()), "Different seeds generated the same world state.");
        const int ticks = 600;
        for (var i = 0; i < ticks; i++) sequential.Step();
        var batches = new[] { 1, 4, 16, 3, 9, 2, 8 };
        var advanced = 0;
        var batchIndex = 0;
        while (advanced < ticks)
        {
            var count = Math.Min(batches[batchIndex++ % batches.Length], ticks - advanced);
            for (var i = 0; i < count; i++) batched.Step();
            // Reading observer data between draw-sized batches must not advance simulation state.
            _ = batched.Creatures.Count;
            _ = batched.StateHash();
            advanced += count;
        }
        Require(sequential.Tick == ticks && batched.Tick == ticks, "The fixed tick count is incorrect.");
        Require(sequential.StateHash().Equals(batched.StateHash()), "Draw-sized step batches changed world state.");
        var restarted = new World(42);
        Require(initialHash.Equals(restarted.StateHash()), "Restart did not reconstruct the initial seed.");
        for (var i = 0; i < ticks; i++) restarted.Step();
        Require(sequential.StateHash().Equals(restarted.StateHash()), "Restarted run did not reproduce the trajectory.");
        return new { seed = 42, ticks, initialHash, finalHash = sequential.StateHash() };
    }

    private static object CheckDrought()
    {
        var normal = new World(91);
        var dry = new World(91);
        var genomes = dry.Creatures.ToDictionary(c => c.Id, c => c.Genes.ToArray());
        dry.SetDrought(true);
        Require(dry.Creatures.All(c => genomes[c.Id].SequenceEqual(c.Genes)), "Enabling drought rewrote existing genomes.");
        var beforeFood = TotalFood(normal);
        // Exercise ecology alone: consumers must not obscure the direct effect on resources.
        for (var i = 0; i < 120; i++)
        {
            normal.Map.EcologyStep(false);
            dry.Map.EcologyStep(true);
        }
        var normalFood = TotalFood(normal);
        var droughtFood = TotalFood(dry);
        Require(droughtFood < normalFood - .001, "Drought did not reduce available plant food versus the same initial map.");
        Require(dry.Creatures.All(c => genomes[c.Id].SequenceEqual(c.Genes)), "Ecology rewrote creature genomes.");
        return new { ecologySteps = 120, beforeFood, normalFood, droughtFood };
    }

    private static object CheckTerrain()
    {
        const float radius = .35f;
        var map = new Terrain(81);
        var riverChecked = false;
        for (var y = 64; y < 116 && !riverChecked; y++)
        for (var x = 40; x < 98 && !riverChecked; x++)
        {
            if (map.Tiles[y * map.Size + x] != 1 || map.Tiles[y * map.Size + x - 1] != 0) continue;
            var end = x;
            while (end < 110 && map.Tiles[y * map.Size + end] == 1) end++;
            if (end - x < 4) continue;
            var left = new Vector2(x - .5f, y + .5f);
            var right = new Vector2(end + .5f, y + .5f);
            if (!map.CanOccupy(left.X, left.Y, radius, 0) || !map.CanOccupy(right.X, right.Y, radius, 0) ||
                !map.HasClearPath(left, right, radius, 1)) continue;
            Require(!map.HasClearPath(left, right, radius, 0), "A land creature can tunnel across a river.");
            Require(map.FindPath(left, right, radius, 0).Count == 0, "A land route crosses the unbroken main river.");
            var swimmingRoute = map.FindPath(left, right, radius, 1);
            Require(swimmingRoute.Count > 0, "An aquatic route cannot cross permitted water.");
            ValidatePath(map, left, swimmingRoute, radius, 1);
            riverChecked = true;
        }
        Require(riverChecked, "No suitable river-crossing fixture was found in the reference terrain.");

        var rockChecked = false;
        var detourWaypoints = 0;
        for (var y = 5; y < 120 && !rockChecked; y++)
        for (var x = 5; x < 120 && !rockChecked; x++)
        {
            if (map.Tiles[y * map.Size + x] != 2) continue;
            var left = new Vector2(x - 1.5f, y + .5f);
            var right = new Vector2(x + 2.5f, y + .5f);
            if (!map.CanOccupy(left.X, left.Y, radius, 0) || !map.CanOccupy(right.X, right.Y, radius, 0)) continue;
            var detour = map.FindPath(left, right, radius, 0);
            if (detour.Count == 0) continue;
            Require(!map.HasClearPath(left, right, radius, 1), "Swimming incorrectly permits passage through rock.");
            Require(detour.Count > 1, "The route did not detour around the rock outcrop.");
            ValidatePath(map, left, detour, radius, 0);
            Require(Vector2.Distance(detour[^1], right) < .001f, "The detour does not end at its requested goal.");
            detourWaypoints = detour.Count;
            rockChecked = true;
        }
        Require(rockChecked, "No suitable rock-detour fixture was found in the reference terrain.");

        // Direct geometry queries do not use cached land components; no pathfinding
        // is requested after editing this isolated geometry fixture.
        var corners = new Terrain(12);
        Array.Clear(corners.Tiles);
        corners.Tiles[63 * corners.Size + 64] = 2;
        corners.Tiles[64 * corners.Size + 63] = 2;
        Require(!corners.HasClearPath(new Vector2(63.5f, 63.5f), new Vector2(64.5f, 64.5f), radius, 0),
            "A diagonal motion cuts through touching obstacle corners.");
        Require(!corners.CanOccupy(.1f, .1f, radius, 1) && !corners.CanOccupy(float.NaN, 1, radius, 0),
            "World boundary or non-finite coordinates were accepted.");

        var natural = new Terrain(42);
        var repeated = new Terrain(42);
        Require(natural.Tiles.SequenceEqual(repeated.Tiles) && natural.Food.SequenceEqual(repeated.Food) &&
            natural.Moisture.SequenceEqual(repeated.Moisture), "Repeated terrain seed changed its output.");
        var rng = new PcgRandom(909, 1);
        var shoresChecked = 0;
        for (var n = 0; n < 20; n++)
        {
            var start = natural.FindSpawn(rng, radius, 0);
            var shore = natural.NearestShore(start.X, start.Y, natural.Size);
            if (!float.IsFinite(shore.X)) continue;
            Require(!natural.IsWater(shore.X, shore.Y) && natural.CanOccupy(shore.X, shore.Y, radius, 0),
                "Drinking shore lies in forbidden terrain.");
            var route = natural.FindPath(start, shore, radius, 0);
            Require(route.Count > 0, "A shore query returned a point unreachable from its land component.");
            ValidatePath(natural, start, route, radius, 0);
            shoresChecked++;
        }
        Require(shoresChecked >= 10, "Natural shore reachability checks were insufficiently exercised.");
        return new { riverChecked, rockChecked, detourWaypoints, shoresChecked };
    }

    private static void ValidatePath(Terrain map, Vector2 from, List<Vector2> path, float radius, float aquatic)
    {
        foreach (var to in path)
        {
            var samples = Math.Max(1, (int)Math.Ceiling(Vector2.Distance(from, to) / .08f));
            for (var i = 0; i <= samples; i++)
            {
                var point = Vector2.Lerp(from, to, (float)i / samples);
                Require(map.CanOccupy(point.X, point.Y, radius, aquatic), "A* waypoint segment crosses blocked terrain.");
            }
            from = to;
        }
    }

    private static object CheckLifecycle()
    {
        const int ticks = 2400;
        var world = new World(42);
        var auditor = new WorldAuditor(world);
        for (var i = 0; i < ticks; i++)
        {
            world.Step();
            auditor.Inspect(world);
        }
        // Lifecycle code must actually run for this check to establish more than spawn validity.
        Require(world.Births > 0, "This reference scenario produced no births; reproduction invariants were not exercised.");
        Require(world.Deaths > 0, "This reference scenario produced no deaths; death invariants were not exercised.");
        return new { ticks, world.Births, world.Deaths, world.Mutations,
            alive = world.Creatures.Count(c => c.Alive), checkedMovementSegments = auditor.MovementSegments,
            checkedBirths = auditor.CheckedBirths, finalHash = world.StateHash() };
    }

    private static object CheckDescendants()
    {
        // A small pedigree with shared ancestors deliberately gives the same
        // descendant several paths from the selected individual.
        var world = new World(11);
        world.Creatures.Clear();
        world.Creatures.AddRange(new[]
        {
            new Creature { Id = 1, Generation = 1 },
            new Creature { Id = 6, Generation = 1 },
            new Creature { Id = 2, ParentA = 1, ParentB = 6, Generation = 2 },
            new Creature { Id = 3, ParentA = 1, ParentB = 6, Generation = 2, Alive = false },
            new Creature { Id = 4, ParentA = 2, ParentB = 3, Generation = 3 },
            new Creature { Id = 5, ParentA = 4, ParentB = 2, Generation = 4 }
        });
        void Expect(int depth, bool livingOnly, int[] expected)
        {
            var actual = world.GetDescendants(1, depth, livingOnly).Select(c => c.Id).Order().ToArray();
            Require(actual.SequenceEqual(expected), $"Depth {depth}, livingOnly={livingOnly}: got [{string.Join(',', actual)}], expected [{string.Join(',', expected)}].");
        }
        Expect(1, false, new[] { 2, 3 });
        Expect(2, false, new[] { 4, 5 });
        Expect(0, false, new[] { 2, 3, 4, 5 });
        Expect(0, true, new[] { 2, 4, 5 });
        Require(world.GetDescendants(5).Count == 0, "A childless creature returned descendants.");
        return new { directChildren = 2, exactSecondGeneration = 2, uniqueDescendants = 4, livingDescendants = 3 };
    }

    private static int RunBenchmark(Options options)
    {
        var world = new World(options.Seed, options.CreateWorldConfig());
        var auditor = new WorldAuditor(world);
        var initialAlive = world.Creatures.Count(c => c.Alive);
        var initialFood = TotalFood(world);
        var samples = new List<object>();
        var maximumAlive = initialAlive;
        var extinctionTick = -1;
        var measuredTicks = 0;
        var elapsedTicks = new List<double>();
        var memoryBefore = GC.GetTotalMemory(true);
        var watch = Stopwatch.StartNew();
        for (var i = 0; i < options.Ticks; i++)
        {
            if (i == options.DroughtAt) world.SetDrought(true);
            var tickStart = Stopwatch.GetTimestamp();
            world.Step();
            elapsedTicks.Add(Stopwatch.GetElapsedTime(tickStart).TotalMilliseconds);
            measuredTicks++;
            auditor.Inspect(world);
            var alive = world.Creatures.Count(c => c.Alive);
            maximumAlive = Math.Max(maximumAlive, alive);
            if (alive == 0 && extinctionTick < 0) extinctionTick = (int)world.Tick;
            if ((i + 1) % 1200 == 0 || i == options.Ticks - 1)
                samples.Add(new { tick = world.Tick, alive, world.Births, world.Deaths, world.Mutations, food = TotalFood(world) });
        }
        watch.Stop();
        elapsedTicks.Sort();
        var elapsed = watch.Elapsed.TotalSeconds;
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            mode = "benchmark", invariantChecksPassed = true, environment = RunEnvironment(), options.Seed,
            configuration = new { mapSize = world.Map.Size, world.Config.GeneratorVersion,
                herbivores = world.Config.Herbivores, carnivores = world.Config.Carnivores,
                requestedTrees = world.Config.TreeCount, actualTrees = world.Map.Trees.Count,
                countsAreLiteral = true },
            requestedTicks = options.Ticks, completedTicks = measuredTicks,
            simulatedSeconds = measuredTicks * .05,
            droughtAtTick = options.DroughtAt < 0 ? (int?)null : options.DroughtAt,
            initialAlive, finalAlive = world.Creatures.Count(c => c.Alive), maximumAlive,
            extinctionTick = extinctionTick < 0 ? (int?)null : extinctionTick,
            outcome = extinctionTick < 0 ? "population present at horizon" : "extinction observed",
            world.Births, world.Deaths, world.Mutations, initialFood, finalFood = TotalFood(world),
            survivingBlockedOver5Seconds=world.Creatures.Count(c=>c.Alive&&c.BlockedSeconds>5),
            survivingMeanDistance=world.Creatures.Where(c=>c.Alive).Select(c=>(double)c.DistanceTravelled).DefaultIfEmpty().Average(),
            remainingCorpses=world.Corpses.Count, fruitEnergy=world.Map.Trees.Sum(t=>t.Fruit.Sum(v=>(double)v)),
            maxGeneration = world.Creatures.Count > 0 ? world.Creatures.Max(c => c.Generation) : 0,
            breedingDiagnostics = world.InspectBreeding(),
            elapsedWallSecondsIncludingAudit = elapsed,
            ticksPerSecondIncludingAudit = elapsed > 0 ? measuredTicks / elapsed : 0,
            coreTickMs = new { median = Percentile(elapsedTicks, .5), p95 = Percentile(elapsedTicks, .95),
                maximum = elapsedTicks.Count > 0 ? elapsedTicks[^1] : 0 },
            managedMemoryDeltaBytes = GC.GetTotalMemory(false) - memoryBefore,
            finalHash = world.StateHash(), samples,
            notes = new[] { "Extinction is not an invariant failure or a claim about real biology.",
                "Core timing excludes per-tick audit work; total elapsed time includes it.",
                "This bounded calibration does not establish long-term ecosystem balance." }
        }, JsonOptions));
        return 0;
    }

    private static double Percentile(List<double> values, double p) =>
        values.Count == 0 ? 0 : values[(int)Math.Clamp(Math.Ceiling(p * values.Count) - 1, 0, values.Count - 1)];

    private static double TotalFood(World world) => world.Map.Food.Sum(x => (double)x)+world.Map.Trees.Sum(t=>t.Fruit.Sum(x=>(double)x));

    private static object RunEnvironment()
    {
        var corePath = typeof(World).Assembly.Location;
        return new
        {
            generatedAtUtc = DateTime.UtcNow,
            commandLine = Environment.CommandLine,
            framework = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            operatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            coreAssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(corePath))).ToLowerInvariant(),
            coreAssemblyLastWriteUtc = File.GetLastWriteTimeUtc(corePath)
        };
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class WorldAuditor
    {
        private readonly Dictionary<int, Snapshot> seen = new();
        private readonly int initialAlive;
        private long lastBirths;
        private long lastDeaths;
        private long lastMutations;
        public long MovementSegments { get; private set; }
        public long CheckedBirths { get; private set; }

        public WorldAuditor(World world)
        {
            initialAlive = world.Creatures.Count(c => c.Alive);
            foreach (var c in world.Creatures) seen.Add(c.Id, Snapshot.From(c));
            lastBirths = world.Births;
            lastDeaths = world.Deaths;
            lastMutations = world.Mutations;
            Inspect(world);
        }

        public void Inspect(World world)
        {
            var ids = new HashSet<int>();
            var parentsUsedThisTick = new HashSet<int>();
            var newborns = 0;
            var outsideParentIntervals = 0;
            foreach (var c in world.Creatures)
            {
                Require(ids.Add(c.Id), $"Tick {world.Tick}: duplicate creature ID {c.Id}.");
                Require(float.IsFinite(c.X) && float.IsFinite(c.Y) && float.IsFinite(c.Radius) && c.Radius > 0,
                    $"Tick {world.Tick}: creature {c.Id} has invalid geometry.");
                Require(float.IsFinite(c.Age) && c.Age >= 0 && float.IsFinite(c.Energy) && c.Energy >= -.001f &&
                    float.IsFinite(c.Hydration) && c.Hydration >= -.001f && c.Hydration <= 100.001f &&
                    float.IsFinite(c.Health) && c.Health >= -.001f && c.Health <= 100.001f &&
                    float.IsFinite(c.Tissue) && c.Tissue >= -.001f && float.IsFinite(c.AdultTissue) && c.AdultTissue > 0,
                    $"Tick {world.Tick}: creature {c.Id} has an invalid reserve, age or health.");
                Require(c.Genes.Length == 6 && c.Genes.All(g => float.IsFinite(g) && g >= 0 && g <= 1),
                    $"Tick {world.Tick}: creature {c.Id} has an invalid genome.");
                if(world.Tick%20==0)Require(c.BrainWeights.Length==Brain.Count&&c.BrainWeights.All(float.IsFinite)&&c.Motors.All(v=>float.IsFinite(v)&&v>=-1&&v<=1),"Invalid neural state.");
                if (c.Alive)
                {
                    Require(c.Health > 0, $"Tick {world.Tick}: creature {c.Id} is alive with zero health.");
                    Require(world.Map.CanOccupy(c.X, c.Y, c.Radius, c.Genes[3]),
                        $"Tick {world.Tick}: creature {c.Id} occupies blocked terrain at {c.X:F3},{c.Y:F3}, radius {c.Radius:F3}, water gene {c.Genes[3]:F3}.");
                    Require(c.Energy <= World.EnergyCapacity(c) + .01f,
                        $"Tick {world.Tick}: creature {c.Id} exceeds its energy capacity.");
                }
                if (seen.TryGetValue(c.Id, out var previous))
                {
                    Require(previous.Alive || !c.Alive, $"Tick {world.Tick}: dead creature {c.Id} resurrected.");
                    Require(previous.ParentA == c.ParentA && previous.ParentB == c.ParentB && previous.Generation == c.Generation &&
                        previous.Genes.SequenceEqual(c.Genes), $"Tick {world.Tick}: creature {c.Id} changed its birth identity/genome.");
                    if (previous.Alive && c.Alive)
                    {
                        var dx = c.X - previous.X;
                        var dy = c.Y - previous.Y;
                        var distance = MathF.Sqrt(dx * dx + dy * dy);
                        var samples = Math.Max(1, (int)MathF.Ceiling(distance / .15f));
                        // Independently sample the traversed body disc, not just the final point.
                        for (var j = 1; j <= samples; j++)
                        {
                            var t = (float)j / samples;
                            Require(world.Map.CanOccupy(previous.X + dx * t, previous.Y + dy * t,
                                Math.Min(previous.Radius, c.Radius), c.Genes[3]),
                                $"Tick {world.Tick}: creature {c.Id} crossed forbidden terrain between ({previous.X:F6},{previous.Y:F6}) and ({c.X:F6},{c.Y:F6}); radius {Math.Min(previous.Radius, c.Radius):F6}, aquatic {c.Genes[3]:F4}.");
                        }
                        MovementSegments++;
                    }
                }
                else
                {
                    newborns++;
                    CheckedBirths++;
                    bool sexual = c.ParentB != 0;
                    Require(c.ParentA > 0 && seen.ContainsKey(c.ParentA) &&
                        (!sexual || (c.ParentA != c.ParentB && seen.ContainsKey(c.ParentB))),
                        $"Tick {world.Tick}: newborn {c.Id} has missing or identical parents.");
                    var a = seen[c.ParentA];
                    var b = sexual ? seen[c.ParentB] : a;
                    Require(a.Alive && b.Alive, $"Tick {world.Tick}: newborn {c.Id} has a previously dead parent.");
                    Require(c.Generation == Math.Max(a.Generation, b.Generation) + 1,
                        $"Tick {world.Tick}: newborn {c.Id} generation is inconsistent.");
                    Require(parentsUsedThisTick.Add(c.ParentA) && (!sexual || parentsUsedThisTick.Add(c.ParentB)),
                        $"Tick {world.Tick}: one parent took part in multiple birth transactions.");
                    var parentA = world.Creatures.First(p => p.Id == c.ParentA);
                    var parentB = sexual ? world.Creatures.First(p => p.Id == c.ParentB) : parentA;
                    Require(parentA.Alive && parentB.Alive, $"Tick {world.Tick}: a dead parent completed a birth transaction.");
                    Require(world.Config.BreedingMode(parentA) == (sexual ? 1 : 2) && parentA.Group == c.Group && parentB.Group == c.Group,
                        $"Tick {world.Tick}: birth {c.Id} contradicts its parent's reproductive mode or group.");
                    var birthRecords = world.Events.Where(e => e.ChildId == c.Id && (e.Kind == "birth" || e.Kind == "mutation")).ToList();
                    Require(birthRecords.Count == 1, $"Tick {world.Tick}: birth {c.Id} does not have exactly one transaction record.");
                    var record = birthRecords[0];
                    Require(record.ParentA == c.ParentA && record.ParentB == c.ParentB,
                        $"Tick {world.Tick}: birth transaction parents differ from genealogy parents.");
                    var childBudget = c.Energy + c.Tissue;
                    var parentPayment = parentA.LastBirthCost + (sexual ? parentB.LastBirthCost : 0);
                    Require(Math.Abs(record.ChildEnergy - childBudget) < .001f &&
                        Math.Abs(record.EnergySpent - parentPayment) < .001f &&
                        Math.Abs(parentPayment * world.Config.BirthEfficiency - childBudget) < .001f,
                        $"Tick {world.Tick}: birth {c.Id} energy transfer is not fully funded by its parent payment(s).");
                    Require(parentA.LastBirthCost > 0 && (!sexual || parentB.LastBirthCost > 0),
                        $"Tick {world.Tick}: birth {c.Id} has an unpaid parent contribution.");
                    var outside = false;
                    for (var i = 0; i < c.Genes.Length; i++)
                        if (c.Genes[i] < Math.Min(a.Genes[i], b.Genes[i]) || c.Genes[i] > Math.Max(a.Genes[i], b.Genes[i])) outside = true;
                    for(int w=0;w<Brain.Count;w++)if(c.BrainWeights[w]<Math.Min(parentA.BrainWeights[w],parentB.BrainWeights[w])||c.BrainWeights[w]>Math.Max(parentA.BrainWeights[w],parentB.BrainWeights[w]))outside=true;
                    if (outside) outsideParentIntervals++;
                }
            }
            Require(world.Births >= lastBirths && world.Deaths >= lastDeaths && world.Mutations >= lastMutations,
                $"Tick {world.Tick}: cumulative lifecycle counters decreased.");
            Require(newborns == world.Births - lastBirths,
                $"Tick {world.Tick}: birth counter does not match new genealogy records.");
            Require(outsideParentIntervals <= world.Mutations - lastMutations,
                $"Tick {world.Tick}: a child escaped the parent gene interval without a mutation record.");
            Require(world.Creatures.Count(c => c.Alive) == initialAlive + world.Births - world.Deaths,
                $"Tick {world.Tick}: living population does not balance births and deaths.");
            Require(world.Map.Food.All(x => float.IsFinite(x) && x >= -.001f),
                $"Tick {world.Tick}: food stock is negative or non-finite.");
            foreach (var c in world.Creatures)
                Require(c.LastBirthCost <= 0 || parentsUsedThisTick.Contains(c.Id),
                    $"Tick {world.Tick}: creature {c.Id} paid for a birth without a committed child.");
            foreach(var tree in world.Map.Trees)Require(tree.Fruit.All(f=>float.IsFinite(f)&&f>=0&&f<=world.Config.FruitEnergy)&&tree.Regrow.All(r=>float.IsFinite(r)&&r>=0),"Invalid fruit stock or timer.");
            Require(world.Corpses.All(c=>(world.Tick-c.CreatedTick)*World.Dt<world.Config.CorpseLifetime),"Expired corpse remained in world.");
            foreach (var c in world.Creatures) seen[c.Id] = Snapshot.From(c);
            lastBirths = world.Births;
            lastDeaths = world.Deaths;
            lastMutations = world.Mutations;
        }
    }

    private sealed record Snapshot(float X, float Y, float Radius, bool Alive, int ParentA, int ParentB, int Generation, float[] Genes)
    {
        public static Snapshot From(Creature c) => new(c.X, c.Y, c.Radius, c.Alive, c.ParentA, c.ParentB, c.Generation, c.Genes.ToArray());
    }

    private sealed record Options(bool SelfTest, int Seed, int Ticks, int DroughtAt, bool Help,
        int? MapSize, int? Herbivores, int? Carnivores, int? Trees)
    {
        public WorldConfig CreateWorldConfig()
        {
            var config = new WorldConfig();
            if (MapSize.HasValue) config.MapSize = MapSize.Value;
            if (Herbivores.HasValue) config.Herbivores = Herbivores.Value;
            if (Carnivores.HasValue) config.Carnivores = Carnivores.Value;
            if (Trees.HasValue) config.TreeCount = Trees.Value;
            config.Validate();
            return config;
        }

        public static Options Parse(string[] args)
        {
            var selfTest = args.Length == 0;
            var seed = 42;
            var ticks = 12000;
            var droughtAt = -1;
            var help = false;
            int? mapSize = null, herbivores = null, carnivores = null, trees = null;
            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                string Value()
                {
                    if (i + 1 >= args.Length) throw new ArgumentException($"Missing value after {arg}.");
                    return args[++i];
                }
                switch (arg)
                {
                    case "--selftest": selfTest = true; break;
                    case "--seed": seed = int.Parse(Value()); break;
                    case "--ticks": ticks = int.Parse(Value()); break;
                    case "--map-size": mapSize = int.Parse(Value()); break;
                    case "--herbivores": herbivores = int.Parse(Value()); break;
                    case "--carnivores": carnivores = int.Parse(Value()); break;
                    case "--trees": trees = int.Parse(Value()); break;
                    case "--drought-at": droughtAt = int.Parse(Value()); break;
                    case "--drought": droughtAt = 0; break;
                    case "--help": case "-h": help = true; break;
                    default: throw new ArgumentException($"Unknown option {arg}. Use --help for usage.");
                }
            }
            if (ticks < 1 || ticks > 2_000_000) throw new ArgumentOutOfRangeException(nameof(ticks), "Ticks must be between 1 and 2,000,000.");
            if (droughtAt < -1) throw new ArgumentOutOfRangeException(nameof(droughtAt), "Drought tick must be nonnegative.");
            if (selfTest && (mapSize.HasValue || herbivores.HasValue || carnivores.HasValue || trees.HasValue))
                throw new ArgumentException("Scale options apply to benchmarks; omit --selftest when selecting a custom world.");
            return new Options(selfTest, seed, ticks, droughtAt, help, mapSize, herbivores, carnivores, trees);
        }
    }
}
