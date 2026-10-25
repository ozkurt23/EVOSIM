extends SceneTree
var checks={}
func _initialize(): call_deferred("run")
func check(name,condition):
	checks[name]=condition
	if not condition: push_error(name)
func run():
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	check("all_settings_have_help",bridge.Settings().all(func(s): return not s.help.is_empty()))
	check("create_custom_world",bridge.CreateWorld("Doğrulama vadisi",81,{"Herbivores":12,"Carnivores":2,"TreeCount":90,"HerbLifespan":0.0}))
	bridge.Paused=false
	for frame in 90: bridge.Advance(1./30)
	bridge.SetDry(true)
	for frame in 30: bridge.Advance(1./30)
	bridge.Paused=true
	var digest=bridge.StateDigest();var head=bridge.CurrentTick
	bridge.ClimateCycle=false
	check("save",bridge.SaveCurrent("Doğrulama vadisi"))
	var slot=bridge.ListWorlds(false)[0]
	check("rename",bridge.RenameWorld(slot.id,"Yeniden adlandırıldı"))
	check("seek_start",bridge.SeekDay(0))
	for i in 100:
		bridge.Advance(0)
		if not bridge.Seeking: break
	check("seek_origin",bridge.CurrentTick==0 and bridge.HeadTick==head)
	check("save_while_preview_preserves_head",bridge.SaveCurrent("Yeniden adlandırıldı"))
	check("branch",bridge.BranchHistory("Ayrı dal"))
	check("branch_is_origin",bridge.CurrentTick==0 and bridge.HeadTick==0)
	check("reopen_original",bridge.OpenWorld(slot.id))
	check("weather_preference_roundtrip",not bridge.ClimateCycle)
	check("full_state_roundtrip",bridge.StateDigest()==digest and bridge.CurrentTick==head)
	check("seek_middle",bridge.SeekDay(head*.025))
	for i in 200:
		bridge.Advance(0)
		if not bridge.Seeking: break
	bridge.Paused=false
	for i in 1000:
		if bridge.CurrentTick>=head: break
		bridge.Advance(.05)
	bridge.Paused=true
	check("historical_replay_exact_future",bridge.CurrentTick==head and bridge.StateDigest()==digest)
	check("trash",bridge.DeleteWorld(slot.id))
	check("deleted_autosave_rejected",not bridge.SaveCurrent("Should not resurrect"))
	check("not_in_library",bridge.ListWorlds(false).all(func(s): return s.id!=slot.id))
	check("restore",bridge.RestoreWorld(slot.id))
	check("restored_full_state",bridge.OpenWorld(slot.id) and bridge.StateDigest()==digest)
	var before=bridge.StateDigest()
	check("invalid_setting_rejected",not bridge.CreateWorld("Bad",3,{"Herbivores":-1}))
	check("invalid_create_preserves_active",before==bridge.StateDigest())
	var export_before=bridge.StateDigest()
	var report_path=bridge.ExportReport()
	check("export_bundle",report_path!="" and FileAccess.file_exists(report_path+"/report.json") and FileAccess.file_exists(report_path+"/population.csv") and FileAccess.file_exists(report_path+"/creatures.csv"))
	check("export_read_only",bridge.StateDigest()==export_before)
	var ok=checks.values().all(func(v): return v)
	FileAccess.open("res://validation/library-checks-19.json",FileAccess.WRITE).store_string(JSON.stringify({"passed":ok,"checks":checks},"  "))
	print("LIBRARY_CHECKS ",checks);quit(0 if ok else 1)
