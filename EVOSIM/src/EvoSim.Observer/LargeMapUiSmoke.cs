using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
 void LargeMapUiSmoke(){
  void Check(bool ok,string why){if(!ok)throw new Exception("Large map UI: "+why);}
  var original=world;var originalSlot=currentSlot;var originalTimeline=timeline;bool oldMenu=mainMenu,oldActive=hasActiveWorld,oldPaused=paused;float oldZoom=view.Zoom,oldX=view.CenterX,oldY=view.CenterY;
  try{
   OpenSetup();draft=new WorldConfig();setupTab=0;ResetSetupScroll();Draw(1);
   var sizeControl=setupControls.Single(c=>c.Item.Entry.Key=="MapSize");Check(sizeControl.Track.W==0,"size uses discrete control");focusedSetupKey="MapSize";
   HandleSetupEvent(new Event{Type=0x300,B=13});Check(draft.MapSize==192,"size selects 192");HandleSetupEvent(new Event{Type=0x300,B=13});Check(draft.MapSize==256,"size selects 256");
   bool rejected=false;try{sizeControl.Item.Entry.Write("129");}catch(ArgumentException){rejected=true;}Check(rejected,"unsupported size rejected");
   var saved=new List<WorldSlot>();
   foreach(int size in new[]{128,192,256}){
    draft=new WorldConfig{MapSize=size,Herbivores=12,Carnivores=2,TreeCount=20};worldName="Boyut deneyi "+size;MakeWorld();paused=true;
    Check(!setupOpen&&world.Map.Size==size&&view.CenterX==size*.5f&&view.CenterY==size*.5f,"creation centers actual world");
    Check(Math.Abs(view.MaximumZoom-5f*size/128)<.001f,"zoom preserves maximum detail");
    Layout();Draw(1);var anchor=view.WorldAt(p.W/2+90,p.H/2-70);view.ZoomAt(2,p.W/2+90,p.H/2-70);var after=view.WorldAt(p.W/2+90,p.H/2-70);
    Check(Math.Abs(anchor.X-after.X)<.0001f&&Math.Abs(anchor.Y-after.Y)<.0001f,"wheel anchor stable");
    var body=world.Creatures[0];bool placed=false;
    for(int y=size-5;y>size*.65&&!placed;y--)for(int x=size-5;x>size*.65;x--)if(world.Map.CanOccupy(x+.5f,y+.5f,body.Radius,body.Genes[3])){body.X=x+.5f;body.Y=y+.5f;placed=true;break;}
    Check(placed,"southeast fixture placed");timeline=TimelineSession.Create(world);FitWorld();Draw(1);var point=view.Screen(body.X,body.Y);Check(view.Pick(world,(int)point.X,(int)point.Y)==body.Id,"pick southeast body");
    Check(SaveCurrent(),"save large world");saved.Add(currentSlot!);Check(currentSlot!.MapSize==size,"metadata size");string hash=world.StateHash();OpenWorld(currentSlot);Check(world.StateHash()==hash&&view.CenterX==size*.5f&&view.CenterY==size*.5f,"reopen exact camera and state");
    var pixels=Convert.FromBase64String(currentSlot!.Preview);int px=(int)(body.X/size*64),py=(int)(body.Y/size*64);Check(pixels[px+py*64] is 5 or 6,"thumbnail samples southeast");
   }
   view.Zoom=view.MaximumZoom;OpenWorld(saved[0]);Check(world.Map.Size==128&&view.MaximumZoom==5,"large-to-small bind resets extent");
  }finally{
   world=original;currentSlot=originalSlot;timeline=originalTimeline;mainMenu=oldMenu;hasActiveWorld=oldActive;paused=oldPaused;setupOpen=false;view.Bind(world);view.Zoom=oldZoom;view.CenterX=oldX;view.CenterY=oldY;
  }
 }
}
