using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 void ObservationUiSmoke(){
  void Check(bool ok,string why){if(!ok)throw new Exception("Observation UI: "+why);}
  void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
  void Click(int x,int y)=>Send(new Event{Type=0x401,Button=1,B=x,C=y});
  byte[] Snapshot(){using var stream=new MemoryStream();world.Save(stream);return stream.ToArray();}
  var original=world;var oldTimeline=timeline;var oldSlot=currentSlot;bool oldMenu=mainMenu,oldActive=hasActiveWorld,oldPaused=paused,oldDetails=details;int oldSelected=selected;
  try{
   CloseObservationPanels();world=new World(42,new WorldConfig{MapSize=256,Herbivores=480,Carnivores=20,TreeCount=350});timeline=TimelineSession.Create(world);currentSlot=null;hasActiveWorld=true;mainMenu=setupOpen=libraryOpen=naming=false;paused=true;
   for(int i=0;i<1200;i++)AdvanceWorld();Check(world.Births>0,"real seeded births available");lineageIndex.Refresh(world,true);
   var parent=world.Creatures.First(c=>c.Alive&&lineageIndex.Family(c.Id)!.Children.Count>0);var family=lineageIndex.Family(parent.Id)!;
   byte[] before=Snapshot();string hash=world.StateHash();long tick=world.Tick;ShowLineageIndividual(parent.Id);details=true;Layout();Draw(1);
   Check(creatureGeneSpots.Count==6,"six actual genes on expanded card");int cardSelection=selected;Click(observationCardRect.X+30,observationCardRect.Y+277);Check(selected==cardSelection,"card text cannot click through");
   Click(p.W-314+219,88+220);Check(lineageOpen&&paused,"contextual lineage button opens paused modal");Draw(1);
   Send(new Event{Type=0x300,B=32});Send(new Event{Type=0x300,B=100});Send(new Event{Type=0x300,B=110});Check(lineageOpen&&!setupOpen&&!world.Drought&&world.Tick==tick,"lineage blocks simulation shortcuts");
   int count=Math.Min(family.Children.Count,4),firstX=lineageRect.X+lineageRect.W/2-(count*190+(count-1)*14)/2;Click(firstX+90,lineageRect.Y+512);Check(lineageId==family.Children[0],"real child node changes family focus");Draw(1);Send(new Event{Type=0x300,B=27});Check(!lineageOpen&&paused,"Escape closes lineage");
   selected=0;showOverview=true;Draw(1);Click(observationCardRect.X+70,observationCardRect.Y+110);Check(showOverview&&selected==0&&!HasResourceSelection,"population card cannot click through");Click(p.W-342+210,88);Check(populationInsightsOpen,"overview contextual genes button");Draw(1);
   Send(new Event{Type=0x300,B=1073741903});Send(new Event{Type=0x300,B=100});Click(p.W/2-65,p.H-51);Check(populationInsightsOpen&&paused&&!world.Drought&&world.Tick==tick,"population modal blocks step/intervention/play");
   Check(populationInsightsCache.Get(world).Living==world.Living,"histogram uses displayed living population");
   Click(populationHealthTabRect.X+20,populationHealthTabRect.Y+10);Draw(1);Check(populationHealthTab&&populationHealth!=null,"life tab opens from actual input");
   var health=populationHealth!;var gates=health.Breeding.ExclusiveFirstBlocker;
   Check(health.Deaths==world.Deaths&&health.DeathCauses.Values.Sum()==world.Deaths,"death reasons account for actual deaths");
   Check(gates.Disabled+gates.Immature+gates.Cooldown+gates.LowEnergy+gates.LowWater+gates.LowHealth+gates.OtherIneligible+gates.NoPositiveMateSignal+gates.ReadyWithSignal==world.Living,"exclusive breeding gates partition living population");
   Send(new Event{Type=0x300,B=100});Send(new Event{Type=0x300,B=1073741903});Check(world.Tick==tick&&!world.Drought,"life tab blocks model shortcuts");
   Click(populationGenesTabRect.X+20,populationGenesTabRect.Y+10);Check(!populationHealthTab,"return to genes");Send(new Event{Type=0x300,B=27});Check(!populationInsightsOpen&&paused,"Escape closes distributions");
   bool picked=false;foreach(var tree in world.Map.Trees){view.CenterX=tree.X;view.CenterY=tree.Y;view.Zoom=6;Layout();var point=view.Screen(tree.X,tree.Y);if(view.Pick(world,(int)point.X,(int)point.Y)!=0)continue;Click((int)point.X,(int)point.Y);picked=resourceHandle?.Kind==InspectableResourceKind.Tree;if(picked)break;}
   Check(picked&&selected==0,"tree pick through real map input");Draw(1);var handle=resourceHandle;Click(observationCardRect.X+50,observationCardRect.Y+100);Check(resourceHandle==handle,"resource text cannot click through");
   Send(new Event{Type=0x300,B=27});Check(!HasResourceSelection,"Escape clears resource");
   var body=world.Creatures.First(c=>c.Alive);view.CenterX=body.X;view.CenterY=body.Y;Layout();var bodyPoint=view.Screen(body.X,body.Y);Click((int)bodyPoint.X,(int)bodyPoint.Y);Check(selected>0&&!HasResourceSelection,"creature picks take priority");
   Check(world.StateHash()==hash&&before.SequenceEqual(Snapshot()),"UI observation leaves full world and RNG unchanged");
   RequestTimelineSeek(0);while(timeline.IsSeeking)timeline.PumpSeek(20,100);SyncTimelineWorld();ShowPopulationInsights();Check(populationInsightsCache.Get(world).Living==500,"past histogram cannot see future deaths/births");
   populationHealthTab=true;Draw(1);Check(populationHealth!.Deaths==0&&populationHealth.Births==0&&populationHealth.DeathCauses.Count==0,"past life tab excludes future lifecycle records");ClosePopulationInsights();ShowLineage(family.Children[0]);Check(!lineageOpen,"unborn individual absent in earlier world");
  }finally{
   CloseObservationPanels();world=original;timeline=oldTimeline;currentSlot=oldSlot;mainMenu=oldMenu;hasActiveWorld=oldActive;paused=oldPaused;details=oldDetails;selected=oldSelected;showOverview=false;timelineOpen=false;FitWorld(1.25f);
  }
 }
}
