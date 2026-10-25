extends SceneTree
func _initialize():call_deferred("run")
func run():
	create_timer(60).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.library.overlay.hide();scene.bridge.Paused=true
	scene.navigate_world(Vector2(64,44)) # origin(40,20), aligned to the four-cell navigation grid
	var digest=scene.bridge.StateDigest();var texture=scene.minimap.texture
	scene.observations.open_panel("population")
	var cold_start=Time.get_ticks_usec();scene.navigate_world(Vector2(100,100));var cold_ms=(Time.get_ticks_usec()-cold_start)/1000.0
	var warm_start=Time.get_ticks_usec();scene.navigate_world(Vector2(64,44));var warm_ms=(Time.get_ticks_usec()-warm_start)/1000.0
	var terrain=scene.scenery.filter(func(n):return n.get_script()==load("res://scripts/terrain_view.gd"))[0]
	var checks={"revisited_terrain_cached":terrain.reused_region,"minimap_texture_retained":texture==scene.minimap.texture,"observations_retained":scene.observations.panels.size()==1,"biology_unchanged":digest==scene.bridge.StateDigest(),"origin_restored":scene.ORIGIN==Vector2(40,20)}
	for frame in 30:await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/region-revisit-38.png")
	var report={"checks":checks,"cold_region_total_ms":cold_ms,"revisit_total_ms":warm_ms,"passed":checks.values().all(func(v):return v)}
	FileAccess.open("res://validation/region-revisit-38.json",FileAccess.WRITE).store_string(JSON.stringify(report,"  "))
	print("REGION_REVISIT ",report);quit(0 if report.passed else 1)
