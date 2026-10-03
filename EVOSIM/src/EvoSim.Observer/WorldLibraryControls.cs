using EvoSim.Observer;
internal sealed partial class ObserverApp
{
    WorldSort worldSort;
    WorldFilter worldFilter;
    Rect librarySortRect, libraryFilterRect;
    string WorldSortLabel => worldSort switch { WorldSort.Name => "Ad · A–Z", WorldSort.Population => "Nüfus · çoktan aza", WorldSort.Time => "Süre · uzundan kısaya", _ => "Son kaydedilen" };
    string WorldFilterLabel => worldFilter switch { WorldFilter.Living => "Canlı var", WorldFilter.Empty => "Canlı yok", _ => "Tüm dünyalar" };
    void CycleWorldSort() { worldSort = (WorldSort)(((int)worldSort + 1) % 4); libraryPage = 0; slotActions = null; }
    void CycleWorldFilter() { worldFilter = (WorldFilter)(((int)worldFilter + 1) % 3); libraryPage = 0; slotActions = null; }
    void DrawLibraryControls(Rect sortRect, Rect filterRect)
    {
        librarySortRect = sortRect; libraryFilterRect = filterRect;
        Button(WorldSortLabel, sortRect, CycleWorldSort, worldSort != WorldSort.Recent, true);
        Button(WorldFilterLabel, filterRect, CycleWorldFilter, worldFilter != WorldFilter.All, true);
    }
}
