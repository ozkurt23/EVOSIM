using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Only leaves fade; trunks and edible fruit remain readable. No Core state changes.
public sealed class CanopyVisibility:MonoBehaviour
{
    static readonly Dictionary<Material,Material> transparent=new Dictionary<Material,Material>();
    Renderer mesh;Material[] materials;bool[] leaves;float alpha=1;float nextCheck;bool covered;
    EvoSimBridge owner;readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
    public void Initialize(EvoSimBridge bridge){
        owner=bridge;mesh=GetComponentInChildren<Renderer>();materials=mesh.sharedMaterials;leaves=new bool[materials.Length];
        for(int i=0;i<materials.Length;i++){
            var original=materials[i];leaves[i]=original.name.Contains("leaf")||original.name.Contains("pine");if(!leaves[i])continue;
            if(!transparent.TryGetValue(original,out var m)||m==null){m=new Material(original);m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);m.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.SetOverrideTag("RenderType","Transparent");m.renderQueue=3000;transparent[original]=m;}
            materials[i]=m;
        }
        mesh.sharedMaterials=materials;
    }
    void LateUpdate(){
        if(owner==null||owner.World==null||mesh==null||Camera.main==null)return;var cam=Camera.main;
        if(Time.unscaledTime>=nextCheck){
            nextCheck=Time.unscaledTime+.12f;covered=false;var b=mesh.bounds;Vector2 min=new Vector2(float.MaxValue,float.MaxValue),max=new Vector2(float.MinValue,float.MinValue);
            for(int i=0;i<8;i++){var p=cam.WorldToScreenPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
            float treeDepth=cam.WorldToScreenPoint(b.center).z;
            if(max.x>=0&&min.x<=Screen.width&&max.y>=0&&min.y<=Screen.height)foreach(var c in owner.World.Creatures){if(!c.Alive)continue;var p=cam.WorldToScreenPoint(new Vector3(c.X,CoreTerrainView.HeightAt(owner.World.Map,c.X,c.Y)+.45f,c.Y));if(p.z>0&&treeDepth<p.z+b.extents.magnitude&&p.x>=min.x-8&&p.x<=max.x+8&&p.y>=min.y-8&&p.y<=max.y+8){covered=true;break;}}
        }
        float next=Mathf.MoveTowards(alpha,covered?.18f:1,Time.unscaledDeltaTime*3);if(Mathf.Approximately(next,alpha))return;alpha=next;
        for(int i=0;i<materials.Length;i++)if(leaves[i]){block.Clear();var color=materials[i].GetColor("_BaseColor");color.a=alpha;block.SetColor("_BaseColor",color);mesh.SetPropertyBlock(block,i);}
    }
}
