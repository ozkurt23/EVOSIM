extends CanvasLayer
var bridge
var hud
var root: Control
var panels=[]
var serial=0
var elapsed=0.0
var world_view
var region_point=Vector2.ZERO
var tracked_ancestor=-1
var note_button: Button
var latest_note={}
var lineage_members=[]
var map_mode=0
var map_samples=[]
var map_overlay
func build(adapter,observer):
	bridge=adapter;hud=observer;layer=3
	root=Control.new();root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT);root.mouse_filter=Control.MOUSE_FILTER_IGNORE;add_child(root)
	var toggle=hud.button("","Gözlemler");toggle.text="◷";hud.root.add_child(toggle)
	toggle.set_anchors_and_offsets_preset(Control.PRESET_BOTTOM_LEFT);toggle.offset_left=62;toggle.offset_right=98;toggle.offset_top=-54;toggle.offset_bottom=-18
	var options=PopupMenu.new();toggle.add_child(options)
	for name in ["Nüfus","Olay günlüğü","Zaman çizelgesi","Gen dağılımları","Kaynaklar","Gözlem raporunu dışa aktar","Yaşamın dağılımı","Besinin dağılımı","Son 30 gün: ölümler","Harita katmanını kapat","Özellik ve üreme başarısı","Ölüm nedenleri ve etkinlikler"]: options.add_item(name)
	toggle.pressed.connect(func(): options.position=Vector2i(62,int(get_viewport().get_visible_rect().size.y)-165);options.popup())
	options.id_pressed.connect(func(id):
		if id>=6 and id<=9:
			map_mode=0 if id==9 else id-5;_refresh();return
		if id>=10:open_panel("success" if id==10 else "outcomes");return
		if id==5:
			var path=bridge.ExportReport()
			if path!="": OS.shell_open(path)
			else: open_panel("events");panels[-1].text.text=bridge.LastError
		else: open_panel(["population","events","timeline","genes","resources"][id]))
	world_view=get_parent()
	map_overlay=load("res://scripts/observation_overlay.gd").new();map_overlay.observer=self;root.add_child(map_overlay);root.move_child(map_overlay,0)
	hud.dial.mouse_filter=Control.MOUSE_FILTER_STOP
	hud.dial.gui_input.connect(func(event):
		if event is InputEventMouseButton and event.pressed and event.button_index==MOUSE_BUTTON_LEFT:open_panel("population"))
	hud.dial.tooltip_text="Nüfusun değişimini incele"
	note_button=hud.button("","Dünyadaki son kayıtlı olay");hud.root.add_child(note_button)
	note_button.set_anchors_and_offsets_preset(Control.PRESET_CENTER_BOTTOM)
	note_button.offset_left=-210;note_button.offset_right=210;note_button.offset_top=-42;note_button.offset_bottom=-12
	note_button.text="Bir canlıya veya boş zemine tıklayarak incele"
	note_button.pressed.connect(func():
		if latest_note.is_empty():return
		if latest_note.located:world_view.navigate_world(Vector2(latest_note.x,latest_note.z));open_panel("creature",int(latest_note.id))
		else:open_panel("population" if latest_note.kind=="population" else "events"))
	get_viewport().size_changed.connect(_layout)

func open_panel(kind: String,id=-1):
	for p in panels:
		if p.kind==kind and p.id==id: _front(p);return
	var trace_started=true
	if kind=="trace": trace_started=bridge.StartLifeTrace(id)
	var node=PanelContainer.new();node.add_theme_stylebox_override("panel",hud.surface(1.0));root.add_child(node)
	var col=VBoxContainer.new();node.add_child(col)
	var header=HBoxContainer.new();col.add_child(header)
	var title=hud.button("","");title.text={"success":"Özellik ve üreme","outcomes":"Dünyada yaşam", "region":"Bu bölgede yaşam","population":"Nüfusun değişimi","events":"Olaylar","timeline":"Geçmiş","creature":"Canlı #%d"%id,"genes":"Gen dağılımları","resources":"Kaynaklar","trace":"Yaşam izi #%d"%id,"family":"Soy bağı #%d"%id,"tree":"Ağaç #%d"%(id if bridge.LivingForest else id+1)}.get(kind,kind);title.size_flags_horizontal=Control.SIZE_EXPAND_FILL;header.add_child(title)
	var close=hud.button("close","Kapat");header.add_child(close)
	var entry={"header":header,"kind":kind,"id":id,"node":node,"slot":serial%4,"col":col,"title":title};serial+=1
	var older=panels.filter(func(p): return p.slot==entry.slot)
	if older.is_empty(): panels.append(entry)
	else: panels.insert(panels.find(older[0]),entry)
	title.pressed.connect(func(): _front(entry))
	close.pressed.connect(func():
		if kind=="trace": bridge.CancelLifeTrace(id)
		panels.erase(entry);node.queue_free();_layout())
	if kind=="population":
		var graph=load("res://scripts/population_graph.gd").new();graph.custom_minimum_size=Vector2(260,160);col.add_child(graph);entry.graph=graph
		graph.day_selected.connect(func(day): bridge.SeekDay(day);open_panel("timeline"))
		col.add_child(hud.label("Otçul • yeşil     Etçil • kum rengi",12))
	elif kind=="trace":
		entry.error="" if trace_started else bridge.LastError
		var graph=load("res://scripts/life_trace_graph.gd").new();graph.custom_minimum_size=Vector2(260,116);col.add_child(graph);entry.graph=graph
		col.add_child(hud.label("Enerji • kum   Su • mavi   Sağlık • yeşil",11))
	elif kind=="creature":
		var trace=hud.button("","Kayıtlı geçmişten enerji, su ve sağlık değişimini incele");trace.text="Yaşam izi";col.add_child(trace)
		trace.pressed.connect(func():open_panel("trace",id))
		var family=hud.button("","Ebeveyn ve yavruların kayıtlarına geç");family.text="Soy bağı";col.add_child(family)
		family.pressed.connect(func():open_panel("family",id))
	elif kind=="region":
		entry.point=region_point
		var genes=hud.button("","Nesiller boyunca özellikler");genes.text="Evrimi incele";col.add_child(genes)
		genes.pressed.connect(func():open_panel("genes"))
	elif kind=="family":
		var scroll=ScrollContainer.new();scroll.custom_minimum_size=Vector2(260,150);col.add_child(scroll)
		var relatives=VBoxContainer.new();relatives.size_flags_horizontal=Control.SIZE_EXPAND_FILL;scroll.add_child(relatives)
		entry.relatives=relatives;entry.family_signature=""
		var track=hud.button("","Atanın yaşayan torunlarını haritada göster; iki ebeveyn de hesaba katılır")
		track.text="Soyu haritada takip et";col.add_child(track)
		track.pressed.connect(func():tracked_ancestor=-1 if tracked_ancestor==id else id;track.text="Takibi bırak" if tracked_ancestor==id else "Soyu haritada takip et")
	elif kind=="success":
		var graph=load("res://scripts/success_graph.gd").new();graph.custom_minimum_size=Vector2(260,140);col.add_child(graph);entry.graph=graph
		var selector=OptionButton.new()
		for name in ["Beden","Algı","Beslenme","Su uyumu","Atılganlık","Sosyal bağ"]:selector.add_item(name)
		col.add_child(selector);entry.selector=selector;selector.item_selected.connect(func(_index):_refresh())
	elif kind=="genes":
		var selector=OptionButton.new()
		for name in ["Beden","Algı","Beslenme","Su uyumu","Atılganlık","Sosyal bağ"]: selector.add_item(name)
		selector.tooltip_text="Sütunlar yaşayanları, ince çizgiler kurucuları gösterir. Her tür kendi içinde yüzdeye çevrilir. Kalıtsal genlerdir; yaşlılık etkisi değildir."
		col.add_child(selector);entry.selector=selector
		selector.item_selected.connect(func(_index): _refresh())
		var graph=load("res://scripts/gene_histogram.gd").new();graph.custom_minimum_size=Vector2(260,106);col.add_child(graph);entry.graph=graph
		col.add_child(hud.label("Otçul • yeşil     Etçil • kum rengi",12))
	elif kind=="timeline":
		var slider=HSlider.new();slider.step=.05;slider.custom_minimum_size=Vector2(260,30);col.add_child(slider);entry.slider=slider
		slider.drag_ended.connect(func(changed):
			if changed: bridge.SeekDay(slider.value))
		var seek=hud.button("","Seçili zamana git");seek.text="Göster";col.add_child(seek);seek.pressed.connect(func(): bridge.SeekDay(slider.value))
		var present=hud.button("","Kayıtlı son ana dön");present.text="Son ana dön";col.add_child(present);present.pressed.connect(func(): bridge.ReturnToPresent())
		var branch=hud.button("","Bu andan ayrı bir dünya oluştur");branch.text="Bu andan yeni dünya";col.add_child(branch)
		branch.pressed.connect(func():
			if not bridge.BranchHistory(bridge.WorldName+" · dal"): entry.text.text=bridge.LastError)
	var text=hud.label("",15);text.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART;text.custom_minimum_size.x=260
	if kind=="events":
		var scroll=ScrollContainer.new();scroll.custom_minimum_size=Vector2(260,170);col.add_child(scroll);text.size_flags_horizontal=Control.SIZE_EXPAND_FILL;scroll.add_child(text)
	else: col.add_child(text)
	entry.text=text;_layout();_refresh()

func _front(entry):
	panels.erase(entry);panels.append(entry);_layout()

func _layout():
	var screen=get_viewport().get_visible_rect().size
	for slot in 4:
		var stack=panels.filter(func(p): return p.slot==slot)
		var x=12.0 if slot<2 else screen.x-302
		var y=190.0 if slot%2==0 else maxf(420,screen.y-330)
		for i in stack.size():
			for content in stack[i].col.get_children(): content.visible=content==stack[i].header or i==stack.size()-1
		var front_height=300.0
		if not stack.is_empty(): front_height=maxf(front_height,stack[-1].node.get_combined_minimum_size().y)
		if slot%2==1: y=maxf(420,screen.y-18-front_height-minf(102,maxi(0,stack.size()-1)*34))
		var gap=minf(34,maxf(1,(screen.y-18-y-front_height)/maxi(1,stack.size()-1)))
		for i in stack.size():
			var p=stack[i]
			for content in p.col.get_children(): content.visible=content==p.header or i==stack.size()-1
			p.node.reset_size();p.node.position=Vector2(x+i*minf(7,14.0/maxi(1,stack.size()-1)),y+i*gap);p.node.custom_minimum_size=Vector2(288,0)
			root.move_child(p.node,root.get_child_count()-1)

func _process(delta):
	elapsed+=delta
	if elapsed>.25 and bridge: elapsed=0;_refresh()
func _refresh():
	map_samples=bridge.ObservationMap(map_mode) if map_mode>0 else []
	lineage_members=bridge.DescendantIds(tracked_ancestor) if tracked_ancestor>=0 else []
	map_overlay.queue_redraw()
	var notes=bridge.ObservationNotes()
	latest_note=notes[0] if not notes.is_empty() else {}
	if not latest_note.is_empty():
		note_button.text="%.0f gün · %s"%[latest_note.day,latest_note.text]
		note_button.text_overrun_behavior=TextServer.OVERRUN_TRIM_ELLIPSIS
		note_button.tooltip_text=latest_note.text+"\nTıkla: ilgili canlının mevcut konumu ve kaydı"
	else:note_button.text="Bir canlıya veya boş zemine tıklayarak incele"
	for p in panels:
		if p.kind=="success":
			p.graph.samples=bridge.ReproductiveSuccess(p.selector.selected);p.graph.queue_redraw()
			p.text.text="Nokta: bir ebeveyn\nYatay: kalıtsal özellik (0–1)\nDikey: olgunluk yaşına ulaşan yavru\nYaş ve çevre de sonucu etkiler; bu grafik nedensellik göstermez."
		elif p.kind=="outcomes":
			var outcome=bridge.PopulationOutcomes();p.text.text="Son 30 günde ölümler"
			for cause in outcome.deaths:p.text.text+="\n%s: %d"%[cause,outcome.deaths[cause]]
			if outcome.deaths.is_empty():p.text.text+="\nKayıt yok."
			p.text.text+="\n\nCanlıların şu anki eylemleri"
			var names={"wander":"Gezinme","move":"Hareket","eat":"Ot yeme","fruit":"Meyve yeme","drink":"Su içme","bite":"Isırma","rest":"Dinlenme","scavenge":"Leşten beslenme"}
			for action in outcome.actions:p.text.text+="\n%s: %d"%[names.get(action,action),outcome.actions[action]]
		elif p.kind=="region":
			var r=bridge.RegionObservation(p.point.x,p.point.y,8)
			p.text.text="8 birim yarıçap · %.0f gün\n%d otçul · %d etçil\n%d ağaç · %d meyve\nYerdeki besin: %.0f enerji\nSu hücresi: %d / %d\n\nSon 30 günde ölümler"%[r.day,r.herbivores,r.carnivores,r.trees,r.fruit,r.grass_energy,r.water_cells,r.cells]
			for cause in r.deaths:p.text.text+="\n%s: %d"%[cause,r.deaths[cause]]
			if r.deaths.is_empty():p.text.text+="\nKayıt yok."
			p.text.tooltip_text="Mevcut kaynak miktarıdır; her canlının bu kaynaklara erişebildiği anlamına gelmez. Ölüm konumları kayıtlı canlı kalıntılarından alınır."

		if p.kind=="population":
			p.graph.samples=bridge.PopulationHistory();p.graph.queue_redraw()
			var stats=bridge.Stats();p.text.text="%d doğum · %d ölüm"%[stats.births,stats.deaths]
		elif p.kind=="events":
			var lines=PackedStringArray()
			for event in bridge.Journal(): lines.append("%.1f gün · %s"%[event.day,event.text])
			p.text.text="\n".join(lines)
		elif p.kind=="trace":
			if p.error!="":p.text.text=p.error;continue
			var trace=bridge.ReadLifeTrace(p.id)
			if trace.missing:p.text.text="Bu iz artık açık değil.";continue
			if trace.error!="":p.text.text=trace.error;continue
			if not trace.complete:p.text.text="Geçmiş inceleniyor… %%%.0f"%(trace.fraction*100);continue
			p.graph.samples=trace.points;p.graph.queue_redraw()
			p.text.text="%.1f–%.1f gün · kayıtlı geçmiş"%[trace.from_day,trace.to_day]
			p.text.tooltip_text="Açıldığı andaki kaydın incelemesi. En fazla son 1200 gün. Canlının henüz doğmadığı veya öldüğü aralıklar çizilmez."
			p.text.mouse_filter=Control.MOUSE_FILTER_STOP
		elif p.kind=="family":
			var family=bridge.CreatureFamily(p.id)
			var signature=JSON.stringify(family)
			if signature!=p.family_signature:
				p.family_signature=signature
				for node in p.relatives.get_children():p.relatives.remove_child(node);node.queue_free()
				if family.is_empty():p.text.text="Bu anda henüz doğmamış.";continue
				for relation in ["parents","children"]:
					p.relatives.add_child(hud.label("Ebeveynler" if relation=="parents" else "Yavrular",12))
					if family[relation].is_empty():p.relatives.add_child(hud.label("—",12))
					for relative in family[relation]:
						var link=hud.button("","Canlının kartını aç")
						link.text="%s #%d · %s"%["Etçil" if relative.predator else "Otçul",relative.id,"Yaşıyor" if relative.alive else "Öldü"]
						link.pressed.connect(_open_relative.bind(int(relative.id)));p.relatives.add_child(link)
				p.text.text="Gösterilen andaki aile kayıtları"
		elif p.kind=="genes":
			var data=bridge.GeneDistributions();var index=p.selector.selected
			var herb=data.herbivores.genes[index];var carn=data.carnivores.genes[index]
			p.graph.herb=herb.bins;p.graph.carn=carn.bins
			var founders=bridge.FounderDistribution(index);p.graph.founder_herb=founders.herb;p.graph.founder_carn=founders.carn;p.graph.queue_redraw()
			var hm="%.2f"%herb.median if herb.has_samples else "—"
			var cm="%.2f"%carn.median if carn.has_samples else "—"
			p.text.text="Ortanca: otçul %s · etçil %s\nSütun: yaşayanlar · çizgi: kurucular\nÖrnek: %d otçul · %d etçil"%[hm,cm,herb.count,carn.count]
		elif p.kind=="tree":
			var tree=bridge.TreeDetails(p.id)
			if tree.is_empty():p.text.text="Bu dünyada ağaç bulunamadı.";continue
			p.text.text="Meyve: %d / %d\nMevcut besin enerjisi: %.0f\nTam meyve enerjisi: %.0f\nTaç yarıçapı: %.2f\nSu erişimi: %%%.0f"%[tree.available,tree.slots,tree.energy,tree.fruit_energy,tree.radius,tree.water_access*100]
			if tree.has("age_days"):
				p.text.text+="\nYaş: %.2f yıl · %s"%[tree.age_days/365.0,"Kuru gövde" if tree.dead else ("Fidan" if tree.growth<1 else "Olgun")]
				p.text.text+=("\nÖlüm: "+tree.death_cause) if tree.dead else "\nSağlık: %%%.0f · Büyüme: %%%.0f"%[tree.health*100,tree.growth*100]
				p.text.text+="\nToprağa ek katkı: %%%.0f"%(tree.soil*100)
			p.text.tooltip_text="Koparılan meyveler dünyanın yenilenme ayarlarına göre geri gelir. Kuraklığın etkisi bu dünyanın meyve dayanıklılığı ayarına bağlıdır. Enerji, modelin besin birimidir; kalori değildir."
			p.text.mouse_filter=Control.MOUSE_FILTER_STOP
		elif p.kind=="resources":
			var r=bridge.ResourceInventory()
			p.text.text="%d ağaç\nMeyve: %d / %d\nMeyve enerjisi: %.0f\nYerdeki besin enerjisi: %.0f\nSu erişimi: %%%.0f\n%d geçici kalıntı"%[r.trees,r.fruit_available,r.fruit_slots,r.fruit_energy,r.ground_energy,r.water_access*100,r.corpses]
			if r.has("living_trees"):
				p.text.text="%d yaşayan ağaç · %d fidan\n%d kuru gövde · %d tohum\n"%[r.living_trees,r.seedlings,r.dead_trunks,r.seeds]+p.text.text.substr(p.text.text.find("\n")+1)
			p.text.tooltip_text="Bir ağacın gövdesine tıklayarak kendi stokunu inceleyebilirsin. Dünyanın tamamındaki mevcut stok. Enerji değerleri kapasite değil, şu anda yenebilen toplam besindir. Su erişimi kuraklık modelinin katsayısıdır; nehir derinliği değildir."
			p.text.mouse_filter=Control.MOUSE_FILTER_STOP
		elif p.kind=="timeline":
			p.slider.min_value=bridge.EarliestTick*.05;p.slider.max_value=maxf(p.slider.min_value+.05,bridge.HeadTick*.05)
			p.text.text="Gösterilen: %.1f gün\nSon an: %.1f gün%s"%[bridge.CurrentTick*.05,bridge.HeadTick*.05," · Aranıyor…" if bridge.Seeking else ""]
		elif p.kind=="creature":
			var c=bridge.CreatureDetails(p.id)
			if c.is_empty(): p.text.text="Bu anda henüz doğmamış.";continue
			p.title.text="%s #%d"%["Etçil" if c.predator else "Otçul",c.id]
			p.text.text="%s · Nesil %d\nYaş %.2f yıl\nEnerji %.0f · Su %.0f · Sağlık %.0f\nEbeveynler %s / %s\n%d yavru · %d torun"%["Yaşıyor" if c.alive else "Öldü",c.generation,c.age/365,c.energy,c.water,c.health,"#%d"%c.parent_a if c.parent_a>0 else "—","#%d"%c.parent_b if c.parent_b>0 else "—",c.children,c.grandchildren]

func _open_relative(id: int):
	open_panel("creature",id)

func clear():
	for p in panels:
		if p.kind=="trace": bridge.CancelLifeTrace(p.id)
		p.node.queue_free()
	panels.clear();serial=0;tracked_ancestor=-1;lineage_members=[];map_mode=0;map_samples=[]
	if map_overlay:map_overlay.queue_redraw()

func inspect_region(point: Vector2):
	region_point=point
	open_panel("region",int(point.x/8)+int(point.y/8)*1000)
