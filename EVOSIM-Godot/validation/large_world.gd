extends SceneTree
func _initialize(): call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	await process_frame
	if not scene.bridge.CreateWorld("Büyük dünya doğrulaması",42,{"MapSize":256,"Herbivores":450,"Carnivores":50,"TreeCount":300}): quit(1);return
	scene._rebuild_world();scene.library.overlay.hide();scene.bridge.Paused=false;scene.bridge.Speed=20
	scene.elapsed=0;scene.capture_path=ProjectSettings.globalize_path("res://previews/large-world-22.png");scene.capture_at=10
