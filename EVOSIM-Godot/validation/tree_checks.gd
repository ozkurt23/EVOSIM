extends SceneTree
func _initialize():call_deferred("run")
func run():
	create_timer(50).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.library.overlay.hide();scene.bridge.Paused=true
	var checks=scene.bridge.ValidateTreeViews();var before=scene.bridge.StateDigest()
	scene.observations.open_panel("population")
	var chosen=-1
	for tree in scene.map.trees:
		var p=Vector2(tree.x,tree.z)-scene.ORIGIN
		if not scene._inside(p,1.5):continue
		var screen=scene.camera.unproject_position(Vector3(p.x,scene._height(p)+1,p.y))
		if screen.x<320 or screen.x>950 or screen.y<200 or screen.y>550:continue
		scene._pick_agent(screen)
		if scene.selected_id>=0:continue
		var event=InputEventMouseButton.new();event.pressed=true;event.button_index=MOUSE_BUTTON_LEFT;event.position=screen
		scene._unhandled_input(event);chosen=int(tree.id);break
	checks.actual_click_opens_tree=chosen>=0 and scene.observations.panels.any(func(p):return p.kind=="tree")
	checks.population_retained=scene.observations.panels.any(func(p):return p.kind=="population")
	scene._process(0);checks.tree_selection_visible=scene.selection_ring.visible and scene.selected_tree_id>=0
	var selected_tree=scene.selected_tree_id
	var count=scene.observations.panels.size();scene._pick_tree(Vector2(-1000,-1000))
	checks.empty_click_no_panel=scene.observations.panels.size()==count
	scene.selected_tree_id=selected_tree
	checks.biology_preserved=before==scene.bridge.StateDigest()
	for i in 30:await process_frame;await RenderingServer.frame_post_draw
	checks.panels_inside=scene.observations.panels.all(func(p):return p.node.get_global_rect().end.y<=root.get_visible_rect().size.y)
	root.get_texture().get_image().save_png("res://previews/tree-inspector-39.png")
	var agent_id=scene.agents.keys()[0]
	var agent_click=InputEventMouseButton.new();agent_click.pressed=true;agent_click.button_index=MOUSE_BUTTON_LEFT
	agent_click.position=scene.camera.unproject_position(scene.agents[agent_id].position+Vector3.UP*.5)
	scene._unhandled_input(agent_click)
	checks.creature_pick_has_priority=scene.selected_id>=0 and scene.selected_tree_id==-1
	checks.new_world=scene.bridge.CreateWorld("Ağaçsız",42,{"Herbivores":1,"Carnivores":0,"TreeCount":0})
	checks.stale_tree_missing=scene.bridge.TreeDetails(chosen).is_empty()
	scene._rebuild_world();checks.world_switch_clears_panels=scene.observations.panels.is_empty()
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/tree-checks-39.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("TREE_CHECKS ",checks);quit(0 if checks.passed else 1)
