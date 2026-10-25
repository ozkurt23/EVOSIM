using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace EvoSim.Core
{
    public sealed class Creature
    {
        public int Id, Group, Generation, ParentA, ParentB;
        public float X, Y, Vx, Vy, Angle, Age, Energy, Hydration, Health, Tissue, AdultTissue, Radius, Speed, ReproductionCooldown;
        public float[] Genes = new float[6];
        public float[] BrainWeights=new float[Brain.Count], BrainMemory=new float[Brain.Hidden], Senses=new float[Brain.Inputs], Motors=new float[Brain.Outputs];
        public float BlockedSeconds, RecentDamage, DistanceTravelled;
        public bool Alive = true;
        public string Action = "wander", Reason = "Çevreyi keşfediyor";
        public int TargetId;
        public float TargetX, TargetY;
        public float Maturity, AttackCooldown, ActionAge, ActionScore, LastSeenAge, PathAge, LastBirthCost;
        public double DeathTime = -1;
        public string DeathCause = "";
        public List<Vector2> Path = new List<Vector2>();
        public int PathIndex;
        public float MaxEnergy { get { return World.EnergyCapacity(this); } }
        public float MaxHydration { get { return 100f; } }
        internal float RepathX = float.NaN, RepathY=0, BlockedAge=0;
        internal float PreviousX, PreviousY;
        internal int FoodCell = -1, CorpseIndex = -1;
    }
    public sealed class Corpse
    {
        public float X, Y, Food;
        public int CreatureId;
        public long CreatedTick;
        public const float Lifetime=90f;
    }
    public sealed class SimEvent
    {
        public double Time;
        public string Text = "", Kind = "";
        public int ParentA, ParentB, ChildId;
        public float EnergySpent, ChildEnergy;
    }
    public sealed class HistorySample
    {
        public double Time;
        public int Herbivores, Carnivores, Births, Deaths;
    }

    /// <summary>
    /// Deterministic CPU reference preview. No Unity, rendering, wall-clock or GPU dependencies.
    /// Replaying a seed in the same runtime reproduces the same state. This is not the complete plan.
    /// </summary>
    public sealed partial class World
    {
        public const float Dt = 0.05f, BirthEfficiency = 0.8f, MutationProbability = 0.001f;
        public const float MaturityAge = 120f, MaximumAge = 1200f;
        public Terrain Map;
        public WorldConfig Config {get;private set;}
        public List<Creature> Creatures = new List<Creature>();
        public List<Corpse> Corpses = new List<Corpse>();
        public List<SimEvent> Events = new List<SimEvent>();
        public List<HistorySample> History = new List<HistorySample>();
        public long Tick;
        public int Seed, Births, Deaths, Mutations;
        public bool Drought;
        public double Time { get { return Tick * 0.05; } }
        public int Living { get { int n = 0; foreach (var c in Creatures) if (c.Alive) n++; return n; } }
        public int Herbivores { get { int n = 0; foreach (var c in Creatures) if (c.Alive && c.Genes[2] < .5f) n++; return n; } }
        public int Carnivores { get { return Living - Herbivores; } }

        private readonly PcgRandom _founders, _exploration, _inheritance, _mutation;
        private readonly PcgRandom _brainFounders, _brainInheritance;
        private int _nextId = 1;
        private readonly List<int>[] _buckets;
        private readonly int _bucketColumns;
        private readonly List<Creature> _neighbors = new List<Creature>();
        private readonly List<FoodRequest> _foodRequests = new List<FoodRequest>();
        private readonly Dictionary<ResourceKey, float> _foodTotals = new Dictionary<ResourceKey, float>();
        private readonly Dictionary<int, float> _damage = new Dictionary<int, float>();
        private readonly List<Creature> _birthCandidates = new List<Creature>();
        private struct FoodRequest { public Creature C; public ResourceKey Source; public float Amount, Efficiency; }

        public World(int seed = 42,WorldConfig config=null):this(seed,config,false) {}
        internal World(int seed,WorldConfig config,bool loading)
        {
            Config=(config??new WorldConfig()).Copy();Config.Validate();
            Seed = seed;
            ulong s = unchecked((ulong)(uint)seed);
            _founders = new PcgRandom(s, 11); _exploration = new PcgRandom(s, 23);
            _inheritance = new PcgRandom(s, 37); _mutation = new PcgRandom(s, 53);
            _brainFounders=new PcgRandom(s,61);_brainInheritance=new PcgRandom(s,67);
            Map = new Terrain(seed,Config);
            _bucketColumns=(Map.Size+7)/8;_buckets=new List<int>[_bucketColumns*_bucketColumns];
            RebuildResourceIndex();
            for (int i = 0; i < _buckets.Length; i++) _buckets[i] = new List<int>();
            if(loading)return;
            for (int i = 0; i < Config.Herbivores+Config.Carnivores; i++)
            {
                int group = i < Config.Herbivores ? 0 : 1;
                var c = new Creature { Id = _nextId++, Group = group, Generation = 0,
                    Age = Config.FounderAgeMin+_founders.NextFloat()*(Config.FounderAgeMax-Config.FounderAgeMin), Health=100, Hydration=100*(Config.InitialWaterMin+_founders.NextFloat()*(Config.InitialWaterMax-Config.InitialWaterMin)) };
                var lower=group==0?Config.HerbMin:Config.CarnMin;var upper=group==0?Config.HerbMax:Config.CarnMax;
                for(int g=0;g<6;g++)c.Genes[g]=lower[g]+(upper[g]-lower[g])*_founders.NextFloat();
                if(_founders.NextFloat()<Config.SwimmerChance)c.Genes[3]=.86f+.12f*_founders.NextFloat();
                c.AdultTissue = AdultTissueFor(c.Genes); c.Tissue = c.AdultTissue;
                c.Energy = EnergyCapacity(c) * Config.InitialEnergy; c.Angle = _founders.NextFloat() * 6.2831853f;
                UpdatePhenotype(c);
                Vector2 p = Map.FindSpawn(_founders, c.Radius, c.Genes[3]);
                for (int attempt = 0; attempt < 100 && Overlaps(p.X, p.Y, c.Radius, null); attempt++)
                    p = Map.FindSpawn(_founders, c.Radius, c.Genes[3]);
                if(Overlaps(p.X,p.Y,c.Radius,null)){
                    bool found=false;
                    for(int cell=0;cell<Map.CellCount;cell++){
                        float x=cell%Map.Size+.5f,y=cell/Map.Size+.5f;
                        if(Map.CanOccupy(x,y,c.Radius,c.Genes[3])&&!Overlaps(x,y,c.Radius,null)){p=new Vector2(x,y);found=true;break;}
                    }
                    if(!found)throw new ArgumentException("Bu koşullarda bütün kurucular için boş alan yok. Nüfusu veya engelleri azalt, haritayı büyüt.");
                }
                c.X = p.X; c.Y = p.Y; c.TargetX = c.X; c.TargetY = c.Y;
                c.ReproductionCooldown = Config.InitialCooldownMin+_founders.NextFloat()*(Config.InitialCooldownMax-Config.InitialCooldownMin);
                c.BrainWeights=Brain.Create(_brainFounders,Config.BrainRange);
                Creatures.Add(c);
            }
            Record("world", "Nehir vadisi başladı · "+Config.Herbivores+" otçul, "+Config.Carnivores+" etçil · kalıtsal ağ");
            SampleHistory();
            InitializeForest();
        }

        public static float EnergyCapacity(Creature c) { return 110f + c.Genes[0] * 90f; }
        public static float AdultTissueFor(float[] genes) { return 35f + genes[0] * 55f; }
        private static float Clamp(float x, float lo, float hi) { return Math.Max(lo, Math.Min(hi, x)); }
        private static float DistanceSquared(float ax, float ay, float bx, float by) { float x = ax - bx, y = ay - by; return x * x + y * y; }
        public static float NeedPressure(float ratio,float onset,float critical=.25f,float boost=2) { return Clamp((onset-ratio)/onset,0,1)+boost*Clamp((critical-ratio)/critical,0,1); }
        private static float Smooth(float x) { x = Clamp(x, 0, 1); return x * x * (3 - 2 * x); }

        public Creature Find(int id)
        {
            if (id > 0 && id <= Creatures.Count && Creatures[id - 1].Id == id) return Creatures[id - 1];
            foreach (var c in Creatures) if (c.Id == id) return c;
            return null;
        }
        public void SetDrought(bool drought)
        {
            if (Drought == drought) return;
            Drought = drought;
            Record("climate", drought ? "Kuraklık başladı · su erişimi ve bitki yenilenmesi azalıyor" : "Yağış düzeni geri döndü · su ve bitkiler kademeli toparlanıyor");
        }

        private void Record(string kind, string text)
        {
            Events.Add(new SimEvent { Time = Time, Kind = kind, Text = text });
            if (Events.Count > 2000) Events.RemoveRange(0, 500);
        }
        private void SampleHistory()
        {
            History.Add(new HistorySample { Time = Time, Herbivores = Herbivores, Carnivores = Carnivores, Births = Births, Deaths = Deaths });
        }
        private void UpdatePhenotype(Creature c)
        {
            c.AdultTissue = AdultTissueFor(c.Genes);
            float birthTissue = c.AdultTissue * .18f;
            c.Maturity = Math.Min(Smooth(c.Age / Config.MaturityAge), Clamp((c.Tissue - birthTissue) / (c.AdultTissue - birthTissue), 0, 1));
            float mass = .32f + (.8f + 1.4f * c.Genes[0] - .32f) * c.Maturity;
            c.Radius = .32f * (float)Math.Sqrt(mass);
            float senescence = 1 - (1-Config.OldSpeed) * Aging(c);
            float fatigue = .6f + .4f * Clamp(c.Energy / (EnergyCapacity(c) * .3f), 0, 1);
            c.Speed = Config.BaseSpeed * (float)Math.Pow(mass, -.25) * (.35f + .65f * c.Maturity)
                * (.35f + .65f * Clamp(c.Health / 100, 0, 1)) * fatigue * senescence;
        }
        public float Aging(Creature c) => Config.Ages(c) ? Smooth((c.Age-Config.DeclineAge)/(Config.MaxAge-Config.DeclineAge)) : 0;
        private float Sight(Creature c) { return (7f + 12f * c.Genes[1]) * (.4f + .6f * c.Maturity) * (1f - .2f * Aging(c)); }
        public float PerceptionRadius(Creature c) { return Sight(c); }
        public float PerceptionAngleDegrees(Creature c) { return (70f + 150f * c.Genes[1]) * (.6f + .4f * c.Maturity); }

        public void Step()
        {
            EnsureResourceIndex();
            Tick++;
            if (Tick % 20 == 0) { RefreshForestYields();Map.EcologyStep(Drought); StepForest(); }
            RebuildIndex();
            int count = Creatures.Count;
            for (int i = 0; i < count; i++)
            {
                Creature c = Creatures[i]; if (!c.Alive) continue;
                c.LastBirthCost = 0; c.ActionAge += Dt; c.LastSeenAge += Dt; c.PathAge += Dt;
                c.AttackCooldown = Math.Max(0, c.AttackCooldown - Dt);
                c.ReproductionCooldown = Math.Max(0, c.ReproductionCooldown - Dt);
                UpdatePhenotype(c);
                c.RecentDamage*=.96f;
                if ((Tick + c.Id) % 4 == 0) Decide(c);
                c.PreviousX = c.X; c.PreviousY = c.Y;
                Move(c);
            }
            ResolveInteractions(count);
            for (int i = 0; i < count; i++)
            {
                Creature c = Creatures[i]; if (!c.Alive) continue;
                float combatDamage;
                if (_damage.TryGetValue(c.Id, out combatDamage)) { c.Health -= combatDamage;c.RecentDamage=Math.Min(1,c.RecentDamage+combatDamage/50); }
                float mass = .32f + ( .8f + 1.4f * c.Genes[0] - .32f) * c.Maturity;
                float speedSq = c.Vx * c.Vx + c.Vy * c.Vy;
                float shelter = ShelterFactor(c);
                float moveCost = Config.MoveCost * mass * speedSq;
                if (Map.IsWater(c.X, c.Y)) moveCost *= Config.SwimCost;
                float cost = Dt * ((Config.BasalCost+Config.MassCost*mass+Config.PerceptionCost*c.Genes[1]+Config.AquaticCost*c.Genes[3]) * shelter + moveCost);
                float shortfall = Math.Max(0, cost - c.Energy); c.Energy = Math.Max(0, c.Energy - cost);
                c.Hydration = Math.Max(0, c.Hydration - Dt * (Config.WaterDrain+Config.MoveWater*speedSq)*(Drought?Config.DryWater:1));
                float hungerDamage = shortfall * 4.5f;
                float thirstDamage = c.Hydration <= 0 ? 2.3f * Dt : 0;
                c.Health -= hungerDamage + thirstDamage;
                c.Age += Dt;
                // A fatal hit or mandatory deficit latches death BEFORE any healing or reproduction.
                if (c.Health <= 0 || (Config.Lifespan(c)>0 && c.Age >= Config.Lifespan(c)))
                {
                    string cause = c.Health > 0 ? "yaşlılık" : combatDamage > Math.Max(hungerDamage, thirstDamage) ? "avlanma / çatışma" : thirstDamage >= hungerDamage ? "susuzluk" : "açlık";
                    Die(c, cause); continue;
                }
                float ageLimit = c.AdultTissue * .18f + c.AdultTissue * .82f * Smooth(c.Age / Config.MaturityAge);
                float tissueGain = Math.Min(Math.Max(0, ageLimit - c.Tissue), Config.GrowthRate * Dt);
                float growthPayment = tissueGain / Config.GrowthEfficiency;
                if (c.Energy >= growthPayment + EnergyCapacity(c) * .15f && tissueGain > 0)
                {
                    float oldTissue = c.Tissue; c.Tissue += tissueGain; UpdatePhenotype(c);
                    if (Map.HasClearPath(new Vector2(c.PreviousX, c.PreviousY), new Vector2(c.X, c.Y), c.Radius, c.Genes[3])) c.Energy -= growthPayment;
                    else { c.Tissue = oldTissue; UpdatePhenotype(c); }
                }
                if (c.Health < 100 && c.Energy > EnergyCapacity(c) * .4f && c.Hydration > 20)
                { float heal = Math.Min(100 - c.Health, Config.HealingRate*Dt*(1-(1-Config.OldHealing)*Aging(c))); c.Health += heal; c.Energy -= heal * .55f; }
            }
            ResolveBirths(count);
            // New corpses cannot be eaten in the tick of death. Decay has an explicit loss.
            for (int i = Corpses.Count - 1; i >= 0; i--)
            {
                var corpse=Corpses[i];float beforeDecay=corpse.Food;
                corpse.Food = Math.Max(0, corpse.Food - Dt * Config.CorpseDecay);
                bool expired=corpse.Food < .001f || (Tick-corpse.CreatedTick)*Dt>=Config.CorpseLifetime;
                double decay=beforeDecay-corpse.Food+(expired?corpse.Food:0);
                Forest.DepositDecay(Map.Cell(corpse.X,corpse.Y),decay);
                if(expired)Corpses.RemoveAt(i);
            }
            if (Tick % 20 == 0) SampleHistory();
        }

        private float ShelterFactor(Creature c)
        {
            if (c.Action != "rest") return 1;
            foreach (Vector2 p in Map.Shelters) if (DistanceSquared(c.X, c.Y, p.X, p.Y) < 3.2f * 3.2f) return .72f;
            return .9f;
        }
        private void RebuildIndex()
        {
            foreach (var bucket in _buckets) bucket.Clear();
            for (int i = 0; i < Creatures.Count; i++)
                if (Creatures[i].Alive) _buckets[Bucket(Creatures[i].X, Creatures[i].Y)].Add(i);
        }
        private int Bucket(float x, float y) { return (int)Clamp(x / 8, 0, _bucketColumns-1) + _bucketColumns * (int)Clamp(y / 8, 0, _bucketColumns-1); }
        private void Neighbors(Creature c, float radius)
        {
            _neighbors.Clear();
            int x0 = (int)Clamp((c.X - radius) / 8, 0, _bucketColumns-1), x1 = (int)Clamp((c.X + radius) / 8, 0, _bucketColumns-1);
            int y0 = (int)Clamp((c.Y - radius) / 8, 0, _bucketColumns-1), y1 = (int)Clamp((c.Y + radius) / 8, 0, _bucketColumns-1);
            for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++)
                foreach (int index in _buckets[x + _bucketColumns * y])
                {
                    var other = Creatures[index];
                    if (other.Alive && other != c && DistanceSquared(c.X, c.Y, other.X, other.Y) <= radius * radius) _neighbors.Add(other);
                }
            _neighbors.Sort((a, b) => a.Id.CompareTo(b.Id));
        }
        private bool Visible(Creature c, float x, float y, float range, bool peripheral = false)
        {
            float dx = x - c.X, dy = y - c.Y, distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance > range) return false;
            if (!peripheral && distance > 1.25f)
            {
                float halfAngle = (70f + 150f * c.Genes[1]) * (.6f + .4f * c.Maturity) * ((float)Math.PI / 360f);
                float dot = (dx * (float)Math.Cos(c.Angle) + dy * (float)Math.Sin(c.Angle)) / distance;
                if (dot < Math.Cos(halfAngle)) return false;
            }
            int steps = Math.Max(1, (int)(distance / .35f));
            for (int i = 1; i < steps; i++)
            {
                int cell = Map.Cell(c.X + dx * i / steps, c.Y + dy * i / steps);
                if (cell < 0 || Map.Tiles[cell] == 2) return false;
            }
            return true;
        }
        private void Decide(Creature c)
        {
            Array.Clear(c.Senses,0,c.Senses.Length);
            var input=c.Senses;float sight=Sight(c);
            input[0]=NeedPressure(c.Energy/c.MaxEnergy,Config.HungerOnset,Config.CriticalNeed,Config.CriticalBoost);input[1]=NeedPressure(c.Hydration/100,Config.ThirstOnset,Config.CriticalNeed,Config.CriticalBoost);input[2]=c.Health/100;
            input[3]=c.RecentDamage;input[4]=c.Maturity;input[5]=Config.BreedingMode(c)!=0&&c.ReproductionCooldown<=0?2*Config.ReproductiveDrive(c):0;
            float ca=(float)Math.Cos(c.Angle),sa=(float)Math.Sin(c.Angle);
            Action<int,float,float,float> encode=(start,x,y,range)=>{
                float dx=x-c.X,dy=y-c.Y,d=(float)Math.Sqrt(dx*dx+dy*dy);
                if(d<.001f)d=.001f;
                input[start]=(dx*ca+dy*sa)/d;input[start+1]=(-dx*sa+dy*ca)/d;input[start+2]=Math.Max(0,1-d/range);
            };
            float nearest=sight*sight;float fx=0,fy=0;
            int x0=Math.Max(0,(int)(c.X-sight)),x1=Math.Min(Map.Size-1,(int)(c.X+sight)),y0=Math.Max(0,(int)(c.Y-sight)),y1=Math.Min(Map.Size-1,(int)(c.Y+sight));
            for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++){
                int cell=x+Map.Size*y;if(Map.Food[cell]<.2f)continue;float d=DistanceSquared(c.X,c.Y,x+.5f,y+.5f);
                if(d<nearest&&Visible(c,x+.5f,y+.5f,sight)){nearest=d;fx=x+.5f;fy=y+.5f;}
            }
            foreach(var node in QueryFruit(c.X,c.Y,sight)){
                if(node.Tree.Fruit[node.Slot]<=0)continue;float d=DistanceSquared(c.X,c.Y,node.X,node.Y);
                if(d<nearest&&Visible(c,node.X,node.Y,sight)){nearest=d;fx=node.X;fy=node.Y;}
            }
            if(nearest<sight*sight)encode(6,fx,fy,sight);
            var shore=Map.NearestShore(c.X,c.Y,sight);
            if(!float.IsNaN(shore.X)&&Visible(c,shore.X,shore.Y,sight))encode(9,shore.X,shore.Y,sight);
            nearest=sight*sight;
            foreach(var corpse in Corpses){float d=DistanceSquared(c.X,c.Y,corpse.X,corpse.Y);if(d<nearest&&Visible(c,corpse.X,corpse.Y,sight)){nearest=d;fx=corpse.X;fy=corpse.Y;}}
            if(nearest<sight*sight)encode(12,fx,fy,sight);
            Neighbors(c,sight);Creature seen=null;nearest=sight*sight;
            foreach(var other in _neighbors){float d=DistanceSquared(c.X,c.Y,other.X,other.Y);if(d<nearest&&Visible(c,other.X,other.Y,sight)){nearest=d;seen=other;}}
            if(seen!=null){encode(15,seen.X,seen.Y,sight);input[18]=Math.Min(1,seen.Radius/.5f);input[19]=seen.Motors[4];input[26]=seen.Genes[2];input[27]=Math.Min(1,(float)Math.Sqrt(seen.Vx*seen.Vx+seen.Vy*seen.Vy)/5);}
            for(int j=0;j<3;j++){
                float angle=c.Angle+(j-1)*.8f;float clear=0;
                for(float d=.25f;d<=3;d+=.25f){if(!Map.CanOccupy(c.X+(float)Math.Cos(angle)*d,c.Y+(float)Math.Sin(angle)*d,c.Radius,c.Genes[3]))break;clear=d;}
                input[20+j]=1-clear/3;
            }
            input[23]=Math.Min(1,c.BlockedSeconds/3);input[24]=c.Vx*ca+c.Vy*sa;input[24]/=5;input[25]=c.Genes[2];input[28]=c.Genes[4];input[29]=c.Genes[5];
            Brain.Evaluate(c.BrainWeights,input,c.BrainMemory,c.Motors);
            // UI describes raw outputs, never claims a learned intention or assigns a reward.
            c.Action=Math.Abs(c.Motors[0])>.15f?"move":"rest";
            c.Reason="Kalıtsal ağ: hareket "+c.Motors[0].ToString("0.00")+", dönüş "+c.Motors[1].ToString("0.00")+", etkileşim "+c.Motors[2].ToString("0.00");
            c.Path.Clear();c.PathIndex=0;c.TargetId=0;
        }
        private void Move(Creature c)
        {
            // Steering is entirely a network output. Wall contact removes penetration;
            // it never selects an escape heading or a new target for the controller.
            c.Angle+=c.Motors[1]*Config.TurnRate*Dt;
            float drive=Math.Abs(c.Motors[0])<.12f?0:c.Motors[0];
            float speed=c.Speed*(Map.IsWater(c.X,c.Y)?.58f:1f);
            float wantedX=(float)Math.Cos(c.Angle)*speed*drive,wantedY=(float)Math.Sin(c.Angle)*speed*drive;
            float ax=wantedX-c.Vx,ay=wantedY-c.Vy,delta=(float)Math.Sqrt(ax*ax+ay*ay),limit=Config.Acceleration*Dt*(1-(1-Config.OldAgility)*Aging(c))/(.6f+c.Genes[0]);
            if(delta>limit){ax*=limit/delta;ay*=limit/delta;}
            float nx=c.X+(c.Vx+ax)*Dt,ny=c.Y+(c.Vy+ay)*Dt,ox=c.X,oy=c.Y;
            var from=new Vector2(ox,oy);
            if(Map.HasClearPath(from,new Vector2(nx,ny),c.Radius,c.Genes[3])){c.X=nx;c.Y=ny;}
            else if(Map.HasClearPath(from,new Vector2(nx,oy),c.Radius,c.Genes[3]))c.X=nx;
            else if(Map.HasClearPath(from,new Vector2(ox,ny),c.Radius,c.Genes[3]))c.Y=ny;
            c.Vx=(c.X-ox)/Dt;c.Vy=(c.Y-oy)/Dt;
            float distance=(float)Math.Sqrt((c.X-ox)*(c.X-ox)+(c.Y-oy)*(c.Y-oy));c.DistanceTravelled+=distance;
            c.BlockedSeconds=Math.Abs(drive)>.2f&&distance<.002f?c.BlockedSeconds+Dt:0;
            c.TargetX=c.X+(float)Math.Cos(c.Angle)*drive*3;c.TargetY=c.Y+(float)Math.Sin(c.Angle)*drive*3;
        }

        private void ResolveInteractions(int count)
        {
            EnsureResourceIndex();
            _foodRequests.Clear();_foodTotals.Clear();_damage.Clear();
            for(int i=0;i<count;i++){
                var c=Creatures[i];if(!c.Alive)continue;
                // User-requested ingestion reflex, only in physical reach. It supplies no direction.
                if(c.Motors[2]>0||NeedPressure(c.Energy/c.MaxEnergy,Config.HungerOnset,Config.CriticalNeed,Config.CriticalBoost)>0||NeedPressure(c.Hydration/100,Config.ThirstOnset,Config.CriticalNeed,Config.CriticalBoost)>0){
                    if(!float.IsNaN(Map.NearestShore(c.X,c.Y,1.5f).X)){
                        c.Hydration=Math.Min(100,c.Hydration+Config.DrinkRate*Dt*Math.Max(.15f,Map.WaterLevel));c.Action="drink";
                    }
                    float plant=.05f+.95f*(1-c.Genes[2])*(1-c.Genes[2]),meat=.05f+.95f*c.Genes[2]*c.Genes[2];
                    ResourceKey source=default;float efficiency=plant,rate=Config.GrassBiteRate,best=.9f*.9f;
                    int cx=(int)c.X,cy=(int)c.Y;
                    for(int y=Math.Max(0,cy-1);y<=Math.Min(Map.Size-1,cy+1);y++)for(int x=Math.Max(0,cx-1);x<=Math.Min(Map.Size-1,cx+1);x++){
                        int cell=x+Map.Size*y;float d=DistanceSquared(c.X,c.Y,x+.5f,y+.5f);
                        if(Map.Food[cell]>.001f&&d<best&&Visible(c,x+.5f,y+.5f,1.5f,true)){source=new ResourceKey(ResourceKind.Grass,cell);best=d;}
                    }
                    // A mouth uses the nearest reachable edible item, not a strategic goal.
                    foreach(var node in QueryFruit(c.X,c.Y,.9f)){
                        if(node.Tree.Fruit[node.Slot]<=0)continue;float d=DistanceSquared(c.X,c.Y,node.X,node.Y);
                        if(d<best&&Visible(c,node.X,node.Y,1.5f,true)){best=d;source=new ResourceKey(ResourceKind.Fruit,node.TreeIndex,node.Slot);rate=Config.FruitBiteRate;efficiency=plant;}
                    }
                    for(int j=0;j<Corpses.Count;j++){var corpse=Corpses[j];float d=DistanceSquared(c.X,c.Y,corpse.X,corpse.Y);
                        if(corpse.CreatedTick<Tick&&d<best&&Visible(c,corpse.X,corpse.Y,1.5f,true)){source=new ResourceKey(ResourceKind.Corpse,j);best=d;rate=Config.FruitBiteRate;efficiency=meat;}}
                    if(source.Kind!=ResourceKind.None){float amount=Math.Min(rate*Dt,Math.Max(0,c.MaxEnergy-c.Energy)/efficiency);
                        _foodRequests.Add(new FoodRequest{C=c,Source=source,Amount=amount,Efficiency=efficiency});float sum;_foodTotals.TryGetValue(source,out sum);_foodTotals[source]=sum+amount;
                        if(amount>0)c.Action=source.Kind==ResourceKind.Fruit?"fruit":source.Kind==ResourceKind.Corpse?"scavenge":"eat";}
                }
                if(c.Motors[3]>0&&c.AttackCooldown<=0&&c.Energy>=Config.BiteCost){
                    Neighbors(c,1.5f);Creature touched=null;float best=float.MaxValue;
                    foreach(var other in _neighbors){float d=DistanceSquared(c.X,c.Y,other.X,other.Y);float reach=c.Radius+other.Radius+.25f;
                        if(d<best&&d<reach*reach&&Visible(c,other.X,other.Y,reach,true)){best=d;touched=other;}}
                    if(touched!=null){float damage=Config.BiteDamage*(.6f+c.Genes[0])*(.35f+.65f*c.Maturity)*(.4f+.6f*c.Health/100);float old;_damage.TryGetValue(touched.Id,out old);_damage[touched.Id]=old+damage;
                        c.Energy-=Config.BiteCost;c.AttackCooldown=Config.BiteCooldown+.3f*c.Genes[0];c.Action="bite";}
                }
            }
            var stocks=new Dictionary<ResourceKey,float>();
            foreach(var request in _foodRequests)if(!stocks.ContainsKey(request.Source))stocks[request.Source]=Stock(request.Source);
            foreach(var request in _foodRequests){float fraction=_foodTotals[request.Source]>0?Math.Min(1,stocks[request.Source]/_foodTotals[request.Source]):0;float taken=request.Amount*fraction;
                request.C.Energy=Math.Min(request.C.MaxEnergy,request.C.Energy+taken*request.Efficiency);Consume(request.Source,taken);}
        }
        private float Stock(ResourceKey source){switch(source.Kind){
            case ResourceKind.Fruit:return Map.Trees[source.Index].Fruit[source.Slot];
            case ResourceKind.Corpse:return Corpses[source.Index].Food;
            case ResourceKind.Grass:return Map.Food[source.Index];
            default:throw new InvalidOperationException("Unknown food source");}}
        private void Consume(ResourceKey source,float amount){
            if(source.Kind==ResourceKind.Fruit){var tree=Map.Trees[source.Index];int slot=source.Slot;
                tree.Fruit[slot]=Math.Max(0,tree.Fruit[slot]-amount);if(tree.Fruit[slot]<.001f){tree.Fruit[slot]=0;tree.Regrow[slot]=Config.FruitRegrow+slot*Config.FruitRegrowStep;}return;}
            if(source.Kind==ResourceKind.Corpse)Corpses[source.Index].Food=Math.Max(0,Corpses[source.Index].Food-amount);
            else if(source.Kind==ResourceKind.Grass)Map.Food[source.Index]=Math.Max(0,Map.Food[source.Index]-amount);
        }

        private bool ReadyToBreed(Creature c)
        { return c.Alive && Config.BreedingMode(c)!=0 && c.Age >= Config.MaturityAge && c.Maturity >= .98f && c.ReproductionCooldown <= 0 && c.Energy >= EnergyCapacity(c)*Config.BreedEnergy && c.Hydration>Config.BreedWater && c.Health>Config.BreedHealth; }
        private void ResolveBirths(int count)
        {
            _birthCandidates.Clear();
            for (int i = 0; i < count; i++) if (Creatures[i].Motors[4]>0 && ReadyToBreed(Creatures[i])) _birthCandidates.Add(Creatures[i]);
            foreach (Creature a in _birthCandidates)
            {
                if (!ReadyToBreed(a)) continue;
                bool asexual=Config.BreedingMode(a)==2;
                Creature b=null;
                if(!asexual){
                    float closest=float.MaxValue;
                    foreach(var candidate in _birthCandidates){if(candidate==a||candidate.Group!=a.Group||!ReadyToBreed(candidate))continue;
                        float d=DistanceSquared(a.X,a.Y,candidate.X,candidate.Y);
                        float reach=a.Radius+candidate.Radius+Config.BreedContactRange;
                        if(d<closest&&d<reach*reach&&Visible(a,candidate.X,candidate.Y,2,true)){closest=d;b=candidate;}}
                    if(b==null||b.Motors[4]<=0)continue;
                    float contact=a.Radius+b.Radius+Config.BreedContactRange;
                    if(DistanceSquared(a.X,a.Y,b.X,b.Y)>contact*contact)continue;
                }
                Vector2 position = new Vector2(float.NaN, float.NaN);
                // Validate placement at a conservative newborn radius BEFORE taking random genome draws.
                for (int j = 0; j < 16; j++)
                {
                    float angle = j * (2f * (float)Math.PI / 16) + (a.Id % 7) * .2f;
                    float px = (asexual?a.X:(a.X+b.X)*.5f) + (float)Math.Cos(angle) * 1.5f;
                    float py = (asexual?a.Y:(a.Y+b.Y)*.5f) + (float)Math.Sin(angle) * 1.5f;
                    if (!Map.IsWater(px, py) && Map.CanOccupy(px, py, .25f, 0) && !Overlaps(px, py, .25f, null)) { position = new Vector2(px, py); break; }
                }
                if (float.IsNaN(position.X)) continue;
                bool mutated; int locus;
                float[] genes = asexual?(float[])a.Genes.Clone():Genetics.Inherit(a.Genes, b.Genes, _inheritance, _mutation, out mutated, out locus, 0);
                float[] brain=asexual?(float[])a.BrainWeights.Clone():Brain.Inherit(a.BrainWeights,b.BrainWeights,_brainInheritance);
                mutated=_mutation.NextFloat()<Config.MutationRate;locus=-1;
                if(mutated){int index=_mutation.NextInt(6+Brain.Count);float sigma=_mutation.NextFloat()<Config.LargeChance?Config.LargeSigma:Config.SmallSigma;
                    if(index<6){locus=index;genes[index]=Genetics.Reflect01(genes[index]+sigma*_mutation.Gaussian());}
                    else{locus=6;brain[index-6]=Clamp(brain[index-6]+sigma*Config.BrainMutationScale*_mutation.Gaussian(),-3,3);}}

                var child = new Creature { Id = _nextId, Group = a.Group, ParentA = a.Id, ParentB = asexual?0:b.Id,
                    Generation = 1 + (asexual?a.Generation:Math.Max(a.Generation, b.Generation)), Genes = genes, BrainWeights=brain, X = position.X, Y = position.Y,
                    TargetX = position.X, TargetY = position.Y, Health=100,Hydration=100*Config.ChildWater,Angle=a.Angle };
                child.AdultTissue = AdultTissueFor(genes); child.Tissue = child.AdultTissue * .18f;
                child.Energy = EnergyCapacity(child) * Config.ChildEnergy;
                float total = (child.Tissue + child.Energy) / Config.BirthEfficiency;
                float parentCost=asexual?total:total*.5f;
                if(a.Energy<parentCost||(!asexual&&b.Energy<parentCost))continue;
                a.Energy-=parentCost;a.LastBirthCost=parentCost;a.ReproductionCooldown=Config.BirthCooldown;
                if(!asexual){b.Energy-=parentCost;b.LastBirthCost=parentCost;b.ReproductionCooldown=Config.BirthCooldown;}
                UpdatePhenotype(child); _nextId++; Creatures.Add(child); Births++;
                if (mutated) Mutations++;
                Events.Add(new SimEvent { Time = Time, Kind = mutated ? "mutation" : "birth",
                    Text = mutated ? "#" + child.Id + " doğdu · nadir mutasyon: " + (locus==6?"karar ağı":Genetics.Names[locus]) : "#" + child.Id + (asexual?" doğdu · tek ebeveyn #"+a.Id:" doğdu · ebeveynler #" + a.Id + " + #" + b.Id),
                    ParentA = a.Id, ParentB = asexual?0:b.Id, ChildId = child.Id, EnergySpent = total, ChildEnergy = child.Tissue + child.Energy });
                if (Events.Count > 2000) Events.RemoveRange(0, 500);
            }
        }
        private bool Overlaps(float x, float y, float radius, Creature ignore)
        {
            foreach (var other in Creatures)
                if (other.Alive && other != ignore && DistanceSquared(x, y, other.X, other.Y) < (radius + other.Radius + .05f) * (radius + other.Radius + .05f)) return true;
            return false;
        }
        private void Die(Creature c, string cause)
        {
            if (!c.Alive) return;
            c.Alive = false; c.Health = Math.Max(0, c.Health); c.DeathTime = Time; c.DeathCause = cause;
            c.Vx = c.Vy = 0; c.Action = "dead"; c.Reason = cause; Deaths++;
            Corpses.Add(new Corpse { X = c.X, Y = c.Y, Food = Math.Max(0, c.Energy + c.Tissue), CreatureId = c.Id, CreatedTick = Tick });
            c.Energy = 0; c.Tissue = 0;
            Record("death", "#" + c.Id + " öldü · " + cause);
        }

        public List<Creature> GetDescendants(int id, int depth = 0, bool livingOnly = false)
        {
            if (depth < 0) throw new ArgumentOutOfRangeException(nameof(depth));
            var frontier = new HashSet<int> { id }; var all = new HashSet<int>(); int level = 0;
            while (frontier.Count > 0 && (depth == 0 || level < depth))
            {
                var next = new HashSet<int>();
                foreach (var c in Creatures)
                    if (frontier.Contains(c.ParentA) || frontier.Contains(c.ParentB))
                    { if (depth != 0 || !all.Contains(c.Id)) next.Add(c.Id); all.Add(c.Id); }
                frontier = next; level++;
            }
            HashSet<int> ids = depth == 0 ? all : frontier;
            var result = new List<Creature>();
            foreach (var c in Creatures) if (ids.Contains(c.Id) && (!livingOnly || c.Alive)) result.Add(c);
            return result;
        }

        public string StateHash()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    WriteRecords(writer,new List<WorldConfig>{Config});writer.Write(Map.Size);
                    writer.Write(Seed); writer.Write(Tick); writer.Write(Drought); writer.Write(Births); writer.Write(Deaths); writer.Write(Mutations); writer.Write(_nextId);
                    foreach(var rng in new[]{_founders,_exploration,_inheritance,_mutation,_brainFounders,_brainInheritance}){writer.Write(rng.State);writer.Write(rng.Increment);}
                    writer.Write(Map.WaterLevel);
                    writer.Write(Map.Trees.Count);foreach(var tree in Map.Trees){writer.Write(tree.X);writer.Write(tree.Y);writer.Write(tree.Radius);writer.Write(tree.FruitRadius);writer.Write(tree.Fruit.Length);foreach(float f in tree.Fruit)writer.Write(f);foreach(float r in tree.Regrow)writer.Write(r);}
                    foreach (float food in Map.Food) writer.Write(food);
                    foreach (float moisture in Map.Moisture) writer.Write(moisture);
                    foreach (byte tile in Map.Tiles) writer.Write(tile);
                    foreach (var c in Creatures)
                    {
                        foreach(float w in c.BrainWeights)writer.Write(w);foreach(float m in c.BrainMemory)writer.Write(m);foreach(float o in c.Motors)writer.Write(o);
                        writer.Write(c.BlockedSeconds);writer.Write(c.RecentDamage);writer.Write(c.DistanceTravelled);
                        writer.Write(c.Id); writer.Write(c.Group); writer.Write(c.Generation); writer.Write(c.ParentA); writer.Write(c.ParentB); writer.Write(c.Alive);
                        writer.Write(c.X); writer.Write(c.Y); writer.Write(c.Vx); writer.Write(c.Vy); writer.Write(c.Angle); writer.Write(c.Age);
                        writer.Write(c.Energy); writer.Write(c.Hydration); writer.Write(c.Health); writer.Write(c.Tissue); writer.Write(c.ReproductionCooldown); writer.Write(c.AttackCooldown);
                        writer.Write(c.Action); writer.Write(c.TargetId); writer.Write(c.TargetX); writer.Write(c.TargetY); writer.Write(c.ActionAge); writer.Write(c.ActionScore);
                        writer.Write(c.LastSeenAge); writer.Write(c.PathAge); writer.Write(c.RepathX); writer.Write(c.RepathY); writer.Write(c.BlockedAge);
                        writer.Write(c.PathIndex); writer.Write(c.FoodCell); writer.Write(c.CorpseIndex); foreach (float gene in c.Genes) writer.Write(gene);
                        writer.Write(c.Path.Count); foreach (var p in c.Path) { writer.Write(p.X); writer.Write(p.Y); }
                    }
                    byte[] forestState=Forest.Save();writer.Write(ForestOriginDay);writer.Write(forestState.Length);writer.Write(forestState);
                    writer.Write(Corpses.Count);
                    foreach (var corpse in Corpses) { writer.Write(corpse.X); writer.Write(corpse.Y); writer.Write(corpse.Food); writer.Write(corpse.CreatureId); writer.Write(corpse.CreatedTick); }
                }
                using (SHA256 sha = SHA256.Create())
                {
                    byte[] hash = sha.ComputeHash(stream.ToArray()); var text = new StringBuilder(64);
                    foreach (byte b in hash) text.Append(b.ToString("x2")); return text.ToString();
                }
            }
        }
    }
}
