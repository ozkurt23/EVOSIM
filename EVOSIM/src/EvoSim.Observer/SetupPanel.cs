using System.Globalization;
using System.Reflection;
using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 bool setupOpen; WorldConfig draft=new(); int formSeed=42,setupTab; string worldName="Yeni vadi",editTitle="Dünya adı"; Action<string>? editSetter;
 static readonly string[] tabs={"Dünya","Otçul","Etçil","Kalıtım","Beden","İhtiyaç","Kaynak","Üreme","Etkileşim"};
 sealed record Entry(string Label,Func<string> Read,Action<string> Write,string Hint,string Key="",double Min=0,double Max=0,bool Whole=false,int Gene=-1,bool Upper=false);
 void OpenSetup(){CloseObservationPanels();HideTimeline();draft=world.Config.Copy();draft.GeneratorVersion=2;formSeed=world.Seed;worldName=currentSlot?.Name??"Yeni vadi";setupOpen=true;menuOpen=false;libraryOpen=false;paused=true;ResetSetupScroll();}
 void Edit(Entry entry){
  if(IsProbability(entry)){editTitle=entry.Key switch{"MutationRate"=>"Mutasyon olasılığı (%)","LargeChance"=>"Büyük sapma olasılığı (%)",_=>"Nadir yüzücü olasılığı (%)"};editSetter=s=>entry.Write((Parse(s)/100).ToString("0.########",CultureInfo.InvariantCulture));nameText=(Parse(entry.Read())*100).ToString("0.######");}
  else if(WorldCalendar.UsesYears(entry.Key)){editTitle=entry.Label;string original=entry.Read();nameText=WorldCalendar.YearInput(Parse(original));string displayed=nameText;editSetter=s=>entry.Write(s==displayed?original:((double)Parse(s)*365).ToString("G17",CultureInfo.InvariantCulture));}
  else{editTitle=entry.Label;editSetter=entry.Write;nameText=entry.Read();}
  naming=true;pinnedSetupHelp=null;setupTooltipRect=default;SDL.SDL_StartTextInput();
 }
 List<Entry> Entries(){var entries=new List<Entry>();string group=tabs[setupTab];
  if(group=="Otçul"||group=="Etçil"){
   var lo=group=="Otçul"?draft.HerbMin:draft.CarnMin;var hi=group=="Otçul"?draft.HerbMax:draft.CarnMax;
   for(int i=0;i<6;i++){int g=i;foreach(bool lower in new[]{true,false}){var array=lower?lo:hi;entries.Add(new Entry(Genetics.Names[g]+(lower?" · Alt":" · Üst"),()=>array[g].ToString("0.######"),s=>{float v=Parse(s);if(v<0||v>1)throw new ArgumentException("Değer 0–1 arasında olmalı.");if(lower&&v>hi[g]||!lower&&v<lo[g])throw new ArgumentException("Alt sınır üst sınırı aşamaz.");array[g]=v;},"Kurucu gen aralığı","Gene"+g,0,1,false,g,!lower));}}
  }
  foreach(var field in typeof(WorldConfig).GetFields()){
   var setting=field.GetCustomAttribute<SettingAttribute>();if(setting==null||setting.Group!=group)continue;
   entries.Add(new Entry(WorldCalendar.SettingLabel(field.Name,setting.Label),()=>Convert.ToDouble(field.GetValue(draft)).ToString("0.######"),s=>{float v=Parse(s);if(v<setting.Min||v>setting.Max)throw new ArgumentException($"{(WorldCalendar.UsesYears(field.Name)?setting.Min/365:setting.Min):0.######}–{(WorldCalendar.UsesYears(field.Name)?setting.Max/365:setting.Max):0.######} aralığında olmalı.");if(field.FieldType==typeof(int)&&v!=(int)v)throw new ArgumentException("Tam sayı gerekli.");if(field.Name=="MapSize"&&v!=128&&v!=192&&v!=256)throw new ArgumentException("Harita boyutu 128, 192 veya 256 olmalı.");ValidateSetupPair(field.Name,v);if(field.FieldType==typeof(int))field.SetValue(draft,(int)v);else field.SetValue(draft,v);},$"{(WorldCalendar.UsesYears(field.Name)?setting.Min/365:setting.Min):0.######}–{(WorldCalendar.UsesYears(field.Name)?setting.Max/365:setting.Max):0.######}",field.Name,setting.Min,setting.Max,field.FieldType==typeof(int)));
  }return entries;
 }
 void ValidateSetupPair(string key,float value){
  if(key=="TreeRadiusMin"&&value>draft.TreeRadiusMax||key=="TreeRadiusMax"&&value<draft.TreeRadiusMin||key=="InitialWaterMin"&&value>draft.InitialWaterMax||key=="InitialWaterMax"&&value<draft.InitialWaterMin||key=="InitialCooldownMin"&&value>draft.InitialCooldownMax||key=="InitialCooldownMax"&&value<draft.InitialCooldownMin||key=="FounderAgeMin"&&value>draft.FounderAgeMax||key=="FounderAgeMax"&&value<draft.FounderAgeMin)throw new ArgumentException("Alt sınır üst sınırı aşamaz.");
  if(key=="CriticalNeed"&&(value>draft.HungerOnset||value>draft.ThirstOnset)||key=="HungerOnset"&&value<draft.CriticalNeed||key=="ThirstOnset"&&value<draft.CriticalNeed)throw new ArgumentException("Kritik eşik ihtiyaç başlangıcını aşamaz.");
 }
 static float Parse(string value){if(!float.TryParse(value.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out float v)||!float.IsFinite(v))throw new ArgumentException("Geçerli bir sayı yaz.");return v;}
 void MakeWorld(){try{draft.Validate();var next=new World(formSeed,draft);if(!SaveCurrent())return;world=next;timeline=TimelineSession.Create(world);timelineOpen=draggingTimeline=false;hasActiveWorld=true;currentSlot=null;FitWorld(1.25f);selected=0;follow=false;accumulator=0;speed=1;currentSlot=store.Save(world,worldName,null,view.Zoom,view.CenterX,view.CenterY,1,timelineArchive:timeline.SerializeArchive());setupOpen=false;mainMenu=false;paused=false;Say(worldName+" oluşturuldu.");}catch(Exception ex){Say(ex.Message);}}
 void SetupPanel(){
  buttons.Clear();setupControls.Clear();setupHelpSpots.Clear();
  p.Box(0,0,p.W,p.H,new Col(29,49,40,165));int width=Math.Min(1120,p.W-64),height=Math.Min(840,p.H-64),x=(p.W-width)/2,y=(p.H-height)/2;
  p.RoundBox(x+3,y+8,width,height,new Col(22,43,31,45),18);p.RoundBox(x,y,width,height,Col.Hex(0xF5F2E9),18);
  p.Text("Yeni bir dünya",x+28,y+23,26,Col.Hex(0x29463B));p.Text("Koşulları seç, hayatın nasıl şekillendiğini izle.  ·  Takvim: 365 gün = 1 yıl.",x+29,y+64,14,Col.Hex(0x70806E));
  p.Text($"6 kalıtsal özellik   ·   30 → 8 → 5 karar ağı   ·   Mutasyon %{draft.MutationRate*100:0.###} / doğum",x+29,y+89,12,Col.Hex(0x718362));
  string[] presets={"Referans","Meyve bolluğu","Avcı baskısı","Yalnız otçul"};for(int i=0;i<4;i++){int index=i;SetupButton(presets[i],new Rect(x+28+i*145,y+124,134,36),()=>{int size=draft.MapSize;draft=WorldConfig.Preset(index);draft.MapSize=size;ResetSetupScroll();});}
  SetupButton($"Tohum · {formSeed}",new Rect(x+width-416,y+124,177,36),()=>Edit(new Entry("Harita tohumu",()=>formSeed.ToString(),s=>{float v=Parse(s);if(v<1||v>999999||v!=(int)v)throw new ArgumentException("1–999999 arasında tam sayı.");formSeed=(int)v;},"")));
  SetupButton("↻ Rastgele",new Rect(x+width-227,y+124,199,36),()=>formSeed=Random.Shared.Next(1,1000000));
  int tabWidth=(width-56)/tabs.Length;for(int i=0;i<tabs.Length;i++){int tab=i;SetupButton(tabs[i],new Rect(x+28+i*tabWidth,y+184,tabWidth-5,35),()=>{setupTab=tab;ResetSetupScroll();},setupTab==i);}
  p.Text(tabs[setupTab]+" koşulları",x+29,y+236,14,Col.Hex(0x426151));p.Text("i  Ayrıntı için üzerine gel   ·   Kaydırarak diğer ayarlar",x+width-420,y+238,11,Col.Hex(0x7D8B76));
  setupViewport=new Rect(x+26,y+267,width-59,height-353);
  var entries=Entries();setupItems=MakeSetupItems(entries);int cardWidth=(setupViewport.W-14)/2;
  setupContentHeight=((setupItems.Count+1)/2)*100;setupScroll=Math.Clamp(setupScroll,0,Math.Max(0,setupContentHeight-setupViewport.H));
  p.Clip(setupViewport);
  for(int i=0;i<setupItems.Count;i++){
   int yy=setupViewport.Y+i/2*100-(int)setupScroll;if(yy+92<setupViewport.Y||yy>setupViewport.Y+setupViewport.H)continue;
   DrawSetupCard(setupItems[i],new Rect(setupViewport.X+i%2*(cardWidth+14),yy,cardWidth,92));
  }
  p.Unclip();
  if(setupContentHeight>setupViewport.H){float track=setupViewport.H,thumb=Math.Max(36,track*track/setupContentHeight),position=setupScroll/(setupContentHeight-setupViewport.H)*(track-thumb);setupScrollTrack=new Rect(x+width-22,setupViewport.Y,9,setupViewport.H);p.RoundBox(setupScrollTrack.X,setupScrollTrack.Y,9,track,Col.Hex(0xE1E6D9),4);p.RoundBox(setupScrollTrack.X,setupScrollTrack.Y+position,9,thumb,Col.Hex(0x9CB29B),4);}else setupScrollTrack=default;
  int foot=y+height-65;p.Line(x+28,foot-18,x+width-28,foot-18,Col.Hex(0xDEE3D6));
  SetupButton(worldName.Length>28?worldName[..28]+"…":worldName,new Rect(x+28,foot,310,38),()=>Edit(new Entry("Yeni dünyanın adı",()=>worldName,s=>worldName=string.IsNullOrWhiteSpace(s)?"Yeni vadi":s.Trim(),"")));
  SetupButton(mainMenu?"Geri":"Devam et",new Rect(x+353,foot,132,38),()=>{setupOpen=false;paused=mainMenu;});
  if(!mainMenu)SetupButton("Dünyalar",new Rect(x+500,foot,133,38),()=>{setupOpen=false;libraryOpen=true;});
  SetupButton("Dünyayı oluştur",new Rect(x+width-239,foot,211,38),MakeWorld,true);
 }
 void SetupButton(string label,Rect rect,Action action,bool active=false){bool hover=rect.Contains(mouseX,mouseY);var ink=active?Col.Hex(0xF9F7ED):Col.Hex(0x46604C);var fill=active?Col.Hex(0x648369):Col.Hex(0xE8EBDD);if(hover)fill=active?Col.Hex(0x57775E):Col.Hex(0xDCE4D2);p.RoundBox(rect.X,rect.Y,rect.W,rect.H,fill,8);p.Text(label,rect.X+13,rect.Y+(rect.H-19)/2,13,ink);buttons.Add((rect,action));}
}
