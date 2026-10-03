using System.Collections.Generic;
using EvoSim.Core;
using UnityEngine;

// Rendering heights only. The Core owns traversability and all drinking rules.
public sealed class CoreTerrainView : MonoBehaviour
{
    public const float LandHeight=.65f, ShoreHeight=.06f, WaterHeight=-.02f;
    public const int ShoreSubdivisions=4;
    // Blend only within 0.28 cells of each old edge: preserve the river layout.
    static float RawField(EvoSim.Core.Terrain map,float x,float y){
        x-=.5f;y-=.5f;int ix=Mathf.FloorToInt(x),iy=Mathf.FloorToInt(y);
        float u=Mathf.Clamp01((x-ix-.22f)/.56f),v=Mathf.Clamp01((y-iy-.22f)/.56f);
        float Sample(int xx,int yy)=>map.Tiles[Mathf.Clamp(xx,0,map.Size-1)+Mathf.Clamp(yy,0,map.Size-1)*map.Size]==1?-1:1;
        return Mathf.Lerp(Mathf.Lerp(Sample(ix,iy),Sample(ix+1,iy),u),Mathf.Lerp(Sample(ix,iy+1),Sample(ix+1,iy+1),u),v);
    }
    public static float ShoreField(EvoSim.Core.Terrain map,float x,float y){
        float gx=x*ShoreSubdivisions,gy=y*ShoreSubdivisions;int ix=Mathf.FloorToInt(gx),iy=Mathf.FloorToInt(gy);float u=gx-ix,v=gy-iy;
        float a=RawField(map,ix/4f,iy/4f),b=RawField(map,ix/4f,(iy+1)/4f),c=RawField(map,(ix+1)/4f,(iy+1)/4f),d=RawField(map,(ix+1)/4f,iy/4f);
        return v>=u?a+(b-a)*v+(c-b)*u:a+(c-d)*v+(d-a)*u;
    }
    public static float HeightAt(EvoSim.Core.Terrain map,float x,float y){float f=ShoreField(map,x,y);return f<-.00001f?WaterHeight:ShoreHeight+(LandHeight-ShoreHeight)*Mathf.Max(0,f);}
    public static float VertexHeight(EvoSim.Core.Terrain map,int x,int y)=>HeightAt(map,x,y);
    public void Build(World world){
        int size=world.Map.Size;var map=world.Map;
        for(int cy=0;cy<size;cy+=16)for(int cx=0;cx<size;cx+=16){
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var triangles=new[]{new List<int>(),new List<int>(),new List<int>()};
            void Face(Vector3 a,Vector3 b,Vector3 c,int mat){
                int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles[mat].AddRange(new[]{n,n+1,n+2});
                Vector3 normal=Vector3.Cross(b-a,c-a).normalized;
                foreach(var p in new[]{a,b,c}){
                    if(mat==0){const float e=.05f;float dx=(RawField(map,p.x+e,p.z)-RawField(map,p.x-e,p.z))*(LandHeight-ShoreHeight)/(2*e),dz=(RawField(map,p.x,p.z+e)-RawField(map,p.x,p.z-e))*(LandHeight-ShoreHeight)/(2*e);normals.Add(new Vector3(-dx,1,-dz).normalized);}else normals.Add(normal);
                }
            }
            void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,int mat){Face(a,b,c,mat);Face(a,c,d,mat);}
            Vector3 Point(float x,float y){float f=RawField(map,x,y);return new Vector3(x,ShoreHeight+(LandHeight-ShoreHeight)*f,y);}
            void LandTriangle(Vector3 a,Vector3 b,Vector3 c){
                var input=new[]{a,b,c};var polygon=new List<Vector3>(4);var crossings=new List<Vector3>(2);
                for(int k=0;k<3;k++){
                    var p=input[k];var q=input[(k+1)%3];bool inside=p.y>=ShoreHeight,other=q.y>=ShoreHeight;
                    if(inside)polygon.Add(p);
                    if(inside!=other){var edge=Vector3.Lerp(p,q,(ShoreHeight-p.y)/(q.y-p.y));edge.y=ShoreHeight;polygon.Add(edge);crossings.Add(edge);}
                }
                for(int k=1;k+1<polygon.Count;k++)Face(polygon[0],polygon[k],polygon[k+1],0);
                if(crossings.Count==2){var p=crossings[0];var q=crossings[1];Quad(p,q,new Vector3(q.x,WaterHeight,q.z),new Vector3(p.x,WaterHeight,p.z),2);Quad(q,p,new Vector3(p.x,WaterHeight,p.z),new Vector3(q.x,WaterHeight,q.z),2);}
            }
            int ex=Mathf.Min(cx+16,size),ey=Mathf.Min(cy+16,size);
            Quad(new Vector3(cx,WaterHeight,cy),new Vector3(cx,WaterHeight,ey),new Vector3(ex,WaterHeight,ey),new Vector3(ex,WaterHeight,cy),1);
            for(int y=cy;y<ey;y++)for(int x=cx;x<ex;x++){
                bool nearWater=false,nearLand=false;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++){
                    bool water=map.Tiles[Mathf.Clamp(x+dx,0,size-1)+Mathf.Clamp(y+dy,0,size-1)*size]==1;nearWater|=water;nearLand|=!water;
                }
                if(!nearLand)continue;
                int subdivisions=nearWater?ShoreSubdivisions:1;float step=1f/subdivisions;
                for(int j=0;j<subdivisions;j++)for(int i=0;i<subdivisions;i++){
                    float xx=x+i*step,yy=y+j*step;var a=Point(xx,yy);var b=Point(xx,yy+step);var c=Point(xx+step,yy+step);var d=Point(xx+step,yy);
                    LandTriangle(a,b,c);LandTriangle(a,c,d);
                    void Border(Vector3 p,Vector3 q){if(p.y<ShoreHeight&&q.y<ShoreHeight)return;if(p.y<ShoreHeight)p=Vector3.Lerp(p,q,(ShoreHeight-p.y)/(q.y-p.y));if(q.y<ShoreHeight)q=Vector3.Lerp(q,p,(ShoreHeight-q.y)/(p.y-q.y));Quad(new Vector3(p.x,-.35f,p.z),p,q,new Vector3(q.x,-.35f,q.z),2);}
                    if(y==0&&j==0)Border(a,d);if(x==size-1&&i==subdivisions-1)Border(d,c);if(y==size-1&&j==subdivisions-1)Border(c,b);if(x==0&&i==0)Border(b,a);
                }
            }
            var mesh=new Mesh{name="Core chunk",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetNormals(normals);var uv=new List<Vector2>();foreach(var vertex in vertices)uv.Add(new Vector2(vertex.x*.17f,vertex.z*.17f));mesh.SetUVs(0,uv);mesh.subMeshCount=3;for(int i=0;i<3;i++)mesh.SetTriangles(triangles[i],i);mesh.RecalculateBounds();
            var chunk=new GameObject("Chunk "+cx+" "+cy);chunk.transform.SetParent(transform,false);chunk.AddComponent<MeshFilter>().sharedMesh=mesh;
            chunk.AddComponent<MeshRenderer>().sharedMaterials=new[]{MakeMaterial("Grass",new Color(.31f,.48f,.20f)),MakeMaterial("Water",new Color(.09f,.36f,.49f)),MakeMaterial("Earth",new Color(.22f,.17f,.12f))};
        }
    }
    static readonly Dictionary<string,Material> cache=new Dictionary<string,Material>();
    public static Material MakeMaterial(string name,Color color){if(cache.TryGetValue(name,out var m)&&m!=null)return m;m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,enableInstancing=true};m.SetColor("_BaseColor",color);
        if(name=="Grass"||name=="Water"){
            var texture=new Texture2D(64,64,TextureFormat.RGB24,true);texture.wrapMode=TextureWrapMode.Repeat;texture.filterMode=FilterMode.Bilinear;
            for(int y=0;y<64;y++)for(int x=0;x<64;x++){float n=Mathf.PerlinNoise(x*.10f,y*.10f);float value=name=="Grass"?.94f+.08f*n:.87f+.15f*n;texture.SetPixel(x,y,new Color(value,value,value));}
            texture.Apply();m.SetTexture("_BaseMap",texture);
        }
        m.SetFloat("_Smoothness",name=="Water"?.55f:.08f);cache[name]=m;return m;}
    void OnDestroy(){foreach(var filter in GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh!=null)Destroy(filter.sharedMesh);}
}
