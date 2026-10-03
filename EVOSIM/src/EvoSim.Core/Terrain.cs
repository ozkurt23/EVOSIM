using System;
using System.Collections.Generic;
using System.Numerics;

namespace EvoSim.Core
{
    /// <summary>A tree's canopy radius. Its trunk occupies its centre terrain cell.</summary>
    public sealed class Tree
    {
        public float X, Y, Radius;
        /// <summary>Fruit distance from the trunk as a fraction of canopy radius.</summary>
        public float FruitRadius=.8f;
        public const int FruitSlots=7;
        public float[] Fruit=new float[FruitSlots];
        public float[] Regrow=new float[FruitSlots];
        public Tree() {}
        public Tree(float x,float y,float radius,int slots=FruitSlots,float energy=18f,float fruitRadius=.8f){X=x;Y=y;Radius=radius;FruitRadius=fruitRadius;Fruit=new float[slots];Regrow=new float[slots];for(int i=0;i<slots;i++)Fruit[i]=energy;}
        public Vector2 FruitPosition(int slot) { float a=slot*6.2831853f/Fruit.Length+.35f; return new Vector2(X+(float)Math.Cos(a)*Radius*FruitRadius,Y+(float)Math.Sin(a)*Radius*FruitRadius); }
    }

    /// <summary>
    /// Engine-independent terrain. Vector2.Y is the world's Z coordinate.
    /// Queries and pathfinding have a single simulation-thread owner. Water geometry
    /// is fixed in this first model; drought changes moisture, drinking supply and plants.
    /// </summary>
    public sealed class Terrain
    {
        public const int DefaultSize = 128;
        public int Size { get; }
        public int CellCount { get; }
        public const float SwimmingThreshold = 0.85f;
        public float[] Food;
        public float[] Moisture;
        public byte[] Tiles;
        public List<Tree> Trees = new List<Tree>();
        public List<Vector2> Shelters = new List<Vector2>();
        public float WaterLevel = 1f;

        private readonly WorldConfig _config;
        private readonly float[] _baseMoisture;
        private readonly float[] _capacity;
        private readonly int[] _landComponent;
        private readonly List<Vector2> _shores = new List<Vector2>();
        private readonly float[] _g;
        private readonly int[] _parent;
        private readonly int[] _seen;
        private readonly int[] _closed;
        private readonly int[] _walkStamp;
        private readonly bool[] _walk;
        private readonly List<OpenNode> _open = new List<OpenNode>(1024);
        private int _searchId;

        private static readonly int[] Dx = { 1, 0, -1, 0, 1, -1, -1, 1 };
        private static readonly int[] Dy = { 0, 1, 0, -1, 1, 1, -1, -1 };

        public Terrain(int seed,WorldConfig config=null)
        {
            _config=config??new WorldConfig();
            _config.Validate();
            Size=_config.MapSize;CellCount=checked(Size*Size);
            Food=new float[CellCount];Moisture=new float[CellCount];Tiles=new byte[CellCount];
            _baseMoisture=new float[CellCount];_capacity=new float[CellCount];_g=new float[CellCount];
            _landComponent=new int[CellCount];_parent=new int[CellCount];_seen=new int[CellCount];_closed=new int[CellCount];_walkStamp=new int[CellCount];_walk=new bool[CellCount];
            bool legacySize=Size==DefaultSize;float layoutScale=Size/(float)DefaultSize;
            float Layout(float coordinate)=>legacySize?coordinate:coordinate*layoutScale;
            float Logical(float coordinate)=>legacySize?coordinate:coordinate/layoutScale;
            // Isolated, explicitly versioned map stream; it never consumes the
            // population/reproduction RNG. Same-platform numerical rules still apply.
            var rng = new PcgRandom(unchecked((ulong)(uint)seed), 17);
            float phase = (float)rng.NextDouble() * 6.283185f;
            float riverShift = (float)rng.NextDouble() * 10f - 5f;
            // Generator v1 preserves the historical 128 map; v2 stretches only larger layout anchors.
            float LegacyRiverX(float y) => 65f + riverShift + 11f * (float)Math.Sin(y * 0.057f + phase)
                + 4f * (float)Math.Sin(y * 0.129f + phase * 0.6f);
            float RiverX(float y)=>Layout(LegacyRiverX(Logical(y)));
            float BranchX(float y){float remaining=58f-Logical(y);return legacySize
                ? LegacyRiverX(58f)+remaining*.68f+2f*(float)Math.Sin(remaining*.12f)
                : RiverX(y)+Layout(remaining*.68f+2f*(float)Math.Sin(remaining*.12f));}

            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float fx = x + 0.5f, fy = y + 0.5f,logicalY=Logical(fy);
                float halfWidth = _config.RiverWidth*(3.6f + 0.8f * (float)Math.Sin(logicalY * 0.09f + phase));
                bool river = Math.Abs(fx - RiverX(fy)) < halfWidth;
                // A narrower tributary joins the main river from the north-east.
                float branchX = Layout(LegacyRiverX(58f) + (58f - logicalY) * 0.68f
                    + 2f * (float)Math.Sin((58f - logicalY) * 0.12f));
                // On larger maps the branch stays east of the current main channel until
                // one monotonic join; scaling the legacy join anchor could create tiny islands.
                if(!legacySize)branchX=BranchX(fy);
                bool branch = logicalY < 58f && Math.Abs(fx - branchX) < 1.65f*_config.RiverWidth;
                // A small lake on the western bank provides a second drinking landscape.
                float px = (fx - Layout(25f + riverShift)) / 6.5f;
                float py = (fy - Layout(88f)) / 9f;
                bool pond = px * px + py * py < 1f + 0.08f * Math.Sin(fy * 0.7f);
                if(_config.LandRegions==1){river=logicalY>24&&logicalY<105&&Math.Abs(fx-RiverX(fy))<halfWidth;branch=false;}
                else if(_config.LandRegions!=3){
                    river=false;branch=false;float spacing=Size/(float)_config.LandRegions;
                    for(int r=1;r<_config.LandRegions;r++){
                        float curve=spacing*r+Math.Min(Layout(5f),spacing*.12f)*(float)Math.Sin(logicalY*.054f+phase+r*.7f);
                        float width=Math.Min(halfWidth,spacing*.16f);
                        if(Math.Abs(fx-curve)<width)river=true;
                    }
                    float lakeX=spacing*.46f,rx=Math.Min(6.5f,spacing*.18f);
                    float lx=(fx-lakeX)/rx,ly=(fy-Layout(88f))/8f;pond=lx*lx+ly*ly<1;
                }
                if (river || branch || pond) Tiles[y * Size + x] = 1;
            }

            if(_config.GeneratorVersion>=2){
                // Conservative coverage of cells crossed by the thin tributary centreline
                // keeps diagonal reaches connected even at the minimum physical river width.
                if(_config.LandRegions==3){float joinY=Layout(58f);for(int y=0;y<joinY;y++){
                    float a=BranchX(y),b=BranchX(Math.Min(y+1f,joinY));
                    int x0=Math.Max(0,(int)Math.Floor(Math.Min(a,b))),x1=Math.Min(Size-1,(int)Math.Floor(Math.Max(a,b)));
                    for(int x=x0;x<=x1;x++)Tiles[y*Size+x]=1;
                }}
                FillEnclosedJunctionSlivers();
            }
            ComputeMoisture();

            // Natural rock outcrops, predominantly away from moist riverbanks.
            for (int cluster = 0; cluster < _config.RockClusters; cluster++)
            {
                float cx = 5f + (float)rng.NextDouble() * (Size-10f);
                float cy = 5f + (float)rng.NextDouble() * (Size-10f);
                if (IsWater(cx, cy)) continue;
                for (int pebble = 0; pebble < 3 + rng.NextInt(5); pebble++)
                {
                    float rx = cx + (float)(rng.NextDouble() - 0.5) * 5f;
                    float ry = cy + (float)(rng.NextDouble() - 0.5) * 5f;
                    float rr = 0.65f + (float)rng.NextDouble() * 1.2f;
                    for (int y = Math.Max(1, (int)(ry - rr)); y <= Math.Min(Size - 2, (int)(ry + rr)); y++)
                    for (int x = Math.Max(1, (int)(rx - rr)); x <= Math.Min(Size - 2, (int)(rx + rr)); x++)
                    {
                        int i = y * Size + x;
                        float dx = x + 0.5f - rx, dy = y + 0.5f - ry;
                        if (Tiles[i] == 0 && dx * dx + dy * dy < rr * rr) Tiles[i] = 2;
                    }
                }
            }

            for (int attempt = 0; attempt < 12000 && Trees.Count < _config.TreeCount && _config.TreeMoistureAffinity>0; attempt++)
            {
                int x = 3 + rng.NextInt(Size - 6), y = 3 + rng.NextInt(Size - 6);
                int i = y * Size + x;
                if (Tiles[i] != 0 || rng.NextDouble() > _baseMoisture[i] * _config.TreeMoistureAffinity) continue;
                var p = new Vector2(x + 0.5f, y + 0.5f);
                if (!CanOccupy(p.X, p.Y, 1.1f, 0f)) continue;
                bool crowded = false;
                foreach (var tree in Trees)
                {
                    float dx = tree.X - p.X, dy = tree.Y - p.Y;
                    if (dx * dx + dy * dy < _config.TreeSpacing*_config.TreeSpacing) { crowded = true; break; }
                }
                if (crowded) continue;
                Tiles[i] = 2;
                Trees.Add(new Tree(p.X, p.Y, _config.TreeRadiusMin + (float)rng.NextDouble() * (_config.TreeRadiusMax-_config.TreeRadiusMin),_config.FruitSlots,_config.FruitEnergy,_config.TreeFruitRadius));
            }

            // Separate, finite meadows. Most land has zero edible grass capacity.
            var meadows=new List<Vector2>();
            for(int j=0;j<_config.MeadowCount;j++){
                var candidate=FindSpawn(rng,.4f,0);
                if(_baseMoisture[Cell(candidate.X,candidate.Y)]>.25f)meadows.Add(candidate);
            }
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++){
                int i=y*Size+x;if(Tiles[i]!=0)continue;float patch=0;
                foreach(var m in meadows){float dx=(x+.5f-m.X)/_config.MeadowX,dy=(y+.5f-m.Y)/_config.MeadowY;float d=dx*dx+dy*dy;
                    patch=Math.Max(patch,Math.Max(0,1-d+.12f*(float)Math.Sin(x*.8f+y*.6f)));}
                if(patch<.13f)continue;
                _capacity[i]=_config.GrassCapacity*patch*(.5f+_baseMoisture[i]);
                Food[i]=_capacity[i]*(.6f+.4f*rng.NextFloat());
            }

            BuildLandComponentsAndShores();
            AddShelterNear(new Vector2(Layout(29f), Layout(34f)));
            AddShelterNear(new Vector2(Layout(91f), Layout(83f)));
            AddShelterNear(new Vector2(Layout(35f), Layout(109f)));
        }

        // The river layout creates boundary-connected land regions. Rasterizing a
        // thin branch junction can leave a one-cell island; absorb such enclosed slivers
        // into the river before deriving moisture or placing resources. No RNG is used.
        private void FillEnclosedJunctionSlivers()
        {
            var seen=new bool[CellCount];var queue=new int[CellCount];
            for(int start=0;start<CellCount;start++){
                if(Tiles[start]==1||seen[start])continue;int head=0,tail=0;bool boundary=false;
                seen[start]=true;queue[tail++]=start;
                while(head<tail){int cell=queue[head++],x=cell%Size,y=cell/Size;if(x==0||y==0||x==Size-1||y==Size-1)boundary=true;
                    for(int d=0;d<4;d++){int xx=x+Dx[d],yy=y+Dy[d];if(xx<0||yy<0||xx>=Size||yy>=Size)continue;int next=xx+yy*Size;if(Tiles[next]==1||seen[next])continue;seen[next]=true;queue[tail++]=next;}
                }
                if(!boundary)for(int i=0;i<tail;i++)Tiles[queue[i]]=1;
            }
        }

        public int LandRegionCount(){var seen=new bool[Tiles.Length];var queue=new Queue<int>();int count=0;for(int i=0;i<Tiles.Length;i++){if(seen[i]||Tiles[i]==1)continue;count++;seen[i]=true;queue.Enqueue(i);while(queue.Count>0){int cell=queue.Dequeue(),x=cell%Size,y=cell/Size;for(int d=0;d<4;d++){int xx=x+Dx[d],yy=y+Dy[d];if(xx<0||yy<0||xx>=Size||yy>=Size)continue;int n=xx+yy*Size;if(!seen[n]&&Tiles[n]!=1){seen[n]=true;queue.Enqueue(n);}}}}return count;}
        public int Cell(float x, float y)
        {
            if (!Finite(x) || !Finite(y) || x < 0f || y < 0f || x >= Size || y >= Size) return -1;
            return (int)y * Size + (int)x;
        }

        public bool IsWater(float x, float y)
        {
            int i = Cell(x, y);
            return i >= 0 && Tiles[i] == 1;
        }

        /// <summary>Exact disc-versus-cell-box clearance, including the map boundary.</summary>
        public bool CanOccupy(float x, float y, float radius, float aquatic)
        {
            if (!Finite(x) || !Finite(y) || !Finite(radius) || !Finite(aquatic) || radius < 0f
                || x - radius < 0f || y - radius < 0f || x + radius >= Size || y + radius >= Size) return false;
            int x0 = Math.Max(0, (int)Math.Floor(x - radius));
            int x1 = Math.Min(Size - 1, (int)Math.Floor(x + radius));
            int y0 = Math.Max(0, (int)Math.Floor(y - radius));
            int y1 = Math.Min(Size - 1, (int)Math.Floor(y + radius));
            float rr = radius * radius;
            for (int cy = y0; cy <= y1; cy++)
            for (int cx = x0; cx <= x1; cx++)
            {
                byte t = Tiles[cy * Size + cx];
                if (t == 0 || (t == 1 && aquatic >= SwimmingThreshold)) continue;
                float nx = Clamp(x, cx, cx + 1f), ny = Clamp(y, cy, cy + 1f);
                float dx = x - nx, dy = y - ny;
                if (dx * dx + dy * dy <= rr + 0.000001f) return false;
            }
            return true;
        }

        public Vector2 FindSpawn(PcgRandom rng, float radius, float aquatic)
        {
            // Founders begin on land even if their genotype would allow swimming.
            for (int attempt = 0; attempt < 12000; attempt++)
            {
                float x = 2f + rng.NextFloat() * (Size - 4f);
                float y = 2f + rng.NextFloat() * (Size - 4f);
                int i = Cell(x, y);
                if (i >= 0 && Tiles[i] == 0 && CanOccupy(x, y, radius, aquatic)) return new Vector2(x, y);
            }
            for (int i = 0; i < Tiles.Length; i++)
            {
                var p = Centre(i);
                if (Tiles[i] == 0 && CanOccupy(p.X, p.Y, radius, aquatic)) return p;
            }
            throw new InvalidOperationException("No legal founder position for the requested body radius.");
        }

        /// <summary>
        /// Returns a land centre in the caller's connected land component, at most
        /// range away, within 1.5 units of water; NaN/NaN means none. Validated for a
        /// radius-0.35 land body; the caller must validate its own body and route.
        /// </summary>
        public Vector2 NearestShore(float x, float y, float range)
        {
            int source = Cell(x, y);
            if (source < 0 || !Finite(range) || range < 0f) return Missing;
            int component = _landComponent[source];
            if (component == 0 && Tiles[source] != 1) return Missing;
            float best = range * range;
            Vector2 result = Missing;
            foreach (var shore in _shores)
            {
                if (component != 0 && _landComponent[Cell(shore.X, shore.Y)] != component) continue;
                float dx = shore.X - x, dy = shore.Y - y, d = dx * dx + dy * dy;
                if (d <= best) { best = d; result = shore; }
            }
            return result;
        }

        /// <summary>8-neighbour A*, exact footprint clearance, stable tie-breaks, no corner cuts.</summary>
        public List<Vector2> FindPath(Vector2 start, Vector2 goal, float radius, float aquatic)
        {
            var path = new List<Vector2>();
            if (!CanOccupy(start.X, start.Y, radius, aquatic) || !CanOccupy(goal.X, goal.Y, radius, aquatic)) return path;
            if (aquatic < SwimmingThreshold && _landComponent[Cell(start.X, start.Y)] != _landComponent[Cell(goal.X, goal.Y)])
                return path;
            if (HasClearPath(start, goal, radius, aquatic)) { path.Add(goal); return path; }

            NextSearch();
            int source = NearestPathCell(start, radius, aquatic);
            int target = NearestPathCell(goal, radius, aquatic);
            if (source < 0 || target < 0) return path;
            _g[source] = 0f; _parent[source] = -1; _seen[source] = _searchId;
            Push(new OpenNode(source, Heuristic(source, target), 0f));
            bool reached = false;
            while (_open.Count > 0)
            {
                OpenNode node = Pop();
                int current = node.Cell;
                if (_closed[current] == _searchId || node.G > _g[current]) continue;
                if (current == target) { reached = true; break; }
                _closed[current] = _searchId;
                int cx = current % Size, cy = current / Size;
                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + Dx[d], ny = cy + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) continue;
                    int next = ny * Size + nx;
                    if (_closed[next] == _searchId || !Walkable(next, radius, aquatic)) continue;
                    if (d >= 4 && (!Walkable(cy * Size + nx, radius, aquatic)
                        || !Walkable(ny * Size + cx, radius, aquatic))) continue;
                    // Endpoint clearance alone is insufficient for larger body discs.
                    if (!HasClearPath(Centre(current), Centre(next), radius, aquatic)) continue;
                    float step = d < 4 ? 1f : 1.41421356f;
                    float waterCost = Tiles[current] == 1 || Tiles[next] == 1 ? 2.2f : 1f;
                    float candidate = _g[current] + step * waterCost;
                    if (_seen[next] == _searchId && candidate >= _g[next]) continue;
                    _seen[next] = _searchId; _g[next] = candidate; _parent[next] = current;
                    Push(new OpenNode(next, candidate + Heuristic(next, target), candidate));
                }
            }
            if (!reached) return path;
            for (int at = target; at >= 0; at = _parent[at]) path.Add(Centre(at));
            path.Reverse();
            if (path.Count > 0 && Vector2.DistanceSquared(path[0], start) < 0.0001f) path.RemoveAt(0);
            if (path.Count == 0 || Vector2.DistanceSquared(path[path.Count - 1], goal) > 0.0001f) path.Add(goal);
            return path;
        }

        /// <summary>
        /// Swept-disc check using the exact minimum distance from the movement
        /// segment to each blocked cell rectangle, including its rounded corners.
        /// </summary>
        public bool HasClearPath(Vector2 from, Vector2 to, float radius, float aquatic)
        {
            if (!CanOccupy(from.X, from.Y, radius, aquatic) || !CanOccupy(to.X, to.Y, radius, aquatic)) return false;
            int x0 = Math.Max(0, (int)Math.Floor(Math.Min(from.X, to.X) - radius));
            int x1 = Math.Min(Size - 1, (int)Math.Floor(Math.Max(from.X, to.X) + radius));
            int y0 = Math.Max(0, (int)Math.Floor(Math.Min(from.Y, to.Y) - radius));
            int y1 = Math.Min(Size - 1, (int)Math.Floor(Math.Max(from.Y, to.Y) + radius));
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                byte t = Tiles[y * Size + x];
                if (t == 0 || (t == 1 && aquatic >= SwimmingThreshold)) continue;
                if (SegmentIntersectsBox(from, to, x, y, x + 1f, y + 1f)) return false;
                float rr = radius * radius + 0.000001f;
                if (PointBoxDistanceSquared(from, x, y) <= rr || PointBoxDistanceSquared(to, x, y) <= rr
                    || PointSegmentDistanceSquared(new Vector2(x, y), from, to) <= rr
                    || PointSegmentDistanceSquared(new Vector2(x + 1f, y), from, to) <= rr
                    || PointSegmentDistanceSquared(new Vector2(x, y + 1f), from, to) <= rr
                    || PointSegmentDistanceSquared(new Vector2(x + 1f, y + 1f), from, to) <= rr) return false;
            }
            return true;
        }

        /// <summary>Call exactly once per simulated second; food units are abstract energy.</summary>
        public void EcologyStep(bool drought)
        {
            float target = drought ? 0.12f : 1f;
            WaterLevel += Clamp(target - WaterLevel, -0.009f, 0.004f);
            foreach(var tree in Trees)for(int slot=0;slot<tree.Fruit.Length;slot++){
                if(tree.Fruit[slot]>0)continue;
                tree.Regrow[slot]=Math.Max(0,tree.Regrow[slot]-(_config.FruitDroughtTolerance+(1-_config.FruitDroughtTolerance)*WaterLevel));
                if(tree.Regrow[slot]<=0)tree.Fruit[slot]=_config.FruitEnergy;
            }
            for (int i = 0; i < Tiles.Length; i++)
            {
                Moisture[i] = _baseMoisture[i] * (0.18f + 0.82f * WaterLevel);
                if (Tiles[i] != 0) { Food[i] = 0f; continue; }
                float capacity = _capacity[i] * (0.12f + 0.88f * WaterLevel);
                float stock = Math.Max(0f, Food[i]);
                if (stock > capacity) stock -= Math.Min(stock - capacity, 0.018f + (1f - WaterLevel) * 0.07f);
                else if (capacity > 0f)
                {
                    float growth = _config.GrassRegrowth*(0.008f + 0.022f * Moisture[i]) * (1f - stock / capacity);
                    stock = Math.Min(capacity, stock + growth);
                }
                Food[i] = stock;
            }
        }

        private void ComputeMoisture()
        {
            var distance = new int[CellCount];
            var queue = new int[CellCount];
            int head = 0, tail = 0;
            for (int i = 0; i < distance.Length; i++)
            {
                distance[i] = int.MaxValue;
                if (Tiles[i] == 1) { distance[i] = 0; queue[tail++] = i; }
            }
            while (head < tail)
            {
                int i = queue[head++], x = i % Size, y = i / Size;
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + Dx[d], ny = y + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) continue;
                    int n = ny * Size + nx;
                    if (distance[n] != int.MaxValue) continue;
                    distance[n] = distance[i] + 1; queue[tail++] = n;
                }
            }
            for (int i = 0; i < distance.Length; i++)
                Moisture[i] = _baseMoisture[i] = 0.10f + 0.90f * (float)Math.Exp(-distance[i] / 15f);
        }

        private void BuildLandComponentsAndShores()
        {
            var queue = new int[CellCount];
            int component = 0;
            for (int root = 0; root < Tiles.Length; root++)
            {
                if (Tiles[root] != 0 || _landComponent[root] != 0) continue;
                var p = Centre(root);
                if (!CanOccupy(p.X, p.Y, 0.35f, 0f)) continue;
                component++;
                int head = 0, tail = 0;
                queue[tail++] = root; _landComponent[root] = component;
                while (head < tail)
                {
                    int cell = queue[head++], x = cell % Size, y = cell / Size;
                    bool besideWater = false;
                    for (int d = 0; d < 8; d++)
                    {
                        int nx = x + Dx[d], ny = y + Dy[d];
                        if (nx < 0 || ny < 0 || nx >= Size || ny >= Size) continue;
                        int n = ny * Size + nx;
                        if (Tiles[n] == 1) besideWater = true;
                        if (d >= 4 || Tiles[n] != 0 || _landComponent[n] != 0) continue;
                        var np = Centre(n);
                        if (!CanOccupy(np.X, np.Y, 0.35f, 0f)) continue;
                        _landComponent[n] = component; queue[tail++] = n;
                    }
                    if (besideWater) _shores.Add(Centre(cell));
                }
            }
        }

        private void AddShelterNear(Vector2 desired)
        {
            for (int ring = 0; ring < 12; ring++)
            for (int y = -ring; y <= ring; y++)
            for (int x = -ring; x <= ring; x++)
            {
                var p = new Vector2((int)desired.X + x + 0.5f, (int)desired.Y + y + 0.5f);
                if (CanOccupy(p.X, p.Y, 1.15f, 0f)) { Shelters.Add(p); return; }
            }
        }

        private int NearestPathCell(Vector2 p, float radius, float aquatic)
        {
            int cell = Cell(p.X, p.Y);
            if (cell < 0) return -1;
            int best = -1; float distance = float.PositiveInfinity;
            int px = cell % Size, py = cell / Size;
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                int x = px + dx, y = py + dy;
                if (x < 0 || y < 0 || x >= Size || y >= Size) continue;
                int n = y * Size + x;
                if (!Walkable(n, radius, aquatic)) continue;
                var q = Centre(n); float d = Vector2.DistanceSquared(p, q);
                if (d < distance && HasClearPath(p, q, radius, aquatic)) { best = n; distance = d; }
            }
            return best;
        }

        private bool Walkable(int cell, float radius, float aquatic)
        {
            if (_walkStamp[cell] == _searchId) return _walk[cell];
            var p = Centre(cell);
            _walkStamp[cell] = _searchId;
            return _walk[cell] = CanOccupy(p.X, p.Y, radius, aquatic);
        }

        private void NextSearch()
        {
            if (++_searchId == int.MaxValue)
            {
                Array.Clear(_seen, 0, _seen.Length); Array.Clear(_closed, 0, _closed.Length);
                Array.Clear(_walkStamp, 0, _walkStamp.Length); _searchId = 1;
            }
            _open.Clear();
        }

        private float Heuristic(int a, int b)
        {
            int dx = Math.Abs(a % Size - b % Size), dy = Math.Abs(a / Size - b / Size);
            return Math.Max(dx, dy) + 0.41421356f * Math.Min(dx, dy);
        }

        private readonly struct OpenNode
        {
            public readonly int Cell;
            public readonly float F, G;
            public OpenNode(int cell, float f, float g) { Cell = cell; F = f; G = g; }
        }

        private static bool Earlier(OpenNode a, OpenNode b) => a.F < b.F || (a.F == b.F && a.Cell < b.Cell);

        private void Push(OpenNode node)
        {
            int i = _open.Count; _open.Add(node);
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (!Earlier(node, _open[parent])) break;
                _open[i] = _open[parent]; i = parent;
            }
            _open[i] = node;
        }

        private OpenNode Pop()
        {
            OpenNode result = _open[0], last = _open[_open.Count - 1];
            _open.RemoveAt(_open.Count - 1);
            if (_open.Count == 0) return result;
            int i = 0;
            while (i * 2 + 1 < _open.Count)
            {
                int child = i * 2 + 1;
                if (child + 1 < _open.Count && Earlier(_open[child + 1], _open[child])) child++;
                if (!Earlier(_open[child], last)) break;
                _open[i] = _open[child]; i = child;
            }
            _open[i] = last; return result;
        }

        private static bool SegmentIntersectsBox(Vector2 a, Vector2 b, float minX, float minY, float maxX, float maxY)
        {
            float enter = 0f, exit = 1f;
            return Clip(a.X, b.X - a.X, minX, maxX, ref enter, ref exit)
                && Clip(a.Y, b.Y - a.Y, minY, maxY, ref enter, ref exit);
        }

        private static bool Clip(float origin, float delta, float min, float max, ref float enter, ref float exit)
        {
            if (Math.Abs(delta) < 0.0000001f) return origin >= min && origin <= max;
            float t0 = (min - origin) / delta, t1 = (max - origin) / delta;
            if (t0 > t1) { float swap = t0; t0 = t1; t1 = swap; }
            enter = Math.Max(enter, t0); exit = Math.Min(exit, t1);
            return enter <= exit;
        }

        private static float PointBoxDistanceSquared(Vector2 point, float x, float y)
        {
            float dx = point.X - Clamp(point.X, x, x + 1f), dy = point.Y - Clamp(point.Y, y, y + 1f);
            return dx * dx + dy * dy;
        }

        private static float PointSegmentDistanceSquared(Vector2 point, Vector2 start, Vector2 end)
        {
            Vector2 d = end - start;
            float lengthSquared = d.LengthSquared();
            float t = lengthSquared > 0f ? Clamp(Vector2.Dot(point - start, d) / lengthSquared, 0f, 1f) : 0f;
            return Vector2.DistanceSquared(point, start + t * d);
        }

        private Vector2 Centre(int cell) => new Vector2(cell % Size + 0.5f, cell / Size + 0.5f);
        private static Vector2 Missing => new Vector2(float.NaN, float.NaN);
        private static float Clamp(float value, float low, float high) => Math.Max(low, Math.Min(high, value));
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
