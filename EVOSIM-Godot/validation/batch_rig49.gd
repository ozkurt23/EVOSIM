extends SceneTree
func _initialize():call_deferred("run")
func find_mesh(node):
	if node is MeshInstance3D:return node.mesh
	for child in node.get_children():
		var found=find_mesh(child)
		if found:return found
	return null
func mesh_for(name):
	var scene=load("res://assets/"+name+".glb").instantiate();var mesh=find_mesh(scene).duplicate();scene.free();return mesh
func run():
	var checks={}
	for predator in [false,true]:
		var v=load("res://scripts/creature_view.gd").new();root.add_child(v);v.build(mesh_for,predator)
		v.pose({"distance":0.,"action":"move","aging":0.,"damage":0.},1,1./60)
		var key="predator" if predator else "herbivore"
		checks[key+"_three_batches"]=v.limb_batches.size()==3
		checks[key+"_no_individual_geometry"]=v.legs.all(func(n):return not n is GeometryInstance3D) and v.feet.all(func(n):return not n is GeometryInstance3D)
		var equal=true
		for i in 4:
			equal=equal and v.limb_batches[0].get_instance_transform(i).is_equal_approx(v.legs[i].transform)
			equal=equal and v.limb_batches[1].get_instance_transform(i).is_equal_approx(v.lower_legs[i].transform)
			equal=equal and v.limb_batches[2].get_instance_transform(i).is_equal_approx(v.feet[i].transform)
		checks[key+"_poses_exact"]=equal;v.free()
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/batch-rig49.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "));print("BATCH_RIG49 ",checks);quit(0 if checks.passed else 1)
