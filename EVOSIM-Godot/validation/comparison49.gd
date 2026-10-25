extends SceneTree
func _initialize():call_deferred("run")
func run():
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);await process_frame
	var b=scene.bridge;var checks={}
	checks.first=b.CreateWorld("İlk vadi",42,{"Herbivores":10,"Carnivores":2}) and b.SaveCurrent("İlk vadi")
	var first=b.ListWorlds(false)[0].id
	checks.second=b.CreateWorld("İkinci vadi",42,{"Herbivores":20,"Carnivores":2,"ForestLifespan":30}) and b.SaveCurrent("İkinci vadi")
	var second=b.ListWorlds(false).filter(func(s):return s.id!=first)[0].id
	var digest=b.StateDigest();var data=b.CompareSavedWorlds(first,second)
	checks.read_only=b.StateDigest()==digest
	checks.same_seed=data.same_seed
	checks.count_difference=data.rows.filter(func(r):return r.label=="Otçul")[0].a==10 and data.rows.filter(func(r):return r.label=="Otçul")[0].b==20
	checks.forest_difference=data.differences.any(func(s):return "Ağaç ömrü" in s)
	scene.library.overlay.show();scene.library._compare();await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/comparison49.png")
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/comparison49.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "));print("COMPARISON49 ",checks);quit(0 if checks.passed else 1)
