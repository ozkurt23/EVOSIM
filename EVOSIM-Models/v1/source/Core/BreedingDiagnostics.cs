using System;
using System.Collections.Generic;

namespace EvoSim.Core
{
    /// <summary>
    /// Mutually exclusive first failing gates, evaluated in the documented order.
    /// These are a current snapshot, not accumulated event or creature-time counters.
    /// </summary>
    public sealed class BreedingGateCounts
    {
        public int Disabled { get; internal set; }
        public int Immature { get; internal set; }
        public int Cooldown { get; internal set; }
        public int LowEnergy { get; internal set; }
        public int LowWater { get; internal set; }
        public int LowHealth { get; internal set; }
        public int OtherIneligible { get; internal set; }
        public int NoPositiveMateSignal { get; internal set; }
        public int ReadyWithSignal { get; internal set; }
        public string Order { get { return "disabled, immature, cooldown, low energy, low water, low health, other ineligible, no positive mate signal, ready with signal"; } }
    }

    /// <summary>
    /// All direct condition counts may overlap and use living creatures as denominator.
    /// Ready counts additionally require the current model's mode-aware ReadyToBreed.
    /// Contact counts apply only to sexual, ready, positively signalling individuals;
    /// they do not promise placement, final payment or an actual birth.
    /// </summary>
    public sealed class BreedingDiagnostics
    {
        public long Tick { get; internal set; }
        public int Living { get; internal set; }
        public int Disabled { get; internal set; }
        public int Immature { get; internal set; }
        public int Cooldown { get; internal set; }
        public int LowEnergy { get; internal set; }
        public int LowWater { get; internal set; }
        public int LowHealth { get; internal set; }
        public int NoPositiveMateSignal { get; internal set; }
        public int ReadyWithSignal { get; internal set; }
        public int ReadyWithoutSignal { get; internal set; }
        public int SexualReadyWithSignal { get; internal set; }
        public int AsexualReadyWithSignal { get; internal set; }
        public int SexualReadyWithNearbyPartner { get; internal set; }
        public long NearbyCompatibleReadyPairs { get; internal set; }
        public BreedingGateCounts ExclusiveFirstBlocker { get; internal set; }
        public string ConditionSemantics { get { return "Direct condition counts overlap; ExclusiveFirstBlocker partitions living creatures. Contact pairs are unique unordered sexual pairs, not births."; } }
    }

    public sealed partial class World
    {
        /// <summary>
        /// Computes diagnostics only when explicitly requested. Does not use RNG,
        /// update senses, rebuild the world's spatial index, or write model state.
        /// A private request-local grid bounds contact search to the existing 2-unit
        /// visibility limit; a densely occupied cell can still require pairwise work.
        /// </summary>
        public BreedingDiagnostics InspectBreeding()
        {
            var result = new BreedingDiagnostics { Tick = Tick, ExclusiveFirstBlocker = new BreedingGateCounts() };
            var sexualReady = new List<Creature>();
            var grid = new Dictionary<long, List<Creature>>();
            foreach (var c in Creatures)
            {
                if (!c.Alive) continue;
                result.Living++;
                int mode = c.Group == 0 ? Config.HerbBreedingMode : Config.CarnBreedingMode;
                bool disabled = mode == 0;
                bool immature = c.Age < Config.MaturityAge || c.Maturity < .98f;
                bool cooldown = c.ReproductionCooldown > 0;
                bool lowEnergy = c.Energy < EnergyCapacity(c) * Config.BreedEnergy;
                bool lowWater = c.Hydration <= Config.BreedWater;
                bool lowHealth = c.Health <= Config.BreedHealth;
                bool noSignal = c.Motors[4] <= 0;
                bool ready = ReadyToBreed(c);
                if (disabled) result.Disabled++;
                if (immature) result.Immature++;
                if (cooldown) result.Cooldown++;
                if (lowEnergy) result.LowEnergy++;
                if (lowWater) result.LowWater++;
                if (lowHealth) result.LowHealth++;
                if (noSignal) result.NoPositiveMateSignal++;

                var gates = result.ExclusiveFirstBlocker;
                if (disabled) gates.Disabled++;
                else if (immature) gates.Immature++;
                else if (cooldown) gates.Cooldown++;
                else if (lowEnergy) gates.LowEnergy++;
                else if (lowWater) gates.LowWater++;
                else if (lowHealth) gates.LowHealth++;
                else if (!ready) gates.OtherIneligible++;
                else if (noSignal) gates.NoPositiveMateSignal++;
                else gates.ReadyWithSignal++;

                if (!ready) continue;
                if (noSignal) { result.ReadyWithoutSignal++; continue; }
                result.ReadyWithSignal++;
                if (mode == 2) { result.AsexualReadyWithSignal++; continue; }
                if (mode != 1) continue;
                result.SexualReadyWithSignal++;
                sexualReady.Add(c);
                long key = BreedingGridKey((int)Math.Floor(c.X / 2f), (int)Math.Floor(c.Y / 2f));
                List<Creature> cell;
                if (!grid.TryGetValue(key, out cell)) { cell = new List<Creature>(); grid.Add(key, cell); }
                cell.Add(c);
            }

            var withPartner = new HashSet<int>();
            foreach (var a in sexualReady)
            {
                int gx = (int)Math.Floor(a.X / 2f), gy = (int)Math.Floor(a.Y / 2f);
                for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    List<Creature> cell;
                    if (!grid.TryGetValue(BreedingGridKey(gx + dx, gy + dy), out cell)) continue;
                    foreach (var b in cell)
                    {
                        if (b.Id <= a.Id || b.Group != a.Group) continue;
                        float x = a.X - b.X, y = a.Y - b.Y;
                        float distanceSquared = x * x + y * y;
                        float contact = a.Radius + b.Radius + Config.BreedContactRange;
                        if (distanceSquared >= contact * contact || distanceSquared > 4f) continue;
                        // The sexual matcher can be initiated from either member.
                        if (!Visible(a, b.X, b.Y, 2f, true) && !Visible(b, a.X, a.Y, 2f, true)) continue;
                        result.NearbyCompatibleReadyPairs++;
                        withPartner.Add(a.Id); withPartner.Add(b.Id);
                    }
                }
            }
            result.SexualReadyWithNearbyPartner = withPartner.Count;
            return result;
        }

        private static long BreedingGridKey(int x, int y)
        {
            return ((long)x << 32) ^ (uint)y;
        }
    }
}
