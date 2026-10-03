using EvoSim.Core;

namespace EvoSim.Observer;

internal sealed record LineageFamily(int Id,IReadOnlyList<int> Parents,IReadOnlyList<int> Children,IReadOnlyList<int> Grandchildren,IReadOnlyList<int> Descendants);
internal readonly record struct LineageCounts(int Children,int Grandchildren,int Descendants,int LivingDescendants,long ObservedTick);

// A view of the supplied world's recorded ancestry. No stepping, RNG or model writes.
// Parent IDs are immutable in Core; births append records, deaths keep those records.
internal sealed class LineageIndex
{
    World? source;int creatureCount=-1;
    readonly Dictionary<int,Creature> individuals=new();
    readonly Dictionary<int,List<int>> children=new();
    readonly Dictionary<int,LineageFamily> families=new();
    readonly Dictionary<int,LineageCounts> counts=new();
    readonly Queue<int> familyOrder=new();
    internal int TopologyRevision { get; private set; }
    internal World? Source=>source;
    internal void Refresh(World world,bool forceLiving=false)
    {
        ArgumentNullException.ThrowIfNull(world);
        if(!ReferenceEquals(source,world)||creatureCount!=world.Creatures.Count)
        {
            source=world;creatureCount=world.Creatures.Count;individuals.Clear();children.Clear();families.Clear();counts.Clear();familyOrder.Clear();
            foreach(var creature in world.Creatures)individuals.Add(creature.Id,creature);
            foreach(var creature in world.Creatures)
            {
                AddParent(creature.ParentA,creature.Id);
                if(creature.ParentB!=creature.ParentA)AddParent(creature.ParentB,creature.Id);
            }
            foreach(var list in children.Values)list.Sort();
            TopologyRevision++;
        }
        if(forceLiving)counts.Clear();
    }
    void AddParent(int parent,int child)
    {
        if(parent<=0||parent==child||!individuals.ContainsKey(parent))return;
        if(!children.TryGetValue(parent,out var list))children.Add(parent,list=new());
        list.Add(child);
    }
    internal Creature? Find(int id)=>individuals.GetValueOrDefault(id);
    internal LineageFamily? Family(int id)
    {
        if(!individuals.TryGetValue(id,out var person))return null;
        if(families.TryGetValue(id,out var cached))return cached;
        int[] parents=new[]{person.ParentA,person.ParentB}.Where(p=>p>0&&p!=id&&individuals.ContainsKey(p)).Distinct().Order().ToArray();
        int[] direct=children.TryGetValue(id,out var next)?next.ToArray():Array.Empty<int>();
        // Exact depth two: a person may be both a child and grandchild in an unusual
        // pedigree, but is counted only once within either displayed relationship.
        var grandchildren=new HashSet<int>();
        foreach(int child in direct)if(children.TryGetValue(child,out var second))foreach(int grandchild in second)if(grandchild!=id)grandchildren.Add(grandchild);
        var descendants=new HashSet<int>();var pending=new Stack<int>(direct);
        while(pending.TryPop(out int child))
        {
            if(child==id||!descendants.Add(child))continue;
            if(children.TryGetValue(child,out var more))foreach(int descendant in more)pending.Push(descendant);
        }
        var family=new LineageFamily(id,Array.AsReadOnly(parents),Array.AsReadOnly(direct),Array.AsReadOnly(grandchildren.Order().ToArray()),Array.AsReadOnly(descendants.Order().ToArray()));
        // Keep the context panel and a few recently visited relatives cheap without
        // retaining a full transitive-closure matrix for a large population.
        while(familyOrder.Count>=8){int old=familyOrder.Dequeue();families.Remove(old);counts.Remove(old);}
        familyOrder.Enqueue(id);families.Add(id,family);return family;
    }
    internal LineageCounts Counts(int id)
    {
        var family=Family(id);if(family==null)return default;
        long tick=source!.Tick;
        if(counts.TryGetValue(id,out var cached)&&tick>=cached.ObservedTick&&tick-cached.ObservedTick<20)return cached;
        int living=0;foreach(int child in family.Descendants)if(individuals[child].Alive)living++;
        var result=new LineageCounts(family.Children.Count,family.Grandchildren.Count,family.Descendants.Count,living,tick);counts[id]=result;return result;
    }
}
