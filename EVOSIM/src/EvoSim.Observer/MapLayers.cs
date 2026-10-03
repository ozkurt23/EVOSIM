using EvoSim.Core;
namespace EvoSim.Observer;

public enum MapLayer { None, Population, PlantFood, Moisture }
public sealed record LayerMap(MapLayer Layer, long Tick, int Columns, double[] Values, double Maximum)
{
    public const int CellSize = 8;
    public static LayerMap Build(World world, MapLayer layer)
    {
        int size = world.Map.Size, columns = (size + CellSize - 1) / CellSize;
        var values = new double[columns * columns];
        int Bucket(float x, float y) => Math.Clamp((int)(x / CellSize), 0, columns - 1) + columns * Math.Clamp((int)(y / CellSize), 0, columns - 1);
        if (layer == MapLayer.Population)
        {
            foreach (var c in world.Creatures) if (c.Alive) values[Bucket(c.X, c.Y)]++;
        }
        else if (layer == MapLayer.PlantFood)
        {
            for (int i = 0; i < world.Map.Food.Length; i++) values[Bucket(i % size + .5f, i / size + .5f)] += world.Map.Food[i];
            foreach (var tree in world.Map.Trees) for (int slot = 0; slot < tree.Fruit.Length; slot++)
            { var position = tree.FruitPosition(slot); values[Bucket(position.X, position.Y)] += tree.Fruit[slot]; }
        }
        else if (layer == MapLayer.Moisture)
        {
            var counts = new int[values.Length];
            for (int i = 0; i < world.Map.Moisture.Length; i++)
            {
                float x = i % size + .5f, y = i / size + .5f;
                if (world.Map.IsWater(x, y)) continue;
                int bucket = Bucket(x, y); values[bucket] += world.Map.Moisture[i]; counts[bucket]++;
            }
            for (int i = 0; i < values.Length; i++) values[i] = counts[i] == 0 ? double.NaN : values[i] / counts[i];
        }
        return new(layer, world.Tick, columns, values, layer == MapLayer.Moisture ? 1 : values.Max());
    }
}
