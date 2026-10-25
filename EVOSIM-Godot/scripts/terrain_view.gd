extends Node3D
## Mesh/shader construction only. The surface object supplies read-only height and substrate queries.
const EXTENT=48.0
var surface
var vertex_cache={}
var field_cache={}
const REGION_CACHE_LIMIT=3
static var region_cache={}
static var region_order=[]
static var cached_topology=""
var reused_region=false

func build(world_surface):
	surface=world_surface
	var digest=HashingContext.new();digest.start(HashingContext.HASH_SHA256)
	# Geometry depends on water/land, not on a tree growing or dying on land.
	var water_mask=PackedByteArray();water_mask.resize(surface.map.tiles.size())
	for i in water_mask.size():water_mask[i]=1 if surface.map.tiles[i]==1 else 0
	digest.update(var_to_bytes([surface.map["size"],water_mask]))
	var topology=digest.finish().hex_encode()
	if topology!=cached_topology:
		region_cache.clear();region_order.clear();cached_topology=topology
	var origin=surface.ORIGIN
	if region_cache.has(origin):
		# Only immutable visual resources are shared; nodes and biology are never cached.
		for entry in region_cache[origin]:
			var view=MeshInstance3D.new();view.mesh=entry.mesh
			view.material_override=entry.material;view.transform=entry.transform;add_child(view)
		region_order.erase(origin);region_order.append(origin);reused_region=true
		return
	_build_terrain()
	var entries=[]
	for child in get_children():
		entries.append({"mesh":child.mesh,"material":child.material_override,"transform":child.transform})
	region_cache[origin]=entries;region_order.append(origin)
	while region_order.size()>REGION_CACHE_LIMIT:region_cache.erase(region_order.pop_front())

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
	vertex_cache.clear();field_cache.clear()
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
	var im = surface.shore_image()
	wm.set_shader_parameter("shore_mask",ImageTexture.create_from_image(im))
	water.material_override = wm
	add_child(water)

func _land_triangle(st: SurfaceTool, soil: SurfaceTool, input: Array):
	var poly: Array = []
	var crossings: Array = []
	for k in 3:
		var a: Vector2=input[k]
		var b: Vector2=input[(k+1)%3]
		var fa=_vertex_field(a)
		var fb=_vertex_field(b)
		if fa>=0: poly.append(a)
		if (fa>=0)!=(fb>=0):
			var p=a.lerp(b,fa/(fa-fb))
			poly.append(p)
			crossings.append(p)
	for k in range(1,poly.size()-1):
		for p in [poly[0],poly[k+1],poly[k]]:
			var vertex=_land_vertex(p)
			st.set_normal(vertex[1])
			st.add_vertex(vertex[0])
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


func _vertex_field(p: Vector2) -> float:
	if not field_cache.has(p): field_cache[p]=_field(p)
	return field_cache[p]

func _land_vertex(p: Vector2) -> Array:
	if not vertex_cache.has(p):
		var normal=Vector3(_height(p-Vector2(.03,0))-_height(p+Vector2(.03,0)),.06,_height(p-Vector2(0,.03))-_height(p+Vector2(0,.03))).normalized()
		vertex_cache[p]=[Vector3(p.x,maxf(.09,_height(p)),p.y),normal]
	return vertex_cache[p]
