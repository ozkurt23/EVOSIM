extends Node3D
## Cosmetic rig only: joint motion follows observed travel and Core interaction state.
var head: Node3D
var body: Node3D
var tail: Node3D
var neck: MeshInstance3D
var legs: Array[Node3D]=[]
var surfaces: Array=[]
var lower_legs: Array[Node3D]=[]
var feet: Array[Node3D]=[]
var hips: Array[Vector3]=[]
var foot_anchors: Array[Vector3]=[]
var swing_starts: Array[Vector3]=[]
var was_swing: Array[bool]=[]
var swing_progress: Array[float]=[]
var swing_targets: Array[Vector3]=[]
var scheduled_swing: Array[bool]=[]
var last_tint=Vector3(-1,-1,-1)
var previous_position=Vector3.ZERO
var previous_transform=Transform3D.IDENTITY
var rig_started=false
var stance_error=0.0
var travel_sign=1.0
var ground_height=Callable()
var phase=0.0
var last_distance=-1.0
var gait_weight=0.0
var run_blend=0.0
var cosmetic_time=0.0
var last_tick=-1
var pose_updates=0
var predator=false
var selected=false
var limb_batches=[]
var animation_elapsed=0.0
func build(mesh_provider: Callable,is_predator: bool):
	predator=is_predator
	var prefix="wolf_" if predator else "deer_"
	body=_part(mesh_provider.call(prefix+"body"),Vector3.ZERO)
	head=_part(mesh_provider.call(prefix+"head"),Vector3(.43,.86,0))
	neck=MeshInstance3D.new();var connector=CylinderMesh.new()
	connector.top_radius=.11;connector.bottom_radius=.17;connector.height=1;connector.radial_segments=8
	neck.mesh=connector;var neck_material=surfaces[0][0].duplicate();neck.material_override=neck_material
	surfaces.append([neck_material,neck_material.albedo_color]);add_child(neck);_connect_neck()
	tail=_part(mesh_provider.call(prefix+"tail"),Vector3(-.60,.68,0))
	for p in [Vector3(.38,.63,-.21),Vector3(.38,.63,.21),Vector3(-.43,.63,-.21),Vector3(-.43,.63,.21)]:
		hips.append(p)
		var upper=_limb(.063,.048);add_child(upper);legs.append(upper)
		var lower=_limb(.045,.028);add_child(lower);lower_legs.append(lower)
		var foot=Node3D.new()
		if predator:
			var paw=SphereMesh.new();paw.radius=.075;paw.height=.075;paw.radial_segments=8;paw.rings=4;foot.set_meta("mesh",paw)
		else:
			var hoof=BoxMesh.new();hoof.size=Vector3(.12,.07,.085);foot.set_meta("mesh",hoof)
		var material=surfaces[0][0].duplicate();material.albedo_color=Color("413d34")
		foot.set_meta("material",material);surfaces.append([material,material.albedo_color]);add_child(foot);feet.append(foot)
		foot.position=Vector3(p.x,.035,p.z)
		foot_anchors.append(Vector3.ZERO);swing_starts.append(Vector3.ZERO);was_swing.append(false);swing_progress.append(1.0);swing_targets.append(Vector3.ZERO);scheduled_swing.append(false)
		_solve_leg(legs.size()-1,foot.position)
	# Twelve independently transformed bones share three instanced draw submissions.
	for nodes in [legs,lower_legs,feet]:
		var batch=MultiMesh.new();batch.transform_format=MultiMesh.TRANSFORM_3D;batch.mesh=nodes[0].get_meta("mesh");batch.instance_count=4
		var view=MultiMeshInstance3D.new();view.multimesh=batch;view.material_override=nodes[0].get_meta("material");add_child(view)
		for i in 4:batch.set_instance_transform(i,nodes[i].transform);nodes[i].remove_meta("mesh");nodes[i].remove_meta("material")
		limb_batches.append(batch)

func _part(mesh: Mesh,p: Vector3) -> Node3D:
	var pivot=Node3D.new();pivot.position=p;add_child(pivot)
	var visual=MeshInstance3D.new();visual.mesh=mesh;pivot.add_child(visual)
	for i in mesh.get_surface_count():
		var material=mesh.surface_get_material(i).duplicate()
		surfaces.append([material,material.albedo_color]);visual.set_surface_override_material(i,material)
	return pivot
func pose(data: Dictionary,tick: int,delta: float):
	# The feet follow rendered displacement, not a hypothetical commanded speed.
	# This also preserves planted feet through turns and interpolation between ticks.
	if bool(data.get("paused",false)) and rig_started: return
	cosmetic_time+=delta
	var moved=global_position.distance_to(previous_position) if rig_started else 0.0
	var travel=float(data.distance)
	var synthetic=bool(data.get("synthetic",false))
	if synthetic and last_distance>=0: moved=maxf(0,travel-last_distance)
	if last_tick>tick: rig_started=false
	last_tick=tick;pose_updates+=1;last_distance=travel
	var local_travel=moved/maxf(.01,scale.x)
	if local_travel>1.5: rig_started=false;local_travel=0 # seek/teleport, never stretch a foot across the map
	if moved>.00001 and not synthetic:
		travel_sign=1.0 if (global_position-previous_position).dot(global_basis.x)>=0 else -1.0
	phase+=minf(local_travel/(.82 if predator else .72),delta*4.0)*TAU
	var actual_speed=local_travel/maxf(delta,.001)
	gait_weight=lerpf(gait_weight,clampf(actual_speed/.6,0,1),1-exp(-delta*14))
	var swimming=bool(data.get("swimming",false))
	var running=actual_speed>1.8
	# Blend walk/trot phases so speeds near the boundary do not snap the hind legs.
	run_blend=lerpf(run_blend,1.0 if running else 0.0,1-exp(-delta*5))
	var offsets=[0.0,.5,lerpf(.75,.5,run_blend),lerpf(.25,0.0,run_blend)]
	stance_error=0.0
	var settled=rig_started and moved<.000001 and global_transform.is_equal_approx(previous_transform) and gait_weight<.01 and not swimming and not was_swing.any(func(v):return v)
	for i in (0 if settled else legs.size()):
		var cycle=fposmod(phase/TAU+offsets[i],1.0)
		var swinging=cycle>.62 and gait_weight>.05
		var neutral=Vector3(hips[i].x,.035,hips[i].z)
		if not rig_started:
			foot_anchors[i]=_grounded(to_global(neutral));swing_starts[i]=foot_anchors[i];was_swing[i]=false;swing_progress[i]=1
		if synthetic:
			var target=neutral+Vector3((.5-cycle)*.65*gait_weight,0,0)
			target.y+=sin(clampf((cycle-.62)/.38,0,1)*PI)*.17*gait_weight
			_solve_leg(i,target)
		elif swimming:
			_solve_leg(i,neutral+Vector3(sin(phase+offsets[i]*TAU)*.24,.16+cos(phase+offsets[i]*TAU)*.10,0))
		else:
			var target=to_local(foot_anchors[i])
			var reach=Vector2(target.x-neutral.x,target.z-neutral.z).length()
			var step_due=swinging and not scheduled_swing[i]
			var settle=gait_weight<.08 and reach>.035
			# Turns and stops use a short recovery step, never teleport a planted hoof.
			if not was_swing[i] and (step_due or reach>.27 or target.distance_to(hips[i])>.675 or settle):
				swing_starts[i]=foot_anchors[i];swing_progress[i]=0;was_swing[i]=true
				swing_targets[i]=_grounded(to_global(neutral+Vector3(.20*gait_weight*travel_sign,0,0)))
			if was_swing[i]:
				var duration=clampf(.27/maxf(actual_speed,.8),.075,.22)
				swing_progress[i]=minf(1,swing_progress[i]+delta/duration)
				var u=swing_progress[i]
				foot_anchors[i]=swing_starts[i].lerp(swing_targets[i],smoothstep(0,1,u))+Vector3.UP*sin(u*PI)*(.085 if predator else .12)*scale.x
				if u>=1:was_swing[i]=false;foot_anchors[i]=_grounded(swing_targets[i])
			_solve_leg(i,to_local(foot_anchors[i]))
			if not was_swing[i]:stance_error=maxf(stance_error,to_global(feet[i].position).distance_to(foot_anchors[i]))
		if swimming:was_swing[i]=false
		scheduled_swing[i]=swinging
	body.position.y=sin(phase*2)*.009*gait_weight+sin(cosmetic_time*2.4)*.003*(1-gait_weight)
	body.rotation.x=sin(phase)*.012*gait_weight
	rig_started=true;previous_position=global_position;previous_transform=global_transform
	var feeding=data.action in ["drink","eat","scavenge"]
	var biting=data.action=="bite"
	var bend=-1.1 if feeding else (.12 if data.action=="fruit" else -.06)
	if biting: bend=-.18+sin(cosmetic_time*9)*.16
	head.rotation.z=lerpf(head.rotation.z,bend,1-exp(-delta*12))
	head.position.y=lerpf(head.position.y,.58 if feeding else .86,1-exp(-delta*12))
	head.position.x=.43+(maxf(0,sin(cosmetic_time*9))*.07 if biting else 0.0)
	tail.rotation.y=sin(phase*.5)*.14*gait_weight
	_connect_neck()
	var tint_state=Vector3(float(data.get("boldness",.5)),float(data.aging),float(data.damage))
	if tint_state.is_equal_approx(last_tint):return
	last_tint=tint_state
	for entry in surfaces:
		var base: Color=entry[1]
		var coat=base
		if base.s>.15 and base.v>.12:
			var boldness=float(data.get("boldness",.5))
			var tint=Color("b77050") if boldness>=.5 else Color("718ea0")
			coat=base.lerp(tint,absf(boldness-.5)*.36)
		var age_color=Color.from_hsv(coat.h,coat.s*(1-.7*data.aging),coat.v)
		entry[0].albedo_color=age_color.lerp(Color(.42,.12,.08),clampf(data.damage,0,1)*.16)
func fall(fraction: float):
	rotation.x=1.45
	scale*=clampf(1-fraction*.25,.75,1)
	for entry in surfaces:
		var c: Color=entry[1];entry[0].albedo_color=Color.from_hsv(c.h,c.s*.28,c.v*.65)

func _connect_neck():
	var shoulder=Vector3(.31,.79,0)
	var end=head.position+Vector3(.05,0,0)
	var direction=end-shoulder
	neck.position=(shoulder+end)*.5
	neck.scale=Vector3(1,maxf(.06,direction.length()),1)
	if direction.length()>.001: neck.quaternion=Quaternion(Vector3.UP,direction.normalized())

func set_selected(value: bool):
	if selected==value: return
	selected=value
	# A restrained inspection fill keeps selected fur legible beneath canopy shadows.
	for entry in surfaces:
		entry[0].emission_enabled=value
		entry[0].emission=entry[1]*.12 if value else Color.BLACK

func _limb(top: float,bottom: float) -> Node3D:
	var node=Node3D.new();var mesh=CylinderMesh.new()
	mesh.top_radius=top;mesh.bottom_radius=bottom;mesh.height=1;mesh.radial_segments=7
	node.set_meta("mesh",mesh);var material=surfaces[0][0].duplicate();node.set_meta("material",material)
	surfaces.append([material,material.albedo_color]);return node

func _segment(node: Node3D,a: Vector3,b: Vector3):
	var direction=b-a;node.position=(a+b)*.5
	node.scale=Vector3(1,maxf(.001,direction.length()),1)
	node.quaternion=Quaternion(Vector3.UP,direction.normalized())

func _solve_leg(i: int,target: Vector3):
	var hip=hips[i]
	var direction=target-hip
	var upper_length=.35
	var lower_length=.35
	var distance=clampf(direction.length(),.05,upper_length+lower_length-.001)
	var axis=direction.normalized() if direction.length()>.00001 else Vector3.DOWN
	# Clamp the endpoint too; clamping only the knee calculation stretched the shin.
	target=hip+axis*distance
	var bend=Vector3(-1 if i<2 else 1,0,0)
	bend=bend-axis*bend.dot(axis)
	if bend.length_squared()<.00001:bend=Vector3.FORWARD-axis*Vector3.FORWARD.dot(axis)
	bend=bend.normalized()
	var along=(upper_length*upper_length-lower_length*lower_length+distance*distance)/(2*distance)
	var knee=hip+axis*along+bend*sqrt(maxf(0,upper_length*upper_length-along*along))
	_segment(legs[i],hip,knee);_segment(lower_legs[i],knee,target)
	feet[i].position=target
	if limb_batches.size()==3:
		limb_batches[0].set_instance_transform(i,legs[i].transform)
		limb_batches[1].set_instance_transform(i,lower_legs[i].transform)
		limb_batches[2].set_instance_transform(i,feet[i].transform)

func _grounded(point: Vector3) -> Vector3:
	if ground_height.is_valid(): point.y=ground_height.call(Vector2(point.x,point.z))+.035*scale.y
	return point
