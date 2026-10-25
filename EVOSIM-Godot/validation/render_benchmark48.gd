extends SceneTree
func _initialize():call_deferred("run")
func run():
	var old="--baseline" in OS.get_cmdline_user_args()
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);await process_frame;scene.set_process(false);scene.profile_stages=true
	if not scene.bridge.CreateWorld("Render ölçümü",42,{"MapSize":128,"Herbivores":450,"Carnivores":50,"TreeCount":300}):quit(1);return
	scene._rebuild_world();scene.library.overlay.hide();scene.bridge.Paused=false;scene.bridge.Speed=20
	if old:scene.surface=load("res://validation/surface_baseline_47.gd").new(scene.map,scene.ORIGIN)
	var stages={"advance":[],"agents":[],"rest":[]};var frames=[];var scripts=[];var last=Time.get_ticks_usec()
	for i in 600:
		var start=Time.get_ticks_usec();scene._process(1./60);var cost=(Time.get_ticks_usec()-start)/1000.0
		await process_frame
		var now=Time.get_ticks_usec()
		if i>=120:frames.append((now-last)/1000.0);scripts.append(cost)
		if i>=120:
			for key in stages:stages[key].append(scene.stage_costs[key])
		last=now
	frames.sort();scripts.sort()
	for key in stages:stages[key].sort();stages[key]=stages[key][stages[key].size()/2]
	var report={"stage_medians":stages,"baseline":old,"frames":frames.size(),"median_frame_ms":frames[frames.size()/2],"p95_frame_ms":frames[int(frames.size()*.95)],"median_script_ms":scripts[scripts.size()/2],"tick":scene.bridge.CurrentTick,"hash":scene.bridge.StateDigest(),"agents":scene.agents.size(),"stats":scene.bridge.Stats()}
	var path="res://validation/render-baseline48.json" if old else "res://validation/render-fast48.json"
	FileAccess.open(path,FileAccess.WRITE).store_string(JSON.stringify(report,"  "));print("RENDER_BENCH ",report);quit()
