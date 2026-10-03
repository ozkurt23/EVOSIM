using System.Collections.Generic;
using EvoSim.Core;
using Tree=EvoSim.Core.Tree;
using UnityEngine;

// Each fruit is a view of a real Core slot; no visual-only food is invented.
public sealed class CoreResourcesView:MonoBehaviour
{
    readonly List<(Tree tree,int slot,GameObject view)> fruit=new List<(Tree,int,GameObject)>();
    readonly List<(int cell,GameObject view)> grass=new List<(int,GameObject)>();
    EvoSimBridge bridge;Mesh grassMesh;
    public int FruitViews=>fruit.Count;
    public void Initialize(EvoSimBridge owner){
        bridge=owner;var red=CoreTerrainView.MakeMaterial("Apple",new Color(.85f,.055f,.028f));
        foreach(var tree in bridge.World.Map.Trees)for(int slot=0;slot<tree.Fruit.Length;slot++){
            var p=tree.FruitPosition(slot);var apple=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(apple.GetComponent<Collider>());apple.name="Apple "+slot;apple.transform.SetParent(transform,false);
            float h=CoreTerrainView.HeightAt(bridge.World.Map,tree.X,tree.Y)+tree.Radius*(1.2f+.15f*(slot%3));
            apple.transform.localPosition=new Vector3(p.X,h,p.Y);apple.transform.localScale=Vector3.one*.36f;apple.GetComponent<Renderer>().sharedMaterial=red;fruit.Add((tree,slot,apple));
        }
        var green=CoreTerrainView.MakeMaterial("Edible grass",new Color(.21f,.39f,.10f));
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(-.22f,0,0),new Vector3(0,.38f,0),new Vector3(.08f,0,0),new Vector3(0,0,-.2f),new Vector3(.04f,.27f,0),new Vector3(0,0,.2f)};mesh.triangles=new[]{0,1,2,2,1,0,3,4,5,5,4,3};mesh.normals=new[]{Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.up,Vector3.up};grassMesh=mesh;
        for(int i=0;i<bridge.World.Map.Food.Length;i++){
            if(bridge.World.Map.Food[i]<1||i%5!=0)continue;int x=i%bridge.World.Map.Size,y=i/bridge.World.Map.Size;
            var tuft=new GameObject("Edible ground patch");tuft.transform.SetParent(transform,false);tuft.transform.localPosition=new Vector3(x+.5f,CoreTerrainView.HeightAt(bridge.World.Map,x+.5f,y+.5f),y+.5f);
            tuft.AddComponent<MeshFilter>().sharedMesh=mesh;tuft.AddComponent<MeshRenderer>().sharedMaterial=green;grass.Add((i,tuft));
        }
        Sync();
    }
    void LateUpdate()=>Sync();
    public void Sync(){foreach(var f in fruit)f.view.SetActive(f.tree.Fruit[f.slot]>.01f);if(bridge!=null)foreach(var g in grass)g.view.SetActive(bridge.World.Map.Food[g.cell]>.1f);}
    void OnDestroy(){if(grassMesh!=null)Destroy(grassMesh);}
}
