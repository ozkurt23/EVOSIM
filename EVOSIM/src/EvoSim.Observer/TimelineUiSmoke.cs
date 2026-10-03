using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 void TimelineUiSmoke(){
  void Check(bool ok,string why){if(!ok)throw new Exception("Timeline UI: "+why);}
  void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
  void FinishSeek(){int safety=0;while(timeline!.IsSeeking){timeline.PumpSeek(20,100);if(++safety>100)throw new Exception("Seek did not finish");}SyncTimelineWorld();}
  var original=world;var originalSlot=currentSlot;var originalTimeline=timeline;bool oldActive=hasActiveWorld,oldMenu=mainMenu,oldPaused=paused;
  try{
   world=new World(67,new WorldConfig{Herbivores=24,Carnivores=3,TreeCount=25});timeline=TimelineSession.Create(world);currentSlot=null;hasActiveWorld=true;mainMenu=false;setupOpen=libraryOpen=naming=false;paused=true;timelineOpen=false;FitWorld(1.4f);
   string middle="";for(int tick=0;tick<720;tick++){if(tick==120||tick==180)ToggleDrought();AdvanceWorld();if(world.Tick==360)middle=world.StateHash();}
   Check(SaveCurrent(),"save recorded head");var parent=currentSlot!;string head=world.StateHash();
   store.Trash(parent);try{Send(new Event{Type=0x100});Check(running&&paused,"failed quit save keeps application open");}finally{store.Restore(parent);}
   ShowTimeline();Layout();Draw(1);int x=timelineTrack.X+timelineTrack.W/2,y=timelineTrack.Y;
   Send(new Event{Type=0x400,B=500,C=350});float oldX=view.CenterX;Send(new Event{Type=0x400,A=4,B=530,C=350});Check(view.CenterX!=oldX,"right drag pans while history is open");FitWorld(1.4f);
   Send(new Event{Type=0x401,Button=1,B=(p.W-176)/2+145,C=p.H-51});Check(menuOpen&&!timelineOpen&&!draggingTimeline,"popup closes timeline overlay");menuOpen=false;ShowTimeline();Draw(1);
   HandleTimelineEvent(new Event{Type=0x401,Button=1,B=x,C=y});Send(new Event{Type=0x300,B=108});Check(libraryOpen&&!timelineOpen&&!draggingTimeline,"opening library cancels scrub drag");libraryOpen=false;ShowTimeline();Draw(1);
   Send(new Event{Type=0x400,B=x,C=y});Send(new Event{Type=0x403,B=-1});Send(new Event{Type=0x403,B=-1});Check(timelineTargetTick==680,"queued wheel input accumulates seek target");timeline.CancelSeek();
   HandleTimelineEvent(new Event{Type=0x401,Button=1,B=x,C=y});HandleTimelineEvent(new Event{Type=0x402,Button=1});Check(timeline.IsSeeking,"scrub starts seek");timeline.PumpSeek(20,1);Check(timeline.HeadWorld.StateHash()==head,"seek leaves live head unchanged");
   HandleTimelineEvent(new Event{Type=0x300,B=27});Check(!timeline.IsSeeking&&world.StateHash()==head&&timelineOpen,"Escape cancels seek first");
   RequestTimelineSeek(360);FinishSeek();Check(world.StateHash()==middle&&timeline.IsPreviewingHistory,"seek displays exact past");
   Check(SaveCurrent()&&store.Load(parent).StateHash()==head,"autosave from past keeps future");
   Draw(1);p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v05-ui"),"timeline-past.bmp"));
   OpenWorld(parent);Check(world.StateHash()==head&&timeline!.HeadTick==720,"reopen returns preserved head");
   ShowTimeline();RequestTimelineSeek(360);FinishSeek();AdvanceWorld();Check(world.Tick==361&&timeline!.HeadTick==720&&currentSlot!.Id==parent.Id,"playback uses recorded future without branching");
   RequestTimelineSeek(360);FinishSeek();Check(BranchTimeline("Geçmişten yeni vadi"),"branch action");Check(currentSlot!.Id!=parent.Id&&timeline!.HeadTick==360&&timeline.EarliestTick==0&&world.StateHash()==middle,"new branch keeps prefix history");
   ToggleDrought();AdvanceWorld();Check(SaveCurrent()&&world.Drought&&store.Load(parent).StateHash()==head,"branch intervention leaves parent future intact");
   var child=currentSlot;var savedChild=world.StateHash();store.Trash(parent);OpenWorld(child!);Check(world.StateHash()==savedChild,"child independent after parent deletion");store.Restore(parent);Check(store.Load(parent).StateHash()==head,"restore keeps parent future");
   RequestTimelineSeek(0);FinishSeek();Check(timeline!.ViewTick==0,"child can still see copied origin");timeline.ReturnToHead();SyncTimelineWorld();
  }finally{
   world=original;currentSlot=originalSlot;timeline=originalTimeline;hasActiveWorld=oldActive;mainMenu=oldMenu;paused=oldPaused;timelineOpen=false;draggingTimeline=false;FitWorld(1.25f);
  }
 }
}
