extends SceneTree
var scene
func _initialize():call_deferred("run")
func run():
	scene=load("res://main.tscn").instantiate();root.add_child(scene)
	scene.library.overlay.hide();scene.bridge.Paused=false;scene.bridge.Speed=2
	var layer=CanvasLayer.new();root.add_child(layer)
	var title=Label.new();title.position=Vector2(360,900);title.add_theme_font_size_override("font_size",44);title.add_theme_color_override("font_color",Color("f2eedb"));title.add_theme_constant_override("outline_size",6);title.add_theme_color_override("font_outline_color",Color("17343f"));layer.add_child(title)
	var sub=Label.new();sub.position=Vector2(363,957);sub.add_theme_font_size_override("font_size",22);sub.add_theme_color_override("font_color",Color("f2eedb"));sub.add_theme_constant_override("outline_size",4);sub.add_theme_color_override("font_outline_color",Color("17343f"));layer.add_child(sub)
	for frame in 360:
		if frame==0:
			title.text="EVOSIM";sub.text="Yaşayan bir dünya. Her nesilde yeni bir hikâye.";scene.zoom=32
		if frame<120:scene.zoom=32-float(frame)/120*4
		if frame==120:
			var best=-1;var score=-1.
			for id in scene.agent_data:
				var a=scene.agent_data[id];var value=float(a.speed)+(5 if a.predator else 0)
				if value>score:score=value;best=id
			if scene.agent_data.has(20):best=20
			scene.selected_id=best;scene.follow_capture=true;scene.zoom=10
			title.text="NESİLLER BOYU DEĞİŞİM";sub.text="Kalıtım · mutasyon · yaşam mücadelesi"
			scene._update_camera(10)
		if frame==240:
			scene.follow_capture=false;scene.zoom=30;scene.focus=Vector3(24,0,24);scene.selected_id=-1;scene._update_camera(10)
			scene.observations.open_panel("population");scene.observations.open_panel("genes")
			title.text="GÖZLEMLE. KARŞILAŞTIR. KEŞFET.";sub.text="Dünyanın koşullarını seç. Evrimin izini sür."
		await process_frame
	quit()
