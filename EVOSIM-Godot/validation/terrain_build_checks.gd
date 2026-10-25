extends SceneTree
func _initialize():call_deferred("run")
func mesh_hash(node):
	var hash=HashingContext.new();hash.start(HashingContext.HASH_SHA256)
	for child in node.get_children():
		if child is MeshInstance3D and child.mesh is ArrayMesh:
			for surface_index in child.mesh.get_surface_count():
				var arrays=child.mesh.surface_get_arrays(surface_index)
				for index in [Mesh.ARRAY_VERTEX,Mesh.ARRAY_NORMAL]:
					if arrays[index]!=null: hash.update(arrays[index].to_byte_array())
	return hash.finish().hex_encode()
func run():
	create_timer(90).timeout.connect(func():quit(2))
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	var before=bridge.StateDigest();var results=[]
	for origin in [Vector2(42,20),Vector2(60,60),Vector2(0,0)]:
		var surface=load("res://scripts/world_surface.gd").new(bridge.GetMap(),origin)
		var hashes=[];var times=[]
		for path in ["res://validation/terrain_baseline_32.gd","res://scripts/terrain_view.gd"]:
			var terrain=load(path).new();root.add_child(terrain)
			var start=Time.get_ticks_usec();terrain.build(surface);times.append((Time.get_ticks_usec()-start)/1000.0)
			hashes.append(mesh_hash(terrain));terrain.free()
		results.append({"origin":str(origin),"mesh_identical":hashes[0]==hashes[1],"baseline_ms":times[0],"cached_ms":times[1],"speedup":times[0]/times[1]})
	var result={"samples":results,"biology_unchanged":before==bridge.StateDigest()}
	result.passed=result.biology_unchanged and results.all(func(r):return r.mesh_identical)
	FileAccess.open("res://validation/terrain-build-33.json",FileAccess.WRITE).store_string(JSON.stringify(result,"  "))
	print("TERRAIN_BUILD ",result);quit(0 if result.passed else 1)
