using System;
using System.Collections.Generic;

namespace EvoSim.Core
{
    public sealed partial class World
    {
        // Resource requests live only inside one interaction resolution. Corpse list
        // indices remain stable until all requests have committed, then decay removes them.
        private enum ResourceKind : byte { None, Grass, Fruit, Corpse }
        private readonly struct ResourceKey : IEquatable<ResourceKey>
        {
            public readonly ResourceKind Kind;
            public readonly int Index, Slot;
            public ResourceKey(ResourceKind kind, int index, int slot = 0) { Kind=kind;Index=index;Slot=slot; }
            public bool Equals(ResourceKey other) => Kind==other.Kind&&Index==other.Index&&Slot==other.Slot;
            public override bool Equals(object obj) => obj is ResourceKey other&&Equals(other);
            public override int GetHashCode() => unchecked(((int)Kind*397^Index)*397^Slot);
        }
        private sealed class FruitNode
        {
            public Tree Tree;
            public int TreeIndex, Slot, Order;
            public float X,Y;
        }
        private readonly struct TreeGeometry
        {
            private readonly Tree tree;
            private readonly float x,y,radius,fraction;
            private readonly int slots;
            public TreeGeometry(Tree value){tree=value;x=value.X;y=value.Y;radius=value.Radius;fraction=value.FruitRadius;slots=value.Fruit.Length;}
            public bool Matches(Tree value)=>ReferenceEquals(tree,value)&&x==value.X&&y==value.Y&&radius==value.Radius&&fraction==value.FruitRadius&&slots==value.Fruit.Length;
        }
        private Terrain _fruitMap;
        private List<FruitNode>[] _fruitBuckets;
        private readonly List<TreeGeometry> _fruitGeometry=new List<TreeGeometry>();
        private readonly List<FruitNode> _nearFruit=new List<FruitNode>();

        private void EnsureResourceIndex()
        {
            if(!ReferenceEquals(_fruitMap,Map)||_fruitGeometry.Count!=Map.Trees.Count){RebuildResourceIndex();return;}
            for(int i=0;i<Map.Trees.Count;i++)if(!_fruitGeometry[i].Matches(Map.Trees[i])){RebuildResourceIndex();return;}
        }
        private void RebuildResourceIndex()
        {
            _fruitMap=Map;_fruitGeometry.Clear();
            int columns=(Map.Size+7)/8;
            _fruitBuckets=new List<FruitNode>[columns*columns];
            for(int i=0;i<_fruitBuckets.Length;i++)_fruitBuckets[i]=new List<FruitNode>();
            int order=0;
            for(int t=0;t<Map.Trees.Count;t++){
                var tree=Map.Trees[t];_fruitGeometry.Add(new TreeGeometry(tree));
                for(int slot=0;slot<tree.Fruit.Length;slot++){
                    var position=tree.FruitPosition(slot);
                    var node=new FruitNode{Tree=tree,TreeIndex=t,Slot=slot,Order=order++,X=position.X,Y=position.Y};
                    int x=(int)Clamp(node.X/8,0,columns-1),y=(int)Clamp(node.Y/8,0,columns-1);
                    _fruitBuckets[x+y*columns].Add(node);
                }
            }
        }
        private List<FruitNode> QueryFruit(float x,float y,float radius)
        {
            _nearFruit.Clear();int columns=(Map.Size+7)/8;
            int x0=(int)Clamp((x-radius)/8,0,columns-1),x1=(int)Clamp((x+radius)/8,0,columns-1);
            int y0=(int)Clamp((y-radius)/8,0,columns-1),y1=(int)Clamp((y+radius)/8,0,columns-1);
            for(int yy=y0;yy<=y1;yy++)for(int xx=x0;xx<=x1;xx++)foreach(var node in _fruitBuckets[xx+yy*columns])
                if(DistanceSquared(x,y,node.X,node.Y)<=radius*radius)_nearFruit.Add(node);
            // Keep the original tree/slot ordering, including equal-distance tie outcomes.
            _nearFruit.Sort((a,b)=>a.Order.CompareTo(b.Order));return _nearFruit;
        }
    }
}
