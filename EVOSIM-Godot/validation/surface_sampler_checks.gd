extends SceneTree
func _initialize():call_deferred("run")
func run():
	var bridge=load("res://scripts/SimulationBridge.cs").new();root.add_child(bridge);bridge.Initialize(42)
	var digest=bridge.StateDigest();var samples=[];var max_error=0.0;var max_pixel_error=0
	for origin in [Vector2(42,20),Vector2.ZERO,Vector2(60,60)]:
		var old=load("res://validation/surface_baseline_47.gd").new(bridge.GetMap(),origin)
		var fast=load("res://scripts/world_surface.gd").new(bridge.GetMap(),origin)
		var start=Time.get_ticks_usec();var expected=PackedFloat64Array()
		for y in 160:
			for x in 160:expected.append(old._height(Vector2(x,y)*.301))
		var old_ms=(Time.get_ticks_usec()-start)/1000.0;start=Time.get_ticks_usec()
		for y in 160:
			for x in 160:max_error=maxf(max_error,absf(expected[x+y*160]-fast._height(Vector2(x,y)*.301)))
		var fast_ms=(Time.get_ticks_usec()-start)/1000.0
		var im=fast.shore_image();var pixel=Image.create(1,1,false,Image.FORMAT_RGB8)
		for y in range(0,256,3):
			for x in range(0,256,3):
				var p=Vector2(x,y)*48.0/255.0;var edge=0.0
				for off in [Vector2.RIGHT,Vector2.LEFT,Vector2.UP,Vector2.DOWN]:edge=maxf(edge,clampf((old._field(p+off*.75)+1)*.5,0,1))
				pixel.set_pixel(0,0,Color(edge,edge,edge));max_pixel_error=maxi(max_pixel_error,absi(pixel.get_data()[0]-im.get_data()[(x+y*256)*3]))
		samples.append({"origin":str(origin),"script_ms":old_ms,"compiled_ms":fast_ms})
	var result={"samples":samples,"height_max_error":max_error,"shore_max_byte_error":max_pixel_error,"biology_unchanged":digest==bridge.StateDigest()}
	result.passed=max_error<.000001 and max_pixel_error<=1 and result.biology_unchanged
	FileAccess.open("res://validation/surface-sampler-48.json",FileAccess.WRITE).store_string(JSON.stringify(result,"  "));print("SURFACE_SAMPLER ",result);quit(0 if result.passed else 1)
