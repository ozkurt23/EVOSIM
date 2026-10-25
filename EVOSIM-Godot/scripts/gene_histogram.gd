extends Control
## Both diets share a count axis; bins are [0,.1), ... [.9,1].
var herb=[]
var carn=[]
var founder_herb=[]
var founder_carn=[]
func _draw():
	if herb.size()!=10 or carn.size()!=10: return
	var highest=0.01
	var totals=[maxi(1,int(Array(herb).reduce(func(a,b):return a+b,0))),maxi(1,int(Array(carn).reduce(func(a,b):return a+b,0)))]
	var originals=[founder_herb,founder_carn]
	for i in 10: highest=maxf(highest,maxf(float(herb[i])/totals[0],float(carn[i])/totals[1]))
	for data in originals:
		var total=maxi(1,int(Array(data).reduce(func(a,b):return a+b,0)))
		for value in data:highest=maxf(highest,float(value)/total)
	var base=size.y-19
	var height=maxf(1,base-18)
	var cell=size.x/10
	for i in 10:
		for diet in 2:
			var value=herb[i] if diet==0 else carn[i]
			var h=float(value)/totals[diet]/highest*height
			draw_rect(Rect2(i*cell+2+diet*cell*.43,base-h,cell*.38,h),Color("a1cf65") if diet==0 else Color("e7b28c"))
	for diet in 2:
		var data=originals[diet]
		if data.size()!=10:continue
		var total=maxi(1,int(Array(data).reduce(func(a,b):return a+b,0)));var points=PackedVector2Array()
		for i in 10:points.append(Vector2((i+.5)*cell,base-float(data[i])/total/highest*height))
		draw_polyline(points,Color("a1cf6580") if diet==0 else Color("e7b28c80"),1.5,true)
	draw_line(Vector2(0,base),Vector2(size.x,base),Color("557985"),1)
	draw_string(ThemeDB.fallback_font,Vector2(0,12),"%%%d · tür içindeki pay"%roundi(highest*100),HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("f3f2df"))
	draw_string(ThemeDB.fallback_font,Vector2(0,size.y-3),"0",HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(size.x-10,size.y-3),"1",HORIZONTAL_ALIGNMENT_LEFT,-1,12,Color("b6ccd0"))
