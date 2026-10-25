extends Control
signal day_selected(day: float)
var samples=[]
var hover_x=-1.0
func _ready():mouse_filter=Control.MOUSE_FILTER_STOP;mouse_exited.connect(func():hover_x=-1;queue_redraw())
func _gui_input(event):
	if event is InputEventMouseMotion:hover_x=event.position.x;queue_redraw()
	if event is InputEventMouseButton and event.pressed and event.button_index==MOUSE_BUTTON_LEFT and samples.size()>1:
		day_selected.emit(lerpf(samples[0].day,samples[-1].day,clampf(event.position.x/size.x,0,1)))
func _draw():
	if samples.size()<2:
		draw_string(ThemeDB.fallback_font,Vector2(4,24),"Geçmiş birikiyor…",HORIZONTAL_ALIGNMENT_LEFT,-1,13,Color("b6ccd0"));return
	var highest=1.0;var peak_flow=1.0
	for i in samples.size():
		var s=samples[i];highest=maxf(highest,maxf(s.herbivores,s.carnivores))
		if i>0:
			var days=maxf(.001,s.day-samples[i-1].day)
			peak_flow=maxf(peak_flow,maxf(s.get("births",0)-samples[i-1].get("births",0),s.get("deaths",0)-samples[i-1].get("deaths",0))/days)
	var end=maxf(.01,samples[-1].day-samples[0].day);var top=size.y*.52;var bottom=size.y-20
	for species in ["herbivores","carnivores"]:
		var points=PackedVector2Array()
		for s in samples:points.append(Vector2((s.day-samples[0].day)/end*size.x,top-s[species]/highest*(top-20)))
		draw_polyline(points,Color("a1cf65") if species=="herbivores" else Color("e7b28c"),2,true)
	for key in ["births","deaths"]:
		var points=PackedVector2Array()
		for i in range(1,samples.size()):
			var s=samples[i];var rate=maxf(0,s.get(key,0)-samples[i-1].get(key,0))/maxf(.001,s.day-samples[i-1].day)
			points.append(Vector2((s.day-samples[0].day)/end*size.x,bottom-rate/peak_flow*(size.y*.25)))
		if points.size()>1:draw_polyline(points,Color("a1cf65") if key=="births" else Color("d39477"),1.5,true)
	draw_string(ThemeDB.fallback_font,Vector2(0,12),"%d canlı"%int(highest),HORIZONTAL_ALIGNMENT_LEFT,-1,11,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(0,top+16),"Doğum / ölüm · en çok %.1f / gün"%peak_flow,HORIZONTAL_ALIGNMENT_LEFT,-1,10,Color("b6ccd0"))
	draw_string(ThemeDB.fallback_font,Vector2(0,size.y-3),"%.0f–%.0f gün · geçmişe gitmek için tıkla"%[samples[0].day,samples[-1].day],HORIZONTAL_ALIGNMENT_LEFT,-1,10,Color("b6ccd0"))
	if hover_x>=0:
		draw_line(Vector2(hover_x,18),Vector2(hover_x,bottom),Color("f3f2df80"))
		var day=lerpf(samples[0].day,samples[-1].day,clampf(hover_x/size.x,0,1))
		tooltip_text="%.1f gün · tıkla ve kayıtlı geçmişe git"%day
