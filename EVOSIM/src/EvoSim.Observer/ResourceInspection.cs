using System.Globalization;
using System.Numerics;
using EvoSim.Core;

namespace EvoSim.Observer;

internal enum InspectableResourceKind { Tree, Grass, Water, Rock, Shelter, Corpse }
// Tree/shelter IDs are indices in immutable terrain; corpse IDs are stable creature IDs.
internal readonly record struct ResourceHandle(InspectableResourceKind Kind,int Id);
internal readonly record struct ResourceMetric(string Label,string Value);
internal readonly record struct InspectedFruitSlot(int Number,float RawEnergy,bool Occupied,double? ApproximateRegrowSeconds);
internal sealed record ResourceReading(ResourceHandle Handle,Vector2 Position,float Radius,string Title,string Subtitle,
    ResourceMetric[] Metrics,string[] Notes,InspectedFruitSlot[] FruitSlots,float? RawEnergy=null,float? LocalMoisture=null);

/// <summary>Observer-only descriptions of current, public Core state. Never steps or edits a world.</summary>
internal static class ResourceInspection
{
    static string Number(double value,string format="0.0")=>value.ToString(format,CultureInfo.InvariantCulture);
    static string Percent(double fraction)=>Number(fraction*100,"0")+"%";
    static ResourceMetric Metric(string label,double value,string unit="",string format="0.0")=>new(label,Number(value,format)+(unit.Length==0?"":" "+unit));
    internal static ResourceReading? Read(World world,ResourceHandle handle)
    {
        ArgumentNullException.ThrowIfNull(world);var map=world.Map;
        switch(handle.Kind)
        {
            case InspectableResourceKind.Tree:
            {
                if(handle.Id<0||handle.Id>=map.Trees.Count)return null;var tree=map.Trees[handle.Id];
                int cell=map.Cell(tree.X,tree.Y);if(cell<0)return null;
                var slots=new InspectedFruitSlot[tree.Fruit.Length];float total=0;int occupied=0;
                // Ecology updates once per simulated second. Water can change at each update,
                // so the current timer/rate ratio is an estimate, never an exact countdown.
                double rate=world.Config.FruitDroughtTolerance+(1-world.Config.FruitDroughtTolerance)*map.WaterLevel;
                for(int i=0;i<slots.Length;i++){
                    float stock=Math.Max(0,tree.Fruit[i]);bool present=stock>0;total+=stock;if(present)occupied++;
                    double? eta=present?null:rate>0?Math.Max(0,tree.Regrow[i])/rate:double.PositiveInfinity;
                    slots[i]=new(i+1,stock,present,eta);
                }
                return new(handle,new(tree.X,tree.Y),tree.Radius,"Meyve ağacı","Ağaç "+(handle.Id+1),
                    new[]{new ResourceMetric("Meyveli yuva",$"{occupied} / {slots.Length}"),Metric("Kalan ham enerji",total,format:"0.##"),new ResourceMetric("Yerel nem",Percent(map.Moisture[cell])),Metric("Taç yarıçapı",tree.Radius,"birim")},
                    new[]{"Yenilenme simülasyonda günde bir hesaplanır; ≈ süreler şimdiki su düzeyine göredir.","Meyve bitince o yuva yeniden büyür. Kısmen yenmiş meyve önce tükenir.","Taç meyvelerin uzaklığını belirler. Gövde hücresi hareketi ve görüşü keser."},slots,total,map.Moisture[cell]);
            }
            case InspectableResourceKind.Corpse:
            {
                var corpse=world.Corpses.FirstOrDefault(c=>c.CreatureId==handle.Id);if(corpse==null||corpse.Food<=0)return null;
                double age=Math.Max(0,(world.Tick-corpse.CreatedTick)*World.Dt),remaining=Math.Max(0,world.Config.CorpseLifetime-age);if(remaining<=0)return null;
                int cell=map.Cell(corpse.X,corpse.Y);if(cell<0)return null;
                return new(handle,new(corpse.X,corpse.Y),.55f,"Ceset","Canlı #"+corpse.CreatureId,
                    new[]{Metric("Kalan ham enerji",corpse.Food),Metric("En geç kaybolma",remaining,"gün"),Metric("Doğal çürüme",world.Config.CorpseDecay,"enerji/gün","0.###")},
                    new[]{"Yenerek veya çürüyerek daha önce tükenebilir.","Alınan enerji, yiyen canlının beslenme genine bağlıdır."},Array.Empty<InspectedFruitSlot>(),corpse.Food,map.Moisture[cell]);
            }
            case InspectableResourceKind.Shelter:
            {
                if(handle.Id<0||handle.Id>=map.Shelters.Count)return null;var point=map.Shelters[handle.Id];
                return new(handle,point,3.2f,"Dinlenme alanı","Doğal barınak",
                    new[]{new ResourceMetric("Dinlenirken taban gider","× 0.72"),new ResourceMetric("Dışarıda dinlenmeye göre","%20 daha az"),Metric("Etki yarıçapı",3.2,"birim")},
                    new[]{"Yalnızca dinlenme eyleminde etkilidir. Hareket dışındaki enerji giderini azaltır.","Besin veya su üretmez; iyileşme hızını doğrudan artırmaz."},Array.Empty<InspectedFruitSlot>());
            }
            default:
            {
                int cell=handle.Id;if(cell<0||cell>=map.CellCount)return null;int x=cell%map.Size,y=cell/map.Size;var point=new Vector2(x+.5f,y+.5f);float moisture=map.Moisture[cell];
                if(handle.Kind==InspectableResourceKind.Grass&&map.Tiles[cell]==0){
                    float stock=Math.Max(0,map.Food[cell]);
                    return new(handle,point,.5f,"Ot parçası","Yerdeki bitkisel besin",
                        new[]{Metric("Kalan ham enerji",stock),Metric("Ot yeme üst hızı",world.Config.GrassBiteRate,"enerji/gün"),Metric("Meyve yeme üst hızı",world.Config.FruitBiteRate,"enerji/gün"),new ResourceMetric("Yerel nem",Percent(moisture))},
                        new[]{"Küçük yer stokları ek besin sağlar. Stok ve yeme hızları dünya ayarlarına bağlıdır.","Ot ve meyvenin ham enerjisi aynı beslenme genine göre sindirilir.","Yenilenme; nem, su düzeyi ve mevcut stoğa bağlıdır."},Array.Empty<InspectedFruitSlot>(),stock,moisture);
                }
                if(handle.Kind==InspectableResourceKind.Water&&map.Tiles[cell]==1){
                    float supply=Math.Max(.15f,map.WaterLevel);
                    return new(handle,point,.5f,"Su kaynağı","Nehir veya göl hücresi",
                        new[]{new ResourceMetric("Dünya su düzeyi",Percent(map.WaterLevel)),new ResourceMetric("İçme arzı çarpanı","× "+Number(supply,"0.00")),Metric("İçme üst hızı",world.Config.DrinkRate*supply,"su/gün"),new ResourceMetric("Yüzme uyumu eşiği","≥ "+Number(Terrain.SwimmingThreshold,"0.00"))},
                        new[]{"Canlılar erişebildikleri kıyıdan içer. Arz çarpanının alt sınırı 0.15'tir.","Geçiş için yüzme uyumu eşiği gerekir. Suda hareket gideri ×"+Number(world.Config.SwimCost)+" olur."},Array.Empty<InspectedFruitSlot>());
                }
                if(handle.Kind==InspectableResourceKind.Rock&&map.Tiles[cell]==2&&!map.Trees.Any(t=>map.Cell(t.X,t.Y)==cell)){
                    return new(handle,point,.5f,"Kaya","Geçilmez arazi hücresi",
                        new[]{new ResourceMetric("Hareket","Engeller"),new ResourceMetric("Görüş","Keser")},
                        new[]{"Canlılar etrafından geçmek zorundadır.","Bu sürümde yapı malzemesi veya yenebilir kaynak değildir."},Array.Empty<InspectedFruitSlot>());
                }
                return null;
            }
        }
    }
}
