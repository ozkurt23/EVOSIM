using System;
using System.Collections.Generic;
using System.Diagnostics;
using EvoSim.Core;
using UnityEngine;

// Core is the sole authority. No Rigidbody, steering, hunting or fleeing logic here.
public sealed class EvoSimBridge : MonoBehaviour
{
    public int seed=42;
    public GameObject herbivorePrefab,carnivorePrefab,treePrefab,rockPrefab;
    public bool paused;
    [Range(1,20)]public int speed=1;
    public World World{get;private set;}
    readonly Dictionary<int,GameObject> creatures=new Dictionary<int,GameObject>();
    readonly List<GameObject> props=new List<GameObject>();
    readonly SimulationClock clock=new SimulationClock();
    public int ViewCount=>creatures.Count;
    public static WorldConfig DemoConfig()=>new WorldConfig{Herbivores=35,Carnivores=5,TreeCount=80};
    void Start(){World=new World(seed,DemoConfig());CreateWorldView();gameObject.AddComponent<CoreResourcesView>().Initialize(this);SyncViews();}
    void Update(){
        if(World==null)return;
        clock.Advance(World,Time.unscaledDeltaTime,speed,paused);SyncViews();
    }
    public void SyncViews(){
        foreach(var c in World.Creatures){
            if(!c.Alive){if(creatures.TryGetValue(c.Id,out var dead)){Destroy(dead);creatures.Remove(c.Id);}continue;}
            if(!creatures.TryGetValue(c.Id,out var go)){
                go=CreateModel(c.Genes[2]<.5f?herbivorePrefab:carnivorePrefab);go.name="Individual_"+c.Id;creatures.Add(c.Id,go);go.AddComponent<CreatureAppearance>();
            }
            go.GetComponent<CreatureAppearance>().Refresh(c,World);
            go.transform.localPosition=new Vector3(c.X,CoreTerrainView.HeightAt(World.Map,c.X,c.Y),c.Y);
            go.transform.localRotation=Quaternion.Euler(0,-c.Angle*Mathf.Rad2Deg,0);go.transform.localScale=Vector3.one*Mathf.Max(.3f,c.Radius*1.8f);
        }
    }
    GameObject CreateModel(GameObject prefab){var root=new GameObject(prefab.name);root.transform.SetParent(transform,false);Instantiate(prefab,root.transform,false);return root;}
    void CreateWorldView(){
        var terrain=new GameObject("Core terrain");terrain.transform.SetParent(transform,false);terrain.AddComponent<CoreTerrainView>().Build(World);
        var treeCells=new HashSet<int>();foreach(var t in World.Map.Trees)treeCells.Add((int)t.X+(int)t.Y*World.Map.Size);
        if(rockPrefab!=null)for(int cell=0;cell<World.Map.Tiles.Length;cell++)if(World.Map.Tiles[cell]==2&&!treeCells.Contains(cell)){
            float x=cell%World.Map.Size+.5f,y=cell/World.Map.Size+.5f;var rock=CreateModel(rockPrefab);rock.name="Core rock";rock.transform.localPosition=new Vector3(x,CoreTerrainView.HeightAt(World.Map,x,y),y);float variation=((cell*73856093u)%997)/997f;rock.transform.localRotation=Quaternion.Euler(0,variation*360,0);rock.transform.localScale=Vector3.one*(.40f+.24f*variation);
        }
        // Small shoreline stones are scenery, deliberately separate from Core rock obstacles.
        if(rockPrefab!=null)for(int y=2;y<World.Map.Size-2;y++)for(int x=2;x<World.Map.Size-2;x++){
            int cell=x+y*World.Map.Size;if(World.Map.Tiles[cell]!=0||(cell*73856093u)%37!=0)continue;
            bool coast=World.Map.Tiles[cell-1]==1||World.Map.Tiles[cell+1]==1||World.Map.Tiles[cell-World.Map.Size]==1||World.Map.Tiles[cell+World.Map.Size]==1;if(!coast)continue;
            for(int j=0;j<3;j++){
                float px=x+.27f+j*.22f,py=y+.34f+((cell+j*7)%5)*.075f;
                if(CoreTerrainView.ShoreField(World.Map,px,py)<.15f)continue;
                var pebble=CreateModel(rockPrefab);pebble.name="Shore pebble";pebble.transform.localPosition=new Vector3(px,CoreTerrainView.HeightAt(World.Map,px,py)-.025f,py);pebble.transform.localScale=Vector3.one*(.15f+.035f*j);pebble.transform.localRotation=Quaternion.Euler(0,(cell*7+j*73)%360,0);
            }
        }
        foreach(var tree in World.Map.Trees){var go=CreateModel(treePrefab);go.name="Core fruit tree";go.transform.localPosition=new Vector3(tree.X,CoreTerrainView.HeightAt(World.Map,tree.X,tree.Y),tree.Y);go.transform.localScale=Vector3.one*tree.Radius*.85f;props.Add(go);go.AddComponent<CanopyVisibility>().Initialize(this);}
    }
    // Intentionally no importer of user LIBRARY-2 saves yet: preserve those untouched.
    public string StateHash()=>World?.StateHash();
}
