extends SceneTree
func _initialize():call_deferred("run")
func run():
	var prefix="before35"
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--art-prefix="):prefix=arg.trim_prefix("--art-prefix=")
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.library.overlay.hide();scene.bridge.Paused=true
	for zoom in [26,16]:
		scene.zoom=zoom;scene.camera.size=zoom
		for warm in 30:await process_frame;await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("res://previews/%s-%d.png"%[prefix,zoom])
	quit()
