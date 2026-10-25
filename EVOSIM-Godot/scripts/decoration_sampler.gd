extends RefCounted
## Independent world-cell streams: moving the rendering window cannot reshuffle plants.
static func sample(origin: Vector2,extent: float,density: float,salt: int) -> PackedVector4Array:
	var result=PackedVector4Array()
	var random=RandomNumberGenerator.new()
	for z in range(floori(origin.y+1),ceili(origin.y+extent-1)):
		for x in range(floori(origin.x+1),ceili(origin.x+extent-1)):
			random.seed=(x*73856093)^(z*19349663)^(salt*83492791)
			var count=floori(density)+(1 if random.randf()<fposmod(density,1.0) else 0)
			for i in count:
				var p=Vector2(x+random.randf(),z+random.randf())
				var angle=random.randf()*TAU;var weight=random.randf()
				if p.x<=origin.x+1 or p.y<=origin.y+1 or p.x>=origin.x+extent-1 or p.y>=origin.y+extent-1: continue
				result.append(Vector4(p.x,p.y,angle,weight))
	return result
