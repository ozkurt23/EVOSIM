using EvoSim.Core;
namespace EvoSim.Observer;

// Read only: retain no Creature references and never rebuild model indices or senses.
public sealed record PopulationHealth(long Tick, int Births, int Deaths, int Mutations,
    IReadOnlyDictionary<string, int> DeathCauses, BreedingDiagnostics Breeding)
{
    public static PopulationHealth Read(World world)
    {
        var causes = world.Creatures.Where(c => !c.Alive)
            .GroupBy(c => string.IsNullOrWhiteSpace(c.DeathCause) ? "Bilinmiyor" : c.DeathCause)
            .ToDictionary(g => g.Key, g => g.Count());
        return new(world.Tick, world.Births, world.Deaths, world.Mutations, causes, world.InspectBreeding());
    }
}

