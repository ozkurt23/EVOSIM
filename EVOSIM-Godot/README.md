# EvoSim — Godot görsel deneme sahnesi

**Durum:** Yaşayan orman modeliyle çalışan Godot gözlemevi; yayın öncesi geliştirme sürümü. Özgün v1 model ve eski kayıt başlatıcısı korunur. Yeni orman modeli ayrı v2 modülüdür. Tamamlanmış Terra Nil düzeyinde sanat kalitesi veya bağımsız masaüstü dağıtımı olarak sunulmaz. Güncel açma ve model ayrımı için aşağıdaki 25 Ekim bölümüne bakın.

## Açma

Bu çalışma alanında `./baslat.sh` Godot .NET ve .NET SDK yollarını bulur, derler, varlıkları içe aktarır ve uygulamayı açar. Betik çalışma dizininden bağımsızdır.

Başka bilgisayarda Godot **4.7.2 .NET** ve **.NET 10 SDK** gerekir. Godot yürütülebilirini `EVOSIM_GODOT` ortam değişkeni ile belirtin veya `godot-mono`/`godot` adıyla PATH'e ekleyin. İlk derleme paket önbelleği eksikse NuGet erişimi gerekebilir. Çalışan simülasyonun çevrimiçi servis gereksinimi yoktur. Henüz bağımsız Windows/macOS/Linux dağıtım paketi hazırlanmadı.

- Tekerlek: yakınlaş/uzaklaş.
- Sağ veya orta tuş sürükleme: haritada gezin.
- Canlıya tıklama: bilgi kartı; kartın çarpısı: kapat.
- Boşluk veya turuncu düğme: duraklat/devam.
- Hız düğmesi: 1×, 2×, 4×, 8×, 16×, 20× hedef hız.
- Küp simgesi: yüksek/dengeli grafik.
- Q/E: kamera döndürme. Esc: pencereyi kapat.

Sol dairede otçul/etçil sayıları ve oranları; yanındaki sayaçta toplam nüfus; sağ üstte yaşayan canlıların ortalama enerji ve su yüzdeleri bulunur. Sayaçlar bütün simülasyon dünyasına aittir; görüntü 48×48 hücrelik kesittir. Zaman gün/yıl cinsindedir. Deneme sahnesi tohum42 kullanır.

## Referansla çalışma

- `PROJECT_AUDIT.md`: gerçek proje mimarisi.
- `research/REFERENCE_ANALYSIS.md`: 20 farklı görsel referansın ayrıştırılması (16 oyun/dünya haritası görünümü + 4 kamuya açık artbook önizlemesi; iki ek dosya tekrar/crop).
- `research/reference-manifest.json`: kaynaklar, çözünürlük, dosya hashleri, tekrar ilişkileri.
- `VISUAL_SPEC.md`: UI ve çevre için kanıta bağlı uygulanabilir kurallar.
- `TECHNICAL_MAPPING.md`: eski çekirdek ile yeni renderer arasındaki sınır.
- `VISUAL_QA.md`: gerçek görüntüler, başarısızlıklar ve kalan farklar.
- `docs/visual-reverse-engineering/SKILL.md`: tekrar kullanılabilir çalışma protokolü.
- `docs/ASSET_PROVENANCE.md`: özgün modeller ve yazı tipi lisansı.

Referans görselleri oyun dokusu/asset'i değildir. `research/` dağıtımın dışında tutulmalıdır. Artbook'un sadece resmî mağazada kamuya açık dört örnek sayfası incelendi.

## Tekrarlanabilir kontrol

```bash
./baslat.sh --resolution 1920x1080 -- --smoke --inspect --capture-at=12 --capture=/absolute/path/capture.png
```

`--detail` seçili canlıya yakın kamera, `--speed=20` hedef hız, `--zoom=45` genel sahne görünümü. Kaydın yanındaki JSON gerçek cihazı, kare sürelerini, simülasyon hash eşleşmesini, arazi sınıflandırmasını ve kontrol testlerini içerir. `-motion.png` aynı durdurulmuş simülasyon adımında ikinci kareyi gösterir.

## Bilinen sınırlar

- Dünya menüsü, kayıtlar, oluşturma ayarları ve dört konumlu panel yığın sistemi Godot katmanına taşındı; aşağıdaki kullanım bölümüne bakın.
- Dört bacak, baş ve kuyruk eklem animasyonları var; ayrı yüzme/gelişmiş saldırı klipleri henüz yok.
- Görsel kabul bekliyor; taş, hayvan yüzeyi, bitki çeşitliliği ve su detayları hâlâ geliştirilecek.
- Önceki testlerde20× sürdürülemiyordu. 24Ekim optimizasyonuyla78kuruculu kısa testte20× sağlandı;500kuruculu256haritada yaklaşık12×. Adımlar atlanmıyor; kalan işlem borcu korunuyor. Sürekli20× veya her nüfusta60FPS garantisi değildir.

## İklim ve durum testleri

Görsel kesitte240günlük örnek çevre döngüsü bulunur:80–160kurak,160–190yağış/toparlanma. Kuraklığın bitki ve su etkileri mevcut biyolojik çekirdekten gelir. Yağmur bu toparlanmanın görsel karşılığıdır; ayrı fiziksel yağış modeli değildir. Hareket animasyonları gerçek hareket/eylem verilerini izler.

`--scenario=drought`, `--scenario=rain`, `--scenario=empty`, `--scenario=cycle` kontrollü test başlangıçlarıdır. `--balanced` düşük maliyetli grafik başlangıcıdır. `SCENARIO_MATRIX.md` uygulanan ve kapsam dışı durumları ayırır. `validation/pose_gallery.gd` sentetik poz galerisidir; `previews/creature-motion-15.gif` gerçek Godot karelerinden oluşturulmuştur.

## 24 Ekim — gözlemevi entegrasyonu

Başlangıçta dünya menüsü açılır. **Vadiye dön** mevcut dünyayı açar; **Yeni dünya** tüm mevcut model ayarlarını ve dört referans profili gösterir. Oransal değerler sürgü, sayılar ve yıl cinsinden yaşlar sayısal alandır. `ⓘ` simgedeki açıklamalar gerçek model kurallarını anlatır. Kuraklık/yağmur döngüsü kurulurken kapatılabilir ve tercihi kayda alınır.

- **Esc / sol alt menü:** kaydederek dünyalar menüsüne dön. İki dakikada bir ve uygulama kapatılırken de kayıt alınır. Kaydetme hatası gösterilir; sessizce üstüne yazılmaz.
- **Dünyalar:** ad alanına yazıp Enter ile yeniden adlandır; Sil, geri alınabilir Silinenler bölümüne taşır. Silinen aktif dünya otomatik kaydedilip yeniden ortaya çıkarılmaz.
- **Eski kayıtlar:** önceki SDL uygulamasındaki dört mevcut dünyayı, asıllarını değiştirmeden bu gözlemevine kopyalayarak açabilir. İçe aktarılan dünyada otomatik hava döngüsü kapalı başlar.
- **Sol alt saat simgesi:** nüfus, olaylar, zaman çizelgesi, gen dağılımları, kaynak stokları veya JSON/CSV gözlem raporu. Geçmişten ayrı dal oluşturma özgün geleceği korur. Raporda canlılar, kalıtsal ağlar, nüfus ve kaynak bilgileri bulunur.
- **Canlı seçimi:** dört ekran konumunda bağımsız gözlem pencereleri; sonraki seçimler aynı konumlarda başlıkları erişilebilir biçimde istiflenir. Başlığa basarak pencere öne alınır.
- **Küçük harita:** sol tıkla büyüt, bir yere tıklayarak bölgesine git; sağ tıkla küçült. Ana haritada sağ/orta tuşla kaydırma, tekerlekle yakınlaşma sürer. Canlıya çift tıklama kamerayı takibe alır; elle kaydırmak takipten çıkarır.

Kayıtlar Godot kullanıcı veri dizinindeki `worlds`, raporlar `reports` klasöründedir. Geliştirme testleri ayrı `validation` kayıt dizinleri kullanır.

### Gerçek sınırlar

Görsel katman aynı anda 48×48 bir bölge kurar; artık 128/192/256 haritanın diğer bölgelerine gidilebilir. Bölge geçişi henüz eşzamansız değildir ve kısa duraksama yaratabilir. Yeni Godot arayüzü eski SDL istemcisindeki bütün araştırma araçlarını henüz kapsamaz. Yangın/sel/hastalık/inşa sistemleri tamamlanmış değildir. Terra Nil görsel kalite kabulü açık kalıyor. Başlatıcı yerel Linux geliştirme ortamında çalışır; bağımsız Windows/macOS dağıtımları hazır değildir.

Biyolojik Core kaynakları değiştirilmeden mevcut Release derlemesi kullanılıyor. Godot tarafındaki gömülü DLL kopyası yalnız arşivin model parmak izini doğrulamak içindir; ikinci bir simülasyon çalıştırmaz. Eski SDL kayıt ve rapor yardımcılarının kaynakları ortak derlenir.

### Yeni araştırma panelleri

Canlı kartındaki **Yaşam izi**, açıldığı ana kadarki kayıt üzerinden enerji yüzdesi, su ve sağlık eğrilerini çıkarır; ana dünyayı ilerletmez. **Soy bağı**, ebeveyn ve doğrudan yavruların kartlarına geçiş sağlar. Ölmüş yavrular kayıtta kalır; geçmişe gidildiğinde gelecekteki doğumlar gösterilmez. Tam çok nesilli soy ağacı henüz yoktur.

**Gen dağılımları** altı kalıtsal özelliğin yaşayan otçul/etçil dağılımını gösterir. **Kaynaklar**, tüm dünyanın gerçek mevcut meyve/ot stoklarını gösterir; su erişimi değeri nehir derinliği değildir. Araştırma panelleri mevcut gözlem menüsünde bulunur, haritaya yeni sabit düğmeler eklenmez.

**Ağaç inceleme:** Gövdeye tıklayın. Meyve adedi, mevcut toplam besin ve tam bir meyvenin enerji değeri ayrı gösterilir. Seçilen gövdede halka görünür; canlıya tıklamak önceliklidir. Bu kart gözlemdir, dünya ayarlarını değiştirmez.

## 25 Ekim — yaşayan orman sürümü

**Güncel varsayılan:** `./baslat.sh` artık yaşayan orman modelini açar. Aynı renderer/UI kaynaklarından ayrı `EVOSIM-Godot-Forest` çalışma dizini hazırlanır; ayrı derlemeler ve `EVOSIM-Forest-v2` kullanıcı kayıt dizini kullanılır. Bu üretilen klasörü elle düzenlemeyin. Asıl kaynaklar bu klasördeki `scripts/`, `shaders/`, `assets/` ve `../EVOSIM-Models/` içindedir.

- `./baslat.sh --legacy`: önceki biyolojik model ve önceki Godot dünyaları. Eski dünya geçmişleri otomatik dönüştürülmez.
- `./yasayan-orman.sh`: doğrudan yeni model. Yeni dünya ayarlarında **Yaşayan orman** sekmesi ve yedi açıklamalı parametre bulunur.
- **Hızlı orman döngüsü** profili: kısa ömür/olgunlaşmayla yaşam döngüsünü gözlemlemek için. Referans ayarlar 60 yıl ömür ve uygun koşullarda 3 yıl büyümedir.
- Ağaçlar tohum yayar; tohumlar su, verimlilik, boşluk ve rekabet uygunsa tutunur. Yenmemiş cesetlerin gerçek çürümesi geçici ve sınırlı toprak katkısı sağlar. Her ceset otomatik bir ağaç üretmez.
- Fidanlar büyümeden meyve vermez. Kuraklık sağlığı ve verimi düşürür; ölen ağaç yapraklarını ve meyvesini kaybeder. Kuru gövde 30 gün sonra kalkar. Ağaç ömrü 0, yalnız yaşlılıktan ölümü kapatır.
- Ağaç paneli yaş, büyüme, sağlık/ölüm nedeni ve toprak katkısını; kaynak paneli yaşayan ağaç, fidan, kuru gövde ve tohum sayılarını gösterir. Kimlikler kalıcıdır; eski bir seçim başka ağaca dönüşmez.
- Kayıt/yeniden açma/geri sarma/dal oluşturma ormanı da kapsar. Eski modelden yeni ormana grafik arayüzlü aktarım henüz yoktur; eski başlatıcı korunur.

Geliştirme başlatıcısı ayrıca Python 3 kullanır. Bağımsız masaüstü paketleri henüz hazırlanmamıştır. Orman modeli, kayıt kimliği olarak yüklenen CoreV2/ForestEcology/V2Archive derlemelerinin birleşik özetini kullanır; gömülü kaynakla çalışan modül kimlikleri uyuşmuyorsa başlatma reddedilir. Görsel kaynak değişiklikleri biyolojik model kimliğini değiştirmez.


## Linux portable preview — iteration 48
Open `../EVOSIM-Linux/baslat.sh` or `EVOSIM.x86_64`. Keep its resource pack, runtime directory and `legacy/` helper together. No editor, Python or SDK is needed to play. This build was smoke-tested on this Linux machine with a fresh user directory and an invalid DOTNET_ROOT; another machine and other desktop OSes are not yet verified.

The library now includes **Eski dünyayı aktar**. Choose an original `.evoworld` file; its source remains unchanged. A separately pinned v1 process extracts the final recorded state, then v2 starts a new forest history at that day. Source history remains in its original file. Import runs asynchronously and pauses the active world. The new archive retains source hashes/provenance.

Build with `tools/package_linux.sh` after installing matching 4.7.2 mono export templates. It fails on C# export errors even when Godot exits zero. See `docs/PERFORMANCE_RESEARCH.md` for reproducible FPS measurements, CPU priorities and limitations. See `docs/RELEASE_ROADMAP.md` for release gates, including observed population extinction in all three ten-year coupled runs.


### Yeni gözlem araçları (49)
Boş zemine tıkla: bölgedeki canlılar, besin ve son30gün ölümleri. Sol üst nüfusa tıkla: nüfus/doğum/ölüm geçmişi; grafikte güne tıkla: geçmişe git. Canlı→Soy bağı→Soyu haritada takip et. Gözlem menüsünden yoğunluk/besin/ölüm katmanı, özellik ve üreme, ölüm nedenleri açılır. Ana menü→Karşılaştır: iki kayıtlı dünyanın ayarlarını ve sonuçlarını gösterir. Karşılaştırılan kayıtlar aynı yaşta olmayabilir; iki dünya nedensellik kanıtı değildir.
