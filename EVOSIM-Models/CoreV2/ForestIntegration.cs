using EvoSim.Ecology;

namespace EvoSim.Core;

public sealed partial class World
{
    public Forest Forest{get;private set;}
    public long ForestOriginDay{get;private set;}
    private Dictionary<long,Tree> forestResources=new();
    private byte[] forestBaseTiles;
    public World(int seed,WorldConfig config,ForestSettings settings):this(seed,config)
    {
        foreach(var tree in Map.Trees){Array.Fill(tree.Fruit,Config.FruitEnergy);Array.Clear(tree.Regrow);}
        InitializeForest(settings);
    }
    private void InitializeForest(ForestSettings settings=null)
    {
        var land=Map.Tiles.Select(t=>t==0).ToArray();
        // Existing trunks are valid founder sites. Rocks remain excluded.
        foreach(var tree in Map.Trees)land[Map.Cell(tree.X,tree.Y)]=true;
        // Imported drought moisture is already attenuated; recover the seed's baseline.
        var baseline=new Terrain(Seed,Config);
        Forest=new Forest(Map.Size,land,baseline.Moisture.Select(m=>(double)Math.Clamp(m,0,1)).ToArray(),
            unchecked((ulong)(uint)Seed)^0x464f524553543031UL,settings);
        forestResources.Clear();
        foreach(var tree in Map.Trees)forestResources[Forest.AddFounder(Map.Cell(tree.X,tree.Y))]=tree;
        ForestOriginDay=Tick/20;
        BuildForestBase(baseline);SyncForestResources();
    }
    private void RestoreForest(byte[] bytes,long originDay)
    {
        var restored=Forest.Load(bytes);
        if(restored.Size!=Map.Size)throw new InvalidDataException("Orman ve biyolojik harita boyutları farklı.");
        Forest=restored;
        ForestOriginDay=originDay;ValidateForestClock();
        if(Forest.Trees.Count!=Map.Trees.Count)throw new InvalidDataException("Ağaç kaynak kimlikleri eşleşmiyor.");
        forestResources.Clear();
        for(int i=0;i<Forest.Trees.Count;i++)
        {
            if(Map.Cell(Map.Trees[i].X,Map.Trees[i].Y)!=Forest.Trees[i].Cell)
                throw new InvalidDataException("Ağaç konumları eşleşmiyor.");
            forestResources[Forest.Trees[i].Id]=Map.Trees[i];
        }
        BuildForestBase(new Terrain(Seed,Config));SyncForestResources(true);
    }
    private void BuildForestBase(Terrain baseline)
    {
        forestBaseTiles=(byte[])baseline.Tiles.Clone();
        foreach(var tree in baseline.Trees)forestBaseTiles[baseline.Cell(tree.X,tree.Y)]=0;
    }
    private void SyncForestResources(bool restoring=false)
    {
        var nextResources=new Dictionary<long,Tree>();var nextTrees=new List<Tree>();
        var yields=new Dictionary<Tree,float>();var tiles=(byte[])forestBaseTiles.Clone();
        foreach(var state in Forest.Trees)
        {
            if(!forestResources.TryGetValue(state.Id,out var tree))
            {
                float radius=Config.TreeRadiusMin+(Config.TreeRadiusMax-Config.TreeRadiusMin)*(float)(((ulong)state.Id*2654435761UL%1000)/1000d);
                tree=new Tree(state.Cell%Map.Size+.5f,state.Cell/Map.Size+.5f,radius,Config.FruitSlots,0,Config.TreeFruitRadius);
            }
            nextResources.Add(state.Id,tree);nextTrees.Add(tree);
            float factor=(float)Forest.FruitFactor(state);yields.Add(tree,factor);
            for(int slot=0;slot<tree.Fruit.Length;slot++)tree.Fruit[slot]=Math.Min(tree.Fruit[slot],Config.FruitEnergy*factor);
            tiles[state.Cell]=2;
            Map.Food[state.Cell]=0;
        }
        bool changed=!tiles.SequenceEqual(Map.Tiles);
        forestResources=nextResources;Map.Trees=nextTrees;Map.SetForestYields(yields);
        if(changed)
        {
            Map.Tiles=tiles;Map.RefreshDynamicObstacles();
            if(!restoring)foreach(var creature in Creatures){creature.Path.Clear();creature.PathIndex=0;}
        }
    }
    private void StepForest()
    {
        var alive=Forest.Trees.Where(t=>t.DeadDay<0).Select(t=>t.Id).ToHashSet();
        Forest.StepDay(Map.WaterLevel,ForestCellOccupied);
        var now=Forest.Trees.Where(t=>t.DeadDay<0).Select(t=>t.Id).ToHashSet();
        int births=now.Except(alive).Count(),deaths=alive.Except(now).Count();
        if(births>0||deaths>0)Record("forest",births+" yeni fidan · "+deaths+" ağaç öldü");
        SyncForestResources();EnsureResourceIndex();
    }
    private void RefreshForestYields()
    {
        var yields=new Dictionary<Tree,float>();
        foreach(var state in Forest.Trees)if(forestResources.TryGetValue(state.Id,out var tree))yields[tree]=(float)Forest.FruitFactor(state);
        Map.SetForestYields(yields);
    }
    public long TreeIdentity(int index)=>Forest.Trees[index].Id;
    private void ValidateForestClock()
    {
        if(Forest==null||ForestOriginDay<0||ForestOriginDay>Tick/20||Forest.Day!=Tick/20-ForestOriginDay)
            throw new InvalidDataException("Orman ve dünya zamanı eşleşmiyor.");
    }
    private bool ForestCellOccupied(int cell)
    {
        float x=cell%Map.Size+.5f,y=cell/Map.Size+.5f;
        // The new tree must not appear inside a living creature or permanent obstacle.
        if(Map.Tiles[cell]!=0)return true;
        return Creatures.Any(c=>c.Alive && Math.Abs(c.X-x)<c.Radius+.5f && Math.Abs(c.Y-y)<c.Radius+.5f);
    }
}
