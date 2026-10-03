using EvoSim.Observer;
using System.Text.Json;
internal sealed partial class ObserverApp
{
    void WorldLibraryQuerySmoke()
    {
        void Check(bool value,string message){if(!value)throw new Exception("Library query: "+message);}
        var records=new List<WorldSlot> {
            new(){Id="b",Name="İncir",Living=0,Tick=400,SavedUtc="2026-09-15T12:00:00Z"},
            new(){Id="a",Name="Ihlamur",Living=10,Tick=200,SavedUtc="2026-09-15T11:00:00Z"},
            new(){Id="c",Name="Çınar",Living=20,Tick=300,SavedUtc="2026-09-15T13:00:00Z"},
            new(){Id="d",Name="Ardıç",Living=20,Tick=100,SavedUtc="2026-09-15T13:00:00Z"}
        };
        string before=JsonSerializer.Serialize(records);
        Check(WorldLibraryQuery.Apply(records,WorldSort.Name,WorldFilter.All).Select(s=>s.Id).SequenceEqual(new[]{"d","c","a","b"}),"Turkish alphabetical order");
        Check(WorldLibraryQuery.Apply(records,WorldSort.Population,WorldFilter.Living).Select(s=>s.Id).SequenceEqual(new[]{"c","d","a"}),"descending count with stable id tie");
        Check(WorldLibraryQuery.Apply(records,WorldSort.Time,WorldFilter.All)[0].Id=="b","longest simulation first");
        Check(WorldLibraryQuery.Apply(records,WorldSort.Recent,WorldFilter.All).Select(s=>s.Id).SequenceEqual(new[]{"c","d","b","a"}),"latest save first with stable tie");
        Check(WorldLibraryQuery.Apply(records,WorldSort.Recent,WorldFilter.Empty).Single().Id=="b","empty includes zero population");
        Check(WorldLibraryQuery.Apply(Array.Empty<WorldSlot>(),WorldSort.Name,WorldFilter.All).Count==0,"empty library");
        Check(before==JsonSerializer.Serialize(records),"source order and metadata unchanged");
        var oldSort=worldSort;var oldFilter=worldFilter;int oldPage=libraryPage;bool oldMenu=mainMenu,oldLibrary=libraryOpen;var oldActions=slotActions;
        try
        {
            worldSort=WorldSort.Recent;worldFilter=WorldFilter.All;libraryPage=0;mainMenu=true;libraryOpen=false;
            var slots=store.List();string savedBefore=JsonSerializer.Serialize(slots);string hash=world.StateHash();
            Layout();Draw(1);
            void Click(Rect rect){var e=new Event{Type=0x401,Button=1,B=rect.X+15,C=rect.Y+15};SDL.SDL_PushEvent(ref e);ProcessEvents();}
            Click(librarySortRect);Check(worldSort==WorldSort.Name&&libraryPage==0,"main menu sort button");
            Click(libraryFilterRect);Check(worldFilter==WorldFilter.Living&&libraryPage==0,"main menu filter button");Draw(1);
            Click(libraryFilterRect);Check(worldFilter==WorldFilter.Empty,"empty filter selectable");Draw(1);
            mainMenu=false;libraryOpen=true;Draw(1);Click(librarySortRect);Check(worldSort==WorldSort.Population,"in-world library shares sort");
            Click(libraryFilterRect);Check(worldFilter==WorldFilter.All,"in-world filter cycles to all");Draw(1);
            Check(hash==world.StateHash()&&savedBefore==JsonSerializer.Serialize(store.List()),"UI query leaves live state and stored metadata unchanged");
            for(int i=0;i<4;i++)CycleWorldSort();Check(worldSort==WorldSort.Population,"sort cycle wraps");
        }
        finally{worldSort=oldSort;worldFilter=oldFilter;libraryPage=oldPage;mainMenu=oldMenu;libraryOpen=oldLibrary;slotActions=oldActions;}
    }
}
