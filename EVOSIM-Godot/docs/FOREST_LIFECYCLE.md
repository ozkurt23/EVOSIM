# Yaşayan orman — ekoloji modeli v2 tasarım sözleşmesi

24 Ekim 2026. Kullanıcının yeni isteği: ağaçların ömrü, yer değiştiren orman dağılımı, ceset kaynaklı toprak verimi, kuraklıkta ölüm ve aşırı çoğalmayı engelleyen denge. **Henüz çalışan simülasyona eklenmedi.** Bu belge uygulanacak mekanik ve kayıt uyumluluğu sözleşmesidir; mevcut v1 Core değişmemiştir.

## Mekanik kararları

- Ağaç yürümez. Ölüm, tohum yayılması ve yeni fidanların tutunması ormanın konumunu zamanla değiştirir.
- Evreler: fidan → erişkin → yaşlanma → ölü gövde → ayrışma. Fidan hemen meyve vermez. Verim, olgunluk ve sağlıkla ölçeklenir. Yaşlılık ömrü0 ise yaşlılıktan ölüm kapalı; kuraklık bağışıklığı anlamına gelmez.
- Su yetersizliği birikimli stres oluşturur. Uzun stres sağlığı düşürür; yağış yaşayan ağacın toparlanmasına yardım eder, ölmüş ağacı diriltmez. Genç fidanın tutunması erişkinden daha kırılgandır. Tüm bu süreçler simülasyon zamanı ile ilerler.
- Ceset doğrudan ağaç oluşturmaz. Yalnız ayrışan, henüz yenmemiş kısmı yakın karasal hücrelerde geçici mineral/verim katkısı oluşturur. Yenen et aynı anda toprağa da yazılmaz; cesedin süre sonunda kalan kısmı yalnız bir kez işlenir. Suya düşen ceset için bu ilk sürümde karaya besin ışınlanmaz.
- Hücresel verim üst sınırla doyar, zamanla azalır ve yeni bitki gelişiminde tüketilir. Aynı noktada100ölüm100kat verim yaratmaz. Başlangıç toprağının verimi vardır; cesetler tek besin kaynağı değildir. Tam fiziksel karbon/azot döngüsü iddiası yoktur.
- Tohum uygun karaya ulaşmalı, yeterli su/verim bulmalı ve çevresinde gelişme alanı olmalı. Yakın erişkinler sınırlı mesafeye tohum yayar. Başlangıçta sonlu, kaydedilen tohum bankası bulunabilir; tamamen yok olmuş orman sınırsız görünmez tohumla kendiliğinden geri gelmez.
- Yerel taç/ kök rekabeti tutunmayı ve verimi azaltır; ağaç aralığı ve uygun kara miktarı taşıma kapasitesini belirler. Hedef ağaç sayısına ulaşmak için ağaç silme/üretme yapılmaz. Teknik kayıt sınırı ekolojik denge mekanizması değildir.
- Karakterlerin mevcut ağları değişmez: yiyeceğin/engellerin yerinin ve miktarının değişmesi duyularına yansır. Hazır göç/orman arama/kaçış davranışı eklenmez.

## Kurulum:7temel ayar ve bir aç/kapat seçeneği

Aşağıdaki başlangıç değerleri deney profili önerisidir; dengesi ölçülmüş sonuç veya gerçek dünya biyolojisi değildir.

| Ayar | Önerilen deney başlangıcı | Kontrol / anlam |
|---|---|---|
| Yaşayan orman | Yeni v2 dünyada açık | Eski v1 dünyaların davranışı değişmez |
| Ağaç ömrü |60yıl | Sayısal;0yaşlılık ölümü kapalı. Bireyler etrafında kayıtlı küçük ömür çeşitliliği |
| Meyve verecek olgunluk |3yıl | Sayısal; fidan büyüme süresi. Ömürden uzun seçilmesi meyvesiz orman deneyi yaratabilir; açıklanır |
| Kuraklık dayanıklılığı |0.6 |0–1sürgü; sağlığın su açığına toleransı, anlık ölüm olasılığı değildir |
| Tohum yayılımı |0.2 |0–1sürgü; sabit günlük azami deneme oranını ölçekler, kare başına olasılık değildir |
| Orman yoğunluğu |0.5 |0–1sürgü; yerel rekabet/taşıma kapasitesini ölçekler; global zorunlu hedef sayı değildir |
| Cesetten toprağa katkı |0.4 |0–1sürgü; gerçekten ayrışan biyokütlenin verime dönüşüm katsayısı |
| Toprak katkısının kalıcılığı |2yıl | Pozitif sayısal yarı ömür;0anında kayıp. Yıllarca sınırsız birikme yok |

Mevcut başlangıç ağaç sayısı, aralık, meyve adedi/enerjisi ve meyve yenilenme ayarları korunur. Bu ayarlar yeni ömür/sağlık süreçleriyle çarpılır, ikinci bir bağımsız meyve sistemi kurulmaz. Ayrıntılı yayılma yarıçapı, stres/toparlanma süreleri ve minimum filizlenme koşulları model sabitleri olarak belgelenir; gerekirse gelişmiş ayarlara alınır. Normal simülasyon ekranına yeni karar düğmesi eklenmez.

Profiller: dengeli orman; hızlı gözlem (8yıl ömür,0.5yıl olgunluk); kırılgan/kurak orman; yoğun ve yavaş orman; yaşlanmayan ağaçlar. Bunlar sonuç garantisi değil deney başlangıcıdır. Kuraklık1day→toplu ölüm gibi ani olaylardan kaçınılır.

## Kod incelemesinden gerçek entegrasyon noktaları

- `EvoSim.Core/Terrain.cs`: mevcut Tree yalnız X/Y/Radius/Fruit/Regrow içeriyor, kalıcı listede; gövde hücresi rock ile aynı Tiles=2. Yeni ölü ağaç temizliği eski kaya hücrelerini yanlışlıkla açmamalı. Kalıcı kaya zemini ile dinamik gövde doluluğu ayrılmalı; yol bulma/bağlantı önbelleği güncellenmeli.
- `Simulation.cs`: EcologyStep20tickte bir çalışır; arayüz takviminde bu1gündür. Ceset ayrışması her tickte; ceset yiyeceği tükenince veya ömrü bitince listeden çıkar. Toprak katkısı bu kaybın gerçekleştiği yerde bir kez işlenmeli. Render kareleri/duvar saati kullanılmaz.
- Kaynak erişimi listeler ve indeksler kullanıyor. Kalıcı ağaç kimliği eklenmeli; listeden silince başka ağacın meyvesini hedefleme/kart gösterme olmamalı. Yaşayan/ölü ağaçlar için açık kayıt politikası ve indeks eşlemesi gerekir.
- `Checkpoint.cs`: alan adları/türleri kesin şemalı; mevcut kayıt otomatik yeni alan kabul etmiyor. Yaş/sağlık, toprak, tohum bankası, ağaç kimlik sayacı, ayrı ekoloji RNG durumu ve dinamik gövde alanı yeni sürümde tam kaydedilmeli ve StateHash'e katılmalı.
- `TimelineArchive`: CoreDLLözeti ve checkpoint sürümü eşleşmezse replay reddedilir. Bu koruma gevşetilmez. Mevcut dünyaya sessiz model yükseltmesi yapılmaz. Eski motor/arşiv çalıştırma yolu korunarak seçili andan ayrı v2dal oluşturma ve yeni köken kaydı gerekir. V1geçmiş v2kurallarıyla tekrar yürütülmez.
- Godot tree MultiMesh/fruit/edible-ground/minimap ve terrain cache invalidasyonu bu yeni durumdan beslenmeli. Her ağaç ölümü bütün48×48sahneyi yeniden üretmemeli. Büyüyen gövde canlıyı aniden hapsedemez; yerleşim ve geçiş politikası test edilmeli.
- Yeni fidanlar küçük/seyrek, sağlıklı erişkinler mevcut sanat dilinde; kuruyan taç seyrelir, ölü gövde meyvesizdir ve süreli kalır. Renk tek gösterge olmaz. Seçili ağaç kartına evre/yaş/sağlık eklenir; HUD aynı kalır.

## Uygulama sırası ve kabul

1. V1Core+arşiv açma yolunu sabitle; v2durum şeması ve ayrı köken/dal aktarımı. Kullanıcı kayıtlarını yalnız kopya fikstürlerle test et.
2. Renderer bağımsız sabit adımlı ağaç/soil/seed modeli, ayrı RNG; disabled modda v1çıktıları eşdeğer. Yeni formatın kesik/bozuk/aşırı kayıtlarını reddet.
3. Ölüm/ayrışma/verim, büyüme/meyve, dinamik engel ve gerçek duyulara bağla. V1arşivi üzerinde yazma yok.
4. Kaydet/yükle+ileri sarma hash eşdeğerliği, geçmişte gelecekteki ağaç sızmaması, dal izolasyonu.
5. Seed42/73/123ile normal/kurak/yağışlı/yoğun ölümlü test;1×/20×aynıtickte aynı sonuç. En az100simülasyon yılı: ağaç sayısı/yaşları, doğan-ölen ağaçlar, mineral stokları, meyve, popülasyon ve CPUmaliyeti raporu. Çöküş de geçerli deney sonucu; gizli nüfus kurtarma yok.
6. Sıfır ağaç/tohum, tüm ağaç ölümü, su üzeri ceset, tamamen yenmiş ceset, üst üste ölümler, dolu alan, sınır hücreleri, fidan üstünde canlı, ölü ağacı inceleme ve geri sarma ayrı testler.
7. Gerçek uygulamada sağlıklı/kuruyan/ölü/fidan sahneleri, seçili kart, yağmur ve kuraklık görüntüleri; mevcut davranışların ve minimal UI'nin korunması.

Bu aşamalar tamamlanmadan özellik 'çalışıyor' olarak işaretlenmez. Filizlenme/ölüm oranları testlerden sonra ayarlanır;60yıl veya0.2gibi başlangıç önerileri bilimsel kalibrasyon değildir.

### Uygulama ilerlemesi — 24 Ekim, iterasyon41

Bağımsız ForestEcology modülü ve29test artık uygulanmış durumda; çalışan oyuna henüz bağlanmadı. V1koruma aracı önceki iterasyonda hazırlandı. Tam dünya kaydı, gerçek ceset/meyve/engel bağlantısı ve görseller açık. Sayısal sonuçlar ../EVOSIM-Models/forest-verification41.json dosyasında;8×8bağımsız fikstürler tam ekosistem deneyi değildir.
