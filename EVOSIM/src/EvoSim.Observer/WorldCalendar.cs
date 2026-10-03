using System.Globalization;
namespace EvoSim.Observer;

// Display calendar only: one legacy simulation second = one world day.
// Keep Core time, checkpoint values and replay tick arithmetic unchanged.
internal static class WorldCalendar
{
    internal const double DaysPerYear=365;
    internal static bool UsesYears(string key)=>key is "HerbLifespan" or "CarnLifespan" or "MaturityAge" or "FounderAgeMin" or "FounderAgeMax" or "MaxAge" or "DeclineAge";
    internal static string SettingLabel(string key,string label)=>label.Replace("(sn)",UsesYears(key)?"(yıl)":"(gün)").Replace("/ sn","/ gün");
    internal static string Age(double days)=>days<365?$"{days:0.#} gün":$"{days/365:0.##} yıl";
    internal static string Duration(double days,bool precise=false)
    {
        if(!double.IsFinite(days)||days<0)return "—";
        double totalMinutes=Math.Floor(days*1440+1e-4);
        double years=Math.Floor(totalMinutes/(365*1440));
        double rest=totalMinutes-years*365*1440;
        int d=(int)(rest/1440),h=(int)(rest%1440/60),m=(int)(rest%60);
        if(precise)return (years>0?$"{years:0} yıl ":"")+$"{d} gün {h:00}:{m:00}";
        if(years>0)return $"{years:0} yıl {d} gün";
        return $"{d} gün {h} saat";
    }
    internal static string CompactDuration(double days)=>days>=365?$"{days/365:0.#} yıl":days>=1?$"{Math.Ceiling(days):0} gün":$"{days*24:0.#} sa";
    internal static string YearInput(double days)=>(days/365).ToString("G17",CultureInfo.InvariantCulture);
}
