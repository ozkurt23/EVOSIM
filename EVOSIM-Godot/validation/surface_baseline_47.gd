extends RefCounted
## Immutable visual interpretation of Core terrain; no collision or simulation mutations.
var map: Dictionary
var ORIGIN: Vector2
var terrain_noise=FastNoiseLite.new()

func _init(state: Dictionary,origin: Vector2):
	map=state;ORIGIN=origin
	terrain_noise.seed=42;terrain_noise.frequency=.09

func _tile(x: int, z: int) -> int:
	var n: int = map["size"]
	return map.tiles[clampi(x,0,n-1)+clampi(z,0,n-1)*n]

func _field(p: Vector2) -> float:
	var center=_raw_field(p)
	if absf(center)>.99: return center
	var total=center*4.0
	for d in [Vector2(.4,0),Vector2(-.4,0),Vector2(0,.4),Vector2(0,-.4)]: total+=_raw_field(p+d)
	return total/8.0

func _raw_field(p: Vector2) -> float:
	var q = p + ORIGIN - Vector2(0.5,0.5)
	var x = floori(q.x)
	var z = floori(q.y)
	var u = q.x-x
	var v = q.y-z
	var a = -1.0 if _tile(x,z)==1 else 1.0
	var b = -1.0 if _tile(x+1,z)==1 else 1.0
	var c = -1.0 if _tile(x,z+1)==1 else 1.0
	var d = -1.0 if _tile(x+1,z+1)==1 else 1.0
	return lerpf(lerpf(a,b,u),lerpf(c,d,u),v)

func _height(p: Vector2) -> float:
	var f = _field(p)
	if f < 0: return -0.055
	var hill = 0.30 + (terrain_noise.get_noise_2d(p.x+ORIGIN.x-42,p.y+ORIGIN.y-20)+1.0)*0.28
	return 0.09 + smoothstep(0.0,1.0,f)*hill

