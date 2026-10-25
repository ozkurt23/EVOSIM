using EvoSim.Core;
using EvoSim.Ecology;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
var checks=new Dictionary<string,bool>();int queries=0;
bool Compare(Terrain map) {
 var flags=BindingFlags.Instance|BindingFlags.NonPublic;
 var shores=(List<Vector2>)typeof(Terrain).GetField("_shores",flags)!.GetValue(map)!;
 var components=(int[])typeof(Terrain).GetField("_landComponent",flags)!.GetValue(map)!;
 Vector2 Brute(float x,float y,float range) {
  var missing=new Vector2(float.NaN,float.NaN);int cell=map.Cell(x,y);
  if(cell<0||!float.IsFinite(range)||range<0)return missing;
  int component=components[cell];if(component==0&&map.Tiles[cell]!=1)return missing;
  float best=range*range;var result=missing;
  foreach(var p in shores) {if(component!=0&&components[map.Cell(p.X,p.Y)]!=component)continue;float dx=p.X-x,dy=p.Y-y,d=dx*dx+dy*dy;if(d<=best){best=d;result=p;}}
  return result;
 }
 for(int i=0;i<10000;i++) {
  float x=(i*7%map.Size)+.5f,y=(i*31%map.Size)+.5f,range=new[]{0f,1.5f,3f,8f,25f,999f,float.NaN,-1f}[i%8];
  var a=Brute(x,y,range);var b=map.NearestShore(x,y,range);queries++;
  if(!(float.IsNaN(a.X)&&float.IsNaN(b.X))&&a!=b)return false;
 }
 return true;
}
foreach(int size in new[]{128,256}) {
 var w=new World(42,new WorldConfig{MapSize=size,Herbivores=1,Carnivores=0,TreeCount=100},new ForestSettings(LifespanDays:3,SeedSpread:0));
 checks[$"initial_{size}"]=Compare(w.Map);
 for(int i=0;i<800;i++)w.Step();
 checks[$"removed_trunks_{size}"]=w.Map.Trees.Count==0&&Compare(w.Map);
 using var stream=new MemoryStream();w.Save(stream);stream.Position=0;var restored=World.Load(stream);
 checks[$"restored_{size}"]=Compare(restored.Map)&&w.StateHash()==restored.StateHash();
}
Console.WriteLine(JsonSerializer.Serialize(new{passed=checks.Values.All(v=>v),queries,checks},new JsonSerializerOptions{WriteIndented=true}));
return checks.Values.All(v=>v)?0:1;
