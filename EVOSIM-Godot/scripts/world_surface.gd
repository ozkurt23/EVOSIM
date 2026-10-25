extends RefCounted
## Immutable visual interpretation of Core terrain; no collision or simulation mutations.
var map: Dictionary
var ORIGIN: Vector2
var sampler=load("res://scripts/SurfaceSampler.cs").new()

func _init(state: Dictionary,origin: Vector2):
	map=state;ORIGIN=origin
	sampler.Configure(map["size"],map.tiles,ORIGIN)

func _tile(x: int,z: int) -> int: return sampler.Tile(x,z)
func _field(p: Vector2) -> float: return sampler.Field(p)
func _raw_field(p: Vector2) -> float: return sampler.RawField(p)
func _height(p: Vector2) -> float: return sampler.Height(p)
func shore_image() -> Image: return sampler.ShoreImage()
