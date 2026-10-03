using EvoSim.Core;
using EvoSim.Observer;
internal sealed partial class ObserverApp
{
 void LifeTraceSmoke(){
  void Check(bool v,string m){if(!v)throw new Exception("Life trace: "+m);}
  byte[] Bytes(World w){using var s=new MemoryStream();w.Save(s);return s.ToArray();}
  void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
  var original=world;var oldTimeline=timeline;var oldSlot=currentSlot;bool oldMenu=mainMenu,oldActive=hasActiveWorld,oldPaused=paused,oldDetails=details;int oldSelected=selected;
  try{
   CloseObservationPanels();world=new World(42);timeline=TimelineSession.Create(world);currentSlot=null;hasActiveWorld=true;mainMenu=setupOpen=libraryOpen=naming=false;paused=true;
   var truth=new Dictionary<long,(bool Alive,float Energy,float Water,float Health,float X,float Y)>();long birth=-1,death=-1;
   for(int i=0;i<6000;i++){AdvanceWorld();if(world.Tick==600){timeline.ExecuteDrought(true);timeline.ExecuteDrought(false);}if(world.Tick==1200)timeline.ExecuteDrought(true);if(world.Tick==2400)timeline.ExecuteDrought(false);
    var c=world.Find(166);if(c!=null){if(birth<0)birth=world.Tick;if(!c.Alive&&death<0)death=world.Tick;truth[world.Tick]=(c.Alive,c.Energy,c.Hydration,c.Health,c.X,c.Y);}}
   byte[] before=Bytes(world),archive=timeline.SerializeArchive();
   selected=166;details=true;Layout();Draw(1);Send(new Event{Type=0x401,Button=1,B=p.W-314+180,C=88+506});Check(lifeTraceOpen&&lifeTrace!=null,"actual card opens trace");Draw(1);
   Send(new Event{Type=0x300,B=100});Send(new Event{Type=0x300,B=32});Send(new Event{Type=0x300,B=118});Check(paused&&world.Tick==6000&&!world.Drought,"trace blocks intervention and playback");
   var job=lifeTrace!;
   Check(job.Points.Count==0,"worker does not publish mutable partial samples");
   Check(job.Completion.Wait(TimeSpan.FromSeconds(60)),"background completion timeout");job.Pump();
   Check(job.Complete&&job.Error==null,"background completion");
   Check(job.WorkerThreadId!=Environment.CurrentManagedThreadId,"replay ran off UI thread");
   Check(job.Points.First().Exists==false&&job.Points.First(p=>p.Exists).Tick==birth&&job.Points.First(p=>p.Exists&&!p.Alive).Tick==death,"exact off-grid life transitions");
   Check(job.Points.Where(p=>p.Exists).All(p=>truth[p.Tick]==(p.Alive,p.Energy!.Value,p.Water!.Value,p.Health!.Value,p.X!.Value,p.Y!.Value)),"individual points match independent forward measurements");
   Draw(1);
   Send(new Event{Type=0x400,B=lifeTraceChart.X,C=lifeTraceChart.Y+100});Draw(1);Check(lifeTraceHovered is {Exists:false},"hover before birth is absent");
   Send(new Event{Type=0x400,B=lifeTraceChart.X+lifeTraceChart.W-1,C=lifeTraceChart.Y+100});Draw(1);Check(lifeTraceHovered is {Exists:true,Alive:false}&&lifeTraceHovered.Tick==6000,"right edge reaches final sample and identifies dead body state");
   Check(before.SequenceEqual(Bytes(world))&&archive.SequenceEqual(timeline.SerializeArchive()),"original checkpoint RNG and archive unchanged");Send(new Event{Type=0x300,B=27});Check(!lifeTraceOpen,"Escape closes complete trace");
   ShowLifeTrace(166);PumpLifeTrace();var cancelled=lifeTrace!;Check(SpinWait.SpinUntil(()=>Volatile.Read(ref cancelled.WorkerThreadId)!=0,10000),"worker started before cancellation");Send(new Event{Type=0x300,B=27});cancelled.Pump();Check(cancelled.Cancelled&&cancelled.Points.Count==0&&!lifeTraceOpen,"cancel clears private work");
   Check(cancelled.Completion.Wait(TimeSpan.FromSeconds(10)),"cancelled worker exits");cancelled.Pump();Check(cancelled.Points.Count==0,"cancelled result cannot reappear");
   RequestTimelineSeek(1200);while(timeline.IsSeeking)timeline.PumpSeek(20,100);SyncTimelineWorld();string viewHash=world.StateHash();ShowLifeTrace(166);
   Check(lifeTrace!.Completion.Wait(TimeSpan.FromSeconds(60)),"past completion timeout");lifeTrace.Pump();
   Check(lifeTrace is {Complete:true}&&lifeTrace.ToTick==1200&&lifeTrace.Points.All(p=>p.Tick<=1200&&p.DeathTime==null),"historical trace cannot see future death");
   Check(world.Tick==1200&&world.StateHash()==viewHash&&before.SequenceEqual(Bytes(timeline.HeadWorld)),"trace does not move displayed time or head");
   Draw(1);int px=lifeTraceChart.X+lifeTraceChart.W/2,py=lifeTraceChart.Y+100;Send(new Event{Type=0x400,B=px,C=py});Draw(1);long target=lifeTraceHovered!.Tick;
   Check(target<=1200&&lifeTraceHovered.Exists&&lifeTraceHovered.Alive,"hover reports actual past sample");
   Send(new Event{Type=0x401,Button=1,B=px,C=py});Check(!lifeTraceOpen&&timelineOpen,"chart click opens timeline");while(timeline.IsSeeking)timeline.PumpSeek(20,100);SyncTimelineWorld();
   Check(world.Tick==target&&before.SequenceEqual(Bytes(timeline.HeadWorld)),"chart seeks exact sample and preserves future");CloseLifeTrace();
  }finally{CloseObservationPanels();world=original;timeline=oldTimeline;currentSlot=oldSlot;mainMenu=oldMenu;hasActiveWorld=oldActive;paused=oldPaused;details=oldDetails;selected=oldSelected;timelineOpen=false;FitWorld(1.25f);}
 }
}
