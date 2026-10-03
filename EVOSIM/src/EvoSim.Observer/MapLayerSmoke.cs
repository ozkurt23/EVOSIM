using EvoSim.Core;
using EvoSim.Observer;
internal sealed partial class ObserverApp
{
    void MapLayerSmoke()
    {
        void Check(bool value,string message){if(!value)throw new Exception("Map layer: "+message);}
        byte[] Bytes(World value){using var s=new MemoryStream();value.Save(s);return s.ToArray();}
        void Send(Event e){SDL.SDL_PushEvent(ref e);ProcessEvents();}
        foreach(int size in new[]{128,192,256})
        {
            var sample=new World(42,new WorldConfig{MapSize=size,Herbivores=12,Carnivores=2,TreeCount=20});
            for(int i=0;i<120;i++)sample.Step();
            byte[] before=Bytes(sample);
            var population=LayerMap.Build(sample,MapLayer.Population);var food=LayerMap.Build(sample,MapLayer.PlantFood);var moisture=LayerMap.Build(sample,MapLayer.Moisture);
            Check(population.Columns==size/8&&population.Values.Sum()==sample.Living,"all map sizes count every living body once");
            double expected=sample.Map.Food.Sum(f=>(double)f)+sample.Map.Trees.Sum(t=>t.Fruit.Sum(f=>(double)f));
            Check(Math.Abs(food.Values.Sum()-expected)<.00001,"plant stock conserved including fruit slots");
            Check(moisture.Maximum==1&&moisture.Values.All(v=>double.IsNaN(v)||(v>=0&&v<=1)),"fixed moisture scale and no-land mask");
            Check(before.SequenceEqual(Bytes(sample)),"full checkpoint including RNG unchanged");
        }
        var edges=new World(9,new WorldConfig{Herbivores=2,Carnivores=0,TreeCount=0});
        edges.Creatures[0].X=7.999f;edges.Creatures[0].Y=0;edges.Creatures[1].X=8;edges.Creatures[1].Y=0;
        var bins=LayerMap.Build(edges,MapLayer.Population);Check(bins.Values[0]==1&&bins.Values[1]==1,"exact bucket boundary");
        edges.Creatures[0].Alive=false;Check(LayerMap.Build(edges,MapLayer.Population).Values.Sum()==1,"dead individuals excluded");
        edges.Creatures[1].Alive=false;Check(LayerMap.Build(edges,MapLayer.Population).Maximum==0,"empty population has zero scale");
        var original=world;var oldTimeline=timeline;var oldSlot=currentSlot;bool oldMenu=mainMenu,oldActive=hasActiveWorld,oldPaused=paused;int oldSelected=selected;var oldLayer=view.Layer;
        try
        {
            CloseObservationPanels();world=new World(42);timeline=TimelineSession.Create(world);currentSlot=null;hasActiveWorld=true;mainMenu=setupOpen=libraryOpen=naming=false;paused=true;selected=0;view.Layer=MapLayer.None;
            for(int i=0;i<1800;i++)AdvanceWorld();byte[] before=Bytes(world);long tick=world.Tick;
            Layout();Draw(1);Send(new Event{Type=0x300,B=118});Draw(1);
            Check(view.Layer==MapLayer.Population&&view.LayerData!.Values.Sum()==world.Living,"keyboard opens actual layer");
            Send(new Event{Type=0x401,Button=1,B=60,C=125});Check(selected==0&&!HasResourceSelection,"legend text cannot pick map underneath");
            float zoom=view.Zoom;Send(new Event{Type=0x403,B=1});Check(view.Zoom==zoom,"legend wheel cannot zoom map");
            Send(new Event{Type=0x300,B=118});Draw(1);Check(view.Layer==MapLayer.PlantFood,"cycle to plant stock");
            Send(new Event{Type=0x300,B=118});Draw(1);Check(view.Layer==MapLayer.Moisture,"cycle to moisture");
            Send(new Event{Type=0x401,Button=1,B=287,C=98});Check(view.Layer==MapLayer.None,"legend close button");
            Check(world.Tick==tick&&before.SequenceEqual(Bytes(world)),"drawing and input never mutate model");
            RequestTimelineSeek(0);while(timeline.IsSeeking)timeline.PumpSeek(20,100);SyncTimelineWorld();view.Layer=MapLayer.Population;Draw(1);
            Check(view.LayerData!.Tick==0&&view.LayerData.Values.Sum()==165,"seek replaces cached future layer");
            Check(before.SequenceEqual(Bytes(timeline.HeadWorld)),"past layer preserves head checkpoint");
        }
        finally {CloseObservationPanels();world=original;timeline=oldTimeline;currentSlot=oldSlot;mainMenu=oldMenu;hasActiveWorld=oldActive;paused=oldPaused;selected=oldSelected;view.Layer=oldLayer;timelineOpen=false;menuOpen=false;FitWorld(1.25f);}
    }
}
