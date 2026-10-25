extends SceneTree
func _initialize(): call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);scene.library.overlay.hide();scene.bridge.Paused=false
	await create_timer(2).timeout
	var best=-1;var score=-1.
	for id in scene.agent_data:
		var c=scene.agent_data[id];var value=c.speed+(2 if c.predator else 0)
		if value>score: score=value;best=id
	if best<0: quit(1);return
	scene.selected_id=best;scene.follow_capture=true;scene.zoom=9
	scene.observations.open_panel("creature",best)
	await create_timer(1).timeout
	var start=scene.bridge.CurrentTick
	for i in 60:
		await create_timer(.10).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("res://previews/live-motion-48-%02d.png"%i)
	FileAccess.open("res://validation/live-motion-48.json",FileAccess.WRITE).store_string(JSON.stringify({"creature_id":best,"start_tick":start,"end_tick":scene.bridge.CurrentTick,"frames":60,"synthetic":false,"source":"Actual Core simulation and Godot viewport"},"  "))
	quit()
