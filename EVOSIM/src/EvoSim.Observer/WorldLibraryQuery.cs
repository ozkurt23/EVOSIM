using System.Globalization;
namespace EvoSim.Observer;

internal enum WorldSort { Recent, Name, Population, Time }
internal enum WorldFilter { All, Living, Empty }
internal static class WorldLibraryQuery
{
    static readonly StringComparer TurkishNames = StringComparer.Create(CultureInfo.GetCultureInfo("tr-TR"), true);
    public static List<WorldSlot> Apply(IEnumerable<WorldSlot> source, WorldSort order, WorldFilter filter)
    {
        var selected = source.Where(s => filter == WorldFilter.All || (filter == WorldFilter.Living ? s.Living > 0 : s.Living == 0));
        IOrderedEnumerable<WorldSlot> sorted = order switch {
            WorldSort.Name => selected.OrderBy(s => s.Name, TurkishNames),
            WorldSort.Population => selected.OrderByDescending(s => s.Living),
            WorldSort.Time => selected.OrderByDescending(s => s.Tick),
            _ => selected.OrderByDescending(s => s.SavedUtc, StringComparer.Ordinal)
        };
        return sorted.ThenBy(s => s.Id, StringComparer.Ordinal).ToList();
    }
}
