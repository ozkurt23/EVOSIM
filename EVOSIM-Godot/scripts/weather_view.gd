extends Node3D
## Climate presentation follows Core water availability and explicit climate inputs.
var dry=0.0
var rain=0.0
var sun: DirectionalLight3D
var environment: Environment
var drops: MultiMeshInstance3D
func build(light: DirectionalLight3D,env: Environment):
	sun=light;environment=env
	var mesh=QuadMesh.new();mesh.size=Vector2(.018,.30)
	var material=ShaderMaterial.new();material.shader=load("res://shaders/rain.gdshader");mesh.material=material
	var mm=MultiMesh.new();mm.transform_format=MultiMesh.TRANSFORM_3D;mm.use_custom_data=true;mm.mesh=mesh;mm.instance_count=1200
	mm.custom_aabb=AABB(Vector3(-2,-1,-2),Vector3(54,12,54))
	var random=RandomNumberGenerator.new();random.seed=942
	for i in mm.instance_count:
		mm.set_instance_transform(i,Transform3D(Basis.IDENTITY,Vector3(random.randf()*48,.65,random.randf()*48)))
		mm.set_instance_custom_data(i,Color(random.randf(),random.randf(),0,0))
	drops=MultiMeshInstance3D.new();drops.multimesh=mm;drops.cast_shadow=GeometryInstance3D.SHADOW_CASTING_SETTING_OFF;add_child(drops)
func update_weather(stats: Dictionary,delta: float,high_quality: bool):
	dry=lerpf(dry,clampf((1-float(stats.water_level))/.88,0,1),1-exp(-delta*.8))
	rain=lerpf(rain,1.0 if stats.rain else 0.0,1-exp(-delta*1.2))
	RenderingServer.global_shader_parameter_set("evosim_dry",dry)
	RenderingServer.global_shader_parameter_set("evosim_rain",rain)
	drops.visible=rain>.01;drops.multimesh.visible_instance_count=1200 if high_quality else 500
	sun.light_energy=lerpf(1.35,1.0,rain)
	sun.light_color=Color("fff0d6").lerp(Color("d8e4eb"),rain*.65)
	environment.fog_density=lerpf(.0007,.0010,rain)
func status() -> Dictionary:
	return {"dryness":dry,"rain":rain,"rain_instances":drops.multimesh.visible_instance_count if drops.visible else 0}
