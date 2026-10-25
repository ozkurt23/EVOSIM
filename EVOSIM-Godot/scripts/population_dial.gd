extends Control
## Original population instrument. Layout evidence: Terra Nil R12–R15.
var herbivores=0
var carnivores=0
func _ready():
	custom_minimum_size=Vector2(168,168)
	mouse_filter=Control.MOUSE_FILTER_STOP
	tooltip_text="Yaşayan nüfus · yeşil otçullar, bakır etçiller\nHalka, iki grubun nüfus içindeki payını gösterir."
func update_counts(herb: int,carn: int):
	if herbivores==herb and carnivores==carn:return
	herbivores=herb;carnivores=carn;queue_redraw()
func _draw():
	var center=Vector2(84,84)
	draw_circle(center,82,Color("19333e"))
	draw_arc(center,82,0,TAU,128,Color("63818a"),2,true)
	draw_arc(center,69,0,TAU,128,Color("557985"),2,true)
	var fraction=float(herbivores)/maxi(1,herbivores+carnivores)
	draw_arc(center,76,-PI/2,-PI/2+TAU*fraction,96,Color("82af46"),8,true)
	if carnivores>0: draw_arc(center,76,-PI/2+TAU*fraction,3*PI/2,64,Color("bd7847"),8,true)
	draw_line(Vector2(18,84),Vector2(150,84),Color("557985"),2,true)
