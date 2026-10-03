using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 sealed class DockCard {public int Id;public bool Expanded;public float Scroll;public int ContentHeight=574;public ResourceHandle? Resource;}
 readonly List<DockCard>[] docks={new(),new(),new(),new()};
 readonly List<(Rect Rect,Action Action)> dockActions=new();
 readonly List<(Rect Rect,DockCard Card)> dockRects=new();
 int nextDock;World? dockWorld;
 bool DrawingDock;Rect dockViewport;DockCard? drawingCard;
 bool HasDocks=>docks.Any(s=>s.Count>0);
 void ClearDocks(){if(docks.Any(s=>s.Any(c=>c.Id==0)))showOverview=false;foreach(var slot in docks)slot.Clear();dockRects.Clear();dockActions.Clear();nextDock=0;dockWorld=null;}
 void OpenDock(int id){
  if(!ReferenceEquals(dockWorld,world)){ClearDocks();dockWorld=world;}
  foreach(var slot in docks){var existing=slot.FirstOrDefault(c=>c.Id==id);if(existing!=null){slot.Remove(existing);slot.Add(existing);if(id>0)selected=id;return;}}
  var target=docks[nextDock++%4];var card=new DockCard{Id=id};
  // First four are visible. Further picks go behind the existing front of their slot.
  target.Insert(0,card);if(id>0)selected=id;else if(id==0)showOverview=true;
 }
 void ToggleOverviewDock(){
  var slot=docks.FirstOrDefault(s=>s.Any(c=>c.Id==0));
  if(slot!=null){slot.RemoveAll(c=>c.Id==0);showOverview=false;}else OpenDock(0);
 }
 void CloseDock(DockCard card){foreach(var slot in docks)slot.Remove(card);if(card.Id==0)showOverview=false;if(selected==card.Id){selected=0;follow=false;}dockRects.Clear();dockActions.Clear();}
 void OpenResourceDock(ResourceHandle handle){int key=-1-(int)handle.Kind*1000000-handle.Id;OpenDock(key);var card=docks.SelectMany(s=>s).First(c=>c.Id==key);card.Resource=handle;card.Expanded=true;}
 bool DocksVisible=>!mainMenu&&!setupOpen&&!libraryOpen&&!naming&&!lifeTraceOpen&&!journalOpen&&!lineageOpen&&!populationInsightsOpen;
 bool HandleDockEvent(Event ev){
  if(!DocksVisible||!HasDocks||menuOpen||timelineOpen)return false;
  if(ev.Type is 0x400 or 0x401 or 0x402){mouseX=ev.B;mouseY=ev.C;}
  var hit=dockRects.LastOrDefault(r=>r.Rect.Contains(mouseX,mouseY));
  if(ev.Type==0x401&&ev.Button==1){var action=dockActions.LastOrDefault(a=>a.Rect.Contains(mouseX,mouseY));if(action.Action!=null){action.Action();return true;}}
  if(hit.Card==null)return false;
  if(ev.Type==0x403){hit.Card.Scroll=Math.Clamp(hit.Card.Scroll+(ev.C==1?ev.B:-ev.B)*42,0,hit.Card.Expanded?Math.Max(0,hit.Card.ContentHeight-hit.Rect.H):0);return true;}
  return ev.Type is 0x400 or 0x401 or 0x402;
 }
 void DrawDocks(){
  if(!ReferenceEquals(dockWorld,world)){ClearDocks();return;}if(!DocksVisible)return;
  dockActions.Clear();dockRects.Clear();
  int slotHeight=Math.Max(270,(p.H-180)/2),width=312;
  for(int i=0;i<4;i++){
   var slot=docks[i];if(slot.Count==0)continue;
   int x=i<2?p.W-width-20:20,y=76+(i%2)*(slotHeight+14);
   var front=slot[^1];int visibleTabs=Math.Min(slot.Count-1,3),offset=visibleTabs*22;
   // Exposed headers switch cards; the overflow control keeps deeper cards reachable.
   for(int j=0;j<visibleTabs;j++){
    var card=slot[slot.Count-2-j];var tab=new Rect(x+6*(visibleTabs-j),y+j*22,width-6*(visibleTabs-j),26);
    p.RoundBox(tab.X,tab.Y,tab.W,tab.H,6,Col.Mix(panel,bg,j*.16f));p.Text(card.Resource!=null?"Kaynak":card.Id==0?"Nüfus":$"#{card.Id} · {(world.Find(card.Id)?.Genes[2]<.5f?"Otçul":"Etçil")}",tab.X+12,tab.Y+5,11,muted);
    dockRects.Add((tab,card));dockActions.Add((tab,()=>{slot.Remove(card);slot.Add(card);if(card.Id>0)selected=card.Id;}));
   }
   if(slot.Count>4){var rotate=new Rect(x+width-73,y,67,21);p.RoundBox(rotate.X,rotate.Y,rotate.W,rotate.H,5,gold);p.Text($"+{slot.Count-4} ↻",rotate.X+8,rotate.Y+4,10,text);dockActions.Add((rotate,()=>{var first=slot[0];slot.RemoveAt(0);slot.Add(first);}));}
   int cardHeight=front.Expanded?slotHeight-offset:front.Id==0?Math.Min(330,slotHeight-offset):Math.Min(250,slotHeight-offset);
   var rect=new Rect(x,y+offset,width,cardHeight);dockRects.Add((rect,front));
   p.RoundBox(rect.X,rect.Y,rect.W,rect.H,12,new Col(247,246,236,245));
   int buttonStart=buttons.Count;DrawingDock=true;drawingCard=front;dockViewport=rect;
   p.Clip(rect);
   if(front.Resource is ResourceHandle handle){var oldWorld=resourceWorld;var oldHandle=resourceHandle;var oldReading=resourceReading;long oldTick=resourceReadingTick;resourceWorld=world;resourceHandle=handle;resourceReading=null;if(CurrentResourceReading()!=null)ResourcePanel();else{p.Text("Bu kaynak artık mevcut değil.",rect.X+18,rect.Y+54,12,muted);Button("×",new Rect(rect.X+rect.W-41,rect.Y+9,31,29),()=>CloseDock(front),false,true);}resourceWorld=oldWorld;resourceHandle=oldHandle;resourceReading=oldReading;resourceReadingTick=oldTick;}
   else if(front.Id==0)DrawDockPopulation(rect);
   else if(world.Find(front.Id) is Creature c){bool oldDetails=details;details=front.Expanded;CreaturePanel(c);details=oldDetails;}
   else p.Text($"#{front.Id} · bu anda mevcut değil",rect.X+15,rect.Y+18,13,muted);
   p.Unclip();DrawingDock=false;drawingCard=null;
   foreach(var button in buttons.Skip(buttonStart))if(button.Rect.Y>=rect.Y&&button.Rect.Y+button.Rect.H<=rect.Y+rect.H)dockActions.Add(button);
   buttons.RemoveRange(buttonStart,buttons.Count-buttonStart);observationCardRect=rect;
   if(front.Expanded){p.Text("Kaydırarak ayrıntıları incele",rect.X+16,rect.Y+rect.H-18,10,muted);}
  }
 }
 void DrawDockPopulation(Rect r){
  int x=r.X+18,y=r.Y+12;p.Text("Nüfus",x,y,17,text);
  Button("×",new Rect(r.X+r.W-41,y-5,31,29),()=>CloseDock(drawingCardForPopulation!),false,true);
  p.Text($"{world.Herbivores} otçul · {world.Carnivores} etçil",x,y+38,13,green);
  Graph(x,y+75,274,105);p.Text($"{world.Births} doğum · {world.Deaths} ölüm",x,y+212,12,text);
  p.Text($"{world.Mutations} mutasyon",x,y+238,12,muted);
  Button("Genler ve yaşam",new Rect(x,y+272,178,30),ShowPopulationInsights,false,true);
 }
 DockCard? drawingCardForPopulation=>docks.SelectMany(s=>s).FirstOrDefault(c=>c.Id==0);
}
