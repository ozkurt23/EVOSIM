using EvoSim.Ecology;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;

var checks=new Dictionary<string,bool>();
Forest Make(ForestSettings? settings=null,ulong seed=42)=>new(8,Enumerable.Repeat(true,64).ToArray(),Enumerable.Repeat(1d,64).ToArray(),seed,settings);
var f=Make();f.AddFounder(18);f.PlantSeed(40);f.DepositDecay(40,60);
checks["roundtrip_exact"]=f.StateHash()==Forest.Load(f.Save()).StateHash();
var replay=Forest.Load(f.Save());
for(int day=0;day<500;day++)
{
 double water=day%200<80?.12:1;
 if(day%30==0){f.DepositDecay(30,8);replay.DepositDecay(30,8);}
 f.StepDay(water);replay.StepDay(water);
 if(day%100==0)replay=Forest.Load(replay.Save());
}
checks["future_replay_exact"]=f.StateHash()==replay.StateHash();
var finite=Make(new(LifespanDays:10,SeedSpread:0));finite.AddFounder(18);
for(int d=0;d<12;d++)finite.StepDay(1);
checks["old_age_death"]=finite.Trees.Single().DeadDay>=0;
checks["dead_no_fruit"]=finite.FruitFactor(finite.Trees[0])==0;
for(int d=0;d<31;d++)finite.StepDay(1);
checks["dead_trunk_expires"]=finite.Trees.Count==0;
var immortal=Make(new(LifespanDays:0,SeedSpread:0));immortal.AddFounder(18);
for(int d=0;d<150;d++)immortal.StepDay(1);
checks["immortal_in_good_water"]=immortal.Trees.Single().DeadDay<0;
for(int d=0;d<150;d++)immortal.StepDay(0);
checks["immortal_still_drought_vulnerable"]=immortal.Trees.Count==0;
for(int d=0;d<20;d++)immortal.StepDay(1);
checks["rain_no_resurrection"]=immortal.Trees.Count==0;
var soil=Make(new(SoilHalfLifeDays:10));soil.DepositDecay(0,double.MaxValue);
checks["soil_saturates"]=soil.SoilAt(0)==1&&soil.DepositDecay(0,100)==0;
for(int d=0;d<10;d++)soil.StepDay(1);
checks["half_life"]=Math.Abs(soil.SoilAt(0)-.5)<1e-12;
checks["corpse_without_seed_no_tree"]=soil.Trees.Count==0;
var map=Enumerable.Repeat(true,64).ToArray();map[0]=false;
var waterForest=new Forest(8,map,Enumerable.Repeat(1d,64).ToArray(),42);
checks["water_rejects_deposit_and_seed"]=waterForest.DepositDecay(0,20)==0&&!waterForest.PlantSeed(0);
map[1]=false;checks["constructor_owns_map"]=waterForest.PlantSeed(1);
var seedling=Make(new(MaturityDays:1000,Density:1,SeedSpread:0));seedling.PlantSeed(18,2000);
for(int d=0;d<1000&&seedling.Trees.Count==0;d++)seedling.StepDay(1);
checks["seed_can_germinate"]=seedling.Trees.Count==1;
checks["seedling_no_instant_fruit"]=seedling.Trees.Count==1&&seedling.FruitFactor(seedling.Trees[0])==0;
var blocked=Make(new(Density:1));blocked.PlantSeed(18,10);
for(int d=0;d<12;d++)blocked.StepDay(1,_=>true);
checks["occupied_cell_not_planted"]=blocked.Trees.Count==0;
checks["seed_bank_expires"]=blocked.SeedCount==0;
bool Reject(byte[] data){try{Forest.Load(data);return false;}catch{return true;}}
checks["truncated_rejected"]=Reject(f.Save()[..40]);
var document=JsonNode.Parse(f.Save())!;document["Format"]="future";
checks["unknown_version_rejected"]=Reject(JsonSerializer.SerializeToUtf8Bytes(document));
document=JsonNode.Parse(f.Save())!;document["RandomState"]=0;
checks["missing_rng_rejected"]=Reject(JsonSerializer.SerializeToUtf8Bytes(document));
document=JsonNode.Parse(f.Save())!;document["Soil"]![0]=-1;
checks["negative_soil_rejected"]=Reject(JsonSerializer.SerializeToUtf8Bytes(document));
var duplicate=Make();duplicate.AddFounder(8);duplicate.AddFounder(40);
document=JsonNode.Parse(duplicate.Save())!;document["Trees"]![1]!["Id"]=document["Trees"]![0]!["Id"]!.GetValue<long>();
checks["duplicate_tree_id_rejected"]=Reject(JsonSerializer.SerializeToUtf8Bytes(document));
bool invalidWater=false;string unchanged=f.StateHash();try{f.StepDay(double.NaN);}catch(ArgumentOutOfRangeException){invalidWater=true;}
checks["invalid_input_atomic"]=invalidWater&&unchanged==f.StateHash();
var runs=new List<object>();var clock=Stopwatch.StartNew();
foreach(ulong seed in new ulong[]{42,73,123})
{
 var forest=Make(new(LifespanDays:2920,MaturityDays:182.5,SeedSpread:.8,Density:.6),seed);
 foreach(int cell in new[]{9,14,49,54})forest.AddFounder(cell);
 int peak=forest.Trees.Count;
 for(int d=0;d<36500;d++)
 {
  if(d%90==0)forest.DepositDecay(27,40);
  forest.StepDay(d%365<45?.12:1);peak=Math.Max(peak,forest.Trees.Count);
 }
 checks["bounded_100_years_"+seed]=peak<=64&&forest.SeedCount<=64;
 bool spacing=true;double minimum=1+2*(1-forest.Settings.Density);
 for(int a=0;a<forest.Trees.Count;a++)for(int b=a+1;b<forest.Trees.Count;b++)
 {int c=forest.Trees[a].Cell,e=forest.Trees[b].Cell;if(Math.Pow(c%8-e%8,2)+Math.Pow(c/8-e/8,2)<minimum*minimum)spacing=false;}
 checks["spacing_preserved_"+seed]=spacing;
 runs.Add(new{seed,peak,remaining=forest.Trees.Count,seeds=forest.SeedCount,soil=forest.SoilAt(27),hash=forest.StateHash()});
}
var report=new{passed=checks.Values.All(v=>v),checks,centuryFixtures=runs,elapsedMs=clock.Elapsed.TotalMilliseconds,
 scope="Isolated 8x8 ecology fixtures, not integrated animal ecosystem or production-map performance."};
Console.WriteLine(JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
return report.passed?0:1;
