using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 Rect observationCardRect;
 void CloseObservationPanels(){ClearDocks();CloseLifeTrace();CloseJournal();CloseLineage();ClosePopulationInsights();ClearResourceSelection();}
 void PrepareObservationModal(){
  HideTimeline();CloseLifeTrace();CloseJournal();CloseLineage();ClosePopulationInsights();menuOpen=setupOpen=libraryOpen=naming=false;
  SDL.SDL_StopTextInput();paused=true;accumulator=0;
 }
 void ShowLineageIndividual(int id){
  var creature=world.Find(id);if(creature is not {Alive:true})return;
  CloseObservationPanels();selected=id;showOverview=false;follow=false;
  view.CenterX=creature.X;view.CenterY=creature.Y;view.Zoom=Math.Clamp(Math.Max(view.Zoom,2.2f*world.Map.Size/128),.8f,view.MaximumZoom);paused=true;
 }
 void PickObservation(int x,int y){
  selected=view.Pick(world,x,y);ClearResourceSelection();if(selected>0){if(showOverview&&!HasDocks)OpenDock(0);OpenDock(selected);}else TrySelectResource(x,y);
  follow=false;menuOpen=false;
 }
}
