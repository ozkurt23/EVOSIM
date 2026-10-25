extends Control
var samples=[]
func _draw():
	var peak=0
	for s in samples:peak=maxi(peak,s.offspring)
	draw_line(Vector2(12,size.y-18),Vector2(size.x,size.y-18),Color("557985"))
	for s in samples:
		var p=Vector2(12+s.gene*(size.x-20),size.y-22-float(s.offspring)/maxi(1,peak)*(size.y-45))
		draw_circle(p,2.5,Color("e7b28ca0") if s.predator else Color("a1cf65a0"))
	draw_string(ThemeDB.fallback_font,Vector2(0,13),"En çok %d yavru · %d birey"%[peak,samples.size()],HORIZONTAL_ALIGNMENT_LEFT,-1,11,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(12,size.y-2),"0",HORIZONTAL_ALIGNMENT_LEFT,-1,11,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(size.x-10,size.y-2),"1",HORIZONTAL_ALIGNMENT_LEFT,-1,11,Color("b6ccd0"))
