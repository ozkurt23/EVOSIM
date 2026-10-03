using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 readonly LineageIndex lineageIndex=new();
 readonly List<(Rect Rect,Action Action)> lineageButtons=new();
 bool lineageOpen;int lineageId,lineagePage,lineageKeyboard=-1;
 Rect lineageRect;const int LineagePageSize=4;
 void ShowLineage(int id)
 {
  if(mainMenu||setupOpen||libraryOpen||naming||world.Find(id)==null)return;
  PrepareObservationModal();lineageIndex.Refresh(world,true);
  lineageOpen=true;FocusLineage(id);
 }
 void FocusLineage(int id)
 {
  lineageIndex.Refresh(world,true);if(lineageIndex.Find(id)==null)return;
  lineageId=id;lineagePage=0;lineageKeyboard=-1;lineageButtons.Clear();
 }
 void CloseLineage(){lineageOpen=false;lineageButtons.Clear();lineageKeyboard=-1;lineageRect=default;}
 void PageLineage(int delta)
 {
  var family=lineageIndex.Family(lineageId);if(family==null)return;
  int last=Math.Max(0,(family.Children.Count-1)/LineagePageSize);
  lineagePage=Math.Clamp(lineagePage+delta,0,last);lineageButtons.Clear();lineageKeyboard=-1;
 }
 bool HandleLineageEvent(Event ev)
 {
  if(!lineageOpen)return false;
  // Own input completely while modal. Quit/resize still reach the application.
  if(ev.Type==0x400){mouseX=ev.B;mouseY=ev.C;return true;}
  if(ev.Type==0x401)
  {
   mouseX=ev.B;mouseY=ev.C;
   if(ev.Button==1){var hit=lineageButtons.LastOrDefault(b=>b.Rect.Contains(mouseX,mouseY));hit.Action?.Invoke();}
   return true;
  }
  if(ev.Type==0x402)return true;
  if(ev.Type==0x403){if(lineageRect.Contains(mouseX,mouseY)){int delta=ev.C==1?-ev.B:ev.B;if(delta!=0)PageLineage(delta<0?1:-1);}return true;}
  if(ev.Type==0x300)
  {
   if(ev.B==27){CloseLineage();return true;}
   if(ev.Repeat!=0)return true;
   if(ev.B==9&&lineageButtons.Count>0){int direction=(ev.C&3)!=0?-1:1;lineageKeyboard=lineageKeyboard<0?(direction>0?0:lineageButtons.Count-1):(lineageKeyboard+direction+lineageButtons.Count)%lineageButtons.Count;}
   else if((ev.B==13||ev.B==32)&&lineageKeyboard>=0&&lineageKeyboard<lineageButtons.Count)lineageButtons[lineageKeyboard].Action();
   else if(ev.B==1073741904||ev.B==1073741899)PageLineage(-1);
   else if(ev.B==1073741903||ev.B==1073741902)PageLineage(1);
   return true;
  }
  return ev.Type>=0x300&&ev.Type<0x400;
 }
 void LineageControl(string label,Rect rect,Action action,bool active=false)
 {
  int index=lineageButtons.Count;bool hover=rect.Contains(mouseX,mouseY);
  p.RoundBox(rect.X,rect.Y,rect.W,rect.H,8,active?gold:hover?Col.Mix(panel,text,.08f):panel);
  if(lineageKeyboard==index)p.RoundBox(rect.X,rect.Y+rect.H-3,rect.W,3,1,green);
  p.Text(label,rect.X+12,rect.Y+(rect.H-20)/2,13,text);lineageButtons.Add((rect,action));
 }
 void DrawLineagePanel()
 {
  if(!lineageOpen)return;
  if(mainMenu||setupOpen||libraryOpen||naming){CloseLineage();return;}
  lineageIndex.Refresh(world);var individual=lineageIndex.Find(lineageId);var family=lineageIndex.Family(lineageId);
  if(individual==null||family==null){CloseLineage();return;}
  lineageButtons.Clear();int width=Math.Min(920,p.W-64),height=650,x=(p.W-width)/2,y=(p.H-height)/2;
  lineageRect=new Rect(x,y,width,height);int center=x+width/2;
  p.Box(0,0,p.W,p.H,new Col(38,62,53,76));p.RoundBox(x+3,y+6,width,height,18,new Col(38,62,53,38));p.RoundBox(x,y,width,height,18,Col.Hex(0xF7F5EC));
  p.Text($"#{individual.Id} · Soy bağı",x+28,y+23,21,text);
  p.Text($"{Clock(world.Time)} · Bu anda kayıtlı aile",x+29,y+54,12,muted);
  LineageControl("×",new Rect(x+width-53,y+20,31,31),CloseLineage);
  if(individual.Alive)LineageControl("Haritada göster",new Rect(x+width-230,y+22,161,32),()=>{int id=lineageId;CloseLineage();ShowLineageIndividual(id);});
  var stats=lineageIndex.Counts(lineageId);var metrics=new[]{("Yavru",stats.Children),("Torun",stats.Grandchildren),("Toplam altsoy",stats.Descendants),("Yaşayan altsoy",stats.LivingDescendants)};
  int metricWidth=(width-68)/4;
  for(int i=0;i<metrics.Length;i++){int mx=x+28+i*(metricWidth+4);p.RoundBox(mx,y+91,metricWidth-4,60,10,Col.Hex(0xE8EDE0));p.Text(metrics[i].Item2.ToString(),mx+15,y+100,22,text);p.Text(metrics[i].Item1,mx+15,y+129,11,muted);}
  p.Line(x+28,y+168,x+width-28,y+168,line);
  p.Text("EBEVEYNLER",x+29,y+182,11,muted);
  int nodeWidth=190,parentY=y+206,focusY=y+330,childrenY=y+477;
  var parentRects=new List<(int Id,Rect Rect)>();
  for(int i=0;i<family.Parents.Count;i++){int px=center-(family.Parents.Count*nodeWidth+(family.Parents.Count-1)*30)/2+i*(nodeWidth+30);parentRects.Add((family.Parents[i],new Rect(px,parentY,nodeWidth,76)));}
  foreach(var parent in parentRects){float px=parent.Rect.X+nodeWidth/2f;p.Line(px,parentY+76,px,y+305,line);p.Line(px,y+305,center,y+305,line);}
  if(parentRects.Count>0)p.Line(center,y+305,center,focusY,line);
  else p.Text("Kurucu birey · kayıtlı ebeveyn yok",center-126,parentY+25,13,muted);
  int last=Math.Max(0,(family.Children.Count-1)/LineagePageSize);lineagePage=Math.Clamp(lineagePage,0,last);
  int[] page=family.Children.Skip(lineagePage*LineagePageSize).Take(LineagePageSize).ToArray();
  if(page.Length>0){p.Line(center,focusY+82,center,y+435,line);for(int i=0;i<page.Length;i++){int cx=center-(page.Length*nodeWidth+(page.Length-1)*14)/2+i*(nodeWidth+14)+nodeWidth/2;p.Line(center,y+435,cx,y+435,line);p.Line(cx,y+435,cx,childrenY,line);}}
  foreach(var parent in parentRects)LineageNode(parent.Id,parent.Rect);
  LineageNode(individual.Id,new Rect(center-nodeWidth/2,focusY,nodeWidth,82),true);
  p.Text($"YAVRULAR · {family.Children.Count}",x+29,y+453,11,muted);
  for(int i=0;i<page.Length;i++){int cx=center-(page.Length*nodeWidth+(page.Length-1)*14)/2+i*(nodeWidth+14);LineageNode(page[i],new Rect(cx,childrenY,nodeWidth,76));}
  if(page.Length==0)p.Text("Bu anda kayıtlı yavrusu yok.",center-109,childrenY+28,13,muted);
  p.Text("Her birey bir kez sayılır. Torun: tam iki soy bağı uzaklığı.",x+29,y+584,11,muted);
  p.Text("Bir yakına tıkla · ailesini incele",x+29,y+608,12,muted);
  if(last>0)
  {
   if(lineagePage>0)LineageControl("←",new Rect(x+width-199,y+586,37,32),()=>PageLineage(-1));
   p.Text($"{lineagePage+1} / {last+1}",x+width-144,y+594,12,muted);
   if(lineagePage<last)LineageControl("→",new Rect(x+width-70,y+586,37,32),()=>PageLineage(1));
  }
 }
 void LineageNode(int id,Rect rect,bool focus=false)
 {
  var creature=lineageIndex.Find(id);if(creature==null)return;
  int index=lineageButtons.Count;bool hover=rect.Contains(mouseX,mouseY);Col edge=focus?gold:line,fill=focus?Col.Hex(0xF4E6C4):creature.Alive?Col.Hex(0xE8EDE0):Col.Hex(0xEBEBE3);
  if(hover)edge=green;if(lineageKeyboard==index)edge=green;
  p.RoundBox(rect.X-1,rect.Y-1,rect.W+2,rect.H+2,11,edge);p.RoundBox(rect.X,rect.Y,rect.W,rect.H,10,fill);
  bool herbivore=creature.Genes[2]<.5f;Col color=herbivore?green:red;if(!creature.Alive)color=Col.Mix(color,muted,.65f);
  if(herbivore)p.Ellipse(rect.X+21,rect.Y+24,8,7,0,color,color,20);else p.Disk(rect.X+21,rect.Y+24,9,color,3);
  p.Text($"#{id}",rect.X+38,rect.Y+12,16,text);
  p.Text($"{(herbivore?"Otçul":"Etçil")} · {creature.Generation}. nesil",rect.X+14,rect.Y+37,11,muted);
  p.Disk(rect.X+18,rect.Y+64,2.5f,creature.Alive?green:muted);p.Text(creature.Alive?"Yaşıyor":"Yaşamı sona erdi",rect.X+28,rect.Y+55,11,creature.Alive?green:muted);
  lineageButtons.Add((rect,()=>FocusLineage(id)));
 }
}
