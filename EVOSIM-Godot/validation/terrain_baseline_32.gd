extends Node3D
## Mesh/shader construction only. The surface object supplies read-only height and substrate queries.
const EXTENT=48.0
var surface

func build(world_surface):
	surface=world_surface
	_build_terrain()

func _field(p: Vector2) -> float: return surface._field(p)
func _height(p: Vector2) -> float: return surface._height(p)

func _mat(_key: String,color: Color) -> StandardMaterial3D:
	var mat=StandardMaterial3D.new();mat.albedo_color=color;mat.roughness=.9;return mat

func _build_terrain():
	var st = SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	var soil = SurfaceTool.new()
	soil.begin(Mesh.PRIMITIVE_TRIANGLES)
	var step = 0.25
	for z in range(int(EXTENT/step)):
		for x in range(int(EXTENT/step)):
			var a = Vector2(x*step,z*step)
			var b = a+Vector2(0,step)
			var c = a+Vector2(step,step)
			var d = a+Vector2(step,0)
			_land_triangle(st,soil,[a,b,c])
			_land_triangle(st,soil,[a,c,d])
	var terrain = MeshInstance3D.new()
	terrain.mesh = st.commit()
	var material = ShaderMaterial.new()
	material.shader = load("res://shaders/terrain.gdshader")
	material.set_shader_parameter("region_offset",surface.ORIGIN-Vector2(42,20))
	terrain.material_override = material
	add_child(terrain)
	var bank = MeshInstance3D.new()
	bank.mesh = soil.commit()
	bank.material_override = _mat("soil",Color("4d5140"))
	add_child(bank)
	_build_skirt()
	var water = MeshInstance3D.new()
	var plane = PlaneMesh.new()
	plane.size = Vector2(EXTENT,EXTENT)
	plane.subdivide_width = 48
	plane.subdivide_depth = 48
	water.mesh = plane
	water.position = Vector3(EXTENT/2,-0.06,EXTENT/2)
	var wm = ShaderMaterial.new()
	wm.shader = load("res://shaders/water.gdshader")
	wm.set_shader_parameter("region_offset",surface.ORIGIN-Vector2(42,20))
	var im = Image.create(256,256,false,Image.FORMAT_RGB8)
	for y in 256:
		for x in 256:
			var p=Vector2(x,y)*EXTENT/255.0
			var edge=0.0
			for off in [Vector2(1,0),Vector2(-1,0),Vector2(0,1),Vector2(0,-1)]:
				edge=maxf(edge,clampf((_field(p+off*0.75)+1)*0.5,0,1))
			im.set_pixel(x,y,Color(edge,edge,edge))
	wm.set_shader_parameter("shore_mask",ImageTexture.create_from_image(im))
	water.material_override = wm
	add_child(water)

func _land_triangle(st: SurfaceTool, soil: SurfaceTool, input: Array):
	var poly: Array = []
	var crossings: Array = []
	for k in 3:
		var a: Vector2=input[k]
		var b: Vector2=input[(k+1)%3]
		var fa=_field(a)
		var fb=_field(b)
		if fa>=0: poly.append(a)
		if (fa>=0)!=(fb>=0):
			var p=a.lerp(b,fa/(fa-fb))
			poly.append(p)
			crossings.append(p)
	for k in range(1,poly.size()-1):
		for p in [poly[0],poly[k+1],poly[k]]:
			var normal=Vector3(_height(p-Vector2(.03,0))-_height(p+Vector2(.03,0)),.06,_height(p-Vector2(0,.03))-_height(p+Vector2(0,.03))).normalized()
			st.set_normal(normal)
			st.add_vertex(Vector3(p.x,maxf(.09,_height(p)),p.y))
	if crossings.size()==2:
		var a: Vector2=crossings[0]
		var b: Vector2=crossings[1]
		for v in [Vector3(a.x,.09,a.y),Vector3(b.x,.09,b.y),Vector3(b.x,-.07,b.y),Vector3(a.x,.09,a.y),Vector3(b.x,-.07,b.y),Vector3(a.x,-.07,a.y)]:
			soil.set_normal(Vector3.UP)
			soil.add_vertex(v)

func _build_skirt():
	var st=SurfaceTool.new()
	st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for side in 4:
		for i in 96:
			var a=Vector2(i*.5,0)
			var b=Vector2((i+1)*.5,0)
			if side==1: a=Vector2(EXTENT,i*.5);b=Vector2(EXTENT,(i+1)*.5)
			if side==2: a=Vector2(EXTENT-i*.5,EXTENT);b=Vector2(EXTENT-(i+1)*.5,EXTENT)
			if side==3: a=Vector2(0,EXTENT-i*.5);b=Vector2(0,EXTENT-(i+1)*.5)
			var top_a=Vector3(a.x,_height(a)-.01,a.y)
			var top_b=Vector3(b.x,_height(b)-.01,b.y)
			for v in [top_a,top_b,Vector3(b.x,-1.1,b.y),top_a,Vector3(b.x,-1.1,b.y),Vector3(a.x,-1.1,a.y)]:
				st.set_normal(Vector3((b-a).y,0,-(b-a).x).normalized())
				st.add_vertex(v)
	var mesh=MeshInstance3D.new()
	mesh.mesh=st.commit()
	var mat=_mat("cut_earth",Color("4c5743"))
	mat.cull_mode=BaseMaterial3D.CULL_DISABLED
	mesh.material_override=mat
	add_child(mesh)

