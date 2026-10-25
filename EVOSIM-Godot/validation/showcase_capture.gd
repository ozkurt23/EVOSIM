extends SceneTree
## Real application capture; isolated world, unchanged biological core.
var scene
var viewport: SubViewport
var out=ProjectSettings.globalize_path("res://../EVOSIM-Showcase/source")
var preview=false
var candidate_scout=false
var retake=false
var records=[]
var subject=-1
var distance_history={}
var wide_focus=Vector3(24,0,24)
var chapters=[0,60,210,285,360,420,480,540,690,765,810]
func _initialize(): call_deferred("run")
func drain():
	var guard=0
	while float(scene.bridge.Stats().pending)>=.05 and guard<1000:
		scene.bridge.Advance(0);guard+=1
	assert(guard<1000)
func advance():
	scene._process(1.0/30.0);drain()
func select_subject(predator):
	var best=-1;var best_score=-INF
	for id in scene.agent_data:
		var a=scene.agent_data[id]
		var p=scene.agents[id].position
		if a.predator!=predator or a.swimming or p.x<9 or p.x>39 or p.z<9 or p.z>39: continue
		var openness=8.0
		for tree in scene.tree_nodes: openness=minf(openness,Vector2(p.x,p.z).distance_to(tree))
		var actual_motion=0.0
		if distance_history.has(id): actual_motion=a.distance-distance_history[id][0]
		var score=actual_motion*5+openness*.55-p.distance_to(Vector3(24,0,24))*.04
		if score>best_score: best=id;best_score=score
	return best
func set_shot(frame):
	scene.library.overlay.hide();scene.observations.clear();scene.selected_id=-1
	scene.ui.show();scene.observations.show();scene.bridge.Paused=false;scene.bridge.Speed=1
	scene.follow_capture=false;subject=-1
	match frame:
		0:
			scene.ui.hide();scene.observations.hide();scene.focus=wide_focus;scene.zoom=24
		60:
			scene.ui.hide();scene.observations.hide();scene.focus=wide_focus;scene.zoom=23.5
		210,285:
			scene.ui.hide();scene.observations.hide();subject=463 if frame==285 else 336
			scene.zoom=8.5 if frame==285 else 9.5
			if subject>=0: scene.focus=scene.agents[subject].position
		360,420,480:
			scene.bridge.Speed={360:2,420:8,480:20}[frame]
			scene.focus=wide_focus;scene.zoom=22
		540:
			scene.focus=wide_focus+Vector3(2,0,-2);scene.zoom=21
			scene.observations.open_panel("population")
			subject=select_subject(false)
			for id in scene.agent_data:
				if scene.agent_data[id].generation>0: subject=id;break
			if subject>=0: scene.observations.open_panel("creature",subject);scene.selected_id=subject
		690:
			scene.bridge.Paused=true;scene.library._setup();scene.library.overlay.show()
			scene.library.world_name.text="Yaşam Vadisi"
			scene.library.seed_input.value=42
		765:
			scene.bridge.ConfigureScenario("rain")
			scene.focus=wide_focus;scene.zoom=22
			scene.weather.update_weather(scene.bridge.Stats(),5.0,true)
		810:
			scene.ui.hide();scene.observations.hide();scene.focus=wide_focus;scene.zoom=24
	# Converge temporal AA after a cut without advancing the simulation.
	scene.camera.size=scene.zoom;scene._update_camera(1)
func camera_move(frame):
	if frame<210:
		var t=float(frame)/210.0
		scene.focus=wide_focus+Vector3(-1.2+2.4*t,0,-.7+1.4*t)
		scene.zoom=24-2*t
	elif frame<360 and subject>=0 and scene.agents.has(subject):
		scene.focus=scene.focus.lerp(scene.agents[subject].position,.07)
		scene.zoom=(8.5 if frame>=285 else 9.5)-float((frame-210)%75)/75.0*.4
	elif frame>=810:
		var t=float(frame-810)/89.0
		scene.zoom=22+2*smoothstep(0,1,t)
		scene.focus=wide_focus+Vector3(t,0,t*.5)
func run():
	preview="--storyboard" in OS.get_cmdline_user_args()
	candidate_scout="--candidates" in OS.get_cmdline_user_args()
	retake="--retake" in OS.get_cmdline_user_args()
	DirAccess.make_dir_recursive_absolute(out)
	viewport=SubViewport.new();viewport.size=Vector2i(3840,2160)
	viewport.size_2d_override=Vector2i(1600,900);viewport.size_2d_override_stretch=true
	viewport.own_world_3d=true;viewport.render_target_update_mode=SubViewport.UPDATE_ALWAYS
	viewport.msaa_3d=Viewport.MSAA_4X;viewport.use_taa=true
	root.add_child(viewport)
	var display=TextureRect.new();display.texture=viewport.get_texture();display.expand_mode=TextureRect.EXPAND_IGNORE_SIZE
	display.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT);root.add_child(display)
	scene=load("res://main.tscn").instantiate();viewport.add_child(scene);scene.set_process(false)
	scene.library.overlay.hide();scene.library.entered_world=false
	assert(scene.bridge.CreateWorld("EVOSIM · Tanıtım",42,{"Herbivores":420,"Carnivores":48,"TreeCount":300}))
	scene.bridge.ClimateCycle=false;scene._rebuild_world();scene.bridge.Paused=false
	# Five days of actual simulation, no staged positions or invented graph samples.
	for i in 20: scene.bridge.Advance(.25);drain()
	scene._update_agents(1)
	Input.mouse_mode=Input.MOUSE_MODE_HIDDEN
	for frame in 900:
		if frame in chapters:
			set_shot(frame)
			for warmup in 5: await process_frame;await RenderingServer.frame_post_draw
		if frame==730:
			for child in scene.library.column.get_children():
				if child is TabContainer: child.current_tab=3
		camera_move(frame);advance()
		for id in scene.agent_data:
			if not distance_history.has(id): distance_history[id]=[]
			distance_history[id].append(scene.agent_data[id].distance)
			if distance_history[id].size()>45: distance_history[id].pop_front()
		await process_frame;await RenderingServer.frame_post_draw
		if candidate_scout and frame in [210,285]:
			await capture_candidates(frame)
		if (not candidate_scout and (not retake or (frame>=210 and frame<360))) and (not preview or frame in chapters):
			var pic=viewport.get_texture().get_image()
			assert(pic.get_width()==3840 and pic.get_height()==2160)
			var prefix="scout" if preview else "frame"
			pic.save_jpg(out+"/%s_%04d.jpg"%[prefix,frame],.98)
		if (candidate_scout or retake) and frame==359: break
		if frame%30==0:
			records.append({"frame":frame,"tick":scene.bridge.CurrentTick,"speed":scene.bridge.Speed,"visible_creatures":scene.agents.size(),"stats":scene.bridge.Stats()})
			print("SHOWCASE_FRAME ",frame," tick=",scene.bridge.CurrentTick," local=",scene.agents.size())
	FileAccess.open(out+("/candidates.json" if candidate_scout else ("/retake.json" if retake else ("/scout.json" if preview else "/capture.json"))),FileAccess.WRITE).store_string(JSON.stringify({"native_resolution":[3840,2160],"fps":30,"frames":900,"offline_capture":true,"real_core":true,"records":records},"  "))
	quit()

func capture_candidates(frame):
	var original_focus=scene.focus
	var candidates=[]
	for id in scene.agent_data:
		var a=scene.agent_data[id]
		var p=scene.agents[id].position
		if a.predator!=(frame==285) or a.swimming: continue
		if p.x<3 or p.x>45 or p.z<3 or p.z>45: continue
		var openness=8.0
		for tree in scene.tree_nodes: openness=minf(openness,Vector2(p.x,p.z).distance_to(tree))
		candidates.append({"id":id,"openness":openness,"position":p,"speed":a.speed})
	candidates.sort_custom(func(a,b):return a.openness>b.openness)
	for c in candidates.slice(0,12):
		scene.focus=c.position;scene._update_camera(1)
		for warm in 3: await process_frame;await RenderingServer.frame_post_draw
		var pic=viewport.get_texture().get_image()
		pic.resize(1280,720)
		pic.save_jpg(out+"/candidate_%04d_%04d.jpg"%[frame,c.id],.94)
	FileAccess.open(out+"/candidates_%d.json"%frame,FileAccess.WRITE).store_string(JSON.stringify(candidates,"  "))
	scene.focus=original_focus;scene._update_camera(1)
