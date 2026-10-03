using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 TimelineSession? timeline;
 bool timelineOpen,draggingTimeline;
 Rect timelinePanelRect,timelineTrack;
 int timelineFirstButton;
 long scrubTick,timelineTargetTick;
 TimelineSession EnsureTimeline(){
  if(timeline==null||(!ReferenceEquals(timeline.DisplayWorld,world)&&!ReferenceEquals(timeline.HeadWorld,world)))timeline=TimelineSession.Create(world);
  return timeline;
 }
 void SyncTimelineWorld(){
  if(timeline==null||ReferenceEquals(world,timeline.DisplayWorld))return;
  CloseObservationPanels();world=timeline.DisplayWorld;view.Bind(world);accumulator=0;
  if(world.Find(selected) is not {Alive:true}){selected=0;follow=false;}
 }
 void AdvanceWorld(){try{var session=EnsureTimeline();session.AdvancePlaybackOneTick();SyncTimelineWorld();}catch(Exception ex){paused=true;Say("İlerleme durduruldu: "+ex.Message);}}
 void ShowTimeline(){CloseObservationPanels();EnsureTimeline();draggingTimeline=false;timelineOpen=true;menuOpen=false;paused=true;accumulator=0;}
 void HideTimeline(){if(timeline?.IsSeeking==true)timeline.CancelSeek();SyncTimelineWorld();timelineOpen=false;draggingTimeline=false;}
 static string TimelineClock(long tick)=>WorldCalendar.Duration(tick*.05,true);
 bool BranchTimeline(string? name=null){
  var session=EnsureTimeline();if(session.IsSeeking){Say("Aranan an hazır olduğunda buradan devam edebilirsin.");return false;}
  if(!SaveCurrent())return false;
  try{
   var child=session.CreateBranch();string label=name??(currentSlot?.Name??"Vadi")+" · "+Clock(child.ViewTick*.05)+" dal";
   var slot=store.Save(child.HeadWorld,label,null,view.Zoom,view.CenterX,view.CenterY,speed,currentSlot?.Id,World.CurrentCheckpointFormat,child.SerializeArchive());
   timeline=child;world=child.DisplayWorld;currentSlot=slot;view.Bind(world);accumulator=0;paused=true;
   Say("Bu andan yeni bir dünya açıldı. Önceki dünyanın geleceği korundu.");return true;
  }catch(Exception ex){Say("Dal oluşturulamadı: "+ex.Message);return false;}
 }
 void RequestTimelineSeek(long target){
  var session=EnsureTimeline();paused=true;accumulator=0;
  try{timelineTargetTick=Math.Clamp(target,session.EarliestTick,session.HeadTick);session.BeginSeek(timelineTargetTick);SyncTimelineWorld();}
  catch(Exception ex){Say("Geçmiş açılamadı: "+ex.Message);}
 }
 long TimelineTickAt(int x){var session=EnsureTimeline();double t=Math.Clamp((double)(x-timelineTrack.X)/Math.Max(1,timelineTrack.W),0,1);return session.EarliestTick+(long)Math.Round(t*(session.HeadTick-session.EarliestTick));}
 bool HandleTimelineEvent(Event ev){
  if(!timelineOpen||mainMenu||setupOpen||libraryOpen||naming)return false;
  var session=EnsureTimeline();
  if(ev.Type==0x300&&ev.B==27){if(session.IsSeeking){session.CancelSeek();SyncTimelineWorld();}else timelineOpen=false;draggingTimeline=false;return true;}
  if(ev.Type==0x300&&ev.B==1073741904){RequestTimelineSeek((session.IsSeeking?timelineTargetTick:session.ViewTick)-1);return true;}
  if(ev.Type==0x400&&draggingTimeline){mouseX=ev.B;mouseY=ev.C;scrubTick=TimelineTickAt(mouseX);return true;}
  if(ev.Type==0x401&&ev.Button==1){mouseX=ev.B;mouseY=ev.C;var hit=new Rect(timelineTrack.X-8,timelineTrack.Y-11,timelineTrack.W+16,30);if(hit.Contains(mouseX,mouseY)){draggingTimeline=true;scrubTick=TimelineTickAt(mouseX);paused=true;return true;}
   if(timelinePanelRect.Contains(mouseX,mouseY)){var button=buttons.Skip(timelineFirstButton).LastOrDefault(b=>b.Rect.Contains(mouseX,mouseY));button.Action?.Invoke();return true;}}
  if(ev.Type==0x402&&ev.Button==1&&draggingTimeline){draggingTimeline=false;RequestTimelineSeek(scrubTick);return true;}
  if(ev.Type==0x403&&timelinePanelRect.Contains(mouseX,mouseY)){int delta=ev.C==1?-ev.B:ev.B;RequestTimelineSeek((session.IsSeeking?timelineTargetTick:session.ViewTick)+delta*20L);return true;}
  return false;
 }
 void DrawTimelinePanel(){
  if(!timelineOpen||mainMenu||setupOpen||libraryOpen||naming){timelinePanelRect=timelineTrack=default;return;}
  var session=EnsureTimeline();timelineFirstButton=buttons.Count;int width=Math.Min(1040,p.W-72),x=(p.W-width)/2,y=p.H-218;timelinePanelRect=new Rect(x,y,width,122);
  p.RoundBox(x+2,y+4,width,122,13,new Col(38,62,53,30));p.RoundBox(x,y,width,122,13,new Col(246,247,234,248));
  p.Text(session.IsSeeking?"Geçmiş hazırlanıyor…":"Dünyanın geçmişi",x+20,y+13,15,text);
  string state=draggingTimeline?TimelineClock(scrubTick):session.IsSeeking?$"Hedef · {TimelineClock(timelineTargetTick)}  ·  %{session.Progress.Fraction*100:0}":TimelineClock(session.ViewTick)+(session.IsPreviewingHistory?" · geçmiş":" · son an");p.Text(state,x+230,y+15,12,muted);
  Button("×",new Rect(x+width-43,y+7,29,29),HideTimeline,false,true);
  timelineTrack=new Rect(x+24,y+51,width-48,6);p.RoundBox(timelineTrack.X,timelineTrack.Y,timelineTrack.W,6,3,line);
  float position=session.HeadTick==session.EarliestTick?1:(float)((double)((draggingTimeline?scrubTick:session.ViewTick)-session.EarliestTick)/(session.HeadTick-session.EarliestTick));
  p.RoundBox(timelineTrack.X,timelineTrack.Y,timelineTrack.W*position,6,3,green);p.Disk(timelineTrack.X+timelineTrack.W*position,timelineTrack.Y+3,7,bg);p.Ring(timelineTrack.X+timelineTrack.W*position,timelineTrack.Y+3,7,green);
  p.Text(Clock(session.EarliestTick*.05),x+24,y+66,11,muted);p.Text(Clock(session.HeadTick*.05),x+width-178,y+66,11,muted);
  p.Text("← → 1 saat 12 dk   ·   Tekerlek: 1 gün",x+23,y+96,11,muted);
  if(session.IsSeeking)Button("Vazgeç",new Rect(x+width-154,y+83,130,29),()=>{session.CancelSeek();SyncTimelineWorld();},false,true);
  else if(session.IsPreviewingHistory){Button("Son ana dön",new Rect(x+width-326,y+83,135,29),()=>{session.ReturnToHead();SyncTimelineWorld();paused=true;},false,true);Button("Buradan devam",new Rect(x+width-180,y+83,156,29),()=>BranchTimeline(),true,true);}
 }
}
