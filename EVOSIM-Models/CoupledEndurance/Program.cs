using EvoSim.Core;
using System.Text.Json;
using System.Diagnostics;
var results=new List<object>();bool passed=true;
foreach(int seed in new[]{42,123,8675309}) {
 var w=new World(seed,new WorldConfig{MapSize=128,Herbivores=70,Carnivores=8,TreeCount=300});
 var clock=Stopwatch.StartNew();var years=new List<object>();int peakAnimals=w.Living,peakTrees=w.Map.Trees.Count;bool roundtrips=true;long extinct=-1;
 for(int day=0;day<3650;day++) {
  if(day%240==80)w.SetDrought(true);if(day%240==160)w.SetDrought(false);
  for(int tick=0;tick<20;tick++)w.Step();
  peakAnimals=Math.Max(peakAnimals,w.Living);peakTrees=Math.Max(peakTrees,w.Map.Trees.Count);
  if(w.Living==0&&extinct<0)extinct=day+1;
  if((day+1)%365==0) {
   using var data=new MemoryStream();w.Save(data);data.Position=0;var restored=World.Load(data);
   roundtrips &= w.StateHash()==restored.StateHash();w=restored;
   years.Add(new{year=(day+1)/365,animals=w.Living,herbivores=w.Herbivores,carnivores=w.Carnivores,trees=w.Map.Trees.Count,births=w.Births,deaths=w.Deaths});
  }
 }
 // Resume both paths, including the forest and every RNG state, across another dry boundary.
 using var saved=new MemoryStream();w.Save(saved);saved.Position=0;var replay=World.Load(saved);
 w.SetDrought(true);replay.SetDrought(true);for(int t=0;t<100;t++){w.Step();replay.Step();}
 bool deterministic=w.StateHash()==replay.StateHash();passed &= roundtrips&&deterministic;
 results.Add(new{seed,years,peakAnimals,peakTrees,extinctionDay=extinct,roundtrips,deterministic,ms=clock.Elapsed.TotalMilliseconds});
 Console.Error.WriteLine($"Seed {seed}: {w.Living} animals, {w.Map.Trees.Count} trees, extinction day {extinct}");
}
Console.WriteLine(JsonSerializer.Serialize(new{passed,simulatedYears=30,results},new JsonSerializerOptions{WriteIndented=true}));
return passed?0:1;
