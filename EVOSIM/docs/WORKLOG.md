# EVOSIM devam günlüğü

## Güncel güvenli teslim: v0.5.8 — 15 Eylül 2026

Kullanıcı 16 Eylül 09:00 İstanbul'a kadar geliştirme istedi. Saatlik heartbeat `evosim-geli-tirme-sabaha-kadar` aktif; devam eden işi kopyalama. Sabah sınırında yeni işe başlama, güvenli noktayı kaydet ve tamamlananlarla kalanları bildir. Kullanıcı telefondan bakıyor: yalnız anlamlı görsel teslimlerde gerçek uygulama screenshot'ı paylaş; her küçük düzeltmede bildirim yapma.

### Çalışan sürüm

`baslat.sh` → `release-v0.5.8`. Kaynak ve çalıştırılabilir paket `outputs/EVOSIM-0.5.8-linux-x64.zip`; önceki v0.5.0/v0.4.1/v0.4/v0.3 ZIP'leri korunur. Eski çalışan uygulamayı kapatma veya release klasörünü ezme. Yeni dilimi ayrı release klasöründe doğrula; testler geçince launcher'ı değiştir. Gerçek kullanıcı dünyalarını testte kullanma.

Core SHA: `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033` (v0.4.1, v0.5.0, v0.5.1 aynı biyoloji). Hazır kaçış/av/koruma/eşe gitme politikası YOK; kalıtsal 357 ağırlıklı ağ hareketi üretir. Mutasyon varsayılanı başarılı doğum başına toplam .001; gizlice artırma. Kullanıcının açık ihtiyaç/refleks istisnası korunur. GPU eğitimi/Unity build'i henüz yok; simülasyon CPU'da.

### Tamamlananlar

- Doğal açık UI/özgün ağaç-kaya-canlı çizimleri; normal simde üç kontrol. Tüm canlılarda otomatik taç saydamlığı ve kontur. Pusula yönleri iptal.
- 85 sayısal dünya ayarı + altı gen açıklaması; bol kurulum, scroll, 0–1 sürgü/gen min-max çift sürgü; ömür ve adet sayısal. Üreme modu/istek, ağaç parametreleri, ayrı ömür/yaşlanma, 128/192/256 harita.
- Dünya kaydet/aç/adlandır/sil/geri al; büyük harita kaynak arama düzeltmeleri. v0.4.1'de 23 Core +13 checkpoint grubu ve225 üretici kombinasyonu doğrulandı.
- v0.5: gerçek geçmiş, kayıtlı geleceği oynat, geçmişten yeni kimlikle dal; otomatik kayıt daima HeadWorld. 10 timeline +7 store grubu, exact replay ve yayın UI smoke.
- v0.5.1: gerçek ağaç/ot/su/kaya/barınak/ceset kartları; altı gen ve mevcut kapasite/ⓘ içeren canlı ayrıntısı; benzersiz altsoy sayımı ve tıklanabilir yerel aile grafiği; yaşayan gen histogramları/ortanca/aralık. Kaynak/soy/nüfus için6+6+6 grup, tam checkpoint/RNG değişmezliği ve gerçek yayın UI smoke. Nüfus kartına tıklamanın haritaya sızması da düzeltildi.

### Mimari ve sahiplik

Son üç alt ajan işi tamamlandı. Şu anda aktif üretim dosyası sahipliği yok; bir sonraki dilim için görevleri yeniden dağıt. Root'un son işi yayın/ekran/ZIP idi. Core'a dokunmayan gözlem panelleri TimelineSession/Archive/WorldStore biçimini değiştirmez.

LIBRARY-2 tek atomik .evoworld dosyasında head checkpoint + TIMELINE-1 arşivini saklar; .bak ve sil/geri al bütünü korur. Origin ve tam komut günlüğü saklanır.600 tick sıkıştırılmış hızlandırıcı,32 MiB opsiyonel cache; cache kırpılınca geçmiş kısalmaz. Aynı tick komutlarında NextCommandIndex korunur. Replay gerçek Core SHA ve şemaya kilitli. LIBRARY-1 eski dosya ayrı v0.5 kopyasına aktarılır; earliest=importtick. Farklı Core ile eski geçmişi sessizce oynatma; gelecekte Core değişecekse açık geçiş/kopya stratejisi şart.

Yeni dosyalar: ResourceInspection/ResourcePanel, LineageIndex/LineagePanel, PopulationInsights/PopulationInsightsPanel; root ObservationIntegration/CreatureDetails/ObservationUiSmoke. Paneller DisplayWorld'ü okur, geleceği sızdırmaz. Soy grafiği yereldir (ebeveynler → fokus → sayfalı dört yavru); küresel grafik/yaşam filmi tamamlanmadı. Histogram değişimi öğrenme kanıtı diye sunulmaz.

### Kanıt ve görüntüler

`artifacts/v0.5.1/ui-smoke.json` passed=true. Logdaki InvalidOperationException, bilerek silinen izole kayıtta başarısız kaydın uygulamayı kapatmadığını sınayan testtir. Üç helper test paketi kendi dondurulmuş girdilerini içerir. Core aynı olduğu için23 biyoloji testini gereksiz tekrar çalıştırma; üretim değişikliği kapsamına uygun testi seç.

Son gerçek yayın görüntüleri `artifacts/v0.5.1/tree.png`, `family.png`, `creature.png`, `insights.png`.256 harita/500 kurucu/350 ağaç,1200 tick (60sn):480 yaşayan,33 doğum,53 ölüm. Ağaç201 gerçek kısmi stok ve yenilenme; #529 gerçek yavru, ebeveynleri #6 ve #31. Root tree/family ekranlarını kullanıcıya paylaşacak/paylaştı; sıradan adımda aynılarını yeniden gönderme. Önceki v0.5/world-timeline.png zaten gönderildi.

### Sıradaki bağımsız, somut dilimler

1. Olay günlüğü v0.5.2 tamamlandı. Sırada gerekirse nüfus/bitki/nem yoğunluk katmanları. Yalnız mevcut olay/veri üzerinden; uydurma öğrenme/niyet metni ekleme. Ek araçlar menü veya bağlamdan açılır, sabit sim düğmeleri çoğalmaz.
2. Uzun koşu ve referans dünya profilleri: sabit eşli tohumlarla gerçek nesil/doğum/ölüm/kaynak/nüfus sonuçları; nadir mutasyonu ve varsayılan fiziği gizlice değiştirme. Yaşayan referans koşulları için açık isimli alternatif presetler düşünülebilir; davranış politikası enjekte etme.
3. Kontrollü dünya karşılaştırma, yaşam filmi, daha uzun geçmişte RAM/kayıt/arama maliyeti. Mevsim/ağaç yaşamı/afet/inşa için önce model geçişi ve checkpoint/replay uyumluluğu tasarla. Planın tümünü bitmiş gibi işaretleme.

### Araçlar

SDK `work/toolchain/dotnet/dotnet`10.0.401. SDL: yayımlanmış klasör veya `work/toolchain/sdl/usr/lib/x86_64-linux-gnu`. App CLI gerçek screenshot üretir: `--capture work/test.bmp --ticks 1200 --map-size 256 --herbivores 480 --carnivores 20 --trees 350 --zoom 3`; ek `--tree`, `--lineage --select 529`, `--insights`, `--details --select 529 --follow`; geçmiş için `--timeline --ticks 1800 --seek-tick 1200`. `--ui-smoke --out work/isolated --legacy-fixture work/v041-checkpoint/fixtures/v04-t1201.bin`. Native OS UI aracı yok; app CLI kullan. BMP→PNG Pillow, view_image ile incele. Kullanıcıya yalnız outputs altındaki mutlak dosya yollarını bağla.

Önceki ayrıntılı notlar `WORKLOG-through-v0.5.md` içinde tarihseldir; buradaki güncel durum önceliklidir.

## v0.5.2 — heartbeat teslimi

Root tek başına EventJournal.cs ve Observer entegrasyonunu tamamladı. Core aynı50e63...; yeni politika/mutasyon değişikliği yok. Filtreler, gerçek doğumdan soy, olayın tickine geçmiş araması, modal girdi koruması ve tam checkpoint değişmezliği yayın UI smoke ile geçti. Uzun olay listesi Core tarafından kırpıldığı için arayüz bunu açıkça söyler. Menüye bağlam aracı eklendi, üç sabit sim kontrolü korunur. Gerçek journal.png 256/500 kurucu/350ağaç,1200tick/480canlı/33doğum/53ölüm/87olay. Bu ekran kullanıcıya milestone olarak paylaşılacak/paylaşıldı. Bütün alt ajanlar tamamlanmış durumda; yeni aktif sahiplik yok. Sonraki heartbeat için öncelik uzun koşu/gerçek sonuç ölçümü veya yoğunluk katmanları.

## 15 Eylül 13:30 UTC heartbeat — uzun koşu ölçüm paketi

`artifacts/ecology-2026-09-15/` altında taşınabilir .NET10 harness, dondurulmuş güncel Core DLL, tam konfigürasyonlu18 JSON, CSV, analiz kodu, rapor ve nüfus grafiği tamamlandı. 6tohum ×3koşul ×24.000tick =432.000tick / toplam6simülasyon saati. Her dünya20dakika,128harita/150otçul/15etçil; referans155ağaç, alternatif350ağaç, referansla birebir aynı başlangıçtan200.sn kuraklık. Kuraklık eşinin başlangıçhash ve müdahale öncesi örnek eşitliği doğrulandı. Ağaç alternatifi farklı RNG tüketebilir; birebir aynı kuruculu nedensel deney diye sunulmaz.

Son nüfus: referans6koşunun2'sinde yalnız1canlı, bol ağaç4/6koşuda2–4canlı, kuraklık0/6. Toplam442doğum,0mutasyon; enyükseknesil2. 0mutasyon .001 oranında beklenebilir (%64.3 sıfır olay olasılığı), hata veya mutasyonu artırma gerekçesi değil. Ölüm kaydı toplam1481susuzluk/1464çatışma/336açlık/117yaşlılık. Bütün koşul ve örnek sayıları, nüfus muhasebesi, kaynak sonluluğu, CoreSHA ve değişmeyen mutasyon doğrulandı; derleme0uyarı/0hata. Gerçek veri grafiği görsel incelendi; oyun ekranı değişmediği için yinelenen screenshot yok.

Üretim kaynakları, launcher, gerçek dünyalar ve v0.5.2 ZIP değişmedi. Ölçüm paketi ayrıca teslim edilir; ZIP'e girdi diye iddia etme. Sonraki öncelik: su erişimi/çatışma/üreme darboğazlarını ayrı ölçmek; bunları kanıtlamadan sadece yeni görsel ekleyip modeli dengeli ilan etme. Model değişikliği gerekiyorsa replay sürüm geçişini önce tasarla. Mevcut rastgele kurucu ağlarla sürdürülebilir ekosistem veya öğrenilmiş beceri henüz yok.

## 15 Eylül 14:31 UTC heartbeat — v0.5.3 yaşam sekmesi

Root tek başına PopulationHealth.cs/PopulationHealthPanel.cs ve mevcut nüfus panelinde Genler/Yaşam sekmeleri ekledi. Salt okunur gerçek ölü birey kayıtları (diğer/bilinmiyor dahil), doğum/ölüm/mutasyon, Core InspectBreeding ilk engel bölümlenmesi ve hazır çift/eşeysiz sayıları. Görüntülenen World referansı/tick ve açılışla cache yenilenir. CoreSHA aynı50e63...; kayıtlar/timeline/mutasyon/politika değişmedi.

Yayımlanmış release-v0.5.3 UI smoke geçti: gerçek tab tıklaması, ölüm muhasebesi, ilk engeller toplamı, modal girdi, checkpoint/RNG değişmezliği, geçmişte0gelecekölüm/doğum. Mevcut UI regresyonları da geçti. Publish0hata; life.png gerçek6000tick/37canlı/24doğum/152ölüm ekranı gözle incelendi. Capture --ecology seçeneği. Artefaktlar artifacts/v0.5.3; önceki18koşu deney paketi yeni ZIP'e dahil. Yeni kaynaklardan sonra Core testi tekrarlanmadı, değişmedi.

Sıradaki öncelik su erişimi ve temas/çatışma darboğazlarını deneylerde ayırmak veya yoğunluk gözlem katmanları. Core değiştirmeden görünür teşhis sağlandı; ekosistem dengesi henüz çözülmüş değil. Kayıt/model geçiş stratejisi olmadan yeni fizik yayımlama. Alt ajanlar hâlâ tamamlanmış, çalışan işlem yok.

## 15 Eylül 15:32 UTC heartbeat — su/ısırma etken deneyi

`artifacts/bottlenecks-2026-09-15/` tamamlandı; ayrı `outputs/EVOSIM-bottleneck-experiments.zip` içinde taşınabilir harness, dondurulmuş Core ve önceki altı referans sonucu,24yeni JSON, CSV/analiz/rapor/grafik/SHA manifesti var. Üretim v0.5.3 kaynağı ve ZIP aynı kaldı; çalışma henüz uygulama ZIP'inde değil. Altı tohum × dört koşul ×24.000tick, toplam576.000tick. Referans, yalnızBiteDamage0, yalnızWaterDrain.19 ve ikisi. Isırma maliyeti/sinyali ve hareket su kaybı devam eder; bu deney bütün saldırıları kapatmıyor veya toplam su kaybını yarılamıyor.

24koşulda aynı kurucu hashleri, yalnız iki istenen konfigürasyon farkı, .001mutasyon, mevcut CoreSHA,504örnekte teşhis öncesi/sonrası tam checkpoint eşitliği, nüfus muhasebesi ve ilk engel bölümlenmesi doğrulandı. Altı referansın başlangıç/son hashleri önceki bağımsız çalışmayla aynı; yeni ölçüm geleceği değiştirmemiş. Grafik incelendi, ZIP CRC ve her dosya eşitliği doğrulandı.

600.sn nüfus ortancası referans14, hasar0:36.5, yarı temel su26, birlikte65.1200.sn sırasıyla0/3/1.5/6. Toplam doğum135/280/149/350; en yüksek nesil1/3/3/3. Birlikte müdahale de uzun vadeli nüfus düşüşünü çözmüyor, öğrenme kanıtı yok.300.sn referanstaki47düşük-sulu bireyin yalnız1'i kıyı sorgusunda; tek anlık örnek, hiç içmedi anlamına gelmez.

Sonraki dilim için somut öneri: bireysel enerji/su/temas geçmişini salt okunur örnekleyip geriye dönük inceleme; canlı niyetini uydurma. Geçmişte geçmişe sızıntı ve kayıt maliyeti için tasarım gerekli. Alternatif deney presetleri olursa değişen kuralları açık isimle göster; hasar0 koşulunu dengeli varsayılan ilan etme. Kullanıcıya görseldeğişiklik yok diye tekrarlanan oyunSS'i gönderilmedi. Aktif ajan/işlem yok.

## 15 Eylül 16:33 UTC heartbeat — v0.5.4 gözlem paketi

Root ObservationExporter.cs ve ExportSmoke.cs ile anlık export'u tamamladı. Gözlem şeması evosim.observation.v2; ayrı benzersiz klasörde report.json/population.csv/creatures.csv/integrity.json. Ölüm zamanı/nedeni (yaşayanda null/boş), ParentA/B, nesil, güncel diet vs kurucuGroup, enerji kapasitesi, gerçek üreme ve ölüm teşhisi, kaynak kayıt adı/kimliği ve earliest/view/head tickler. EskiJSON ve dünya kayıtları değiştirilmedi. Tam dosyalar aynı klasörde .partial staging'den Directory.Move ile yayımlanır; yakalanan hata temizlenir, zorla kapanmada .partial kalabilir; fsync dayanıklılığı iddia edilmez. Rapor devam kaydı veya sürekli bireysel zamanserisi değil.

Gerçek yayımlanmış UI smoke geçti. Yeni testler: gerçek1800tickdoğum/ölüm/soy alanları; tamcheckpoint/RNG değişmezliği; aynı tickte farklı paket kimliği; tr-TR altında TextFieldParser ile virgül/tırnak/yeni satır/UTF8 ve tüm sütunlar; livingdeathboş; dosyaSHA; yapayNaN serileştirme hatasında completedpaketyok/stagingtemiz; geçmiş0tick165kurucu/0gelecekölüm/doğum ve headcheckpointkorunması. Başlangıçtaki testfixture değişiklikleri geri alınıp checkpoint eşitlendi. Pythoncsv ile gerçek örnek ayrıca sayıldı; artifacts/v0.5.4/example-report içinde. artifacts/v0.5.4/screen-check.png gerçek yayınUI'dan incelendi; görünüm aynı olduğu için kullanıcıya tekrarSSyok. Son publishuyarı/hata yok; Core50e63... aynı, biyoloji/replay testleri gereksiz tekrarlanmadı.

Yeni biçim docs/OBSERVATION-FORMAT-v2.md içinde ayrıntılı; varsayılanlar ve eski çalışan sürüm korundu. v0.5.4 paketi önceki iki deney arşivini de içerir. Sıradaki önemli iş bireysel enerji/su/temas geçmişi veya harita yoğunluk katmanları; sürekliliği bozan mekanik değişiklikten önce kayıt/replay geçişi şart. Aktif alt ajan ve süreç yok.

## 15 Eylül 17:34 UTC heartbeat — v0.5.5 harita katmanları

Root MapLayers.cs/MapLayerView.cs/MapLayerPanel.cs/MapLayerSmoke.cs ekledi; WorldView artık partial.8×8bölge canlı/ot+meyvehamstok/kara-nemortalaması. FruitPosition ile gerçek meyvekonumu; su hücresi nemortalamasına katılmaz, tam su bölgesiNaN çizilmez. Canlı/besin0–anlıkglobalmax, nem0–1; üst sınır ve anlam solüstlegende. V veya menü Katman döngüsü, ×kapalı. Normal3sabitkontrol korunur; legendclick/wheel/rightdrag haritaya sızmaz. CacheWorld/tick/mode/drought; geometrytekSDLçağrısı, varlıklarüstte. Katmanseçimi oturumgörseli, disk/kaydaeklenmedi.

YayınUI smoke passed:128/192/256toplamlar, kaynakstokkorunumu, sınır7.999/8, ölü/boşnüfus, nemaralığı, tamcheckpoint/RNG, Vmodları/×, legendgirdi, geçmiş0tick165canlı/headkorunumu. Öncekiregresyonlarveexporttestlerigeçti. Üçcapturegerçek256/500/350seed42/1200tick:480canlı33doğum53ölüm; layer-1/2/3.png görselincelendi. Katman1max4canlı, katman2max298.77hamenerji. Nativewindowtitle eski0.5.2kalmıştı0.5.5düzeltildi; ProgramprintveObservationExporter.versionda0.5.5. İlerideüçversiyonalanınıbirliktegüncelle. Core50e63aynı.

Release-v0.5.5vepaket ayrık yayımlandı, eskilerkorundu. Kalanbaşlıcaiş bireyselenerji/su/temasgeçmişi, kontrollükarşılaştırmaUIveyaCoregeçiştasarımısonrasıekolojikgeliştirme. Katmanlarhazıroldu; yenidenyazma. Hiçbiryeni davranış/mutasyon/fizik yok. Aktifajan/süreçyok.

## 15 Eylül 18:34 UTC heartbeat — v0.5.6 kitaplık sıralama/filtre

Root önce kitaplıkta nüfus/süre zaten olduğunu gördü; bunları tekrar eklemedi. WorldLibraryQuery/Controls/QuerySmoke ile salt okunur metadata üzerinden Recent/Name/Population/Time sıralaması ve All/Living/Empty filtresi ekledi. Türkçe StringComparer, eşitlikteId kararlılığı, sayfa0 ve slotActions temizliği. Ana menü ve dünya içi kitaplıkta aynı oturum tercihleri; boşsonuç ve eşleşen/toplam. Canlıyok etiketi başlangıçboşdünyayı da kapsar. Filtre silinenler listesinde de işler; no-schema-change.

Yayımlanan UI smoke passed: Türkçe adlarÇ/I/İ, nüfus/süre/tarih/tie, boşliste, kaynakliste/metadata değişmezliği, gerçekmainmenuveinworldkontroller, hash/metadata korunumu. Öncekiregresyonlar geçti. artifacts/v0.5.6/library.png gerçekizole3kayıtlıUIsmoke menüsü; görselincelendi. Kullanıcıkayıtlarıtesttekullanılmadı. Core50e63...aynı, WorldStorekaynağıveşemasıdeğişmedi; sürümProgram/Native/Exporter0.5.6.

Sonraki öncelikler bireyselenerji/su/temasgeçmişi ve/veya ekolojik darboğazlar. Kitaplık filtreleri artık hazır; tekraruygulama. Tümdilimsafe release-v0.5.6, eskilerkorundu. Aktifajan/süreçyok.

## 15 Eylül 19:34 UTC heartbeat — bireysel geçmiş aracı temeli

Üretim uygulaması v0.5.6 olarak kaldı. Ayrı `artifacts/individual-trace/` ve `outputs/EVOSIM-individual-trace.zip`: .NET10 CLI/harness, dondurulmuş mevcutCore+TimelineSession/Archive/WorldStore, gerçekizoleörnekkayıt, JSONizleri, grafik ve testler. Arayüzdüğmesi henüz yok. IndividualTrace.Build özel headkopyası+arşivde replay yapar; mevcutcallerhead/view/disk değiştirilmez. Varsayılan20tickörnek; doğum/ölüm değişimleri veendpoint ızgaradışıiseayrıca. Existsfalse doğumöncesi; ölümsonrası alanlar son durum. Enerji/maxenerji/su/sağlık/konum/yaş/sonAction/deathfields ve herörnekteWorldStateHash. Ayrı temas/içme günlüğü yok; etiketörtülmesi ve netdeğişim neden payı iddiaedilmez.

6000tick ileri koşuda heradımhash ve yavru166beden/konumölçümü bağımsızkaydedildi. Replayörnekleri, tam492tickdoğum/4001tickölüm,37tickgrid vehead-only,gelecek/eskioriginreddi,callerfullcheckpoint/RNG/archive eşitliği geçti. Aynı600ticktekuraklıkaç/kapa,1200aç/2400kapa replaydoğru. CLI gerçekizole.evoworld'dan çalıştı,kaynakSHAaynı,mevcutçıktıyazılmadı,geçersizgelecekteçıktıyok. Ek1000–2000pencerefullexampleileaynı,20.001düzenliörnekisteğiboundsreddiyle doğrulandı. Ençok200.000span/20.000regularsample; seekhazırlıkmaliyeti ayrıca. Sonbuild0uyarı/0hata.

Örnek303nokta: #166,24.60sn doğum,200.05sn ölüm; örnekgrafik incelendi. example-trace.png oyunSS'i değil ölçümgrafiği. ZIPdosyaları+CRCvefrozeninputsüretimkaynaklarıeşitliği doğrulandı. Mevcutv0.5.6 ZIP değiştirilmedi; buayrıaraçorada henüzyok.

Sıradaki mantıklı dilim: canlıkartından iptal edilebilir, kareler arasına bölünmüş traceişi; üst sınır mevcut DisplayWorld.Tick olmalı, gelecekgizliliğikorunmalı. Şuandaki Build senkronCLI ve head aralığına göre; doğrudanUIbutonunabağlamak kilitler. Önce cooperativejob API tasarla,sonrapanel. Kaynak snapshot'ı sabitle,başlangıçpenceresi ilkcanlıysa doğumetiketini uydurma. Aktifajan/süreçyok.

## 15 Eylül 20:35 UTC heartbeat — v0.5.7 oyun içi yaşam izi

Root LifeTraceJob/Panel/Smoke ekledi. CanlıAyrıntı kartındaYaşamizi; modalShow sourcecurrentViewTick üst sınır, maxson24000tick; kaynakHeadSave+archiveSerialize immutablebytes. Pump6ms/100step hedefi; özelTimeline replay ve mevcutgörünümü değiştirmeden örnek.20tick+birth/death/end, readonlypoints; cancelrefs/points temizler. Hazırlık snapshotserialize/decode/arşivload bölünemez,6msgarantisi yok; docs net. İlkcanlıpunktbirthdiyeetiketlenmez; ölümsonrasıçizgileruzatılmaz. Panelkapanınca pausedkalır.

6000tickbağımsızilerikoşuyavru166heradımEnergy/Water/Health/XY ölçümleriylebirebir; tamdoğum/ölüm,7stepPumpcap, gerçekkartbutonu, modalD/space/V engeli, Escapeclosevebaşlatılmışişcancel, futuredeathisolation1200tick, displayedWorldhash/headfullcheckpoint/RNG/archiveaynılığı yayında doğrulandı. MevcuttümUIsmokegeçti. trace.png gerçekseed42defaultiklim6000tick37canlı24doğum152ölüm,166açlıktanölmüş. CLIörneğindekiiklimmüdahalelerinormalcapture'dayok,sonuçlarıkarıştırma.

Native/Program/Exporter0.5.7; Core50e63...aynı. Capture --trace otomatikTimelinebaşlatır ve seçiliIDpanelinihazırlar; --select166 --ticks6000. YeniUIgörselincelendi. Release0.5.7/ZIP öncekiCLIaraç arşivini de içerir. Snapshotperformans/başlangıçcancellationdahaasenkronhalegelebilir; tamtemas/içmeolaylogu yok. Sonraki iş gerçekmekanikdarboğazaraştırması veya yaşamizindeseçiliörnekdetayı/başlangıçmaliyeti; mevcutpaneli tekrar yazma. Aktifajan/süreçyok.

## 15 Eylül 21:35 UTC heartbeat — v0.5.8 yaşam izi etkileşimi

LifeTracePanel TracePointAt en yakın gerçek örneğe tutunur; hover ile sayısal değer/zaman/konum halkası. Doğum öncesi Exists=false, ölüm sonrası son beden durumu açık. Sağ kenar W-1 paydasıyla ToTick son örneğe erişir. Mouse click örnek Tick değerini kopyalar, ShowTimeline paneli/cancel job kapatır, RequestTimelineSeek tam hedefe gider. Yeni Core Step/branch yok; grafik izinin mevcut ToTick sınırı korunur.

Yayın UI smoke yeni kontrolleri geçti: prebirth hover, dead end/right edge6000, geçmiş1200 içinde hover, click timeline/tam örnek hedefi ve HeadWorld full checkpoint aynı. Önceki bağımsız trace/iptal/replay ve tüm UI regresyonları geçti. hover.png gerçek130sn Enerji%2.2/su100/sağlık74.2; konum halkası görsel incelendi. CLI --trace-hover-tick2600. Program/Native/Exporter0.5.8; Core50e63 aynı.

Release-v0.5.8 ve ZIP bağımsız; eskiler korundu. Bu küçük görsel etkileşim için art arda benzer SS gerekmez; kullanıcıya özellik ve paket duyurusu yeterli, screenshot kanıtı artifacts altında. Sonraki öncelik snapshot başlangıç maliyeti veya somut ekolojik mekanik araştırması; şema değişikliğinde geçişi düşün. Aktif ajan/süreç yok.

## 16 Eylül — v0.5.9

Yaşam izi ölçümü tamamlandı (12 koşu); arka planda özel dünya replay, değişmez sonuç yayını ve iptal eklendi. Yayınlanmış uygulamanın UI testi, bağımsız ileri koşu eşitliği, kaynak kayıt değişmezliği ve geçmiş izolasyonu geçti. Core değişmedi. Detay: RELEASE-v0.5.9.md. Kalıtım beden genleri yanında 357 karar ağı ağırlığını içerir; yaşam boyu ağırlık öğrenmesi/anı aktarımı mevcut değildir.

## 16 Eylül 2026 — Blender + Unity görsel katman kararı

Kullanıcının son tasarım metni doğrultusunda gerçek 3D modeller Blender'da, görsel dünya Unity URP'de geliştirilecek. Biyoloji mevcut C# Core'da kalacak; hareket iki boyutlu, görsel kamera eğik ortografik. Ayrıntılı geçiş listesi `../EVOSIM-3D/README.md` paketinde.

İlk Blender sahnesi ve sekiz FBX model üretildi; FBX geri aktarımı geçti. Runtime bağlantı kodu Unity kütüphanelerine karşı derlendi. Unity Editor kuruldu, ancak geçerli lisans bulunmadığından Editor derlemesi/sahne çalışması doğrulanamadı. Bu işler tamamlanmış Unity geçişi olarak sayılmayacak.

Öncelik: sanat sahnesi kalitesi ve okunabilirlik → Unity/Core durum eşitliği → kayıt/geçmiş → arayüz eşitliği → büyük harita performansı. Ağaç saydamlığı, elmalar, LOD, su/gölge iyileştirmeleri açık. Takvim, otçul/etçil olay etiketleri ve dört konumlu panel yığınları SDL aday kaynakta korundu; yayın v0.5.9 olarak kaldı.

## 3 Ekim 2026 — Unity çalıştırılabilir 3D önizleme

Unity Linux uygulaması derlendi ve çalıştırıldı. 35 otçul/5 etçil/80 ağaç; 560 gerçek meyve yuvası görünümü, örneklenmiş ot besin hücreleri, gerçek kaya engelleri, eğimli kıyılar, modele uygun yükseklik ve korunmuş FBX kök dönüşümü. Örtüşen yapraklarda yaklaşık ekran tabanlı saydamlık; atılganlık rengi/yaş doygunluğu; kamera gezinme, canlı kartı, 1–20× ve gün/yıl gösterimi.

400 adımlık bağımsız Core karşılaştırması 30/60/144 FPS zaman girdilerinde ve 1×/20× hızda geçti; pause/backlog ve 233 kıyı köşesi kontrol edildi. Son standalone 103 tick sonunda Core hash eşitliği, 40 canlı görünümü, 560 meyve görünümü, ölçek ve zemin yüksekliği kontrolünden geçti. Test GPU'su Intel ARL; 5090 performans iddiası yok. Core DLL kararlı v0.5.9 ile aynı. Gerçek uygulama görüntüsü EVOSIM-3D/previews/unity-player.png.

Bu bir göç önizlemesi: kaydetme/kitaplık/geri sarma, kapsamlı dünya ayarları, çoklu gözlem panelleri ve gelişmiş sanat/animasyon/LOD henüz Unity'ye taşınmadı. Üretim başlatıcısı v0.5.9 olarak kaldı. Sonraki öncelik kayıt/ayar/panel eşitliği ve organik kıyı-bitki görselleri.

## 3 Ekim — daha yumuşak kıyılar

Kıyı hücreleri kenar başına 4 alt parçayla çiziliyor; kara/su sınırı üçgenler üzerinde kesilerek büyük kare köşeleri dar (0.28 hücre) bir bantta yuvarlatılıyor. İç bölgeler kaba parçalarda kalıyor. Aynı ince yüzey enterpolasyonu canlıların/bitkilerin yüksekliğinde kullanılıyor. Nehir güzergâhı, hücre merkezlerindeki kara/su sınıflaması ve biyoloji değişmedi. Seyrek küçük kıyı taşları görsel dekor olarak eklendi; bunlar Core'daki engel kayalarından ayrıdır.

Geometri testi 50.293 üçgende geçti; yüzey yüksekliği hesabının en büyük hatası 0.0000241 görsel birimden küçük. Bütün hücre merkezleri ve Core hash'i korundu. Yeni gerçek uygulama görüntüsü: `EVOSIM-3D/previews/unity-soft-shore.png`.
