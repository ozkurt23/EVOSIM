using System;

namespace EvoSim.Core
{
    /// <summary>PCG-XSH-RR. Streams are explicit, so exploration never consumes mutation draws.</summary>
    public sealed class PcgRandom
    {
        public ulong State { get; private set; }
        public ulong Increment { get; private set; }
        public PcgRandom(ulong seed, ulong stream = 1)
        {
            State = 0;
            Increment = (stream << 1) | 1UL;
            NextUInt();
            State = unchecked(State + seed);
            NextUInt();
        }
        internal void Restore(ulong state,ulong increment) { if((increment&1)==0)throw new ArgumentException("Invalid RNG stream"); State=state;Increment=increment; }
        public uint NextUInt()
        {
            ulong old = State;
            State = unchecked(old * 6364136223846793005UL + Increment);
            uint shifted = (uint)(((old >> 18) ^ old) >> 27);
            int rotation = (int)(old >> 59);
            return (shifted >> rotation) | (shifted << ((-rotation) & 31));
        }
        public float NextFloat() { return (NextUInt() >> 8) * (1f / 16777216f); }
        public double NextDouble() { return NextUInt() * (1.0 / 4294967296.0); }
        public int NextInt(int exclusiveMaximum)
        {
            if (exclusiveMaximum <= 0) throw new ArgumentOutOfRangeException(nameof(exclusiveMaximum));
            uint bound = (uint)exclusiveMaximum;
            uint threshold = unchecked(0u - bound) % bound;
            uint r;
            do { r = NextUInt(); } while (r < threshold);
            return (int)(r % bound);
        }
        public float Gaussian()
        {
            double u = Math.Max(1e-12, NextDouble());
            return (float)(Math.Sqrt(-2 * Math.Log(u)) * Math.Cos(2 * Math.PI * NextDouble()));
        }
    }

    public static class Genetics
    {
        public const int Count = 6;
        public const int Body = 0, Perception = 1, Diet = 2, Aquatic = 3, Boldness = 4, Bond = 5;
        public static readonly string[] Names = { "Beden", "Algı", "Beslenme", "Su uyumu", "Atılganlık", "Sosyal bağ" };
        public static float Reflect01(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            float wrapped = value % 2f;
            if (wrapped < 0) wrapped += 2f;
            return wrapped <= 1f ? wrapped : 2f - wrapped;
        }
        public static float[] Inherit(float[] a, float[] b, PcgRandom inheritanceRng,
            PcgRandom mutationRng, out bool mutated, out int locus, float probability = 0.001f)
        {
            if (a == null || b == null || a.Length != Count || b.Length != Count)
                throw new ArgumentException("Each parent must have six genes.");
            if (probability < 0 || probability > 1) throw new ArgumentOutOfRangeException(nameof(probability));
            var genes = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                if (a[i] < 0 || a[i] > 1 || b[i] < 0 || b[i] > 1 || float.IsNaN(a[i]) || float.IsNaN(b[i]))
                    throw new ArgumentException("Parental genes must be finite normalized values.");
                genes[i] = Math.Min(a[i], b[i]) + Math.Abs(a[i] - b[i]) * inheritanceRng.NextFloat();
            }
            mutated = mutationRng.NextFloat() < probability;
            locus = -1;
            if (mutated)
            {
                locus = mutationRng.NextInt(Count);
                float sigma = mutationRng.NextFloat() < 0.05f ? 0.10f : 0.025f;
                genes[locus] = Reflect01(genes[locus] + sigma * mutationRng.Gaussian());
            }
            return genes;
        }
    }
}
