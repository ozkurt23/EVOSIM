extends RefCounted
## Original, reusable small botanical meshes. No simulation state or RNG access.
static func fern() -> ArrayMesh:
	var st=SurfaceTool.new();st.begin(Mesh.PRIMITIVE_TRIANGLES)
	for frond in 7:
		var angle=frond*2.399
		var axis=Vector3(cos(angle),0,sin(angle))
		var side=Vector3(-sin(angle),0,cos(angle))
		var length=.55+.055*(frond%3)
		for segment in range(1,11):
			var t=segment/11.0
			var p=axis*(length*t)+Vector3.UP*(sin(t*PI*.9)*.40+.03)
			var width=sin(t*PI)*.14
			for sign in [-1,1]:
				var tip=p+side*width*sign+axis*.09
				var mid=(p+tip)*.5+Vector3.UP*.025
				for point in [p,mid-axis*.025,tip,p,tip,mid+axis*.025]:
					st.set_normal((Vector3.UP+side*.1*sign).normalized());st.add_vertex(point)
	var mesh=st.commit()
	var material=ShaderMaterial.new();material.shader=load("res://shaders/foliage.gdshader")
	material.set_shader_parameter("base_color",Color("527b42"))
	material.set_shader_parameter("strength",.045)
	mesh.surface_set_material(0,material)
	return mesh
