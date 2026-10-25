extends SceneTree
func _initialize():call_deferred("run")
func wait_trace(bridge,id):
	var result=bridge.ReadLifeTrace(id)
	while not result.missing and not result.complete and result.error=="":
		await process_frame
		result=bridge.ReadLifeTrace(id)
	return result
func run():
	create_timer(50).timeout.connect(func():quit(2))
	var scene=load("res://main.tscn").instantiate();root.add_child(scene);scene.set_process(false)
	scene.library.overlay.hide();scene.bridge.Paused=false
	for i in 80:
		scene.bridge.Advance(.25)
		while scene.bridge.Stats().pending>=.05:scene.bridge.Advance(0)
	scene._update_agents(1)
	scene.bridge.Paused=true;scene._process(0)
	var checks={};var before=scene.bridge.StateDigest();var day=scene.bridge.CurrentTick*.05
	var c=scene.bridge.CreatureDetails(1)
	scene.observations.open_panel("population");scene.observations.open_panel("creature",1)
	var card=scene.observations.panels[-1]
	var button=card.col.get_children().filter(func(n):return n is Button and n.text=="Yaşam izi")[0]
	button.pressed.emit()
	checks.card_opens_trace=scene.observations.panels.any(func(p):return p.kind=="trace")
	var result=await wait_trace(scene.bridge,1)
	checks.replay_completed=result.complete and result.error==""
	checks.exact_endpoint=result.points[-1].day==day
	var last=result.points[-1]
	checks.matches_recorded_vitals=is_equal_approx(last.energy,clampf(c.energy/c.max_energy*100,0,100)) and is_equal_approx(last.water,c.water) and is_equal_approx(last.health,c.health)
	checks.observation_did_not_advance_world=before==scene.bridge.StateDigest()
	scene.observations._refresh()
	await process_frame;await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://previews/life-trace-32.png")
	scene.observations.clear();checks.clear_cancels=scene.bridge.ReadLifeTrace(1).missing
	checks.seek_started=scene.bridge.SeekDay(5)
	while scene.bridge.Seeking:scene.bridge.Advance(0);await process_frame
	var past_before=scene.bridge.StateDigest()
	checks.past_trace_started=scene.bridge.StartLifeTrace(1)
	result=await wait_trace(scene.bridge,1)
	checks.past_trace_has_no_future=result.to_day==5 and result.points[-1].day==5
	checks.past_trace_preserves_view=past_before==scene.bridge.StateDigest() and scene.bridge.HeadTick*.05==day
	scene.bridge.CancelLifeTrace(1)
	checks.cancelled_result_absent=scene.bridge.ReadLifeTrace(1).missing
	scene.bridge.ReturnToPresent();scene.bridge.StartLifeTrace(1)
	scene.bridge.CreateWorld("Başka dünya",7,{"Herbivores":1,"Carnivores":0})
	checks.world_switch_invalidates_trace=scene.bridge.ReadLifeTrace(1).missing
	checks.passed=checks.values().all(func(v):return v)
	FileAccess.open("res://validation/trace-checks-32.json",FileAccess.WRITE).store_string(JSON.stringify(checks,"  "))
	print("TRACE_CHECKS ",checks);quit(0 if checks.passed else 1)
