extends SceneTree
var checks={}
func _initialize(): call_deferred("run")
func find_mesh(node):
	if node is MeshInstance3D: return node.mesh
	for c in node.get_children():
		var result=find_mesh(c)
		if result: return result
	return null
func mesh_for(name):
	var scene=load("res://assets/"+name+".glb").instantiate();var mesh=find_mesh(scene).duplicate();scene.free();return mesh
func run():
	var view=load("res://scripts/creature_view.gd").new();root.add_child(view);view.build(mesh_for,false)
	view.ground_height=func(_point): return 0.
	var data={"distance":0.,"radius":.35,"speed":.6,"action":"move","aging":0.,"damage":0.}
	var planted_checks=0;var max_slip=0.;var max_ground_error=0.
	for frame in 180:
		var before=[];for foot in view.feet: before.append(foot.global_position)
		var previous=view.was_swing.duplicate()
		view.position.x+=.008;data.distance+=.008
		view.pose(data,frame,1./60)
		for i in 4:
			if frame>2 and not previous[i] and not view.was_swing[i] and before[i].distance_to(view.feet[i].global_position)<.15:
				planted_checks+=1;max_slip=maxf(max_slip,before[i].distance_to(view.feet[i].global_position))
				max_ground_error=maxf(max_ground_error,absf(view.feet[i].global_position.y-.035))
	checks.stance_samples=planted_checks>100
	checks.planted_feet=max_slip<.0001
	checks.ground_contact=max_ground_error<.0001
	for frame in range(180,230):
		view.position.x-=.008;data.distance+=.008;view.pose(data,frame,1./60)
	checks.backward_gait=view.travel_sign<0
	view.rotation.y+=.6;view.pose(data,230,1./60)
	checks.turn_finite=view.feet.all(func(f): return f.position.is_finite())
	var pose=view.feet.map(func(f): return f.transform)
	data.paused=true;view.pose(data,230,1.);data.paused=false
	checks.paused_exact=range(4).all(func(i): return view.feet[i].transform==pose[i])
	data.swimming=true;view.pose(data,231,1./60)
	checks.swimming_raises_feet=view.feet.all(func(f): return f.position.y>.08)
	data.swimming=false;view.rig_started=false;view.ground_height=func(point): return point.x*.08
	var slope_error=0.
	for frame in range(232,332):
		view.position.x+=.008;view.position.y=view.position.x*.08;view.pose(data,frame,1./60)
		for i in 4:
			if not view.was_swing[i]: slope_error=maxf(slope_error,absf(view.feet[i].global_position.y-view.feet[i].global_position.x*.08-.035))
	checks.slope_contact=slope_error<.0001
	# Stop mid-stride; raised feet must settle without continued forward movement.
	data.speed=0;data.distance=view.last_distance
	for frame in range(332,392):view.pose(data,frame,1./60)
	checks.stop_settles=view.was_swing.all(func(v):return not v)
	view._solve_leg(0,Vector3(100,-20,30))
	checks.unreachable_upper_fixed=absf(view.legs[0].scale.y-.35)<.00001
	checks.unreachable_lower_fixed=absf(view.lower_legs[0].scale.y-.35)<.00001
	checks.reachable_endpoint=view.feet[0].position.distance_to(view.hips[0])<.7
	var ok=checks.values().all(func(v): return v)
	FileAccess.open("res://validation/locomotion-checks-46.json",FileAccess.WRITE).store_string(JSON.stringify({"passed":ok,"checks":checks,"planted_samples":planted_checks,"max_slip":max_slip,"ground_error":max_ground_error,"slope_error":slope_error},"  "))
	print("LOCOMOTION ",checks);quit(0 if ok else 1)
