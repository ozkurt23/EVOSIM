using EvoSim.Core;
using System.Text.Json;

/// <summary>
/// Stable visual reference, NOT the terrain's private carrying capacity.
/// An independent terrain generator supplies initial stock and potential grass cells.
/// Its isolated RNG never advances or mutates the observed world's state.
/// </summary>
internal sealed class GroundFoodReference
{
    private string signature = "";
    private float[] initialStock = Array.Empty<float>();
    private static readonly JsonSerializerOptions Options = new() { IncludeFields = true };

    public float[] For(World world)
    {
        string next = world.Seed + ":" + JsonSerializer.Serialize(world.Config, Options);
        if (next != signature)
        {
            var initialTerrain = new Terrain(world.Seed, world.Config);
            initialStock = initialTerrain.Food;
            signature = next;
        }
        return initialStock;
    }
}
