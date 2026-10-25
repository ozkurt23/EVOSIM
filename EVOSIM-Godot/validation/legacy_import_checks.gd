extends SceneTree
func _initialize():call_deferred("run")
func run():
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	var source=""
	for arg in OS.get_cmdline_user_args():
		if arg.begins_with("--source="):source=arg.trim_prefix("--source=")
	var before=FileAccess.get_sha256(source);var checks={}
	checks.started=bridge.BeginLegacyImport(source)
	checks.concurrent_rejected=not bridge.BeginLegacyImport(source)
	var result=0;var frames=0
	while result==0 and frames<600:
		await create_timer(.02).timeout;result=bridge.PollLegacyImport();frames+=1
	checks.completed=result==1;checks.source_untouched=before==FileAccess.get_sha256(source)
	checks.new_library_entry=bridge.ListWorlds(false).size()==1
	checks.paused=bridge.Paused;checks.no_old_history=bridge.EarliestTick==bridge.HeadTick
	if result==1:
		var digest=bridge.StateDigest();var slot=bridge.ListWorlds(false)[0]
		checks.reopen=bridge.OpenWorld(slot.id) and bridge.StateDigest()==digest
	checks.bad_file_rejected=not bridge.BeginLegacyImport(source+".missing")
	var report={"checks":checks,"error":bridge.LastError,"passed":checks.values().all(func(v):return v)}
	FileAccess.open("res://validation/legacy-import-48.json" if OS.has_feature("editor") else "user://legacy-import-48.json",FileAccess.WRITE).store_string(JSON.stringify(report,"  "));print("LEGACY_IMPORT ",report);quit(0 if report.passed else 1)
