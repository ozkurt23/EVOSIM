extends SceneTree
func _initialize():call_deferred("run")
func run():
	create_timer(50).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.bridge.CreateWorld("Görünürlük denemesi",42,{"Herbivores":420,"Carnivores":48})
	scene._rebuild_world();scene.library.overlay.hide();scene.bridge.Paused=true
	scene.zoom=13;scene.camera.size=13;scene.focus=Vector3(24,0,24);scene._update_camera(1)
	var before=scene.bridge.StateDigest()
	scene._update_canopies(0)
	var checks={"bounded_shader_slots":scene.canopy_reveal_ids.size()<=16}
	var size=scene.get_viewport().get_visible_rect().size
	checks.only_on_screen_targets=scene.canopy_reveal_ids.all(func(id):
		var p=scene.camera.unproject_position(scene.agents[id].position+Vector3.UP*.55)/size
		return p.x>=-.06 and p.x<=1.06 and p.y>=-.06 and p.y<=1.06)
	checks.nonempty_in_crowd=not scene.canopy_reveal_ids.is_empty()
	if checks.nonempty_in_crowd:
		var id=scene.canopy_reveal_ids[-1];scene.selected_id=id;scene._update_canopies(0)
		checks.selected_has_priority=scene.canopy_reveal_ids[0]==id
	checks.biology_unchanged=before==scene.bridge.StateDigest()
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/canopy-34.png")
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/canopy-checks-34.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("CANOPY_CHECKS ",checks);quit(0 if checks.passed else 1)
