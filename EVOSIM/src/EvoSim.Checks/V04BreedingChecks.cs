using System.Numerics;
using System.Text.Json;
using EvoSim.Core;

internal static partial class Program
{
    private static World BreedingFixture(int population, int mode, float mateSignal = 1, float drive = .5f)
    {
        var cfg = new WorldConfig
        {
            Herbivores = population, Carnivores = 0, HerbBreedingMode = mode, HerbReproductiveDrive = drive,
            TreeCount = 0, RockClusters = 0, MeadowCount = 0, MutationRate = 0,
            BasalCost = 0, MassCost = 0, PerceptionCost = 0, AquaticCost = 0, MoveCost = 0,
            WaterDrain = 0, MoveWater = 0, InitialCooldownMin = 0, InitialCooldownMax = 0, InitialEnergy = 1
        };
        var world = new World(442, cfg);
        var p = world.Map.FindSpawn(new PcgRandom(923, 1), 3, 0);
        for (var i = 0; i < world.Creatures.Count; i++)
        {
            var c = world.Creatures[i];
            c.X = p.X + i * 1.1f; c.Y = p.Y;
            c.Age = 200; c.Energy = c.MaxEnergy; c.Hydration = 100; c.Health = 100;
            c.Tissue = c.AdultTissue; c.ReproductionCooldown = 0;
            ConstantBrain(c, mate: mateSignal);
        }
        return world;
    }

    private static void AuditedSteps(World world, int count)
    {
        var audit = new WorldAuditor(world);
        for (var i = 0; i < count; i++) { world.Step(); audit.Inspect(world); }
    }

    private static object CheckBreedingModes()
    {
        var disabled = BreedingFixture(2, 0);
        AuditedSteps(disabled, 8);
        Require(disabled.Births == 0 && disabled.Creatures.All(c => c.LastBirthCost == 0),
            "Disabled reproduction accepted a positive neural mate signal or charged a birth.");

        var noConsent = BreedingFixture(2, 1);
        ConstantBrain(noConsent.Creatures[1], mate: -1);
        AuditedSteps(noConsent, 8);
        Require(noConsent.Births == 0, "Sexual reproduction ignored one parent's nonpositive mate signal.");
        var noPartner = BreedingFixture(1, 1);
        AuditedSteps(noPartner, 8);
        Require(noPartner.Births == 0, "Sexual reproduction succeeded without a second parent.");
        var noAsexualSignal = BreedingFixture(1, 2, -1);
        AuditedSteps(noAsexualSignal, 8);
        Require(noAsexualSignal.Births == 0, "Asexual mode overrode the neural mate output.");

        var asexual = BreedingFixture(1, 2);
        var parent = asexual.Creatures[0];
        var beforeEnergy = parent.Energy;
        AuditedSteps(asexual, 8);
        Require(asexual.Births == 1, "Eligible, positively signalling asexual parent failed to reproduce.");
        var child = asexual.Creatures.Single(c => c.ParentA == parent.Id);
        var birth = asexual.Events.Single(e => e.ChildId == child.Id);
        Require(child.ParentB == 0 && child.Generation == parent.Generation + 1,
            "Asexual birth fabricated a second parent or lost its generation.");
        Require(Math.Abs(beforeEnergy - parent.Energy - birth.EnergySpent) < .001f &&
            Math.Abs(birth.EnergySpent * asexual.Config.BirthEfficiency - birth.ChildEnergy) < .001f,
            "The single parent did not pay the entire asexual birth budget.");
        Require(child.Genes.SequenceEqual(parent.Genes) && child.BrainWeights.SequenceEqual(parent.BrainWeights) &&
            !ReferenceEquals(child.Genes, parent.Genes) && !ReferenceEquals(child.BrainWeights, parent.BrainWeights),
            "Mutation-free asexual inheritance must independently clone body and network arrays.");
        Require(asexual.GetDescendants(parent.Id, 1).Select(c => c.Id).SequenceEqual(new[] { child.Id }),
            "The single-parent child was not counted correctly in genealogy.");

        var sexual = BreedingFixture(2, 1);
        var sexualEnergy = sexual.Creatures.Sum(c => c.Energy);
        AuditedSteps(sexual, 8);
        Require(sexual.Births == 1, "A compatible sexual pair did not produce one funded child.");
        var sexualChild = sexual.Creatures.Single(c => c.ParentA != 0);
        var sexualBirth = sexual.Events.Single(e => e.ChildId == sexualChild.Id);
        Require(sexualChild.ParentB > 0 && sexualChild.ParentA != sexualChild.ParentB,
            "Sexual mode lost its two distinct parents.");
        Require(Math.Abs(sexualEnergy - sexual.Creatures.Where(c => c.ParentA == 0).Sum(c => c.Energy) - sexualBirth.EnergySpent) < .001f,
            "Sexual parents did not fund the complete child budget.");

        var unaffordable = BreedingFixture(1, 2);
        unaffordable.Config.BirthEfficiency = .1f;
        unaffordable.Config.ChildEnergy = .5f;
        var reserve = unaffordable.Creatures[0].Energy;
        AuditedSteps(unaffordable, 8);
        Require(unaffordable.Births == 0 && unaffordable.Creatures.Count == 1 &&
            Math.Abs(unaffordable.Creatures[0].Energy - reserve) < .001f,
            "An unaffordable asexual birth created a child or charged an incomplete transaction.");

        var mutant = BreedingFixture(1, 2);
        mutant.Config.MutationRate = 1;
        AuditedSteps(mutant, 8);
        var mutantParent = mutant.Creatures[0];
        var mutantChild = mutant.Creatures.Single(c => c.ParentA != 0);
        int differences = mutantChild.Genes.Zip(mutantParent.Genes).Count(v => v.First != v.Second) +
            mutantChild.BrainWeights.Zip(mutantParent.BrainWeights).Count(v => v.First != v.Second);
        Require(mutant.Births == 1 && mutant.Mutations == 1 && differences == 1,
            "Asexual birth did not preserve the single-event body/network mutation budget.");

        var genealogy = BreedingFixture(1, 2);
        genealogy.Creatures.Clear();
        genealogy.Creatures.AddRange(new[]
        {
            new Creature { Id = 1, Generation = 0 },
            new Creature { Id = 2, ParentA = 1, ParentB = 0, Generation = 1 },
            new Creature { Id = 3, ParentA = 2, ParentB = 0, Generation = 2 },
            new Creature { Id = 4, ParentA = 1, ParentB = 0, Generation = 1, Alive = false }
        });
        Require(genealogy.GetDescendants(1).Select(c => c.Id).Order().SequenceEqual(new[] { 2, 3, 4 }) &&
            genealogy.GetDescendants(1, 2).Select(c => c.Id).SequenceEqual(new[] { 3 }) &&
            genealogy.GetDescendants(1, 0, true).Select(c => c.Id).Order().SequenceEqual(new[] { 2, 3 }),
            "Single-parent genealogy duplicated, omitted, or misclassified descendants.");
        return new { disabled = true, bothSexualSignalsRequired = true, asexualSignalRequired = true,
            fundedAsexualBirths = asexual.Births, fundedSexualBirths = sexual.Births,
            asexualPayment = birth.EnergySpent, asexualMutationDifferences = differences, singleParentGenealogy = true };
    }

    private static object CheckReproductiveDrive()
    {
        float InputAt(float drive, int mode = 1)
        {
            var world = BreedingFixture(1, mode, -1, drive);
            AuditedSteps(world, 4);
            Require(world.Births == 0, "Reproductive drive forced a birth through a nonpositive motor output.");
            return world.Creatures[0].Senses[5];
        }
        var zero = InputAt(0);
        var normal = InputAt(.5f);
        var high = InputAt(1);
        var disabled = InputAt(1, 0);
        Require(zero == 0 && normal == 1 && high == 2 && disabled == 0,
            "Reproductive drive does not scale the existing input or disabled mode leaks a drive signal.");
        var cooldown = BreedingFixture(1, 1, -1, 1);
        cooldown.Creatures[0].ReproductionCooldown = 60;
        AuditedSteps(cooldown, 4);
        Require(cooldown.Creatures[0].Senses[5] == 0, "A cooling-down parent still receives the ready drive signal.");
        // Zero drive removes the input, but does not replace the explicit 'off' mode.
        var independentPositiveMotor = BreedingFixture(1, 2, 1, 0);
        AuditedSteps(independentPositiveMotor, 8);
        Require(independentPositiveMotor.Births == 1,
            "Zero drive became a hidden ban even though the network independently signalled reproduction.");
        return new { zero, defaultDrive = normal, high, disabled, zeroDriveIsNotDisabled = true,
            populationImprovementNotAsserted = true };
    }

    private static object CheckBreedingDiagnostics()
    {
        var cfg = new WorldConfig { Herbivores = 9, Carnivores = 1, HerbBreedingMode = 1, CarnBreedingMode = 0,
            TreeCount = 0, RockClusters = 0, MeadowCount = 0 };
        var world = new World(812, cfg);
        foreach (var c in world.Creatures)
        {
            c.Age = 200; c.Maturity = 1; c.Tissue = c.AdultTissue; c.Energy = c.MaxEnergy;
            c.Hydration = 100; c.Health = 100; c.ReproductionCooldown = 0; c.Motors[4] = 1;
        }
        var point = world.Map.FindSpawn(new PcgRandom(91), 3, 0);
        world.Creatures[0].X = point.X; world.Creatures[0].Y = point.Y;
        world.Creatures[1].X = point.X + 1.4f; world.Creatures[1].Y = point.Y;
        var farRng = new PcgRandom(97);
        Vector2 far;
        do { far = world.Map.FindSpawn(farRng, .6f, 0); } while (Vector2.Distance(point, far) < 5);
        world.Creatures[8].X = far.X; world.Creatures[8].Y = far.Y;
        var c2 = world.Creatures[2]; c2.Age = 1; c2.ReproductionCooldown = 20;
        c2.Energy = c2.Hydration = c2.Health = c2.Motors[4] = 0;
        world.Creatures[3].ReproductionCooldown = 20; world.Creatures[3].Energy = 0;
        world.Creatures[4].Energy = 0; world.Creatures[5].Hydration = 0;
        world.Creatures[6].Health = 0; world.Creatures[7].Motors[4] = 0;
        var before = world.StateHash();
        var senses = world.Creatures.Select(c => c.Senses.ToArray()).ToArray();
        var report = world.InspectBreeding();
        Require(report.Living == 10 && report.Disabled == 1 && report.Immature == 1 && report.Cooldown == 2 &&
            report.LowEnergy == 3 && report.LowWater == 2 && report.LowHealth == 2 && report.NoPositiveMateSignal == 2,
            "Overlapping diagnostic conditions lost compound deficits or disabled individuals.");
        Require(report.ReadyWithSignal == 3 && report.ReadyWithoutSignal == 1 &&
            report.SexualReadyWithSignal == 3 && report.NearbyCompatibleReadyPairs == 1 && report.SexualReadyWithNearbyPartner == 2,
            "Readiness or unique compatible-contact counts are incorrect.");
        var gates = report.ExclusiveFirstBlocker;
        Require(gates.Disabled == 1 && gates.Immature == 1 && gates.Cooldown == 1 && gates.LowEnergy == 1 &&
            gates.LowWater == 1 && gates.LowHealth == 1 && gates.OtherIneligible == 0 && gates.NoPositiveMateSignal == 1 && gates.ReadyWithSignal == 3,
            "Exclusive first-blocker gates do not partition the diagnostic fixture.");
        Require(before == world.StateHash() && senses.Select((s, i) => s.SequenceEqual(world.Creatures[i].Senses)).All(x => x),
            "InspectBreeding changed state, RNG, or sensory inputs.");
        Require(JsonSerializer.Serialize(report) == JsonSerializer.Serialize(world.InspectBreeding()),
            "Diagnostics changed on repeated reads of the same world.");
        world.Config.BreedContactRange = 0;
        Require(world.InspectBreeding().NearbyCompatibleReadyPairs == 0, "Contact diagnostics ignored the configured range.");
        world.Config.HerbBreedingMode = 2;
        var asexual = world.InspectBreeding();
        Require(asexual.AsexualReadyWithSignal == 3 && asexual.SexualReadyWithSignal == 0 &&
            asexual.NearbyCompatibleReadyPairs == 0, "Asexual readiness was misreported as sexual mate contacts.");

        // Two identical worlds, but one has repeated diagnostic reads between steps.
        // Equality after further simulation exercises RNG and future-state neutrality.
        var observed = new World(42, new WorldConfig { Herbivores = 30, Carnivores = 4 });
        var control = new World(42, new WorldConfig { Herbivores = 30, Carnivores = 4 });
        for (var tick = 0; tick < 400; tick++)
        {
            _ = observed.InspectBreeding();
            if (tick % 7 == 0) _ = observed.InspectBreeding();
            observed.Step(); control.Step();
        }
        Require(observed.StateHash() == control.StateHash(), "Diagnostic reads changed the future simulation/RNG trajectory.");
        return new { overlappingConditions = true, exclusivePartition = true, uniqueContactPairs = report.NearbyCompatibleReadyPairs,
            configuredContactRange = true, asexualNeedsNoPartner = true, stateAndSensesUnchanged = true, futureTicksMatched = 400 };
    }

    private static object CheckTreeProfiles()
    {
        var defaults = new WorldConfig();
        Require(defaults.TreeRadiusMin == 1.35f && defaults.TreeRadiusMax == 2.45f && defaults.TreeSpacing == 3 &&
            defaults.TreeFruitRadius == .8f && defaults.FruitDroughtTolerance == .2f,
            "Default tree profile changed the established model values.");
        var cfg = new WorldConfig { TreeCount = 30, TreeRadiusMin = 3, TreeRadiusMax = 3,
            TreeSpacing = 7, TreeFruitRadius = .35f, FruitSlots = 9, FruitEnergy = 22 };
        var map = new Terrain(73, cfg);
        Require(map.Trees.Count == 30 && map.Trees.All(t => t.Radius == 3 && t.Fruit.Length == 9 && t.Fruit.All(f => f == 22)),
            "Custom tree geometry or resource profile was not applied.");
        foreach (var tree in map.Trees)
        {
            Require(Math.Abs(Vector2.Distance(new Vector2(tree.X, tree.Y), tree.FruitPosition(0)) - 1.05f) < .0001f,
                "Tree fruit placement did not use the configured canopy fraction.");
            foreach (var other in map.Trees)
                if (!ReferenceEquals(tree, other)) Require(Vector2.Distance(new(tree.X, tree.Y), new(other.X, other.Y)) >= 7 - .0001f,
                    "Tree spacing does not constrain physical placement.");
        }
        var normalTolerance = new Terrain(42, defaults);
        var resistant = new Terrain(42, new WorldConfig { FruitDroughtTolerance = 1 });
        normalTolerance.WaterLevel = resistant.WaterLevel = .12f;
        normalTolerance.Trees[0].Fruit[0] = resistant.Trees[0].Fruit[0] = 0;
        normalTolerance.Trees[0].Regrow[0] = resistant.Trees[0].Regrow[0] = 45;
        for (var i = 0; i < 45; i++) { normalTolerance.EcologyStep(true); resistant.EcologyStep(true); }
        Require(normalTolerance.Trees[0].Fruit[0] == 0 && resistant.Trees[0].Fruit[0] == 18,
            "Fruit drought tolerance did not change actual regrowth timing.");
        return new { customTrees = map.Trees.Count, fruitDistance = 1.05, minimumTreeSpacing = 7,
            droughtToleranceChangesRegrowth = true, defaultsPreserved = true };
    }
}
