using System.Text.Json;
using System.Security.Cryptography;

namespace EvoSim.Ecology;

public sealed record ForestSettings(double LifespanDays=21900,double MaturityDays=1095,
    double DroughtTolerance=.6,double SeedSpread=.2,double Density=.5,
    double CorpseContribution=.4,double SoilHalfLifeDays=730)
{
    public void Validate()
    {
        foreach(double value in new[]{LifespanDays,MaturityDays,SoilHalfLifeDays})
            if(!double.IsFinite(value)||value<0||value>36500000)throw new ArgumentOutOfRangeException(nameof(value));
        foreach(double value in new[]{DroughtTolerance,SeedSpread,Density,CorpseContribution})
            if(!double.IsFinite(value)||value<0||value>1)throw new ArgumentOutOfRangeException(nameof(value));
    }
}
public sealed record ForestTree(long Id,int Cell,double Age,double Growth,double Health,
    double LifespanMultiplier,long DeadDay=-1);
public sealed record ForestSeed(int Cell,long Expires);
public sealed record ForestSnapshot(string Format,int Size,ForestSettings Settings,bool[] Land,
    double[] BaseMoisture,double[] Soil,long Day,long NextId,ulong RandomState,ForestTree[] Trees,ForestSeed[] Seeds);

/// <summary>Fixed daily ecology; no renderer, clock, creature policy or legacy Core dependency.
/// Deposits must represent actual decay, never eaten mass. Host owns that accounting.</summary>
public sealed class Forest
{
    public const string Format="EVOSIM-FOREST-1";
    public const int MaximumBytes=16*1024*1024;
    readonly bool[] land;
    readonly double[] moisture,soil;
    readonly List<ForestTree> trees=new();
    readonly SortedDictionary<int,long> seeds=new();
    const int BucketSize=8;
    readonly Dictionary<int,List<int>> buckets=new();
    readonly List<int> neighbours=new();
    readonly HashSet<int> treeCells=new();
    int Bucket(int cell)=>(cell/Size/BucketSize)*32+cell%Size/BucketSize;
    void IndexTree(int index)
    {
        int cell=trees[index].Cell,key=Bucket(cell);
        if(!buckets.TryGetValue(key,out var entries))buckets[key]=entries=new();
        entries.Add(index);treeCells.Add(cell);
    }
    void RebuildIndex()
    {
        buckets.Clear();treeCells.Clear();
        for(int i=0;i<trees.Count;i++)IndexTree(i);
    }
    List<int> Nearby(int cell,double radius)
    {
        neighbours.Clear();int x=cell%Size,y=cell/Size;
        int x0=Math.Max(0,(int)Math.Floor((x-radius)/BucketSize)),x1=Math.Min((Size-1)/BucketSize,(int)((x+radius)/BucketSize));
        int y0=Math.Max(0,(int)Math.Floor((y-radius)/BucketSize)),y1=Math.Min((Size-1)/BucketSize,(int)((y+radius)/BucketSize));
        for(int by=y0;by<=y1;by++)for(int bx=x0;bx<=x1;bx++)
            if(buckets.TryGetValue(by*32+bx,out var entries))neighbours.AddRange(entries);
        // Preserve the original tree-list accumulation order exactly, including
        // floating-point rounding and sequential deaths within the daily step.
        neighbours.Sort();return neighbours;
    }
    bool Crowded(int cell,double spacing)
    {
        foreach(int index in Nearby(cell,spacing))
        {
            var other=trees[index];double dx=cell%Size-other.Cell%Size,dy=cell/Size-other.Cell/Size;
            if(dx*dx+dy*dy<spacing*spacing)return true;
        }
        return false;
    }
    ulong random;
    long nextId=1;
    public int Size{get;}
    public long Day{get;private set;}
    public ForestSettings Settings{get;}
    public IReadOnlyList<ForestTree> Trees=>trees.AsReadOnly();
    public int SeedCount=>seeds.Count;
    public double SoilAt(int cell)=>soil[cell];
    public Forest(int size,bool[] land,double[] baseMoisture,ulong seed,ForestSettings? settings=null)
    {
        if(size<2||size>256||land.Length!=size*size||baseMoisture.Length!=land.Length)throw new ArgumentException("Invalid map dimensions.");
        if(baseMoisture.Any(v=>!double.IsFinite(v)||v<0||v>1))throw new ArgumentException("Invalid moisture.");
        Size=size;this.land=(bool[])land.Clone();moisture=(double[])baseMoisture.Clone();soil=new double[land.Length];
        Settings=settings??new();Settings.Validate();random=seed==0?0x9e3779b97f4a7c15UL:seed;
    }
    double Next()
    {
        // Fully persisted independent stream; never consumes the animal RNG.
        random^=random>>12;random^=random<<25;random^=random>>27;
        return ((unchecked(random*2685821657736338717UL))>>11)*(1.0/(1UL<<53));
    }
    public long AddFounder(int cell,double age=0)
    {
        if(nextId==long.MaxValue)throw new InvalidOperationException("Tree identity limit reached.");
        if(cell<0||cell>=land.Length||!land[cell]||treeCells.Contains(cell)||!double.IsFinite(age)||age<0)
            throw new ArgumentException("Invalid founder site or age.");
        long id=nextId++;trees.Add(new(id,cell,age,Settings.MaturityDays,1,.85+Next()*.3));IndexTree(trees.Count-1);return id;
    }
    public bool PlantSeed(int cell,int viabilityDays=365)
    {
        if(cell<0||cell>=land.Length||!land[cell]||viabilityDays<=0)return false;
        seeds[cell]=Math.Max(seeds.GetValueOrDefault(cell),checked(Day+viabilityDays));return true;
    }
    public double DepositDecay(int cell,double decayedMass)
    {
        if(!double.IsFinite(decayedMass)||decayedMass<0)throw new ArgumentOutOfRangeException(nameof(decayedMass));
        if(cell<0||cell>=land.Length||!land[cell])return 0;
        // Nutrients saturate locally. No tree is directly produced by a deposit.
        double contribution=Math.Min(1-soil[cell],decayedMass*.01*Settings.CorpseContribution);
        soil[cell]+=contribution;return contribution;
    }
    double Fertility(int cell)=>Math.Clamp(.25+moisture[cell]*.5+soil[cell]*.5,0,1);
    double Competition(int cell)
    {
        double radius=2+4*(1-Settings.Density),pressure=0;
        foreach(int index in Nearby(cell,radius))
        {
            var tree=trees[index];
            if(tree.DeadDay>=0||tree.Cell==cell)continue;
            double dx=cell%Size-tree.Cell%Size,dy=cell/Size-tree.Cell/Size;
            pressure+=Math.Max(0,1-(dx*dx+dy*dy)/(radius*radius));
        }
        return 1/(1+pressure);
    }
    public double FruitFactor(ForestTree tree)=>tree.DeadDay>=0||tree.Growth<Settings.MaturityDays
        ?0:tree.Health*Fertility(tree.Cell)*Competition(tree.Cell);
    public void StepDay(double waterAccess,Func<int,bool>? occupied=null)
    {
        if(!double.IsFinite(waterAccess)||waterAccess<0||waterAccess>1)throw new ArgumentOutOfRangeException(nameof(waterAccess));
        if(Day==long.MaxValue||nextId>long.MaxValue-land.Length)throw new InvalidOperationException("Forest counter limit reached.");
        Day++;
        double retention=Settings.SoilHalfLifeDays==0?0:Math.Pow(.5,1/Settings.SoilHalfLifeDays);
        for(int i=0;i<soil.Length;i++)soil[i]*=retention;
        for(int i=0;i<trees.Count;i++)
        {
            var tree=trees[i];if(tree.DeadDay>=0)continue;
            double wet=waterAccess*(.35+.65*moisture[tree.Cell]);
            double threshold=.08+.30*(1-Settings.DroughtTolerance);
            double deficit=Math.Max(0,(threshold-wet)/threshold);
            double health=Math.Clamp(tree.Health-deficit/90+(deficit==0?.002*wet:0),0,1);
            double growth=tree.Growth+health*Math.Min(1,wet/.35)*Fertility(tree.Cell)*Competition(tree.Cell);
            double age=tree.Age+1;
            bool dead=health<=0||(Settings.LifespanDays>0&&age>=Settings.LifespanDays*tree.LifespanMultiplier);
            trees[i]=tree with{Age=age,Growth=growth,Health=health,DeadDay=dead?Day:-1};
            if(dead)continue;
            if(tree.Growth<Settings.MaturityDays)soil[tree.Cell]=Math.Max(0,soil[tree.Cell]-.001*health);
            if(growth>=Settings.MaturityDays&&Next()<.08*Settings.SeedSpread*health)
            {
                int x=tree.Cell%Size+(int)(Next()*25)-12,y=tree.Cell/Size+(int)(Next()*25)-12;
                if(x>=0&&y>=0&&x<Size&&y<Size)PlantSeed(y*Size+x);
            }
        }
        // Dead trunks persist thirty days; IDs are never reused after removal.
        if(trees.RemoveAll(t=>t.DeadDay>=0&&Day-t.DeadDay>=30)>0)RebuildIndex();
        foreach(var seed in seeds.ToArray())
        {
            if(seed.Value<=Day){seeds.Remove(seed.Key);continue;}
            int cell=seed.Key;double wet=waterAccess*(.35+.65*moisture[cell]);
            double spacing=1+2*(1-Settings.Density);
            bool crowded=Crowded(cell,spacing);
            if(wet<.22||crowded||occupied?.Invoke(cell)==true)continue;
            double chance=.025*Settings.Density*Fertility(cell)*Competition(cell);
            if(Next()>=chance)continue;
            trees.Add(new(nextId++,cell,0,0,1,.85+Next()*.3));IndexTree(trees.Count-1);soil[cell]=Math.Max(0,soil[cell]-.1);seeds.Remove(cell);
        }
    }
    public byte[] Save()=>JsonSerializer.SerializeToUtf8Bytes(new ForestSnapshot(Format,Size,Settings,land,moisture,soil,Day,nextId,random,
        trees.ToArray(),seeds.Select(s=>new ForestSeed(s.Key,s.Value)).ToArray()));
    public string StateHash()=>Convert.ToHexString(SHA256.HashData(Save()));
    public static Forest Load(byte[] data)
    {
        if(data.Length>MaximumBytes)throw new InvalidDataException("Forest snapshot too large.");
        var s=JsonSerializer.Deserialize<ForestSnapshot>(data)??throw new InvalidDataException("Empty snapshot.");
        if(s.Format!=Format||s.Settings==null||s.Land==null||s.BaseMoisture==null||s.Soil==null||s.Trees==null||s.Seeds==null)
            throw new InvalidDataException("Unsupported forest snapshot.");
        var forest=new Forest(s.Size,s.Land,s.BaseMoisture,s.RandomState,s.Settings);
        bool Unit(double value)=>double.IsFinite(value)&&value>=0&&value<=1;
        if(s.RandomState==0||s.Day<0||s.NextId<1||s.Soil.Length!=s.Land.Length||s.Soil.Any(v=>!Unit(v))||s.Trees.Length>s.Land.Length||s.Seeds.Length>s.Land.Length)
            throw new InvalidDataException("Invalid forest state.");
        var ids=new HashSet<long>();var cells=new HashSet<int>();
        foreach(var tree in s.Trees)
        {
            if(tree==null||tree.Id<1||tree.Id>=s.NextId||!ids.Add(tree.Id)||tree.Cell<0||tree.Cell>=s.Land.Length||!s.Land[tree.Cell]||!cells.Add(tree.Cell)
                ||!Unit(tree.Health)||!double.IsFinite(tree.Age)||tree.Age<0||!double.IsFinite(tree.Growth)||tree.Growth<0
                ||!double.IsFinite(tree.LifespanMultiplier)||tree.LifespanMultiplier<.85||tree.LifespanMultiplier>1.15||tree.DeadDay< -1||tree.DeadDay>s.Day)
                throw new InvalidDataException("Invalid tree state.");
        }
        foreach(var seed in s.Seeds)
            if(seed==null||seed.Cell<0||seed.Cell>=s.Land.Length||!s.Land[seed.Cell]||seed.Expires<=s.Day||!forest.seeds.TryAdd(seed.Cell,seed.Expires))
                throw new InvalidDataException("Invalid seed state.");
        forest.Day=s.Day;forest.nextId=s.NextId;forest.random=s.RandomState;
        Array.Copy(s.Soil,forest.soil,s.Soil.Length);forest.trees.AddRange(s.Trees);forest.RebuildIndex();return forest;
    }
}
