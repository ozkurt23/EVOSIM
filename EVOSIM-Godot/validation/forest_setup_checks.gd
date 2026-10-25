extends SceneTree
func _initialize():call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);scene.bridge.Paused=true
	scene.library._setup();scene.library.overlay.show()
	var tabs=scene.library.column.get_children().filter(func(n):return n is TabContainer)[0]
	for i in tabs.get_tab_count():
		if tabs.get_tab_title(i)=="Yaşayan orman":tabs.current_tab=i
	var checks={"forest_tab":tabs.get_tab_title(tabs.current_tab)=="Yaşayan orman","normalized_slider":scene.library.editors.ForestDensity is HSlider,"years_numeric":scene.library.editors.ForestLifespan is SpinBox,"help_present":not scene.library.editors.ForestCorpseContribution.tooltip_text.is_empty()}
	for i in 15:await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/forest-settings47.png")
	scene.library._preset(4)
	checks.preset_applied=scene.library.values.ForestLifespan==2 and scene.library.values.ForestSeedSpread==.8
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/forest-settings47.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("FOREST_SETTINGS ",checks);quit(0 if checks.passed else 1)
