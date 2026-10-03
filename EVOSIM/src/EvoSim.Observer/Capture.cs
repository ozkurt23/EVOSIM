using EvoSim.Core;

internal sealed partial class ObserverApp
{
 void CaptureWorld(){
  bool withTimeline=args.Contains("--trace")||args.Contains("--timeline")||Arg("--seek-tick")!=null;
  if(withTimeline)EnsureTimeline();
  if(args.Contains("--drought")){if(withTimeline)timeline!.ExecuteDrought(true);else world.SetDrought(true);}
  for(int i=0;i<captureTicks;i++){if(withTimeline)timeline!.AdvancePlaybackOneTick();else world.Step();}
  if(withTimeline){
   ShowTimeline();
   if(long.TryParse(Arg("--seek-tick"),out long seekTick)){RequestTimelineSeek(seekTick);while(timeline!.IsSeeking)timeline.PumpSeek(6,100);SyncTimelineWorld();}
  }
  if(float.TryParse(Arg("--zoom"),out float z))view.Zoom=z;
  if(args.Contains("--follow")&&world.Find(selected) is Creature chosen){view.CenterX=chosen.X;view.CenterY=chosen.Y;}
  if(args.Contains("--setup")){OpenSetup();if(int.TryParse(Arg("--setup-tab"),out int tab))setupTab=Math.Clamp(tab,0,8);if(float.TryParse(Arg("--setup-scroll"),out float scroll))setupScroll=Math.Max(0,scroll);}
  if(args.Contains("--overview"))showOverview=true;if(args.Contains("--library"))libraryOpen=true;if(args.Contains("--main-menu")){mainMenu=true;paused=true;}
  if(args.Contains("--details"))details=true;
  if(args.Contains("--tree")){
   int treeId=int.TryParse(Arg("--tree-id"),out int givenTree)?givenTree:world.Map.Trees.Select((t,i)=>(Tree:t,Id:i)).OrderByDescending(item=>item.Tree.Fruit.Any(f=>f<=0)&&item.Tree.Fruit.Any(f=>f>0)).ThenBy(item=>Math.Pow(item.Tree.X-world.Map.Size*.5f,2)+Math.Pow(item.Tree.Y-world.Map.Size*.5f,2)).Select(item=>item.Id).FirstOrDefault(-1);
   if(treeId>=0&&treeId<world.Map.Trees.Count){var tree=world.Map.Trees[treeId];view.CenterX=tree.X;view.CenterY=tree.Y;Layout();var point=view.Screen(tree.X,tree.Y);TrySelectResource((int)point.X,(int)point.Y);}
  }
  if(args.Contains("--insights") || args.Contains("--ecology"))ShowPopulationInsights();
  if(args.Contains("--ecology"))populationHealthTab=true;
  if(args.Contains("--lineage")){
   lineageIndex.Refresh(world,true);int id=selected;
   if(id==0){var withChildren=world.Creatures.Where(c=>lineageIndex.Family(c.Id)!.Children.Count>0).ToList();id=withChildren.OrderByDescending(c=>c.ParentA>0).ThenByDescending(c=>lineageIndex.Family(c.Id)!.Children.Count).Select(c=>c.Id).FirstOrDefault(world.Creatures.FirstOrDefault()?.Id??0);}
   ShowLineage(id);
  }
  if(args.Contains("--journal"))ShowJournal();
  if(int.TryParse(Arg("--layer"),out int layer))view.Layer=(EvoSim.Observer.MapLayer)Math.Clamp(layer,0,3);
  if(args.Contains("--trace")){ShowLifeTrace(selected);if(lifeTrace!=null){lifeTrace.Completion.GetAwaiter().GetResult();lifeTrace.Pump();}}
  if(int.TryParse(Arg("--dock-count"),out int dockCount)){OpenDock(0);foreach(var creature in world.Creatures.Take(dockCount-1))OpenDock(creature.Id);}
  Layout();
  if(lifeTrace is {Complete:true} traced&&long.TryParse(Arg("--trace-hover-tick"),out long hoverTick)){Draw(1);mouseX=lifeTraceChart.X+(int)(Math.Clamp((hoverTick-traced.FromTick)/(double)Math.Max(1,traced.ToTick-traced.FromTick),0,1)*(lifeTraceChart.W-1));mouseY=lifeTraceChart.Y+lifeTraceChart.H/2;}
  for(int warm=0;warm<6;warm++){Draw(1);Thread.Sleep(30);}Draw(1);p.Screenshot(Path.GetFullPath(capturePath!));
  Console.WriteLine($"Capture: {capturePath}; tick={world.Tick}, head={timeline?.HeadTick??world.Tick}, living={world.Living}, births={world.Births}, deaths={world.Deaths}, lineage={(lineageOpen?lineageId:0)}, resource={resourceHandle}");
 }
}
