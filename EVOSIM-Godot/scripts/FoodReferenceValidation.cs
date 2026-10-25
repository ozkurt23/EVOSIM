using EvoSim.Core;
using State = Godot.Collections.Dictionary;

public partial class SimulationBridge
{
    public State ValidateFoodReference()
    {
        var config=Configuration();config.Herbivores=1;config.Carnivores=0;
        var probe=new World(42,config);var cache=new GroundFoodReference();
        string before=probe.StateHash();var initial=cache.For(probe);
        bool readOnly=before==probe.StateHash();
        int cell=Array.FindIndex(initial,x=>x>.1f);
        float original=initial[cell];probe.Map.Food[cell]=0;
        bool depleted=cache.For(probe)[cell]==original;
        for(int tick=0;tick<20;tick++)probe.Step();
        bool regrown=probe.Map.Food[cell]>0 && cache.For(probe)[cell]==original;
        using var stream=new MemoryStream();probe.Save(stream);stream.Position=0;
        var restored=World.Load(stream);bool restoredReference=cache.For(restored).SequenceEqual(initial);
        var other=config.Copy();other.MapSize=256;var large=new World(42,other);
        bool resized=cache.For(large).Length==256*256 && cache.For(probe).SequenceEqual(initial);
        return new State{{"passed",readOnly&&depleted&&regrown&&restoredReference&&resized},
            {"read_only",readOnly},{"depletion_preserves_reference",depleted},{"regrowth_preserves_reference",regrown},
            {"load_preserves_reference",restoredReference},{"map_change_invalidates_cache",resized}};
    }
}
