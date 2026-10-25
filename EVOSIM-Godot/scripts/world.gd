extends Node3D

const DecorationSampler = preload("res://scripts/decoration_sampler.gd")
const Bridge = preload("res://scripts/SimulationBridge.cs")
const EXTENT = 48.0
var ORIGIN = Vector2(42, 20)
var bridge
var map: Dictionary
var surface
var profile_stages=false
var stage_costs={}
var rng = RandomNumberGenerator.new()
var terrain_noise = FastNoiseLite.new()
var camera: Camera3D
var focus = Vector3(24, 0, 24)
var zoom = 30.0
var yaw = 0.66
var agents = {}
var tree_nodes: Array = []
var forest_instances={}
var forest_nodes=[]
var fruit_visuals=[]
var forest_signature=""
var canopy_reveal_ids=[]
var fruit_nodes: Array = []
var stats_label: Label
var quality_label: Button
var paused_button: Button
var elapsed = 0.0
var frame_times: Array = []
var selected_id = -1
var selected_tree_id=-1
var selection_ring: MeshInstance3D
var ui: CanvasLayer
var agent_data = {}
var capture_path = ""
var capture_at = 7.0
var captured = false
var high_quality = true
var edible_patches = {}
var mesh_cache = {}
var last_tick = -1
var materials = {}
var performance_label: Label
var gpu_times: Array=[]
var render_cpu_times: Array=[]
var script_times: Array=[]
var smoke = false
var inspect_requested = false
var follow_capture = false
var control_results = {}
var clock_validation = {}
var weather
var corpse_views={}
var library
var observations
var scenery=[]
var minimap
var autosave_elapsed=0.0
var scenario="normal"
var start_balanced=false
var sun_light: DirectionalLight3D
var environment: Environment

func _ready():
	get_tree().auto_accept_quit=false
	RenderingServer.global_shader_parameter_set("evosim_region_offset",Vector2.ZERO)
	RenderingServer.viewport_set_measure_render_time(get_viewport().get_viewport_rid(),true)
	rng.seed = 2407
	terrain_noise.seed = 42
	terrain_noise.frequency = 0.09
	bridge = Bridge.new()
	add_child(bridge)
	bridge.Initialize(42)
	if not bridge.ModelReady:
		push_error("Simülasyon modeli başlatılamadı; kayıtlar açılmadı.");get_tree().quit(1);return
	map = bridge.GetMap()
	surface=load("res://scripts/world_surface.gd").new(map,ORIGIN)
	for arg in OS.get_cmdline_user_args():
		if arg=="--smoke": smoke=true
		if arg=="--balanced": start_balanced=true
		if arg.begins_with("--scenario="): scenario=arg.trim_prefix("--scenario=")
		if arg=="--inspect": inspect_requested=true
		if arg=="--detail": inspect_requested=true;follow_capture=true;zoom=24
		if arg.begins_with("--zoom="): zoom=float(arg.trim_prefix("--zoom="))
		if arg.begins_with("--speed="): bridge.Speed=int(arg.trim_prefix("--speed="))
		if arg.begins_with("--capture="): capture_path = arg.trim_prefix("--capture=")
		if arg.begins_with("--capture-at="): capture_at = float(arg.trim_prefix("--capture-at="))
	if not bridge.ConfigureScenario(scenario):
		get_tree().quit(1);return
	map=bridge.GetMap()
	surface=load("res://scripts/world_surface.gd").new(map,ORIGIN)
	_setup_lighting()
	var before_scenery=get_children()
	_build_terrain()
	_build_trees()
	_build_rocks()
	_build_ground_cover()
	_build_edible_patches()
	_build_fruit()
	_build_details()
	_build_ferns()
	scenery=get_children().filter(func(node): return not node in before_scenery)
	_setup_camera()
	_setup_ui()
	weather=load("res://scripts/weather_view.gd").new();add_child(weather)
	weather.build(sun_light,environment)
	if start_balanced: _toggle_quality()
	_update_agents(1.0)
	if smoke: _control_smoke()
	if inspect_requested and agents.size():
		var closest=INF
		for id in agents:
			var distance=agents[id].position.distance_to(focus)
			if distance<closest: selected_id=id;closest=distance
	if capture_path!="": set_process_unhandled_input(false)
	elif not smoke: library.show_library()
	print("EVOSIM_READY renderer=", RenderingServer.get_current_rendering_method(), " trees=", tree_nodes.size())

func _setup_lighting():
	var env = Environment.new()
	environment=env
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("708b84")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("c2d7d4")
	env.ambient_light_energy = 0.45
	env.tonemap_mode = Environment.TONE_MAPPER_ACES
	env.ssao_enabled = true
	env.ssao_radius = 1.1
	env.ssao_intensity = 1.05
	env.ssao_light_affect = 0.25
	env.fog_enabled = true
	env.fog_sky_affect = 0.0
	env.fog_light_color = Color("91b8af")
	env.fog_density = 0.0007
	var world_env = WorldEnvironment.new()
	world_env.environment = env
	add_child(world_env)
	var sun = DirectionalLight3D.new()
	sun_light=sun
	sun.rotation_degrees = Vector3(-52, -32, 0)
	sun.light_color = Color("fff0d6")
	sun.light_energy = 1.35
	sun.light_angular_distance = 2.2
	sun.shadow_enabled = true
	sun.directional_shadow_max_distance = 150
	sun.directional_shadow_mode = DirectionalLight3D.SHADOW_PARALLEL_4_SPLITS
	sun.shadow_bias = 0.015
	add_child(sun)







func _mat(key: String, color: Color) -> StandardMaterial3D:
	if materials.has(key): return materials[key]
	var m=StandardMaterial3D.new()
	m.albedo_color=color
	m.roughness=.9
	materials[key]=m
	return m

func _find_mesh(node: Node) -> MeshInstance3D:
	if node is MeshInstance3D: return node
	for child in node.get_children():
		var found=_find_mesh(child)
		if found: return found
	return null

func _mesh(kind: String) -> Mesh:
	if mesh_cache.has(kind): return mesh_cache[kind]
	var packed: PackedScene=load("res://assets/"+kind+".glb")
	var scene=packed.instantiate()
	var source=_find_mesh(scene)
	var mesh=source.mesh.duplicate()
	for i in mesh.get_surface_count():
		var original=mesh.surface_get_material(i)
		var name=original.resource_name.to_lower()
		if "stone" in name:
			var rock_material=ShaderMaterial.new()
			rock_material.shader=load("res://shaders/rock.gdshader")
			mesh.surface_set_material(i,rock_material)
		if "leaf" in name or "pine" in name:
			var m=ShaderMaterial.new()
			m.shader=load("res://shaders/foliage.gdshader")
			m.set_shader_parameter("base_color",original.albedo_color)
			mesh.surface_set_material(i,m)
	mesh_cache[kind]=mesh
	scene.free()
	return mesh

func _place(kind: String,p: Vector2,s: float,rot: float) -> MeshInstance3D:
	var node=MeshInstance3D.new()
	node.mesh=_mesh(kind)
	node.position=Vector3(p.x,_height(p),p.y)
	node.scale=Vector3.ONE*s
	node.rotation.y=rot
	add_child(node)
	return node

func _inside(p: Vector2,margin=1.0) -> bool:
	return p.x>margin and p.y>margin and p.x<EXTENT-margin and p.y<EXTENT-margin

func _build_trees():
	var before=get_children()
	forest_instances.clear();tree_nodes.clear()
	forest_signature=_tree_signature(map.trees)
	var tree_groups={}
	for t in map.trees:
		var p=Vector2(t.x,t.z)-ORIGIN
		if not _inside(p,1.5): continue
		rng.seed=int(t.x*9371+t.z*6733)
		var kind="oak" if rng.randf()<.55 else ("alder" if rng.randf()<.7 else "pine")
		if kind!="pine" and int(t.x*13+t.z*7)%2==0: kind+="_b"
		var scale=t.radius*.58
		var key=kind+"_"+str(int(p.x/16))+"_"+str(int(p.y/16))
		if not tree_groups.has(key): tree_groups[key]={"kind":kind,"poses":[],"states":[]}
		var basis=Basis(Vector3.UP,rng.randf()*TAU).scaled(Vector3(scale,scale*rng.randf_range(.9,1.15),scale))
		tree_groups[key].states.append(t)
		tree_groups[key].poses.append(Transform3D(basis,Vector3(p.x,_height(p),p.y)))
		tree_nodes.append(p)
		if rng.randf()<.6:
			var q=p+Vector2(rng.randf_range(-1.4,1.4),rng.randf_range(-1.4,1.4))
			if _field(q)>.6: _place("shrub",q,rng.randf_range(.55,.9),rng.randf()*TAU)
	for key in tree_groups:
		var group=tree_groups[key]
		var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D
		mm.use_custom_data=true
		mm.mesh=_mesh(group.kind);mm.instance_count=group.poses.size()
		for i in group.poses.size():
			mm.set_instance_transform(i,group.poses[i])
			var origin: Vector3=group.poses[i].origin
			var patch=(sin((origin.x+ORIGIN.x-42)*.22+1.7)*cos((origin.z+ORIGIN.y-20)*.17)+1.0)*.5
			var warm=smoothstep(.40,.85,patch)*.85 if group.kind!="pine" else 0.0
			var state=group.states[i]
			forest_instances[int(state.id)]={"mm":mm,"index":i,"base":group.poses[i],"warm":warm}
			_update_tree_instance(state)
		var grove=MultiMeshInstance3D.new();grove.multimesh=mm;grove.name="Grove_"+key
		grove.visibility_range_end=150
		add_child(grove)
	forest_nodes=get_children().filter(func(node):return not node in before)

func _tree_signature(states) -> String:
	var ids=PackedStringArray()
	for t in states:
		if _inside(Vector2(t.x,t.z)-ORIGIN,1.5):ids.append(str(t.id))
	return ",".join(ids)

func _update_tree_instance(state):
	if not forest_instances.has(int(state.id)):return
	var item=forest_instances[int(state.id)]
	var growth=.22+.78*float(state.get("growth",1.0))
	var transform: Transform3D=item.base
	transform.basis=transform.basis.scaled(Vector3.ONE*growth)
	item.mm.set_instance_transform(item.index,transform)
	item.mm.set_instance_custom_data(item.index,Color(0,item.warm,1.0-float(state.get("health",1.0)),1.0 if state.get("dead",false) else 0.0))

func _refresh_forest():
	if not bridge.LivingForest:return
	var states=bridge.GetTreeStates()
	map.trees=states
	if _tree_signature(states)!=forest_signature:
		for node in forest_nodes+fruit_visuals:
			if is_instance_valid(node):node.free()
		fruit_nodes.clear()
		_build_trees();_build_fruit()
		scenery=scenery.filter(func(node):return is_instance_valid(node))
		scenery.append_array(forest_nodes);scenery.append_array(fruit_visuals)
		if minimap:minimap.rebuild()
	else:
		for state in states:_update_tree_instance(state)

func _build_rocks():
	# Existing occupied rock cells; no collision is added by decoration.
	for z in range(2,46):
		for x in range(2,46):
			var p=Vector2(x+.5,z+.5)
			if _tile(int(x+ORIGIN.x),int(z+ORIGIN.y))==2:
				var tree_cell=false
				for t in map.trees:
					if int(t.x)==int(x+ORIGIN.x) and int(t.z)==int(z+ORIGIN.y): tree_cell=true;break
				var world_x=x+int(ORIGIN.x);var world_z=z+int(ORIGIN.y)
				rng.seed=(world_x*73856093)^(world_z*19349663)^843
				if not tree_cell and (world_x+world_z)%3==0: _place("rock_b" if (world_x+world_z)%2==0 else "rock",p,rng.randf_range(.85,1.55),rng.randf()*TAU)

func _grass_mesh() -> ArrayMesh:
	var st=SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for j in 7:
		var angle=j*2.399
		var base=Vector3(cos(angle)*.065,0,sin(angle)*.065)
		var side=Vector3(-sin(angle),0,cos(angle))*.019
		var bend=Vector3(cos(angle),0,sin(angle))
		var h=.19+j*.026
		var mid=base+Vector3.UP*h*.62+bend*.045
		var tip=base+Vector3.UP*h+bend*.13
		for p in [base-side,mid-side*.65,base+side,base+side,mid-side*.65,mid+side*.65,mid-side*.65,tip,mid+side*.65]:
			st.set_normal(Vector3.UP)
			st.add_vertex(p)
	var mesh=st.commit()
	var m=ShaderMaterial.new()
	m.shader=load("res://shaders/foliage.gdshader")
	m.set_shader_parameter("base_color",Color("708a42"))
	m.set_shader_parameter("strength",.07)
	mesh.surface_set_material(0,m)
	return mesh

func _build_ground_cover():
	var groups={}
	for sample in DecorationSampler.sample(ORIGIN,EXTENT,17.0,1):
		var global_point=Vector2(sample.x,sample.y)
		var p=global_point-ORIGIN
		if _field(p)<.6: continue
		var density=terrain_noise.get_noise_2d((global_point.x-42)*2.3,(global_point.y-20)*2.3)
		if density<-.05 or sample.w>.55: continue
		var chunk=Vector2i(int(p.x/12),int(p.y/12))
		if not groups.has(chunk): groups[chunk]=[]
		groups[chunk].append(Transform3D(Basis(Vector3.UP,sample.z).scaled(Vector3.ONE*(.7+sample.w/.55*.7)),Vector3(p.x,_height(p),p.y)))
	var mesh=_grass_mesh()
	for key in groups:
		var mm=MultiMesh.new()
		mm.transform_format=MultiMesh.TRANSFORM_3D
		mm.mesh=mesh
		mm.instance_count=groups[key].size()
		for i in mm.instance_count: mm.set_instance_transform(i,groups[key][i])
		var node=MultiMeshInstance3D.new()
		node.multimesh=mm
		node.name="MeadowPatch"
		node.add_to_group("ground_cover")
		node.visibility_range_end=125
		node.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		add_child(node)
	# Original shoreline boulders/reed-like low cover, sparse and ground-anchored.
	for j in 800:
		var p=Vector2(rng.randf_range(1,47),rng.randf_range(1,47))
		if _field(p)>.3 and _field(p)<.85 and terrain_noise.get_noise_2d(p.x*.55,p.y*.55)>.05:
			_place("rock",p,rng.randf_range(.18,.45),rng.randf()*TAU)

func _build_fruit():
	var sphere=SphereMesh.new();sphere.radius=.13;sphere.height=.26;sphere.radial_segments=8;sphere.rings=4
	sphere.material=_mat("fruit",Color("cb5131"))
	var fruits=[]
	for f in bridge.GetFruitRegion(ORIGIN.x,ORIGIN.y,ORIGIN.x+EXTENT,ORIGIN.y+EXTENT):
		var p=Vector2(f.x,f.z)-ORIGIN
		if _inside(p,1.5) and _inside(Vector2(f.tree_x,f.tree_z)-ORIGIN,1.5):fruits.append(f)
	var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D;mm.mesh=sphere;mm.instance_count=fruits.size()
	for i in fruits.size():
		var f=fruits[i];var p=Vector2(f.x,f.z)-ORIGIN
		var position=Vector3(p.x,_height(Vector2(f.tree_x,f.tree_z)-ORIGIN)+f.radius*(1.02+.1*(int(f.slot)%3)),p.y)
		var transform=Transform3D(Basis.IDENTITY,position)
		mm.set_instance_transform(i,transform if f.available else Transform3D(Basis.IDENTITY.scaled(Vector3.ZERO),position))
		fruit_nodes.append([mm,f.tree_x,f.tree_z,f.slot,i,transform,bool(f.available)])
	var node=MultiMeshInstance3D.new();node.multimesh=mm;add_child(node);fruit_visuals=[node]

func _update_fruit_stock(available):
	for f in fruit_nodes:
		var present=bool(available.get(Vector3(f[1],f[2],f[3]),false))
		if present==f[6]:continue
		f[6]=present
		f[0].set_instance_transform(f[4],f[5] if present else Transform3D(Basis.IDENTITY.scaled(Vector3.ZERO),f[5].origin))

func _setup_camera():
	camera=Camera3D.new()
	camera.projection=Camera3D.PROJECTION_ORTHOGONAL
	camera.size=zoom
	camera.far=240
	add_child(camera)
	_update_camera(1.0)

func _update_camera(delta: float):
	camera.size=lerpf(camera.size,zoom,1-exp(-delta*9))
	camera.position=focus+Vector3(sin(yaw)*65,59,cos(yaw)*65)
	camera.look_at(focus,Vector3.UP)

func _setup_ui():
	ui=load("res://scripts/observer_ui.gd").new()
	add_child(ui)
	ui.pause_requested.connect(func(): bridge.Paused=not bridge.Paused)
	ui.speed_requested.connect(func(): bridge.Speed=1 if bridge.Speed==20 else mini(20,bridge.Speed*2))
	ui.quality_requested.connect(_toggle_quality)
	ui.selection_closed.connect(func(): selected_id=-1)
	selection_ring=MeshInstance3D.new()
	var ring=TorusMesh.new();ring.inner_radius=.69;ring.outer_radius=.73;ring.rings=40;ring.ring_segments=6
	selection_ring.mesh=ring
	var mat=StandardMaterial3D.new();mat.shading_mode=BaseMaterial3D.SHADING_MODE_UNSHADED
	mat.albedo_color=Color("e5bb7b")
	selection_ring.material_override=mat;selection_ring.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
	selection_ring.visible=false;add_child(selection_ring)
	library=load("res://scripts/library_ui.gd").new();add_child(library);library.build(bridge,ui)
	library.world_changed.connect(_rebuild_world)
	observations=load("res://scripts/observation_ui.gd").new();add_child(observations);observations.build(bridge,ui)
	minimap=load("res://scripts/world_minimap.gd").new();ui.root.add_child(minimap);minimap.build(bridge,self,ui)

func _toggle_quality():
	high_quality=not high_quality
	bridge.FrameBudgetMs=7 if high_quality else 4
	ui.quality_button.tooltip_text="Ayrıntılı görünüm" if high_quality else "Akıcılık öncelikli · yoğunlukta gerçek simülasyon hızı hedefin altında kalabilir"
	for child in get_children():
		if child is WorldEnvironment: child.environment.ssao_enabled=high_quality
		if child is MultiMeshInstance3D and child.is_in_group("ground_cover"): child.multimesh.visible_instance_count=-1 if high_quality else child.multimesh.instance_count/2


func _unhandled_input(event):
	if library and library.overlay.visible: return
	if event is InputEventMouseButton and event.pressed:
		if event.button_index==MOUSE_BUTTON_LEFT:
			_pick_agent(event.position)
			if selected_id<0:_pick_tree(event.position)
			if selected_id<0 and selected_tree_id<0:
				var hit=Plane(Vector3.UP,0).intersects_ray(camera.project_ray_origin(event.position),camera.project_ray_normal(event.position))
				if hit!=null and _inside(Vector2(hit.x,hit.z),0):observations.inspect_region(Vector2(hit.x,hit.z)+ORIGIN)
			if event.double_click: follow_capture=selected_id>=0
		if event.button_index==MOUSE_BUTTON_WHEEL_UP: zoom=clampf(zoom*.9,16,80)
		if event.button_index==MOUSE_BUTTON_WHEEL_DOWN: zoom=clampf(zoom/ .9,16,80)
	if event is InputEventMouseMotion and (Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT) or Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE)):
		follow_capture=false
		var right=camera.global_basis.x
		var forward=Vector3.UP.cross(right)
		focus-= (right*event.relative.x-forward*event.relative.y)*camera.size/get_viewport().get_visible_rect().size.y
		var global_focus=Vector2(focus.x,focus.z)+ORIGIN
		global_focus=global_focus.clamp(Vector2.ONE*5,Vector2.ONE*(float(map["size"])-5))
		focus.x=global_focus.x-ORIGIN.x;focus.z=global_focus.y-ORIGIN.y
		if focus.x<10 or focus.x>38 or focus.z<10 or focus.z>38: navigate_world(global_focus)
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode==KEY_SPACE: bridge.Paused=not bridge.Paused
		if event.keycode==KEY_Q: yaw-=.12
		if event.keycode==KEY_E: yaw+=.12
		if event.keycode==KEY_ESCAPE:
			library.show_library()

func _update_agents(delta):
	var live={}
	for a in bridge.GetAgentsRegion(ORIGIN.x,ORIGIN.y,ORIGIN.x+EXTENT,ORIGIN.y+EXTENT):
		var p=Vector2(a.x,a.z)-ORIGIN
		if not _inside(p,.4): continue
		live[a.id]=true
		agent_data[a.id]=a
		if not agents.has(a.id):
			var view=load("res://scripts/creature_view.gd").new();view.build(_mesh,a.predator);view.ground_height=_height
			add_child(view);view.position=Vector3(p.x,_height(p),p.y);agents[a.id]=view
		var node=agents[a.id]
		node.scale=Vector3.ONE*a.radius*1.8
		node.set_selected(a.id==selected_id)
		var target=Vector3(p.x,_height(p),p.y)
		if not bridge.Paused:
			node.position=node.position.lerp(target,1-exp(-delta*15))
			node.rotation.y=lerp_angle(node.rotation.y,-a.angle,1-exp(-delta*12))
		if a.swimming: node.position.y=-a.radius*1.15
		a["paused"]=bridge.Paused
		# Keep biological state and interpolated positions current; only skip invisible rig work.
		var screen_point=camera.unproject_position(node.global_position+Vector3.UP*.5)
		var on_screen=not camera.is_position_behind(node.global_position) and get_viewport().get_visible_rect().grow(100).has_point(screen_point)
		if on_screen or a.id==selected_id:
			node.visible=true;node.animation_elapsed+=delta
			# At overview scale legs are only a few pixels; retain full rate for close inspection.
			if camera.size<22 or a.id==selected_id or node.animation_elapsed>=1.0/30.0:
				node.pose(a,bridge.CurrentTick,node.animation_elapsed);node.animation_elapsed=0.0
		else:
			node.visible=false;node.rig_started=false;node.animation_elapsed=0.0

	for id in agents.keys():
		if not live.has(id):
			agents[id].queue_free();agents.erase(id);agent_data.erase(id)
			if selected_id==id:
				selected_id=-1
				if ui: ui.card.hide()

func _process(delta):
	if not bridge or not camera: return
	var process_start=Time.get_ticks_usec()
	elapsed+=delta
	bridge.Advance(delta)
	var advance_end=Time.get_ticks_usec() if profile_stages else 0
	autosave_elapsed+=delta
	if autosave_elapsed>120 and library.entered_world and capture_path=="":
		autosave_elapsed=0
		if not bridge.SaveCurrent(bridge.WorldName): library.show_library()
	_update_agents(delta)
	var agents_end=Time.get_ticks_usec() if profile_stages else 0
	if follow_capture and agents.has(selected_id):
		focus=agents[selected_id].position
		if capture_path=="" and (focus.x<12 or focus.x>36 or focus.z<12 or focus.z>36): navigate_world(Vector2(focus.x,focus.z)+ORIGIN)
	_update_camera(delta)
	_update_canopies(delta)
	var stats=bridge.Stats()
	ui.update_world(stats,bridge.Paused,bridge.Speed,high_quality)
	weather.update_weather(stats,delta,high_quality)
	selection_ring.visible=selected_id>=0 and agents.has(selected_id)
	if selection_ring.visible:
		if capture_path!="": ui.show_creature(agent_data[selected_id])
		selection_ring.position=agents[selected_id].position+Vector3.UP*.035
	elif selected_tree_id>=0:
		var matches=map.trees.filter(func(t):return int(t.id)==selected_tree_id)
		if not matches.is_empty():
			var tree=matches[0];var p=Vector2(tree.x,tree.z)-ORIGIN
			selection_ring.visible=_inside(p,1.5)
			selection_ring.position=Vector3(p.x,_height(p)+.04,p.y)
	if capture_path!="" and elapsed>2 and not captured:
		frame_times.append(delta*1000)
		gpu_times.append(RenderingServer.viewport_get_measured_render_time_gpu(get_viewport().get_viewport_rid()))
		render_cpu_times.append(RenderingServer.viewport_get_measured_render_time_cpu(get_viewport().get_viewport_rid())+RenderingServer.get_frame_setup_time_cpu())
		script_times.append((Time.get_ticks_usec()-process_start)/1000.0)
	if int(int(stats.tick)/20) != last_tick:
		last_tick=int(int(stats.tick)/20)
		_refresh_forest()
		_update_edible_patches()
		_update_corpses()
		var fruits=bridge.GetFruitRegion(ORIGIN.x,ORIGIN.y,ORIGIN.x+EXTENT,ORIGIN.y+EXTENT)
		var available={}
		for f in fruits: available[Vector3(f.tree_x,f.tree_z,f.slot)]=f.available
		_update_fruit_stock(available)
	if profile_stages:stage_costs={"advance":(advance_end-process_start)/1000.0,"agents":(agents_end-advance_end)/1000.0,"rest":(Time.get_ticks_usec()-agents_end)/1000.0}
	if capture_path!="" and elapsed>=capture_at and not captured:
		captured=true
		_capture()

func _capture():
	bridge.Paused=true
	await RenderingServer.frame_post_draw
	var image=get_viewport().get_texture().get_image()
	image.save_png(capture_path)
	var report=bridge.Validate()
	var mismatches=0
	for z in 48:
		for x in 48:
			if (_field(Vector2(x+.5,z+.5))<0)!=(_tile(x+int(ORIGIN.x),z+int(ORIGIN.y))==1): mismatches+=1
	report["terrain_cell_classification_errors"]=mismatches
	report["passed"]=report.passed and mismatches==0
	if scenario=="cycle": report["passed"]=report.passed and bridge.Stats().climate_commands==2 and not bridge.Stats().drought
	if scenario=="empty": report["passed"]=report.passed and bridge.Stats().herbivores+bridge.Stats().carnivores==0
	if scenario=="drought": report["passed"]=report.passed and bridge.Stats().drought and bridge.Stats().water_level<.2
	if scenario=="rain": report["passed"]=report.passed and bridge.Stats().rain and not bridge.Stats().drought
	for passed in control_results.values(): report["passed"]=report.passed and passed
	frame_times.sort()
	gpu_times.sort();render_cpu_times.sort();script_times.sort()
	report["gpu_median_ms"]=gpu_times[gpu_times.size()/2] if gpu_times.size() else 0
	report["render_cpu_median_ms"]=render_cpu_times[render_cpu_times.size()/2] if render_cpu_times.size() else 0
	report["script_median_ms"]=script_times[script_times.size()/2] if script_times.size() else 0
	report["draw_calls"]=Performance.get_monitor(Performance.RENDER_TOTAL_DRAW_CALLS_IN_FRAME)
	report["primitives"]=Performance.get_monitor(Performance.RENDER_TOTAL_PRIMITIVES_IN_FRAME)
	report["controls"]=control_results
	report["clock_validation"]=clock_validation
	report["speed"]=bridge.Speed
	report["map_size"]=map["size"]
	report["view_origin"]=[ORIGIN.x,ORIGIN.y]
	report["pending_simulation_seconds"]=bridge.Stats().pending
	report["frames"]=frame_times.size()
	report["median_frame_ms"]=frame_times[frame_times.size()/2] if frame_times.size()>0 else 0
	report["p95_frame_ms"]=frame_times[int(frame_times.size()*.95)] if frame_times.size()>0 else 0
	report["visible_creatures"]=agents.size()
	var predators=0
	for id in agents:
		if agent_data[id].predator: predators+=1
	report["visible_predators"]=predators
	report["renderer"]=RenderingServer.get_current_rendering_method()
	report["gpu"]=RenderingServer.get_video_adapter_name()
	report["camera_size"]=camera.size
	report["camera_focus"]=[focus.x,focus.y,focus.z]
	report["resolution"]=[image.get_width(),image.get_height()]
	report["world_meters"]=bridge.Stats()
	report["climate_visual"]=weather.status()
	report["corpse_views"]=corpse_views.size()
	var expected_corpses=0
	for c in bridge.GetCorpses():
		if _inside(Vector2(c.x,c.z)-ORIGIN,.4): expected_corpses+=1
	report["corpse_sync"]=corpse_views.size()==expected_corpses
	report["passed"]=report.passed and report.corpse_sync

	report["ui_card_below_meters"]=not ui.card.visible or ui.card.get_global_rect().position.y>=90
	FileAccess.open(capture_path+".json",FileAccess.WRITE).store_string(JSON.stringify(report,"  "))
	if smoke:
		var tick_before=bridge.CurrentTick
		await get_tree().create_timer(.75).timeout
		await RenderingServer.frame_post_draw
		get_viewport().get_texture().get_image().save_png(capture_path.trim_suffix(".png")+"-motion.png")
		report["paused_motion_same_tick"]=bridge.CurrentTick==tick_before
		FileAccess.open(capture_path+".json",FileAccess.WRITE).store_string(JSON.stringify(report,"  "))
	print("EVOSIM_CAPTURE ",JSON.stringify(report))
	get_tree().quit(0 if report.passed else 1)


# A solid diorama edge closes the cropped visual window, not the simulation map.

func _build_details():
	# Tiny meadow flowers use an instanced mesh; no simulation collision or food.
	var flower=SphereMesh.new()
	flower.radius=.045;flower.height=.065;flower.radial_segments=6;flower.rings=3
	flower.material=_mat("petal",Color("e8ddaa"))
	var poses=[]
	for sample in DecorationSampler.sample(ORIGIN,EXTENT,1.18,2):
		var p=Vector2(sample.x,sample.y)-ORIGIN
		if _field(p)<.75 or terrain_noise.get_noise_2d((sample.x-42)*1.8,(sample.y-20)*1.8)<.18: continue
		poses.append(Transform3D(Basis.IDENTITY,Vector3(p.x,_height(p)+.18,p.y)))
	var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D;mm.mesh=flower;mm.instance_count=poses.size()
	for i in poses.size(): mm.set_instance_transform(i,poses[i])
	var inst=MultiMeshInstance3D.new();inst.multimesh=mm;inst.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF;add_child(inst)
	# Shore reeds are land-side only: never block drinking or alter navigation.
	var reeds=_grass_mesh().duplicate(true)
	var reed_mat=reeds.surface_get_material(0).duplicate()
	reed_mat.set_shader_parameter("base_color",Color("66733a"))
	reeds.surface_set_material(0,reed_mat)
	poses=[]
	for sample in DecorationSampler.sample(ORIGIN,EXTENT,1.89,3):
		var p=Vector2(sample.x,sample.y)-ORIGIN
		var f=_field(p)
		if f<.1 or f>.8 or terrain_noise.get_noise_2d((sample.x-42)*2,(sample.y-20)*2)<-.1: continue
		poses.append(Transform3D(Basis(Vector3.UP,sample.z).scaled(Vector3(.8,2.5,.8)),Vector3(p.x,_height(p),p.y)))
	mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D;mm.mesh=reeds;mm.instance_count=poses.size()
	for i in poses.size(): mm.set_instance_transform(i,poses[i])
	inst=MultiMeshInstance3D.new();inst.multimesh=mm;inst.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF;add_child(inst)

func _update_canopies(_delta):
	# Local holes around animals preserve the rest of each tree's silhouette.
	var spots=PackedVector4Array()
	var size=get_viewport().get_visible_rect().size
	var candidates=[]
	canopy_reveal_ids.clear()
	for id in agents:
		var point=agents[id].position+Vector3.UP*.55
		if camera.is_position_behind(point):continue
		var uv=camera.unproject_position(point)/size
		if uv.x<-.06 or uv.x>1.06 or uv.y<-.06 or uv.y>1.06:continue
		candidates.append({"id":id,"point":point,"uv":uv,"priority":-1.0 if id==selected_id else uv.distance_squared_to(Vector2(.5,.5))})
	candidates.sort_custom(func(a,b):return a.priority<b.priority)
	for c in candidates.slice(0,16):
		canopy_reveal_ids.append(c.id)
		spots.append(Vector4(c.uv.x,c.uv.y,camera.to_local(c.point).z,clampf((1.25 if c.id==selected_id else .8)/camera.size,.012,.055)))
	var count=spots.size()
	while spots.size()<16: spots.append(Vector4.ZERO)
	for mesh in mesh_cache.values():
		for i in mesh.get_surface_count():
			var mat=mesh.surface_get_material(i)
			if mat is ShaderMaterial and mat.shader.resource_path.ends_with("foliage.gdshader"):
				mat.set_shader_parameter("reveal",spots)
				mat.set_shader_parameter("reveal_count",count)

func _pick_agent(pos):
	selected_tree_id=-1
	var nearest=28.0
	selected_id=-1
	for id in agents:
		var dist=camera.unproject_position(agents[id].position+Vector3.UP*.5).distance_to(pos)
		if dist<nearest: selected_id=id;nearest=dist
	if selected_id<0: ui.card.hide()
	elif observations: observations.open_panel("creature",selected_id)


func _pick_tree(pos):
	var nearest=22.0;var chosen=-1
	for tree in map.trees:
		var p=Vector2(tree.x,tree.z)-ORIGIN
		if not _inside(p,1.5):continue
		var trunk=Vector3(p.x,_height(p)+1.0,p.y)
		if camera.is_position_behind(trunk):continue
		var distance=camera.unproject_position(trunk).distance_to(pos)
		if distance<nearest:nearest=distance;chosen=int(tree.id)
	selected_tree_id=chosen
	if chosen>=0:observations.open_panel("tree",chosen)
	return chosen


func _control_smoke():
	clock_validation=bridge.ValidateClocks()
	control_results["fixed_tick_across_fps"]=clock_validation.passed
	var initial_speed=bridge.Speed
	ui.paused_button.pressed.emit()
	control_results["hud_pause_signal"]=bridge.Paused
	ui.paused_button.pressed.emit()
	ui.speed_button.pressed.emit()
	control_results["hud_speed_signal"]=bridge.Speed!=initial_speed
	bridge.Speed=initial_speed
	var quality_before=high_quality
	ui.quality_button.pressed.emit()
	control_results["hud_quality_signal"]=high_quality!=quality_before
	ui.quality_button.pressed.emit()
	var old_zoom=zoom
	var event=InputEventMouseButton.new();event.button_index=MOUSE_BUTTON_WHEEL_UP;event.pressed=true
	_unhandled_input(event)
	control_results["wheel_zoom"]=zoom<old_zoom
	zoom=old_zoom
	var key=InputEventKey.new();key.pressed=true;key.keycode=KEY_SPACE
	var before=bridge.CurrentTick
	_unhandled_input(key);bridge.Advance(.2)
	control_results["pause_holds_tick"]=bridge.Paused and bridge.CurrentTick==before
	_unhandled_input(key)
	control_results["resume"]=not bridge.Paused
	_toggle_quality();control_results["quality_toggle"]=high_quality!=quality_before;_toggle_quality()
	if agents.size():
		var id=agents.keys()[0]
		_pick_agent(camera.unproject_position(agents[id].position+Vector3.UP*.5))
		control_results["creature_pick"]=selected_id==id
		selected_id=-1;ui.card.hide()
	var previous_yaw=yaw
	key.keycode=KEY_E;_unhandled_input(key)
	control_results["camera_rotate"]=yaw>previous_yaw
	yaw=previous_yaw


func _build_edible_patches():
	var mesh=_grass_mesh().duplicate(true)
	var mat=mesh.surface_get_material(0).duplicate()
	mat.set_shader_parameter("base_color",Color("799b38"))
	mat.set_shader_parameter("use_growth",true)
	mesh.surface_set_material(0,mat)
	var groups={}
	for z in 48:
		for x in 48:
			var cell=(z+int(ORIGIN.y))*int(map["size"])+x+int(ORIGIN.x)
			if map.food_reference[cell]<=0 or _tile(x+int(ORIGIN.x),z+int(ORIGIN.y))!=0: continue
			var chunk=Vector2i(x/12,z/12)
			if not groups.has(chunk): groups[chunk]=[]
			rng.seed=(int(x+ORIGIN.x)*73856093)^(int(z+ORIGIN.y)*19349663)^991
			for i in 14:
				var p=Vector2(x+.5+rng.randf_range(-.42,.42),z+.5+rng.randf_range(-.42,.42))
				var pose=Transform3D(Basis(Vector3.UP,rng.randf()*TAU).scaled(Vector3(1.1,1.4,1.1)),Vector3(p.x,_height(p),p.y))
				groups[chunk].append({"cell":cell,"pose":pose})
	for chunk in groups:
		var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D;mm.use_custom_data=true
		mm.mesh=mesh;mm.instance_count=groups[chunk].size()
		for i in mm.instance_count:
			var item=groups[chunk][i]
			mm.set_instance_transform(i,item.pose)
			mm.set_instance_custom_data(i,Color(clampf(map.food[item.cell]/maxf(.00001,map.food_reference[item.cell]),0,1),0,0,0))
			if not edible_patches.has(item.cell): edible_patches[item.cell]={"mm":mm,"indices":[],"reference_stock":maxf(.00001,float(map.food_reference[item.cell]))}
			edible_patches[item.cell].indices.append(i)
		var node=MultiMeshInstance3D.new();node.multimesh=mm
		node.add_to_group("edible_grass")
		node.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF
		add_child(node)

func _update_edible_patches():
	var food=bridge.GetGroundFood()
	for cell in edible_patches:
		var patch=edible_patches[cell]
		var growth=clampf(food[cell]/patch.reference_stock,0,1)
		for index in patch.indices: patch.mm.set_instance_custom_data(index,Color(growth,0,0,0))


func _build_ferns():
	var poses=[]
	for t in map.trees:
		rng.seed=int(t.x*9371+t.z*6733)^717
		for i in 3:
			var p=Vector2(t.x,t.z)-ORIGIN+Vector2.from_angle(rng.randf()*TAU)*rng.randf_range(.8,2.0)
			var angle=rng.randf()*TAU;var size_scale=rng.randf_range(.8,1.3)
			if not _inside(p,1) or _field(p)<.8: continue
			poses.append(Transform3D(Basis(Vector3.UP,angle).scaled(Vector3.ONE*size_scale),Vector3(p.x,_height(p),p.y)))
	var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D
	mm.mesh=load("res://scripts/procedural_meshes.gd").fern();mm.instance_count=poses.size()
	for i in poses.size(): mm.set_instance_transform(i,poses[i])
	var node=MultiMeshInstance3D.new();node.multimesh=mm;node.visibility_range_end=105
	add_child(node)


func _tile(x: int,z: int) -> int: return surface._tile(x,z)
func _field(p: Vector2) -> float: return surface._field(p)
func _height(p: Vector2) -> float: return surface._height(p)

func _build_terrain():
	var terrain=load("res://scripts/terrain_view.gd").new()
	add_child(terrain)
	terrain.build(surface)

func _update_corpses():
	var present={}
	for c in bridge.GetCorpses():
		var p=Vector2(c.x,c.z)-ORIGIN
		if not _inside(p,.4): continue
		present[c.id]=true
		if not corpse_views.has(c.id):
			var view=load("res://scripts/creature_view.gd").new();view.build(_mesh,c.predator)
			add_child(view);corpse_views[c.id]=view
		var view=corpse_views[c.id]
		view.position=Vector3(p.x,_height(p)+.1,p.y);view.scale=Vector3.ONE*c.radius*1.8
		view.fall(c.decay)
	for id in corpse_views.keys():
		if not present.has(id): corpse_views[id].queue_free();corpse_views.erase(id)

func _rebuild_world(reset_view=true):
	for node in scenery:
		if is_instance_valid(node): node.free()
	for node in agents.values(): node.free()
	for node in corpse_views.values(): node.free()
	agents.clear();agent_data.clear();corpse_views.clear();tree_nodes.clear();fruit_nodes.clear();edible_patches.clear()
	selected_id=-1;selected_tree_id=-1;last_tick=-1;ui.card.hide()
	if reset_view:
		observations.clear();ORIGIN=Vector2(42,20)
	map=bridge.GetMap();surface=load("res://scripts/world_surface.gd").new(map,ORIGIN)
	RenderingServer.global_shader_parameter_set("evosim_region_offset",ORIGIN-Vector2(42,20))
	rng.seed=2407
	var before=get_children()
	_build_terrain();_build_trees();_build_rocks();_build_ground_cover();_build_edible_patches();_build_fruit();_build_details();_build_ferns()
	scenery=get_children().filter(func(node): return not node in before)
	if reset_view: focus=Vector3(24,0,24)
	_update_agents(1);_update_corpses()
	if minimap:
		if reset_view:minimap.rebuild()
		else:minimap.queue_redraw()

func navigate_world(point: Vector2):
	var new_origin=(point-Vector2.ONE*24).clamp(Vector2.ZERO,Vector2.ONE*(float(map["size"])-EXTENT))
	new_origin=new_origin.snapped(Vector2.ONE*4)
	var local=point-new_origin
	if new_origin!=ORIGIN:
		var previous_selection=selected_id
		ORIGIN=new_origin;focus=Vector3(local.x,0,local.y);_rebuild_world(false)
		selected_id=previous_selection if agents.has(previous_selection) else -1
	else: focus=Vector3(local.x,0,local.y)

func _notification(what):
	if what==NOTIFICATION_WM_CLOSE_REQUEST:
		if bridge and library and library.entered_world and bridge.CanResume and not bridge.SaveCurrent(bridge.WorldName):
			library.show_library();return
		get_tree().quit()
