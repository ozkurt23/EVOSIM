using System.Globalization;
using EvoSim.Core;
using EvoSim.Observer;

internal sealed partial class ObserverApp
{
    World? resourceWorld;
    ResourceHandle? resourceHandle;
    ResourceReading? resourceReading;
    long resourceReadingTick=-1;
    bool HasResourceSelection=>CurrentResourceReading()!=null;
    void ClearResourceSelection(){resourceWorld=null;resourceHandle=null;resourceReading=null;resourceReadingTick=-1;}
    ResourceReading? CurrentResourceReading()
    {
        if(!ReferenceEquals(resourceWorld,world)){ClearResourceSelection();return null;}
        if(resourceHandle is not ResourceHandle handle)return null;
        if(resourceReading==null||resourceReadingTick!=world.Tick){resourceReading=ResourceInspection.Read(world,handle);resourceReadingTick=world.Tick;}
        if(resourceReading==null)ClearResourceSelection();return resourceReading;
    }
    bool TrySelectResource(int x,int y)
    {
        ClearResourceSelection();if(!view.View.Contains(x,y))return false;view.Bind(world);
        var point=view.WorldAt(x,y);int cell=world.Map.Cell(point.X,point.Y);if(cell<0)return false;
        float scale=Math.Abs(view.Screen(1,0).X-view.Screen(0,0).X);
        float Distance(float wx,float wy){var screen=view.Screen(wx,wy);float dx=screen.X-x,dy=screen.Y-y;return dx*dx+dy*dy;}
        ResourceHandle? chosen=null;float nearest=float.PositiveInfinity;
        // The tree canopy is the top resource layer. Living bodies take priority in Program.
        for(int i=0;i<world.Map.Trees.Count;i++){
            var tree=world.Map.Trees[i];float radius=Math.Max(6,tree.Radius*scale+2),distance=Distance(tree.X,tree.Y);
            if((distance<=radius*radius||view.TreeBounds(tree).Contains(x,y))&&distance<nearest){nearest=distance;chosen=new(InspectableResourceKind.Tree,i);}
        }
        if(chosen==null){
            foreach(var corpse in world.Corpses){if(corpse.Food<=0||(world.Tick-corpse.CreatedTick)*World.Dt>=world.Config.CorpseLifetime)continue;
                float radius=Math.Max(6,scale*.55f),distance=Distance(corpse.X,corpse.Y);
                if(distance<=radius*radius&&distance<nearest){nearest=distance;chosen=new(InspectableResourceKind.Corpse,corpse.CreatureId);}
            }
        }
        if(chosen==null){
            for(int i=0;i<world.Map.Shelters.Count;i++){
                var shelter=world.Map.Shelters[i];var s=view.Screen(shelter.X,shelter.Y);
                float dx=(x-s.X)/Math.Max(7,scale*2.2f),dy=(y-s.Y)/Math.Max(5,scale*1.5f),distance=Distance(shelter.X,shelter.Y);
                if(dx*dx+dy*dy<=1&&distance<nearest){nearest=distance;chosen=new(InspectableResourceKind.Shelter,i);}
            }
        }
        if(chosen==null){
            if(world.Map.Tiles[cell]==1)chosen=new(InspectableResourceKind.Water,cell);
            else if(world.Map.Tiles[cell]==2)chosen=new(InspectableResourceKind.Rock,cell);
            // Match the visible grass threshold. Plain ground never opens a resource card.
            else if(world.Map.Food[cell]>=.15f)chosen=new(InspectableResourceKind.Grass,cell);
        }
        if(chosen==null)return false;
        var reading=ResourceInspection.Read(world,chosen.Value);if(reading==null)return false;
        resourceWorld=world;resourceHandle=chosen;resourceReading=reading;resourceReadingTick=world.Tick;
        selected=0;follow=false;OpenResourceDock(chosen.Value);return true;
    }
    void DrawResourceHighlight()
    {
        var reading=CurrentResourceReading();if(reading==null||mainMenu||setupOpen||libraryOpen||naming)return;
        var s=view.Screen(reading.Position.X,reading.Position.Y);float scale=Math.Abs(view.Screen(1,0).X-view.Screen(0,0).X);
        p.Clip(view.View);
        if(reading.Handle.Kind is InspectableResourceKind.Tree or InspectableResourceKind.Shelter or InspectableResourceKind.Corpse){
            float radius=Math.Max(8,reading.Radius*scale+3);p.Ring(s.X,s.Y,radius,new Col(251,244,211,210));p.Ring(s.X,s.Y,radius+1.7f,new Col(76,115,85,170));
        }else{
            float half=Math.Max(4,scale*.5f+1),length=Math.Clamp(half*.55f,3,8);var ink=new Col(246,242,210,240);
            for(int dy=-1;dy<=1;dy+=2)for(int dx=-1;dx<=1;dx+=2){float xx=s.X+dx*half,yy=s.Y+dy*half;p.Line(xx,yy,xx-dx*length,yy,ink);p.Line(xx,yy,xx,yy-dy*length,ink);}
        }
        p.Unclip();
    }
    static string[] ResourceNoteLines(IEnumerable<string> paragraphs)
    {
        var lines=new List<string>();foreach(string paragraph in paragraphs){string row="";foreach(string word in paragraph.Split(' ')){
            if(row.Length>0&&row.Length+word.Length+1>43){lines.Add(row);row="";}
            row+=(row.Length==0?"":" ")+word;
        }if(row.Length>0)lines.Add(row);lines.Add("");}if(lines.Count>0)lines.RemoveAt(lines.Count-1);return lines.ToArray();
    }
    static string ResourceEta(double? seconds)
    {
        if(seconds==null)return "";if(!double.IsFinite(seconds.Value))return "Bekliyor";
        return "≈"+WorldCalendar.CompactDuration(seconds.Value);
    }
    void ResourcePanel()
    {
        var reading=CurrentResourceReading();if(reading==null||mainMenu||setupOpen||libraryOpen||naming)return;
        var notes=ResourceNoteLines(reading.Notes);int slotRows=(reading.FruitSlots.Length+3)/4;
        int width=DrawingDock?312:314,x=DrawingDock?dockViewport.X:p.W-width-20,y=DrawingDock?dockViewport.Y-(int)drawingCard!.Scroll:82,metricsHeight=reading.Metrics.Length*25,fruitHeight=slotRows==0?0:26+slotRows*44;
        int height=76+metricsHeight+fruitHeight+20+notes.Length*16;
        if(DrawingDock)drawingCard!.ContentHeight=height;
        observationCardRect=new Rect(x,y,width,height);
        p.RoundBox(x+2,y+4,width,height,15,new Col(38,62,53,28));p.RoundBox(x,y,width,height,15,new Col(246,247,234,245));
        var accent=reading.Handle.Kind==InspectableResourceKind.Water?blue:reading.Handle.Kind==InspectableResourceKind.Corpse?red:green;
        p.Disk(x+22,y+25,4,accent);p.Text(reading.Title,x+36,y+14,18,text);p.Text(reading.Subtitle,x+18,y+43,11,muted);
        var card=drawingCard;Button("×",new Rect(x+width-39,y+9,28,28),()=>{if(card!=null)CloseDock(card);ClearResourceSelection();},false,true);
        int rowY=y+74;
        foreach(var metric in reading.Metrics){p.Text(metric.Label,x+18,rowY,12,muted);p.Text(metric.Value,x+172,rowY-1,13,text);rowY+=25;}
        if(slotRows>0){
            p.Text("Meyve yuvaları · E: ham enerji",x+18,rowY+2,11,muted);rowY+=24;
            for(int i=0;i<reading.FruitSlots.Length;i++){
                var fruit=reading.FruitSlots[i];int xx=x+18+(i%4)*70,yy=rowY+(i/4)*44;
                p.RoundBox(xx,yy,64,39,7,fruit.Occupied?new Col(231,236,218):new Col(238,237,222));
                p.Disk(xx+10,yy+11,2.6f,fruit.Occupied?red:line);p.Text("#"+fruit.Number,xx+18,yy+4,10,muted);
                string value=fruit.Occupied?fruit.RawEnergy.ToString("0.##",CultureInfo.InvariantCulture)+" E":ResourceEta(fruit.ApproximateRegrowSeconds);
                p.Text(value,xx+7,yy+21,10,fruit.Occupied?text:muted);
            }
            rowY+=slotRows*44+2;
        }
        p.Line(x+18,rowY+3,x+width-18,rowY+3,line);rowY+=14;
        foreach(string note in notes){if(note.Length>0)p.Text(note,x+18,rowY,11,muted);rowY+=16;}
    }
}
