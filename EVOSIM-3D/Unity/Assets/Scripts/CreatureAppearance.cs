using UnityEngine;
using EvoSim.Core;
public sealed class CreatureAppearance:MonoBehaviour
{
    Renderer mesh;Material[] materials;readonly MaterialPropertyBlock block=new MaterialPropertyBlock();
    void Awake(){mesh=GetComponentInChildren<Renderer>();materials=mesh.sharedMaterials;}
    public void Refresh(Creature c,World world){
        float lifespan=c.Group==0?world.Config.HerbLifespan:world.Config.CarnLifespan;
        float age=lifespan<=0?0:Mathf.Clamp01(c.Age/lifespan);
        Color baseColor=Color.Lerp(new Color(.12f,.55f,.76f),new Color(.85f,.17f,.07f),c.Genes[4]);
        Color.RGBToHSV(baseColor,out float h,out float s,out float v);var color=Color.HSVToRGB(h,s*Mathf.Lerp(1,.24f,age),v);
        for(int i=0;i<materials.Length;i++)if(materials[i].name.StartsWith("blue")||materials[i].name.StartsWith("red")){block.Clear();block.SetColor("_BaseColor",color);mesh.SetPropertyBlock(block,i);}
    }
}
