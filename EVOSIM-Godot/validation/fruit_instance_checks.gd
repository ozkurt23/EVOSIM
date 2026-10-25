extends SceneTree
func _initialize():call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);scene.bridge.Paused=true;scene.library.overlay.hide()
	var checks={"one_fruit_draw_object":scene.fruit_visuals.size()==1,"has_fruit_slots":scene.fruit_nodes.size()>100}
	await process_frame;await RenderingServer.frame_post_draw
	var stock={}
	for f in scene.bridge.GetFruit():stock[Vector3(f.tree_x,f.tree_z,f.slot)]=f.available
	scene._update_fruit_stock(stock)
	checks.stock_matches=scene.fruit_nodes.all(func(f):return f[6]==stock.get(Vector3(f[1],f[2],f[3]),false))
	var before=scene.bridge.StateDigest()
	scene._update_fruit_stock({})
	await process_frame;await RenderingServer.frame_post_draw
	checks.empty_slots_hidden=scene.fruit_nodes.all(func(f):return f[0].get_instance_transform(f[4]).basis.x.length()<.00001)
	scene._update_fruit_stock(stock)
	await process_frame;await RenderingServer.frame_post_draw
	checks.stock_restored=scene.fruit_nodes.all(func(f):return (f[0].get_instance_transform(f[4]).basis.x.length()>.5)==f[6])
	checks.read_only=before==scene.bridge.StateDigest()
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/fruit-instances47.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FRUIT_INSTANCES ",checks);quit(0 if checks.passed else 1)
