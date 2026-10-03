using System;
namespace EvoSim.Core
{
    // Fixed topology, inherited weights. No training reward, hand-written action selector,
    // predator tag, target navigator or emergency override exists in this controller.
    public static class Brain
    {
        public const int Inputs=30, Hidden=8, Outputs=5;
        public const int Count=Hidden*(Inputs+Hidden+1)+Outputs*(Hidden+1);
        public static float[] Create(PcgRandom rng,float range=.9f)
        {
            var w=new float[Count];
            for(int i=0;i<w.Length;i++)w[i]=(rng.NextFloat()*2-1)*range;
            return w;
        }
        public static void Evaluate(float[] weights,float[] input,float[] memory,float[] output)
        {
            var next=new float[Hidden];int k=0;
            for(int h=0;h<Hidden;h++){
                float sum=weights[k++];
                for(int i=0;i<Inputs;i++)sum+=weights[k++]*input[i];
                for(int i=0;i<Hidden;i++)sum+=weights[k++]*memory[i];
                next[h]=(float)Math.Tanh(sum);
            }
            for(int o=0;o<Outputs;o++){
                float sum=weights[k++];for(int h=0;h<Hidden;h++)sum+=weights[k++]*next[h];
                output[o]=(float)Math.Tanh(sum);
            }
            Array.Copy(next,memory,Hidden);
        }
        public static float[] Inherit(float[] a,float[] b,PcgRandom rng)
        {
            var w=new float[Count];for(int i=0;i<Count;i++)w[i]=Math.Min(a[i],b[i])+Math.Abs(a[i]-b[i])*rng.NextFloat();return w;
        }
    }
}
