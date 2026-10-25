using Godot;

/// Read-only visual terrain sampling. Never reads or changes simulation state.
public partial class SurfaceSampler : RefCounted
{
    private int size;
    private int[] tiles = [];
    private Vector2 origin;
    private readonly FastNoiseLite noise = new() { Seed = 42, Frequency = .09f };

    public void Configure(int mapSize, int[] mapTiles, Vector2 mapOrigin)
    {
        size = mapSize;
        tiles = (int[])mapTiles.Clone();
        origin = mapOrigin;
    }
    public int Tile(int x, int z) => tiles[Math.Clamp(x, 0, size - 1) + Math.Clamp(z, 0, size - 1) * size];
    private double Water(int x, int z) => Tile(x,z) == 1 ? -1 : 1;
    public double RawField(Vector2 p)
    {
        var q = p + origin - new Vector2(.5f,.5f);
        int x = (int)Math.Floor(q.X), z = (int)Math.Floor(q.Y);
        double u = q.X-x, v = q.Y-z;
        double a = Water(x,z), b = Water(x+1,z), c = Water(x,z+1), d = Water(x+1,z+1);
        double ab = a+(b-a)*u, cd = c+(d-c)*u;
        return ab+(cd-ab)*v;
    }
    public double Field(Vector2 p)
    {
        double center = RawField(p);
        if (Math.Abs(center) > .99) return center;
        return (center*4 + RawField(p+new Vector2(.4f,0)) + RawField(p+new Vector2(-.4f,0))
            + RawField(p+new Vector2(0,.4f)) + RawField(p+new Vector2(0,-.4f)))/8;
    }
    public double Height(Vector2 p)
    {
        double f = Field(p);
        if (f < 0) return -.055;
        // GDScript scalar arithmetic is double precision; preserve it until the engine call.
        double hill = .30+(noise.GetNoise2D((float)((double)p.X+origin.X-42), (float)((double)p.Y+origin.Y-20))+1.0)*.28;
        f = Math.Clamp(f,0,1);
        return .09+f*f*(3-2*f)*hill;
    }
    public Image ShoreImage()
    {
        // One bulk upload replaces 262,144 script sampling calls and 65,536 pixel calls.
        byte[] pixels = new byte[256*256*3];
        for (int y=0; y<256; y++) for (int x=0; x<256; x++)
        {
            var p = new Vector2(x,y)*48f/255f;
            double edge = Math.Max(Math.Max(Field(p+new Vector2(.75f,0)),Field(p-new Vector2(.75f,0))),
                Math.Max(Field(p+new Vector2(0,.75f)),Field(p-new Vector2(0,.75f))));
            byte value = (byte)(Math.Clamp((edge+1)*.5,0,1)*255);
            int i = (x+y*256)*3;
            pixels[i]=pixels[i+1]=pixels[i+2]=value;
        }
        return Image.CreateFromData(256,256,false,Image.Format.Rgb8,pixels);
    }
}
