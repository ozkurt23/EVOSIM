extends SceneTree
func _initialize(): call_deferred("run")
func run():
	create_timer(40).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene)
	await process_frame
	var checks=scene.bridge.ValidateResearchViews()
	var before=scene.bridge.StateDigest()
	scene.library.overlay.hide();scene.bridge.Paused=true
	for kind in ["population","genes","resources"]: scene.observations.open_panel(kind)
	scene.observations.open_panel("creature",1)
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/research-31.png")
	checks.four_panels_coexist=scene.observations.panels.size()==4
	checks.panel_bounds=scene.observations.panels.all(func(p):return p.node.get_global_rect().end.y<=root.get_visible_rect().size.y)
	var gene=scene.observations.panels.filter(func(p):return p.kind=="genes")[0]
	gene.selector.select(3);scene.observations._refresh()
	checks.all_six_genes_selectable=gene.selector.item_count==6 and gene.graph.herb.size()==10
	checks.ui_preserves_state=before==scene.bridge.StateDigest()
	checks.create_treeless=scene.bridge.CreateWorld("Ağaçsız araştırma",42,{"Herbivores":1,"Carnivores":0,"TreeCount":0})
	checks.new_world_invalidates_resources=scene.bridge.ResourceInventory().trees==0
	checks.extinction_scenario=scene.bridge.ConfigureScenario("empty")
	scene.observations._refresh()
	checks.empty_has_dash=gene.text.text.contains("—") and Array(gene.graph.herb).all(func(n):return n==0)
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/research-checks-31.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("RESEARCH_CHECKS ",checks);quit(0 if checks.passed else 1)
