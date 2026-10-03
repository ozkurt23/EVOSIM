using EvoSim.Core;
using EvoSim.Observer;

namespace EvoSim.Observer {
 internal sealed record JournalEntry(long Tick,string Kind,string Text,int ChildId);
 internal static class EventJournal {
  internal static JournalEntry[] Read(World world,int filter=0)=>world.Events.Where(e=>e.Time<=world.Time+1e-7&&Matches(e.Kind,filter)).Reverse().Select(e=>new JournalEntry((long)Math.Round(e.Time/.05),e.Kind,Describe(world,e),e.ChildId)).ToArray();
  internal static string Describe(World world,SimEvent e){
   if(e.Kind is not ("birth" or "mutation" or "death"))return e.Text;
   int id=e.ChildId;
   // Older death records carry the individual ID only in their leading #ID text.
   if(e.Kind=="death"){
    int end=e.Text.IndexOf(' ');
    if(!e.Text.StartsWith('#')||end<2||!int.TryParse(e.Text.AsSpan(1,end-1),out id))return e.Text;
   }
   var creature=world.Find(id);
   return creature==null?e.Text:$"{(creature.Genes[2]<.5f?"Otçul":"Etçil")} · {e.Text}";
  }
  internal static bool Matches(string kind,int filter)=>filter switch{1=>kind is "birth" or "mutation",2=>kind=="death",3=>kind=="mutation",4=>kind is "climate" or "world",_=>true};
 }
}

internal sealed partial class ObserverApp {
 bool journalOpen;int journalFilter,journalOffset;World? journalWorld;long journalTick=-1;
 JournalEntry[] journalEntries=Array.Empty<JournalEntry>();
 readonly List<(Rect Rect,Action Action)> journalButtons=new();
 Rect journalRect;const int JournalRows=9;
 void CloseJournal(){journalOpen=false;journalButtons.Clear();journalWorld=null;}
 void ShowJournal(){if(mainMenu||!hasActiveWorld)return;PrepareObservationModal();journalOpen=true;journalFilter=journalOffset=0;RefreshJournal();}
 void RefreshJournal(){journalEntries=EventJournal.Read(world,journalFilter);journalWorld=world;journalTick=world.Tick;journalOffset=Math.Clamp(journalOffset,0,Math.Max(0,journalEntries.Length-JournalRows));}
 void JournalScroll(int delta){journalOffset=Math.Clamp(journalOffset+delta,0,Math.Max(0,journalEntries.Length-JournalRows));journalButtons.Clear();}
 bool HandleJournalEvent(Event ev){
  if(!journalOpen)return false;
  if(ev.Type==0x400){mouseX=ev.B;mouseY=ev.C;return true;}
  if(ev.Type==0x401){mouseX=ev.B;mouseY=ev.C;if(ev.Button==1)journalButtons.LastOrDefault(b=>b.Rect.Contains(mouseX,mouseY)).Action?.Invoke();return true;}
  if(ev.Type==0x402)return true;
  if(ev.Type==0x403){JournalScroll((ev.C==1?ev.B:-ev.B)*3);return true;}
  if(ev.Type==0x300){switch(ev.B){case 27:CloseJournal();break;case 1073741906:JournalScroll(-1);break;case 1073741905:JournalScroll(1);break;case 1073741899:JournalScroll(-JournalRows);break;case 1073741902:JournalScroll(JournalRows);break;case 1073741898:JournalScroll(-int.MaxValue);break;case 1073741901:JournalScroll(int.MaxValue-journalOffset);break;}return true;}
  return ev.Type>=0x300&&ev.Type<0x400;
 }
 void JournalButton(string label,Rect r,Action action,bool active=false){p.RoundBox(r.X,r.Y,r.W,r.H,7,active?gold:r.Contains(mouseX,mouseY)?Col.Mix(panel,text,.08f):panel);p.Text(label,r.X+11,r.Y+8,12,text);journalButtons.Add((r,action));}
 void DrawJournal(){
  if(!journalOpen)return;if(mainMenu||setupOpen||libraryOpen||naming){CloseJournal();return;}
  if(!ReferenceEquals(journalWorld,world)||journalTick!=world.Tick)RefreshJournal();journalButtons.Clear();
  int width=Math.Min(1040,p.W-64),height=690,x=(p.W-width)/2,y=(p.H-height)/2;journalRect=new Rect(x,y,width,height);
  p.Box(0,0,p.W,p.H,new Col(38,62,53,90));p.RoundBox(x+3,y+6,width,height,18,new Col(38,62,53,35));p.RoundBox(x,y,width,height,18,Col.Hex(0xF7F5EC));
  p.Text("Dünyanın günlüğü",x+28,y+24,23,text);p.Text($"{Clock(world.Time)} · {world.Events.Count} saklanan olay · gözlem duraklatıldı",x+29,y+60,12,muted);
  JournalButton("×",new Rect(x+width-51,y+22,30,30),CloseJournal);
  string[] filters={"Tümü","Doğumlar","Ölümler","Mutasyonlar","Çevre"};for(int i=0;i<filters.Length;i++){int f=i;JournalButton(filters[i],new Rect(x+28+i*132,y+92,122,32),()=>{journalFilter=f;journalOffset=0;RefreshJournal();},journalFilter==i);}
  int rowY=y+145;for(int i=0;i<JournalRows;i++){
   int n=journalOffset+i;if(n>=journalEntries.Length)break;var e=journalEntries[n];int yy=rowY+i*51;
   var color=e.Kind switch{"birth"=>green,"mutation"=>gold,"death"=>red,"climate"=>blue,_=>muted};
   p.Line(x+28,yy+47,x+width-28,yy+47,line.Alpha(120));p.Disk(x+32,yy+17,3,color);p.Text(Clock(e.Tick*.05),x+45,yy+10,10,muted);Wrap(e.Text,x+218,yy+4,12,text,Math.Max(30,(width-424)/7),2);
   if(e.ChildId>0&&world.Find(e.ChildId)!=null)JournalButton("Soy",new Rect(x+width-162,yy+6,52,30),()=>ShowLineage(e.ChildId));
   if(timeline!=null&&e.Tick>=timeline.EarliestTick&&e.Tick<=timeline.HeadTick)JournalButton("Anı gör",new Rect(x+width-101,yy+6,73,30),()=>{ShowTimeline();RequestTimelineSeek(e.Tick);});
  }
  if(journalEntries.Length==0)p.Text("Bu filtrede kayıtlı olay yok.",x+29,rowY+40,15,muted);
  int end=Math.Min(journalEntries.Length,journalOffset+JournalRows);p.Text(journalEntries.Length==0?"0 olay":$"{journalOffset+1}–{end} / {journalEntries.Length} · tekerlek veya ↑ ↓",x+29,y+616,12,muted);
  p.Text("Bu anın saklanan olaylarıdır. Uzun koşularda eski satırlar kırpılır.",x+29,y+646,12,muted);
  if(journalEntries.Length>JournalRows){int trackY=rowY,trackH=JournalRows*51-5;float thumb=Math.Max(20,trackH*JournalRows/(float)journalEntries.Length),pos=journalOffset/(float)(journalEntries.Length-JournalRows);p.RoundBox(x+width-14,trackY,4,trackH,2,line);p.RoundBox(x+width-14,trackY+(trackH-thumb)*pos,4,thumb,2,green);}
 }
 void JournalSmoke(){
  void Check(bool ok,string why){if(!ok)throw new Exception("Journal: "+why);}
  void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
  var old=world;var oldSession=timeline;bool oldMenu=mainMenu,oldActive=hasActiveWorld;try{
   world=new World(42);timeline=TimelineSession.Create(world);hasActiveWorld=true;mainMenu=false;for(int i=0;i<1200;i++)AdvanceWorld();timeline.ExecuteDrought(true);
   using var before=new MemoryStream();world.Save(before);string hash=world.StateHash();ShowJournal();Layout();Draw(1);
   Check(journalEntries.Length==world.Events.Count&&journalEntries[0].Kind=="climate","actual reverse ordered events");
   Send(new Event{Type=0x300,B=100});Send(new Event{Type=0x300,B=1073741903});Check(world.StateHash()==hash&&journalOpen&&paused,"modal blocks simulation");
   Send(new Event{Type=0x403,B=-1});Check(journalOffset==Math.Min(3,Math.Max(0,journalEntries.Length-JournalRows)),"wheel scroll");
   Draw(1);Send(new Event{Type=0x401,Button=1,B=journalRect.X+28+132+20,C=journalRect.Y+106});Check(journalFilter==1,"click birth filter");Check(journalEntries.All(e=>e.Kind is "birth" or "mutation"),"birth filter includes mutation births");
   if(journalEntries.Length>0){int child=journalEntries[0].ChildId;Draw(1);Send(new Event{Type=0x401,Button=1,B=journalRect.X+journalRect.W-145,C=journalRect.Y+166});Check(lineageOpen&&lineageId==child,"birth opens actual family");CloseLineage();ShowJournal();}
   journalFilter=3;RefreshJournal();Check(journalEntries.All(e=>e.Kind=="mutation"),"rare filter");Check(EventJournal.Matches("mutation",1)&&!EventJournal.Matches("death",4),"filter boundaries");
   using var after=new MemoryStream();world.Save(after);Check(before.ToArray().SequenceEqual(after.ToArray()),"full checkpoint unchanged");
   journalFilter=4;RefreshJournal();Draw(1);int initial=Array.FindIndex(journalEntries,e=>e.Tick==0);Check(initial>=0,"origin event available");Send(new Event{Type=0x401,Button=1,B=journalRect.X+journalRect.W-65,C=journalRect.Y+166+initial*51});Check(!journalOpen&&timelineOpen,"event opens timeline");while(timeline.IsSeeking)timeline.PumpSeek(20,100);SyncTimelineWorld();ShowJournal();Check(journalEntries.All(e=>e.Tick==0)&&journalEntries.Length==1,"past journal cannot see future");
   Send(new Event{Type=0x300,B=27});Check(!journalOpen,"Escape closes");
  }finally{CloseJournal();world=old;timeline=oldSession;mainMenu=oldMenu;hasActiveWorld=oldActive;timelineOpen=false;FitWorld();}
 }
}
