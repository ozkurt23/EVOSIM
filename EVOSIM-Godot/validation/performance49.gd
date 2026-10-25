extends SceneTree
func _initialize():call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);await process_frame
	if not scene.bridge.CreateWorld("Akıcılık denemesi",42,{"MapSize":128,"Herbivores":450,"Carnivores":50,"TreeCount":300}):quit(1);return
	scene._rebuild_world();scene.library.overlay.hide();scene.bridge.Paused=false;scene.bridge.Speed=20
	if "--balanced" in OS.get_cmdline_user_args():scene._toggle_quality()
	scene.elapsed=0;scene.capture_path=ProjectSettings.globalize_path("res://previews/performance49.png");scene.capture_at=12
