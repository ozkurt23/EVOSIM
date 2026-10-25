extends SceneTree
func _initialize():call_deferred("run")
func run():
	create_timer(50).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.library.overlay.hide();scene.bridge.Paused=true
	var checks=scene.bridge.ValidateFamilyViews();var before=scene.bridge.StateDigest()
	scene.observations.open_panel("population");scene.observations.open_panel("creature",1)
	var card=scene.observations.panels[-1]
	card.col.get_children().filter(func(n):return n is Button and n.text=="Soy bağı")[0].pressed.emit()
	checks.card_opens_family=scene.observations.panels.any(func(p):return p.kind=="family" and p.id==1)
	scene.observations._open_relative(2)
	checks.relative_opens_card=scene.observations.panels.any(func(p):return p.kind=="creature" and p.id==2)
	checks.population_stays_open=scene.observations.panels.any(func(p):return p.kind=="population")
	for frame in 30:await process_frame;await RenderingServer.frame_post_draw
	checks.panels_inside=scene.observations.panels.all(func(p):return p.node.get_global_rect().end.y<=root.get_visible_rect().size.y)
	checks.ui_preserves_biology=before==scene.bridge.StateDigest()
	root.get_texture().get_image().save_png("res://previews/family-37.png")
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/family-checks-37.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FAMILY_CHECKS ",checks);quit(0 if checks.passed else 1)
