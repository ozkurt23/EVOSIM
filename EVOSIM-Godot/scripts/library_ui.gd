extends CanvasLayer
## Reference grammar: restrained navy frames, ivory type, orange active actions.
signal world_changed
var bridge
var hud
var overlay: ColorRect
var column: VBoxContainer
var error_label: Label
var values={}
var editors={}
var world_name: LineEdit
var seed_input: SpinBox
var resume_button: Button
var was_paused=true
var trash=false
var legacy=false
var entered_world=false
var climate_choice: CheckBox
var import_dialog: FileDialog

func build(adapter,observer):
	bridge=adapter;hud=observer;layer=5
	overlay=ColorRect.new();overlay.color=Color(0.035,.075,.085,.83)
	overlay.theme=_theme()
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT);add_child(overlay)
	var center=CenterContainer.new();center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT);overlay.add_child(center)
	var panel=PanelContainer.new();panel.custom_minimum_size=Vector2(820,620);panel.add_theme_stylebox_override("panel",hud.surface());center.add_child(panel)
	column=VBoxContainer.new();column.add_theme_constant_override("separation",12);panel.add_child(column)
	overlay.hide()
	var menu=hud.button("","Dünyalar · Esc");menu.text="☰";hud.root.add_child(menu)
	menu.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT);menu.offset_left=18;menu.offset_right=54;menu.offset_top=-54;menu.offset_bottom=-18
	menu.pressed.connect(show_library)

func _clear():
	for node in column.get_children(): node.free()

func _button(text: String,callback: Callable,parent: Node):
	var b=hud.button("",text);b.text=text;b.pressed.connect(callback);parent.add_child(b);return b

func show_library():
	if entered_world and not bridge.SaveCurrent(bridge.WorldName):
		bridge.Paused=true;overlay.show();_library();_error();return
	was_paused=bridge.Paused;bridge.Paused=true;overlay.show();_library()

func _library():
	_clear()
	column.add_child(hud.label("EVOSIM  /  Yaşayan orman" if bridge.LivingForest else "EVOSIM  /  Dünyaların",26))
	var actions=HBoxContainer.new();column.add_child(actions)
	resume_button=_button("Vadiye dön",close,actions);resume_button.disabled=not bridge.CanResume
	_button("Yeni dünya",_setup,actions)
	_button("Karşılaştır",_compare,actions)
	_button("Kaydet",func():
		if bridge.SaveCurrent(bridge.WorldName): _library()
		else: _error(),actions)
	_button("Silinenler" if not trash else "Dünyalar",func(): trash=not trash;legacy=false;_library(),actions)
	if not bridge.LivingForest: _button("Eski kayıtlar" if not legacy else "Dünyalar",func(): legacy=not legacy;trash=false;_library(),actions)
	if bridge.LivingForest: _button("Eski dünyayı aktar",_choose_legacy,actions)
	var scroll=ScrollContainer.new();scroll.custom_minimum_size=Vector2(790,420);scroll.size_flags_vertical=Control.SIZE_EXPAND_FILL;column.add_child(scroll)
	var list=VBoxContainer.new();list.size_flags_horizontal=Control.SIZE_EXPAND_FILL;scroll.add_child(list)
	for slot in (bridge.ListLegacyWorlds() if legacy else bridge.ListWorlds(trash)):
		var row=HBoxContainer.new();list.add_child(row)
		var name=LineEdit.new();name.text=slot.name;name.custom_minimum_size.x=260;row.add_child(name)
		name.text_submitted.connect(func(text):
			if not bridge.RenameWorld(slot.id,text): _error())
		name.tooltip_text="Adı düzenle ve Enter'a bas"
		var info=hud.label("%d gün · %d canlı"%[slot.days,slot.living],13);info.custom_minimum_size.x=160;row.add_child(info)
		if legacy:
			name.editable=false
			_button("İçe aktar",func():
				if bridge.ImportLegacyWorld(slot.id): world_changed.emit();close()
				else: _error(),row)
		elif trash:
			name.editable=false
			_button("Geri al",func():
				if bridge.RestoreWorld(slot.id): _library()
				else: _error(),row)
		else:
			_button("Aç",func():
				if bridge.OpenWorld(slot.id): world_changed.emit();close()
				else: _error(),row)
			_button("Sil",func():
				if bridge.DeleteWorld(slot.id): _library()
				else: _error(),row).tooltip_text="Silinenler'e taşı; oradan geri alınabilir"
	if list.get_child_count()==0: list.add_child(hud.label("Henüz kayıtlı dünya yok." if not trash else "Silinen dünya yok.",16))
	error_label=hud.label("Orman dünyaları ayrı saklanır. Eski dünyalar özgün başlatıcıda korunur." if bridge.LivingForest else "Kayıtlar bu bilgisayarda saklanır. Silinen dünyalar geri alınabilir.",13);column.add_child(error_label)

func _error(): error_label.text=bridge.LastError
func close(): entered_world=true;overlay.hide();bridge.Paused=was_paused

func _setup():
	_clear();values.clear();editors.clear()
	column.add_child(hud.label("Yeni bir ekosistem",26))
	var heading=HBoxContainer.new();column.add_child(heading)
	world_name=LineEdit.new();world_name.text="Yeni vadi";world_name.custom_minimum_size.x=260;heading.add_child(world_name)
	seed_input=SpinBox.new();seed_input.min_value=0;seed_input.max_value=2147483647;seed_input.step=1;seed_input.custom_minimum_size.x=170;seed_input.value=randi()%2147483647;heading.add_child(seed_input);seed_input.tooltip_text="Dünya tohumu · aynı ayarlar ve tohum aynı başlangıcı üretir"
	var presets=OptionButton.new();for name in (["Referans","Yalnız otçullar","Yaşlanmayanlar","Seyrek vadi","Hızlı orman döngüsü"] if bridge.LivingForest else ["Referans","Yalnız otçullar","Yaşlanmayanlar","Seyrek vadi"]): presets.add_item(name)
	heading.add_child(presets);presets.item_selected.connect(_preset)
	climate_choice=CheckBox.new();climate_choice.text="Mevsimsel kuraklık ve yağmur";climate_choice.button_pressed=true;column.add_child(climate_choice)
	climate_choice.tooltip_text="240 günlük döngü: 80 gün ılıman, 80 gün kuraklık, ardından toparlanma. Yağmur ilk 30 toparlanma gününde görünür."
	var tabs=TabContainer.new();tabs.custom_minimum_size=Vector2(790,440);tabs.size_flags_vertical=Control.SIZE_EXPAND_FILL;column.add_child(tabs)
	var groups={}
	for setting in bridge.Settings():
		if not groups.has(setting.group):
			var scroll=ScrollContainer.new();scroll.name=setting.group;tabs.add_child(scroll)
			var rows=VBoxContainer.new();rows.size_flags_horizontal=Control.SIZE_EXPAND_FILL;scroll.add_child(rows);groups[setting.group]=rows
		var row=HBoxContainer.new();groups[setting.group].add_child(row)
		var title=hud.label(setting.label,15);title.custom_minimum_size.x=370;row.add_child(title)
		var help=hud.label("ⓘ",16);help.tooltip_text=setting.help;help.mouse_filter=Control.MOUSE_FILTER_STOP;row.add_child(help)
		values[setting.key]=setting.value
		var editor
		if setting.normalized:
			editor=HSlider.new();editor.min_value=setting.min;editor.max_value=setting.max;editor.step=.001;editor.custom_minimum_size=Vector2(180,30)
		else:
			editor=SpinBox.new();editor.min_value=setting.min;editor.max_value=setting.max;editor.allow_greater=true
			editor.step=1 if setting.integer else .001;editor.custom_minimum_size.x=180
		editor.value=setting.value;row.add_child(editor);editors[setting.key]=editor;editor.tooltip_text=setting.help
		var readout=hud.label("%.3f"%setting.value,12) if setting.normalized else null
		if readout: row.add_child(readout)
		editor.value_changed.connect(func(v):
			values[setting.key]=v
			if readout: readout.text="%.3f"%v)
	error_label=hud.label("Ömür 0: yaşlılıktan ölüm kapalı. Ayarlar yeni dünya kurulurken uygulanır.",13);column.add_child(error_label)
	var buttons=HBoxContainer.new();column.add_child(buttons)
	_button("Geri",_library,buttons)
	_button("Dünyayı oluştur",func():
		if bridge.CreateWorld(world_name.text,int(seed_input.value),values):
			bridge.ClimateCycle=climate_choice.button_pressed
			if not bridge.SaveCurrent(world_name.text): _error();return
			world_changed.emit();was_paused=false;close()
		else: _error(),buttons)

func _preset(index: int):
	for setting in bridge.Settings(): editors[setting.key].value=setting.value
	if index==1: editors.Carnivores.value=0
	if index==2:
		for key in ["HerbLifespan","CarnLifespan","HerbAging","CarnAging"]: editors[key].value=0
	if index==3: editors.Herbivores.value=30;editors.Carnivores.value=3;editors.TreeCount.value=100

	if index==4 and editors.has("ForestLifespan"):
		editors.ForestLifespan.value=2;editors.ForestMaturity.value=.15
		editors.ForestSeedSpread.value=.8;editors.ForestDensity.value=.6

func _theme() -> Theme:
	var theme=Theme.new();theme.default_font_size=16
	for type in ["LineEdit","OptionButton","TabContainer","TabBar","SpinBox","TooltipPanel"]:
		for state in ["normal","panel","tab_unselected"]: theme.set_stylebox(state,type,hud.surface(1))
		var active=hud.surface(1);active.bg_color=Color("315261");active.border_color=Color("f3f2df")
		for state in ["focus","hover","tab_selected"]: theme.set_stylebox(state,type,active)
		for state in ["font_color","font_selected_color","font_unselected_color"]: theme.set_color(state,type,Color("f3f2df"))
	theme.set_stylebox("panel","TooltipPanel",hud.surface(.94));theme.set_color("font_color","TooltipLabel",Color("f3f2df"))
	return theme

func _choose_legacy():
	if bridge.ImportBusy:return
	import_dialog=FileDialog.new();import_dialog.file_mode=FileDialog.FILE_MODE_OPEN_FILE
	import_dialog.access=FileDialog.ACCESS_FILESYSTEM;import_dialog.filters=PackedStringArray(["*.evoworld ; EVOSIM dünyası"])
	import_dialog.title="Eski dünyadan yeni orman dünyası oluştur"
	import_dialog.file_selected.connect(func(path):
		if bridge.BeginLegacyImport(path):
			_clear();column.add_child(hud.label("Dünya aktarılıyor…",26))
			error_label=hud.label("Kaynak korunur. Orman geçmişi seçilen kaydın son gününden başlar.",14);column.add_child(error_label)
		else:_error()
		import_dialog.queue_free())
	import_dialog.canceled.connect(func():import_dialog.queue_free())
	add_child(import_dialog);import_dialog.popup_centered(Vector2i(850,560))

func _process(_delta):
	if not bridge or not bridge.ImportBusy:return
	var result=bridge.PollLegacyImport()
	if result==1:
		was_paused=true;world_changed.emit();close()
	elif result==-1:_library();_error()

func _compare():
	_clear();column.add_child(hud.label("İki dünyanın sonuçları",26))
	var worlds=bridge.ListWorlds(false)
	if worlds.size()<2:
		column.add_child(hud.label("Karşılaştırmak için en az iki kayıtlı dünya gerekli.",15))
		_button("Geri",_library,column);return
	var row=HBoxContainer.new();column.add_child(row)
	var a=OptionButton.new();var b=OptionButton.new()
	for slot in worlds:a.add_item(slot.name);b.add_item(slot.name)
	a.custom_minimum_size.x=330;b.custom_minimum_size.x=330;row.add_child(a);row.add_child(b);b.select(1)
	var scroll=ScrollContainer.new();scroll.custom_minimum_size=Vector2(790,440);column.add_child(scroll)
	var output=VBoxContainer.new();scroll.add_child(output)
	var update=func():
		for node in output.get_children():node.free()
		var result=bridge.CompareSavedWorlds(worlds[a.selected].id,worlds[b.selected].id)
		if result.is_empty():output.add_child(hud.label(bridge.LastError,14));return
		output.add_child(hud.label("Aynı tohum" if result.same_seed else "Farklı tohumlar · koşullar birebir karşılaştırılamaz",14))
		var headings=HBoxContainer.new();output.add_child(headings)
		var spacer=Control.new();spacer.custom_minimum_size.x=430;headings.add_child(spacer)
		for name in [worlds[a.selected].name,worlds[b.selected].name]:
			var title=hud.label(name,12);title.custom_minimum_size.x=140;title.text_overrun_behavior=TextServer.OVERRUN_TRIM_ELLIPSIS;headings.add_child(title)
		for metric in result.rows:
			var item=HBoxContainer.new();output.add_child(item)
			var title=hud.label(metric.label,14);title.custom_minimum_size.x=430;item.add_child(title)
			for value in [metric.a,metric.b]:
				var label=hud.label(str(roundi(value)) if is_equal_approx(value,roundf(value)) else "%.2f"%value,14);label.custom_minimum_size.x=140;item.add_child(label)
		output.add_child(hud.label("\nDeğişen başlangıç ayarları",16))
		for difference in result.differences:output.add_child(hud.label(difference,13))
		output.add_child(hud.label("Tek koşu nedenselliği kanıtlamaz. Günleri ve başlangıç nüfuslarını birlikte değerlendirin.",12))
	a.item_selected.connect(func(_index):update.call());b.item_selected.connect(func(_index):update.call())
	update.call();_button("Geri",_library,column)
