using EvoSim.Core;
using State=Godot.Collections.Dictionary;
using Rows=Godot.Collections.Array;
#if EVOSIM_FOREST
using EvoSim.Ecology;
#endif

public partial class SimulationBridge
{
#if EVOSIM_FOREST
    public bool LivingForest=>true;
    private static World CreateSimulationWorld(int seed,WorldConfig config,State values=null)
    {
        double Value(string key,double fallback)=>values!=null&&values.ContainsKey(key)?values[key].AsDouble():fallback;
        var settings=new ForestSettings(Value("ForestLifespan",60)*365,Value("ForestMaturity",3)*365,
            Value("ForestDroughtTolerance",.6),Value("ForestSeedSpread",.2),Value("ForestDensity",.5),
            Value("ForestCorpseContribution",.4),Value("ForestSoilHalfLife",2)*365);
        settings.Validate();return new World(seed,config,settings);
    }
    private int TreeId(int index)=>checked((int)world.TreeIdentity(index));
    private void AddTreeEcology(State row,int index)
    {
        var tree=world.Forest.Trees[index];
        row["growth"]=world.Forest.Settings.MaturityDays<=0?1:Math.Clamp(tree.Growth/world.Forest.Settings.MaturityDays,0,1);
        row["health"]=tree.Health;row["dead"]=tree.DeadDay>=0;row["age_days"]=tree.Age;
    }
    private static void AddForestSettings(Rows rows)
    {
        void Add(string key,string label,double value,bool unit,string help)=>rows.Add(new State{
            {"key",key},{"group","Yaşayan orman"},{"label",label},{"value",value},{"min",0d},
            {"max",unit?1d:100000d},{"integer",false},{"normalized",unit},{"help",help}});
        Add("ForestLifespan","Ağaç ömrü · yıl",60,false,"Bireysel ömür ±%15 değişir. 0: yaşlılıktan ölüm kapalı; kuraklık yine öldürebilir.");
        Add("ForestMaturity","Olgunlaşma · yıl",3,false,"Uygun koşullardaki büyüme süresi. Susuzluk ve komşu rekabeti uzatır; fidanlar meyve vermez.");
        Add("ForestDroughtTolerance","Kuraklık dayanıklılığı",.6,true,"Susuzluk sağlığı zaman içinde azaltır. Yağmur yaşayan ağaçları iyileştirir; ölüleri diriltmez.");
        Add("ForestSeedSpread","Tohum yayılımı",.2,true,"Olgun sağlıklı ağaçların yakın çevreye tohum bırakma olasılığını ölçekler. Tohumlar bir yıl canlı kalır.");
        Add("ForestDensity","Orman yoğunluğu",.5,true,"Fidanların tutunma şansını, asgari aralığını ve ışık/besin rekabetini birlikte belirler.");
        Add("ForestCorpseContribution","Çürümenin toprağa katkısı",.4,true,"Yenmemiş ve gerçekten çürüyen ceset kütlesi toprağı besler. Tohum ve su yoksa ağaç oluşmaz; katkı yerel olarak doygunluğa ulaşır.");
        Add("ForestSoilHalfLife","Toprak katkısı yarı ömrü · yıl",2,false,"Cesetten gelen ek verimliliğin yarıya inme süresi. 0: katkı sonraki günlük adımda kaybolur.");
    }
#else
    public bool LivingForest=>false;
    private static World CreateSimulationWorld(int seed,WorldConfig config,State values=null)=>new(seed,config);
    private int TreeId(int index)=>index;
    private void AddTreeEcology(State row,int index){}
    private static void AddForestSettings(Rows rows){}
#endif
    public Rows GetTreeStates()
    {
        var rows=new Rows();
        for(int i=0;i<world.Map.Trees.Count;i++)
        {
            var tree=world.Map.Trees[i];
            var row=new State{{"id",TreeId(i)},{"x",tree.X},{"z",tree.Y},{"radius",tree.Radius}};
            AddTreeEcology(row,i);rows.Add(row);
        }
        return rows;
    }
}
