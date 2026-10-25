using EvoSim.Core;
using EvoSim.Ecology;
using System.Text.Json;
using System.Diagnostics;
var timer=Stopwatch.StartNew();var rows=new List<object>();
// A real generated landscape, without animal stepping, isolates long forest turnover.
var source=new World(42,new WorldConfig{TreeCount=300,Herbivores=1,Carnivores=0});
var f=Forest.Load(source.Forest.Save());int peak=f.Trees.Count,min=f.Trees.Count;long births=0,lastId=f.Trees.Max(t=>t.Id);
for(int day=0;day<36500;day++) {
 if(day%120==0)f.DepositDecay((day*31)%(128*128),20);
 f.StepDay(day%240>=80&&day%240<160?.16:1);
 peak=Math.Max(peak,f.Trees.Count);min=Math.Min(min,f.Trees.Count);
 long id=f.Trees.Count==0?lastId:f.Trees.Max(t=>t.Id);births+=Math.Max(0,id-lastId);lastId=Math.Max(id,lastId);
 if(day%3650==3649){rows.Add(new{year=(day+1)/365,trees=f.Trees.Count,living=f.Trees.Count(t=>t.DeadDay<0),seeds=f.SeedCount});Console.Error.WriteLine($"Forest year {(day+1)/365}: {f.Trees.Count}");}
 if(day%365==0)f=Forest.Load(f.Save());
}
var check=new{bounded=peak<=128*128,renewal=births>0,finiteSoil=Enumerable.Range(0,128*128).All(i=>double.IsFinite(f.SoilAt(i))&&f.SoilAt(i)<=1),roundtrip=f.StateHash()==Forest.Load(f.Save()).StateHash()};
Console.WriteLine(JsonSerializer.Serialize(new{scope="100 years, seed42 generated 128x128 terrain; isolated forest, no animal population claim",checks=check,peak,min,births,rows,hash=f.StateHash(),milliseconds=timer.Elapsed.TotalMilliseconds},new JsonSerializerOptions{WriteIndented=true}));
