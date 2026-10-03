# EVOSIM — Ana geliştirme planı

**Revizyon:** 15 Eylül 2026 · Terra Nil sanat yönü, büyük harita, dünya geçmişi ve gözlem araçları.
**Çalışan sürüm:** v0.5.9, C# çekirdek + yerel SDL2 gözlemci. Biyolojik çekirdek v0.4.1/v0.5.0 ile aynıdır.
**Bu belgenin durumu:** Uygulama planı. “Planlandı” maddeleri henüz oyunda değildir.

Bu belge güncel uygulama sırasını ve kabul koşullarını belirler. `docs/PLAN-v0.2.1-ARCHIVE.md` önceki çalışan model sözleşmesini; `docs/PLAN-v0.1-ARCHIVE.md` ilk ayrıntılı tasarımı korur. Yeni gereksinimlerde bu belge önceliklidir. `docs/FEATURE-BACKLOG.md` bütün F01–F72 hedeflerini takip eder; hiçbir fikir yalnızca arşivde unutulmaz. Kaynak dosyalarda yapılmış olmak, bir maddenin doğrulanmış olması demek değildir.

## 1. Ürün hedefi ve değişmez kararlar

EVOSIM, kullanıcının koşullarını seçtiği dünyalarda canlıları, soyları ve ekolojik sonuçları uzun süre gözleyebildiği kişisel bir yapay yaşam laboratuvarıdır. Görsel olarak keyif vermesi, anlaşılabilirliği ve kayıt güvenilirliği temel işlevlerdir.

- Dünya rastgele üretilir; kullanıcı harita çizmek zorunda kalmaz.
- Kamera yer düzlemine yaklaşık 85–90° bakar. Son kullanıcı tercihiyle dört yön kontrolü ilk kapsamdan çıkarıldı; örtülme saydamlık ve konturla çözülecek.
- Normal gözlemde harita baskındır. Sürekli açık ayar duvarı, metin yığını veya etiket kalabalığı olmaz.
- Terra Nil'e yakın doğal palet, boyanmış hissi veren yüzeyler, katmanlı bitki örtüsü ve sakin hareket hedeflenir. EVOSIM'in özgün assetleri bu görsel dilde hazırlanır.
- Her biyolojik mekanik bir durum, kapasite, maliyet, kaynak veya duyuyu somut olarak etkiler. Her gözlem aracı gerçek veriyi gösterir.
- Kalıtsal özellik, geçici beden durumu, dünya kuralı ve görsel tercih farklı veri sınıflarıdır.
- Hazır “avcıdan kaç”, “yavruna koş”, “eşe git”, “sürüye katıl” politikaları eklenmez. Motor kararlarını kalıtsal ağ verir. Bunlar gözlenebilecek sonuçlardır.
- Açlık/susuzluk sinyali ve ihtiyaç anında erişilen kaynağı tüketme refleksi, kullanıcının istediği açık beden kurallarıdır.
- Varsayılan mutasyon doğum başına toplam %0,1 kalır. Arayüzde yüzde gösterilir. Gizlice yükseltilmez; düşük mutasyonun yakınsamayı veya zekâyı garanti ettiği söylenmez.
- Eşeyli kalıtımda her sayısal değer ebeveyn aralığından sürekli rastgele seçilir; nadir mutasyon aralığın dışına çıkabilir. Bu basitleştirilmiş modeldir; insan genetiğinin birebir temsili olarak sunulmaz.
- Görsel büyütme, kamera, animasyon ve UI simülasyon sonucunu değiştirmez.
- Ölümsüzlük şu anki anlamını korur: yaşlılıktan ölüm kapalı olabilir; açlık, susuzluk ve hasar geçerlidir. Yaşlanmayı kapatmak ayrı ayardır.

## 2. Mevcut durum: ne gerçekten çalışıyor?

| Sistem | v0.5.2 durumu | Sonraki eksik |
|---|---|---|
| Yerel uygulama | C# .NET Standard 2.1 Core, SDL2 pencere | Modüllere ayrılmış UI, görsel üretim hattı, Unity sunum katmanı |
| Harita | 128²/192²/256², rastgele seed, düzeltilmiş 1–5 kara bölgesi, ağaç/kaya/çayır ayarları | 512² ölçümü, yükseklik, biyomlar, gelişmiş su |
| Karar | 30 girdi, 8 tekrarlayan birim, 5 çıktı, 357 kalıtsal ağırlık | Genişleyen duyular/eylemler için sürümlü şema; davranış deneyleri |
| Kalıtım | Altı beden geni, eşeyli aralık kalıtımı / eşeysiz kopya, düşük mutasyon | Kalıtsal güdü genişlemesi, gen dağılımları ve karşılaştırma |
| Beden | Enerji, su, sağlık, gelişim, yaş etkileri, grup bazında ömür | Daha zengin ihtiyaçlar ve açıklanmış ayarlar |
| Kaynak | Kırmızı meyve, sınırlı çayır, süreli ceset; taç/mesafe/nem/meyve halkası/kuraklık ayarları | Ağaç türleri, yaşam döngüsü, mevsim, tohum |
| Üreme | Grup bazlı kapalı/eşeyli/eşeysiz; güdü girdisi; tam bedel; anlık engel tanısı | Güdünün yeni kalıtsal şemaya geçişi, yatırım/bakım araştırması |
| Kayıt | İsimli kayıtlar, RNG dahil devam, atomik dünya+geçmiş, yedek; eski kayıt ayrı sürüm dalına açılır | Çok uzun geçmişin depolama ve kayıt maliyeti |
| Zaman çizelgesi | Kaydedilmiş herhangi bir adıma geri/ileri sarma, iptal, kayıtlı geleceği oynatma ve ayrı dünyada dallanma | Yaşam filmi, deney karşılaştırması, gelişmiş dal görünümü |
| Arayüz | Açık tema, bilgi baloncukları, sürgüler/scroll, önizlemeler, sil/geri al, minimal HUD; bağlamsal soy/gen/kaynak incelemesi | Kontrollü deney ve karşılaştırma araçları |
| Kamera | Pan, zoom, takip; tüm canlılarda yumuşak taç saydamlığı ve kontur | Büyük haritada parça çizimi / LOD |
| Hız | 1–20× hedef; gerçek hız göstergesi | Büyük dünyada bütçe, uzun süre ölçümü, koşullu GPU |
| Analiz | Nüfus grafiği, yerel tıklanabilir aile grafiği, benzersiz soy sayıları, altı gen histogramı, kaynak kartları, JSON/CSV, başsız runner | Küresel soy görünümü, dağılımların zaman içindeki değişimi, kontrollü karşılaştırma, yaşam filmi |
| Afet/yapı | Aç/kapat kuraklık; hazır korunak | Mevsimler, sel, yangın, inşa ve materyal |
| Öğrenme | Yaşam içinde ağırlık güncellemesi yok; geçici ağ belleği var | Ayrı, açıkça etiketli öğrenme araştırması |

Son v0.2 kontrolleri: 14 çekirdek grubu ve v0.2.1 menü akış testi geçti. Seed42, 600 simülasyon saniyelik normal koşuda 165 başlangıçtan 18 canlı kaldı, 24 doğum görüldü; kuraklık koşusunda 13 canlı ve 27 doğum. Bunlar uzun vadeli denge veya beceri evrimi kanıtı değildir. İlk kalite işi, nüfus kaybını ve üreme darboğazlarını daha iyi ölçmektir. Başarı ölçütü her dünyayı zorla yaşatmak değildir.

**v0.3–v0.4 kabul kaydı:** v0.3 görsel/kurulum/silme paketi tamamlandı. v0.4.0'da 18 çekirdek test grubu geçti; eski sürümden iki fixture 400 adım sonra tam kayıt verisiyle aynı geleceği üretiyor. Yeni üreme/ağaç ayarları ve ayrı sürüm dalı uygulandı. v0.4.1 ile 128/192/256 boyutları, kaynak indeksi ve üretici sürümü geçişi tamamlandı; 23 Core grubu ve 225 yeni harita kombinasyonu geçti. Ek yoğunluk presetleri ve daha kapsamlı ağaç yaşamı hâlâ açık. `RELEASE-v0.4.md` ve `artifacts/v0.4/` kanıtları içerir.

**v0.5.0 kabul kaydı:** Zaman çizelgesi motoru, aynı anda girilen kuraklık komutlarının sırası, geçmişten ayrı dünyaya dallanma ve tek dosyalı LIBRARY-2 kaydı tamamlandı. Bağımsız sabit v0.4.1 Core referansına karşı 10/10 test grubu geçti; 28 geçmiş hedefi ve 1.800 adımlık oynatma tam kayıt baytlarıyla eşleşti. 15 arşiv ve beş dünya kapsayıcısı bozulması reddedildi. Biyolojik model, ağ, genler ve ödül/eylem politikası değişmedi. Ayrıntı ve ölçüm sınırları `docs/RELEASE-v0.5.md` ile `artifacts/v0.5/` içinde. Yaşam filmi, soy grafiği ve kontrollü deney paneli tamamlanmış sayılmaz.

**v0.5.1 kabul kaydı:** Canlı kartında altı gen + açıklamalar ve gerçek hız/görüş kapasitesi, tıklanabilir yerel soy grafiği, nüfusun altı gen dağılımı ve altı kaynak türünün kartları tamamlandı. Soy, nüfus ve kaynak okuma testleri ayrı ayrı 6/6 grup geçti; salt okunurluk, tam checkpoint/gelecek eşitliği ve geçmişte gelecek verisinin sızmaması sınandı. Son yayın UI smoke kaynak/kart/modal/yavru seçimi ve nüfus kartında tıklama sızmaması akışlarında geçti. Core ve timeline kimliği aynı kaldı. Bu dilim küresel soy grafiği, yaşam filmi veya kontrollü deney paneli değildir; `docs/RELEASE-v0.5.1.md` ve `artifacts/v0.5.1/` gerçek kapsamı belgeler.

## 3. Sanat yönetimi: Terra Nil yönü

Referans: [Terra Nil resmî oyun galerisi](https://store.steampowered.com/app/1593030/Terra_Nil/). Palet, bitki katmanları, su/kıyı geçişleri ve sakin atmosfer incelenir. Referanstaki eğik izometrik açı yerine EVOSIM'in neredeyse dik kamerası kullanılır.

### 3.1 Palet ve malzemeler

| Öğe | Hedef |
|---|---|
| Zemin | Birbiriyle uyumlu sıcak yeşiller, seyrek toprak dokusu, büyük ve yumuşak renk geçişleri |
| Su | Mavi-yeşil ana ton, açık sığlık, okunaklı kıyı, düşük kontrastlı akış çizgileri |
| Ağaç | Birkaç büyük yaprak kütlesi; ayrılan gövde/taç/gölge; tek tek aşırı yaprak çizimi yok |
| Meyve | Yaprak üstünde seçilen kırmızı, açık vurgu ve koyu kenar; stokla tutarlı görünüm |
| Çayır | Bölgesel kümeler, dış kenarda seyrelme, besin miktarıyla değişen yoğunluk |
| Kaya | Tekrarlanan küçük gri piramitler yerine farklı ölçekte yuvarlak/kırıklı siluetler |
| Beden | Temiz dış kontur, gençlerde doygun iç renk; yaşta doygunluk kaybı |
| UI | Sıcak kırık beyaz yüzey, koyu yeşil yazı, adaçayı ve açık altın vurgu |

Palet adayları: yüzey #F4F1E7, ikincil #E6EADB, metin #263E35, vurgu #D4B768; zemin #91AA75/#6C926D, su #5B9FA7/#8AC0BD. Bunlar görsel QA sonrası sabitlenecek, değişmez model değerleri değildir.

### 3.2 Asset envanteri ve üretim

- İlk görsel geçiş: toprak, kıyı, nehir/gölet, 3 kaya ailesi, 3 ağaç ailesi, meyve, çayır, sazlık, çalı, iki canlı temel silueti, yavru/yaşlı varyantı, ceset, hazır korunak.
- Başlangıçta farklı ağaç şekilleri aynı mekanik profile bağlı olabilir; ayrı mekanik türler v0.4'te açık özellik tablolarıyla eklenir. Dekor başka, yenebilir nesne başka okunur.
- Her kaynak için boş/kısmi/dolu; ağaçta fidan/yetişkin/kuru/yanmış durumları ilgili mekanik devreye girdikçe üretilir.
- Canlı şekli: mevcut otçul yuvarlak, etçil köşeli ayrımı korunur. Beden/kafa oranı, kulak, kuyruk ve uzuv varyasyonu okunurluk testinden sonra genişletilir.
- Hareket, yeme, içme, dinlenme, darbe ve ölüm için hafif animasyon. Fizik merkezini oynatmaz; kalabalıkta animasyon ayrıntısı azaltılır.
- Düzenlenebilir kaynak + atlas + pivot + gövde ayak izi + seçim alanı + örtülme maskesi tutulur. 2D kaynaklar ilk etapta kod/SVG ve gerekirse özgün raster atlas; 3D aşamasında Blender kaynakları ve instancing uyumlu materyaller.
- Asset dosyası sayısı artarken tek tip ışık yönü, ölçek, kontur ve kırmızı meyve okunurluğu korunur. Her tekil bireye ayrı texture oluşturulmaz; ortak atlas ve parametrik renk/ölçek kullanılır.
- Görsel rastgelelik ayrı akıştır; oyun biyolojisinin RNG'sini tüketmez.

### 3.3 Büyük haritada görünürlük

Haritanın tamamına bakarken yüzlerce farklı beden ayrıntısını aynı anda göstermek mümkün değildir. Yakınlık düzeyine göre üç gösterim planlanır:

1. **Yakın:** gerçek beden, animasyon, meyve stokları ve ihtiyaç ayrıntısı.
2. **Orta:** sade beden silueti, en az 2 ekran pikseli kontur hedefi, düşük dekor yoğunluğu.
3. **Uzak:** küçük ama seçilebilir otçul/etçil işaretleri; çok yoğun bölgelerde isteğe bağlı yoğunluk katmanı. İşaretin büyüklüğü gerçek fiziksel yarıçapı değiştirmez.

Seçili/favori canlı uzakta da işaretini korur. Üst üste işaretlerde aday listesi veya yakınlaştırma kolaylığı sağlanır. Renk hem agresifliği hem yaşı kesin davranış olarak açıklamaz: gen rengi ve gözlenen davranış ayrı bilgi alanıdır.

## 4. Ağaç arkasındaki canlılar: otomatik görünürlük

Son kullanıcı tercihi: örtülme otomatik çözülürse pusula/kuzey/güney düğmeleri gereksizdir. Dört yön ilk paketten çıkarıldı; yalnız ileride gerçek bir gözlem ihtiyacı çıkarsa yeniden değerlendirilir. Kamera yaklaşık85–90° tepeden kalır.

- Taç altındaki **her canlı** için yerel örtüşme hesaplanır. İlgili yaprak bölgesi hafif saydamlaşır; beden konturu görünür kalır. Seçili canlı daha belirgin son katmanda çizilir.
- Yalnız seçili canlı değil, seçilmeyenler de ormanda izlenebilir. Saydamlık geçişi yumuşaktır; sınırda yanıp sönmez. Yoğun ormanda üst üste alfa katmanları renk çamuru oluşturmaz.
- Gövdenin gerçek çarpışma/görüş engeli kalır. Kameranın yaprakları soldurması canlının duyularına görüş üstünlüğü vermez.
- Sadece ekrandaki taçlar ve yakın canlı adayları işlenir; ağaç×bütün canlılar taraması yapılmaz.
- Taç, gövde, meyve ve gölge ayrı katmanlardır. Konturun meyve/beden karmaşası yaratmadığı gerçek ekran görüntüsünde kontrol edilir.
- Dünya→ekran ve ters dönüşüm, seçim/pan/zoom/takip için tek modülde kalır. Görsel büyütme biyolojik konumu veya boyutu değiştirmez.

**Kabul:** yoğun ormanda seçilmeyen canlılar da okunur; 1080p/1440p ve farklı zoomlarda elma, taş ve beden ayrılır; yakınlaşma/kalite/örtülme core hash'ini değiştirmez. Bu sağlanınca yeni kamera düğmesi eklenmez.

## 5. UI ve bilgi baloncukları

### 5.1 Akış

Ana menü → dünya kartları (küçük önizleme, ad, süre, nüfus, son kayıt) → aç veya yeni dünya. Dünya kartının küçük işlem menüsünde Adlandır ve Sil bulunur. Sil, kayıt/metadata/ilgili geçmişi geri alınabilir çöp alanına taşır; kısa “Geri al” bildirimi verir. Kalıcı temizleme çöp alanında ayrı, açık işlemdir. Silme sırasında çalışan kayıt kuyruğu durdurulur; otomatik kayıt silinen dünyayı yeniden oluşturmaz. Bir dalı silmek kardeş/ebeveyn dünyayı veya paylaşılan geçmişi bozmamalıdır; referanslar çözülmeden ortak veri kaldırılmaz. Kurulumda geniş, dikey kaydırılabilir ayar alanı; kategori başlıkları, arama ve presetler. İleri ayarlar erişilebilir kalır. Sayfa sayfa 14 alan zorunluluğu kaldırılır. Simülasyonda harita, küçük zaman kontrolleri ve isteğe bağlı paneller. Dünyadan çık kaydı doğrulayıp menüye döner.

### 5.2 Ayar tanımı tek kaynaktan gelir

Her ayar: kararlı kimlik, Türkçe ad, birim, varsayılan, alt/üst sınır, adım, açıklama, etki, maliyet, bağımlılık, veri sınıfı, değiştirilebilirlik zamanı ve sürüm taşır. UI, doğrulayıcı, preset ve model referansı aynı tanımı kullanır.

- **0–1 özellikler sürgüyle** düzenlenir. Birey grubu için min/max gerekiyorsa aynı çizgi üzerinde iki tutamaç kullanılır; alt değer üstü geçemez. Tam 0 ve 1 erişilebilir. Okunur değer yanında görünür.
- Sürgünün üzerinde fare tekerleği ince ayar yapabilir; imleç dışında tekerlek yalnız sayfayı kaydırır. Klavye okları da ince ayarlar. Sayfa scroll'u kazara genleri değiştirmez.
- Mutasyon gibi küçük olasılıklarda sıfır + düşük değerlere hassas ölçek ve ince adım; %0,1 kolay seçilir. Gösterilen yüzde ile kaydedilen0–1 tutarlı kalır.
- **Ömür, süre, adet, nüfus ve fiziksel boyut** sayısal alan olarak kalır; keyfî sürgü üst sınırına hapsedilmez. Modelin gerçek güvenli aralığı/kapasite kontrolü yine vardır ve açıklanır. 0 ömür sınırsız anlamını korur.
- Simülasyonda ana kontroller duraklat/devam, hız ve tek araç menüsüdür. Analiz, geçmiş ve müdahaleler isteğe bağlı açılır; sürekli karar bekleyen popup veya yeni özellik başına sabit düğme eklenmez.
- Her başlığın yanında küçük **ⓘ**. Yaklaşık 350 ms hover veya klavye odağı sonrası görünür; üzerine tıklayınca sabitlenebilir.
- Hafif saydam açık kart, gölge, yeterli kontrast; yazı saydamlaştırılmaz. Ekran kenarında taşmadan açılır, imlecin okuma bölgesini kapatmaz.
- Klavyeyle Tab odağı, Enter ile sabitleme, Esc ile kapama. Sayısal alanlarda Türkçe virgül desteklenir.
- İlk iki satır: “Neyi değiştirir?” ve “Artırırsan ne olur?”. Gerektiğinde birim/varsayılan ve ilgili sınır eklenir.
- Olasılıklar kullanıcıya **%** ile sunulur; dosyada 0–1 saklanır. %0,1 mutasyonu “0.001” hesaplama zorunluluğu olmadan düzenlemek mümkün olur.
- Hatalı min/max aynı satırda açıklanır; değer gizlice düzeltilmez. Pasif ayarın neden kapalı olduğu yazılır.
- “Sadece görünüm”, “Kurucu gen aralığı”, “Dünya kuralı”, “Canlının anlık durumu” görsel olarak ayrılır.
- Dünya içinde kural değişikliği yapılacaksa müdahale kaydı ve dal mantığı kullanılır. Sessizce çalışan dünyayı yeniden yazan form olmaz.

Örnek açıklamalar:

| Ayar | Baloncuk |
|---|---|
| Mutasyon olasılığı | Bir doğumda yeni bir gen değişimi oluşma ihtimali. %0,1 yaklaşık bin doğumda bir beklenen olaydır; her bininci doğumda kesin gerçekleşmez. |
| Üreme güdüsü | Bedenin üremeye yönelik ihtiyaç sinyalinin gücü. Eş seçimini ve hareketi ağ belirler; yüksek değer doğumu garanti etmez. |
| Meyve yenilenmesi | Tükenen meyvenin yeniden oluşması için temel süre. Uzun süre daha seyrek besin sağlar; kuraklık ve mevsim ayrıca etkileyebilir. |
| Taç yarıçapı | Ağacın görsel örtüsünü ve model etkinse gölge alanını değiştirir. Gövde engel yarıçapından ayrıdır. |
| Ömür | Yaşlılıktan ölüm sınırı, simülasyon saniyesi. 0 sınırsız; diğer ölüm nedenleri devam eder. |
| Kara bölgesi | Suyun ayırdığı kara parçalarının hedef sayısı. Şekilleri ve kaynak yerleri tohumla değişir. |

**Kabul:** bütün kullanıcıya açık ayarlar açıklamalı; tooltip form alanını kullanılamaz yapmaz; ekran sınırı, hover gecikmesi, klavye ve yüzde dönüşümleri test edilir.

## 6. Canlı ayarları ve üreme isteği

### 6.1 Parametre katmanları

- **Kurucu grup profili:** nüfus, beden genlerinin aralıkları, başlangıç yaş/rezervleri, ömür ve yaşlanma, üreme modu ve güdü dağılımı.
- **Kalıtsal beden hedefi:** v0.4.0 güdüyü grup bazında dünya kuralı olarak ekler (0.5 varsayılan, hazır olma girdisi 2×güdü); yedinci gen değildir. Sonraki şema geçişinde mevcut altı gruba ek olarak kullanıcı tarafından istenen temel üreme güdüsü. Yeni gen için sürümlü şema gerekir; “yavruyu koru”, “avcıdan kaç” gibi ayrı dar davranış genleri eklenmez.
- **Anlık durum:** enerji/su/sağlık, yaş, üreme beklemesi; sonra yorgunluk, sıcaklık maruziyeti, taşınan yük gibi yalnız mekaniği olan durumlar.
- **Dünya katsayıları:** tüketim, enerji maliyetleri, üreme bütçeleri, hasar ve çevre kuralları.

Önce otçul ve etçil bağımsız profilleri; daha sonra birden fazla adlandırılmış kurucu soy profili. Görünen beslenme özelliği ile soy/grup kimliği aynı şey değildir. Başlangıç profil etiketini “evrimleşmiş yeni tür” diye adlandırmayız.

### 6.2 Üreme modeli kararı

1. Grup bazında **kapalı / eşeyli / eşeysiz**. Eşeysiz mod ayrı model ve karşılaştırma profili olarak eklenir.
2. Kullanıcı üreme güdüsünün alt/üst sınırını seçer, yavru iki ebeveyn aralığından alır; eşeysizde kopya+nadir mutasyon.
3. Güdü ve bedensel hazırlıktan 0–1 arası ihtiyaç sinyali türetilir. Bu ağ girdisidir; hareket/eş bulma/iletişim kodlanmaz.
4. Güdü 0 içsel üreme baskısı oluşturmaz; tamamen üremesiz dünya için ayrı “üreme kapalı” modu kullanılır. Üreme eylemi yine ağın olumlu çıktısını gerektirir. “Çok istekli” birey de yanlış karar veya kötü koşul yüzünden üreyemeyebilir.
5. Eşeyli doğum: iki tarafın açıklık sinyali, grup uyumu, erişim/temas, olgunluk, sağlık/su, gerçek enerji ödemesi ve bekleme süresi gerekir.
6. Eşeysiz doğum: tek ebeveyn gerçek toplam bedeli öder; eş arama maliyetinin kalktığı raporlanır. İki modun maliyetleri sessizce eşitlenmiş sayılmaz.
7. Başlangıçta tek yavru korunur. Yavru adedi/yatırım seçeneği sonra aynı toplam bütçenin bölünmesiyle eklenir; yavru başına ücretsiz enerji yoktur.
8. Gebelik/yumurta, mevsimsel doğurganlık, eş tercihi ve gerçek üreme izolasyonu altyapı ve deneylerle sonraki aşamadır.

Güdü yeni giriş olduğu için 30→8→5 ağına alan eklemek eski kayıtların davranışını değiştirir. Eski ağ şeması tanınır; geçişte yeni giriş bağlantıları sıfır başlar veya eski sürümle devam seçilir. Migrasyon, aynı gelecek garantisiyle karıştırılmaz. Doğum başına toplam mutasyon bütçesi ağ/gen büyüklüğünden bağımsız kalır.

**Kabul:** kapalı mod doğurmaz; 0 güdü ihtiyaç sinyali üretmez; ağ sinyali olmadan pozitif güdü doğurtmaz; ebeveyn bütçesi korunur; yavru aralıkları ve mutasyon doğru; aynı motor sinyali altında uygunluk ile isteğin farkı ölçülür. Yüksek güdünün gerçek nüfus etkisi çoklu seed ile raporlanır. Rastgele ağda güçlü ihtiyaç sinyali her zaman daha çok üreme üretmez; bunu garanti eden bir düğme gibi sunulmaz.

## 7. Ağaçlar ve bitki ekolojisi

Kullanıcı ağaç profilini ve başlangıç dağılımını düzenler; tek tek ağaç çizmek zorunda kalmaz. Tek ağaç seçilerek mevcut stok/yaş/sağlık incelenebilir. Türlere göre farklı meyve sayısı için treeIndex×globalSlots kodlaması, kararlı kaynak kimliği/offset sistemine çevrilir. Ağaç türü ayarlarını başlangıç profiline, anlık ağaç verisini inceleme kartına koyarız.

| Özellik ailesi | Ayarlar | Somut etkisi | Aşama |
|---|---|---|---|
| Yerleşim | Ağaç adedi/yoğunluğu, kümelenme, suya yakınlık, minimum aralık | Kaynak dağılımı ve erişim mesafesi | v0.4 |
| Beden | Gövde yarıçapı, taç yarıçapı, boy aralığı | Gövde çarpışması; taç örtüsü ve etkinse gölge | v0.4 |
| Meyve | Kapasite, enerji, tüketme süresi, yenilenme, meyve dönemi | Gerçek stok ve kalori akışı | v0.4 |
| Yaşam | Fidan süresi, büyüme hızı, ömür, sağlık | Üretime başlama, engel/taç gelişimi, ölüm | v0.4–v0.6 |
| Çevre | Nem gereksinimi, sıcaklık aralığı, kuraklık dayanımı | Üretim ve sağlık değişimi | v0.6 |
| Çoğalma | Tohum sıklığı, yayılma mesafesi, çimlenme şartları | Yeni bitki konumu, yoğunluk, kaynak rekabeti | v0.6 |
| Hasat/düşme | Düşen meyve oranı, yerde bozulma, tohum taşınması | Erişilebilir besin, toprak/bitki bağlantısı | v0.6 |
| Malzeme | Dal/odun miktarı, toplama maliyeti, dayanıklılık | Yuva ve onarım girdisi | v0.7 |
| Yangın | Yanıcılık, nem/yakıt ilişkisi, yeniden büyüme | Isı, bitki/malzeme kaybı, toparlanma | v0.6–v0.7 |

Ayarı tanımlanan bir mekanik çalışmıyorsa kullanıcıya etkin kaydırıcı olarak gösterilmez. Tür görünüşleri aynı alan verisini okur; görsel büyüklükle fiziksel gövde farklıysa açıklanır. Meyveler gerçek dünya konumunda olur, kamera dönüşünde enerjiye erişim değişmez.

Yenilenme ve üretim, bitkinin nem/ışık/sağlık kapasitesine dayanır. Modelde dışarıdan gelen bitki enerjisi açık girdidir; hayvanlar veya depolar kendiliğinden enerji üretemez. İleride çürümeden gelen mineral madde, enerji birimiyle birleştirilmez.

## 8. Gerçek büyük dünya

- Harita boyutu hedefleri: 128, 256 ve 512 kare kenarı; 1024 yalnız ölçüm sonrası deneysel. Büyük boyutlar tamamlanmış destek iddiası değildir.
- `Terrain.Size`, 16384 kayıt uzunluğu, sabit hücre kovaları, 64 merkez varsayımı ve çizim ölçekleri kaldırılır; genişlik/yükseklik veri sözleşmesine taşınır.
- Ağaç, çayır ve kaya için adet veya yoğunluk seçeneği. Yoğunluk boyuttan adet üretir; kullanıcıya sonuç önizlemede gösterilir. İkisi çelişen ayrı kontroller olmaz.
- Seed, boyut ve arazi profili tekrarlanabilir başlangıç üretir. İstenen kara sayısı ile gerçekleşen sayısı doğrulanır; yetersiz alan/yerleşim başarısızlığı açıklanır.
- Arazi üretimi yükseklik/nem ve su ağına bağlı gelişir. Dar nehir/ada/kıyı/koridor profilleri ayrı preset olur.
- 32–64 hücrelik parçalarla arazi çizimi ve önbellek; yalnız ekrandaki parçalar çizilir. Küçük sabit texture'ın büyütülmesiyle görüntü bulanıklaştırılmaz.
- Canlı algısı, temas, ağaç örtülmesi ve kaynak sorguları yerel indekslerle sınırlanır. Görünmeyen canlılar aynı biyolojik simülasyonda kalır.
- Bellek/kapasite sınırına gelmek doğal yok oluş değildir; dünya duraklatılır ve sebep gösterilir. Sessiz canlı silme veya üremeyi gizlice düşürme yoktur.

## 9. Kayıt, zaman çizelgesi ve geçmişten dallanma

**v0.5.0 uygulandı.** Dünya dosyası, son canlı durumla o sonuca götüren geçmişi aynı atomik kayıt içinde taşır. Geçmiş bir video değildir: aynı biyolojik çekirdek, başlangıç durumu ve sıralı müdahalelerden seçilen an yeniden hesaplanır.

- `EVOSIM-LIBRARY-2`, isim/kamera/soy-dal metadata'sı, tam son dünya checkpoint'i ve `EVOSIM-TIMELINE-1` arşivini tek `.evoworld` dosyasında tutar. Bütünlük özetleri ve uzunluk sınırları yazmadan/yüklemeden kontrol edilir; yarım yazma önceki geçerli kayıt/yedeğin yerine geçmez.
- İlk kaydedilmiş durum değişmez başlangıç checkpoint'idir. Kuraklığı aç/kapat komutları tick ve kesin sıra ile tutulur. RNG'nin bütün durumu Core checkpoint'inde yer alır; yeni rastgele olay türleri eklenirse ayrıca sürümlü komut sözleşmesi gerekir.
- Her **600 fizik adımında, yani 30 simülasyon saniyesinde**, GZip ile sıkıştırılmış hızlandırıcı checkpoint denenir. **32 MiB** bütçe yalnız bu hızlandırıcıları sınırlar. Eski hızlandırıcılar kaldırılabilir; başlangıç ve tam komut günlüğü silinmez, erişilebilir geçmiş aralığı daralmaz. Otomatik son kayıt sıklığı ayrı bir gerçek zaman ayarıdır.
- Her snapshot `NextCommandIndex` taşır. Bir tick içinde birden çok kuraklık değişikliği varsa checkpoint'te uygulanmış komutlar tekrarlanmaz; aranan tick'e kadar bütün kayıtlı komutlar sırasıyla uygulanır.
- Canlı son dünya (`HeadWorld`) ile ekranda incelenen geçmiş (`DisplayWorld`) ayrı referanslardır. Geri sararken, otomatik kaydederken ve kaydedip çıkarken dosyada **son dünya** korunur. Kayıt tekrar açıldığında son an gösterilir.
- Geçmişte Oynat veya tek adım, kaydedilmiş geleceği yürütür. Son ana ulaşıldığında tam checkpoint özeti doğrulanır ve canlı ilerlemeye dönülür. **Buradan devam**, geçmişin o ana kadarki başlangıç/komutlarını koruyan yeni kimlikli ve bağımsız bir dünya oluşturur. Eski dünyanın geleceği değişmez. Geçmişte kuraklık müdahalesi de önce açıkça etiketli yeni dal akışına geçer.
- Araç menüsündeki **Geçmiş** zaman çubuğunu açar; çubuk sürüklenir, sol ok tek adım geri gider, çubuk üzerindeki tekerlek bir simülasyon saniyesi değiştirir. En eski/son an görünür. Arama iptal edilirse önceki görünüm ve son dünya aynen kalır; **Son ana dön** canlı son durumu gösterir.
- Yeniden hesaplama UI döngüsüne her seferinde en çok 100 adım ve 6 ms hedef bütçeyle iş verir. Bir tek fizik adımı bölünemediği için bütçe o adım kadar aşılabilir. İlk checkpoint açma işlemi senkrondur; arama başında kısa bekleme yaratabilir. Ölçülmüş 256/500 kuruculu koşulda ilk açma 14,6–20,4 ms, 599 adım replay yaklaşık 0,98–1,01 saniye, en uzun iş parçası 7,9 ms idi; bunlar paylaşılan makinedeki örnek CPU ölçümleridir, azami yük garantisi değildir.
- **LIBRARY-1 eski kayıtları** önce ayrı `· v0.5` devam dünyasına alınır; kaynak dosya ve kimliği korunur. Bu dünyanın en eski erişilebilir zamanı, eski dosyanın gerçek kaydedildiği tick'tir. Önceki kaydedilmemiş yaşamı için geçmiş üretilmez.
- Replay, **gerçek Core DLL SHA-256** ve checkpoint şemasıyla kilitlidir. v0.5.0 Core özeti `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`, şema `EVOSIM-0.4-WORLD-2`'dir. Başka modelin geçmişini bu modelde çalıştırma isteği açık hatayla reddedilir; sessiz model güncellemesi yapılmaz. Çapraz runtime/CPU/GPU bit eşitliği bu testlerle kanıtlanmış değildir.
- Sil/geri al, dünya+gömülü geçmiş ile yedeğini birlikte yönetir. Bir dalın kendi dosyası vardır; ebeveynin silinmesi çocuğun geçmişini bozmaz. Origin ve hızlandırıcıların bellek içi paylaşımı dosyalar arasında zorunlu bağımlılık yaratmaz.

**Doğrulandı:** kesintisiz referansla tam kayıt baytları ve hash eşitliği; aynı-tick komut sırası; 1 KiB test bütçesinde origin'den derin replay; yarım aramayı iptal; eski geleceğin korunması; geçmiş görünürken kaydet/aç; eski kayıt içe aktarma; sil/geri al; bozuk veri ve farklı model reddi. Ayrıntılı bağımsız rapor `artifacts/v0.5/timeline-tests/` altında tutulur.

**Açık işler:** soy/yaşam filmi, deney ve dal karşılaştırma ekranları, önemli an sabitleme, ek müdahale türleri ve çok uzun koşul ölçümü. 32 MiB bütün simülasyonun RAM sınırı değildir: başlangıç, komutlar, son dünya ve Core soy/geçmiş verisi ayrıca büyür. Arşiv üst sınırı 512 MiB, komut sınırı 2.000.000'dur; doluluk açık hata üretir, geçmiş sessizce silinmez. Tam dosya yazımı ve checkpoint açma senkrondur; daha büyük yükte iş parçalama veya arka plan hazırlığı ayrı doğrulama kapısıdır.

## 10. İklim, yaşam koşulları ve afetler

Her sistem bağımsız açılabilir; başlangıç presetleri bütün seçenekleri aynı anda açmaz.

| Mekanik | Durum/bedel zinciri |
|---|---|
| Gün/gece | Işık → algı menzili; sıcaklık → su/enerji kaybı; ekrandaki gece okunaklı kalır |
| Mevsim | Sıcaklık/yağış → nem ve meyve dönemi → kaynak erişimi |
| Yağış/kuraklık | Nem ve su yüzeyi → bitki üretimi, içme erişimi, su ihtiyacı |
| Isı/soğuk | Maruziyet → su/enerji gideri ve aşırı durumda sağlık kaybı |
| Sel | Derinlik/geçiş alanı → yüzme maliyeti, erişim, yapı hasarı |
| Yangın | Yakıt+nem+rüzgâr → yayılma/ısı/hasar, stok kaybı, dumanla algı |
| Fırtına | Ağaç/yapı hasarı, düşen materyal, yeni engeller |
| Uyku/yorgunluk | Uyanık kalma/eylem → yorgunluk, dinlenme → toparlanma; savunmasızlık maliyeti |
| Hastalık/parazit | Temas veya çevresel yük → rezerv/sağlık kaybı; açık bulaşma ve iyileşme |

Yangın var diye otomatik kaçış emri verilmez: ısı/hasar/görsel sinyali ve yasal motor eylemleri sunulur. Afetler doğa kurallarıyla sonuç üretir. Yağış ve olay şiddeti/süre/sıklığı ayarlanır; rastgele olay tohumu tutulur. Deprem/heyelan ve ayrıntılı akış ayrı araştırma işleri olarak kalır.

## 11. Yuva, sosyal davranış ve deneyim

### 11.1 Yapı

Önce topla/taşı/bırak/yap/onar temel eylemleri, taşıma kapasitesi, yük-hız maliyeti ve materyal bütçesi; sonra yuva. Yuva kapasite, gölge/maruziyet, bozulma, bakım ve doluluk taşır. Üretici ölünce yapı hemen silinmez. Ağ yeni eylem kanallarını seçer; “hazır yuva yapma zekâsı” diye görev listesi uygulanmaz. Eylemlerin kullanılabilmesiyle davranışın gerçekten evrimleşmesi ayrı kabul sonuçlarıdır.

### 11.2 Sosyal bağlantı

Yakınlık/algılanabilir kimlik, kısa bellek, iletişim ve kaynak aktarımı altyapısı. Alarm yayınlama bedellidir; alıcıya hazır “kaç” komutu vermez. Yavru koruma, paylaşma, grup halinde yaşama ve pusu davranış olarak ölçülür. Önceden var olan sosyal bağ geninin neyi duyulur/kullanılabilir kıldığı açık belgelenir; soy ağacının tamamı beyne gizli bilgi olarak verilmez.

### 11.3 Öğrenme

Kalıtsal evrim ana modeldir. Geçici ağ belleği, kaynak/karşılaşma hafızası ve yaşam içinde ağırlık güncellemesi ayrı özelliklerdir. Yaşam boyu öğrenme kapalı varsayılanlı araştırma profili olur. Eğitim hedefleri, güncelleme sıklığı, eğitim/değerlendirme seed'leri ve kayıtları açıklanır. GPU eğitimi kurulmadan eğitim süresi sözü verilmez. Temel profile elde eğitilmiş başarılı davranış sessizce yerleştirilmez.

## 12. Gözlem ve deney araçları

### 12.1 v0.5.1 ile çalışan gözlem dilimi

- **Canlı kartı:** enerji/su/sağlık, yaş, nesil ve eyleme ek olarak Beden, Algı, Beslenme, Su uyumu, Atılganlık ve Sosyal bağ genleri salt okunur çubuklar ve sayılarla gösterilir; bu kart genleri değiştirmez. Küçük i baloncuğu mevcut parametre açıklamasını verir. Modelin güncel hız kapasitesi, görüş yarıçapı/açısı ve yaş etkisi ayrı görünür; kapasite ile o anki hareket hızı karıştırılmaz.
- **Yerel soy grafiği:** ebeveynler → odaktaki birey → dört kişilik sayfalarda yavrular. Yaşayan ve ölmüş kimliklere tıklanabilir; yaşayan odak haritada gösterilebilir. Doğrudan yavru ve tam iki bağ uzaklığındaki torunlar kendi kümelerinde benzersizdir. Bütün altsoy, farklı soy yollarının birleşimiyle hesaplanır; aynı birey iki kez sayılmaz. Tek ebeveyn, kurucu ve ortak atalı soylar desteklenir.
- **Gen dağılımları:** yalnız yaşayan bireyler için altı genin on aralıklı histogramı ve en az/ortanca/en çok değerleri. Gruplar kurucu Group alanı yerine güncel beslenme geniyle ayrılır: <0,5 otçul ağırlıklı, ≥0,5 etçil ağırlıklı. Her grubun çubukları kendi örneklem sayısına göre normalize edilir. Boş grup açıkça boş gösterilir. Su uyumu ≥0,85, fiziksel olgunluk ve etkin yaşlanma kurallarıyla sayısal özetler eklenir.
- **Kaynak kartları:** ağaç, ot, su, kaya, doğal barınak ve ceset tıklanır. Kalan ham enerji, meyve yuvaları, yerel nem ve uygun fizik/ekoloji bilgileri gösterilir. Meyve yenilenmesi şimdiki su düzeyine göre tahmindir; ham besin enerjisi sindirilmiş enerjiyle eşitlenmez. Su düzeyi küresel arz olarak açıklanır; yerel derinlik sanılmaz. Modelde olmayan yaş/sağlık/kapasite alanları uydurulmaz.
- **Girdi ve geçmiş güvenliği:** büyük soy/gen panelleri gözlemi duraklatır ve arka plana fare/klavye girişi geçirmez. Bütün kartlar görüntülenen World'ü okur; canlı son dünya geçmiş görüntüsüne sızmaz. Dünya referansı değişince seçim/önbellek yenilenir. Soy topolojisi World referansı+kayıt sayısı ile, yaşayan özetler en geç 20 tick'te güncellenir; modal açılırken taze değer alınır.
- **Model değişmezliği:** panel açma, veri hesaplama ve çizim Core alanlarını veya RNG'yi değiştirmez. Soy/nüfus/kaynak paketlerinde ayrı ayrı altı bağımsız grup; tam checkpoint baytları, aynı gelecek ve UI akışları ile bu sınır doğrulandı.

### 12.2 Sonraki gözlem ve deney hedefleri

- Küresel benzersiz soy grafiği, favori bireyler, ölüm anına gitme ve gerçek kayıtlı rotadan yaşam filmi. Yerel grafiğin tamamlanması bu hedefleri kapatmaz.
- Ölüm nedeni, kaynak tüketimi, hareket mesafesi, duvarda bekleme, bölge geçişleri, doğum aralığı ve yetişkinliğe ulaşan yavru oranının zaman serileri.
- Nüfus/yaş/gen dağılımlarının zaman içindeki değişimi, kaynak üretim/tüketimi ve soy çeşitliliği. Tek ortalama veya tek histogram yakınsama/öğrenme diye yorumlanmaz.
- İsteğe bağlı yoğunluk, besin/nem, sıcaklık, soy, tehlike/hasar katmanları. Bunlar canlı algısına sızmaz.
- Gerçek olaylardan keşif günlüğü: ilk nehir geçişi, ilk mutasyon, soyun bitmesi, kıtlık, ilk yapı. Eşik ve sorgu tanımı kaydedilir; hikâye uydurulmaz.
- Dünya karşılaştırması: yan yana veya üst üste grafik; ayar farkları listesi, eşleştirilmiş seed grubu, sonuç dağılımı.
- Başsız koşu kuyruğu: hedef süre veya yok oluş; başarısız koşular da raporlanır. Hata ayıklama sonrası başlangıç grubu en az 20 seed; yeterli bilimsel örneklem iddiası değildir.
- D01 kaynak kümelenmesi, D02 kuraklık, D03 üreme modu, D04 mutasyon, D05 su erişimi, D06 avcı etkisi, D07 korunak, D08 inşaat, D09 sosyal bağ, D10 bellek, D11 kalıtsal ağ/öğrenme, D12 CPU/GPU karşılaştırma paketleri.

## 13. Teknik mimari ve GPU

### 13.1 Korunacak omurga

Core, Renderer, UI, Config/Schema, WorldStore/Replay, Runner/Analysis, AssetPipeline modülleri. Model saati sabit 20 Hz; mevcut 5 Hz karar ve 1 Hz ekoloji baz çizgidir. Değişiklik model sürümüdür; performans için sessizce düşürülmez.

Tick sözleşmesi: snapshot → sınırlı duyular → ağ niyetleri → hareket/temas → ortak kaynak ve hasar arbitrajı → beden/ölüm → bütçeli doğum → ekoloji olayları → kayıt/istatistik. Kesin alt sıra ilgili model sürümünde test edilir. UI canlı mutable dünyayı ayrı thread'den okumaz; render snapshot alır.

Hareket; dönüş/ivme, beden yarıçapı, süpürülmüş çarpışma, yüzme ve enerjiye bağlıdır. Yerel fizik kayması olabilir; yol bulucu canlının yerine stratejik hedef seçmez. Hareketin hatalı olmasıyla ağın kötü motor üretmesi ayrı ölçülür.

### 13.2 Ölçek

Önce allocation ve tarama profili: ağın her çağrıda dizi ayırması, bütün canlı/ağaç sorguları, doğum adayları, uzun soy listesi, kayıt ve texture çizimleri. Typed array/SoA, yerel spatial index, deterministik intent/commit ve iş parçalama. Kameranın dışında kalan canlıları dondurarak hız kazanılmaz.

CPU referansı doğruluk kaynağıdır. Harita/kaynak/olay/beyin akışları ayrıdır. Paralel commit sırası kararlı kimlikle belirlenir. Uzun soy olay deposuna taşınır; yaşayan bedenlerle bütün tarih aynı sıcak döngüde tutulmaz.

### 13.3 GPU ve sunum kararı

- İlk tercih: GPU hızlandırmalı çizim, atlas, batching/instancing, görünür parça seçimi, yeniden kullanılabilir geometri. SDL gözlemci üzerinde uygulanabilen işler bekletilmez.
- Unity/URP sunum hedefi korunur; kurulum ve yerel Linux çalışma doğrulanmadan mevcut gözlemci kaldırılmaz. Core motor bağımsız kalır. Resmî paket/sürüm uyumluluğu uygulama gününde kontrol edilir.
- 357 ağırlıklı küçük ağ tek tek GPU'ya gönderilmez. Yeterli nüfus/çoklu dünya için toplu çıkarım, çevre alanı hesabı ve eğitim ayrı adaylardır.
- İşlem, CPU→GPU aktarımı, bariyer, okuma ve çizim dahil uçtan uca ölçülür. Aynı iş yükünde en az 1,5× toplam kazanım proje hedefi; kazanç yoksa CPU yürütümü korunur.
- GPU yüzde kullanımını doldurmak hedef değildir. Dizüstünün sıcak koşu davranışı ve arayüz gecikmesi ölçülür; güç/fan ayarları değiştirilmez.
- CPU aynı sürüm/runtime'da bit düzeyinde tekrar; farklı GPU aritmetiğinde tolerans + olay/dağılım karşılaştırması. Küçük sayısal farkların uzun dönem ayrışmaya dönüşebileceği kayıtta görünür.

Performans test matrisi hedefleri: 128²/165, 256²/500, 512²/2.000; 1024²/10.000 deneysel. 1080p ve 1440p; yakın/uzak, yoğun kıyı, yoğun orman, uzun soy, kayıt/replay yükü; 1× ve 20×. 30 dakikalık sıcak koşuda gerçek hız, p95/p99 frame/tick, bellek, kayıt gecikmesi ve GPU transferleri raporlanır. 60 FPS hedef, 20× ise makine ve yüke bağlı hedeftir; sonuç ölçülmeden desteklenen kapasite ilan edilmez.

## 14. Yeni sürüm sırası ve tamamlanma ölçütleri

Sürüm numaraları önceki taslak yol haritasını günceller. Bunlar takvim veya tek gece bitirme sözü değildir. Önce çalışır ve ölçülmüş dilimler çıkar; ileri araştırmalar kararlı sürüme sessizce karıştırılmaz.

| Sürüm | Kapsam | Bitti sayılma koşulu |
|---|---|---|
| v0.3 — Görünüm ve anlaşılabilirlik | Terra Nil yönünde ilk tam asset seti, sıcak UI, ⓘ açıklamalar, yüzde girişleri, dünya sil/geri al, otomatik taç saydamlığı ve kontur | Gerçek oyun ekranlarında orman/zoom QA; bütün ayarlar açıklamalı; kamera/tema core hash'ini değiştirmiyor; kayıtlar açılıyor |
| v0.4 — Dünya kuralları | Üreme güdüsü ve modları, ağaç profilleri, kayıt şeması geçişi, 256² ilk büyük harita, güçlü presetler | Gen/beden/maliyet testleri, eski dosyanın korunması, ağaç ve üreme ayarlarının ölçülmüş etkisi |
| v0.5.0 — Geçmiş ve dallanma · tamamlandı | Zaman çizelgesi, kayıtlı geleceği oynatma, geçmişten ayrı dünya, atomik dünya+geçmiş ve eski kayıt içe aktarma | 10/10 bağımsız replay/persistence grubu, komut sırası, dal ve yedek korunması; görünür UI akışı |
| v0.5.1 — Gözlem kartları · tamamlandı | Altı genli canlı kartı, yerel soy grafiği, nüfus histogramları, altı kaynak türünün kartı | Soy/nüfus/kaynak için 6+6+6 bağımsız grup; model baytları değişmiyor, geçmiş verisi doğru, bağlamsal UI çalışıyor |
| v0.5.x — Deney ve geniş gözlem · planlandı | Yaşam filmi, küresel soy görünümü, zaman serileri/katmanlar, dal karşılaştırması ve seed kuyruğu | Eşleştirilmiş deney raporu, açık gözlem tanımları ve veri doğruluğu |
| v0.6 — Yaşayan çevre | 512² ölçümü, mevsim/gün-gece/yağış, ısı, ağaç yaşamı/tohum, sel ve yangın | Her çevre sisteminin açık etki/bütçe/karşı deney kontrolü; erişim ve ölüm tutarlılığı |
| v0.7 — Birey ve çevre inşası | Yük, malzeme, yuva, depolama, bakım, basit iletişim ve bellek | Aktarım/üretim bütçeleri, eylem şeması ve kayıt; davranışların gerçekten kullanılıp kullanılmadığı raporu |
| v0.8 — Ölçek ve sunum | Veri odaklı çekirdek, paralellik, koşullu GPU; Unity/URP, hafif 3D | Referans doğruluğu, toplam hız kazancı, aynı görünürlük ve seçim; mevcut çalışan alternatif korunur |
| v0.9 — Araştırma | Yaşam içinde öğrenme; hastalık/parazit; gelişmiş sosyal/üreme/çevre araştırmaları | Her mod etiketli, kapatılabilir, kayıtlı ve bağımsız değerlendirilmiş |
| v1.0 — Kararlı kişisel laboratuvar | Seçilmiş mekaniklerin bütünleşmesi, kurulum/kurtarma, ses/kalite/erişilebilirlik, uzun kullanım | Sürüm geçişi, uzun koşu, performans, belgeler ve uçtan uca kullanıcı akışları doğrulanmış |

Performans, kayıt ve hata düzeltme ihtiyaç oluştuğu sürümde yapılır; v0.8'e kadar bekletilmez. Yürüyüş animasyonu da ilk görsel sette başlayabilir. 72 fikirdeki araştırma sonuçlarının tümünün evrimleşmesi v1.0 sözü değildir; her birinin durumu envanterde açık kalır.

## 15. İlk uygulama paketi: bağımlılık sırası

1. Çalışan v0.2.1 kaydını ve test fixture'ını koru; yeni geliştirme dizini/sürümü aç.
2. Parametre tanımlarını açıklama/birim/yüzde/bağımlılık verisiyle merkezileştir; tooltip bileşenini kur.
3. Renk/boşluk/tipografi/düğme/kart/pencere tokenları; ana menü ve kurulumun sıcak teması.
4. Kamera dönüşümünü tek modüle al; seçim, pan/zoom ve determinism testi.
5. Terra Nil referans panosu, mevcut sahnenin yeni asset örnekleri; taç/gövde/gölge ayrımı; bütün canlılarda örtülme.
6. Üç zoom düzeyinde okunurluk; yeni sabit düğme eklemeden açık bilgi kartları; gerçek ekran görüntüleriyle görsel kabul.
7. Sürüm destekli config/brain/checkpoint şeması; sonra üreme güdüsü ve genişletilmiş ağaç profilleri.
8. Dinamik boyut ve parça çizimi; 256² gözlem, kıyı/orman/nüfus performans kapısı.
9. Kaydet/çık/aç/sil/geri al/reset/kurulum/isim/yüzde/tooltip/kamera testleri; paket ve sürüm notu.
10. v0.5.0 geçmiş çizelgesi/dal güvenliği ve v0.5.1 bağlamsal gözlem kartları tamamlandı. Sonraki dilim kontrollü deney, karşılaştırma ve geniş gözlem araçları; ardından iklim/afet/yuva altyapısı. Her birine ayrı model ve veri kabul kapısı uygulanır.

Bağımsız UI, render ve mekanik işleri paralel yürüyebilir; ortak şemayı değiştiren işler önce sözleşmede birleştirilir. Kullanıcıdan aynı yönde tekrar onay istenmez. Uygulama sırasında gereksinim belirsizliğinde önce eldeki tercihler kullanılır; yalnız sonucu esaslı değiştiren tercihler sorulur.

## 16. Tamamlanma ve teslim disiplini

- [ ] İstenen davranış/arayüz akışı uygulamada çalışıyor.
- [ ] Her yeni mekanikte etki, maliyet, duyusal erişim ve sınırlar belgeli.
- [ ] Hazır davranış politikası eklenmedi; istisna olan beden refleksi açık.
- [ ] Canlı/ağaç ayarları gerçek hesapta etkili; olmayan özelliğin etkin kontrolü yok.
- [ ] Kalıtım, mutasyon, enerji/su, ölüm, doğum, kaynak ve kimlik testleri geçiyor.
- [ ] Yeni şema eski kaydı silmiyor; aynı sürümde save/load ve replay doğru.
- [ ] Yeni UI gerçek ekranda incelendi; yoğun orman, küçük/büyük pencere ve yoğun orman denenmiş.
- [ ] Hız, yok oluş ve kayıt başarısızlığı doğru etiketli.
- [ ] Başarılı sonuçların yanında çöken/yok olan deneyler de raporda.
- [ ] Kullanım rehberi, güncel plan, envanter durumu, paket ve bilinen sınırlar eşleşiyor.

## 17. Açık belirsizlikler ve alınan kararlar

- Çok düşük mutasyon + aralıktan ağırlık kalıtımı, çeşitliliği daraltabilir ve mevcut beceriyi bozabilir. Çözüm gizli mutasyon/klonlama değil; kurucu çeşitliliği, ağ şeması ve çoklu seed ölçümüdür.
- Üreme isteği eklenmesi nüfusu kendiliğinden dengelemez. Temas/uygunluk/algı/enerji/sinyal ayrı ölçülür; “çok üresin” ayarının sonucu dürüstçe gösterilir.
- Kamera yön düğmesi ilk pakette yoktur; taç saydamlığı ve kontur otomatik çalışır.
- Her büyük haritanın tamamında birey ayrıntısı okunamaz; uzak görünümde simgeler ve isteğe bağlı yoğunluk katmanı kullanılır.
- v0.4 tipli/adlı checkpoint, v0.4.1 boyut/kaynak kimliği ve v0.5.0 gömülü timeline geçişleri tamamlandı. Origin/komut günlüğü korunurken hızlandırıcılar bütçeyle kırpılır; uzun Core soy verisi ve senkron kayıt/decode maliyeti izlenmelidir.
- Bütün ileri mekanikleri bir gecede kararlı ve dengeli hâle getirme garantisi yoktur. Çalışma, tamamlanmış/test edilmiş dilimler üretir; prototip ile kararlı özellik ayrı işaretlenir.
- Tooltip, sürgüler, sıcak sanat/UI, otomatik örtülme, grup bazlı üreme güdüsü, 128/192/256 harita, gerçek geçmiş/dallanma ve yerel soy/gen/kaynak gözlem araçları uygulanmıştır. İleri hedefler envanterdeki ayrı kabul koşulları sağlanmadan tamamlandı sayılmaz.

## v0.5.2 teslimi

Filtreli gerçek olay günlüğü, doğumdan soy inceleme ve olaydan geçmiş anına geçiş tamamlandı. Tam kayıt değişmezliği ve geçmişte gelecek verisinin sızmaması yayın UI testinde doğrulandı. Core olay penceresi sınırlıdır (2000 aşılınca500 eski satır kırpılır); otomatik keşif yorumları ve sınırsız olay arşivi sonraki işlerdir.

### v0.5.3 gözlem teslimi

Nüfus panelinde Yaşam sekmesi: ölü bireylerden birikimli nedenler, anlık özel ilk üreme engeli, hazır çiftler ve eşeysiz hazır sayıları. Core ve kayıt kimliği değişmedi; gerçek yayın UI smoke ve geçmiş/tam checkpoint değişmezliği doğrulandı. Uzun koşu18dünya ölçümü sürdürülebilir nüfus göstermedi; su/çatışma/üreme darboğazlarının ayrıştırılması açık önceliktir.

### v0.5.4 gözlem paketi

V2 anlık JSON/nüfus/birey CSV paketi; gerçek ölüm/soy/üreme alanları, kaynak dünya ve görüntülenen geçmiş anı, bütünlük manifesti ve staging yayımlama. Gerçek yayın UI smoke, CSV kültür/kaçış, hata temizliği ve geçmiş/checkpoint korunması doğrulandı. Sürekli bireysel yaşam geçmişi henüz açık hedef.

### v0.5.5 harita gözlemi

8×8 bölgelerde canlı, bitkisel ham besin ve kara nemi katmanları tamamlandı. V/menü döngüsü ve küçük açıklama; üç sabit kontrol korunur. Büyük haritalarda toplamlar, sınırlar, input ve geçmiş/full checkpoint değişmezliği yayın UI smoke ile geçti. Core ve kayıt kimliği aynı.

### v0.5.6 kitaplık

Ana menü/dünya içi kitaplıkta kayıt tarihi, Türkçe ad, nüfus ve süre sırası; tümü/canlı var/canlı yok filtresi. Disk şeması aynı. Gerçek UI tıklamaları ve salt okunur metadata/simülasyon kontrolü doğrulandı.

### v0.5.7 bireysel gözlem

Canlı kartından son20dakikalık enerji/su/sağlık ve örnek konum izini açma, karelere bölünmüş replay ve iptal tamamlandı. Pencere görüntülenen anda biter; gelecekteki ölüm sızmaz. Snapshot açma/serileştirme henüz bölünemez; sert hazırlık gecikme garantisi yok. Bağımsız ileri ölçümle eşitlik ve tam kullanıcı dünya/arşiv değişmezliği yayın UI smoke ile doğrulandı. Tam yaşam filmi/kalıcı bireysel olay deposu halen açık hedef.

### v0.5.8 iz etkileşimi

Yaşam izi hover ile sayısal örnek/konum eşlemesi ve tıklamayla gerçek timeline adımına geçiş eklendi. Doğum öncesi ve ölüm sonrası durumlar ayrılır. Geçmiş sınırı/head checkpoint korunması yayın UI smoke ile doğrulandı.

## 16 Eylül 2026 — Yaşam izi performansı

Dört ölçek ve üç tekrar ölçüldü. v0.5.9 özel replay hesabını arka plana taşıdı; UI yalnız değişmez sonuç okur. Kopya yakalama hâlâ senkron. Toplam replay maliyetini azaltma ve sürdürülebilir ekoloji deneyleri açık hedeflerdir. Başarılı kaçma/beslenme stratejileri henüz doğrulanmadı.

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
