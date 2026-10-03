using EvoSim.Core;
using EvoSim.Observer;
internal sealed partial class ObserverApp
{
 void CalendarDockSmoke(){
  void Check(bool ok,string message){if(!ok)throw new Exception("Calendar/docks: "+message);}
  void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
  byte[] Bytes(World w){using var s=new MemoryStream();w.Save(s);return s.ToArray();}
  var oldWorld=world;var oldDraft=draft;int oldTab=setupTab,oldSelected=selected;bool oldMenu=mainMenu,oldActive=hasActiveWorld,oldPaused=paused;
  try{
   Check(WorldCalendar.Duration(2+15/24d)=="2 gün 15 saat","days and hours");
   Check(WorldCalendar.Duration(365)=="1 yıl 0 gün"&&WorldCalendar.Duration(1200)=="3 yıl 105 gün","year boundary");
   Check(WorldCalendar.Duration(.05,true)=="0 gün 01:12","single tick retains exact calendar time");
   draft=new WorldConfig();setupTab=1;var lifespan=Entries().Single(e=>e.Key=="HerbLifespan");
   Edit(lifespan);editSetter!(nameText);Check(draft.HerbLifespan==1200,"unchanged edit retains exact raw value");
   editSetter!("100");Check(draft.HerbLifespan==36500,"100 years converts to 36500 days");
   editSetter!("0");Check(draft.HerbLifespan==0,"immortality preserved");naming=false;editSetter=null;SDL.SDL_StopTextInput();
   CloseObservationPanels();world=new World(42);mainMenu=setupOpen=libraryOpen=menuOpen=false;hasActiveWorld=true;paused=true;selected=0;
   byte[] before=Bytes(world);OpenDock(0);for(int i=1;i<=8;i++)OpenDock(i);
   Check(docks.Select(s=>s.Count).SequenceEqual(new[]{3,2,2,2}),"nine panels distributed 3/2/2/2");
   Check(docks[0][^1].Id==0&&docks[1][^1].Id==1,"new panels stay behind existing fronts");
   OpenDock(8);Check(docks[0][^1].Id==8&&docks.Sum(s=>s.Count)==9,"same individual raises existing card");
   Layout();Draw(1);var tab=dockActions.First();Send(new Event{Type=0x401,Button=1,B=tab.Rect.X+10,C=tab.Rect.Y+10});Check(docks.Sum(s=>s.Count)==9,"stack tab switches without creating duplicate");
   Draw(1);var rect=dockRects.Last().Rect;int picked=selected;Send(new Event{Type=0x401,Button=1,B=rect.X+35,C=rect.Y+135});Check(selected==picked,"panel body blocks map clicks");
   float zoom=view.Zoom;mouseX=rect.X+20;mouseY=rect.Y+140;Send(new Event{Type=0x403,B=-1});Check(view.Zoom==zoom,"panel wheel does not zoom map");
   ClearDocks();OpenDock(0);view.CenterX=world.Creatures[0].X;view.CenterY=world.Creatures[0].Y;Layout();Draw(1);var pt=view.Screen(world.Creatures[0].X,world.Creatures[0].Y);Send(new Event{Type=0x401,Button=1,B=(int)pt.X,C=(int)pt.Y});
   Check(showOverview&&docks[0].Any(c=>c.Id==0)&&docks[1].Any(c=>c.Id>0),"real map pick preserves graph and places creature below");
   Check(before.SequenceEqual(Bytes(world)),"panel operations preserve full world state");
   var herb=world.Creatures.First(c=>c.Genes[2]<.5f);var carn=world.Creatures.First(c=>c.Genes[2]>=.5f);
   foreach(var c in new[]{herb,carn}){
    string kind=c.Genes[2]<.5f?"Otçul":"Etçil";
    Check(EventJournal.Describe(world,new SimEvent{Kind="birth",ChildId=c.Id,Text=$"#{c.Id} doğdu"}).StartsWith(kind),"birth diet");
    Check(EventJournal.Describe(world,new SimEvent{Kind="death",Text=$"#{c.Id} öldü · açlık"}).StartsWith(kind),"legacy death diet");
    Check(EventJournal.Describe(world,new SimEvent{Kind="mutation",ChildId=c.Id,Text=$"#{c.Id} doğdu · mutasyon"}).StartsWith(kind),"mutation birth diet");
   }
   Check(EventJournal.Describe(world,new SimEvent{Kind="death",Text="Eski kayıt"})=="Eski kayıt","missing subject never guessed");
  }finally{CloseObservationPanels();world=oldWorld;draft=oldDraft;setupTab=oldTab;mainMenu=oldMenu;hasActiveWorld=oldActive;paused=oldPaused;selected=oldSelected;showOverview=false;naming=false;editSetter=null;FitWorld();}
 }
}
