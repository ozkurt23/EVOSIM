extends SceneTree
var checks={}
func _initialize(): call_deferred("run")
func capture(name):
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/"+name+"-28.png")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	await create_timer(1).timeout
	await capture("library")
	checks.minimap_visible=scene.minimap.get_global_rect().position.y>=0 and scene.minimap.get_global_rect().end.y<=root.get_visible_rect().size.y
	checks.menu_pauses=scene.bridge.Paused and scene.library.overlay.visible
	if scene.bridge.LivingForest:
		scene.library._choose_legacy();await capture("legacy-import-picker")
		checks.import_filter=scene.library.import_dialog.filters[0].begins_with("*.evoworld")
		scene.library.import_dialog.canceled.emit();await process_frame
		scene.library._library()
	scene.library._setup();await capture("world-setup")
	checks.settings_count=scene.library.editors.size()==scene.bridge.Settings().size()
	scene.library._preset(2)
	checks.immortal_preset=scene.library.values.HerbLifespan==0 and scene.library.values.CarnLifespan==0
	scene.library._preset(0)
	scene.library.close();scene.bridge.Paused=true
	scene.observations.open_panel("population")
	for id in range(1,9): scene.observations.open_panel("creature",id)
	checks.nine_panels=scene.observations.panels.size()==9
	var counts=[0,0,0,0]
	for p in scene.observations.panels: counts[p.slot]+=1
	checks.round_robin=counts==[3,2,2,2]
	checks.graph_retained=scene.observations.panels.any(func(p): return p.kind=="population")
	var first=scene.observations.panels.filter(func(p): return p.kind=="population")[0];scene.observations._front(first)
	checks.front_selection=scene.observations.panels[-1]==first
	await capture("observation-stacks")
	for id in range(9,25): scene.observations.open_panel("creature",id)
	await process_frame
	checks.many_panels_inside=scene.observations.panels.all(func(p): return p.node.get_global_rect().end.y<=root.get_visible_rect().size.y)
	scene.observations.clear()
	scene.observations.clear();scene.observations.open_panel("timeline");scene.observations.open_panel("events")
	await capture("timeline")
	checks.no_fake_history=scene.bridge.HeadTick==0
	checks.create_other_seed=scene.bridge.CreateWorld("Akarsu",89,{"Herbivores":12,"Carnivores":2,"TreeCount":80})
	scene._rebuild_world()
	checks.rebuild_clears_panels=scene.observations.panels.is_empty()
	scene.navigate_world(Vector2(100,100))
	checks.reaches_distant_region=scene.ORIGIN.x>50 and scene.ORIGIN.y>50
	await capture("distant-region")
	checks.create_large_map=scene.bridge.CreateWorld("Büyük alan",42,{"MapSize":256,"Herbivores":12,"Carnivores":2})
	scene._rebuild_world();scene.navigate_world(Vector2(230,230))
	checks.large_region=scene.ORIGIN.x>128
	checks.create_small_after_large=scene.bridge.CreateWorld("Küçük alan",42,{"MapSize":128,"Herbivores":12,"Carnivores":2})
	var biological_hash=scene.bridge.StateDigest()
	scene._rebuild_world();scene._update_edible_patches()
	checks.small_origin_reset=scene.ORIGIN==Vector2(42,20)
	checks.renderer_preserves_state=biological_hash==scene.bridge.StateDigest()
	var ok=checks.values().all(func(v): return v)
	FileAccess.open("res://validation/ui-checks-19.json",FileAccess.WRITE).store_string(JSON.stringify({"passed":ok,"checks":checks},"  "))
	print("UI_CHECKS ",checks);quit(0 if ok else 1)
