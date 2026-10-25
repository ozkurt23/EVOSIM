extends SceneTree
var scene
var checks={}
func _initialize():call_deferred("run")
func advance_days(days):
	var target=scene.bridge.CurrentTick+int(days*20)
	scene.bridge.Paused=false;scene.bridge.Speed=20
	var iterations=0
	while scene.bridge.CurrentTick<target and iterations<100000:
		scene.bridge.Advance(.25);iterations+=1
		if iterations%80==0:await process_frame
	scene.bridge.Paused=true
	checks.advance_progress=scene.bridge.CurrentTick>=target
	scene._refresh_forest()
func capture(name):
	scene._process(0)
	for i in 12:await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/"+name+".png")
func run():
	create_timer(120).timeout.connect(func():quit(2))
	scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.set_process(false);scene.library.overlay.hide()
	checks.forest_host=scene.bridge.LivingForest
	checks.settings=scene.bridge.Settings().filter(func(s):return s.group=="Yaşayan orman").size()==7
	checks.create=scene.bridge.CreateWorld("Orman yaşam döngüsü testi",42,{"TreeCount":300,"Herbivores":20,"Carnivores":2,"ForestLifespan":.15,"ForestMaturity":.005,"ForestSeedSpread":1.0,"ForestDensity":.8})
	scene.bridge.ClimateCycle=false;scene._rebuild_world();scene._update_camera(1)
	var initial=scene.bridge.GetTreeStates();var first=int(initial[0].id)
	checks.stable_id_initial=not scene.bridge.TreeDetails(first).is_empty()
	scene.observations.open_panel("resources")
	var visible=initial.filter(func(t):return scene._inside(Vector2(t.x,t.z)-scene.ORIGIN,1.5))
	if not visible.is_empty():scene.observations.open_panel("tree",int(visible[0].id))
	await capture("forest-mature45")
	await advance_days(20)
	var earlier=scene.bridge.StateDigest();var day=scene.bridge.CurrentTick*.05
	checks.save=scene.bridge.SaveCurrent("Orman yaşam döngüsü testi")
	var terrain_before=scene.surface
	await advance_days(45)
	var states=scene.bridge.GetTreeStates()
	checks.dead_trees=states.any(func(t):return t.dead)
	checks.new_seedlings=states.any(func(t):return int(t.id)>300)
	checks.stable_id_no_retarget=scene.bridge.TreeDetails(first).is_empty() or int(scene.bridge.TreeDetails(first).id)==first
	checks.renderer_matches=scene.forest_instances.size()==states.filter(func(t):return scene._inside(Vector2(t.x,t.z)-scene.ORIGIN,1.5)).size()
	await capture("forest-renewal45")
	checks.terrain_preserved=scene.surface==terrain_before
	var head=scene.bridge.StateDigest();checks.save_head=scene.bridge.SaveCurrent("Orman yaşam döngüsü testi")
	checks.seek=scene.bridge.SeekDay(day)
	while scene.bridge.Seeking:scene.bridge.Advance(0)
	checks.seek_exact=scene.bridge.StateDigest()==earlier
	scene._refresh_forest();await capture("forest-rewind45")
	scene.bridge.ReturnToPresent();checks.head_exact=scene.bridge.StateDigest()==head
	var slots=scene.bridge.ListWorlds(false);checks.reopen=scene.bridge.OpenWorld(slots[0].id)
	checks.reopen_exact=scene.bridge.StateDigest()==head
	await advance_days(50)
	checks.original_trunks_removed=scene.bridge.GetTreeStates().all(func(t):return int(t.id)>300)
	checks.expired_inspection_empty=scene.bridge.TreeDetails(first).is_empty()
	checks.branch_seek=scene.bridge.SeekDay(day)
	while scene.bridge.Seeking:scene.bridge.Advance(0)
	checks.branch=scene.bridge.BranchHistory("Orman dalı")
	checks.branch_origin=scene.bridge.StateDigest()==earlier
	checks.library_has_independent_worlds=scene.bridge.ListWorlds(false).size()==2
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/forest-host45.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FOREST_HOST ",checks);quit(0 if checks.passed else 1)
