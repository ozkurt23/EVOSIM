using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using EvoSim.Core;
using EvoSim.Observer;

CultureInfo.CurrentCulture=CultureInfo.InvariantCulture;
try { using var app=new ObserverApp(args); app.Run(); }
catch(Exception ex){Console.Error.WriteLine("EVOSIM başlatılamadı: "+ex);Environment.ExitCode=1;}

internal sealed partial class ObserverApp : IDisposable {
 readonly Painter p; readonly WorldView view; World world; readonly string[] args;
 readonly WorldStore store;WorldSlot? currentSlot,renameSlot;bool mainMenu=false,hasActiveWorld=true;bool libraryOpen=false,naming=false,saveAs=false;string nameText="";int libraryPage=0;double lastAutoSave;
 bool running=true,paused=false,follow=false,menuOpen=false,showOverview=false,details=false;int speed=1,selected,mouseX,mouseY;double accumulator,elapsed,resetArmedUntil,toastUntil,fpsTimer,performanceTime;int frames,perfTicks;float fps,achieved=1;string toast="";
 WorldSlot? slotActions;bool trashView;
 readonly List<(Rect Rect,Action Action)> buttons=new();
 readonly Col bg=Col.Hex(0xF4F1E7),panel=Col.Hex(0xE6EADB),text=Col.Hex(0x263E35),muted=Col.Hex(0x69796B),line=Col.Hex(0xCDD7C4),gold=Col.Hex(0xD8BD77),green=Col.Hex(0x61846B),red=Col.Hex(0xB96853),blue=Col.Hex(0x4F91A0);
 readonly bool capture;readonly string? capturePath;readonly int captureTicks;
 public ObserverApp(string[] args){this.args=args;capturePath=Arg("--capture");capture=capturePath!=null||args.Contains("--ui-smoke");captureTicks=int.TryParse(Arg("--ticks"),out int ticks)?ticks:0;int seed=int.TryParse(Arg("--seed"),out int s)?s:42;selected=int.TryParse(Arg("--select"),out int id)?id:0;
  if(capture)Environment.SetEnvironmentVariable("SDL_VIDEODRIVER","dummy");p=new Painter(capture);view=new(p);var startupConfig=new WorldConfig();if(int.TryParse(Arg("--map-size"),out int mapSize))startupConfig.MapSize=mapSize;if(int.TryParse(Arg("--herbivores"),out int herb))startupConfig.Herbivores=herb;if(int.TryParse(Arg("--carnivores"),out int carn))startupConfig.Carnivores=carn;if(int.TryParse(Arg("--trees"),out int trees))startupConfig.TreeCount=trees;world=new World(seed,startupConfig);FitWorld(1.25f);
  string dataRoot=Environment.GetEnvironmentVariable("EVOSIM_DATA_DIR")??Path.Combine(AppContext.BaseDirectory,"observations");
  store=new WorldStore(capture?Path.Combine(Path.GetFullPath(Arg("--out")??"work/capture"),"worlds"):Path.Combine(Path.GetDirectoryName(dataRoot)!,"worlds"));
  if(!capture){hasActiveWorld=false;mainMenu=true;paused=true;}
 }
 string? Arg(string name){int i=Array.IndexOf(args,name);return i>=0&&i+1<args.Length?args[i+1]:null;}
 public void Run(){
  if(args.Contains("--ui-smoke")){Smoke();return;}
  if(capture){CaptureWorld();return;}
  Console.WriteLine("EVOSIM 0.5.10 · kalıtsal ağ · seed="+world.Seed);
  var watch=Stopwatch.StartNew();double previous=watch.Elapsed.TotalSeconds;
  while(running){double now=watch.Elapsed.TotalSeconds,dt=now-previous;previous=now;elapsed=now;Layout();ProcessEvents();PumpLifeTrace();
   if(timeline?.IsSeeking==true){try{timeline.PumpSeek(6,100);SyncTimelineWorld();}catch(Exception ex){timeline.CancelSeek();SyncTimelineWorld();Say("Geçmiş açılamadı: "+ex.Message);}}
   if(hasActiveWorld&&!mainMenu&&now-lastAutoSave>=30){if(!SaveCurrent())paused=true;lastAutoSave=now;}
   if(!mainMenu&&!paused&&timeline?.IsSeeking!=true&&(world.Living>0||timeline?.IsPreviewingHistory==true))accumulator+=Math.Min(dt,.25)*speed;
   int steps=0;var budget=Stopwatch.StartNew();while(!mainMenu&&!paused&&timeline?.IsSeeking!=true&&accumulator>=.05&&steps<160&&budget.Elapsed.TotalMilliseconds<18){AdvanceWorld();accumulator=Math.Max(0,accumulator-.05);steps++;}
   if(paused)accumulator=0;perfTicks+=steps;performanceTime+=dt;if(performanceTime>=1){achieved=(float)(perfTicks*.05/performanceTime);performanceTime=0;perfTicks=0;}
   if(follow){var c=world.Find(selected);if(c!=null&&c.Alive){view.CenterX=c.X;view.CenterY=c.Y;}}
   frames++;fpsTimer+=dt;if(fpsTimer>=.6){fps=(float)(frames/fpsTimer);frames=0;fpsTimer=0;}
   Draw(paused?1:Math.Clamp((float)(accumulator/.05),0,1));SDL.SDL_RenderPresent(p.Renderer);
   if(watch.Elapsed.TotalSeconds-now<.004)Thread.Sleep(2);
  }
 }
 void Smoke(){
  void Check(bool ok,string message){if(!ok)throw new Exception("UI smoke: "+message);}
  void Key(int key){var e=new Event{Type=0x300,B=key};SDL.SDL_PushEvent(ref e);ProcessEvents();}
  void Click(int x,int y){var e=new Event{Type=0x401,Button=1,B=x,C=y};SDL.SDL_PushEvent(ref e);ProcessEvents();}
  Layout();Draw(1);elapsed=1;Click(p.W/2,p.H-43);Click(p.W/2,p.H-43);Check(speed==4,"speed button");Key(48);Check(speed==20,"20x speed");Key(32);Check(paused,"space pause");accumulator=4;long tick=world.Tick;Key(1073741903);Check(world.Tick==tick+1&&accumulator==0,"exact single step");
  var first=world.Creatures.First(c=>c.Alive);var point=view.Screen(first.X,first.Y);Click((int)point.X,(int)point.Y);Check(selected>0,"creature selection");Key(102);Check(follow,"follow");Key(105);Check(view.ShowSenses,"target overlay");Key(100);Check(world.Drought,"drought");for(int i=0;i<100;i++)AdvanceWorld();Draw(1);
  string dir=Path.GetFullPath(Arg("--out")??"artifacts/ui-smoke");Directory.CreateDirectory(dir);Environment.SetEnvironmentVariable("EVOSIM_DATA_DIR",dir);Export();Check(Directory.GetFiles(dir,"report.json",SearchOption.AllDirectories).Length>0,"JSON export");Check(Directory.GetFiles(dir,"population.csv",SearchOption.AllDirectories).Length>0,"CSV export");p.Screenshot(Path.Combine(dir,"selected.bmp"));
  string checkpointHash=world.StateHash();SaveCurrent();Check(currentSlot!=null,"world save");var savedSlot=currentSlot!;AdvanceWorld();OpenWorld(savedSlot);Check(world.StateHash()==checkpointHash,"world load exact state");store.Rename(savedSlot,"Deneme Çınar");Check(store.List().Any(s=>s.Name=="Deneme Çınar"),"Turkish world rename");
  tick=world.Tick;Key(114);Check(world.Tick==tick,"restart confirmation");Key(114);Check(world.Tick==0&&world.Seed==42,"same-seed restart");Key(27);Check(selected==0,"clear selection");Key(110);Check(setupOpen,"startup settings");draft=WorldConfig.Preset(3);draft.LandRegions=4;draft.HerbLifespan=0;draft.HerbAging=0;worldName="Ölümsüz vadi";MakeWorld();Check(!setupOpen&&world.Living==100&&world.Map.LandRegionCount()==4&&world.Config.HerbLifespan==0,"custom world creation");SaveCurrent();string customHash=world.StateHash();OpenWorld(currentSlot!);Check(world.StateHash()==customHash&&world.Config.HerbAging==0,"custom world saved");string menuHash=world.StateHash();Layout();Draw(1);Click((p.W-176)/2+145,p.H-51);Draw(1);Click((p.W-176)/2+50,p.H-115);Check(mainMenu&&!hasActiveWorld,"exit to menu");Key(32);Key(1073741903);Check(paused&&world.StateHash()==menuHash,"menu blocks simulation keys");Layout();Draw(1);p.Screenshot(Path.Combine(dir,"main-menu.bmp"));Click((p.W-820)/2+635,(p.H-680)/2+160);Check(!mainMenu&&hasActiveWorld&&!paused&&world.StateHash()==menuHash,"open from main menu");ExitWorld();Draw(1);Click((p.W-820)/2+650,(p.H-680)/2+48);Check(setupOpen&&mainMenu,"new world from menu");Key(27);Check(!setupOpen&&mainMenu,"cancel setup returns menu");SetupControlsSmoke();WorldDeletionSmoke();LegacyMigrationSmoke();LargeMapUiSmoke();TimelineUiSmoke();ObservationUiSmoke();JournalSmoke();ExportSmoke();MapLayerSmoke();WorldLibraryQuerySmoke();LifeTraceSmoke();CalendarDockSmoke();Console.WriteLine(JsonSerializer.Serialize(new{mode="ui-smoke",passed=true,checks=new[]{"calendar/year edit/immortality/9 stacked panels/map input/diet journal","map layers/count conservation/boundaries/full checkpoint/history/input","world library sorting/filtering/Turkish names/input/read-only","life trace/background thread/cancel/immutable publication/forward equivalence/past isolation","speed button","pause key","exact single step","creature pick","follow","target overlay","drought","JSON/CSV export","atomic report bundle/CSV roundtrip/culture/history/hash","confirmed reset","clear selection","20x speed","exact world load","Turkish rename","custom world creation","custom world saved","exit saves world","menu blocks simulation","menu opens exact save","new world and cancel","world delete/restore","sliders/tooltips/scroll","breeding mode and tree controls","128/192/256 create/select/zoom/save/preview","timeline scrub/cancel/playback/branch/autosave/restore","journal filters/scroll/modal/read-only/past isolation","resource pick and card input boundaries","contextual genealogy and real child node","population histograms and modal input isolation","life tab death accounting/exclusive breeding gates/history isolation","observation full checkpoint unchanged and historical data isolation","legacy migration branch (when fixture supplied)"}}));
 }
 void WorldDeletionSmoke(){var slot=store.List().First();string hash=store.Load(slot).StateHash();store.Trash(slot);if(store.List().Any(s=>s.Id==slot.Id)||!store.List(true).Any(s=>s.Id==slot.Id))throw new Exception("Delete listing failed");bool rejected=false;try{store.Save(world,slot.Name,slot,1,64,64,1);}catch(InvalidOperationException){rejected=true;}if(!rejected)throw new Exception("Deleted world resurrected");store.Restore(slot);if(store.Load(slot).StateHash()!=hash)throw new Exception("Restore changed world");}
 void LegacyMigrationSmoke(){
  string? fixture=Arg("--legacy-fixture");if(fixture==null)return;
  byte[] payload=File.ReadAllBytes(fixture);string directory=Path.Combine(Path.GetFullPath(Arg("--out")??"work/capture"),"migration-worlds");Directory.CreateDirectory(directory);
  var isolated=new WorldStore(directory);var source=new WorldSlot{Id=Guid.NewGuid().ToString("N"),Name="Eski aile vadisi",SavedUtc=DateTime.UtcNow.ToString("O"),Sha256=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(payload))};
  string path=Path.Combine(directory,source.Id+".evoworld");using(var w=new BinaryWriter(File.Create(path))){w.Write("EVOSIM-LIBRARY-1");w.Write(JsonSerializer.Serialize(source));w.Write(payload.Length);w.Write(payload);}
  var before=File.ReadAllBytes(path);var opened=isolated.OpenForCurrentVersion(source);
  if(!opened.Migrated||opened.Slot.Id==source.Id||opened.Slot.SourceWorldId!=source.Id||!before.SequenceEqual(File.ReadAllBytes(path)))throw new Exception("Legacy branch did not preserve source");
  if(isolated.Load(opened.Slot).StateHash()!=opened.World.StateHash())throw new Exception("Legacy branch changed state");
  var replay=TimelineSession.Open(opened.World,opened.TimelineArchive);replay.AdvancePlaybackOneTick();isolated.Save(replay.HeadWorld,opened.Slot.Name,opened.Slot,1,64,64,1,timelineArchive:replay.SerializeArchive());
  if(!before.SequenceEqual(File.ReadAllBytes(path))||isolated.List().Count!=2||isolated.OpenForCurrentVersion(opened.Slot).Migrated)throw new Exception("Legacy autosave overwrote or duplicated branch");
 }
 void FitWorld(float zoom=1){view.Bind(world);view.Zoom=zoom;view.CenterX=view.CenterY=world.Map.Size*.5f;follow=false;}
 void Layout(){view.Bind(world);p.Resize();view.View=new Rect(0,0,p.W,p.H);}
 void ProcessEvents(){
  while(SDL.SDL_PollEvent(out var ev)!=0){if(HandleLifeTraceEvent(ev)||HandleJournalEvent(ev)||HandleLineageEvent(ev)||HandlePopulationInsightsEvent(ev)||HandleSetupEvent(ev)||HandleTimelineEvent(ev)||HandleDockEvent(ev))continue;switch(ev.Type){
   case 0x100:if(SaveCurrent())running=false;else paused=true;break;
   case 0x303:if(naming&&nameText.Length<64)nameText+=ev.TextValue();break;
   case 0x400: {int oldX=mouseX,oldY=mouseY;mouseX=ev.B;mouseY=ev.C;uint state=(uint)ev.A;if(!mainMenu&&!setupOpen&&!libraryOpen&&!naming&&(state&4)!=0&&!layerLegend.Contains(mouseX,mouseY)&&view.View.Contains(mouseX,mouseY)){view.Pan(mouseX-oldX,mouseY-oldY);follow=false;}break;}
   case 0x401: if(ev.Button==1){mouseX=ev.B;mouseY=ev.C;var hit=buttons.LastOrDefault(b=>b.Rect.Contains(mouseX,mouseY));if(hit.Action!=null)hit.Action();else if(mainMenu){slotActions=null;}else if(!mainMenu&&!setupOpen&&!libraryOpen&&!naming&&!layerLegend.Contains(mouseX,mouseY)&&!observationCardRect.Contains(mouseX,mouseY)&&view.View.Contains(mouseX,mouseY)){PickObservation(mouseX,mouseY);}}break;
   case 0x403:if(!mainMenu&&!setupOpen&&!libraryOpen&&!naming&&!layerLegend.Contains(mouseX,mouseY)&&view.View.Contains(mouseX,mouseY)){view.ZoomAt(ev.C==1?-ev.B:ev.B,mouseX,mouseY);follow=false;}break;
   case 0x300:if(naming){if(ev.B==13)CommitName();else if(ev.B==27){naming=false;SDL.SDL_StopTextInput();}else if(ev.B==8&&nameText.Length>0)nameText=nameText[..^1];else if(ev.B==97&&(ev.C&0xC0)!=0)nameText="";break;}if(setupOpen){if(ev.B==27)setupOpen=false;break;}if(libraryOpen){if(ev.B==27)libraryOpen=false;break;}if(mainMenu)break;if(ev.Repeat!=0)break;switch(ev.B){case 27:selected=0;follow=false;ClearResourceSelection();break;case 32:paused=!paused;break;case 49:speed=1;break;case 50:speed=2;break;case 52:speed=4;break;case 56:speed=8;break;case 48:speed=20;break;case 108:HideTimeline();libraryOpen=!libraryOpen;paused=true;break;case 100:ToggleDrought();break;case 102:follow=!follow&&selected>0;break;case 105:view.ShowSenses=!view.ShowSenses;break;case 114:Restart(false);break;case 110:OpenSetup();break;case 115:Export();break;case 1073741903:paused=true;accumulator=0;if(timeline?.IsSeeking!=true)AdvanceWorld();break;case 104:FitWorld();break;case 118:CycleMapLayer();break;}break;
  }}
 }
 void Button(string label,Rect r,Action click,bool active=false,bool quiet=false){
  bool hover=r.Contains(mouseX,mouseY);var c=active?gold:quiet?panel:Col.Hex(0xE1E7D9);if(hover)c=Col.Mix(c,text,.09f);p.RoundBox(r.X,r.Y,r.W,r.H,8,c);p.Text(label,r.X+13,r.Y+(r.H-21)/2,14,text);buttons.Add((r,click));
 }
 void Draw(float blend){
  buttons.Clear();layerLegend=default;observationCardRect=default;creatureGeneSpots.Clear();p.Clear(bg);view.Draw(world,selected,blend);if(!mainMenu)DrawResourceHighlight();
  if(!mainMenu){p.Box(20,20,146,43,new Col(245,243,233,235));p.Text("E V O S I M",35,32,16,text);
  p.Box(p.W-324,20,304,43,new Col(245,243,233,235));p.Disk(p.W-306,42,3,world.Drought?gold:green);p.Text($"{world.Living} canlı",p.W-293,32,14,text);p.Text(Clock(world.Time),p.W-180,34,12,muted);}
  if(!mainMenu){
  if(timeline?.IsPreviewingHistory==true&&!timelineOpen){p.RoundBox(p.W-99,69,79,24,7,new Col(245,243,233,235));p.Text("Geçmiş",p.W-85,74,11,muted);}
  var c=world.Find(selected);if(HasDocks)DrawDocks();else if(c!=null)CreaturePanel(c);else if(HasResourceSelection)ResourcePanel();else if(showOverview)Overview();
  DrawMapLayerLegend();Bottom();DrawTimelinePanel();}
  if(mainMenu)MainMenuPanel();
  if(libraryOpen)LibraryPanel();if(setupOpen)SetupPanel();if(naming)NamePanel();
  if(elapsed<toastUntil){int width=Math.Min(p.W-40,Math.Max(300,toast.Length*7)),toastY=timelineOpen&&!mainMenu&&!setupOpen&&!libraryOpen&&!naming?p.H-264:p.H-121;p.Box((p.W-width)/2,toastY,width,32,new Col(245,243,233,245));p.Text(toast,(p.W-width)/2+12,toastY+8,12,text);}
  DrawSetupHelp();DrawCreatureHelp();DrawLineagePanel();DrawPopulationInsightsPanel();DrawJournal();DrawLifeTracePanel();
 }
 static string ActionName(string action)=>action switch {"move"=>"Hareket ediyor","fruit"=>"Meyve yiyor","bite"=>"Isırıyor","wander"=>"Keşfediyor","eat"=>"Otlanıyor","drink"=>"Su içiyor","hunt"=>"Avlanıyor","flee"=>"Uzaklaşıyor","rest"=>"Dinleniyor","mate"=>"Eş arıyor","scavenge"=>"Leşten besleniyor","dead"=>"Yaşamı sona erdi",_=>"Gözleniyor"};
 static string Clock(double seconds)=>WorldCalendar.Duration(seconds);
 World? breedingWorld;BreedingDiagnostics? breedingSnapshot;
 void Overview(){float x=p.W-342,y=87;
  observationCardRect=new Rect((int)x-18,(int)y-12,340,593);
  p.RoundBox(x-18,y-12,340,593,14,new Col(245,243,233,245));p.Text("Nüfus",x,y,17,text);Button("×",new Rect(p.W-61,(int)y-5,32,30),()=>showOverview=false,false,true);Button("Genler",new Rect((int)x+167,(int)y-5,103,30),ShowPopulationInsights,false,true);
  p.Disk(x+4,y+46,3,green);p.Text($"{world.Herbivores} otçul",x+15,y+35,13,text);p.Disk(x+150,y+46,3,red);p.Text($"{world.Carnivores} etçil",x+162,y+35,13,text);
  Graph(x,y+80,292,100);p.Text($"{world.Births} doğum   ·   {world.Deaths} ölüm",x,y+213,13,text);p.Text($"{world.Mutations} mutasyon   ·   Tohum {world.Seed}",x,y+239,12,muted);
  if(breedingWorld!=world||breedingSnapshot==null||world.Tick<breedingSnapshot.Tick||world.Tick-breedingSnapshot.Tick>=20){breedingWorld=world;breedingSnapshot=world.InspectBreeding();}
  var d=breedingSnapshot;var gates=d.ExclusiveFirstBlocker;p.Line(x,y+271,x+292,y+271,line);p.Text("Üreme · şu an",x,y+286,16,text);
  var rows=new (string Label,int Count)[]{("Üreme kapalı",gates.Disabled),("Henüz yetişkin değil",gates.Immature),("Üreme bekleme süresinde",gates.Cooldown),("Enerjisi yetersiz",gates.LowEnergy),("Su / sağlık yetersiz",gates.LowWater+gates.LowHealth+gates.OtherIneligible),("Uygun, ağ sinyali yok",gates.NoPositiveMateSignal),("Uygun ve istek sinyali var",gates.ReadyWithSignal)};
  for(int i=0;i<rows.Length;i++){float yy=y+318+i*25;p.Text(rows[i].Label,x,yy,12,muted);p.Text(rows[i].Count.ToString(),x+263,yy,12,text);}
  p.Text($"Yakın eş: {d.SexualReadyWithNearbyPartner}   ·   Eşeysiz hazır: {d.AsexualReadyWithSignal}",x,y+502,12,green);
  p.Text("Her canlı ilk engelinde sayılır.",x,y+535,11,muted);p.Text("Uygunluk, doğum garantisi değildir.",x,y+553,11,muted);
 }
 void Graph(float x,float y,float w,float h){
  for(int i=0;i<3;i++)p.Line(x,y+i*h/2,x+w,y+i*h/2,line.Alpha(120));
  var history=world.History;if(history.Count<2){p.Text("İlk gözlemler toplanıyor…",x+12,y+40,13,muted);return;}
  int start=Math.Max(0,history.Count-240);float max=20;for(int i=start;i<history.Count;i++)max=Math.Max(max,history[i].Herbivores+history[i].Carnivores);max=MathF.Ceiling(max/50)*50;
  double first=history[start].Time,last=history[^1].Time,span=Math.Max(1,last-first);
  for(int i=start+1;i<history.Count;i++){var a=history[i-1];var b=history[i];float ax=x+(float)((a.Time-first)/span)*w,bx=x+(float)((b.Time-first)/span)*w;p.Line(ax,y+h-a.Herbivores/max*h,bx,y+h-b.Herbivores/max*h,green);p.Line(ax,y+h-a.Carnivores/max*h,bx,y+h-b.Carnivores/max*h,red);}
  p.Text(Clock(first),x,y+h+9,10,muted);p.Text(Clock(last),x+w-128,y+h+9,10,muted);p.Text($"{max:0}",x+w-30,y-18,10,muted);
 }
 void Meter(string title,float value,float maximum,float x,float y,Col col){float ratio=Math.Clamp(maximum>0?value/maximum:0,0,1);p.Text(title,x,y,14,muted);p.Text($"{ratio*100:0}%",x+226,y,14,text);p.Box(x,y+27,274,5,line);p.Box(x,y+27,274*ratio,5,col);}
 void CompactMeter(string label,float ratio,float x,float y,Col c){p.Text(label,x,y,12,muted);p.Box(x+65,y+7,175,4,line);p.Box(x+65,y+7,175*Math.Clamp(ratio,0,1),4,c);}
 void Wrap(string value,float x,float y,int size,Col col,int length,int maxLines){var words=value.Split(' ');string row="";int n=0;foreach(string word in words){if(row.Length+word.Length>length&&row.Length>0){p.Text(row,x,y+n*(size+7),size,col);row="";if(++n==maxLines)return;}row+=(row.Length==0?"":" ")+word;}if(n<maxLines)p.Text(row,x,y+n*(size+7),size,col);}
 void Bottom(){int x=(p.W-176)/2,y=p.H-72;
  p.Box(x-6,y-6,188,55,new Col(245,243,233,240));
  Button(paused?"▶":"Ⅱ",new Rect(x,y,44,42),()=>paused=!paused,paused,true);
  Button($"{speed}×",new Rect(x+54,y,60,42),()=>speed=speed switch{1=>2,2=>4,4=>8,8=>12,12=>20,_=>1},false,true);
  Button("···",new Rect(x+124,y,44,42),()=>{if(!menuOpen)HideTimeline();menuOpen=!menuOpen;},menuOpen,true);
  if(!paused&&elapsed>3&&speed>1&&Math.Abs(achieved-speed)>.3)p.Text($"{achieved:0.0}× gerçekleşen",x+32,y-20,11,muted);
  if(menuOpen){int mx=x-26,my=y-453;p.Box(mx-8,my-44,246,483,new Col(245,243,233,250));
   void Item(string label,int row,Action action,bool active=false){Button(label,new Rect(mx,my+(row+1)*36,230,32),()=>{action();menuOpen=false;},active,true);}
   Item(timeline?.IsPreviewingHistory==true?"Yeni dalda kuraklık":world.Drought?"Kuraklığı bitir":"Kuraklık",0,ToggleDrought,world.Drought);
   Item("Katman · "+LayerName(view.Layer),-2,CycleMapLayer,view.Layer!=MapLayer.None);
   Item("Olay günlüğü",-1,ShowJournal);
   Item("Geçmiş",1,ShowTimeline,timelineOpen);
   Item("Nüfus grafiği",2,()=>{ClearResourceSelection();ToggleOverviewDock();},showOverview);
   Item("Dünyalar",3,()=>{HideTimeline();libraryOpen=true;paused=true;});
   Item("Tüm harita",4,()=>{FitWorld();});
   Item("Aynı vadiyi başlat",5,()=>Restart(false));
   Item("Yeni dünya kur",6,OpenSetup);
   Item("Dünyayı kaydet",8,()=>{if(SaveCurrent())Say("Dünya kaydedildi.");});
   Item("Gözlem raporu",9,Export);
   Item("Dünyadan çık",10,ExitWorld);
   Item(view.ShowSenses?"Yönü gizle":"Seçili canlının yönü",7,()=>view.ShowSenses=!view.ShowSenses,view.ShowSenses);
  }
 }
 bool SaveCurrent(){if(!hasActiveWorld||(capture&&!args.Contains("--ui-smoke")))return true;try{var session=EnsureTimeline();currentSlot=store.Save(session.HeadWorld,currentSlot?.Name??$"Vadi {session.HeadWorld.Seed} · {DateTime.Now:dd.MM HH:mm}",currentSlot,view.Zoom,view.CenterX,view.CenterY,speed,timelineArchive:session.SerializeArchive());return true;}catch(Exception ex){Say("Dünya kaydedilemedi: "+ex.Message);Console.Error.WriteLine(ex);return false;}}
 void OpenWorld(WorldSlot slot){CloseObservationPanels();slotActions=null;var opened=store.OpenForCurrentVersion(slot);var nextTimeline=TimelineSession.Open(opened.World,opened.TimelineArchive);slot=opened.Slot;world=opened.World;timeline=nextTimeline;timelineOpen=draggingTimeline=false;hasActiveWorld=true;mainMenu=false;setupOpen=false;currentSlot=slot;view.Bind(world);view.Zoom=float.IsFinite(slot.Zoom)?Math.Clamp(slot.Zoom,.8f,5f*world.Map.Size/128):1;view.CenterX=float.IsFinite(slot.X)?Math.Clamp(slot.X,0,world.Map.Size):world.Map.Size*.5f;view.CenterY=float.IsFinite(slot.Y)?Math.Clamp(slot.Y,0,world.Map.Size):world.Map.Size*.5f;speed=new[]{1,2,4,8,12,20}.Contains(slot.Speed)?slot.Speed:1;selected=0;follow=false;accumulator=0;paused=true;libraryOpen=false;menuOpen=false;Say(opened.Migrated?"Eski kaydın korundu; yeni sürüm için ayrı dünya açıldı.":slot.Name+" açıldı.");}
 void ExitWorld(){if(!SaveCurrent())return;CloseObservationPanels();if(timeline!=null){timeline.ReturnToHead();SyncTimelineWorld();}timelineOpen=false;slotActions=null;trashView=false;paused=true;accumulator=0;mainMenu=true;hasActiveWorld=false;setupOpen=libraryOpen=menuOpen=false;selected=0;follow=false;libraryPage=0;}
 void MainMenuPanel(){
  buttons.Clear();p.Box(0,0,p.W,p.H,new Col(233,237,221,175));int x=(p.W-820)/2,y=(p.H-680)/2;
  p.RoundBox(x+3,y+7,820,680,22,new Col(38,62,53,30));p.RoundBox(x,y,820,680,22,bg);p.Text("E V O S I M",x+34,y+29,30,text);
  p.Text(trashView?"Silinen dünyalar":"Dünyaların",x+34,y+94,18,muted);
  Button("+ Yeni dünya oluştur",new Rect(x+529,y+27,257,46),()=>{OpenSetup();worldName="Yeni vadi";formSeed=Random.Shared.Next(1,1000000);},true);
  var allSlots=store.List(trashView);var slots=WorldLibraryQuery.Apply(allSlots,worldSort,worldFilter);DrawLibraryControls(new Rect(x+379,y+87,220,35),new Rect(x+610,y+87,176,35));p.Text($"{slots.Count} / {allSlots.Count}",x+270,y+99,12,muted);libraryPage=Math.Clamp(libraryPage,0,Math.Max(0,(slots.Count-1)/7));
  if(slots.Count==0&&allSlots.Count>0){p.Text("Bu filtreye uyan dünya yok.",x+34,y+194,20,text);p.Text("Üstteki durum filtresini değiştirerek diğer kayıtları görebilirsin.",x+34,y+231,14,muted);}
  if(allSlots.Count==0){p.Text(trashView?"Silinen dünya yok.":"Henüz bir dünya yok.",x+34,y+194,20,text);p.Text(trashView?"Sildiğin dünyaları buradan geri alabilirsin.":"İlk dünyanın koşullarını seçerek başlayabilirsin.",x+34,y+231,15,muted);}
  for(int i=0;i<7;i++){int index=libraryPage*7+i;if(index>=slots.Count)break;var slot=slots[index];int row=y+143+i*62;
   p.RoundBox(x+24,row-5,772,55,10,new Col(230,236,222));DrawWorldPreview(slot,new Rect(x+33,row,46,46));string label=slot.Name.Length>38?slot.Name[..37]+"…":slot.Name;
   p.Text(label,x+93,row+2,17,text);p.Text($"{Clock(slot.Tick*.05)}  ·  {slot.Living} canlı  ·  {slot.MapSize}²  ·  {slot.SavedUtc[..10]}",x+93,row+27,12,muted);
   Button(trashView?"Geri al":"Aç",new Rect(x+596,row+3,83,36),()=>{try{if(trashView){store.Restore(slot);Say(slot.Name+" geri alındı.");}else{OpenWorld(slot);paused=false;}}catch(Exception ex){Say(ex.Message);}});
   if(!trashView)Button("···",new Rect(x+718,row+3,59,36),()=>slotActions=slotActions?.Id==slot.Id?null:slot);
  }
  if(slots.Count>7){Button("‹",new Rect(x+34,y+594,44,35),()=>libraryPage--);p.Text($"{libraryPage+1} / {(slots.Count+6)/7}",x+98,y+604,13,muted);Button("›",new Rect(x+159,y+594,44,35),()=>libraryPage++);}
  Button(trashView?"← Dünyalar":"Silinenler",new Rect(x+34,y+623,151,35),()=>{trashView=!trashView;slotActions=null;libraryPage=0;});Button("Uygulamayı kapat",new Rect(x+596,y+619,190,38),()=>running=false);
  if(slotActions!=null){var slot=slotActions;p.RoundBox(x+545,y+112,245,102,12,panel);Button("Adlandır",new Rect(x+555,y+119,225,37),()=>{slotActions=null;BeginName(slot,false);});Button("Sil",new Rect(x+555,y+162,225,37),()=>DeleteWorld(slot));}
 }
 void DeleteWorld(WorldSlot slot){try{store.Trash(slot);slotActions=null;if(currentSlot?.Id==slot.Id){currentSlot=null;hasActiveWorld=false;paused=true;}Say(slot.Name+" silindi. Silinenler bölümünden geri alabilirsin.");}catch(Exception ex){Say(ex.Message);}}
 void BeginName(WorldSlot? slot,bool copy){HideTimeline();editSetter=null;editTitle="Dünya adı";renameSlot=slot;saveAs=copy;nameText=slot?.Name??currentSlot?.Name??"Yeni vadi";naming=true;paused=true;SDL.SDL_StartTextInput();}
 void CommitName(){try{if(editSetter!=null){editSetter(nameText);editSetter=null;naming=false;SDL.SDL_StopTextInput();return;}if(saveAs){if(!BranchTimeline(nameText))return;}else if(renameSlot!=null){var renamed=store.Rename(renameSlot,nameText);if(currentSlot?.Id==renamed.Id)currentSlot=renamed;}naming=false;SDL.SDL_StopTextInput();Say("Dünya adı kaydedildi.");}catch(Exception ex){Say(ex.Message);}}
 void LibraryPanel(){buttons.Clear();p.Box(0,0,p.W,p.H,new Col(226,233,216,160));int x=(p.W-584)/2,y=(p.H-580)/2;p.Box(x,y,584,580,bg);p.Text("Dünyalar",x+25,y+23,24,text);Button("×",new Rect(x+530,y+19,32,32),()=>libraryOpen=false);
  Button("Kaydet",new Rect(x+25,y+70,107,36),()=>{if(SaveCurrent())Say("Dünya kaydedildi.");});Button("Yeni kayıt",new Rect(x+146,y+70,129,36),()=>BeginName(null,true));
  var allSlots=store.List();var slots=WorldLibraryQuery.Apply(allSlots,worldSort,worldFilter);DrawLibraryControls(new Rect(x+304,y+18,215,35),new Rect(x+151,y+18,142,35));if(slots.Count==0)p.Text(allSlots.Count==0?"Henüz kayıt yok.":"Bu filtreye uyan dünya yok.",x+25,y+148,16,muted);libraryPage=Math.Clamp(libraryPage,0,Math.Max(0,(slots.Count-1)/7));
  for(int i=0;i<7;i++){int index=libraryPage*7+i;if(index>=slots.Count)break;var slot=slots[index];int row=y+129+i*55;
   string label=slot.Name.Length>31?slot.Name[..30]+"…":slot.Name;p.Text(label,x+25,row,15,currentSlot?.Id==slot.Id?gold:text);p.Text($"{Clock(slot.Tick*.05)}  ·  {slot.Living} canlı  ·  {slot.MapSize}²  ·  {slot.SavedUtc[..10]}",x+25,row+23,11,muted);
   Button("Aç",new Rect(x+409,row,61,34),()=>{try{if(SaveCurrent())OpenWorld(slot);}catch(Exception ex){Say(ex.Message);}});Button("Ad",new Rect(x+482,row,59,34),()=>BeginName(slot,false));
  }
  if(slots.Count>7){Button("‹",new Rect(x+25,y+530,45,32),()=>libraryPage=Math.Max(0,libraryPage-1));Button("›",new Rect(x+83,y+530,45,32),()=>libraryPage++);}
  p.Text("30 saniyede bir ve çıkarken otomatik kayıt",x+194,y+540,12,muted);
 }
 void NamePanel(){buttons.Clear();p.Box(0,0,p.W,p.H,new Col(226,233,216,190));int x=(p.W-490)/2,y=(p.H-200)/2;p.Box(x,y,490,200,panel);p.Text(editTitle,x+24,y+20,17,text);p.Box(x+24,y+63,442,43,bg);p.Text(nameText.Length>42?nameText[^42..]:nameText,x+34,y+75,16,text);Button("Kaydet",new Rect(x+24,y+131,108,39),CommitName);Button("Vazgeç",new Rect(x+146,y+131,108,39),()=>{naming=false;SDL.SDL_StopTextInput();});p.Text("Ctrl+A: temizle  ·  Enter: kaydet",x+265,y+145,11,muted);}
 void ToggleDrought(){var session=EnsureTimeline();if(session.IsSeeking){Say("Önce aranan anın hazırlanmasını bekle.");return;}if(session.IsPreviewingHistory&&!BranchTimeline())return;session=EnsureTimeline();session.ExecuteDrought(!session.DisplayWorld.Drought);SyncTimelineWorld();Say(world.Drought?"Kuraklık başladı. Bitki üretimi ve su dengesi değişiyor.":"Ilıman koşullar geri geldi.");}
 void Restart(bool newSeed){if(resetArmedUntil==0||elapsed>resetArmedUntil){resetArmedUntil=elapsed+4;Say("Aynı tohumla yeni bir dünya başlayacak. Devam için bir kez daha bas.");return;}if(!SaveCurrent())return;CloseObservationPanels();int seed=newSeed?Random.Shared.Next(1,1000000):world.Seed;world=new World(seed,world.Config);timeline=TimelineSession.Create(world);timelineOpen=draggingTimeline=false;currentSlot=null;SaveCurrent();selected=0;follow=false;accumulator=0;FitWorld(1.25f);resetArmedUntil=0;Say($"Tohum {seed} ile yeni gözlem başladı.");}
 void Say(string message){toast=message;toastUntil=elapsed+5;}
 void Export(){try{
  if(timeline?.IsSeeking==true){Say("Rapor için geçmiş aramasının bitmesini bekle.");return;}
  string dir=Environment.GetEnvironmentVariable("EVOSIM_DATA_DIR")??Path.Combine(AppContext.BaseDirectory,"observations");
  ObservationExporter.Write(world,dir,timeline?.HeadTick??world.Tick,timeline?.EarliestTick??world.Tick,currentSlot?.Id,currentSlot?.Name);
  Say("Gözlem paketi: JSON, nüfus ve birey CSV'leri observations klasöründe.");
 }catch(Exception ex){Console.Error.WriteLine(ex);Say("Rapor yazılamadı. Klasör erişimini kontrol et.");}}
 public void Dispose(){CloseLifeTrace();DisposePreviews();view.Dispose();p.Dispose();}
}
