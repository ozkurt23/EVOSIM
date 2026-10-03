using System.Collections.ObjectModel;
using EvoSim.Core;

namespace EvoSim.Observer;

internal sealed record GeneDistribution(int Count, float? Minimum, float? Median, float? Maximum,
    ReadOnlyCollection<int> Bins);

internal sealed record PopulationDietInsights(int Count, int Swimmers, int Mature, int Aging,
    ReadOnlyCollection<GeneDistribution> Genes);

internal sealed record PopulationInsightsSnapshot(long Tick, PopulationDietInsights Herbivorous,
    PopulationDietInsights Carnivorous)
{
    public int Living => Herbivorous.Count + Carnivorous.Count;
    public int Swimmers => Herbivorous.Swimmers + Carnivorous.Swimmers;
    public int Mature => Herbivorous.Mature + Carnivorous.Mature;
    public int Aging => Herbivorous.Aging + Carnivorous.Aging;
}

// A description of the supplied world's living bodies, with no stepping, RNG,
// sensory refresh or persistent simulation counters. Founder Group is not diet.
internal static class PopulationInsights
{
    public const int BinCount = 10;
    public const float SwimmerThreshold = .85f;

    public static PopulationInsightsSnapshot Inspect(World world)
    {
        ArgumentNullException.ThrowIfNull(world);
        var herbivorous = new List<Creature>();
        var carnivorous = new List<Creature>();
        foreach (var creature in world.Creatures)
        {
            if (!creature.Alive) continue;
            (creature.Genes[Genetics.Diet] < .5f ? herbivorous : carnivorous).Add(creature);
        }
        return new PopulationInsightsSnapshot(world.Tick, Describe(world, herbivorous), Describe(world, carnivorous));
    }

    private static PopulationDietInsights Describe(World world, List<Creature> creatures)
    {
        var genes = new GeneDistribution[Genetics.Count];
        for (var gene = 0; gene < genes.Length; gene++)
        {
            var values = new float[creatures.Count];
            var bins = new int[BinCount];
            for (var i = 0; i < creatures.Count; i++)
            {
                float value = creatures[i].Genes[gene];
                values[i] = value;
                // Bins are [0,.1), [.1,.2), ... [.9,1], including exactly 1.
                bins[Math.Clamp((int)(value * BinCount), 0, BinCount - 1)]++;
            }
            Array.Sort(values);
            float? minimum = null, median = null, maximum = null;
            if (values.Length > 0)
            {
                minimum = values[0]; maximum = values[^1];
                int middle = values.Length / 2;
                median = values.Length % 2 == 0 ? (values[middle - 1] + values[middle]) * .5f : values[middle];
            }
            genes[gene] = new GeneDistribution(values.Length, minimum, median, maximum, Array.AsReadOnly(bins));
        }
        int swimmers = 0, mature = 0, aging = 0;
        foreach (var creature in creatures)
        {
            if (creature.Genes[Genetics.Aquatic] >= SwimmerThreshold) swimmers++;
            if (creature.Age >= world.Config.MaturityAge && creature.Maturity >= .98f) mature++;
            if (world.Aging(creature) > 0) aging++;
        }
        return new PopulationDietInsights(creatures.Count, swimmers, mature, aging, Array.AsReadOnly(genes));
    }
}

internal sealed class PopulationInsightsCache
{
    private World? source;
    private PopulationInsightsSnapshot? snapshot;
    public PopulationInsightsSnapshot Get(World world)
    {
        if (!ReferenceEquals(source, world) || snapshot == null || world.Tick < snapshot.Tick || world.Tick - snapshot.Tick >= 20)
        {
            source = world;
            snapshot = PopulationInsights.Inspect(world);
        }
        return snapshot;
    }
    public void Invalidate() { source = null; snapshot = null; }
}
