extends SceneTree
func _initialize(): call_deferred("run")
func run():
	if DisplayServer.get_name()=="headless":
		push_error("Food visual checks require a real rendering server; run without --headless.")
		quit(2);return
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	await process_frame
	var checks=scene.bridge.ValidateFoodReference()
	var before=scene.bridge.StateDigest()
	var cell=scene.edible_patches.keys()[0]
	var nearest=INF
	for key in scene.edible_patches:
		var p=Vector2(int(key)%int(scene.map.size),int(int(key)/int(scene.map.size)))-scene.ORIGIN+Vector2(.5,.5)
		var clearance=INF
		for tree in scene.tree_nodes: clearance=minf(clearance,p.distance_to(tree))
		if clearance>3 and p.distance_to(Vector2(24,24))<nearest:
			nearest=p.distance_to(Vector2(24,24));cell=key
	var point=Vector2(int(cell)%int(scene.map.size),int(int(cell)/int(scene.map.size)))-scene.ORIGIN+Vector2(.5,.5)
	scene.focus=Vector3(point.x,scene._height(point),point.y);scene.zoom=4;scene.camera.size=4;scene._update_camera(1)
	scene.library.overlay.hide();scene.ui.hide();scene.observations.hide();scene.set_process(false)
	var reference=scene.edible_patches[cell].reference_stock
	# Synthetic renderer snapshot only; the actual biological world is untouched.
	var food=scene.map.food.duplicate();food[cell]=0;scene.map.food=food
	for node in get_nodes_in_group("edible_grass"): node.free()
	scene.edible_patches.clear();scene._build_edible_patches()
	checks.depleted_cell_still_has_instances=scene.edible_patches.has(cell)
	var patch=scene.edible_patches[cell]
	checks.depleted_initially_hidden=is_zero_approx(patch.mm.get_instance_custom_data(patch.indices[0]).r)
	checks.rebuild_reference_stable=is_equal_approx(reference,patch.reference_stock)
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/food-depleted-30.png")
	scene._update_edible_patches()
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/food-restored-30.png")
	checks.actual_stock_restores_visual=patch.mm.get_instance_custom_data(patch.indices[0]).r>0
	var actual_food=scene.bridge.GetGroundFood()
	var matching=true
	for key in scene.edible_patches:
		var other=scene.edible_patches[key]
		var expected=clampf(actual_food[key]/other.reference_stock,0,1)
		matching=matching and absf(other.mm.get_instance_custom_data(other.indices[0]).r-expected)<.002
	checks.all_patch_growth_matches_actual_stock=matching
	checks.renderer_did_not_change_biology=before==scene.bridge.StateDigest()
	checks.passed=checks.values().all(func(v): return v)
	FileAccess.open("res://validation/food-checks-30.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FOOD_CHECKS ",checks);quit(0 if checks.passed else 1)
