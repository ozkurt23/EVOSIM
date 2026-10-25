using EvoSim.Core;
using EvoSim.Ecology;
using EvoSim.ForestArchive;
using System.Text;
using System.Text.Json;
var checks=new Dictionary<string,bool>();
var world=new World(42,new WorldConfig{Herbivores=4,Carnivores=1,TreeCount=3},new ForestSettings(LifespanDays:2,MaturityDays:.5,SeedSpread:0));
var session=ForestSession.Create(world);string origin=world.StateHash();
string early="";
for(int i=0;i<800;i++)
{
 if(i==100)session.SetDrought(true);
 if(i==200)session.SetDrought(false);
 session.Advance();if(world.Tick==20)early=world.StateHash();
}
string head=world.StateHash();var headTick=world.Tick;
checks["lifecycle_changed_world"]=world.Map.Trees.Count==0;
void Seek(ForestSession s,long tick){s.Seek(tick);while(s.Seeking)s.Pump();if(s.Error!=null)throw new Exception(s.Error);}
Seek(session,20);checks["exact_early_forest"]=session.Display.StateHash()==early&&session.Display.Map.Trees.Count==3;
checks["head_preserved_during_seek"]=world.StateHash()==head&&session.HeadTick==headTick;
var branch=session.Branch();branch.SetDrought(true);branch.Advance();
checks["branch_independent"]=session.Display.StateHash()==early&&branch.Display.Tick==21&&branch.Display.Drought;
string dir=Path.Combine(Path.GetTempPath(),"evosim-v2-archive-"+Guid.NewGuid().ToString("N"));
try
{
 string id=session.Save(dir,"V2 arşiv");var reopened=ForestSession.Open(dir,id);
 checks["save_while_preview_keeps_head"]=reopened.Display.StateHash()==head;
 Seek(reopened,0);checks["origin_reopens_exact"]=reopened.Display.StateHash()==origin;
 while(reopened.Display.Tick<reopened.HeadTick)reopened.Advance();
 checks["full_replay_exact"]=reopened.Display.StateHash()==head;
 session.ReturnToHead();session.Advance();session.Save(dir,"V2 devam",id);
 checks["existing_save_preserves_model"]=ForestSession.Open(dir,id).Display.StateHash()==session.Display.StateHash();
 byte[] archive=session.ExportTimeline();ForestSession.ValidateTimeline(archive);
 byte[] hash=Encoding.UTF8.GetBytes(ModelIdentity.Value);int offset=archive.AsSpan().IndexOf(hash);
 if(offset<0)throw new Exception("Fingerprint absent.");archive[offset]=archive[offset]==(byte)'0'?(byte)'1':(byte)'0';
 bool rejected=false;try{ForestSession.ValidateTimeline(archive);}catch(InvalidDataException){rejected=true;}
 checks["different_model_rejected"]=rejected;
 byte[] a=[1],b=[2],c=[3];string initial=ModelIdentity.Combine(a,b,c);
 checks["ecology_changes_fingerprint"]=initial!=ModelIdentity.Combine(a,[9],c);
 checks["archive_changes_fingerprint"]=initial!=ModelIdentity.Combine(a,b,[9]);
 checks["core_changes_fingerprint"]=initial!=ModelIdentity.Combine([9],b,c);
 var report=new{passed=checks.Values.All(v=>v),checks,model=ModelIdentity.Value};
 Console.WriteLine(JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));return report.passed?0:1;
}
finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
