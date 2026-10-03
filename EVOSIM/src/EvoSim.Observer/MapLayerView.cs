using EvoSim.Core;
namespace EvoSim.Observer;

internal sealed partial class WorldView
{
    public MapLayer Layer;
    public LayerMap? LayerData { get; private set; }
    World? layerWorld; bool layerDrought;
    Vertex[] layerVertices = new Vertex[0];
    public static Col LayerColor(MapLayer layer, float fraction)
    {
        fraction = Math.Clamp(fraction, 0, 1);
        if (layer == MapLayer.Moisture) return Col.Mix(Col.Hex(0xC99D65), Col.Hex(0x248DAB), fraction);
        var low = layer == MapLayer.Population ? Col.Hex(0x42A7B7) : Col.Hex(0x76A14E);
        var middle = Col.Hex(0xE9CA66); var high = Col.Hex(0xCA6449);
        return fraction < .5f ? Col.Mix(low, middle, fraction * 2) : Col.Mix(middle, high, fraction * 2 - 1);
    }
    void DrawMapLayer(World world)
    {
        if (Layer == MapLayer.None) { LayerData = null; layerWorld = null; return; }
        if (!ReferenceEquals(layerWorld, world) || LayerData?.Layer != Layer || LayerData.Tick != world.Tick || layerDrought != world.Drought)
        { LayerData = LayerMap.Build(world, Layer); layerWorld = world; layerDrought = world.Drought; }
        var data = LayerData!; int count = 0;
        if (layerVertices.Length < data.Values.Length * 6) layerVertices = new Vertex[data.Values.Length * 6];
        for (int i = 0; i < data.Values.Length; i++)
        {
            double value = data.Values[i]; if (!double.IsFinite(value) || (value <= 0 && Layer != MapLayer.Moisture)) continue;
            float x = i % data.Columns * LayerMap.CellSize, y = i / data.Columns * LayerMap.CellSize;
            float x2=Math.Min(world.Map.Size,x+LayerMap.CellSize),y2=Math.Min(world.Map.Size,y+LayerMap.CellSize);
            var a=Project(x,y,LandHeight+.01f);var b=Project(x2,y,LandHeight+.01f);var c=Project(x2,y2,LandHeight+.01f);var d=Project(x,y2,LandHeight+.01f);
            float fraction = data.Maximum > 0 ? (float)(value / data.Maximum) : 0;
            var color = LayerColor(Layer, fraction).Alpha((byte)(Layer == MapLayer.Moisture ? 140 : 65 + 100 * Math.Sqrt(fraction)));
            layerVertices[count++] = new(a.X, a.Y, color); layerVertices[count++] = new(b.X, b.Y, color); layerVertices[count++] = new(c.X, c.Y, color);
            layerVertices[count++] = new(a.X, a.Y, color); layerVertices[count++] = new(c.X, c.Y, color); layerVertices[count++] = new(d.X, d.Y, color);
        }
        if (count > 0) SDL.SDL_RenderGeometry(p.Renderer, IntPtr.Zero, layerVertices, count, null, 0);
    }
}
