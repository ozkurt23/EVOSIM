using EvoSim.Ecology;
using System.Diagnostics;
using System.Text.Json;
var runs=new List<object>();
foreach(int count in new[]{300,1200,3000}) {
 int size=128;var moisture=Enumerable.Range(0,size*size).Select(i=>.2+(i*17%79)/100d).ToArray();
 var f=new Forest(size,Enumerable.Repeat(true,size*size).ToArray(),moisture,42,new(LifespanDays:180,MaturityDays:20,SeedSpread:.5));
 for(int i=0;i<count;i++)f.AddFounder((i*7919)%(size*size),i%100);
 var timer=Stopwatch.StartNew();
 for(int d=0;d<240;d++) {if(d%7==0)f.DepositDecay(d*31,20);f.StepDay(d%100<35?.1:1);foreach(var t in f.Trees)_=f.FruitFactor(t);if(d==119)f=Forest.Load(f.Save());}
 timer.Stop();runs.Add(new{count,ms=timer.Elapsed.TotalMilliseconds,hash=f.StateHash(),trees=f.Trees.Count,seeds=f.SeedCount});
}
Console.WriteLine(JsonSerializer.Serialize(runs,new JsonSerializerOptions{WriteIndented=true}));
