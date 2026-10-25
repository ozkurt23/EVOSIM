extern alias V1;
using EvoSim.Core;
using System.Text.Json;
var checks=new Dictionary<string,bool>();
byte[] Save(World world){using var stream=new MemoryStream();world.Save(stream);return stream.ToArray();}
World Load(byte[] bytes)=>World.Load(new MemoryStream(bytes,false));
bool Reject(Action action){try{action();return false;}catch{return true;}}
var old=new V1::EvoSim.Core.World(42,new V1::EvoSim.Core.WorldConfig{Herbivores=8,Carnivores=2,TreeCount=12});
for(int i=0;i<50;i++)old.Step();
string originalHash=old.StateHash();using var oldBytes=new MemoryStream();old.Save(oldBytes);
checks["implicit_upgrade_rejected"]=Reject(()=>Load(oldBytes.ToArray()));
var imported=World.ImportV1Origin(new MemoryStream(oldBytes.ToArray(),false));
checks["explicit_import_keeps_tick_and_population"]=imported.Tick==old.Tick&&imported.Living==old.Living;
checks["import_creates_forest_founders"]=imported.Forest.Trees.Count==old.Map.Trees.Count;
checks["v1_source_state_unchanged"]=old.StateHash()==originalHash;
byte[] checkpoint=Save(imported);var loaded=Load(checkpoint);
checks["composite_roundtrip_exact"]=loaded.StateHash()==imported.StateHash();
checks["v1_rejects_v2_checkpoint"]=Reject(()=>V1::EvoSim.Core.World.Load(new MemoryStream(checkpoint,false)));
for(int i=0;i<500;i++)
{
 if(i==50){imported.SetDrought(true);loaded.SetDrought(true);}
 if(i==200){imported.SetDrought(false);loaded.SetDrought(false);}
 imported.Step();loaded.Step();if(i%100==0)loaded=Load(Save(loaded));
}
checks["composite_future_replay_exact"]=imported.StateHash()==loaded.StateHash();
var corrupt=(byte[])checkpoint.Clone();corrupt[^10]^=1;
checks["corrupted_forest_envelope_rejected"]=Reject(()=>Load(corrupt));
checks["truncated_envelope_rejected"]=Reject(()=>Load(checkpoint[..^8]));
World Fixture(float lifetime=90)
{
 var w=new World(73,new WorldConfig{Herbivores=1,Carnivores=0,TreeCount=0,CorpseDecay=1,CorpseLifetime=lifetime});
 w.Creatures[0].Alive=false;return w;
}
int Land(World w)=>Array.FindIndex(w.Map.Tiles,t=>t==0);
void CorpseAt(World w,int cell,float food)=>w.Corpses.Add(new Corpse{CreatureId=w.Creatures[0].Id,CreatedTick=w.Tick,X=cell%w.Map.Size+.5f,Y=cell/w.Map.Size+.5f,Food=food});
var decay=Fixture();int land=Land(decay);CorpseAt(decay,land,10);decay.Step();
checks["actual_decay_deposits_only_lost_mass"]=Math.Abs(decay.Forest.SoilAt(land)-(10-decay.Corpses[0].Food)*.004)<1e-10;
var eaten=Fixture();land=Land(eaten);CorpseAt(eaten,land,0);eaten.Step();
checks["consumed_corpse_no_double_nutrients"]=eaten.Forest.SoilAt(land)==0&&eaten.Corpses.Count==0;
var expiry=Fixture(5);expiry.Tick=99;land=Land(expiry);CorpseAt(expiry,land,10);expiry.Corpses[0].CreatedTick=0;expiry.Step();double deposited=expiry.Forest.SoilAt(land);expiry.Step();
checks["expiry_residual_once"]=Math.Abs(deposited-.04)<1e-10&&expiry.Corpses.Count==0&&expiry.Forest.SoilAt(land)==deposited;
var water=Fixture();int river=Array.FindIndex(water.Map.Tiles,t=>t==1);CorpseAt(water,river,10);water.Step();
checks["river_corpse_no_land_deposit"]=water.Forest.SoilAt(river)==0;
var fresh=new World(42,new WorldConfig{Herbivores=1,Carnivores=0,TreeCount=1});
for(int i=0;i<40;i++)fresh.Step();checks["daily_tick_cadence"]=fresh.Forest.Day==2;
fresh.Forest.StepDay(1);checks["desynchronized_save_rejected"]=Reject(()=>Save(fresh));
var custom=new World(42,new WorldConfig{Herbivores=1,Carnivores=0,TreeCount=1},new EvoSim.Ecology.ForestSettings(LifespanDays:0,SeedSpread:0));
checks["forest_settings_roundtrip"]=Load(Save(custom)).Forest.Settings==custom.Forest.Settings;
var shortForest=new World(42,new WorldConfig{Herbivores=1,Carnivores=0,TreeCount=1},new EvoSim.Ecology.ForestSettings(LifespanDays:2,SeedSpread:0));
int trunk=shortForest.Map.Cell(shortForest.Map.Trees[0].X,shortForest.Map.Trees[0].Y);
var rocks=Enumerable.Range(0,shortForest.Map.CellCount).Where(c=>shortForest.Map.Tiles[c]==2&&c!=trunk).ToArray();
for(int i=0;i<60;i++)shortForest.Step();
checks["dead_tree_has_no_actual_fruit"]=shortForest.Map.Trees[0].Fruit.All(f=>f==0)&&shortForest.Forest.Trees[0].DeadDay>=0;
for(int i=0;i<640;i++)shortForest.Step();
checks["expired_tree_opens_ground"]=shortForest.Map.Trees.Count==0&&shortForest.Map.Tiles[trunk]==0;
checks["permanent_rocks_preserved"]=rocks.All(c=>shortForest.Map.Tiles[c]==2);
checks["removed_tree_checkpoint_exact"]=Load(Save(shortForest)).StateHash()==shortForest.StateHash();
var growth=new World(73,new WorldConfig{Herbivores=1,Carnivores=0,TreeCount=0},new EvoSim.Ecology.ForestSettings(LifespanDays:0,MaturityDays:.5,Density:1,SeedSpread:0));
int site=Enumerable.Range(0,growth.Map.CellCount).First(c=>growth.Map.Tiles[c]==0&&c%growth.Map.Size>8&&c/growth.Map.Size>8&&Math.Abs(c%growth.Map.Size-growth.Creatures[0].X)>20);
growth.Forest.PlantSeed(site,2000);
for(int i=0;i<40000&&growth.Map.Trees.Count==0;i++)growth.Step();
checks["seed_becomes_real_tree"]=growth.Map.Trees.Count==1&&growth.Map.Tiles[site]==2;
checks["new_seedling_no_actual_fruit"]=growth.Map.Trees.Count==1&&growth.Map.Trees[0].Fruit.All(f=>f==0);
checks["growing_trunk_blocks_ground"]=!growth.Map.CanOccupy(site%growth.Map.Size+.5f,site/growth.Map.Size+.5f,.1f,0);
long identity=growth.TreeIdentity(0);growth.Creatures[0].Alive=false;
for(int i=0;i<200;i++)growth.Step();
checks["mature_tree_real_fruit"]=growth.Map.Trees[0].Fruit.Any(f=>f>0);
growth.Creatures[0].Path.Add(new System.Numerics.Vector2(10.5f,10.5f));
var growthLoaded=Load(Save(growth));
checks["stable_tree_identity_and_saved_path"]=growthLoaded.TreeIdentity(0)==identity&&growthLoaded.StateHash()==growth.StateHash();
for(int i=0;i<100;i++){growth.Step();growthLoaded.Step();}
checks["grown_tree_future_exact"]=growthLoaded.StateHash()==growth.StateHash();
checks["forest_lifecycle_journal"]=shortForest.Events.Any(e=>e.Kind=="forest"&&e.Text.Contains("1 ağaç öldü"))&&growth.Events.Any(e=>e.Kind=="forest"&&e.Text.Contains("1 yeni fidan"));
checks["full_checkpoint_replay_exact"]=Save(growth).SequenceEqual(Save(growthLoaded));
var report=new{passed=checks.Values.All(v=>v),checks,scope="Experimental v2 checkpoints, corpse decay, real fruit and dynamic tree obstacles. Godot and v2 timeline remain unintegrated."};
Console.WriteLine(JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));return report.passed?0:1;
