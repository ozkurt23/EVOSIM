extends Control
var bridge
var world
var texture: ImageTexture
var expanded=false
var map_size=128
var markers=PackedVector3Array()
var marker_elapsed=0.0
func build(adapter,scene,hud):
	bridge=adapter;world=scene
	set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT)
	offset_left=110;offset_right=146;offset_top=-54;offset_bottom=-18;tooltip_text="Dünya haritası · aç / bir bölgeye git"
	mouse_filter=Control.MOUSE_FILTER_STOP;rebuild()
func rebuild():
	var data=bridge.GetMap();map_size=data["size"]
	var image=Image.create(map_size,map_size,false,Image.FORMAT_RGB8)
	for y in map_size:
		for x in map_size:
			var tile=data.tiles[y*map_size+x]
			image.set_pixel(x,y,Color("427b91") if tile==1 else (Color("646854") if tile==2 else Color("70935a")))
	texture=ImageTexture.create_from_image(image);queue_redraw()
func _draw():
	if texture: draw_texture_rect(texture,Rect2(Vector2.ZERO,size),false)
	draw_rect(Rect2(Vector2.ZERO,size),Color("d8dcc7"),false,2)
	if expanded:
		var factor=size.x/map_size
		draw_rect(Rect2(world.ORIGIN*factor,Vector2.ONE*world.EXTENT*factor),Color("fff2ce"),false,2)
		for c in markers: draw_circle(Vector2(c.x,c.y)*factor,1.8,Color("de9a75") if c.z>.5 else Color("ecedb4"))
func _gui_input(event):
	if event is InputEventMouseButton and event.pressed:
		accept_event()
		if event.button_index==MOUSE_BUTTON_RIGHT or not expanded:
			expanded=not expanded
			if expanded: markers=bridge.GetMapMarkers();marker_elapsed=0
			var side=180 if expanded else 36;offset_right=110+side;offset_top=-side-18;offset_bottom=-18;queue_redraw()
		elif event.button_index==MOUSE_BUTTON_LEFT:
			world.navigate_world(event.position/size*map_size);queue_redraw()
func _process(delta):
	if not expanded:return
	marker_elapsed+=delta
	if marker_elapsed>=.2:
		marker_elapsed=0;markers=bridge.GetMapMarkers()
	queue_redraw()
