using System.Reflection;
using EvoSim.Core;
using EvoSim.Observer;
using State=Godot.Collections.Dictionary;
using Rows=Godot.Collections.Array;

public partial class SimulationBridge
{
    public State CompareSavedWorlds(string first,string second)
    {
        var result=new State();Attempt(()=>{
            EnsureStore();var a=store.Load(Slot(first));var b=store.Load(Slot(second));
            var rows=new Rows();
            void Add(string label,double av,double bv)=>rows.Add(new State{{"label",label},{"a",av},{"b",bv}});
            Add("Gün",a.Time,b.Time);Add("Otçul",a.Herbivores,b.Herbivores);Add("Etçil",a.Carnivores,b.Carnivores);
            Add("Toplam doğum",a.Births,b.Births);Add("Toplam ölüm",a.Deaths,b.Deaths);Add("Ağaç / gövde",a.Map.Trees.Count,b.Map.Trees.Count);
            Add("Su erişim oranı",a.Map.WaterLevel,b.Map.WaterLevel);
            string[] names={"Beden","Algı","Beslenme","Su uyumu","Atılganlık","Sosyal bağ"};
            for(int i=0;i<6;i++)for(int diet=0;diet<2;diet++) {
                var av=a.Creatures.Where(c=>c.Alive && (c.Genes[2]>=.5f?1:0)==diet).Select(c=>(double)c.Genes[i]).ToArray();
                var bv=b.Creatures.Where(c=>c.Alive && (c.Genes[2]>=.5f?1:0)==diet).Select(c=>(double)c.Genes[i]).ToArray();
                // Empty groups have no mean; avoid presenting zero as an observation.
                if(av.Length>0&&bv.Length>0)Add(names[i]+(diet==0?" · otçul ortalaması":" · etçil ortalaması"),av.Average(),bv.Average());
            }
            var differences=new Godot.Collections.Array<string>();
            foreach(var field in typeof(WorldConfig).GetFields()) {
                var attribute=field.GetCustomAttribute<SettingAttribute>();if(attribute==null)continue;
                var av=field.GetValue(a.Config);var bv=field.GetValue(b.Config);
                if(!Equals(av,bv)) {double scale=WorldCalendar.UsesYears(field.Name)?365:1;differences.Add(WorldCalendar.SettingLabel(field.Name,attribute.Label)+$": {Convert.ToDouble(av)/scale:0.###} → {Convert.ToDouble(bv)/scale:0.###}");}
            }
            foreach(string group in new[]{"HerbMin","HerbMax","CarnMin","CarnMax"}) {
                var field=typeof(WorldConfig).GetField(group);var av=(float[])field.GetValue(a.Config);var bv=(float[])field.GetValue(b.Config);
                for(int i=0;i<6;i++)if(av[i]!=bv[i])differences.Add((group.StartsWith("Herb")?"Otçul ":"Etçil ")+names[i]+(group.EndsWith("Min")?" alt":" üst")+$": {av[i]:0.###} → {bv[i]:0.###}");
            }
#if EVOSIM_FOREST
            string[] labels={"Ağaç ömrü · yıl","Olgunlaşma · yıl","Kuraklık dayanıklılığı","Tohum yayılımı","Orman yoğunluğu","Çürümenin toprağa katkısı","Toprak katkısı yarı ömrü · yıl"};
            string[] properties={"LifespanDays","MaturityDays","DroughtTolerance","SeedSpread","Density","CorpseContribution","SoilHalfLifeDays"};
            for(int i=0;i<properties.Length;i++) {
                var property=a.Forest.Settings.GetType().GetProperty(properties[i]);double av=(double)property.GetValue(a.Forest.Settings),bv=(double)property.GetValue(b.Forest.Settings);
                if(av!=bv){double scale=properties[i].EndsWith("Days")?365:1;differences.Add(labels[i]+$": {av/scale:0.###} → {bv/scale:0.###}");}
            }
#endif
            result=new State{{"rows",rows},{"differences",differences},{"same_seed",a.Seed==b.Seed},{"a_seed",a.Seed},{"b_seed",b.Seed}};
        });return result;
    }
}
