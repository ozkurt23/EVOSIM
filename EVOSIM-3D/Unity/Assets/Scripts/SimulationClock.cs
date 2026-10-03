using System;
using System.Diagnostics;
using EvoSim.Core;

// Retains unfinished steps when the frame budget is exhausted; never skips Core ticks.
public sealed class SimulationClock
{
    public double PendingSeconds{get;private set;}
    public int Advance(World world,double seconds,int speed,bool paused,int maxSteps=160,double budgetMs=18){
        if(paused)return 0;
        PendingSeconds+=Math.Min(Math.Max(seconds,0),.25)*Math.Clamp(speed,1,20);
        var timer=Stopwatch.StartNew();int count=0;
        while(PendingSeconds+1e-10>=World.Dt&&count<maxSteps&&timer.Elapsed.TotalMilliseconds<budgetMs){world.Step();PendingSeconds-=World.Dt;count++;}
        return count;
    }
}
