extends SceneTree
var terrain_script=load("res://scripts/terrain_view.gd")
func _initialize():call_deferred("run")
func signature(node):
	var digest=HashingContext.new();digest.start(HashingContext.HASH_SHA256)
	for child in node.get_children():
		digest.update(var_to_bytes(child.transform))
		if child.mesh is ArrayMesh:
			for index in child.mesh.get_surface_count():digest.update(var_to_bytes(child.mesh.surface_get_arrays(index)))
		if child.material_override is ShaderMaterial:
			var m=child.material_override
			digest.update(var_to_bytes(m.get_shader_parameter("region_offset")))
			if m.shader.resource_path.ends_with("water.gdshader"):
				digest.update(m.get_shader_parameter("shore_mask").get_image().get_data())
	return digest.finish().hex_encode()
func build(data,origin):
	var surface=load("res://scripts/world_surface.gd").new(data,origin)
	var terrain=terrain_script.new();root.add_child(terrain)
	var start=Time.get_ticks_usec();terrain.build(surface)
	return {"node":terrain,"ms":(Time.get_ticks_usec()-start)/1000.0,"reused":terrain.reused_region,"hash":signature(terrain)}
func run():
	create_timer(60).timeout.connect(func():quit(2))
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	var before=bridge.StateDigest();var data=bridge.GetMap();var origin=Vector2(42,20)
	terrain_script.region_cache.clear();terrain_script.region_order.clear()
	var first=build(data,origin);var warm=build(data,origin)
	var checks={"first_is_cold":not first.reused,"second_is_reused":warm.reused,"geometry_shore_transform_exact":first.hash==warm.hash,"distinct_scene_nodes":first.node!=warm.node}
	var forest_data=data.duplicate(true)
	for i in forest_data.tiles.size():
		if forest_data.tiles[i]==0:forest_data.tiles[i]=2;break
	var forest_change=build(forest_data,origin)
	checks.tree_growth_keeps_terrain_cache=forest_change.reused and forest_change.hash==first.hash
	forest_change.node.free()
	first.node.free();warm.node.free()
	for other in [Vector2(0,0),Vector2(20,20),Vector2(60,60)]:
		var result=build(data,other);result.node.free()
	checks.bounded_three_regions=terrain_script.region_cache.size()==3
	checks.least_recent_evicted=not terrain_script.region_cache.has(origin)
	var recent=build(data,Vector2(60,60));checks.recent_survives=recent.reused;recent.node.free()
	checks.biology_unchanged=before==bridge.StateDigest()
	bridge.CreateWorld("Başka topoğrafya",123,{"Herbivores":1,"Carnivores":0})
	var changed=build(bridge.GetMap(),Vector2(60,60))
	checks.topology_invalidates=not changed.reused and terrain_script.region_cache.size()==1
	checks.new_geometry_differs=changed.hash!=recent.hash;changed.node.free()
	var report={"checks":checks,"cold_ms":first.ms,"revisit_ms":warm.ms,"passed":checks.values().all(func(v):return v)}
	FileAccess.open("res://validation/region-cache-46.json",FileAccess.WRITE).store_string(JSON.stringify(report,"  "))
	print("REGION_CACHE_CHECKS ",report);quit(0 if report.passed else 1)
