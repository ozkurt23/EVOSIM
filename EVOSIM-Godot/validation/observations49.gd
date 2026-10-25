extends SceneTree
func _initialize():call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);await process_frame
	scene.library.overlay.hide();scene.bridge.Paused=false;scene.bridge.Speed=20
	for i in 300:scene.bridge.Advance(1./60)
	scene.bridge.Paused=true
	var checks=scene.bridge.ValidateObservations()
	var before=scene.bridge.StateDigest()
	scene.observations.inspect_region(Vector2(64,44));scene.observations.open_panel("population");scene.observations.open_panel("genes")
	scene.observations.open_panel("family",1);scene.observations.tracked_ancestor=1;scene.observations._refresh()
	checks.region_panel=scene.observations.panels.any(func(p):return p.kind=="region" and p.text.text.contains("Son 30"))
	checks.history_has_flows=scene.bridge.PopulationHistory().all(func(s):return s.has("births") and s.has("deaths"))
	checks.founder_overlay=scene.observations.panels.filter(func(p):return p.kind=="genes")[0].graph.founder_herb.size()==10
	checks.observation_read_only=before==scene.bridge.StateDigest()
	checks.map_density_total=Array(scene.bridge.ObservationMap(1)).reduce(func(n,r):return n+r.value,0)==scene.bridge.Stats().herbivores+scene.bridge.Stats().carnivores
	checks.map_empty_mode=scene.bridge.ObservationMap(0).is_empty()
	checks.success_counts=scene.bridge.ReproductiveSuccess(0).all(func(r):return r.offspring>=0 and r.gene>=0 and r.gene<=1)
	scene.observations.open_panel("success");scene.observations.open_panel("outcomes")
	scene.observations.map_mode=1;scene.observations._refresh()
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/observations49.png")
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/observations49.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "));print("OBSERVATIONS49 ",checks);quit(0 if checks.passed else 1)
