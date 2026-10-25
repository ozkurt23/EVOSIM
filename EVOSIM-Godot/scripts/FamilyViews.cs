using EvoSim.Core;
using State = Godot.Collections.Dictionary;
using Rows = Godot.Collections.Array;

public partial class SimulationBridge
{
    public State CreatureFamily(int id) => FamilyOf(world,id);

    // Query only the displayed world: historical panels must never read future births.
    private static State FamilyOf(World source,int id)
    {
        var subject=source.Find(id);
        if(subject==null)return new State();
        static State Describe(Creature c) => new(){{"id",c.Id},{"alive",c.Alive},
            {"predator",c.Genes[2]>=.5f},{"generation",c.Generation}};
        var parents=new Rows();var children=new Rows();
        foreach(int parent in new[]{subject.ParentA,subject.ParentB}.Where(p=>p>0).Distinct())
            if(source.Find(parent) is Creature c)parents.Add(Describe(c));
        foreach(var child in source.Creatures.Where(c=>c.ParentA==id||c.ParentB==id).OrderBy(c=>c.Id))
            children.Add(Describe(child));
        return new State{{"id",id},{"parents",parents},{"children",children}};
    }

    public State ValidateFamilyViews()
    {
        var before=world.StateHash();CreatureFamily(1);
        var checks=new State{{"query_preserves_world",world.StateHash()==before}};
        var config=Configuration();config.Herbivores=4;config.Carnivores=0;
        var fixture=new World(42,config);
        var a=fixture.Creatures[0];var b=fixture.Creatures[1];var child=fixture.Creatures[2];var grandchild=fixture.Creatures[3];
        child.ParentA=a.Id;child.ParentB=b.Id;child.Alive=false;
        grandchild.ParentA=child.Id;grandchild.ParentB=child.Id;
        var family=FamilyOf(fixture,child.Id);
        checks["two_actual_parents"]=family["parents"].AsGodotArray().Count==2;
        checks["dead_child_retained"]=!FamilyOf(fixture,a.Id)["children"].AsGodotArray()[0].AsGodotDictionary()["alive"].AsBool();
        checks["duplicate_parent_not_duplicated"]=FamilyOf(fixture,grandchild.Id)["parents"].AsGodotArray().Count==1;
        checks["founder_has_no_fake_parent"]=FamilyOf(fixture,a.Id)["parents"].AsGodotArray().Count==0;
        checks["only_direct_children"]=FamilyOf(fixture,a.Id)["children"].AsGodotArray().Count==1;
        fixture.Creatures.Remove(grandchild);
        checks["absent_future_child_not_leaked"]=FamilyOf(fixture,child.Id)["children"].AsGodotArray().Count==0;
        checks["missing_creature_empty"]=FamilyOf(fixture,grandchild.Id).Count==0;
        checks["passed"]=checks.Values.All(v=>v.AsBool());return checks;
    }
}
