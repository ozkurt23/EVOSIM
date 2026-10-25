using System;
using System.Reflection;
namespace EvoSim.Core
{
 [AttributeUsage(AttributeTargets.Field)] public sealed class SettingAttribute:Attribute {
  public string Group,Label;public double Min,Max;
  public SettingAttribute(string group,string label,double min,double max){Group=group;Label=label;Min=min;Max=max;}
 }
 public sealed class WorldConfig {
  [Setting("Dünya","Harita boyutu",128,256)] public int MapSize=128;
  public int GeneratorVersion=2;
  [Setting("Dünya","Otçul sayısı",0,2000)] public int Herbivores=150;
  [Setting("Dünya","Etçil sayısı",0,2000)] public int Carnivores=15;
  [Setting("Dünya","Ağaç sayısı",0,350)] public int TreeCount=155;
  [Setting("Dünya","Kaya kümeleri",0,40)] public int RockClusters=15;
  [Setting("Dünya","Nehirle ayrılan kara bölgesi",1,5)] public int LandRegions=3;
  [Setting("Dünya","Nehir genişliği çarpanı",.4,2)] public float RiverWidth=1;
  [Setting("Dünya","Nadir yüzücü olasılığı",0,1)] public float SwimmerChance=.02f;
  [Setting("Otçul","Ömür (sn) · 0 = yaşlılıktan ölmez",0,100000)] public float HerbLifespan=1200;
  [Setting("Etçil","Ömür (sn) · 0 = yaşlılıktan ölmez",0,100000)] public float CarnLifespan=1200;
  [Setting("Otçul","Yaşlanma · 0 kapalı / 1 açık",0,1)] public int HerbAging=1;
  [Setting("Etçil","Yaşlanma · 0 kapalı / 1 açık",0,1)] public int CarnAging=1;
  [Setting("Otçul","Üreme biçimi · 0 kapalı / 1 eşeyli / 2 eşeysiz",0,2)] public int HerbBreedingMode=1;
  [Setting("Etçil","Üreme biçimi · 0 kapalı / 1 eşeyli / 2 eşeysiz",0,2)] public int CarnBreedingMode=1;
  [Setting("Otçul","Üreme isteği",0,1)] public float HerbReproductiveDrive=.5f;
  [Setting("Etçil","Üreme isteği",0,1)] public float CarnReproductiveDrive=.5f;
  public int BreedingMode(Creature c)=>c.Group==0?HerbBreedingMode:CarnBreedingMode;
  public float ReproductiveDrive(Creature c)=>c.Group==0?HerbReproductiveDrive:CarnReproductiveDrive;
  public float Lifespan(Creature c)=>c.Group==0?HerbLifespan:CarnLifespan;
  public bool Ages(Creature c)=>(c.Group==0?HerbAging:CarnAging)!=0;
  public float[] HerbMin={.15f,.25f,.02f,.08f,.1f,.2f},HerbMax={.8f,.85f,.14f,.43f,.6f,.85f};
  public float[] CarnMin={.15f,.25f,.84f,.08f,.55f,.2f},CarnMax={.8f,.85f,.98f,.43f,.95f,.85f};
  [Setting("Kalıtım","Doğum başına mutasyon olasılığı",0,1)] public float MutationRate=.001f;
  [Setting("Kalıtım","Küçük mutasyon sapması",0,1)] public float SmallSigma=.025f;
  [Setting("Kalıtım","Büyük mutasyon sapması",0,1)] public float LargeSigma=.10f;
  [Setting("Kalıtım","Büyük sapma olasılığı",0,1)] public float LargeChance=.05f;
  [Setting("Kalıtım","Beyin mutasyon çarpanı",0,20)] public float BrainMutationScale=4;
  [Setting("Kalıtım","Kurucu ağ ağırlık aralığı ±",.01,3)] public float BrainRange=.9f;
  [Setting("Beden","Yetişkinlik yaşı (sn)",10,1000)] public float MaturityAge=120;
  [Setting("Beden","Kurucu en küçük yaş (sn)",0,2000)] public float FounderAgeMin=120;
  [Setting("Beden","Kurucu en büyük yaş (sn)",0,2000)] public float FounderAgeMax=360;
  [Setting("Beden","Yaşlanmanın tamamlandığı yaş (sn)",100,5000)] public float MaxAge=1200;
  [Setting("Beden","Yaşlılık başlangıcı (sn)",0,4000)] public float DeclineAge=350;
  [Setting("Beden","Yaşlı hız oranı",.05,1)] public float OldSpeed=.4f;
  [Setting("Beden","Yaşlı çeviklik oranı",.05,1)] public float OldAgility=.5f;
  [Setting("Beden","Yaşlı iyileşme oranı",0,1)] public float OldHealing=.35f;
  [Setting("Beden","Temel hareket hızı",.1,12)] public float BaseSpeed=3.7f;
  [Setting("Beden","Dönüş hızı",.1,8)] public float TurnRate=3.2f;
  [Setting("Beden","İvme",.1,20)] public float Acceleration=7;
  [Setting("İhtiyaç","Kurucu enerji oranı",.05,1)] public float InitialEnergy=.84f;
  [Setting("İhtiyaç","Kurucu minimum su oranı",0,1)] public float InitialWaterMin=.85f;
  [Setting("İhtiyaç","Kurucu maksimum su oranı",0,1)] public float InitialWaterMax=1;
  [Setting("İhtiyaç","Açlık başlangıç oranı",.05,1)] public float HungerOnset=.60f;
  [Setting("İhtiyaç","Susuzluk başlangıç oranı",.05,1)] public float ThirstOnset=.65f;
  [Setting("İhtiyaç","Kritik ihtiyaç oranı",.01,.9)] public float CriticalNeed=.25f;
  [Setting("İhtiyaç","Kritik sinyal çarpanı",0,5)] public float CriticalBoost=2;
  [Setting("İhtiyaç","Taban enerji gideri / sn",0,2)] public float BasalCost=.15f;
  [Setting("İhtiyaç","Kütle enerji gideri",0,1)] public float MassCost=.10f;
  [Setting("İhtiyaç","Algı enerji gideri",0,1)] public float PerceptionCost=.065f;
  [Setting("İhtiyaç","Su uyumu enerji gideri",0,1)] public float AquaticCost=.035f;
  [Setting("İhtiyaç","Hareket enerji gideri",0,.5)] public float MoveCost=.018f;
  [Setting("İhtiyaç","Suda hareket gider çarpanı",1,8)] public float SwimCost=2.4f;
  [Setting("İhtiyaç","Su kaybı / sn",0,2)] public float WaterDrain=.38f;
  [Setting("İhtiyaç","Hareket su kaybı",0,.2)] public float MoveWater=.02f;
  [Setting("İhtiyaç","Kuraklık su kaybı çarpanı",1,4)] public float DryWater=1.3f;
  [Setting("İhtiyaç","İçme hızı / sn",1,80)] public float DrinkRate=24;
  [Setting("Kaynak","En küçük ağaç tacı yarıçapı",.5,6)] public float TreeRadiusMin=1.35f;
  [Setting("Kaynak","En büyük ağaç tacı yarıçapı",.5,6)] public float TreeRadiusMax=2.45f;
  [Setting("Kaynak","Ağaçlar arası en az mesafe",2,12)] public float TreeSpacing=3f;
  [Setting("Kaynak","Ağaç yerleşim nem katsayısı",0,1)] public float TreeMoistureAffinity=.66f;
  [Setting("Kaynak","Meyvenin taç yarıçapı oranı",0,1)] public float TreeFruitRadius=.8f;
  [Setting("Kaynak","Kuraklıkta meyve yenileme direnci",0,1)] public float FruitDroughtTolerance=.2f;
  [Setting("Kaynak","Ağaç başına meyve",1,12)] public int FruitSlots=7;
  [Setting("Kaynak","Bir meyvenin ham enerjisi",1,80)] public float FruitEnergy=18;
  [Setting("Kaynak","Meyve yenilenme tabanı (sn)",5,600)] public float FruitRegrow=45;
  [Setting("Kaynak","Meyveler arası ek bekleme (sn)",0,30)] public float FruitRegrowStep=4;
  [Setting("Kaynak","Meyve tüketme hızı / sn",.1,40)] public float FruitBiteRate=12;
  [Setting("Kaynak","Çayır alanı sayısı",0,30)] public int MeadowCount=13;
  [Setting("Kaynak","Çayır yatay yarıçapı",1,15)] public float MeadowX=6;
  [Setting("Kaynak","Çayır dikey yarıçapı",1,15)] public float MeadowY=4;
  [Setting("Kaynak","Ot kapasitesi çarpanı",.1,15)] public float GrassCapacity=3.5f;
  [Setting("Kaynak","Ot tüketme hızı / sn",.1,10)] public float GrassBiteRate=1.8f;
  [Setting("Kaynak","Ot yenilenme çarpanı",0,5)] public float GrassRegrowth=1;
  [Setting("Kaynak","Ceset ömrü (sn)",5,600)] public float CorpseLifetime=90;
  [Setting("Kaynak","Ceset çürüme hızı / sn",0,1)] public float CorpseDecay=.025f;
  [Setting("Üreme","Eşleşme temas mesafesi",0,4)] public float BreedContactRange=.8f;
  [Setting("Üreme","Doğum verimi",.1,1)] public float BirthEfficiency=.8f;
  [Setting("Üreme","Yavru enerji oranı",.05,.5)] public float ChildEnergy=.30f;
  [Setting("Üreme","Yavru su oranı",0,1)] public float ChildWater=.85f;
  [Setting("Üreme","Üreme için enerji oranı",.4,1)] public float BreedEnergy=.67f;
  [Setting("Üreme","Üreme için su seviyesi",0,100)] public float BreedWater=35;
  [Setting("Üreme","Üreme için sağlık",1,100)] public float BreedHealth=65;
  [Setting("Üreme","Doğumdan sonra bekleme (sn)",1,600)] public float BirthCooldown=60;
  [Setting("Üreme","İlk üreme beklemesi altı (sn)",0,300)] public float InitialCooldownMin=8;
  [Setting("Üreme","İlk üreme beklemesi üstü (sn)",0,300)] public float InitialCooldownMax=43;
  [Setting("Etkileşim","Isırma temel hasarı",0,60)] public float BiteDamage=15;
  [Setting("Etkileşim","Isırma enerji gideri",0,10)] public float BiteCost=1.2f;
  [Setting("Etkileşim","Isırma beklemesi (sn)",.1,10)] public float BiteCooldown=.8f;
  [Setting("Etkileşim","Büyüme hızı / sn",.05,3)] public float GrowthRate=.65f;
  [Setting("Etkileşim","Büyüme verimi",.1,1)] public float GrowthEfficiency=.8f;
  [Setting("Etkileşim","İyileşme / sn",0,3)] public float HealingRate=.42f;
  public void Validate(){foreach(var f in GetType().GetFields()){var a=f.GetCustomAttribute<SettingAttribute>();if(a==null)continue;double v=Convert.ToDouble(f.GetValue(this));if(double.IsNaN(v)||double.IsInfinity(v)||v<a.Min||v>a.Max)throw new ArgumentException(a.Label+": "+a.Min+"–"+a.Max+" aralığında olmalı.");}
   if(MapSize!=128&&MapSize!=192&&MapSize!=256)throw new ArgumentException("Harita boyutu 128, 192 veya 256 olmalı.");
   if(GeneratorVersion!=1&&GeneratorVersion!=2)throw new ArgumentException("Bu dünya üretici sürümü desteklenmiyor.");
   if(GeneratorVersion==1&&MapSize!=128)throw new ArgumentException("Eski dünya üreticisi yalnızca 128 boyutunu destekler.");
   if(TreeRadiusMin>TreeRadiusMax)throw new ArgumentException("En küçük ağaç tacı yarıçapı en büyüğünden büyük olamaz.");
   if(Herbivores+Carnivores==0)throw new ArgumentException("En az bir canlı gerekli.");
   if(FounderAgeMin>FounderAgeMax||FounderAgeMax>=MaxAge||DeclineAge>=MaxAge||MaturityAge>=MaxAge)throw new ArgumentException("Başlangıç/olgunluk/yaşlılık yaşları azami yaştan küçük olmalı.");
   if((Herbivores>0&&HerbLifespan>0&&FounderAgeMax>=HerbLifespan)||(Carnivores>0&&CarnLifespan>0&&FounderAgeMax>=CarnLifespan))throw new ArgumentException("Kurucu yaşları seçilen ömürden küçük olmalı; 0 ömür sınırsızdır.");
   if(InitialWaterMin>InitialWaterMax||InitialCooldownMin>InitialCooldownMax)throw new ArgumentException("Alt sınır üst sınırı aşamaz.");
   if(CriticalNeed>HungerOnset||CriticalNeed>ThirstOnset)throw new ArgumentException("Kritik eşik ihtiyaç başlangıcını aşamaz.");
   ValidateGenes(HerbMin,HerbMax);ValidateGenes(CarnMin,CarnMax);
  }
  static void ValidateGenes(float[] min,float[] max){if(min.Length!=6||max.Length!=6)throw new ArgumentException("Altı özellik gerekli.");for(int i=0;i<6;i++)if(float.IsNaN(min[i])||float.IsNaN(max[i])||min[i]<0||max[i]>1||min[i]>max[i])throw new ArgumentException("Özellik aralığı 0–1 içinde, alt ≤ üst olmalı.");}
  public WorldConfig Copy(){var c=(WorldConfig)MemberwiseClone();c.HerbMin=(float[])HerbMin.Clone();c.HerbMax=(float[])HerbMax.Clone();c.CarnMin=(float[])CarnMin.Clone();c.CarnMax=(float[])CarnMax.Clone();return c;}
  public static WorldConfig Preset(int index){var c=new WorldConfig();if(index==1){c.TreeCount=220;c.FruitEnergy=28;c.FruitRegrow=30;}else if(index==2){c.Herbivores=170;c.Carnivores=40;}else if(index==3){c.Herbivores=100;c.Carnivores=0;c.TreeCount=110;}return c;}
 }
}
