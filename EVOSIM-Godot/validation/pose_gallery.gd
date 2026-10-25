extends SceneTree
## Explicit synthetic visual fixtures, NOT claims of learned behavior.
var world: Node3D
var views=[]
var mesh_cache={}
var ticks=0
var checks={}
var target="res://previews/pose-gallery-49"
func _initialize():
	call_deferred("start")
func find_mesh(node):
	if node is MeshInstance3D: return node.mesh
	for child in node.get_children():
		var mesh=find_mesh(child)
		if mesh: return mesh
	return null
func mesh_for(name: String) -> Mesh:
	if mesh_cache.has(name): return mesh_cache[name]
	var scene=load("res://assets/"+name+".glb").instantiate()
	var mesh=find_mesh(scene).duplicate();scene.free();mesh_cache[name]=mesh;return mesh
func start():
	world=Node3D.new();root.add_child(world)
	var env=WorldEnvironment.new();env.environment=Environment.new();env.environment.background_mode=Environment.BG_COLOR;env.environment.background_color=Color("19333e")
	env.environment.ambient_light_source=Environment.AMBIENT_SOURCE_COLOR;env.environment.ambient_light_color=Color("bac9c9");env.environment.ambient_light_energy=.7
	world.add_child(env)
	var sun=DirectionalLight3D.new();sun.rotation_degrees=Vector3(-55,-25,0);sun.light_energy=1.3;sun.shadow_enabled=true;world.add_child(sun)
	var cam=Camera3D.new();cam.projection=Camera3D.PROJECTION_ORTHOGONAL;cam.size=12.6;cam.position=Vector3(2,12,13);world.add_child(cam);cam.look_at(Vector3(0,0,0));cam.current=true
	var ground=MeshInstance3D.new();var plane=PlaneMesh.new();plane.size=Vector2(22,15);ground.mesh=plane
	var mat=StandardMaterial3D.new();mat.albedo_color=Color("708261");mat.roughness=.96;ground.material_override=mat;world.add_child(ground)
	var fixtures=[
		["Otçul · yürüyüş","move",false,0.0,1.0], ["Avcı · yürüyüş","move",true,0.0,1.0], ["Isırma","bite",true,0.0,1.0],
		["Ot yeme","eat",false,0.0,1.0], ["Su içme","drink",false,0.0,1.0], ["Meyve","fruit",false,0.0,1.0],
		["Yavru","rest",false,0.0,.55], ["Yaşlı","rest",true,1.0,1.0], ["Ceset","dead",false,1.0,1.0]]
	for i in fixtures.size():
		var f=fixtures[i];var view=load("res://scripts/creature_view.gd").new();view.build(mesh_for,f[2]);world.add_child(view)
		view.position=Vector3((i%3-1)*5,0,(i/3-1)*3.6);view.rotation.y=-.45;view.scale=Vector3.ONE*f[4]
		if f[1]=="dead": view.fall(.5)
		var data={"synthetic":true,"distance":0.,"radius":.35,"speed":1.0 if f[1]=="move" else 0.0,"action":f[1],"aging":f[3],"damage":0.}
		views.append([view,data])
		var caption=Label3D.new();caption.text=f[0];caption.font_size=38;caption.pixel_size=.008;caption.position=view.position+Vector3(0,.4,1.4);caption.rotation_degrees.x=-60;caption.modulate=Color("fff4d5");world.add_child(caption)
	var layer=CanvasLayer.new();root.add_child(layer);var title=Label.new();title.text="EVOSIM · Animasyon / siluet testleri — sentetik durum örnekleri";title.position=Vector2(24,18);title.add_theme_font_size_override("font_size",22);layer.add_child(title)
	await process_frame
	for frame in 24:
		for entry in views:
			entry[1].distance+=.06 if entry[1].action=="move" else 0.
			entry[0].pose(entry[1],frame,.08)
		await create_timer(.08).timeout
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png(target+"-%02d.png"%frame)
	var walker=views[0][0];var before=walker.legs[0].rotation.z
	views[0][1].paused=true;walker.pose(views[0][1],23,.5)
	checks["pause_freezes_joints"]=is_equal_approx(before,walker.legs[0].rotation.z)
	checks["walk_moves_legs"]=absf(walker.legs[0].rotation.z)>.01
	checks["articulated_knees"]=walker.lower_legs.size()==4 and walker.feet.size()==4
	checks["feeding_lowers_head"]=views[3][0].head.rotation.z<-.5 and views[4][0].head.rotation.z<-.5
	checks["fruit_head_up"]=views[5][0].head.rotation.z>0
	checks["juvenile_scale"]=views[6][0].scale.x<views[0][0].scale.x
	checks["old_desaturates"]=views[7][0].surfaces[0][0].albedo_color.s<views[7][0].surfaces[0][1].s
	checks["corpse_side_pose"]=views[8][0].rotation.x>1
	var ok=true
	for v in checks.values(): ok=ok and v
	FileAccess.open(target+".json",FileAccess.WRITE).store_string(JSON.stringify({"passed":ok,"synthetic_fixtures":true,"checks":checks},"  "))
	print("POSE_GALLERY ",checks);quit(0 if ok else 1)
