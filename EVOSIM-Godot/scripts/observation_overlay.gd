extends Control
var observer
func _ready():mouse_filter=Control.MOUSE_FILTER_IGNORE
func _process(_delta):
	if observer and (observer.map_mode>0 or observer.tracked_ancestor>=0 or observer.panels.any(func(p):return p.kind=="region")):queue_redraw()
func _draw():
	if not observer:return
	var world=observer.world_view
	if not world or not world.camera:return
	var peak=1.0
	for cell in observer.map_samples:peak=maxf(peak,cell.value)
	for cell in observer.map_samples:
		var point=Vector2(cell.x,cell.z)-world.ORIGIN
		if not world._inside(point,0):continue
		var p=world.camera.unproject_position(Vector3(point.x,world._height(point)+.1,point.y))
		var color=[Color.TRANSPARENT,Color("a1cf65"),Color("e7b28c"),Color("c58370")][observer.map_mode]
		color.a=.12+.25*cell.value/peak
		draw_circle(p,clampf(180/world.camera.size,3,15)+15*sqrt(cell.value/peak),color)
	if observer.map_mode>0:
		draw_string(ThemeDB.fallback_font,Vector2(340,30),["","Yaşam yoğunluğu","Besin enerjisi · erişim hesabı değildir","Son 30 günde ölümler"][observer.map_mode],HORIZONTAL_ALIGNMENT_LEFT,-1,14,Color("f3f2df"))
	for id in observer.lineage_members:
		if not world.agents.has(id):continue
		var node=world.agents[id]
		if not node.visible:continue
		var p=world.camera.unproject_position(node.global_position+Vector3.UP*.4)
		draw_arc(p,10,0,TAU,24,Color("e7b28c"),2,true)
	for panel in observer.panels:
		if panel.kind!="region":continue
		var center=panel.point-world.ORIGIN;var points=PackedVector2Array()
		for i in 65:
			var p=center+Vector2.from_angle(i*TAU/64)*8
			points.append(world.camera.unproject_position(Vector3(p.x,world._height(p)+.08,p.y)))
		draw_polyline(points,Color("e7b28c99"),1.5,true)
