extends SceneTree
func _initialize(): call_deferred("run")
func run():
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	var results=[]
	for slot in bridge.ListLegacyWorlds():
		var ok=bridge.ImportLegacyWorld(slot.id)
		results.append({"source_id":slot.id,"passed":ok,"error":bridge.LastError})
	FileAccess.open("res://validation/import-checks-23.json",FileAccess.WRITE).store_string(JSON.stringify(results,"  "))
	print("IMPORT ",results);quit(0 if results.all(func(r): return r.passed) else 1)
