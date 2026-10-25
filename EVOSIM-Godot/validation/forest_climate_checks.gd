extends "res://validation/forest_host_checks.gd"
func run():
	create_timer(120).timeout.connect(func():quit(2))
	scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.set_process(false);scene.library.overlay.hide()
	checks.create=scene.bridge.CreateWorld("Kurak orman testi",42,{"TreeCount":300,"Herbivores":20,"Carnivores":2,"ForestLifespan":0,"ForestDroughtTolerance":0.0,"ForestSeedSpread":1.0,"ForestMaturity":.02})
	scene.bridge.ClimateCycle=false;scene._rebuild_world();scene._update_camera(1)
	scene.bridge.SetDry(true);await advance_days(155)
	var stressed=scene.bridge.GetTreeStates()
	checks.water_limited=scene.bridge.Stats().water_level<.13
	checks.health_falls=stressed.any(func(t):return t.health<.7)
	scene.weather.update_weather(scene.bridge.Stats(),100,true)
	scene.observations.open_panel("resources")
	await capture("forest-drought47")
	await advance_days(90)
	checks.age_immune_still_dies=scene.bridge.GetTreeStates().all(func(t):return int(t.id)>300 or t.dead)
	var dead={}
	for t in scene.bridge.GetTreeStates():if t.dead:dead[int(t.id)]=true
	scene.bridge.SetDry(false);await advance_days(120)
	checks.water_recovers=scene.bridge.Stats().water_level>.59
	checks.no_resurrection=scene.bridge.GetTreeStates().all(func(t):return not dead.has(int(t.id)) or t.dead)
	checks.renewal_after_rain=scene.bridge.GetTreeStates().any(func(t):return int(t.id)>300 and not t.dead)
	scene.weather.update_weather(scene.bridge.Stats(),100,true)
	await capture("forest-recovery47")
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/forest-climate47.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FOREST_CLIMATE ",checks);quit(0 if checks.passed else 1)
