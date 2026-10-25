extends CanvasLayer
## Reference-derived observer instruments; simulation decisions never live here.
signal pause_requested
signal speed_requested
signal quality_requested
signal selection_closed
const INK=Color("f3f2df")
const MUTED=Color("b6ccd0")
var root: Control
var dial: Control
var stats_label: Label
var total_label: Label
var herb_count: Label
var carn_count: Label
var paused_button: Button
var speed_button: Button
var quality_button: Button
var card: PanelContainer
var card_title: Label
var card_subtitle: Label
var energy_bar: ProgressBar
var water_bar: ProgressBar
var energy_value: Label
var water_value: Label
var climate_label: Label
var action_label: Label
var world_energy: ProgressBar
var world_water: ProgressBar
var world_energy_value: Label
var world_water_value: Label

func surface(opacity=0.97,radius=2) -> StyleBoxFlat:
	var box=StyleBoxFlat.new();box.bg_color=Color("19333e");box.bg_color.a=opacity
	box.set_corner_radius_all(radius);box.set_border_width_all(2);box.border_color=Color("557985")
	box.content_margin_left=10;box.content_margin_right=10;box.content_margin_top=7;box.content_margin_bottom=7
	return box
func label(text: String,size=16,color=INK) -> Label:
	var node=Label.new();node.text=text
	node.add_theme_font_size_override("font_size",size);node.add_theme_color_override("font_color",color)
	if size>=17: node.add_theme_font_override("font",preload("res://assets/fonts/DejaVuSans-Bold.ttf"))
	return node
func icon(name: String) -> Texture2D:
	return load("res://assets/ui/"+name+".svg")
func button(name: String,tip: String) -> Button:
	var b=Button.new();b.custom_minimum_size=Vector2(36,36);b.tooltip_text=tip
	b.mouse_default_cursor_shape=Control.CURSOR_POINTING_HAND
	if name!="": b.icon=icon(name)
	b.icon_alignment=HORIZONTAL_ALIGNMENT_CENTER;b.add_theme_font_size_override("font_size",17)
	b.add_theme_color_override("font_color",INK)
	var normal=surface();normal.content_margin_left=5;normal.content_margin_right=5
	var hover=normal.duplicate();hover.border_color=INK;hover.bg_color=Color("315261")
	var pressed=normal.duplicate();pressed.bg_color=Color("b85a20");pressed.border_color=INK
	b.add_theme_stylebox_override("normal",normal);b.add_theme_stylebox_override("hover",hover)
	b.add_theme_stylebox_override("pressed",pressed);b.add_theme_stylebox_override("focus",hover)
	return b
func picture(parent: Control,name: String,pos: Vector2,tint=INK):
	var p=TextureRect.new();p.texture=icon(name);p.position=pos;p.size=Vector2(25,25)
	p.stretch_mode=TextureRect.STRETCH_KEEP_ASPECT_CENTERED;p.modulate=tint;p.mouse_filter=Control.MOUSE_FILTER_IGNORE
	parent.add_child(p)
func _ready():
	root=Control.new();root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter=Control.MOUSE_FILTER_IGNORE;add_child(root)
	var theme=Theme.new();theme.default_font_size=16
	theme.set_stylebox("panel","TooltipPanel",surface(.94));theme.set_color("font_color","TooltipLabel",INK);root.theme=theme
	var counter=Panel.new();counter.position=Vector2(154,26);counter.size=Vector2(156,52)
	counter.add_theme_stylebox_override("panel",surface());root.add_child(counter)
	picture(counter,"brand",Vector2(27,13),Color("9fca68"))
	total_label=label("0",26);total_label.position=Vector2(61,7);counter.add_child(total_label)
	counter.tooltip_text="Toplam yaşayan canlı · dünyanın tamamı"
	stats_label=label("",13,MUTED);stats_label.position=Vector2(181,134);root.add_child(stats_label)
	stats_label.tooltip_text="Dünya takvimi · 365 gün = 1 yıl"
	var playback=HBoxContainer.new();playback.position=Vector2(181,92);playback.add_theme_constant_override("separation",3);root.add_child(playback)
	paused_button=button("pause","Duraklat / devam et · Boşluk")
	var action=surface();action.bg_color=Color("b85a20");action.border_color=INK
	paused_button.add_theme_stylebox_override("normal",action)
	paused_button.pressed.connect(func(): pause_requested.emit());playback.add_child(paused_button)
	speed_button=button("","Simülasyon hızı · 1× – 20×");speed_button.custom_minimum_size.x=62
	speed_button.pressed.connect(func(): speed_requested.emit());playback.add_child(speed_button)
	quality_button=button("quality","Grafik kalitesi · yüksek / dengeli")
	var utility=surface(1,18);utility.bg_color=Color("b85a20");utility.border_color=INK
	quality_button.add_theme_stylebox_override("normal",utility)
	quality_button.position=Vector2(320,32);quality_button.pressed.connect(func(): quality_requested.emit());root.add_child(quality_button)
	dial=load("res://scripts/population_dial.gd").new();dial.position=Vector2(8,8);dial.size=Vector2(168,168);root.add_child(dial)
	picture(dial,"leaf",Vector2(40,43),Color("a1cf65"));picture(dial,"paw",Vector2(40,97),Color("e7b28c"))
	herb_count=label("0",23);herb_count.position=Vector2(76,39);dial.add_child(herb_count)
	carn_count=label("0",23);carn_count.position=Vector2(76,93);dial.add_child(carn_count)
	_build_world_meters();_build_card()
	var climate_panel=PanelContainer.new();root.add_child(climate_panel)
	climate_panel.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT)
	climate_panel.offset_left=-306;climate_panel.offset_right=-12;climate_panel.offset_top=83
	var climate_style=surface();climate_style.content_margin_top=3;climate_style.content_margin_bottom=3
	climate_panel.add_theme_stylebox_override("panel",climate_style)
	climate_label=label("",12,MUTED);climate_panel.add_child(climate_label)
	climate_panel.tooltip_text="Kuraklık su erişimini ve bitki yenilenmesini azaltır. Yağmur, kuraklık sonrası mevcut toparlanmayı gösterir."

func _build_world_meters():
	var panel=PanelContainer.new();root.add_child(panel);panel.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT)
	panel.offset_left=-306;panel.offset_right=-12;panel.offset_top=12
	panel.add_theme_stylebox_override("panel",surface())
	var col=VBoxContainer.new();col.add_theme_constant_override("separation",7);panel.add_child(col)
	for name in ["leaf","water"]:
		var row=HBoxContainer.new();row.add_theme_constant_override("separation",9);col.add_child(row)
		var p=TextureRect.new();p.texture=icon(name);p.custom_minimum_size=Vector2(20,23);p.stretch_mode=TextureRect.STRETCH_KEEP_ASPECT_CENTERED;row.add_child(p)
		var bar=_bar(Color("d6a944") if name=="leaf" else Color("4fadd5"));bar.custom_minimum_size=Vector2(175,12)
		bar.size_flags_vertical=Control.SIZE_SHRINK_CENTER;row.add_child(bar)
		var value=label("0%",17);value.custom_minimum_size.x=43;row.add_child(value)
		row.tooltip_text="Yaşayan canlıların ortalama enerji doluluğu" if name=="leaf" else "Yaşayan canlıların ortalama su doluluğu"
		if name=="leaf": world_energy=bar;world_energy_value=value
		else: world_water=bar;world_water_value=value
func _build_card():
	card=PanelContainer.new();root.add_child(card);card.set_anchors_and_offsets_preset(Control.PRESET_TOP_RIGHT)
	card.offset_left=-306;card.offset_right=-12;card.offset_top=119
	card.add_theme_stylebox_override("panel",surface())
	var content=VBoxContainer.new();content.add_theme_constant_override("separation",9);card.add_child(content)
	var header=HBoxContainer.new();content.add_child(header)
	card_title=label("",18);card_title.size_flags_horizontal=Control.SIZE_EXPAND_FILL;header.add_child(card_title)
	var close=button("close","Gözlemi kapat");close.pressed.connect(func(): card.hide();selection_closed.emit());header.add_child(close)
	card_subtitle=label("",13,MUTED);content.add_child(card_subtitle)
	action_label=label("",13,MUTED);content.add_child(action_label)
	energy_value=label("Enerji",13,MUTED);content.add_child(energy_value)
	energy_bar=_bar(Color("d6a944"));content.add_child(energy_bar)
	water_value=label("Su",13,MUTED);content.add_child(water_value)
	water_bar=_bar(Color("4fadd5"));content.add_child(water_bar);card.hide()
func _bar(color: Color) -> ProgressBar:
	var bar=ProgressBar.new();bar.custom_minimum_size=Vector2(220,12);bar.show_percentage=false
	var bg=StyleBoxFlat.new();bg.bg_color=Color("10242e");bg.set_border_width_all(1);bg.border_color=Color("7a8c8e");bg.set_corner_radius_all(2)
	var fill=StyleBoxFlat.new();fill.bg_color=color;fill.set_corner_radius_all(1)
	bar.add_theme_stylebox_override("background",bg);bar.add_theme_stylebox_override("fill",fill);return bar
func update_world(stats: Dictionary,paused: bool,speed: int,high_quality: bool):
	herb_count.text=str(stats.herbivores);carn_count.text=str(stats.carnivores)
	climate_label.text=("Kuraklık" if stats.drought else ("Yağmur · toparlanma" if stats.rain else "Ilıman"))+"  ·  Su erişimi %d%%"%(roundi(stats.water_level*100))
	total_label.text=str(stats.herbivores+stats.carnivores);dial.update_counts(stats.herbivores,stats.carnivores)
	var days=int(stats.days);var hours=int((stats.days-days)*24)
	stats_label.text=("%d yıl · %d gün"%[days/365,days%365]) if days>=365 else "%d gün · %d sa"%[days,hours]
	paused_button.icon=icon("play" if paused else "pause")
	speed_button.tooltip_text="Hedef %d× · gerçekleşen %.1f× · bekleyen %.1f gün"%[speed,stats.get("effective_speed",speed),stats.pending]
	speed_button.text=str(speed)+"×";quality_button.modulate=Color.WHITE if high_quality else Color(.65,.72,.73)
	world_energy.value=stats.mean_energy;world_water.value=stats.mean_water
	world_energy_value.text="%d%%"%stats.mean_energy;world_water_value.text="%d%%"%stats.mean_water
func show_creature(data: Dictionary):
	var actions={"move":"Hareket ediyor","rest":"Dinleniyor","drink":"Su içiyor","eat":"Ot yiyor","fruit":"Meyve yiyor","scavenge":"Leşten besleniyor","bite":"Isırıyor","wander":"Gözlem"}
	action_label.text=actions.get(data.action,"Gözlem")+(" · Yavru" if data.maturity<.98 else "")
	if data.health<30: action_label.text+=" · Yaralı"
	card.show();card_title.text="%s #%d"%["Etçil" if data.predator else "Otçul",data.id]
	card_subtitle.text="Nesil %d   ·   %s"%[data.generation,("%.1f yıl"%(data.age/365)) if data.age>=365 else "%.1f gün"%data.age]
	energy_value.text="Enerji   %d / %d"%[data.energy,data.max_energy]
	energy_bar.value=100*data.energy/maxf(1,data.max_energy)
	water_value.text="Su   %d / 100"%data.water;water_bar.value=data.water
