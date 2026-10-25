using EvoSim.Core;
using System.Text.Json;
using System.Diagnostics;
var rows=new List<object>();
foreach(var entry in new[]{(128,70,8),(256,450,50)}) {
 var w=new World(42,new WorldConfig{MapSize=entry.Item1,Herbivores=entry.Item2,Carnivores=entry.Item3,TreeCount=300,LandRegions=3,RockClusters=9});
 var clock=Stopwatch.StartNew();
 for(int tick=0;tick<2000;tick++){if(tick==1600)w.SetDrought(true);w.Step();}
 clock.Stop();rows.Add(new{size=entry.Item1,founders=entry.Item2+entry.Item3,ms=clock.Elapsed.TotalMilliseconds,hash=w.StateHash(),living=w.Living,births=w.Births,deaths=w.Deaths,trees=w.Map.Trees.Count});
}
Console.WriteLine(JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
