using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 readonly List<(Rect Rect,int Gene)> creatureGeneSpots=new();
 int hoveredCreatureGene=-1;double creatureGeneHoverSince;
 void CreaturePanel(Creature c){
  float x=DrawingDock?dockViewport.X+18:p.W-314,y=DrawingDock?dockViewport.Y+12-drawingCard!.Scroll:88;int height=details?574:250;observationCardRect=new Rect((int)x-18,(int)y-12,312,height);
  p.RoundBox(observationCardRect.X,observationCardRect.Y,observationCardRect.W,observationCardRect.H,12,new Col(245,243,233,245));
  var card=drawingCard;
  p.Text($"#{c.Id}  ·  {(c.Genes[2]<.5f?"Otçul":"Etçil")}",x,y,17,text);
  Button("×",new Rect((int)x+256,(int)y-5,31,29),()=>{if(card!=null)CloseDock(card);else{selected=0;follow=false;}},false,true);
  p.Text($"{WorldCalendar.Age(c.Age)}   ·   {c.Generation}. nesil",x,y+29,12,muted);
  CompactMeter("Enerji",c.Energy/c.MaxEnergy,x,y+60,green);CompactMeter("Su",c.Hydration/c.MaxHydration,x,y+86,blue);CompactMeter("Sağlık",c.Health/100,x,y+112,red);
  p.Text(ActionName(c.Action),x,y+149,16,c.Alive?green:red);
  lineageIndex.Refresh(world);var familyCounts=lineageIndex.Counts(c.Id);
  p.Text($"{familyCounts.Children} yavru   ·   {familyCounts.Grandchildren} torun",x,y+179,12,muted);
  Button(follow?"Takipte":"Takip",new Rect((int)x,(int)y+207,80,28),()=>{if(c.Alive){selected=c.Id;follow=!follow;}},follow,true);
  Button(details?"Daha az":"Ayrıntı",new Rect((int)x+91,(int)y+207,85,28),()=>{if(card!=null){card.Expanded=!card.Expanded;card.Scroll=0;}else details=!details;},false,true);
  Button("Soy",new Rect((int)x+187,(int)y+207,80,28),()=>ShowLineage(c.Id),false,true);
  if(!details)return;
  p.Line(x,y+248,x+266,y+248,line);
  p.Text(c.ParentA==0?"Kurucu birey":c.ParentB==0?$"Tek ebeveyn #{c.ParentA}":$"Ebeveynler #{c.ParentA} × #{c.ParentB}",x,y+259,12,muted);
  for(int i=0;i<Genetics.Count;i++){
   float yy=y+287+i*25;p.Text(Genetics.Names[i],x,yy,12,muted);
   var help=new Rect((int)x+94,(int)yy-2,20,20);p.Disk(help.X+10,help.Y+10,7,line);p.Text("i",help.X+8,help.Y+2,11,text);if(!DrawingDock||(help.Y>=dockViewport.Y&&help.Y+help.H<=dockViewport.Y+dockViewport.H))creatureGeneSpots.Add((help,i));
   p.RoundBox(x+124,yy+7,94,4,2,line);p.RoundBox(x+124,yy+7,94*c.Genes[i],4,2,i==2?Col.Mix(green,red,c.Genes[i]):green);p.Text(c.Genes[i].ToString("0.000"),x+229,yy,12,text);
  }
  p.Line(x,y+440,x+266,y+440,line);
  p.Text($"Hız kapasitesi  {c.Speed:0.00} birim/gün",x,y+452,12,muted);
  p.Text($"Görüş  {world.PerceptionRadius(c):0.0} birim · {world.PerceptionAngleDegrees(c):0}°",x,y+477,12,muted);
  p.Text($"Yaş etkisi  %{world.Aging(c)*100:0}",x,y+502,12,muted);
  Button("Yaşam izi",new Rect((int)x+137,(int)y+495,130,27),()=>ShowLifeTrace(c.Id),false,true);
  Wrap(c.Reason??"",x,y+525,11,muted,39,2);
 }
 void DrawCreatureHelp(){
  if(mainMenu||setupOpen||libraryOpen||naming||menuOpen||lineageOpen||populationInsightsOpen||lifeTraceOpen){hoveredCreatureGene=-1;return;}
  int gene=-1;Rect spot=default;foreach(var item in creatureGeneSpots)if(item.Rect.Contains(mouseX,mouseY)){gene=item.Gene;spot=item.Rect;break;}
  if(gene!=hoveredCreatureGene){hoveredCreatureGene=gene;creatureGeneHoverSince=elapsed;}
  if(gene<0||elapsed-creatureGeneHoverSince<.4)return;
  var rows=SetupWrap(SettingHelp.For("Gene"+gene),46);int width=365,height=58+rows.Count*18,x=Math.Max(12,observationCardRect.X-width-12),y=Math.Clamp(spot.Y,12,p.H-height-12);
  p.RoundBox(x+2,y+3,width,height,10,new Col(31,54,40,35));p.RoundBox(x,y,width,height,10,new Col(250,253,241,246));p.Text(Genetics.Names[gene],x+16,y+13,13,text);
  for(int i=0;i<rows.Count;i++)p.Text(rows[i],x+16,y+39+i*18,12,muted);
 }
}
