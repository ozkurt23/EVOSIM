extends Control
var samples=[]
func _draw():
	var base=size.y-22;var h=base-17
	draw_line(Vector2(0,base),Vector2(size.x,base),Color("557985"),1)
	if samples.is_empty():return
	var start=float(samples[0].day);var span=maxf(.05,samples[-1].day-start)
	var fields=["energy","water","health"];var colors=[Color("dcac68"),Color("5cadd0"),Color("a1cf65")]
	for k in 3:
		var line=PackedVector2Array()
		for p in samples:
			if not p.exists or not p.alive:
				if line.size()>1:draw_polyline(line,colors[k],1.5,true)
				line.clear();continue
			line.append(Vector2((p.day-start)/span*size.x,base-clampf(p[fields[k]],0,100)*h/100))
		if line.size()>1:draw_polyline(line,colors[k],1.5,true)
		elif line.size()==1:draw_circle(line[0],2,colors[k])
	draw_string(ThemeDB.fallback_font,Vector2(0,12),"%100",HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(0,size.y-3),"%.0f gün"%start,HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(size.x-68,size.y-3),"%.0f gün"%samples[-1].day,HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("b6ccd0"))
