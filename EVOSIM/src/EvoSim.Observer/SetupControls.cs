using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 sealed record SetupItem(Entry Entry,Entry? Upper=null);
 sealed record SetupControl(SetupItem Item,Rect Card,Rect Track);
 sealed record SetupHelpSpot(Entry Entry,Rect Rect);
 readonly List<SetupControl> setupControls=new();readonly List<SetupHelpSpot> setupHelpSpots=new();List<SetupItem> setupItems=new();
 Rect setupViewport,setupScrollTrack,setupTooltipRect;float setupScroll;int setupContentHeight;SetupControl? draggingSetup;bool draggingUpper,draggingSetupScroll;string? focusedSetupKey,hoverSetupKey;double hoverSetupSince;SetupHelpSpot? pinnedSetupHelp;
 void ResetSetupScroll(){setupScroll=0;draggingSetup=null;draggingSetupScroll=false;focusedSetupKey=null;hoverSetupKey=null;pinnedSetupHelp=null;setupTooltipRect=default;}
 static List<SetupItem> MakeSetupItems(List<Entry> entries){var result=new List<SetupItem>();for(int i=0;i<entries.Count;i++){var entry=entries[i];if(entry.Gene>=0&&!entry.Upper&&i+1<entries.Count&&entries[i+1].Gene==entry.Gene){result.Add(new SetupItem(entry,entries[++i]));}else if(!entry.Upper)result.Add(new SetupItem(entry));}return result;}
 static bool IsSetupSlider(Entry entry)=>!entry.Whole&&entry.Max<=1&&entry.Max>entry.Min;
 static bool IsProbability(Entry entry)=>entry.Key is "MutationRate" or "LargeChance" or "SwimmerChance";
 static double SliderPosition(Entry entry,double value){if(entry.Key=="MutationRate")return value<=.01?Math.Clamp(value/.01*.8,0,.8):.8+Math.Sqrt((value-.01)/.99)*.2;return Math.Clamp((value-entry.Min)/(entry.Max-entry.Min),0,1);}
 static double SliderValue(Entry entry,double position){position=Math.Clamp(position,0,1);if(entry.Key=="MutationRate")return position<=.8?position/.8*.01:.01+Math.Pow((position-.8)/.2,2)*.99;return entry.Min+position*(entry.Max-entry.Min);}
 static bool IsBreedingMode(Entry entry)=>entry.Key is "HerbBreedingMode" or "CarnBreedingMode";
 static bool IsMapSize(Entry entry)=>entry.Key=="MapSize";
 static double NextMapSize(Entry entry)=>Parse(entry.Read()) switch{128=>192,192=>256,_=>128};
 static string ModeName(int mode)=>mode switch{0=>"Kapalı",2=>"Eşeysiz",_=>"Eşeyli"};
 static string SetupValue(Entry entry)=>IsProbability(entry)?$"%{Parse(entry.Read())*100:0.###}":WorldCalendar.UsesYears(entry.Key)?$"{Parse(entry.Read())/365:0.###} yıl":entry.Read();
 void DrawSetupCard(SetupItem item,Rect r){
  var entry=item.Entry;bool focused=focusedSetupKey==entry.Key;p.RoundBox(r.X,r.Y,r.W,r.H,focused?Col.Hex(0xE4EBDB):Col.Hex(0xFFFEF7),10);
  string title=item.Upper!=null?Genetics.Names[entry.Gene]:IsBreedingMode(entry)?"Üreme biçimi":entry.Label.Replace(" · 0 kapalı / 1 açık","");var labelLines=SetupWrap(title,39).Take(2).ToArray();for(int j=0;j<labelLines.Length;j++)p.Text(labelLines[j],r.X+15,r.Y+10+j*16,13,Col.Hex(0x365642));
  var infoRect=new Rect(r.X+r.W-35,r.Y+9,24,25);bool infoHot=infoRect.Contains(mouseX,mouseY);p.Disk(infoRect.X+12,infoRect.Y+12,9,infoHot?Col.Hex(0xCADAC4):Col.Hex(0xE9EEDF));p.Text("i",infoRect.X+10,infoRect.Y+4,12,Col.Hex(0x668064));
  if(FullyInsideSetup(infoRect))setupHelpSpots.Add(new SetupHelpSpot(entry,infoRect));
  if(item.Upper!=null){
   var track=new Rect(r.X+107,r.Y+60,r.W-214,7);double lo=Parse(entry.Read()),hi=Parse(item.Upper.Read());p.RoundBox(track.X,track.Y,track.W,track.H,Col.Hex(0xDDE6D7),3);p.RoundBox(track.X+track.W*(float)lo,track.Y,track.W*(float)(hi-lo),track.H,Col.Hex(0x90AD88),3);
   DrawSetupHandle(track,lo,focused&&!draggingUpper);DrawSetupHandle(track,hi,focused&&draggingUpper);
   var lowRect=new Rect(r.X+15,r.Y+47,78,32);var highRect=new Rect(r.X+r.W-93,r.Y+47,78,32);
   if(FullyInsideSetup(lowRect))SetupButton(entry.Read(),lowRect,()=>{focusedSetupKey=entry.Key;draggingUpper=false;Edit(entry);});
   if(FullyInsideSetup(highRect))SetupButton(item.Upper.Read(),highRect,()=>{focusedSetupKey=entry.Key;draggingUpper=true;Edit(item.Upper);});
   setupControls.Add(new SetupControl(item,r,track));
  }else if(IsSetupSlider(entry)){
   var track=new Rect(r.X+18,r.Y+60,r.W-155,7);double position=SliderPosition(entry,Parse(entry.Read()));p.RoundBox(track.X,track.Y,track.W,track.H,Col.Hex(0xDEE7D6),3);p.RoundBox(track.X,track.Y,track.W*(float)position,track.H,Col.Hex(0x95B18A),3);DrawSetupHandle(track,position,focused);
   if(entry.Key=="MutationRate"){p.Line(track.X+track.W*.8f,track.Y+10,track.X+track.W*.8f,track.Y+14,Col.Hex(0x9EB28E));p.Text("%0",track.X,r.Y+76,9,Col.Hex(0x85937A));p.Text("%1",track.X+track.W*.8f-7,r.Y+76,9,Col.Hex(0x85937A));p.Text("%100",track.X+track.W-21,r.Y+76,9,Col.Hex(0x85937A));}
   else {p.Text(entry.Min.ToString("0.###"),track.X,r.Y+76,9,Col.Hex(0x85937A));p.Text(entry.Max.ToString("0.###"),track.X+track.W-11,r.Y+76,9,Col.Hex(0x85937A));}
   var valueRect=new Rect(r.X+r.W-118,r.Y+47,103,32);if(FullyInsideSetup(valueRect))SetupButton(SetupValue(entry),valueRect,()=>{focusedSetupKey=entry.Key;Edit(entry);});setupControls.Add(new SetupControl(item,r,track));
  }else{
   string hint=IsMapSize(entry)?$"{Math.Pow(Parse(entry.Read())/128,2):0.##} × referans alan":IsBreedingMode(entry)?"Dokunarak değiştir":entry.Key.EndsWith("Lifespan")?"0 = yaşlılıktan ölmez":entry.Whole&&entry.Max==1?"0 kapalı  ·  1 açık":entry.Whole?"Tam sayı  ·  "+entry.Hint:"Sayısal değer  ·  "+entry.Hint;p.Text(hint,r.X+16,r.Y+62,11,Col.Hex(0x809078));
   var valueRect=new Rect(r.X+r.W-132,r.Y+47,117,32);if(FullyInsideSetup(valueRect))SetupButton(IsMapSize(entry)?$"{entry.Read()} × {entry.Read()}":IsBreedingMode(entry)?ModeName((int)Parse(entry.Read())):entry.Whole&&entry.Max==1?(Parse(entry.Read())>0?"Açık":"Kapalı"):SetupValue(entry),valueRect,()=>{focusedSetupKey=entry.Key;if(IsMapSize(entry))WriteSetupValue(entry,NextMapSize(entry));else if(IsBreedingMode(entry))WriteSetupValue(entry,((int)Parse(entry.Read())+1)%3);else if(entry.Whole&&entry.Max==1)WriteSetupValue(entry,Parse(entry.Read())>0?0:1);else Edit(entry);});setupControls.Add(new SetupControl(item,r,default));
  }
 }
 void DrawSetupHandle(Rect track,double position,bool selectedHandle){float xx=track.X+(float)position*track.W,yy=track.Y+3;p.Disk(xx,yy+1,9,new Col(39,69,43,30));p.Disk(xx,yy,8,selectedHandle?Col.Hex(0x597B58):Col.Hex(0xFEFFF8));p.Ring(xx,yy,8,Col.Hex(0x83A37A));p.Disk(xx,yy,3,Col.Hex(0x93AF88));}
 bool FullyInsideSetup(Rect r)=>r.Y>=setupViewport.Y&&r.Y+r.H<=setupViewport.Y+setupViewport.H;
 bool OnSetupTrack(SetupControl control,int x,int y)=>control.Track.W>0&&setupViewport.Contains(x,y)&&new Rect(control.Track.X-9,control.Track.Y-13,control.Track.W+18,30).Contains(x,y);
 bool WriteSetupValue(Entry entry,double value){try{value=Math.Round(Math.Clamp(value,entry.Min,entry.Max),6);entry.Write(value.ToString("0.######",System.Globalization.CultureInfo.InvariantCulture));return true;}catch(ArgumentException ex){Say(ex.Message);return false;}}
 void SetSetupSlider(SetupControl control,int x){var entry=draggingUpper&&control.Item.Upper!=null?control.Item.Upper:control.Item.Entry;double value=SliderValue(entry,(double)(x-control.Track.X)/control.Track.W);if(control.Item.Upper!=null){double lo=Parse(control.Item.Entry.Read()),hi=Parse(control.Item.Upper.Read());value=draggingUpper?Math.Max(value,lo):Math.Min(value,hi);}WriteSetupValue(entry,value);}
 static bool ChooseUpperHandle(SetupControl control,int x){if(control.Item.Upper==null)return false;float lo=control.Track.X+Parse(control.Item.Entry.Read())*control.Track.W,hi=control.Track.X+Parse(control.Item.Upper.Read())*control.Track.W;return Math.Abs(hi-lo)<.1f?x>=lo:Math.Abs(x-hi)<Math.Abs(x-lo);}
 void NudgeSetupSlider(SetupControl control,int direction){var entry=draggingUpper&&control.Item.Upper!=null?control.Item.Upper:control.Item.Entry;double step=entry.Key=="MutationRate"?.00001:.001;double value=Parse(entry.Read())+direction*step;if(control.Item.Upper!=null)value=draggingUpper?Math.Max(value,Parse(control.Item.Entry.Read())):Math.Min(value,Parse(control.Item.Upper.Read()));WriteSetupValue(entry,value);}
 void ScrollSetup(float amount){setupScroll=Math.Clamp(setupScroll+amount,0,Math.Max(0,setupContentHeight-setupViewport.H));hoverSetupKey=null;pinnedSetupHelp=null;}
 bool HandleSetupEvent(Event ev){
  if(!setupOpen||naming){draggingSetup=null;draggingSetupScroll=false;return false;}
  switch(ev.Type){
   case 0x400:
    mouseX=ev.B;mouseY=ev.C;if(draggingSetup!=null)SetSetupSlider(draggingSetup,mouseX);else if(draggingSetupScroll)MoveSetupScrollbar(mouseY);return true;
   case 0x402:
    if(ev.Button==1&&(draggingSetup!=null||draggingSetupScroll)){draggingSetup=null;draggingSetupScroll=false;return true;}break;
   case 0x401:
    mouseX=ev.B;mouseY=ev.C;if(ev.Button!=1)return false;
    if(pinnedSetupHelp!=null&&pinnedSetupHelp.Rect.Contains(mouseX,mouseY)){pinnedSetupHelp=null;setupTooltipRect=default;return true;}
    if(setupTooltipRect.W>0&&setupTooltipRect.Contains(mouseX,mouseY)){pinnedSetupHelp??=setupHelpSpots.FirstOrDefault(s=>s.Entry.Key==hoverSetupKey);return true;}
    var info=setupHelpSpots.FirstOrDefault(s=>s.Rect.Contains(mouseX,mouseY));if(info!=null){pinnedSetupHelp=pinnedSetupHelp?.Entry.Key==info.Entry.Key?null:info;focusedSetupKey=info.Entry.Key;return true;}
    pinnedSetupHelp=null;
    if(setupScrollTrack.W>0&&setupScrollTrack.Contains(mouseX,mouseY)){draggingSetupScroll=true;MoveSetupScrollbar(mouseY);return true;}
    var clicked=setupControls.FirstOrDefault(c=>OnSetupTrack(c,mouseX,mouseY));if(clicked!=null){draggingSetup=clicked;focusedSetupKey=clicked.Item.Entry.Key;draggingUpper=ChooseUpperHandle(clicked,mouseX);SetSetupSlider(clicked,mouseX);return true;}break;
   case 0x403:
    if(setupTooltipRect.W>0&&setupTooltipRect.Contains(mouseX,mouseY))return true;if(!setupViewport.Contains(mouseX,mouseY))return true;int delta=ev.C==1?-ev.B:ev.B;var hovered=setupControls.FirstOrDefault(c=>OnSetupTrack(c,mouseX,mouseY));if(hovered!=null&&delta!=0){focusedSetupKey=hovered.Item.Entry.Key;draggingUpper=ChooseUpperHandle(hovered,mouseX);NudgeSetupSlider(hovered,Math.Sign(delta));}else ScrollSetup(-delta*60);return true;
   case 0x300:
    if(ev.B==27&&pinnedSetupHelp!=null){pinnedSetupHelp=null;setupTooltipRect=default;hoverSetupKey=null;return true;}
    if(ev.B==9){if(setupItems.Count==0)return true;int index=setupItems.FindIndex(i=>i.Entry.Key==focusedSetupKey);bool reverse=(ev.C&3)!=0;index=index<0?(reverse?setupItems.Count-1:0):(index+(reverse?-1:1)+setupItems.Count)%setupItems.Count;focusedSetupKey=setupItems[index].Entry.Key;draggingUpper=false;int top=index/2*100;if(top<setupScroll)setupScroll=top;if(top+92>setupScroll+setupViewport.H)setupScroll=Math.Min(Math.Max(0,setupContentHeight-setupViewport.H),top+92-setupViewport.H);pinnedSetupHelp=null;return true;}
    if(ev.B==1073741899){ScrollSetup(-setupViewport.H*.8f);return true;}if(ev.B==1073741902){ScrollSetup(setupViewport.H*.8f);return true;}
    var focused=setupControls.FirstOrDefault(c=>c.Item.Entry.Key==focusedSetupKey);if(focused==null)return false;
    if(ev.B==105){var focusedInfo=setupHelpSpots.FirstOrDefault(s=>s.Entry.Key==focusedSetupKey);if(focusedInfo!=null)pinnedSetupHelp=pinnedSetupHelp==null?focusedInfo:null;return true;}
    if(ev.B==13&&IsMapSize(focused.Item.Entry)){WriteSetupValue(focused.Item.Entry,NextMapSize(focused.Item.Entry));return true;}
    if(ev.B==13&&IsBreedingMode(focused.Item.Entry)){WriteSetupValue(focused.Item.Entry,((int)Parse(focused.Item.Entry.Read())+1)%3);return true;}
    if(ev.B==13){Edit(draggingUpper&&focused.Item.Upper!=null?focused.Item.Upper:focused.Item.Entry);return true;}
    if(ev.B==1073741906||ev.B==1073741905){if(focused.Item.Upper!=null){draggingUpper=ev.B==1073741906;return true;}}
    if(focused.Track.W>0&&(ev.B==1073741904||ev.B==1073741903)){NudgeSetupSlider(focused,ev.B==1073741903?1:-1);return true;}break;
  }return false;
 }
 void MoveSetupScrollbar(int y){if(setupScrollTrack.H<=0)return;float fraction=Math.Clamp((float)(y-setupScrollTrack.Y)/setupScrollTrack.H,0,1);setupScroll=fraction*Math.Max(0,setupContentHeight-setupViewport.H);pinnedSetupHelp=null;}
 static List<string> SetupWrap(string text,int maxChars){var lines=new List<string>();string current="";foreach(string word in text.Split(' ')){if(current.Length+word.Length+1>maxChars&&current.Length>0){lines.Add(current);current="";}current+=(current.Length==0?"":" ")+word;}if(current.Length>0)lines.Add(current);return lines;}
 void DrawSetupHelp(){
  setupTooltipRect=default;
  if(!setupOpen||naming){hoverSetupKey=null;return;}
  var hovered=setupHelpSpots.FirstOrDefault(s=>s.Rect.Contains(mouseX,mouseY));string? key=hovered?.Entry.Key;if(key!=hoverSetupKey){hoverSetupKey=key;hoverSetupSince=elapsed;}
  var spot=pinnedSetupHelp??(hovered!=null&&elapsed-hoverSetupSince>=.4?hovered:null);if(spot==null)return;
  var entry=spot.Entry;var lines=SetupWrap(SettingHelp.For(entry.Key),49);int width=386,height=94+lines.Count*18;bool probability=IsProbability(entry);if(probability)height+=20;
  int x=Math.Clamp(spot.Rect.X-width+26,16,p.W-width-16),y=spot.Rect.Y+34;if(y+height>p.H-16)y=spot.Rect.Y-height-8;y=Math.Clamp(y,12,p.H-height-12);setupTooltipRect=new Rect(x,y,width,height);
  p.RoundBox(x+2,y+4,width,height,new Col(31,54,40,45),11);p.RoundBox(x,y,width,height,new Col(250,253,241,247),11);string title=entry.Gene>=0?Genetics.Names[entry.Gene]:entry.Label;var titleLines=SetupWrap(title,43);p.Text(titleLines[0],x+17,y+13,13,Col.Hex(0x36513D));int bodyY=y+40;
  for(int i=0;i<lines.Count;i++)p.Text(lines[i],x+17,bodyY+i*18,12,Col.Hex(0x4A624B));int bottom=bodyY+lines.Count*18+9;
  string boundary=entry.Gene>=0?"Alt–üst aralığından rastgele kurucu gen":"İzin verilen aralık: "+entry.Hint;p.Text(boundary,x+17,bottom,11,Col.Hex(0x7B8E70));
  if(probability){p.Text($"{entry.Read()} ham değer = {SetupValue(entry)}",x+17,bottom+18,11,Col.Hex(0x4E7852));bottom+=20;}
  p.Text(pinnedSetupHelp!=null?"Bilgi sabitlendi · kapatmak için i simgesine dokun":"Sabitlemek için i simgesine dokun",x+17,bottom+19,10,Col.Hex(0x91A086));
 }
 void SetupControlsSmoke(){
  void Check(bool ok,string why){if(!ok)throw new Exception("Setup controls: "+why);}
  var oldDraft=draft;bool oldOpen=setupOpen,oldNaming=naming,oldPaused=paused;int oldTab=setupTab,oldX=mouseX,oldY=mouseY;float oldScroll=setupScroll;double oldElapsed=elapsed;string oldNameText=nameText,oldEditTitle=editTitle;var oldEditSetter=editSetter;
  try{
   Check(SettingHelp.MissingFields().Length==0,"every model setting has help: "+string.Join(", ",SettingHelp.MissingFields()));
   elapsed=Math.Max(elapsed,toastUntil+1);OpenSetup();draft=WorldConfig.Preset(0);setupTab=3;ResetSetupScroll();Layout();Draw(1);
   var mutation=setupControls.Single(c=>c.Item.Entry.Key=="MutationRate");Check(Math.Abs(SliderValue(mutation.Item.Entry,SliderPosition(mutation.Item.Entry,.001))-.001)<1e-8,"fine mutation slider round-trip");Check(Math.Abs(Parse(mutation.Item.Entry.Read())-.001)<1e-8,"opening slider preserves 0.1% mutation");
   mouseX=mutation.Track.X+(int)(mutation.Track.W*SliderPosition(mutation.Item.Entry,.001));mouseY=mutation.Track.Y;float before=draft.MutationRate;Check(HandleSetupEvent(new Event{Type=0x403,B=1}),"wheel handled");Check(Math.Abs(draft.MutationRate-before-.00001)<1e-7,"mutation wheel fine step");
   int dragX=mutation.Track.X+(int)(mutation.Track.W*.4);HandleSetupEvent(new Event{Type=0x401,Button=1,B=dragX,C=mutation.Track.Y});HandleSetupEvent(new Event{Type=0x400,A=1,B=mutation.Track.X+(int)(mutation.Track.W*.64),C=mutation.Track.Y});HandleSetupEvent(new Event{Type=0x402,Button=1});Check(draft.MutationRate>.007&&draft.MutationRate<.009,"mouse drag adjusts mutation");
   draft.MutationRate=.001f;Edit(mutation.Item.Entry);Check(nameText=="0.1"&&editTitle.Contains("%"),"probability editor uses percent");nameText="0.2";CommitName();Check(Math.Abs(draft.MutationRate-.002f)<1e-8,"percentage converted to raw fraction");
   draft.MutationRate=.001f;var help=setupHelpSpots.Single(s=>s.Entry.Key=="MutationRate");mouseX=help.Rect.X+10;mouseY=help.Rect.Y+10;Draw(1);Check(setupTooltipRect.W==0,"tooltip waits before opening");elapsed+=.41;Draw(1);Check(setupTooltipRect.W>0,"tooltip appears after hover delay");HandleSetupEvent(new Event{Type=0x401,Button=1,B=help.Rect.X+10,C=help.Rect.Y+10});Check(pinnedSetupHelp!=null,"information click pins help");Draw(1);DrawSetupHelp();
   float protectedMutation=draft.MutationRate;mouseX=setupTooltipRect.X+40;mouseY=setupTooltipRect.Y+70;HandleSetupEvent(new Event{Type=0x403,B=1});Check(draft.MutationRate==protectedMutation,"tooltip blocks wheel on obscured control");Check(HandleSetupEvent(new Event{Type=0x300,B=27})&&setupOpen&&pinnedSetupHelp==null,"escape closes help first");HandleSetupEvent(new Event{Type=0x401,Button=1,B=help.Rect.X+10,C=help.Rect.Y+10});Draw(1);p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v03-setup/smoke"),"setup-help.bmp"));
   setupTab=1;ResetSetupScroll();Draw(1);p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v03-setup/smoke"),"setup-gene-ranges.bmp"));var range=setupControls.First(c=>c.Item.Upper!=null);double max=Parse(range.Item.Upper!.Read());draggingUpper=false;SetSetupSlider(range,range.Track.X+range.Track.W);Check(Parse(range.Item.Entry.Read())<=max,"range handles cannot cross");bool rejected=false;try{range.Item.Entry.Write("1");}catch(ArgumentException){rejected=true;}Check(rejected,"numeric min cannot cross max");Check(ChooseUpperHandle(range,range.Track.X+range.Track.W),"coincident handles can separate upward");
   focusedSetupKey=range.Item.Entry.Key;HandleSetupEvent(new Event{Type=0x300,B=1073741906});Check(draggingUpper,"keyboard selects upper handle");HandleSetupEvent(new Event{Type=0x300,B=1073741904});Check(Parse(range.Item.Upper.Read())>=Parse(range.Item.Entry.Read()),"keyboard maintains range");
   Check(setupControls.Single(c=>c.Item.Entry.Key=="HerbLifespan").Track.W==0,"lifespan remains numeric");
   draft=WorldConfig.Preset(0);setupTab=1;ResetSetupScroll();Draw(1);ScrollSetup(10000);Draw(1);var breeding=setupControls.Single(c=>c.Item.Entry.Key=="HerbBreedingMode");focusedSetupKey=breeding.Item.Entry.Key;HandleSetupEvent(new Event{Type=0x300,B=13});Check(draft.HerbBreedingMode==2&&!naming,"mode cycles with a readable control");draft.HerbBreedingMode=1;Draw(1);p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v04-ui"),"setup-reproduction.bmp"));
   setupTab=6;ResetSetupScroll();Draw(1);p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v04-ui"),"setup-trees.bmp"));
   setupTab=5;ResetSetupScroll();Draw(1);mouseX=setupViewport.X+3;mouseY=setupViewport.Y+3;HandleSetupEvent(new Event{Type=0x403,B=-3});Check(setupScroll>0,"form scrolls vertically");Draw(1);Check(setupControls.All(c=>c.Card.Y+c.Card.H>=setupViewport.Y),"offscreen controls omitted");p.Screenshot(Path.Combine(Path.GetFullPath(Arg("--out")??"work/v03-setup/smoke"),"setup-scroll.bmp"));
   float savedScroll=setupScroll;mouseX=5;mouseY=5;HandleSetupEvent(new Event{Type=0x403,B=-3});Check(setupScroll==savedScroll,"wheel outside form leaves scroll unchanged");naming=true;Check(!HandleSetupEvent(new Event{Type=0x300,B=13}),"naming stays in root event handler");
  }finally{draft=oldDraft;setupOpen=oldOpen;naming=oldNaming;paused=oldPaused;setupTab=oldTab;setupScroll=oldScroll;mouseX=oldX;mouseY=oldY;elapsed=oldElapsed;draggingSetup=null;draggingSetupScroll=false;pinnedSetupHelp=null;hoverSetupKey=null;setupTooltipRect=default;nameText=oldNameText;editTitle=oldEditTitle;editSetter=oldEditSetter;}
 }
}
