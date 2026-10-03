using System.Collections;
using System.Numerics;
using System.Reflection;
using EvoSim.Core;

internal static partial class Program
{
    private static object CheckScaleOptions()
    {
        var defaults = new WorldConfig();
        Require(defaults.GeneratorVersion == 2, "New worlds do not default to the corrected terrain generator.");
        var legacy = Options.Parse(new[] { "--ticks", "1", "--seed", "42" }).CreateWorldConfig();
        Require(legacy.MapSize == defaults.MapSize && legacy.Herbivores == defaults.Herbivores &&
            legacy.Carnivores == defaults.Carnivores && legacy.TreeCount == defaults.TreeCount,
            "Existing benchmark options no longer preserve the default world.");
        var custom = Options.Parse(new[] { "--ticks", "1", "--map-size", "256", "--herbivores", "3",
            "--carnivores", "2", "--trees", "4" }).CreateWorldConfig();
        Require(custom.MapSize == 256 && custom.Herbivores == 3 && custom.Carnivores == 2 && custom.TreeCount == 4,
            "Scale options did not preserve literal requested counts.");
        var onlySize = Options.Parse(new[] { "--map-size", "192" }).CreateWorldConfig();
        Require(onlySize.MapSize == 192 && onlySize.Herbivores == defaults.Herbivores &&
            onlySize.Carnivores == defaults.Carnivores && onlySize.TreeCount == defaults.TreeCount,
            "Changing map size silently changed population or resource counts.");
        foreach (var args in new[]
        {
            new[] { "--map-size", "129" }, new[] { "--herbivores", "-1" },
            new[] { "--carnivores", "-1" }, new[] { "--trees", "-1" }
        })
        {
            bool rejected = false;
            try { _ = Options.Parse(args).CreateWorldConfig(); } catch (ArgumentException) { rejected = true; }
            Require(rejected, $"Invalid benchmark scale options were accepted: {string.Join(' ', args)}.");
        }
        foreach (var size in new[] { 128, 192, 256 })
            new WorldConfig { MapSize = size, GeneratorVersion = 2 }.Validate();
        new WorldConfig { MapSize = 128, GeneratorVersion = 1 }.Validate();
        foreach (var invalid in new[]
        {
            new WorldConfig { GeneratorVersion = 3 },
            new WorldConfig { MapSize = 192, GeneratorVersion = 1 },
            new WorldConfig { MapSize = 256, GeneratorVersion = 1 }
        })
        {
            bool rejected = false;
            try { invalid.Validate(); } catch (ArgumentException) { rejected = true; }
            Require(rejected, $"Unsupported map/generator combination accepted: {invalid.MapSize}/{invalid.GeneratorVersion}.");
        }
        return new { defaultsPreserved = true, literalCounts = true, supportedSizes = new[] { 128, 192, 256 },
            defaultGenerator = defaults.GeneratorVersion, legacyGeneratorRestrictedTo128 = true, unknownGeneratorRejected = true };
    }

    private static object CheckLargeTerrain()
    {
        var checkedMaps = 0;
        foreach (var size in new[] { 192, 256 })
        foreach (var seed in new[] { 7, 135 })
        foreach (var regions in new[] { 1, 2, 3, 4, 5 })
        {
            var cfg = new WorldConfig { MapSize = size, LandRegions = regions, Herbivores = 6, Carnivores = 2,
                TreeCount = 10, RockClusters = 2, MeadowCount = 3 };
            var world = new World(seed, cfg);
            Require(world.Map.Size == size && world.Map.CellCount == size * size &&
                world.Map.Food.Length == size * size && world.Map.Moisture.Length == size * size && world.Map.Tiles.Length == size * size,
                $"Seed {seed}, size {size}: environmental arrays do not use the instance dimensions.");
            Require(world.Map.LandRegionCount() == regions,
                $"Seed {seed}, size {size}: expected {regions} land regions, got {world.Map.LandRegionCount()}.");
            Require(world.Creatures.All(c => world.Map.CanOccupy(c.X, c.Y, c.Radius, c.Genes[3])),
                $"Seed {seed}, size {size}: a founder occupies invalid terrain.");
            checkedMaps++;
        }
        // These two 128-cell configurations produced 9 and 4 components in
        // generator 1. New worlds must honor the requested three regions.
        var narrowAndWideRegressions = new[] { (Seed: 7, Width: 2f), (Seed: 135, Width: .4f) };
        foreach (var regression in narrowAndWideRegressions)
        {
            var map = new Terrain(regression.Seed, new WorldConfig { MapSize = 128,
                LandRegions = 3, RiverWidth = regression.Width, TreeCount = 0, RockClusters = 0 });
            Require(map.LandRegionCount() == 3,
                $"New 128 generator: seed {regression.Seed}, width {regression.Width} returned {map.LandRegionCount()} regions instead of 3.");
        }
        var small = new Terrain(42, new WorldConfig { MapSize = 128, TreeCount = 0, RockClusters = 0 });
        foreach (var size in new[] { 192, 256 })
        {
            var map = new Terrain(42, new WorldConfig { MapSize = size, TreeCount = 0, RockClusters = 0 });
            Require(small.Size == 128 && small.CellCount == 16384 && small.Cell(140, 140) == -1,
                "Constructing a large map changed an existing smaller map's dimensions.");
            Require(map.Cell(size - .5f, size - .5f) == map.CellCount - 1 && map.Cell(size, size - 1) == -1 &&
                map.Cell(128.25f, 128.25f) >= 0, "Southeast map indexing still assumes the 128-cell boundary.");
            // Only direct geometry queries follow edits to this isolated corner;
            // no topology-dependent cached pathfinding is requested here.
            for (var y = size - 8; y < size; y++)
            for (var x = size - 8; x < size; x++) map.Tiles[y * size + x] = 0;
            const float radius = .35f;
            Require(map.CanOccupy(size - .5f, size - .5f, radius, 0) &&
                !map.CanOccupy(size - .2f, size - .2f, radius, 0), "Southeast body-boundary clearance is incorrect.");
            var cell = (size - 3) * size + size - 3;
            var from = new Vector2(size - 4.5f, size - 2.5f);
            var to = new Vector2(size - 1.5f, size - 2.5f);
            map.Tiles[cell] = 2;
            Require(!map.HasClearPath(from, to, radius, 1), "A body tunnels through a rock beyond coordinate 127.");
            map.Tiles[cell] = 1;
            Require(!map.HasClearPath(from, to, radius, 0) && map.HasClearPath(from, to, radius, 1),
                "Large-map water crossing lost the distinction between land and aquatic bodies.");
        }
        return new { checkedMaps, sizes = new[] { 192, 256 }, regionCounts = new[] { 1, 2, 3, 4, 5 }, southeastGeometry = true,
            corrected128Regressions = narrowAndWideRegressions.Select(r => new { seed = r.Seed, riverWidth = r.Width, requestedRegions = 3 }) };
    }

    private static object CheckLargeResourceIdentity()
    {
        var cfg = new WorldConfig { MapSize = 256, Herbivores = 2, Carnivores = 1,
            HerbBreedingMode = 0, CarnBreedingMode = 0, TreeCount = 0, RockClusters = 0, MeadowCount = 0,
            BasalCost = 0, MassCost = 0, PerceptionCost = 0, AquaticCost = 0, MoveCost = 0,
            WaterDrain = 0, MoveWater = 0, CorpseDecay = 0 };
        var world = new World(42, cfg);
        Array.Clear(world.Map.Food);
        world.Corpses.Clear();
        // This exact grass index was previously the numeric base for corpse #0.
        var grassPosition = new Vector2(.5f, 64.5f);
        var grassCell = world.Map.Cell(grassPosition.X, grassPosition.Y);
        Require(grassCell == 16384, "The resource collision fixture must address grass above the former map limit.");
        world.Map.Food[grassCell] = 10;
        var corpsePosition = new Vector2(180.5f, 190.5f);
        world.Corpses.Add(new Corpse { X = corpsePosition.X, Y = corpsePosition.Y, Food = 20, CreatedTick = 0, CreatureId = 999 });
        var tree = new Tree(210, 205, 1.35f, cfg.FruitSlots, cfg.FruitEnergy);
        world.Map.Trees.Add(tree);
        var positions = new[] { grassPosition, tree.FruitPosition(0), corpsePosition };
        for (var i = 0; i < world.Creatures.Count; i++)
        {
            var c = world.Creatures[i];
            c.X = positions[i].X; c.Y = positions[i].Y; c.Age = 200; c.Energy = 10; c.Hydration = 100; c.Health = 100;
            c.Genes[2] = i == 2 ? 1 : 0;
            ConstantBrain(c, interact: 1, mate: -1);
            Require(world.Map.CanOccupy(c.X, c.Y, c.Radius, c.Genes[3]), "A resource fixture body is not legally placed.");
        }
        AuditedSteps(world, 1);
        var grassTaken = cfg.GrassBiteRate * World.Dt;
        var richFoodTaken = cfg.FruitBiteRate * World.Dt;
        Require(Math.Abs(world.Map.Food[grassCell] - (10 - grassTaken)) < .0001f &&
            Math.Abs(world.Corpses[0].Food - (20 - richFoodTaken)) < .0001f &&
            Math.Abs(tree.Fruit[0] - (cfg.FruitEnergy - richFoodTaken)) < .0001f,
            "Simultaneous grass/corpse/fruit requests debited an aliased or incorrect source.");
        Require(Math.Abs(world.Creatures[0].Energy - (10 + grassTaken)) < .0001f &&
            Math.Abs(world.Creatures[1].Energy - (10 + richFoodTaken)) < .0001f &&
            Math.Abs(world.Creatures[2].Energy - (10 + richFoodTaken)) < .0001f,
            "Independent food categories did not deliver their actual debited energy.");
        Require(tree.Fruit.Skip(1).All(v => v == cfg.FruitEnergy) && world.Map.Food.Where((_, i) => i != grassCell).All(v => v == 0),
            "A food debit changed an unrelated fruit slot or grass cell.");
        return new { grassCell, mapSize = world.Map.Size, simultaneousSources = 3, grassTaken, corpseTaken = richFoodTaken, fruitTaken = richFoodTaken };
    }

    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    private static object? InvokePrivate(World world, string name, params object[] args)
    {
        var method = typeof(World).GetMethod(name, PrivateInstance) ?? throw new InvalidOperationException($"Missing test-observed method {name}.");
        return method.Invoke(world, args);
    }

    private static T NodeField<T>(object node, string name)
    {
        var field = node.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Missing fruit index field {name}.");
        return (T)field.GetValue(node)!;
    }

    private static object CheckLargeSpatialQueries()
    {
        var livingQueries = 0;
        var fruitQueries = 0;
        foreach (var size in new[] { 192, 256 })
        {
            var world = new World(82, new WorldConfig { MapSize = size, Herbivores = 18, Carnivores = 0, TreeCount = 0, RockClusters = 0 });
            var points = new[]
            {
                new Vector2(127.99f,136), new Vector2(128.01f,136), new Vector2(135.99f,136), new Vector2(136.01f,136),
                new Vector2(size-8.01f,size-8), new Vector2(size-7.99f,size-8), new Vector2(size-.5f,size-.5f),
                new Vector2(8.01f,8), new Vector2(7.99f,8), new Vector2(64,129), new Vector2(64,128.9f),
                new Vector2(.5f,.5f), new Vector2(size/2f,size/2f), new Vector2(128,136), new Vector2(149,149),
                new Vector2(64,64), new Vector2(128.2f,136.1f), new Vector2(128.2f,136.1f)
            };
            for (var i = 0; i < points.Length; i++) { world.Creatures[i].X = points[i].X; world.Creatures[i].Y = points[i].Y; }
            world.Creatures[16].Alive = false;
            _ = InvokePrivate(world, "RebuildIndex");
            var neighborsField = typeof(World).GetField("_neighbors", PrivateInstance)!;
            foreach (var index in new[] { 0, 2, 4, 6, 7, 9, 11 })
            foreach (var radius in new[] { .03f, .3f, 1.5f, 8f, 18f, 64f, 300f })
            {
                var origin = world.Creatures[index];
                _ = InvokePrivate(world, "Neighbors", origin, radius);
                var actual = ((IEnumerable<Creature>)neighborsField.GetValue(world)!).Select(c => c.Id).ToArray();
                var expected = world.Creatures.Where(c => c.Alive && c != origin &&
                    (c.X-origin.X)*(c.X-origin.X)+(c.Y-origin.Y)*(c.Y-origin.Y) <= radius*radius).Select(c => c.Id).Order().ToArray();
                Require(actual.SequenceEqual(expected), $"Size {size}, neighbor {index}, radius {radius}: index differs from brute force.");
                livingQueries++;
            }

            var centers = new[] { new Vector2(127.7f,135.9f), new Vector2(128.1f,136.1f),
                new Vector2(size-3,size-3), new Vector2(7.8f,8.1f), new Vector2(151,145) };
            foreach (var center in centers) world.Map.Trees.Add(new Tree(center.X, center.Y, 1.35f));
            world.Map.Trees[0].Fruit[0] = 0; // Availability changes must not corrupt geometric candidate identities.
            _ = InvokePrivate(world, "EnsureResourceIndex");
            var beforeQueries = world.StateHash();
            var queryPoints = centers.Concat(new[] { world.Map.Trees[0].FruitPosition(0), new Vector2(size-.5f,size-.5f) }).ToArray();
            foreach (var point in queryPoints)
            foreach (var radius in new[] { 0f, .5f, 1.5f, 8f, 20f, 300f })
            {
                var actual = new List<(int Tree, int Slot)>();
                foreach (var node in (IEnumerable)InvokePrivate(world, "QueryFruit", point.X, point.Y, radius)!)
                {
                    int treeIndex = NodeField<int>(node!, "TreeIndex"), slot = NodeField<int>(node!, "Slot");
                    var expectedPosition = world.Map.Trees[treeIndex].FruitPosition(slot);
                    Require(NodeField<float>(node!, "X") == expectedPosition.X && NodeField<float>(node!, "Y") == expectedPosition.Y &&
                        ReferenceEquals(NodeField<Tree>(node!, "Tree"), world.Map.Trees[treeIndex]), "Fruit node identity or cached coordinate is inconsistent.");
                    actual.Add((treeIndex, slot));
                }
                var expected = new List<(int Tree, int Slot)>();
                for (var t = 0; t < world.Map.Trees.Count; t++)
                for (var slot = 0; slot < world.Map.Trees[t].Fruit.Length; slot++)
                {
                    var p = world.Map.Trees[t].FruitPosition(slot);
                    float dx = p.X-point.X, dy = p.Y-point.Y;
                    if (dx*dx+dy*dy <= radius*radius) expected.Add((t, slot));
                }
                Require(actual.SequenceEqual(expected), $"Size {size}, fruit query at {point}, radius {radius}: index/order differs from brute force.");
                fruitQueries++;
            }
            Require(beforeQueries == world.StateHash(), "Candidate fruit queries changed physical state or RNG.");
        }
        return new { sizes = new[] { 192, 256 }, livingQueries, fruitQueries, originalCandidateOrderPreserved = true };
    }

    private static object CheckLargeWorldIndependence()
    {
        var sizes = new[] { 128, 192, 256 };
        World Create(int size) => new World(91, new WorldConfig { MapSize = size, Herbivores = 12, Carnivores = 3, TreeCount = 12 });
        var referenceHashes = new Dictionary<int, string>();
        foreach (var size in sizes)
        {
            var reference = Create(size);
            for (var tick = 0; tick < 160; tick++) reference.Step();
            referenceHashes.Add(size, reference.StateHash());
        }
        var interleaved = sizes.Select(Create).ToArray();
        for (var tick = 0; tick < 160; tick++)
        {
            interleaved[0].Step(); interleaved[2].Step(); interleaved[1].Step();
        }
        foreach (var world in interleaved)
            Require(world.StateHash() == referenceHashes[world.Map.Size], $"Interleaving other map sizes changed size {world.Map.Size}'s trajectory.");
        var checkpoints = new List<object>();
        foreach (var size in new[] { 192, 256 })
        {
            var original = interleaved.Single(w => w.Map.Size == size);
            original.SetDrought(true);
            using var bytes = new MemoryStream(); original.Save(bytes); bytes.Position = 0;
            var restored = World.Load(bytes);
            Require(restored.Map.Size == size && restored.Map.CellCount == size*size && restored.StateHash() == original.StateHash(),
                $"Large checkpoint lost size {size}'s map data or initial state.");
            for (var tick = 0; tick < 200; tick++) { original.Step(); restored.Step(); }
            Require(restored.StateHash() == original.StateHash(), $"Large checkpoint future diverged at size {size}.");
            checkpoints.Add(new { size, savedTick = 160, continuedTicks = 200, checkpointBytes = bytes.Length });
        }
        return new { interleavedSizes = sizes, interleavedTicks = 160, checkpoints };
    }
}
