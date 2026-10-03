using System;
using UnityEngine;

// A small observation-only HUD: controls never send steering commands to creatures.
public sealed class ObserverControls : MonoBehaviour
{
    EvoSimBridge bridge;Camera cam;Vector3 lastMouse;int selected=-1;
    readonly int[] speeds={1,2,5,10,20};
    GUIStyle label,button,small;
    void Start(){bridge=FindFirstObjectByType<EvoSimBridge>();cam=GetComponent<Camera>();}
    bool OverUi()=>Input.mousePosition.y>Screen.height-76||selected>=0&&Input.mousePosition.x>Screen.width-290&&Input.mousePosition.y>Screen.height-250;
    void Update(){
        if(bridge==null||bridge.World==null)return;
        if(Input.GetKeyDown(KeyCode.Space))bridge.paused=!bridge.paused;
        if(Input.GetKeyDown(KeyCode.Escape))selected=-1;
        if(!OverUi())cam.orthographicSize=Mathf.Clamp(cam.orthographicSize*Mathf.Exp(-Input.mouseScrollDelta.y*.12f),7,110);
        if(Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(2))lastMouse=Input.mousePosition;
        if(Input.GetMouseButton(1)||Input.GetMouseButton(2)){
            Vector3 delta=Input.mousePosition-lastMouse;lastMouse=Input.mousePosition;
            Vector3 right=cam.transform.right;right.y=0;right.Normalize();Vector3 forward=Vector3.Cross(right,Vector3.up);
            cam.transform.position-= (right*delta.x+forward*delta.y)*cam.orthographicSize*2/Screen.height;
        }
        if(Input.GetMouseButtonDown(0)&&!OverUi()){
            float best=22*22;selected=-1;
            foreach(var c in bridge.World.Creatures){if(!c.Alive)continue;var p=cam.WorldToScreenPoint(new Vector3(c.X,CoreTerrainView.HeightAt(bridge.World.Map,c.X,c.Y)+.4f,c.Y));if(p.z<0)continue;float d=((Vector2)p-(Vector2)Input.mousePosition).sqrMagnitude;if(d<best){best=d;selected=c.Id;}}
        }
    }
    void Styles(){if(label!=null)return;label=new GUIStyle(GUI.skin.label){fontSize=17,normal={textColor=new Color(.9f,.95f,.88f)}};small=new GUIStyle(label){fontSize=13};button=new GUIStyle(GUI.skin.button){fontSize=17};}
    public static string Calendar(double days){long hours=(long)Math.Floor(days*24);return (hours/(365*24)>0?hours/(365*24)+" yıl ":"")+(hours/24%365)+" gün "+(hours%24)+" saat";}
    void OnGUI(){
        if(bridge==null||bridge.World==null)return;Styles();var world=bridge.World;
        GUI.Box(new Rect(16,16,520,54),GUIContent.none);
        GUI.Label(new Rect(29,28,275,30),"EVOSIM  ·  "+Calendar(world.Time),label);
        if(GUI.Button(new Rect(309,26,42,32),bridge.paused?"▶":"Ⅱ",button))bridge.paused=!bridge.paused;
        if(GUI.Button(new Rect(360,26,63,32),bridge.speed+"×",button)){int i=Array.IndexOf(speeds,bridge.speed);bridge.speed=speeds[(i+1)%speeds.Length];}
        GUI.Label(new Rect(435,28,95,28),world.Herbivores+" / "+world.Carnivores,small);
        GUI.Label(new Rect(24,Screen.height-32,760,24),"Tekerlek: yakınlaş  ·  Sağ sürükle: gezin  ·  Canlıya tıkla: incele  ·  Boşluk: duraklat",small);
        if(selected<0)return;var c=world.Find(selected);if(c==null)return;
        float x=Screen.width-282;GUI.Box(new Rect(x,16,266,224),GUIContent.none);
        GUI.Label(new Rect(x+15,28,225,28),(c.Genes[2]<.5f?"OTÇUL":"ETÇİL")+"  #"+c.Id,label);
        if(GUI.Button(new Rect(x+225,25,26,26),"×"))selected=-1;
        GUI.Label(new Rect(x+15,65,240,170),"Yaş: "+Calendar(c.Age)+"\nEnerji: "+Mathf.RoundToInt(c.Energy)+" / "+Mathf.RoundToInt(c.MaxEnergy)+"\nSu: %"+Mathf.RoundToInt(c.Hydration)+"\nNesil: "+c.Generation+"\n"+(c.Alive?c.Reason:"Öldü: "+c.DeathCause),small);
    }
}
