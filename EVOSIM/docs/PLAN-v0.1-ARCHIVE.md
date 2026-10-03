# EVOSIM — Ana tasarım ve uygulama planı

**Belge tarihi:** 15 Eylül 2026  
**Plan sürümü:** 1.0  
**Durum:** Uygulama öncesi tasarım; aşağıdaki sürümler henüz geliştirilmiş değildir.  
**Belgenin amacı:** EVOSIM'in ilk çalışan dünyasından uzun süreli araştırma hobisine kadar bütün geliştirmeyi aynı kurallarla yönlendirmek.  
**Kanonik dosya:** `outputs/plan.md`. Proje deposu oluşturulduğunda bu belgenin bir kopyası `docs/plan.md` içine alınır; iki bağımsız ana plan tutulmaz.

## İçindekiler

- [1. Kullanım, karar statüleri ve değişiklik yöntemi](#section-01)
- [2. Amaç ve ürün ilkeleri](#section-02)
- [3. Karar defteri](#section-03)
- [4. Nedensellik sözleşmesi ve parametre düzeni](#section-04)
- [5. Dünya modeli ve ekoloji şartnamesi](#section-05)
- [6. Geliştirme boyunca korunacak deney disiplini](#section-06)
- [7. Yaşam, karar ve hareket mekanikleri](#section-07)
- [8. Zaman, bilgi ve neden–sonuç sözleşmesi](#section-08)
- [9. Altı kalıtsal grup, değişken durumlar ve türetilen kapasiteler](#section-09)
- [10. Büyüme, enerji ve bedensel hareket](#section-10)
- [11. Algı: Canlı yalnızca erişebildiği bilgiyi kullanır](#section-11)
- [12. Kesin karar önceliği](#section-12)
- [13. Yön bulma ve yerel kaçınma](#section-13)
- [14. Paylaşılan kaynaklar, saldırı ve tick sırası](#section-14)
- [15. Üreme ve çok düşük mutasyon](#section-15)
- [16. Soy, ölüm ve gelecek davranışların sınırları](#section-16)
- [17. Kabul kapıları](#section-17)
- [18. Mekanikleri uygulamaya dönüştüren tamamlayıcı kararlar](#section-18)
- [19. Görsel kimlik ve asset üretim sistemi](#section-19)
- [20. Kullanıcı deneyimi: Bir deneyin başından sonuna](#section-20)
- [21. Teknik mimari, GPU kullanımı ve deney güvenilirliği](#section-21)
- [22. Sürüm yol haritası ve tamamlanma ölçütleri](#section-22)
  - [v0.0 — Proje iskeleti ve görsel dil denemesi](#section-22-version-01)
  - [v0.1 — İlk yaşayan nehir vadisi](#section-22-version-02)
  - [v0.2 — Güvenilir deney laboratuvarı](#section-22-version-03)
  - [v0.3 — Mevsimler ve çevresel bozulmalar](#section-22-version-04)
  - [v0.4 — Yuva ve çevre mühendisliği](#section-22-version-05)
  - [v0.5 — Bağlar, gruplar ve sınırlı hafıza](#section-22-version-06)
  - [v0.6 — Ölçülmüş ölçek büyütme ve koşullu GPU hesabı](#section-22-version-07)
  - [v0.7 — Nöroevrim araştırma kolu](#section-22-version-08)
  - [v0.8 — Deneyimden öğrenme araştırması](#section-22-version-09)
  - [v0.9 — İsteğe bağlı hafif 3D ve sunum olgunluğu](#section-22-version-10)
  - [v1.0 — Kararlı kişisel laboratuvar](#section-22-version-11)
- [23. İlk uygulama iş listesi ve bağımlılıklar](#section-23)
- [24. Gelecek mekanikler: 72 fikrin etki ve sürüm matrisi](#section-24)
- [25. Deney kütüphanesi ve ölçü tanımları](#section-25)
- [26. Rastgelelik, kayıt biçimi ve hata davranışı ayrıntıları](#section-26)
- [27. Test, kalibrasyon ve sürüm kabul stratejisi](#section-27)
- [28. Bakım, geliştirme düzeni ve risk kaydı](#section-28)
- [29. Kısa terimler sözlüğü](#section-29)
- [30. Planın kapanış kararı ve sıradaki işlem](#section-30)

---

<a id="section-01"></a>

## 1. Kullanım, karar statüleri ve değişiklik yöntemi

Bu belge hem ürün tasarımı hem simülasyon şartnamesi hem de uygulama sırasıdır. Özellik listesinde bir mekanizmanın bulunması, ilk sürüme alınacağı anlamına gelmez. Her geliştirme işi ilgili sürümün kapsamından ve kabul ölçütlerinden çıkarılır.

Karar statüleri:

- **Sabit karar:** Kullanıcıyla uzlaşılan veya bu planda uygulama temeli olarak seçilen davranış. Değişirse gerekçesi ve etkilenen deneyler kaydedilir.
- **Başlangıç değeri:** İlk kalibrasyon için sayı; biyolojik sabit veya ölçülmüş performans değildir. Değişmesi beklenebilir.
- **Koşullu özellik:** Açık bir ihtiyaç, ölçüm veya önceki sürümün başarısı gerçekleşince geliştirilir.
- **Gelecek araştırması:** Ana oyuna zorunlu olarak girmeyen, ayrı deney kollarında sınanacak fikir.

Uygulamada bir çelişki görülürse önce bu belgedeki **Karar defteri** esas alınır. Bir parametrenin tek kanonik tanımı yapılandırma şemasında tutulur. Kod içindeki geçici bir düzeltme ile belgenin sessizce ayrışmasına izin verilmez.

Her anlamlı model değişikliği için şu kayıt tutulur: karar kimliği, eski davranış, yeni davranış, neden, beklenen etkiler, kabul kontrolü, model sürümü. Yalnızca renk veya arayüz değişen sürümler ile biyolojinin değiştiği sürümler ayrı işaretlenir.

<a id="section-02"></a>

## 2. Amaç ve ürün ilkeleri

EVOSIM, kuş bakışı izlenen karasal bir yapay yaşam laboratuvarıdır. Kullanıcı dünyanın başlangıç koşullarını ve kurallarını belirler; canlıların kaynaklar, tehlikeler, diğer canlılar ve üreme maliyetleriyle ilişkisini gözlemler. Uzun vadede incelenen şeyler nüfusun dağılımı, soyların devamı, kalıtsal özelliklerin dağılımı, davranışların yaygınlaşması ve canlıların çevrede bıraktığı izlerdir.

### Başarı tanımı

1. Bir akşam açılıp keyifle izlenebilen sade ve akıcı bir dünya oluşur.
2. Kullanıcı bir canlının neden bir eylemi seçtiğini ve bir nüfus değişiminin olası nedenlerini inceleyebilir.
3. Aynı başlangıçtan farklı kurallarla deneyler yapılabilir; sonuçlar tekrarlı koşularla karşılaştırılabilir.
4. Yeni özellikler mevcut ihtiyaç, kapasite, maliyet ve kaynak ilişkilerine bağlanır.
5. Görsel kalite arttığında simülasyonun kuralları kendiliğinden değişmez.
6. Nüfusun çökmesi, beklenen davranışın oluşmaması ve bir soyun yok olması geçerli deney sonuçlarıdır.

### Temel sınırlar

- Sonuçlar, tanımlanan yapay model ve koşulları hakkında yorumlanır. İnsan toplulukları veya gerçek biyoloji hakkında doğrudan genel kanıt sayılmaz.
- Evrim, bireyin büyümesi, deneyimle öğrenmesi ve yazılıma yeni mekanik eklenmesi ayrı olaylardır.
- Yalnızca kodda mümkün olan eylemler gerçekleştirilebilir. Örneğin malzeme taşıma ve yerleştirme tanımlanmadan inşaat davranışı ortaya çıkmaz.
- Zengin davranış için her davranışa ayrı bir kişilik kaydırıcısı eklenmez. Az sayıda kalıtsal grup ile değişen durumlar ve ortak çevre kuralları birleştirilir.
- Başarılı bir soy mutlaka en hızlı veya en saldırgan soy değildir. Avantajların maliyetleri açıkça tanımlanır.
- Kullanıcı müdahaleleri, otomatik afetler ve normal simülasyon olayları kayıtta ayrılır.
- Uygulama tamamen yerel çalışmayı esas alır; ilk sürüm için bulut hizmeti, ücretli veri kaynağı veya sürekli ağ bağlantısı gerekmez.

<a id="section-03"></a>

## 3. Karar defteri

| Kimlik | Karar | Statü / açıklama |
|---|---|---|
| K01 | Unity 6.3 LTS + C# | Motor sürümü proje başlangıcında uygun yama sürümüne sabitlenir; yükseltmeler ayrı doğrulanır. |
| K02 | Motor nesnelerinden bağımsız simülasyon modeli | C# referans çekirdeği ve açık hızlandırıcı adaptörleri. |
| K03 | Karasal, tepeden organik estetik | Thrive referansındaki yumuşak saydam canlı hissi; toprak, bitki ve nehirler ayrı karasal palet. |
| K04 | Ortografik kamera; zemine 90° bakış | İleride en fazla yaklaşık 85°; serbest perspektif kamera ürün hedefi değil. |
| K05 | URP'nin 3D/Universal Renderer yolu, XZ düzleminde dünya | İlk nesneler düz yüzeylerdir. İleride 3D modeller için kamera ve veri düzeni korunur; önceki genel 2D Renderer önerisi bu kararla netleştirilmiştir. |
| K06 | Sabit simülasyon adımı 0,05 saniye, 20 Hz | Başlangıç çözünürlüğü; hızlandırma, aynı adımlardan daha çok çalıştırılarak yapılır. |
| K07 | Karar değerlendirmesi normalde 5 Hz | Canlı kimliğine göre dağıtılır; kritik tehlike olayları sıradaki simülasyon adımında karar kesmesi yaratabilir. |
| K08 | Yavaş ekoloji başlangıçta 1 Hz | Tam sayı adım aralıkları; alt sistemler arası güncelleme sırası açık. |
| K09 | İlk harita 128 × 128 çevre hücresi | Hareket sürekli koordinatlıdır; hücreler ekolojik ve uzamsal sorgu altyapısıdır. |
| K10 | İlk başlangıç nüfusu 150 otçul + 15 etçil | Kalibrasyon değeri; ayrı başlangıç kümeleri, rastgele özellik çeşitliliği. |
| K11 | İlk sürüm eşeyli üreme | Uyumlu iki farklı yetişkin; zorunlu erkek/dişi rolleri ilk modelde yok. Enerji katkısı ve çocuk sayısı kayıtlı. |
| K12 | Altı kalıtsal özellik grubu | Beden, algı, beslenme, suya uyum, atılganlık, sosyal bağ. Sonraki ekler karar kaydı gerektirir. |
| K13 | Ebeveyn kalıtsal değerleri arasında rastgele yavru değeri | Ortalama ve ikili ebeveyn seçimi yerine özellik başına aralık örnekleme. |
| K14 | Mutasyon yavru başına 0,001 olasılıkla başlar | %0,1 başlangıç kalibrasyon değeri; olduğunda seçilen bir kalıtsal gruptaki bir değer değişir. Olasılık özellik sayısıyla farkında olmadan katlanmaz. |
| K15 | Mutasyon çoğunlukla küçük, iki yönlüdür | Nadir daha büyük sapmalar; değişim ölçeği ebeveyn farkına bağlı değildir. Sıfır mutasyon deney ayarı da desteklenir. |
| K16 | İlk sürümde önceden sinir ağı eğitimi yok | İhtiyaç/risk temelli karar kuralları ve kalıtsal eğilimler; model varsayımları görünür. |
| K17 | İlk çevre olayı kuraklık; ilk barınaklar hazır korunak | İnşaat v0.4; seller, yangınlar ve mevsimler v0.3. |
| K18 | Her mekanik nedensel etki sözleşmesi taşır | Tetikleyici → dünya/durum/maliyet etkisi → karar etkisi → ölçüm → karşı deney. |
| K19 | Popülasyon ve enerjiye sessiz müdahale yok | Otomatik yeniden doğurma, gizli besin takviyesi, sessiz nüfus budama yapılmaz. |
| K20 | CPU referans koşusu bilimsel temel | GPU hızlandırmalı koşu backend ve sayısal sınırlarıyla etiketlenir; bit düzeyinde platformlar arası aynılık vaat edilmez. |
| K21 | GPU kullanımı baştan çizimde; hesapta ölçüme göre | Hedef GPU doluluk yüzdesi değil, doğru sonuç başına duvar saati ve enerji verimi. |
| K22 | Kamera, grafik kalite ayarı ve kozmetik rastgelelik biyolojiyi değiştirmez | Görsel LOD ile simülasyon ayrıntısı azaltılmaz. |
| K23 | Başlangıç, mevcut RNG durumu, sürümler ve yapılandırma saklanır | Seed tek başına devam kaydı değildir. |
| K24 | Torun sayıları benzersiz canlı kimlikleri üzerinden hesaplanır | İki akraba dalı yeniden birleştiğinde aynı torun iki kez sayılmaz. |
| K25 | Sinir ağı evrimi ve yaşam boyu öğrenme ayrı deney kolları | Ortak eğitilmiş politika bütün canlılara kendiliğinden bireysel öğrenme vermez. |

<a id="section-04"></a>

## 4. Nedensellik sözleşmesi ve parametre düzeni

### Üç veri katmanı

**Kalıtsal potansiyel:** Doğumda belirlenen genom değerleri. Yetişkin beden hedefi, algı kapasitesi, sindirim uyumu ve davranış eğilimleri burada bulunur. Kıtlık, yara veya korku bu değerleri doğrudan değiştirmez.

**Bireysel durum:** Yaş, kütle/gelişim, enerji, hidrasyon, sağlık, konum, hareket, hedef, üreme durumu; ileride yük, deneyim ve hafıza. Koşullarla değişir ve kalıtsal değerlerle birlikte mevcut kapasiteyi belirler.

**Çevre ve kurallar:** Kaynak stokları ve yenilenmesi, arazi geçirgenliği, nem, sıcaklık, korunaklar, afetler, tüketim ve hareket maliyetleri. Kullanıcının dünya seçenekleri bu katmanda yoğunlaşır.

Bir olayın anlamlı olması her zaman yeni bir bireysel sayaç gerektirmez. Bir nehir kolunun kapanması ulaşılabilir hedefleri değiştirir; bu gerçek bir mekanik etkidir. Sadece görünüşü değiştiren süsler ayrı kozmetik listesinde bulunur ve deneysel etki iddiası taşımaz.

### Her yeni özelliğin zorunlu kaydı

| Alan | Açıklama |
|---|---|
| Mekanik kimliği ve sürümü | Örneğin `ECO_DROUGHT_01`. |
| Tetiklenme koşulu | Kullanıcı olayı, hava süreci, temas, yaş veya başka açık neden. |
| Girdi ve ölçü birimleri | Saniye, dünya birimi, normalize kapasite, enerji birimi. |
| Doğrudan etki | Hangi stok, kapasite, risk veya durum ne kadar değişiyor? |
| Bedel ve sınırlama | Enerji, zaman, alan, malzeme veya karşılaşma riski. |
| Karar sistemine bağlantı | Hangi gözlem ve hangi eylem faydası etkileniyor? |
| Kalıtım / öğrenme bağlantısı | Varsa hangi grup; durum ve gen karıştırılmaz. |
| Gözlenebilir sonuç | Olay günlüğü, grafik, harita veya birey açıklaması. |
| Karşı deney | Özellik kapalı veya tek katsayı değiştirilmiş eş başlangıç. |
| Kontrol | Kaynak korunumu, sınır durumu, performans, kayıt/yeniden yükleme. |

### Örnek: kuraklık

Kuraklık olayının şiddeti ve süresi hava sisteminde tutulur. Nem ve su yenilenmesi etkilenir; bitki yenilenmesi nemden hesaplanır. İçilebilir su azalınca bireylerin ulaşabildiği su kaynakları değişir. Susuz kalan bireyin kapasitesi ve su arama faydası etkilenir. Ne kadar bireyin ne sebeple öldüğü, göç mesafesi, kıyı yoğunluğu ve hidrasyon dağılımı ölçülür. Kuraklık, özel bir `kuraklıkta_göç_et` kuralı üzerinden nüfusu başka yere taşımaz.

### Örnek: yuva

Yuva malzeme, taşıma ve yapım zamanı gerektirir. Konumuna göre dinlenme sırasında maruz kalmayı veya tehlikeden korunmayı değiştirir. Yuvanın kapasitesi sınırlıdır. Kullanıcının `yuva` anahtarı bütün canlıları otomatik ev sahibi yapmaz. Korunakların faydası ve inşaat maliyetleri karar hesabında ayrı değerlendirilir.

### Basit kullanıcı ayarları ve ileri ayarlar

Ana panelde başlangıç nüfusu, dünya tohumu, kaynak bolluğu, üreme biçimi, mutasyon olasılığı, afet sıklığı ve koşu süresi bulunur. Başlangıç sürümünde kullanılamayan mekanik etkinmiş gibi gösterilmez. İleri panelde model katsayıları açılır; bütün değerler bir deney yapılandırmasına kaydedilir. Her canlı için ayrı elle ince ayar normal deney akışı değildir; bireye müdahale ayrı olay olarak kaydedilir.

<a id="section-05"></a>

## 5. Dünya modeli ve ekoloji şartnamesi

### Koordinatlar, ölçek ve sınırlar

- Çekirdek konumu iki sayı olarak saklar: `x` ve `z`. Bir çevre hücresi başlangıçta 1 dünya birimidir.
- Harita sınırı `0 ≤ x < width`, `0 ≤ z < height` olarak tanımlıdır. İlk sürüm kapalı sınırlıdır; karşı kenardan çıkma yoktur.
- Unity'de düşey eksen Y'dir. Y yalnızca görüntü katmanı veya ileri arazi yüksekliği için kullanılır; erken sürüm hareketi düzlemdedir.
- Hücre çözünürlüğü ekolojiyi etkiler. Aynı seed ile çözünürlüğü değiştirmek aynı deney sayılmaz.
- Canlılar nokta değildir; yarıçapları geçiş, sınır ve temas kontrollerinde kullanılır.
- Yüklenecek harita dünya nesneleriyle birlikte doğrulanır: boş erişilebilir alan, içilebilir kıyı, başlangıçtaki besin dağılımı, engel yüzünden kapalı spawn noktaları.
- Kurulumda geçersiz bir harita hata olarak açıklanır. Geçerli koşu başladıktan sonraki doğal sıkışma veya yok oluş sessizce düzeltilmez.

### Çevre hücre alanları

| Alan | İlk sürüm | Gelecek kullanım |
|---|---|---|
| Kara/su/kaya sınıfı | Evet | Değişen arazi ve kazılabilirlik. |
| Su derinliği veya geçiş zorluğu | Basit sabit alan | Akış, sel, geçici su alanları. |
| Nem | Suya uzaklık ve kuraklık etkisi | Yağış, gölge, toprak tutma. |
| Bitki enerji stoku | Yenilebilir bitki miktarı | Farklı bitki türleri ve tohum bankası. |
| Kaynak yenilenme kapasitesi | Nemden türetilir | Toprak besini ve mevsim. |
| Geçirgenlik ve hareket maliyeti | Arazi ve engellerden | Patika, çamur, kar, yapı. |
| Korunak etkisi | Sabit korunak nesnelerinden | Yapı kalitesi, doluluk, hasar. |
| Sıcaklık, yakıt, hastalık yükü | İlk sürümde etkin değil | İlgili mekanik sürümünde şemaya eklenir. |

Görüntüdeki ağaç ile simülasyondaki ağaç farklı anlam taşımaz: engel olan gövdenin kapladığı alan, üretim kapasitesi ve varsa gölge alanı açık veriden gelir. Kozmetik bir ot parçası besin kaynağı gibi seçilemez. Gerçek kaynakların görünümü stokla ilişkilidir.

### Su ve bitki yerleşimi

İlk haritada bir ana nehir ve kıyı çevresinde nem gradyanı vardır. Ağaç ve yenilebilir bitki konumları karada, uygun nem ve boşluk koşullarında örneklenir. Bitki yüksekliği, ağaç rengi ve küçük desen farkları ayrı kozmetik rastgelelik kullanır. Böylece görüntü varyasyonunu değiştirmek nüfusun başlangıcını değiştirmez.

İlk sürümde su akışkanlar fiziğiyle çözülmez. Nehir geometrisi ile kıyı erişimi açıktır; kuraklık su stokları/yenilenme ve nem üzerinde tanımlı etkiler uygular. Su yatağının daralması eklendiğinde navigasyon maliyet sürümü güncellenir. Canlıların içmek için yüzmesi gerekmez; erişilebilir kara kıyısından içme mesafesi içinde kaynak tüketirler.

Bitki stoku her ekoloji adımında kapasiteye doğru yenilenir. Yenilenme miktarı toprak/nem katsayıları ve kalan kapasite ile sınırlıdır. Bir hücrede tüketim ve yenilenme sırası sabittir. Aynı anda beslenen canlılar aynı son enerji birimini birden fazla tüketemez. Besin stoku hiçbir adımda negatif olamaz.

### Kaynak ekonomisi

Dünya bütçesi dışarıdan gelen girdileri açık tutar: bitki üretimi çevresel bir girdidir; tüketim, hareket, metabolizma, yaralanma, çürüme ve üreme aktarımı kayıtlı süreçlerdir. Bu soyut enerji birimlerinin fiziksel joule olduğu iddia edilmez. İleride gerçek birimlere geçilecekse bütün katsayılar ve deney kimliği yeniden tanımlanır.

Üreme enerji üretmez. Yavrunun başlangıç enerjisi ve beden maliyeti ebeveynlerin katkısı ile tanımlı verimden gelir. Etçil, bir canlıya hasar verdiği için ayrıca enerji kazanmaz; enerji tüketilebilir besin aktarımıyla gelir. Ceset stokları, ölüm anındaki tanımlı bütçeden oluşur ve çürüme/tüketimle azalır.

### Afet sözleşmesi

Afetler etkinleştirilmiş bir dünyada ayrı olay akışından oluşur. `kind`, `startTick`, `durationTicks`, `footprint`, `intensity`, `cause`, `seedStream` kayıtları vardır. Uzun sakin dönemler varsayılandır. Olay planı, nüfusun “sıkıcı görünmesine” veya belirli soyun başarısına bakarak sessizce değiştirilmez.

Afetlerin bölge dışına etkisi açık zincirle gerçekleşir. Yangın doğrudan haritanın öbür ucundaki açlığı arttırmaz; yakıtı ve bitkiyi azaltır, bu değişim kaynak hareketini ve nüfusu etkiler. Aynı başlangıç koşusunda afet kapalı karşı deney yapılabilir.

İlk kuraklık için olay öncesi, olay sırası ve iyileşme pencerelerinde ayrı ölçüm alınır. Sel ve yangın eklendiğinde kaçılabilir güvenli bölgelerin her zaman bulunacağı garanti edilmez. Canlıların sıkışması veya tamamen yok olması raporda görünür.

### Yapı ve nesne modeli

Korunak, ceset, taş, dal, yuva ve bitki ayrı kimlikli nesneler olabilir. Dünya hücrelerinin alanlarıyla ilişkileri açıktır. Her nesnenin türü, konumu, kapsadığı alan, kalan stok/dayanıklılık, oluşma nedeni ve isteğe bağlı üretici kimliği saklanır. “Üreten kişi” ile “şu an kullanan kişi” ayrı bilgilerdir; v0.4 yapıları üreticileri ölünce kaybolmaz.

<a id="section-06"></a>

## 6. Geliştirme boyunca korunacak deney disiplini

- Önce küçük ve anlaşılır sistem; sonra daha fazla etkileşim.
- Her yeni mekanik için en az bir kontrol dünyası ve en az bir mekanik kapalı karşılaştırma.
- Harita, kaynak ve davranış katsayıları aynı anda değiştirilip tek nedene bağlanmaz.
- Grafiklerde ortalama yanında dağılım, nüfus büyüklüğü ve sağ kalan sayısı gösterilir.
- Sadece başarılı koşular değil, yok oluşlar ve performans sınırına takılan koşular da saklanır.
- Her canlı hedef seçiminde kendisine tanımlanan duyuların gördüğü bilgileri kullanır. İlk sürümde statik arazi rota servisine açıktır; bu navigasyon sadeleştirmesi manifestte belirtilir. Analiz sisteminin gizli kaynak ve canlı durumlarını bilmesi karar sistemine bu bilgiyi sağlamaz.
- Uygulama sürümü, model sürümü, kayıt şeması ve görsel varlık sürümü ayrı kaydedilir.
- Geliştirme sırasındaki hızlandırma, dünya kurallarının hızlandırma oranına bağlı çalışmasını doğurmaz.

<a id="section-07"></a>

## 7. Yaşam, karar ve hareket mekanikleri

> Bu bölüm bir simülasyon tasarımıdır. Formüller doğanın birebir açıklaması olarak kullanılmaz. **Sabit karar** sistemin sözleşmesini, **başlangıç değeri** ise deneyle ayarlanacak bir modeli anlatır. Parametre ekranında az sayıda anlaşılır denetim gösterilir; teknik katsayılar sürümlenmiş model profilinde tutulur.

<a id="section-08"></a>

## 8. Zaman, bilgi ve neden–sonuç sözleşmesi

**Sabit karar:** Simülasyon 20 Hz, `dt = 0,05 s` sabit zaman adımıyla ilerler. Bütün süreler tamsayı tick üzerinden tutulur. Karar güncellemesi 5 Hz'dir; bireylerin fazları `creatureId % 4` ile dağıtılır. Ekoloji 1 Hz'de, her 20 tick'te güncellenir. Çizim kareleri bu saati değiştirmez. Acil tehlike ve kaynak/rota geçerliliği her tick kontrol edilir; canlı yeni karar sırasını beklerken boğulmaz veya silinmiş besine yürümeyi sürdürmez.

Her mekanik aşağıdaki zincirde tanımlanır:

`olay → çevre/durum değişimi → kapasite veya maliyet değişimi → algılanan seçenekler → karar → sonuç → ölçüm`

Kuraklık geni değiştirmez; suyu ve yenilenme hızını değiştirir. Yuva kendiliğinden mutluluk puanı üretmez; dinlenme maliyetini veya dış etkiye maruziyeti değiştirir. Bir özellik avantaj sağlıyorsa enerji, zaman, malzeme, kapasite sınırı veya risk bakımından karşılığı bulunur. Doğrudan etkisi ve ölçümü olmayan yeni mekanik eklenmez.

<a id="section-09"></a>

## 9. Altı kalıtsal grup, değişken durumlar ve türetilen kapasiteler

İlk genom altı sürekli, normalize değer içerir. İleride bir grubun genişlemesi genom şema sürümünü artırır; bağımsız davranış başına yeni gen açılmaz.

| Gen grubu | İlk temsil | Türetilen sonuç ve bedel |
|---|---|---|
| Beden | Yetişkin beden potansiyeli | Güç, depo, erişim yarıçapı; daha yüksek bakım maliyeti ve düşük çeviklik |
| Algı | Algı kapasitesi | Görüş mesafesi ve açısı; algılama bakım maliyeti |
| Beslenme | Bitkisel–hayvansal uyum | Besinden yararlanma verimi; iki uçta aynı anda tam verim yok |
| Su uyumu | Yüzme kapasitesi | Aşılabilir su zorluğu, yüzme hızı; karada bakım veya hareket bedeli |
| Atılganlık | Risk ve yaklaşma eğilimi | Tehdide/çatışmaya yaklaşma değerlendirmesi; güvenliği geçersiz kılmaz |
| Sosyal bağ | Yakın durma ve bağ kurma eğilimi | Eş/yavru etkileşim ağırlığı; temas ve zaman maliyeti |

İlk otçul ve etçil nüfusları farklı beslenme değer aralıklarından başlatılır. Başlangıç sindirim modeli `s ∈ [0,1]` için `eta_plant = 0.05 + 0.95(1-s)^2`, `eta_meat = 0.05 + 0.95s^2` olarak önerilir. Verim gerçek tüketilen enerjiye uygulanır; sabit otçul veya etçil enerji bonusu yoktur. Ara değerler iki kaynaktan yararlanabilir, her ikisinde uzman verimini elde etmez. Görünür otçul/etçil etiketi ekolojik gruptur; yeni biyolojik tür oluştuğu iddia edilmez. Ara değerler ve sınıflandırma değişimi bir olay olarak kaydedilir.

**Anlık durum:** konum, yön, hız, yaş, enerji rezervi, beden dokusu, su rezervi, sağlık, yaralanma, üreme bekleme süresi, geçerli eylem ve hedef. Ebeveyn kimlikleri, doğum tick'i ve genom doğumdan sonra değişmez. v0.5'te tanışıklık ve yer hafızası eklenir; bunlar genom değildir.

**Türetilen kapasite:** mevcut beden, azami hız, ivme, dönüş hızı, kuvvet, görüş, depo sınırı ve çevresel tolerans. Kaydedilen temel durumdan hesaplanır; farklı sistemlerde ikinci bir kopyası bağımsız değiştirilmez.

<a id="section-10"></a>

## 10. Büyüme, enerji ve bedensel hareket

Beden kütlesi görünüş ve fiziksel kapasitenin ortak kaynağıdır. Yaş büyümeye izin verir; dokunun oluşması kaynak gerektirir. Yavruya bedelsiz et kütlesi eklenmez.

```text
m_age = smoothstep(0, 1, age / maturityAge)
m_nutrition = clamp((B - B_birth) / (B_adult - B_birth), 0, 1)
m = min(m_age, m_nutrition)
bodyMass = juvenileMass + (adultMass - juvenileMass) * m
radius = radiusScale * sqrt(bodyMass)
v_max = v_base * bodyMass^(-0.25)
        * (0.35 + 0.65 * m) * injuryFactor * fatigueFactor * terrainFactor
```

Üs ve `0.35` **başlangıç katsayılarıdır**. Doku hedefi yaşın izin verdiği sınırı aşmaz; fiziksel kütle ile doku enerjisi eşlemesi aynı model sabitini kullanır. Benzer olgunlukta küçükler genel olarak çevik, büyükler güçlüdür; yavru küçük olduğu için yetişkinlerden hızlı olmaz. Dönüş hızı ve ivme de bedenle azalır. Yürüyüş `position += velocity * dt` ile hesaplanır; hız ve yön hedefe aniden sıçramaz. Yavaşlama mesafesi varış hızına göre belirlenir. Yerçekimi, tam rijit gövde fiziği ve zıplama v0.1 kapsamına girmez.

```text
E_next = E + assimilatedFood
           - dt * (basalCost + perceptionCost + movementCost + exposureCost)
           - actionCost - growthCost - reproductiveTransfer
movementCost = k_move * bodyMass * (realizedSelfMotionSpeed / v_reference)^2 * terrainCost
```

Hareket maliyeti, bu tick'teki hareket çözümünden sonra canlının kendi ilerlemesiyle gerçekleşen yol uzunluğunun `dt`ye bölünmesiyle hesaplanır; niyet hızı veya önceki tick hızı ücretlendirilmez. Temas çözümünün küçük ayırma düzeltmeleri kendi ilerlemesi sayılmaz. Birden çok arazi parçası geçilirse maliyet, parçaların yol/süre paylarıyla biriktirilir. Başlangıçta taban ve algı giderleri hesaplanır; harekete bağlı enerji ve su giderleri çözümden sonra eklenir.

Su rezervi metabolizma, hareket ve çevre sıcaklığıyla azalır; içme rezervi üst sınıra kadar doldurur. Rezerv sıfırken enerji veya su açığı sağlığa zarar verir. İyileşme ayrıca enerji gerektirir. Sağlık sıfır olduğunda ölüm oluşur. Açlık ve susuzluk sinyalleri depoların doluluk oranlarından türetilir.

Doku enerjisi `B`, kullanılabilir enerji `E`'den ayrı tutulur. Büyüme `E → B` dönüşümüdür ve verimi en fazla birdir. Üremede ebeveynden düşülen enerji yavrunun `E+B` toplamını ve üreme kaybını karşılar. Leşin yenebilir enerjisi ölüm anındaki kayıtlı rezerv/doku ile sınırlıdır. Böylece doğum, büyüme ve leş üzerinden enerji çoğaltma açığı oluşmaz. Bitki fotosentezi modelde açık bir dış enerji girdisidir; bütün dünya kapalı enerji sistemi gibi sunulmaz.

<a id="section-11"></a>

## 11. Algı: Canlı yalnızca erişebildiği bilgiyi kullanır

Görüş mesafesi ve açısı genetik potansiyel ile olgunluğun birleşimidir. Yavrunun algısı sınırlıdır; yaşlılık kapasiteyi azaltabilir. Başlangıç profili yetişkin görüş açısını örneğin 70–220 derece aralığına eşler; gerçek aralık küçük deneylerle ayarlanır.

Algılama önce yakın hücreleri tarar, sonra mesafe, bakış açısı ve görüş hattı filtresi uygular. Kayalar ve yoğun bitki engelleyicileri görüşü kesebilir; su tek başına opak duvar değildir. Tür etiketi, genom, bütün haritanın besinleri veya hedefin iç enerji değeri karar sistemine bedelsiz verilmez. Algılanabilen beden, hareket ve görünür tehdit bilgisi kullanılır.

v0.1'de küresel keşfedilmiş harita hafızası yoktur. Algılanan bir hedef seçildikten sonra yol bulucu arazi üzerinden rota üretebilir; bu bir **harita bilgisi sadeleştirmesi** olarak deney manifestinde belirtilir. Hedef seçimi gizli besinleri keşfetmez. v0.5'te keşfedilmiş alan ve son görülme zamanı eklenerek arazi bilgisi de sınırlandırılır.

Hedef görüşten çıkınca kısa bir doğrulanmış son konum süresi tutulur; bu süre dolduğunda iz sürme sona erer. Yeni algı verisi olmadan hedefin gerçek konumu izlenmez. Tehlikenin çok yakına girmesi, normal 5 Hz karar temposunu beklemeden acil değerlendirme başlatır.

<a id="section-12"></a>

## 12. Kesin karar önceliği

Karar sistemi iki aşamalıdır: önce zorunlu güvenlik/geçerlilik, ardından ihtiyaca dayalı fayda hesabı. Her kararın nedeni ve en iyi birkaç alternatifin puanı inceleme panelinden görülebilir.

### Zorunlu katman

1. **Ölüm/işlevsiz durum:** ölü canlı eylem üretemez. Geçersiz hedef ve kapasiteyi aşan rota iptal edilir.
2. **Yakın ölüm riski:** yangın, boğulma, aktif saldırı, kritik susuzluk/açlık birlikte değerlendirilir. Sabit biçimde her zaman avcıyı açlığa üstün saymak yerine tahmini ölüme kalan süre kullanılır.
3. **Güvenli çıkış veya yaşamı uzatan eylem:** tehlikeden kaçış, kıyıya dönüş, acil içme/beslenme. Hesap yalnızca bilinen durumdan yapılır.

Yakın ölüm sınıfı için `tau <= emergencyHorizon` koşulu kullanılır. Eşit tahmini sürelerde sıra: mevcut zararlı çevreden çıkma, aktif saldırıdan kaçma, su, besin; son bağlayıcı eylem kimliğidir. Kaçış mümkün değilse ve savunma beklenen hayatta kalma süresini artırıyorsa saldırı seçilebilir. v0.1'de yavru koruma, yakın ölüm katmanını geçersiz kılmaz.

### Normal fayda katmanı

```text
U(action) = needPressure * expectedBenefit * successEstimate
            - energyCost - travelTimeCost - injuryRisk - opportunityCost
```

Terimler boyutsuz ortak aralığa normalize edilir. Açlık/susuzluk basıncı depo azaldıkça doğrusal olmayan biçimde yükselir. Geçerli adaylar: içme, beslenme, avlanma, kaçınma, dinlenme, eş arama/eşleşme ve keşif. Kaynak yoksa o eylem aday değildir. v0.1 keşfi erişilebilir yönlerden kararlı bir ara hedef seçer ve bir süre sürdürür; her karede rastgele dönmez. v0.5 bellek eklendiğinde az ziyaret edilmiş/keşfedilmemiş alan tercihi ayrıca etkinleşir.

**Kararlılık kararı:** mevcut eyleme devam bonusu ve asgari sürdürme süresi uygulanır. Başlangıçta normal eylem değişimi için yeni puan en az `0.08` daha iyi olmalı ve `0.5 s` geçmelidir. Acil durum, kaybolan kaynak, imkânsız rota veya hedef ölümü bu beklemeyi keser. Eşit puanlar sırasıyla mevcut eylem, eylem kodu, hedef kimliği ile çözülür. Rastgelelik gereken keşif ayrı, yeniden üretilebilir akış kullanır.

```text
if dead: return
validateCurrentAction()
if imminentLethalRisk(): chooseBestSurvivalAction(); return
if decisionTickFor(id):
    candidates = buildLegalActions(perceivedWorld, state, phenotype)
    scored = score(candidates)
    next = stableArgMax(scored)
    if maySwitchWithHysteresis(next): commit(next)
```

Yiyecek, eş ve su için görünmez kaynak rezervasyonu yoktur. Karar dünyayı sahiplenmez; eylemin gerçekten gerçekleştiği tick'te kaynak uygunluğu yeniden doğrulanır.

<a id="section-13"></a>

## 13. Yön bulma ve yerel kaçınma

Dünya 128 × 128 çevre hücresidir; canlı konumu süreklidir. Küresel rota 8 komşulu A* ile bulunur. Çapraz köşe kesme engellenir. Düğüm maliyeti mesafe, arazi enerji maliyeti ve bilinen tehlikeden oluşur; eşit maliyetli düğümler hücre kimliğiyle sıralanır. Geçilemez hücreye yüksek maliyet verip geçmesine izin verilmez.

Her canlı her kararında baştan yol aramaz. Hedef, geçirgenlik sürümü, rota koridoru veya kapasite değişince rota geçersizleşir. Rota istekleri tick başına bütçelenir; bekleyen canlı güvenli yerde yavaşlar. Kesilmiş bir rotaya devam ederek nehri aşmaz. Uzun rotalar geçerli yakın ara hedeflere ayrılır.

Yerel hareket; rota yönü, komşulardan ayrışma, engelden kaçınma ve varış yavaşlamasını birleştirir. Kuvvetler ivme sınırıyla uygulanır. Bir canlının sıkışması görünmez ışınlanmayla çözülmez: süre eşiğinde yeni ara hedef veya rota aranır. Kalabalıkta karşılıklı kilitlenme için kimlikten türeyen tutarlı geçiş önceliği uygulanır; bu önceliğin kaynak erişimine sistematik etkisi ölçülür.

Suya yaklaşımda içme hedefi su hücresinin merkezi değil, suya komşu erişilebilir kıyı noktasıdır. Yüzme uygunluğu su derinliği, akış zorluğu ve kapasite sınırıyla belirlenir. Erişilemeyen su geçidi rota aramasından çıkarılır. Akarsu taşınca içeride kalan canlı kıyı arar; yüzme enerjisi yetmezse maruziyet hasarı alır. Yüzebilen canlı da keyfî hızla nehri aşmaz.

<a id="section-14"></a>

## 14. Paylaşılan kaynaklar, saldırı ve tick sırası

Her tick mevcut durumdan niyetler üretilir; paralel işler ortak kaynağı doğrudan değiştirmez. Sonuçlar sabit sırayla tek commit aşamasında uygulanır.

1. Bu tick'e ait kullanıcı komutları, planlı çevre olayları ve o tick'e denk gelen ekoloji uygulanır.
2. Başlangıç durumu sabitlenir; konumsal indeks, kapasite/algı/acil karar ve normal karar fazı hesaplanır. Taban ve algı borcu bu aşamada hesaplanır fakat henüz düşülmez; hareket borcu gerçekleşen hareket çözüldükten sonra eklenir.
3. Hareket niyetleri, geçirgenlik ve yerel temas çözümü hesaplanır.
4. Son konumdan beslenme/içme/saldırı/eşleşme niyetlerinin geçerliliği doğrulanır.
5. Paylaşılan kaynak tahsisi ve eşzamanlı hasar hesapları tamamlanır.
6. Tüketim rezervlere aktarılır; eşzamanlı hasar ve zorunlu giderlerin karşılanamayan kısmı uygulanır. Yaş ilerletilir ve sağlık/azami yaş ölüm koşulları kilitlenir. Yalnız sağ kalanlara rezervleri ölçüsünde büyüme ve iyileşme uygulanır; ölüm kilidi aynı tick içinde geri alınamaz.
7. Hâlâ geçerli üreme işlemleri atomik olarak yapılır; doğum/ölüm commit'i, olaylar ve ölçümler yayımlanır. Ölen ebeveyn aynı tick'in sonunda üreyemez.

Aynı kaynak için talepler toplamı miktarı aşarsa oransal paylaştırılır. Sayısal kalan, `(tick, resourceId)` kaynaklı döner sıra ile dağıtılır; düşük kimlikli canlı her zaman önce yemez. Miktarlar negatif olmaz. Aynı tick'teki saldırılar başlangıç geçerliliğinden hesaplanıp birlikte uygulanır; iki canlı aynı tick'te birbirini öldürebilir. Sonraki tick leş tüketimi açılır; ölüm işlemi iki kez çalışmaz.

Saldırı menzil, yön ve bekleme süresi gerektirir. Kuvvet beden/sağlıkla, maliyet ve darbe aralığı bedenle ilişkilidir. Atılganlık doğrudan hasar çarpanı değildir; saldırı kararını etkiler. Otçul savunabilir; etçil etiketi zorunlu sürekli saldırı anlamına gelmez. Besin tüketim hızı sınırlıdır; büyük bir leş tek tick'te yutulmaz.

<a id="section-15"></a>

## 15. Üreme ve çok düşük mutasyon

v0.1'de eşeyli üreme iki farklı, üreme bakımından uyumlu ve yeterli rezervi bulunan birey ister. Üreme olgunluğu `age >= maturityAge` ve `m >= 0.95` koşullarının birlikte sağlanmasıdır; 0.95 başlangıç model eşiğidir. Uyumluluk deneyin kurucu üreme grubu etiketidir; ilk otçul ve etçil kurucu grupları kendi içlerinde eşleşir. Ekolojik beslenme etiketi değişse de üreme grubu kendiliğinden değişmez. Zorunlu erkek/dişi rolleri ilk modelde yoktur; iki ebeveynli üreme açık bir sadeleştirmedir. Üreme uzaklık, iki tarafın geçerli niyeti ve bekleme süresi koşullarına bağlıdır. Bir birey bir tick'te yalnızca bir eşleşmeye katılır. Çift anahtarı `(minId, maxId)` ile yinelenme önlenir. Yakın akraba eşleşmeleri yasaklanmaz; kaydedilip ayrıca ölçülür.

Her sürekli kalıtsal değer için:

```text
childGene[i] = Uniform(min(parentA[i], parentB[i]), max(parentA[i], parentB[i]))
mutationEvent = Bernoulli(p_mutation_per_birth)
if mutationEvent:
    locus = UniformChoice(6 loci)
    delta = 0.95 olasılıkla Normal(0, 0.025), aksi hâlde Normal(0, 0.10)
    childGene[locus] = reflectIntoUnitInterval(childGene[locus] + delta)
```

**Başlangıç değeri:** `p_mutation_per_birth = 0.001`, yani yaklaşık her bin doğumda bir mutasyon olayı beklentisi. Bu gen başına olasılık değildir. Bir mutasyonun ebeveyn aralığı dışına çıkması mümkündür; kesin değildir. Delta ebeveyn farkıyla çarpılmaz; aynı değerde iki ebeveyn de çeşitlilik üretebilir. Sınırda kırpma yerine yansıtma kullanılır; yöntem kayıt sürümüne dâhildir. Değerler kalibrasyonla değişebilir, koşu ortasında sessizce değişmez. Dünya zorlandığı veya bir özellik gerekli olduğu için mutasyon tetiklenmez; olasılık çevrenin ihtiyacına bakmaz. Düşük mutasyon bir dengeye yakınsamayı garanti etmez; salınım veya yok oluş da geçerli sonuçtur.

Doğumdan önce iki ebeveynin enerji aktarımı ve erişilebilir yavru konumu birlikte doğrulanır. Uygun yer yoksa işlem ertelenir; enerji düşülmez. Doğum, ebeveyn enerji kesintileri, genom ve soy kaydı tek atomik işlemdir. Yavru güvenli kara noktasına yerleşir; dolu hücreye veya kapasitesini aşan suya doğmaz. İlk sürümde gebelik yerine işlem ve bekleme süresi kullanılır; gebelik/litter sistemi sonraki bir model değişikliğidir.

<a id="section-16"></a>

## 16. Soy, ölüm ve gelecek davranışların sınırları

Kimlik `runId + monotonicCreatureId` ile benzersizdir; ölen kimlik yeniden kullanılmaz. Başlangıç canlıları kurucu olarak işaretlenir. `generation = 1 + max(parentGeneration)` tanımdır; genel zaman yerine kullanılmaz. İki ebeveynli soy bir ağaçtan çok yönlü döngüsüz grafiktir. Torun sayımında aynı bireye iki ebeveyn yolu ile ulaşılsa da **tek kez** sayılır. Doğrudan yavru, tam iki ebeveyn-çocuk kenarıyla erişilen torun ve bütün altsoy ayrı gösterilir; yaşayan/toplam ölçümleri karıştırılmaz. Akraba eşleşmelerinde aynı birey bir atanın hem çocuğu hem torunu olabilir; kategori içlerinde kimlikler benzersizdir, kategorilerin toplamı bütün altsoy sayısı değildir.

Ölüm kaydı yaş, konum, genom, temel neden ve katkıda bulunan hasarları içerir. Aynı tick'te açlık ve saldırı varsa ikisi de saklanır; ana neden, aşağıdaki ölüm nedeni sözleşmesindeki o tick'in en büyük pozitif sağlık kaybı kuralından seçilir. Büyük altsoy sorguları her tick tüm genom grafiğini gezmez; önbellekli/asenkron yapılır ve gösterilen zaman belirtilir. Yok olan nüfusu otomatik yeniden doğurma, gizli kaynak ekleme veya mutasyonla kurtarma uygulanmaz. Kullanıcının açık müdahalesi ayrı, zaman damgalı deney olayıdır.

v0.4 yuva inşasında toplama, taşıma, bırakma temel eylemleri eklenir; malzeme miktarı, işçilik enerjisi, bozulma ve barınma etkisi tanımlanır. v0.5 koruma; bağ, algılanan yavru tehlikesi, müdahale maliyeti ve kaçış seçeneğinden hesaplanır. Canlı bütün türün geleceğini veya gizli soy ağacını bilmez. v0.7–v0.8'de evrilen/öğrenen beyinler aynı algı ve yasal eylem arayüzünü kullanır; hareket, kaynak ve enerji kurallarını atlayamaz.

<a id="section-17"></a>

## 17. Kabul kapıları

| Kapı | Başarı koşulu |
|---|---|
| Zaman | Aynı tick sayısında 1× ve hızlandırılmış koşu aynı referans durum özetini verir |
| Algı | Duvar arkasındaki veya görüş açısı dışındaki görünmeyen besin seçilemez |
| Hareket | Köşe kesme, yasak su geçişi, sınırsız ivme ve sıkışmada ışınlanma oluşmaz |
| İçme | Kara canlısı geçerli kıyı noktasından içer; içmek için nehre girmek zorunda kalmaz |
| Karar | Acil kaçış normal beklemeyi keser; eşit adaylarda yeniden oynatılabilir seçim oluşur |
| Kaynak | Çoklu tüketimde kaynak negatif olmaz; alınan toplam kaynak mevcut miktarı aşmaz |
| Enerji | Doğum, büyüme, leş zincirinde kayıtsız enerji üretimi bulunmaz |
| Çatışma | Paralel iş tamamlama sırası v0.1 referans sonucunu değiştirmez |
| Kalıtım | Mutasyonsuz örnekler ebeveyn aralığında kalır; eşit ebeveynlerde mutasyon çalışır |
| Mutasyon | Tohumlu geniş örneklemde olay frekansı belirlenen binom güven aralığıyla uyumludur |
| Soy | Ortak atalı ebeveynlerde altsoy çift sayılmaz; kimlikler yeniden kullanılmaz |
| Ekoloji | Kuraklık su/yenilenme alanını değiştirir; genomu doğrudan yazmaz |

v0.1 bu kapılarla küçük uçtan uca dünyayı kurar. v0.2 deney güvenilirliğini ve kayıtları sağlamlaştırır; v0.3 mevsimler/afetler, v0.4 inşa, v0.5 grup/hafıza katmanlarını açar. Daha sonraki hızlandırma ve öğrenme adımları ölçüme ve bu sözleşmelerin korunmasına bağlıdır.

<a id="section-18"></a>

## 18. Mekanikleri uygulamaya dönüştüren tamamlayıcı kararlar

### İlk modelin yaşam profili ve kalibrasyon tablosu

Aşağıdaki sayılar uygulanacak ilk profilin başlangıç değerleridir. Gerçek canlılarla fiziksel eşdeğerlik veya dengeli ekosistem garantisi taşımazlar. Değiştirildiğinde koşunun config hash'i değişir. Doğum/ölüm/enerji testlerinde küçük yapay profiller ayrıca kullanılabilir.

| Ayar | Başlangıç | Anlam |
|---|---|---|
| `tickSeconds` | 0.05 | Sabit hareket/yaşam adımı. |
| `decisionEveryTicks` | 4 | Normal karar periyodu. |
| `ecologyEveryTicks` | 20 | Yavaş çevre periyodu. |
| `mapWidth`, `mapHeight` | 128, 128 | Ekolojik hücre sayısı. |
| `cellWorldSize` | 1 | Hücre kenar uzunluğu. |
| `initialHerbivores`, `initialCarnivores` | 150, 15 | İki kurucu üreme kümesinin büyüklüğü. |
| `maturityAgeSeconds` | 120 | Yaşa bağlı gelişimin yetişkinliğe ulaşma eşiği; beslenme yetersizse beden geride kalabilir. |
| `senescenceStartSeconds` | 900 | Yaşlılık etkisinin başlangıcı. |
| `maximumAgeSeconds` | 1200 | İlk modelde ortak azami yaş; yaş kaynaklı ölüm açık ve belirlenimlidir. |
| `founderAgeRangeSeconds` | 120–360 | Başlangıç bireyleri olgun ve farklı yaşlarda; başlangıç kohortu etkisi raporlanır. |
| `reproductionCooldownSeconds` | 60 | Doğum sonrası iki ebeveyn için bekleme süresi. |
| `mutationProbabilityPerBirth` | 0.001 | Özellik başına değil yavru başına. |
| `mutationSmallSigma`, `mutationLargeSigma` | 0.025, 0.10 | Normalize gen ölçeğinde. |
| `largeMutationConditionalProbability` | 0.05 | Mutasyon gerçekleşmişse büyük bileşenin olasılığı. |
| `actionSwitchMargin`, `minimumActionSeconds` | 0.08, 0.5 | Normal davranışın titremesini azaltır. |
| `lastSeenTargetSeconds` | 1.0 | Yalnızca eylem sürdürme için kısa son konum; uzun süreli öğrenme değildir. |
| `emergencyHorizonSeconds` | 2.0 | Tahmini ölüm zamanı bu pencereye girince acil katman. |
| `maximumRouteStartsPerTick` | 32 | Deterministik iş bütçesi; duvar saati süresine göre değişmez. |
| `snapshotIntervalSeconds` | 60 | Simülasyon zamanı; kuyruk dolarsa hesap güvenli sınırda yavaşlar. |
| `timeSeriesIntervalSeconds` | 1 | Temel istatistik örneklemesi. |

Metabolizma, bitki üretimi, yaralanma hasarı, saldırı darbe aralığı ve doğum enerji katkıları uygulanırken tek bir açık `ModelProfile` içinde tanımlanacak ve denge deneyleriyle belirlenecektir. Bu katsayılara burada uydurma kesin biyolojik değer atanmaz. **Kesin davranış**, her katsayının hangi denklemi ve hangi bütçeyi etkilediğidir; **kalibrasyon işi**, denklemin sayısal ölçeğidir. Kalibrasyon tamamlanmadan v0.1 sürüm kabulü verilmez.

Yaşlılık başlangıcından azami yaşa kadar hareket/algı potansiyeli ortak bir eğriyle hafif azalır. Bu çarpan genomu değiştirmez. İlk sürümün bütün bireylere ortak yaş profili model sadeleştirmesidir; ömür geninin evrimi ayrı sürüm kararıdır. Beyazlaşma `age/maximumAge` üzerinden hesaplanır; yaralanmış bireyin yakında öleceğinin görsel kehaneti değildir.

### Kurucu nüfus ve üreme grupları

Kurucu üreme etiketi ayrı bir kimliktir. İlk otçul ve etçil kümeler aynı etiketli uygun bireylerle eşleşir. Etiket bir genom skoru veya güncel yeme tercihi değildir. Bu tercih, ilk modelde aniden her şeyin aynı üreme havuzunda karışmasını önleyen sadeleştirmedir. Akraba eşleşmeleri kaydedilir; ilk sürümde gizli genetik ceza eklenmez. Cinsiyet rolleri, gebelik, eşeyli uyumsuzluk veya akraba eşleşmesi maliyeti daha sonra açık deney parametreleri olabilir.

Kurucuların doku enerjisi `B = B_adult` olarak kurulur; sağlık %100, enerji rezervi kendi kapasitesinin %70'i, su rezervi %80'i ve üreme bekleme süresi sıfırdır. Bunlar `founderHealthFraction`, `founderEnergyFraction`, `founderHydrationFraction` profil alanlarıdır. Başlangıçta dışarıdan eklenen beden ve rezerv toplamı dünya bütçesine başlangıç girdisi olarak kaydedilir. Yaşları ve dokuları yetişkinlik koşulunu birlikte sağlar.

Başlangıçta sıfır çeşitlilik verilmez. Beslenme değerleri iki uçta kümelenir; beden/algı/atılganlık/sosyal bağ daha geniş ama yapılandırmada görünen dağılımlardan örneklenir. Suya uyum başlangıçta düşük değerlerde yoğunlaşır; nadir geçiş potansiyeli var veya yok seçeneği deneyin ayrı başlangıç koşuludur. Her kurulmuş dünyada mutlaka başarılı yüzücü bulunacağı garanti edilmez.

Sosyal bağ geninin v0.1'deki etkinliği görünen uygun eşe yaklaşma/temas sürdürme faydasına bağlanır. İleri yavru koruma ve grup belleği henüz etkin değildir. Bir özellik ilk modelde hiçbir denklemi etkilemiyorsa “etkin gen” diye sunulmaz; özellik etkisi kayıtlı kontrol deneyinde gösterilmelidir.

### Hareketin çarpışma sözleşmesi

Statik engellerle kontrol yalnızca varış noktasında yapılmaz. Önceki ve önerilen konum arasındaki süpürülen beden diski değerlendirilir; yüksek hızda ince duvardan geçme engellenir. Geçiş kontrolünde beden yarıçapı kullanılır. A* geçiş verisi yarıçap sınıfına göre önbelleklenebilir, son hareket gerçek yarıçapla doğrulanır.

Dinamik canlıların yerel kaçınması sert çarpışma çözümünün önündeki yumuşak harekettir. Kalan örtüşmeler kararlı çift listesi ve sınırlı düzeltmeyle ele alınır; düzeltme statik engelin içine itemez. Görsel şeklin hafif taşması ile fiziksel beden diskinin örtüşmesi aynı şey sayılmaz. Yoğun kalabalıkta tam katı cisim davranışı hedeflenmez; kullanılan yumuşak beden/temas varsayımı belgelenir. Bir iterasyon sınırı varsa model profilinde sabittir.

Hareketin kapasitesi çevre veya yaralanmayla düştüğünde geçerli hız tanımlı frenleme sınırına uyar; yasak bölgeyi daha uzun süre geçerli kabul etmez. Nihai hız sınırlaması, ivme ve temas düzeltmesinin sırası test edilir. Bireyler geçilemez suya içmek için yürümez; kıyıdan içme eylemi ayrı erişim koşuluna sahiptir.

### Yol arama bütçesi ve bilgi sınırı

Rota işlerinin tick bütçesi yapılandırmadadır. İşlerin tamamlanması için gereken gerçek süre simülasyon hızını düşürebilir; farklı bilgisayarın aynı tick'te farklı rotaları hesaplamasına yol açmaz. Sıra bekleyen canlı güvenli durma/yerel kaçınma yapar. Acil rota talepleri önceliklidir; eşitler tick ile dönen kimlik sırasındadır. Bütçe uzun süre yetersizse uyarı ve ölçüm vardır.

A* sezgisinde en düşük geçiş maliyetiyle çarpılmış düz çizgi/sekizli ızgara uzaklığı kullanılır; ek tehlike maliyetleri negatif olamaz. Her rotada hedef, harita geçirgenlik sürümü, beden/su kapasitesi sınıfı ve oluşma tick'i saklanır. Yeniden rota; hedef kaybı, dünya değişimi, geçiş kapasitesi düşmesi veya uzun süre ilerleyememe nedeniyle başlar. Dinamik bir komşu geçti diye bütün yol her tick yeniden hesaplanmaz.

v0.1 rota servisinin statik araziye erişimi model varsayımıdır. Servis gizli hayvanları, gizli besin stoklarını veya gelecekteki afetleri hedef seçimine vermez. v0.5'te keşfedilmiş alan hafızası kullanıldığında bu **yeni model sürümü** olur; eski sonuçlarla navigasyon kapasitesi farkı belirtilir.

### Ölüm nedeni ve eşzamanlı olay kuralı

Hasarlar tick içinde neden ve kaynak kimliğiyle biriktirilir. Sağlık tükendiğinde `primaryCause`, o tick'teki en büyük pozitif sağlık kaybına göre seçilir; eşitlikte sabit kod sırası kullanılır. Bütün katkılar ayrıca saklanır. Azami yaşa erişme ayrı ölüm koşuludur; aynı tick'te daha önce ölümcül hasar oluştuysa hasar ana neden, azami yaş ek koşul olarak tutulur. Böylece işçi tamamlanma sırası “son darbe”yi değiştirmez.

Doğum işlemi ölüm işaretlerinden sonra kontrol edilir. Aynı tick'te hasar alıp yeterli sağlıkla kalan ebeveyn ancak son enerji/konum/uyumluluk koşulları hâlâ sağlanıyorsa ürer. Bütün doğum talepleri ortak konum doğrulamasından geçer; iki farklı çift aynı boş noktaya iki örtüşen yavru yerleştiremez. Yer yoksa ebeveyn enerjisi kesilmez.

### Deneyim ve öğrenmenin aşamalı sözleşmesi

v0.1'de davranışın birkaç tick sürmesini sağlayan hedef/son konum durumu vardır. v0.5'te buna sınırlı kapasitede ve zamanla unutulan bellek eklenir: görülen kaynak konumu, son görülme zamanı, tehlike izi ve tanışıklık kaydı. Bütün dünya geçmişi her canlıda tutulmaz. Bellek kapasitesi ve unutma kuralı model ayarıdır; performans daralınca sessizce azaltılmaz.

v0.7'de evrilen sinir ağı, sabit topoloji ve aynı gözlem/eylem sözleşmesiyle deney kolu olur. Beden genleri ile çok sayıdaki ağ ağırlığı için mutasyon bütçeleri açıkça ayrılır. Ağırlık sayısı artınca toplam yavru başına değişim olasılığı farkında olmadan artamaz. Aralıkla kalıtımın ağ davranışını korumadığı durumlar ayrı test edilir; “ebeveyn becerisi kesin geçer” varsayımı kurulmaz.

v0.8 öğrenme araştırması şu dört modu birbirinden ayırır: sabit kural denetleyici, nesiller arası evrilen denetleyici, dışarıda eğitilmiş ortak sabit politika, bireyin ömrü içinde değişen denetleyici. Aynı raporda karıştırılmaz. Dışarıda eğitilen politika, eğitim seed'lerinden ayrı değerlendirme dünyalarında sınanır. Canlının gizli genomu/geleceği veya diğer bireyin iç sağlık bilgisi gözlem olarak sızmaz.

Öğrenilmiş ağırlıkların veya yer belleğinin yavruya aktarımı varsayılan kapalıdır. Öğrenme sırasında ödenen hesap maliyeti gerçek biyolojik enerjiye otomatik eşitlenmez; bilişsel enerji maliyeti eklenecekse ayrı model kuralıdır. Sosyal taklit veya bilgi aktarımı, algılanabilen davranış ve iletişim eylemi üzerinden gerçekleşir. Yavru ebeveynin bütün rota veritabanını doğumda kopyalamaz.

<a id="section-19"></a>

## 19. Görsel kimlik ve asset üretim sistemi

### Sanat yönü

Seçilen referans Thrive'daki organik, hafif saydam ve yumuşak kenarlı canlı görünümüdür. EVOSIM'de ortam karasaldır: mat toprak, yosunlu alanlar, bitki kümeleri, kayalar ve okunaklı su yolları bulunur. Sürekli yüzen parçacıklar, bütün ekrana mavi sis veya deniz altı ışınları varsayılan değildir. Canlılar zemine temas hissiyle ilerler; gövdelerinin altında küçük ve sakin bir temas gölgesi olabilir.

Sanatın amacı, saatlerce bakılabilir bir dünya ve ilk bakışta okunabilir canlı durumlarıdır. Gerçekçi doku çözünürlüğü, yoğun çiçek parçacıkları ve abartılı parlama kalite ölçütü sayılmaz. Kalite ölçütleri siluet, renk uyumu, hareketin akıcılığı, kaynakların anlaşılması ve kalabalıkta okunurluktur.

### Başlangıç paleti

Bu renkler ilk görsel sahne için başlangıç değerleridir; yaş ve saldırganlık semantiği sabittir. Bütün renkler tek bir palette varlıkta tutulur, dosyalar arasında kopyalanmaz.

| Rol | Başlangıç rengi | Kullanım |
|---|---|---|
| Kuru toprak | `#3E4139` | Koyu ama tam siyah olmayan mat zemin. |
| Nemli zemin | `#303D36` | Suyla ilişkili bölgeler, yumuşak geçiş. |
| Ot ve düşük bitki | `#61785B` | Canlılardan daha düşük doygunluk. |
| Ağaç tepesi | `#536B56` | Az iç detay; büyük düz renk kütlesi. |
| Kaya | `#777B73` | Keskin dış sınır, sakin iç ton. |
| Sığ su | `#477E86` | Erişilebilir kıyı yakınları. |
| Derin su | `#284F61` | Geçiş zorluğunu destekleyen ton; gerçek mekanik derinlikten türetilir. |
| Sakin canlı | `#54A7D8` | Saldırganlık skalasının mavi ucu. |
| Orta eğilim | `#A17BC0` | Skalanın orta tonu. |
| Atılgan/agresif canlı | `#DE6668` | Kırmızı uç; alarm arayüzü rengiyle bağlam ayrılır. |
| Yaşlılık beyazı | `#E4E8DC` | Gövdenin beyazlaşması. |
| Seçim vurgusu | `#E7CD87` | Yalnızca seçili birey/nesnede ince halka. |
| Arayüz yüzeyi | `#202825` | Dünya üstünde sakin, okunaklı panel. |
| Arayüz yazısı | `#E2E8E0` | Kısa metinler, iyi kontrast. |

Mavi-mor-kırmızı geçiş algısal olarak düzgün bir renk uzayında hazırlanıp shader'a tablo/parametre olarak verilir. Yaş bütün gövdeyi aynı anda sıfır doygunluğa itmez: gövdenin içi açılır, kenarda eğilim renginin okunabilir bir kısmı kalır. Beyazlaşma kronolojik yaşın modeldeki yaşam eğrisi içindeki konumunu gösterir; kesin kalan ömür tahmini olarak sunulmaz.

### Bilginin görünüşe eşlenmesi

| Veri | Görsel kanal | Kural |
|---|---|---|
| Kalıtsal atılganlık | Ana renk | Anlık açlık veya tek saldırıda sürekli renk değiştirmez. |
| Kronolojik yaş | Gövde içinin açılması | Başlangıç eğrisi `whiteMix = 0.85 × smoothstep(0.35,1.0,ageNorm)`; kalibrasyon değeri. |
| Mevcut gelişmiş beden | Gövde alanı | Yavru/yetişkin gelişimi ve genomdan türeyen gerçek bedenle tutarlı. |
| Beslenme uyumu | Siluet/ön uç | Otçul yuvarlak, etçil hafif uzamış ön uç; ara değerlerde yumuşak geçiş. |
| Yön | Gövde ekseni ve ön uç | Hareket yönünden ayrı bakış yönü varsa veride açık tutulur. |
| Mevcut hareket | Deformasyon | Hız arttıkça hafif uzama; durunca birkaç karede normale dönüş. |
| Sağlık | Yakında küçük iz/ikon | Ana saldırganlık rengi yara rengine feda edilmez. |
| Seçim | Dış halka | Tür, yaş veya sağlıkla karıştırılmayan arayüz işareti. |
| Suya uyum | İsteğe bağlı küçük anatomik detay | Yalnızca ilgili sürümde; nadirliği veya yeteneği sahte biçimde belirginleştirmez. |

Siluet bir klinik veya biyolojik teşhis aracı değildir; uygulamanın tanımladığı özelliklerin görsel kodudur. Renk görme farklılıkları için eğilim skalası isteğe bağlı kenar çentikleri/işaretlerle de okunabilir. İnceleme panelinde sayısal değer her zaman bulunur. Isı haritalarındaki renklerin canlı rengiyle karışmaması için etkin katman adı ve lejandı görünürdür.

### Canlı gövdesinin üretimi

1. Kanonik yönü +Z olan birkaç basit kapalı 2D kontur hazırlanır.
2. Kontur, merkez ve dış halka gibi az sayıda köşeden oluşan düz bir meshe dönüştürülür. İlk LOD için yaklaşık 16–32 dış köşe yalnızca başlangıç sanat bütçesidir.
3. Siluet, genom ve olgunlaşma verisinden sınırlandırılmış biçimde türetilir. Kontur kendini kesemez; alan ile çekirdekteki beden yarıçapı uyumu doğrulanır.
4. Bir veya az sayıda paylaşılan malzeme kullanılır. Yaş, renk, boyut, yön ve hareket fazı birey verisidir; her canlı için ayrı material oluşmaz.
5. Hafif iç renk lekesi, kenar tonu ve hareket hissi shader ile üretilir. Bunların biyolojik bir karşılığı yoksa açıkça kozmetik kabul edilir.
6. Kozmetik faz `visualSeed + EntityId` üzerinden türetilir; doğum/mutasyon RNG akışına dokunmaz.
7. Shader bozulur veya desteklenmezse aynı silueti ve bilgi kodlarını koruyan düz renkli yedek malzeme vardır.

İlk canlı için yapay zekâ ile üretilmiş tek bir raster resme bağımlılık kurulmaz. Parametrik ve düzenlenebilir kaynaklar ana varlıktır. İleride kavram araştırması veya özel doku için görsel üretim kullanılabilir; nihai bilgi kodlarının ve geometrinin kontrolü korunur.

### Dünya asset envanteri ve sürüm ilerlemesi

| Asset ailesi | v0.0–v0.1 | Sonraki gelişim | Kaynak / sınır |
|---|---|---|---|
| Canlı ana gövdesi | 2 temel uç siluet ve aradaki değişimler | v0.5 sosyal etkileşim hareketleri, v0.9 küçük 3D beden | Kontur verisi + mesh üretici + shader. |
| Yaş/renk malzemesi | Mavi-mor-kırmızı, beyazlaşma, kenar | Bütün sürümlerde aynı anlam | Bir merkezî renk fonksiyonu. |
| Hareket görselleri | İlerleme, durma, dönüş, beslenme | Kazma/taşıma, sonra iskelet animasyonu | Simülasyon hızından ve eylem fazından beslenir. |
| Yavru | Aynı sistemin gelişim oranı düşük gövdesi | Ebeveyn yakınlığı ve grup etkileşimi | Ayrı rastgele yetişkin modeli değildir. |
| Ceset | Soluk ve hareketsiz gövde, kalan kaynakla küçülme | Çürüme evreleri | Kaynak bitince kaldırılır; popülasyonda sayılmaz. |
| Zemin | Nem/kuru toprak alanları, az doku | Mevsim, yanmış zemin, çamur, kar | Çevre verisinden renk; kozmetik desen ayrı. |
| Nehir ve gölet | Kıyı, sığ/derin ton, hafif yüzey hareketi | Sel sınırı, kuruma, buz, akış yönü | Su geometrisi ile görsel kıyı çakışır. |
| Ot ve yenebilir besin | 3–5 sade varyasyon | Meyve, tohum, farklı besin stokları | Stok ile doluluk görünümü bağlı. |
| Ağaç | 3 tepe, 2 gövde biçimi başlangıç seti | Yaprak dökümü, yanma, devrilme, 3D model | Engel gövdesi ile dekoratif taç ayrılır. |
| Çalı | 2–3 sade küme | Tohum, meyve ve gölge varyasyonları | Kaynak/engel rolü nesne verisinde. |
| Taş/kaya | 3–4 siluet ve boyut | Taşınabilir taş, geçit, heyelan | Taşınabilirlik görünümden rastgele belirlenmez. |
| Hazır korunak | Kaya altı veya basit oyuk işareti | v0.4 malzemeli yuva | Korunak alanı incelemede görünür. |
| Dal/yaprak/yapı malzemesi | v0.1 dekor gerekirse; taşınabilir değil | v0.4 stok ve fiziksel yük | Toplanabilir olanlar ayrı seçilebilir nesne. |
| Yuva | v0.4 taslak, yapım, tamamlanmış, hasarlı | Kapasite ve farklı malzemeler | İlerleme/hasar verisinden evre. |
| Yangın/duman | v0.3 sınırlı yerel efekt | Rüzgâr ve yanmış alan | Sürekli ekran kaplayan parçacık yok. |
| Yağmur/kar/rüzgâr | v0.3 isteğe bağlı görsel katman | Biyolojik etkiler dünya alanlarında | Görsel efekt kapatılsa da model aynı. |
| Koku/iletişim | v0.5 tanılama katmanı | Öğrenme araştırmaları | Varsayılan görünmez; anlamlı kısa işaretler. |
| Hastalık/parazit | İlgili araştırma sürümü | Yakın incelemede işaretler | Tanı paneli, ana renk koduyla çatışmaz. |
| Arayüz ikonları | Seçim, duraklat, adım, hız, kayıt, katman | Yapı/deney/öğrenme araçları | Aynı çizgi kalınlığı ve kaynak dosyası. |
| Grafik/ısı haritası | Nüfus, özellik dağılımı, yoğunluk | Karşılaştırma, soy ve etki haritaları | Gerçek ölçüm verisinden. |
| Ses | İsteğe bağlı az ortam sesi ve UI geri bildirimi | Yerel doğal olay işaretleri | Sessiz mod tam işlevlidir; her canlı ayrı ses üretmez. |

### Dosyalar, ölçek ve içeri aktarım

Önerilen düzen: `Art/Source/Vector`, `Art/Source/Contours`, `Art/Source/Blender`, `Art/Export/Sprites`, `Art/Materials`, `Art/Palettes`, `Art/Manifests`. Üretim araçları `Tools/Art` altında yer alır. Kaynaklar sürüm kontrolünde, tekrar üretilebilir çıktılar manifest ile ilişkilidir.

Başlangıçta düz görseller için 256 piksel/dünya birimi ortak ölçek seçilebilir; bu çözünürlük fiziksel beden boyutu değildir. Pivot yerdeki temas/merkez noktasıdır. Çok küçük canlılar için kaybolan detay eklenmez. Sayısal veri maskeleri ile renk dokularının renk uzayı ayarları ayrılır. Saydam kenarda koyu hale, atlas taşması ve filtreleme hatası farklı arka planlarda kontrol edilir. Atlas grupları yükleme ve malzeme kullanımına göre ayrılır; tüm büyük dünya tek dev dokuya sıkıştırılmaz.

Manifest her varlığın kimliğini, kaynağını, üretim aracını/sürümünü, rolünü, biyolojik alan bağlantısını, ölçeğini ve varsa dış kaynak lisansını tutar. İnternetten seçilen oyun görselleri referans klasöründeki bağlantılardır; yeniden kullanılabilir oyun asseti oldukları varsayılmaz. EVOSIM gövdeleri ve çevre şekilleri kendi kaynaklarından üretilir.

### Katmanlar, ağaçlar ve seçim

Çizim sırası kavramsal olarak zemin → su/kıyı → alçak bitki → gölge → canlı/nesne → yüksek taç → seçim/harita işaretleridir. Gerçek Y ofsetleri ve derinlik kuralları tek yerde tanımlanır; üst üste yüzey titreşimi oluşmaz.

Ağaç tepesi bir canlıyı gizlediğinde seçili canlı için gövdeyi koruyan ince kontur veya taç saydamlaşması kullanılabilir. Bu işlem canlıya ek görüş sağlamaz. Yakındaki canlıyı seçme, ekran pikselinin rengine göre değil çekirdek konumuna ve beden yarıçapına göre yapılır. Üst üste bireylerde kısa liste veya tekrar tıklamayla döngü bulunur. Büyük taç tıklaması alttaki canlının seçimini sürekli engellemez.

### Hareket estetiği ve ileride 3D

Görüntü önceki ve tamamlanmış yeni simülasyon konumu arasında yumuşatılır. Render katmanı bağımsız yol hesaplamaz. Dönüşte gövde yönü sınırlı hızla değişir; çekirdekteki gerçek yön ile aradaki fark küçük ve denetlenebilir kalır. Duraklatılmış dünyada biyolojik hareket durur; arayüz geçişleri devam edebilir.

v0.9'da Blender kaynaklı düşük ayrıntılı beden ve bitkiler eklenebilir. Kamera, renk semantiği, seçim ve simülasyon çarpışması korunur. İskeletli yürüyüş, yeni fiziksel yetenek eklemez. 3D mesh bacağı bir engelin üstünden geçiyormuş gibi çiziyorsa çekirdekteki geçiş kuralı değiştirilmeden animasyon düzeltilir. Eski 2D görünüm bir seçenek olarak kalabilir.

### Görsel kabul sahneleri

- Tek canlıda genç/yetişkin/yaşlı ve üç eğilim rengi birlikte okunur.
- Açık kıyı, koyu zemin ve bitki altında aynı canlı seçilebilir.
- 165 ve 500 canlı görüntüsünde etiket kalabalığı oluşmaz; varsayılan sürekli sağlık çubuğu yoktur.
- En yakın ve en uzak zoom düzeylerinde gövdeler, kaynaklar ve seçili birey anlaşılır.
- 30/60 FPS ve duraklatılmış görünümde renk/animasyon kuralları tutarlıdır.
- Saydam canlıların yığılmasında gövde sınırı kaybolmaz; GPU piksel maliyeti raporlanır.
- Renk körlüğü yardımcı işaretleri açıkken bütün kritik bilgiler okunabilir.
- Statik görüntüde güzel görünmek yanında 10 dakikalık seyirde dikkat dağıtan titreşim/parlama bulunmaz.

<a id="section-20"></a>

## 20. Kullanıcı deneyimi: Bir deneyin başından sonuna

### Açılış ve dünya oluşturma

Uygulama son deney, yeni deney ve kayıtlar olmak üzere kısa bir başlangıç ekranı açar. Kullanıcı ilk kez açtığında küçük nehir vadisi önayarını görür. Oluşturma ekranında seed, başlangıç nüfusu, kaynak bolluğu, üreme biçimi, mutasyon olasılığı ve etkin çevre olayları bulunur. Açıklamalar kısa ve somuttur. GPU backend, model sürümü ve ileri katsayılar araştırma/ileri bölümündedir.

Önizleme harita oluşturma seed'ini değiştirmez. Aynı önizlemeyi yeniden açmak yeni bir rastgele dünya üretmez. Kullanıcı “yeni seed” seçtiğinde sayı görünür değişir. Başlangıç geçerlilik hataları harita üzerinde işaretlenir. Dünyanın tamamını dolduran taş gibi sorunlar düzeltilmeden koşu başlatılmaz.

### Gözlem ekranı

Ana alan dünyadır. Üstte deney adı, simülasyon zamanı ve nüfus; altta duraklat/adım/hız; sağda isteğe bağlı inceleme paneli bulunur. Küçük bir grafik çekmecesi açılıp kapanabilir. Ana akışta geliştirici logları, shader terimleri veya model eğitim ayarları görünmez.

Kamera fare sürükleme ile taşınır, tekerlekle odak noktasına doğru yakınlaşır. Seçili bireyi takip düğmesi vardır; birey öldüğünde kamera başka bireye sessizce geçmez. `Space` duraklat/devam, tek adım için ayrı tuş ve düğme, `Esc` seçimi/paneli kapatma gibi kısayollar yardımda gösterilir. Kısayollar metin girerken tetiklenmez.

Hız seçenekleri başlangıçta 1×, 2×, 4×, 8× hedefleridir. Gerçek hız ayrıca gösterilir. Tek adım tam bir simülasyon tick'idir; “bir gün” düğmesi eklenirse kaç tick olduğu açıktır. Simülasyon saniyesi ile görsel gün/gece çevrimi ayrı tanımlanır; bir dünya günü 24 gerçek saate eşit varsayılmaz.

### Birey inceleme

Panelde kalıcı kısa kimlik, yaş/gelişim, mevcut eylem, enerji/hidrasyon/sağlık, genom ve mevcut kapasite ayrı gruplar halinde bulunur. “Neden?” alanı son kararın gerçek girdilerini ve en güçlü alternatifini gösterir: örneğin `su arıyor: hidrasyon düşük, kıyıya erişim var; besin arama ikinci sırada`.

Ebeveyn bağlantıları, doğan yavrular, yaşayan yavrular, benzersiz torunlar ve bütün yaşayan soy devamı ayrı sayılardır. Grafikte akraba dallar birleşebilir; çizim bunu ağaç gibi yanlış çoğaltmaz. Büyük soy grafiği kademeli açılır; bütün tarih her kare yeniden hesaplanmaz.

Algı konisi, hedef, rota ve tehlike işaretleri yalnızca seçim/tanı görünümünde açılır. Bunlar canlının gerçek algısını gösterir; en yakındaki gizli av analiz sistemince biliniyor diye panelde görülmüş hedef sayılmaz. Kullanıcı bireye isim takabilir; isim vermek fiziksel veya kalıtsal avantaj sağlamaz.

### Harita katmanları ve grafikler

Bir seferde tek ana ısı katmanı: nüfus yoğunluğu, nem, besin, ölüm yoğunluğu, ortalama yaş, kalıtsal özellik dağılımı veya soy yayılımı. Seçim değiştiğinde lejand ve birim değişir. Ortalama değer düşük örnek sayısıyla hesaplanmışsa sayı/örnek azlığı görünürdür. Boş hücre sıfır değerli dolu hücre gibi boyanmaz.

Grafikler nüfus, doğum/ölüm nedenleri, kaynak stoku ve özellik dağılımlarını içerir. Yaş ortalaması değiştiğinde bunun genetik değişim olduğu iddia edilmez. Zaman serileri örnekleme aralığıyla etiketlenir. Çok eski veri çizimde özetlenebilir; ham kaydın tutulma ayarı ve disk maliyeti açıktır.

### Olay günlüğü ve sessiz gözlem

Günlük gerçek olaylardan oluşur: doğum, ölüm, mutasyon, ilk kayıtlı geçiş, afet başlangıcı/sonu, soyun son üyesinin ölümü, yeni yapı. “Kahramanlık” veya “ihanet” gibi yorumlar temel olay kaydı değildir. İlginç olay özeti kullanılırsa hangi ölçütle seçildiği görülebilir.

Bildirimler her doğumda açılmaz. Kullanıcı seçili canlı, büyük nüfus değişimi, afet ve deney bitişi için tercih yapabilir. Aynı afete ait yüz ölüm tek özet olarak gösterilebilir; ayrıntı kaybolmaz. Ses ve bildirim kapalıyken bütün işlevler kullanılabilir.

### Kaydetme, devam ve yok oluş

Kaydetme tamamlanan bir tick sınırında tutarlı snapshot oluşturur. Arayüz kaydın hazırlanmasını ve tamamlanmasını ayırır. Başarısız kayıt başarı mesajı üretmez. Son sağlam kayıt listede kalır. Devam ederken model/backend farkı açık gösterilir; eski kayıt sessizce yeni kurallarla sürdürülmez.

Nüfus sıfır olunca deney sonucu ekranı “yok oluş” olarak açılabilir; dünya ekolojisini sürdürme veya koşuyu bitirme kullanıcı ayarıdır. Otomatik yeni canlı yoktur. Kullanıcı yeniden nüfus eklerse yeni müdahale olayı ve tercihen ayrı dal oluşur.

Kaynak/RAM sınırı aşılınca erken durdurma nedeni `capacity_limit` olarak kaydedilir. Bu koşu doğal yok oluş istatistiğine katılmaz. Canlıları görünmez biçimde silerek performans korunmaz.

### Deney sonucu ve karşılaştırma

Sonuçta süre, seed, model/backend, başlangıç ve son nüfus, doğum/ölüm nedenleri, yaşayan soylar, kaynak değişimi, özellik dağılımları, dış müdahaleler ve performans özeti bulunur. Kullanıcı aynı başlangıçtan bir kuralı değiştirip karşı koşu açabilir. Yeni koşu, eski koşunun kaydı üzerine yazılmaz.

Deneyleri yan yana karşılaştırırken aynı simülasyon zamanları hizalanır. Afetin başlangıcı gibi olay temelli hizalama ayrı seçenektir. Birden fazla seed'de ortanca ve dağılım gösterilir; tek “kazanan dünya” puanı bütün değerlendirmeyi kaplamaz. Kullanıcı tablo verisini dışa aktarabilir; ölçü adları, birimler ve örnekleme aralıkları dosyada kalır.

### Kullanıcı deneyimi kabul ölçütleri

İlk açılıştan önayar deney başlatmaya kadar temel akış anlaşılır olmalı; kullanıcı genleri ve mevcut sağlık/enerjiyi karıştırmamalı; hız düğmesi gerçek durumu göstermeli; kayıt sonrası aynı birey bulunabilmeli; bir ölümün kayıtlı nedeni ve zamanına erişilebilmeli; yok oluşta sonuçlar korunmalı; seçili canlıyı kamera dışında kaybetme geri alınabilmeli. Bu kontroller ilk iki sürümde yapılan gerçek görevlerle doğrulanır.

<a id="section-21"></a>

## 21. Teknik mimari, GPU kullanımı ve deney güvenilirliği

### Kesin teknoloji kararları

EVOSIM, Unity 6.3 LTS + C# üzerinde geliştirilecek. İlk kurulumda doğrulanan editör yaması ve paket sürümleri kilitlenecek; deney ortasında otomatik yükseltme yapılmayacak. Unity 6.3, resmî uzun süreli destek sürümleri arasındadır. [Unity sürüm desteği](https://unity.com/releases/unity-6/support)

Görsel temel, **URP’nin 3D renderer’ı içinde düz XZ dünya, ortografik kamera ve düzlem üzerine çizilen 2D gövdeler** olacak. Kamera ilk aşamada zemine 90° bakacak; isteğe bağlı 3D aşamasında yaklaşık 85° olacaktır. Böylece render altyapısını değiştirmeden düz gövdelerin yerine küçük meshler geçebilir. Simülasyonun iki koordinatı render katmanında X/Z’ye dönüştürülür; yükseklik ilk sürümde görsel ayrıştırma içindir.

Referans simülasyon, UnityEngine bağımlılığı olmayan C# kütüphanesi olacak. Unity ile uyumlu .NET Standard 2.1 hedefi başlangıç adayıdır; kullanılan C# dil özellikleri Unity derleme denemesiyle doğrulanır. Komut satırı çalıştırıcısı aynı kütüphaneyi çağırır. Python yalnızca analiz ve gelecekte öğrenme deneylerinde devreye girer. Görsel shader kodu HLSL/Shader Graph, düzenlenebilir vektör kaynakları SVG, gelecekteki 3D kaynakları Blender biçiminde tutulur. Blender modelleme ve Python ile araç üretimini destekler. [Blender modelleme araçları](https://www.blender.org/features/modeling/)

15 Eylül 2026 tarihinde bu bilgisayarda araçla okunan donanım: Intel Core Ultra 9 275HX, 24 mantıksal işlemci; yaklaşık 64 GB RAM; RTX 5090 Laptop GPU, yaklaşık 24 GB VRAM; Ubuntu 24.04.4 LTS ve NVIDIA 595.84 sürücüsü. Bunlar EVOSIM performans sonucu değildir. İlk geliştirme oturumunda envanter yeniden kaydedilir; laptop GPU, masaüstü 5090 performansıyla özdeş varsayılmaz.

### Modüller ve veri sahipliği

| Modül | Sorumluluk ve sınır |
|---|---|
| `Evosim.Core` | Dünya durumu, özellikler, yaşam döngüsü, hareket, kaynaklar, karar ve etkileşim kuralları; otorite burada |
| `Evosim.Contracts` | Yapılandırma, kimlik, komut, olay, kayıt şemaları; motor nesnesi içermez |
| `Evosim.Runner` | Penceresiz deney, seed listeleri, koşu bütçesi, checkpoint, ölçüm |
| `Evosim.Unity` | Kamera, çizim, seçim, ses, arayüz; çekirdeğe zaman damgalı komut gönderir |
| `Evosim.Backends.Burst` | Aynı hesap sözleşmesinin isteğe bağlı Jobs/Burst uygulaması |
| `Evosim.Backends.Compute` | Karşılaştırma kapısını geçen GPU alan hesapları; ayrı backend etiketi |
| `Evosim.Analysis` | Olay ve zaman serisinden karşılaştırmalar; simülasyon kararlarına müdahale etmez |
| `Evosim.Learning` | Sonraki sürümlerde gözlem/eylem protokolü, politikalar, eğitim ve değerlendirme |

Canlı başına `GameObject + Update + Rigidbody` modeli kullanılmayacak. Çekirdek, alanları ayrı dizilerde tutan **SoA** düzeni kullanır: konumlar, hızlar, enerji, sağlık, genler, eylemler ayrı, ardışık bellek bloklarındadır. Bir sistem yalnızca ihtiyaç duyduğu alanları tarar. Unity görünür canlıları bu verilerden topluca çizer.

Kalıcı `EntityId`, dizideki slottan ayrıdır. Slot geri kullanılabilir; kimlik kullanılamaz. Soy kaydı kimliğe bağlı kalır. Doğum/ölüm, adım sonundaki komut tamponundan uygulanır; taranan dizinin ortasında ekleme/silme yapılmaz. Önceden ayrılmış tamponlar, nesne havuzları ve yeniden kullanılan arama listeleri kullanılır. Her kare LINQ, string oluşturma veya tüm nüfusu kopyalama alışkanlığı kurulmaz.

Çekirdeğin tek yazarı vardır. Görüntüleme önceki ve son tamamlanmış görüntü durumunu çift tamponla okur. Dosya yazma ayrı iş parçacığında, değişmez bir checkpoint kopyası üzerinden gerçekleşir. Unity nesnelerine ana iş parçacığı dışında erişilmez. Saf çekirdek ile NativeArray tabanlı backend aynı veri sözleşmesini paylaşır; bütün dizileri her adım iki tarafa kopyalayan bir adaptör tasarlanmaz.

### Simülasyon saati ve adım sırası

Ana saat `tick` tam sayısıdır; `dt = 0,05` simülasyon saniyesi, yani 20 Hz. Hareket, metabolizma, temas ve tehlike kesmeleri her tick çalışır. Normal karar yenileme 5 Hz, bitki/nem/iklim hesabı 1 Hz’dir. Karar fazları canlı kimliğine göre dört tick’e dağıtılır; bir canlı normal kararını her dört tick’te bir yeniler. Acil tehlike kontrolü bu takvimi beklemez. UI grafik güncellemesi 2–5 Hz, çizim ekran yenilemesindedir.

Bir tick’in hesap sözleşmesi:

1. Bu tick’e ait kullanıcı komutlarını ve takvimli dış olayları uygula; gerekiyorsa ekolojiyi ilerlet.
2. Başlangıç durumunu sabitle; konumsal indeksi oluştur/güncelle.
3. Taban/algı borcunu (henüz uygulamadan), kapasite, algı ve acil durumları hesapla; sıradaki canlılarda kararı yenile.
4. Hareket ve etkileşim niyetlerini üret; henüz başka canlının durumunu değiştirme.
5. Önerilen hareketi engeller ve yerel çakışmalarla çöz; gerçekleşen kendi ilerlemesinden hareket giderini ekle ve etkileşim mesafesini son konumdan doğrula.
6. Paylaşılan besin/etkileşim taleplerini çöz; tüketimi rezervlere aktar, eşzamanlı hasarı ve zorunlu giderlerin karşılanamayan kısmını uygula. Yaşı ilerlet, sağlık/azami yaş ölümünü kilitle; yalnız sağ kalanlara yeterli rezervle büyüme/iyileşme uygula.
7. Hâlâ geçerli eşleşmelerden doğumları atomik uygula; doğum/ölüm ve yapı değişimlerini commit et. Aynı kaynak iki kez tüketilemez; ölen ebeveyn sonradan üreyemez.
8. Olayları, gerekli ölçümleri ve durum hash’ini yayımla; sonraki tick’e geç.

Aynı besine uzanan iki canlının başarısı iş parçacığının önce bitmesine bağlı olamaz. Menzil ve uygunluk doğrulandıktan sonra bölünebilir kaynak talepleri oransal paylaştırılır; sayısal kalan tick ve kaynak kimliğinden türetilen döner sırayla dağıtılır. Eşleşme gibi bölünemeyen çatışmalı işlemlerde uygun çiftler için tick ve katılımcı kimliklerinden türetilen kararlı kura kullanılır. Sadece en küçük kimliğin sürekli kazanması önlenir. Bu çözümün sırası model sürümünün parçasıdır.

Hız düğmesi `dt` değerini büyütmez; duvar saatinde daha fazla tick yürütür. 4× istenip 2,7× üretilebiliyorsa arayüz gerçek 2,7× hızını gösterir. Tick atlamak, kamera dışında canlıları daha az hesaplamak veya nüfus artınca algıyı sessizce kısmak yasaktır. Görsel modda bir kareyi sonsuz yakalama döngüsüne sokmamak için hesap dilimi sınırlanır; açıkça daha yavaş simülasyon zamanı ilerler. Penceresiz modda kare hedefi yoktur.

### CPU kullanımı ve mekânsal arama

128 × 128 başlangıç alanı çevre hücreleri taşır; canlılar hücre merkezlerine bağlı olmadan sürekli koordinatlarda hareket eder. Yakın komşu arama uniform spatial grid ile yapılır. Görüş yarıçapının kapsadığı hücreler taranır, gerçek mesafe/açı/görüş engeli daha sonra sınanır. Başlangıçta bütün canlı çiftlerini tarayan O(N²) yöntem kurulmaz. Tek hücrede yığılma ayrı stres senaryosudur; indeksin ortalama hızlı olması en kötü durumu çözmüş sayılmaz.

CPU referans backend önce okunabilir seri kodla kurulur. Profilde anlamlı payı olan bağımsız algı, maliyet ve hareket niyeti döngüleri daha sonra Jobs/Burst’a alınabilir. Jobs bağımlılıkları okuma/yazma sınırlarını açıkça belirler; paylaşılan sonuçlar sabit sıralı birleştirme aşamasına gider. Unity Job System bağımsız işlerin birden fazla CPU çekirdeğinde yürütülmesini sağlar. [Unity Job System](https://docs.unity3d.com/6000.3/Documentation/Manual/job-system-overview.html)

24 mantıksal işlemcinin tamamı her zaman simülasyona ayrılmaz. Editör/arayüz ve işletim sistemi için pay bırakılır; 1/4/8/12/16 işçi ile toplam deney verimi ölçülür. Birden fazla koşu açıldığında her koşuya çok sayıda işçi vererek CPU’yu aşırı yüklemekten kaçınılır. Küçük dünyalarda paralel iş zamanlama maliyeti kazanımdan büyükse seri yol korunur.

### GPU planı: Önce çizim, sonra ölçülmüş hesap

Amaç GPU kullanım göstergesini yüzde 100 yapmak değildir. Amaç, **aynı model doğruluğuyla daha fazla deney ve iyi görüntü** elde etmektir. İlk görsel denemeden itibaren GPU çizim, organik gövde shader’ı, su yüzeyi ve görsel katmanları çalıştırır; v0.1’de yaşamsal kararların otoritesi CPU’dadır. Bu ölçekte cihazlar arası aktarım, küçük bir hesabın kendisinden pahalı olabilir. Hızın yanında deney başına enerji ve toplam karar/saniye/watt ölçülür; sıcaklık nedeniyle uzun koşuda yavaşlayan bir ayar kısa benchmark’ı kazansa da tercih edilmez.

Canlı rengi, yaş, büyüklük, yön ve hareket fazı instance verisi olarak yüklenir; canlı başına ayrı material oluşturulmaz. Statik dünya parçalara ayrılır, yalnızca değişen parçalar güncellenir. Çok sayıda örnek çizmek gerektiğinde `Graphics.RenderMeshInstanced` veya GPU tarafından hazırlanan komutlarla `Graphics.RenderMeshIndirect` ölçülür. Yeni kod eski `DrawMeshInstancedIndirect` üzerine kurulmaz. API seçimi, seçilen Unity yaması ve Linux grafik arayüzündeki gerçek çizim ölçümüne bağlıdır. [Unity RenderMeshIndirect](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.RenderMeshIndirect.html), [eski API durumu](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html)

Görsel LOD yalnızca çizim ayrıntısını azaltır: uzaktaki canlıda iç organ benzeri şekiller, deformasyon ve ince kenar efekti sadeleşir. Saydam gövdelerde üst üste piksel çizimi ayrıca ölçülür. Hafif saydam görünüm mümkün olduğunca tek gövde katmanı ve sınırlı kenar etkisiyle sağlanır; çok katlı parlama ve tam ekran blur varsayılan yapılmaz. Kamera dışındaki canlıların biyolojisi tam hızda sürer.

v0.6 GPU hesap adayları, büyük ızgaralarda nem/ısı/koku yayılımı, toplu alan analizleri ve sonraki öğrenme sürümlerinde sinir ağı batch’leridir. Kalıtım kayıtları, düzensiz soy ağacı sorguları ve küçük kaynak çekişmeleri başlangıçta CPU’da kalır. GPU’da da spatial grid/kompakt hücre listesi gerekir; O(N²) sorunu daha çok çekirdekle örtülmez.

Seçilen GPU alanı cihaz belleğinde kalır, iki tamponla ilerler. Her canlı için küçük ayrı kopyalar veya senkron `GetData` döngüleri kullanılmaz. Sadece toplu istekler, gerekli hücre dilimleri ve özetler taşınır. Okumalar sınırlandırılmış kuyrukta, tick/epoch etiketiyle yapılır. Asenkron okuma birkaç kare gecikme ekleyebilir; bu yüzden geç gelen veri hangi simülasyon adımına ait olduğunu taşır. [Unity AsyncGPUReadback](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Rendering.AsyncGPUReadback.html)

**Gecikme fiziği değiştiremez:** CPU bir sonraki ekoloji epoch’u için GPU sonucu istiyorsa, tanımlanmış sınırda sonucu bekler; grafik son tamamlanmış durumu çizmeye devam edebilir. Eski veriyi sessizce kullanmak veya zamanında gelmedi diye bir afet adımını atlamak yasaktır. Grafik tanılama verileri gecikebilir ve yaşı gösterilir. GPU hatasında koşu checkpoint’te durdurulur; backend değişimi yeni dal olarak kaydedilir.

GPU’ya geçiş kapısı: Temsilî büyük iş yükünde, aktarım ve senkronizasyon dâhil en az 1,5× uçtan uca kazanım; hata toleransları ve model değişmezleri geçerli; 30 dakikalık sıcak çalışmada kararlı davranış. Bu eşik proje kararıdır, ölçülmüş kazanım değildir. Karşılamayan görev CPU’da kalır.

### Yeniden üretilebilirlik ve kayıt

Üç çalışma etiketi bulunur:

- **Reference-CPU:** Sabit build, runtime, platform ve yapılandırmada aynı seed/komutlarla aynı olaylar ve durum hash’leri hedeflenir.
- **Validated-CPU:** Jobs/Burst kullanır; referansla test sözleşmesi sağlanır. Sayısal fark varsa eşdeğerlik sınırı açıkça kaydedilir.
- **Accelerated-GPU:** GPU modeli ve tolerans sürümü raporlanır; CPU veya farklı GPU’larla bit düzeyinde aynılık vaat edilmez.

Burst’ün kayan nokta seçenekleri hesap sırasını ve sonuçlarını etkileyebilir. Belgelenen deterministik mod da bütün programın, veri girişlerinin ve paralel arbitrajının kendiliğinden deterministik olması anlamına gelmez. Kilitlenen sürümde desteklenen seçenekler doğrulanacak; ilk eşdeğerlik geçişinde hızlı matematik optimizasyonları kapalı tutulacaktır. [Burst sayısal modları](https://docs.unity3d.com/Packages/com.unity.burst@1.8/manual/compilation-burstcompile.html)

Rastgelelik algoritması tanımlı ve testli olacaktır; framework’ün sürümle değişebilen varsayılanına güvenilmez. Harita, dış afetler, üreme, mutasyon ve davranış için ayrı akışlar/sayaçlar kullanılır. Paralel döngüde paylaşılan rastgele sayı nesnesi yoktur. Harita ve afetlerin rastgele akışı nüfusun kaç doğum yaptığına bağlı kaymamalıdır; eşleştirilmiş deneylerde aynı dış koşullar korunabilir.

Checkpoint şunları içerir: schema/model/build sürümü; ayar ve asset manifest hash’i; backend/numeric mode; tick; tüm canlı durumları ve genleri; kaynaklar/alanlar/yapılar; RNG durumu veya sayaçları; bekleyen olaylar/niyetler; karar fazları; kalıcı kimlik sayacı; soy kaydı ve öğrenme belleği. Dosya önce geçici adla yazılır, checksum doğrulanıp atomik ad değişimiyle tamamlanır. Eski dosya üzerine başarısız yarım kayıt bırakılmaz. Şema geçişi kopya üzerinde yapılır; özgün dosya korunur.

Her adımın devasa tam dökümü tutulmaz. 1 Hz zaman serisi, doğum/ölüm/mutasyon/afet gibi ayrık olaylar ve varsayılan 60 simülasyon saniyesinde checkpoint kullanılır. Ayrıntılı hareket geçmişi seçili canlı veya süreli tanılamayla sınırlanır. Gerçek geriye sarma, checkpoint + komut/olay günlüğünden tekrar çalıştırmadır. Bit eşdeğerliği olmayan GPU koşusunun görsel geçmişi örneklenmiş kayıt olarak etiketlenir.

### Eğitim ve deneyim mimarisi

v0.1–v0.6 için ön eğitim gerekmiyor. Doğal seçilim yaşam döngüsü içinde oluşur. v0.5’te ziyaret edilen kaynak, tanıdık birey ve tehlikeli yer belleği eklenebilir; bu tek başına sinir ağı eğitimi değildir.

v0.7’de nöroevrim araştırma modudur: Küçük ağın kalıtsal değerleri açıkça tanımlanır; topoloji başlangıçta sabittir. Vücut genleri, beynin kalıtsal parametreleri ve geçici bellek ayrı saklanır. İnsan tarafından tanımlanan tek bir başarı puanı varsayılan biyolojik seçilimin yerine geçmez; gerçek enerji, ölüm ve üreme sonuçları kullanılmaya devam eder.

v0.8’de yaşam boyunca öğrenme ayrı deney koludur. PPO ile birçok canlıdan veri toplayıp ortak bir politika eğitmek, her bireyin kendi ömründe öğrenmesiyle aynı şey değildir. Ortak politika öğrenimi kullanılırsa politika sürümü bütün koşuya yazılır; bireysel deneyim iddiası yapılmaz. Bireysel öğrenmede değişen ağırlık/belleğin yavruya geçişi varsayılan kapalıdır. Genetik aktarım, öğrenilmiş ağırlık ve deneyim aktarımı ayrı müdahale anahtarlarıdır.

ML-Agents + PyTorch bağlantısı adaydır, hazır uyumluluk garantisi değildir. Resmî ML-Agents kurulum sayfası Unity/Python paketlerinin eşleşmesini ister; bazı örnek CUDA/PyTorch sürümleri Blackwell için eski kalabilir. PyTorch 2.7 Blackwell/CUDA 12.8 desteğini duyurmuştur; sonraki sürümlerin CUDA dağıtımı değişebilmektedir. Kurulum gününde desteklenen sürüm kesişimi bulunup kilit dosyası üretilir. [ML-Agents kurulumu](https://github.com/Unity-Technologies/ml-agents/blob/develop/com.unity.ml-agents/Documentation~/Installation.md), [PyTorch Blackwell desteği](https://pytorch.org/blog/pytorch-2-7/), [sonraki CUDA dağıtım değişimi](https://pytorch.org/blog/pytorch-2-12-release-blog/)

Uyumluluk kapısı: CUDA tensör işlemi, kısa örnek eğitim, Unity haberleşmesi, politika dışa aktarımı, aynı modelin Unity’de çalıştırılması, kaydet/devam et ve 30 dakikalık kararlılık denemesi. Başarısızsa bağımlılıkları zorla yükseltmek yerine ayrı adapter/ortam incelenir. GPU’da toplu gözlem/eylem aktarımı kullanılır; görsel kamera gözlemleri ilk eğitim girdisi olmaz. Canlının erişemediği dünya bilgisi politika girdisine sızdırılmaz.

Eğitim saatleri önceden vaat edilmez. Ölçümler: ajan karar/s, simülasyon saniyesi/duvar saniyesi, update/s, aktarım payı, örnek başına bellek ve ayrı seed’lerde davranış başarısı. GPU’da büyük batch hızlı olabilir ama politika güncelliğini veya bireysel öğrenme anlamını değiştirebilir; batch boyutu da deney ayarıdır.

### Performans kapıları ve doğrulama

Aşağıdakiler **henüz ölçülmemiş mühendislik hedefleridir**; hedef tutmuyorsa sürüm raporunda gerçek değer yazılır.

| Aşama | Temsilî yük | İlk hedef / karar |
|---|---|---|
| v0.1 | 128² alan, 165 başlangıç canlısı; 500 canlı stres koşusu | 1080p’de 60 FPS hedefi; 1× için 50 ms tick sınırı, p95 hesap ≤5 ms hedefi |
| v0.2 | Aynı model, 1.000 canlı; grafik kapalı tekrarlar | Aynı sonuçlarla en az 5× koşu hedefi; RAM/olay günlüğü sınırlı büyüme |
| v0.3–v0.5 | 256² alan, 2.000 canlı, afet/yapı/bellek etkin | 1× korunması, p95 tick ≤25 ms hedefi; yoğun yığılma ayrıca raporlanır |
| v0.6 | 512²/1024² alan, 5.000–20.000 canlı araştırma yükleri | Kapasite sözü verilmez; GPU geçişi 1,5× uçtan uca kazanç kapısıyla seçilir |
| v0.7–v0.8 | Küçük doğrulanmış dünyaların paralel kopyaları | Sabit sayıda kopya hedeflenmez; toplam karar/s ve davranış kalitesi optimize edilir |
| v0.9–v1.0 | Aynı kayıt 2D/3D ve farklı grafik düzeyleri | Biyolojik sonuçlar değişmez; görünümün CPU/GPU maliyeti ayrıştırılır |

Profil örnekleri: bol kaynak, kıtlıkta yığılma, nehir geçidinde kalabalık, eşzamanlı doğum dalgası, yangın/sel, tüm harita yakınlaştırması, uzun soy geçmişi. Rapor; p50/p95/p99 tick ve frame süresi, allocation/GC, RAM/VRAM, transfer, GPU compute/render zamanı, sıcaklık/güç ve gerçek hız katsayısını içerir. Başlangıç ısınması ile 30 dakikalık sürdürülen laptop performansı ayrı gösterilir. Fan sesi ve enerji tüketimi için 30 FPS gözlem/60 FPS etkileşim tercihi sunulabilir.

Zorunlu testler: kayıt sonrası devamın eşleşmesi; farklı render FPS ve kamerada aynı çekirdek sonucu; permüte edilmiş işçi bitiş sıralarında aynı arbitraj; negatif kaynak/enerji, NaN ve çift tüketim engeli; kalıtım dağılımı ve mutasyon sıklığı; doğum/ölüm kimlik güvenliği; CPU backend eşdeğerliği; GPU alanlarında tolerans ve korunum; aynı dış koşullarla çoklu seed deneyleri. Performans değişikliği biyolojik modeli değiştiriyorsa yeni model sürümüdür, görünmez optimizasyon değildir.

Ana teknik hat küçük doğrulanmış dünya → deney/kayıt → seçilmiş ekoloji/inşa/sosyal mekanikler → kararlı laboratuvardır. v0.6 GPU, v0.7 nöroevrim, v0.8 bireysel öğrenme ve v0.9 3D ayrı koşullu kollardır; v1.0 bu kolların tamamlanmasını beklemek zorunda değildir. Her etkinleştirilen aşama kendi bağımlılıklarını, tekrar çalıştırılabilirliğini ve açıklanabilirliğini korur.

<a id="section-22"></a>

## 22. Sürüm yol haritası ve tamamlanma ölçütleri

Sürümler takvim vaadi değildir. Ana hat sürümlerinde bir sonraki adımın giriş koşulu gereken önceki modelin kabul kontrollerini geçmesidir. GPU, öğrenme ve 3D ayrı koşullu kollardır; bütün kollar kararlı v1.0'ın önkoşulu değildir. GPU optimizasyonu ölçüm gerektiğinde erken yapılabilir; yeni ekolojik mekanikler yalnızca performans amacıyla değiştirilmez.

<a id="section-22-version-01"></a>

### v0.0 — Proje iskeleti ve görsel dil denemesi

**Amaç:** Kod, veri, çizim ve deney saatinin birbirinden ayrıldığını; seçilen estetiğin tepeden okunabildiğini göstermek.

- Depo, dizin düzeni, Unity yama/paket kilitleri ve C# referans kütüphanesi kurulur.
- Sabit tick saati, seed/config yükleme, küçük koşu manifesti ve komut satırı iskeleti hazırlanır.
- Tek örnek sahnede toprak, su, ağaç, kaya ve parametrik canlı gövdesi bulunur.
- Genç/yetişkin/yaşlı, sakin/orta/agresif ve küçük/büyük görsel örnekleri hazırlanır.
- Kamera, seçim, duraklatma ve GPU çizim ölçümü çalışır.
- Biyolojik deney sonucu üretilmiş gibi sunulmaz; bu sürüm sanat/altyapı denemesidir.

**Çıkış:** Aynı canlı verisi renk/yaş/ölçek kurallarını doğru gösterir; kamera neredeyse diktir; çekirdek Unity dışında da küçük bir tick denemesini çalıştırır; baseline profiler raporu vardır.

<a id="section-22-version-02"></a>

### v0.1 — İlk yaşayan nehir vadisi

**Amaç:** Küçük, görsel olarak keyifli ve nedensel olarak açıklanabilir uçtan uca dünya.

- 128² harita, 150 otçul/15 etçil başlangıcı; su/bitki/kaya/hazır korunak.
- Altı genom değeri, yaş/gelişim, enerji/doku, hidrasyon, sağlık.
- Algı, A*, yerel hareket, kıyıdan içme, beslenme, avlanma/savunma, dinlenme, eşleşme.
- Atomik eşeyli doğum, düşük mutasyon, ölüm/ceset, soy kimlikleri.
- Tek kuraklık olayı; normal ve kuraklık koşusunun karşılaştırılması.
- Temel soy paneli, nüfus grafiği, gerçek eylem açıklaması, kaydet/devam.
- İnşaat ve öğrenme bu sürümün davranış modeli değildir; hazır korunaklar dinlenmeye etki eder.

**Çıkış:** Hareket, enerji, kaynak, kalıtım, soy ve kayıt kapıları geçer. Normal profil ve kuraklık profili aynı başlangıç seed'leriyle çalıştırılır. Gözlenen yok oluşlar saklanır; sürümün başarılı sayılması bütün dünyaların hayatta kalmasına bağlı değildir. 1080p/165 canlı gözlem hedefi ölçülür; gerçek değer raporlanır. En az bir tam azami yaşam süresini kapsayan referans koşu tamamlanır veya doğal yok oluşla açıklanır.

<a id="section-22-version-03"></a>

### v0.2 — Güvenilir deney laboratuvarı

**Amaç:** Tek ilginç koşudan tekrarlı ve karşılaştırılabilir deneylere geçmek.

- Penceresiz runner, seed listeleri, süre/kapasite sınırları, dışa aktarma.
- Koşu dalları, kayıt şeması, snapshot+komut replay, hata kurtarma.
- Benzersiz altsoy sorguları, özellik histogramları, ölüm nedenleri ve yoğunluk katmanları.
- Eşeysiz üreme seçeneği: tek ebeveyn enerji katkısı, kopyalama+aynı mutasyon sözleşmesi; eş arama maliyeti kalkar ve bu fark raporlanır.
- Eşeyli/eşeysiz karşılaştırmada başlangıç nüfusu, kaynak girdisi, üreme enerji bütçesi ve dış olaylar açıkça eşleştirilir.
- CPU profilindeki gerçek darboğazlar giderilir; gereken bağımsız işler ölçülerek paralelleştirilir.

**Çıkış:** Aynı kayıtla kesintisiz/devam koşusu referansta eşleşir; render kapalı/açık sonuç değişmez; en az 20 eşleştirilmiş seed'lik temel karşılaştırma tamamlanır ve dağılım raporlanır. 20 sayısı kesin istatistiksel yeterlilik iddiası değildir; ileri araştırmada güç/örneklem değerlendirmesi yapılır.

<a id="section-22-version-04"></a>

### v0.3 — Mevsimler ve çevresel bozulmalar

- Sıcaklık, gün/gece, yağış, değişken nem, geçici gölet ve bitki mevsimi.
- Önce kuraklık iyileştirmesi; sonra ayrı etkinleştirilebilir sel ve yangın.
- Korunak/sıcaklık/enerji etkileri; küçük soğuk veya sıcak dalgası deneyleri.
- Harita geçirgenlik sürümü ve çevre değişince güvenli rota geçersizleştirmesi.
- Afet öncesi/sırası/sonrası ölçüm pencereleri, olay planı ve koşu karşılaştırması.

**Çıkış:** Her afetin etki sözleşmesi, kapalı karşı deneyi ve kaynak/hasar kontrolü vardır. Görsel efekt kapalıyken afet aynı sonucu verir. Afet simülasyonun gerçek hızına göre seyrekleşmez veya sıklaşmaz. Birden fazla afet ancak ayrı ayrı doğrulandıktan sonra birlikte etkinleştirilir.

<a id="section-22-version-05"></a>

### v0.4 — Yuva ve çevre mühendisliği

- Topla, taşı, bırak, kaz/yap, kullan, onar temel eylemleri kademeli eklenir.
- Dal/taş stoku, yük kapasitesi, taşıma hızı/enerjisi, yapım işçiliği ve yıpranma.
- Tek basit yuva biçimi, sınırlı kapasite ve dinlenme/korunak faydası.
- Üreticiden bağımsız yapı ömrü, terk edilme ve yeni kullanıcılar.
- Besin taşıma/depo ancak bütçe testleri geçtikten sonra; patika etkisi ayrı anahtardır.
- İnşaat eyleminin beklenen faydası; güvenlik ihtiyacı, bilinen malzeme, yer uygunluğu, kaynak maliyeti ve kalma olasılığından hesaplanır.

**Çıkış:** Malzeme ve enerji korunur; boşluktan yuva oluşmaz; aynı malzeme iki yapıda kullanılamaz; yapı yapıcısı ölünce doğru kalır. Yuva açık/kapalı ve maliyet değişimli deneyler karşılaştırılır. Su akışını değiştiren barajlar bu sürümün zorunlu kabulü değildir; v0.3 su altyapısı yeterliyse ayrı deneydir.

<a id="section-22-version-06"></a>

### v0.5 — Bağlar, gruplar ve sınırlı hafıza

- Bireye ait sınırlı yer ve tanışıklık belleği; son görülme ve unutma.
- Algılanabilen tehlike işareti/alarm; kısa menzilli iletişim maliyeti.
- Yakın durma, gruptan ayrılma, bilinen yuvaya dönme ve yavru yakınlığı.
- Yavru korumada bağ × algılanan tehlike × müdahale faydası; enerji/zaman/yaralanma bedeli.
- Yavruların ebeveynlerini veya bakım verenleri tanıması doğum/temas olaylarından; görünmeyen soy grafiğinden bilgi aktarımı yok.
- Harita keşfi ve bilgi sınırı yeni model profili olarak eklenir.

**Çıkış:** Bellek kapasitesi sınırlı; unutulan bilgiyle hedef takip edilmez. Grup güvenliği ve kaynak rekabeti birlikte ölçülür. Koruma sürekli başarı veya fedakârlık garantisi taşımaz. Sosyal bağ parametresi değiştirilmiş eş koşulda davranış etkisi açıklanabilir.

<a id="section-22-version-07"></a>

### v0.6 — Ölçülmüş ölçek büyütme ve koşullu GPU hesabı

- Önce referans ve Jobs/Burst darboğaz profili; sonra büyük alan/çoklu dünya adayları.
- Toplu çizim, saydamlık maliyeti, güncellenen dünya parçaları ve görsel LOD iyileştirilir.
- GPU'da alan hesabı veya toplu denetleyici yalnızca aktarım dâhil anlamlı kazanımla etkinleşir.
- Backend etiketi, epoch bariyerleri, referans karşılaştırmaları ve tolerans sürümü tamamlanır.
- Laptopta ısınmış uzun koşu ile güç/verim profili kaydedilir.

**Çıkış:** İlgili iş yükünde en az 1,5× uçtan uca kazanım proje eşiği karşılanır; doğruluk kapıları geçer. Kazanım yoksa CPU sonucu ve gerekçe kabul edilir, gereksiz GPU backend üretmek sürüm amacı değildir. Büyük nüfus sayıları kapasite vaadi olarak sunulmaz.

<a id="section-22-version-08"></a>

### v0.7 — Nöroevrim araştırma kolu

- Küçük sabit topolojili denetleyici, ortak gözlem/eylem arayüzü.
- Genom şeması, ağırlık kalıtımı, yavru başına açık değişim bütçesi.
- Vücut genleri ve karar denetleyicisi için ayrı sabitleme deneyleri.
- Enerji/ölüm/üreme sonuçlarıyla doğal seçilim; keyfî başarı puanı gerekirse ayrı mod.
- Kural denetleyicisi aynı dünyada karşılaştırma temeli olarak korunur.

**Çıkış:** Ağ hareket/enerji/algı kısıtlarını aşamaz; kayıtla ağırlıklar aynen geri gelir; tek iyi koşuyla “zekâ gelişti” sonucu verilmez. Kontrolcülerin kararları ve dağılımları ayrı seed'lerde raporlanır. Ağırlık kalıtımının beceri bozulması ölçülür.

<a id="section-22-version-09"></a>

### v0.8 — Deneyimden öğrenme araştırması

- Önce Unity–Python–PyTorch bağlantı uyumluluğu; kısa GPU eğitim testi ve model aktarımı.
- Ortak politika eğitimi ile bireysel yaşam boyu öğrenme ayrı profil ve kayıtlarla geliştirilir.
- Sabit genom + öğrenme açık/kapalı; sabit politika + genom evrimi açık/kapalı karşılaştırmaları.
- Eğitim ve değerlendirme seed'leri ayrılır, farklı kaynak/afet dağılımında değerlendirme yapılır.
- Bellek/ağırlık güncelleme sıklığı, batch, hesap süresi ve varsa biyolojik biliş maliyeti açık tutulur.

**Çıkış:** Öğrenme algoritması düzeyinde tekrar üretilebilir kayıtlar, eğitim eğrileri ve bağımsız değerlendirme vardır. Gerçek bireysel öğrenme olmadan bu etiket kullanılmaz. Ödül tanımının oluşturduğu davranış yan etkileri belgelenir; eğitim süresi ölçülür.

<a id="section-22-version-10"></a>

### v0.9 — İsteğe bağlı hafif 3D ve sunum olgunluğu

- Kamera yaklaşık 85°ye kadar eğilebilir; tepeden okunurluk korunur.
- Blender kaynakları, küçük bedenler, bitkiler, yuvalar ve hafif yürüyüş animasyonları.
- 2D/3D görünümler aynı çekirdek verisini kullanır; fiziksel beden kontrolü ayrı tutulur.
- Yoğunluk/ısı haritası ve soy katmanları 3D görüntüde okunur.
- Görsel kalite seçenekleri ve sessiz uzun gözlem modu tamamlanır.

**Çıkış:** Aynı kayıt farklı görünümde aynı biyolojiyle ilerler. 85°te ağaçlar ve gövdeler seçim/okunurluğu bozmaz. 3D görsel maliyeti raporlanır; yalnızca yeni model eklendiği için yaş/hız/risk semantiği değişmez.

<a id="section-22-version-11"></a>

### v1.0 — Kararlı kişisel laboratuvar

**Amaç:** Seçilmiş mekaniklerle güvenilir, sürdürülebilir ve iyi belgelenmiş hobi uygulaması.

- Kurulum, kullanım, ayarlar, model varsayımları ve deney örnekleri belgelenir.
- Sağlam kayıtlar, açık şema geçişi ve tekrar çalıştırılabilir örnek dünyalar bulunur.
- Temel ekoloji/soy/karar sistemi ve seçilmiş ileri mekanikler kararlı hâle gelir.
- Uzun gözlem, uzun soy geçmişi, farklı başlangıç seed'leri ve kaynak sınırları test edilir.
- Performans sınırları ve desteklenen işletim sistemi/driver kombinasyonu yayımlanan sürüm notunda açıklanır.

v1.0, fikir havuzundaki 72 maddenin tamamının bitmesi demek değildir. v0.6 GPU hesabı, v0.7–v0.8 öğrenme ve v0.9 3D koşullu araştırma kollarıdır; çekirdek kararlılığı sağlandığında bu kolların tamamlanmasını beklemeden kararlı laboratuvar sürümü çıkarılabilir. Sonraki sürümler ayrı araştırma paketleri taşır.

<a id="section-23"></a>

## 23. İlk uygulama iş listesi ve bağımlılıklar

İlk geliştirme sırası aşağıdadır. Her işin çıktısı incelenebilir olmalıdır; bütün sistemi bir kerede yazıp en son çalıştırma yaklaşımı kullanılmaz.

| İş | Somut çıktı | Önkoşul |
|---|---|---|
| T01 | Proje deposu, bu planın aktarımı, Unity/C# sürüm kilitleri | Plan tamam. |
| T02 | Boş Unity sahnesi ve bağımsız Core/Runner derlemesi | T01. |
| T03 | Kimlik, config şeması, birimler, RNG akışları, tick saati | T02. |
| T04 | Parametrik canlı, küçük doğa sahnesi, kamera/renk incelemesi | T02–T03. |
| T05 | Harita hücreleri, nehir/kıyı, bitki stoku, spatial index | T03. |
| T06 | Tek canlının hareketi, statik engel ve kıyıdan içme | T05. |
| T07 | Enerji/doku/hidrasyon/yaş/ölüm ve bütçe testleri | T06. |
| T08 | Algı, utility, acil katman, gerçek neden paneli | T07. |
| T09 | Çoklu tüketim ve hareket arbitrajı, etçil/otçul etkileşim | T08. |
| T10 | Çiftleşme, atomik doğum, genom, mutasyon ve DAG soy | T09. |
| T11 | Hazır korunak, dinlenme maliyeti ve kuraklık | T10. |
| T12 | Kayıt/devam, temel grafikler, hız kontrolü | T03–T11. |
| T13 | Normal/kuraklık kalibrasyonu, smoke/uzun koşu, görsel QA | T12. |
| T14 | v0.1 sürüm notu, bilinen sınırlar ve sonraki iş seçimi | T13. |

İlk uygulama sırasında yeni büyük mekanik fikri gelirse mevcut dilim tamamlanır, fikir geleceğe eklenir. Bilimsel hata, veri kaybı veya temel kullanıcı akışının kırılması aynı dilimde düzeltilir. Erken görsel deneme sonrası kullanıcı tercihleri kaynak dosyalara ve planın sanat bölümüne işlenir; uygulanmamış kararın tamamlandığı söylenmez.

<a id="section-24"></a>

## 24. Gelecek mekanikler: 72 fikrin etki ve sürüm matrisi

Bu envanter önceki fikir havuzunu kaybetmemek için tutulur. **İlk sürüm** sütunu ilk uygulanabilir aşamayı gösterir; aynı aşamada bütün maddelerin bitmesi zorunlu değildir. `Araştırma` etiketi olan işler, kararlı çekirdeğe eklenmeden önce ayrı deney profilinde sınanır. Aşağıdaki etkiler uygulamanın model kurallarıdır; gerçek doğa hakkında otomatik sonuç sayılmaz.

### Haritanın yaşamı

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F01 | Nem haritası | Nem → bitki yenilenmesi ve zemin maliyeti → besin/rota seçimi. | v0.1; nem–stok–nüfus konumsal ilişkisi. |
| F02 | Mevsimsel gölet | Yağış/buharlaşma → su stoku ve içilebilir kıyı → hidrasyon ve hedef erişimi. | v0.3; içme yolculuğu ve susuzluk ölümleri. |
| F03 | Çukur/yükselti | Yükseklik → su birikimi/maruziyet → güvenli yer seçimi. | v0.3 basit alan; daha karmaşık topoğrafya araştırma. |
| F04 | Gölge/sıcaklık | Gölge → ısı maruziyeti ve su kaybı → dinlenme zamanı/konumu. | v0.3; açık/gölgeli alanda rezerv kaybı. |
| F05 | Mağara/dar geçit | Geçit genişliği ile beden yarıçapı → erişilebilirlik ve korunak. | v0.1 kaya aralığı; mağara v0.4; beden sınıfına göre erişim. |
| F06 | Değişen nehir | Geometri/akış değişimi → rota bağlantısı, nem ve izolasyon. | v0.3; bölge bağlantısı ve soyların ayrışması. |

### Bitkiler ve kaynaklar

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F07 | Tükenen/yenilenen otlak | Tüketim ve yenilenme → besin stoku → beslenme ve keşif. | v0.1; stok, tüketim ve üretim bütçesi. |
| F08 | Meyve dönemi | Mevsimsel üretim darbesi → yerel yüksek enerji → hareket ve karşılaşma yoğunluğu. | v0.3; meyve zamanı nüfus akışı. |
| F09 | Tohum taşıma | Tohumun taşınması/bırakılması → uygun hücrede yeni bitki; taşıma/çimlenme maliyeti. | v0.4; kaynak kökeni ve yeni bitki konumu. |
| F10 | Yuva çevresinde bitki | Depolanan besinden tohum düşmesi → yerel yenilenme → sonraki kuşağın kaynak erişimi. | v0.4; yuva çevresi ve kontrol bölgesi stoku. |
| F11 | Besin çeşitleri | Sindirim verimi ve erişim/işleme süresi → net enerji kazancı. | v0.1 bitki/et; ek türler v0.3+; tüketim tercihi. |
| F12 | Tohum bankası | Bekleyen tohum + uygun nem → gecikmeli yeniden büyüme. | v0.3/araştırma; afet sonrası toparlanma süresi. |

### Günlük yaşam

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F13 | Yavru/yetişkin/yaşlı | Yaş ve doku → hareket/algı/üreme kapasitesi. | v0.1; yaşam evresi kapasite eğrileri. |
| F14 | Dinlenme/uyku | Hareket kesilmesi ve uygun yer → enerji harcaması/iyileşme; uyanma gecikmesi ayrı maliyet olabilir. | v0.1 dinlenme; uyku v0.3; zaman bütçesi. |
| F15 | Gündüz/gece etkinliği | Işık ve ısı → algı/maruziyet → faaliyet zamanı. | v0.3; saatlere göre hareket ve ölüm. |
| F16 | Yaralanma/iyileşme | Hasar → sağlık ve hız azalması; iyileşme → enerji tüketimi. | v0.1; iyileşme süresi ve ikinci karşılaşma riski. |
| F17 | Enerji depolama | Rezerv kapasitesi, yük/beden maliyeti → beslenme ve kıtlığa dayanma. | v0.1 rezerv; yağ dokusu ayrımı araştırma. |
| F18 | Hareketsiz zor dönem | Faaliyet azaltma → düşük tüketim, düşük tepki ve üreme fırsatı. | v0.3 araştırma; açık/kapalı enerji ve sağkalım. |

### Beslenme ve avlanma

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F19 | Kovalamaca | Hız/ivme/dönüş/dayanıklılık → temas ihtimali; takip enerji harcar. | v0.1; yakalama ve başarısız takip maliyeti. |
| F20 | Pusu | Bilinen geçitte bekleme → düşük hareket maliyeti, fırsatın gelmeme riski. | v0.5 hafıza; bekleyerek yakalanan besin/enerji. |
| F21 | Fırsatçı av seçimi | Görünen beden/hareket ve erişim → tahmini kazanç/risk. | v0.1; görünmeyen sağlık bilgisi kullanılmaz. |
| F22 | Leşçilik | Ceset stoku ve sindirim → avlanmasız enerji; erişim/rekabet bedeli. | v0.1; leşten gelen besin oranı. |
| F23 | Besin taşıma/depo | Yük → hız/maliyet; depolama → sonraki erişim ve bozulma riski. | v0.4; depoya giren/çıkan/bozulan stok. |
| F24 | Depo baskını | Erişilebilir yabancı stok → enerji; karşılaşma ve savunma riski. | v0.4–v0.5; üretici/kullanıcı farklılığı. |

### Kalıtım ve üreme

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F25 | Farklı kardeşler | Ebeveyn aralıklarından bağımsız örnekleme → farklı kapasite/eğilim bileşimleri. | v0.1; kardeş dağılımı ve aralık testi. |
| F26 | Nadir mutasyon | Yavru başına seyrek gen değişimi → potansiyel kapasite/maliyet değişimi. | v0.1; gerçekleşen oran, sapma ve soyda kalış. |
| F27 | Özellik birleşimleri | Beden/algı/su uyumu maliyetleri aynı bütçede birleşir. | v0.1; koşullu özellik dağılımları. |
| F28 | Yavru sayısı/yatırım | Sabit üreme bütçesinin bölünmesi → yavru başlangıç rezervi ve bakım ihtiyacı. | Tek yavru v0.1; yavru grubu araştırması v0.5+. |
| F29 | Eş seçimi | Görülen uyumlu eş, mesafe, kendi durum ve bağ eğilimi → eşleşme fırsatı/maliyeti. | v0.1 temel; öğrenilen tercihler v0.5+. |
| F30 | Ayrılan soylar | Coğrafi ayrılma → karşılaşma ve üreme havuzu dağılımı. | v0.1 soy ölçümü; gerçek üreme izolasyonu araştırma. |

### Yuvalar ve çevre mühendisliği

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F31 | Güvenli dinlenme yeri | Korunak → maruziyet/dinlenme maliyeti; ulaşım ve doluluk bedeli. | v0.1; korunak kullanan/kullanmayan zaman bütçesi. |
| F32 | Malzemeli yuva | Topla/taşı/yerleştir → malzeme+enerji azalır, korunak kapasitesi artar. | v0.4; maliyet ve barınma getirisi. |
| F33 | Kazma | Kazılabilirlik+işçilik → oyuk alanı; enerji, süre ve çökmeye açıklık. | v0.4 araştırma; toprak hacmi/işçilik bütçesi. |
| F34 | Miras kalan yapı | Yapı ömrü üreticiden bağımsız → sonraki bireylerin erişimi. | v0.4; yapı yaşı, üretici altsoyunun kullanım oranı. |
| F35 | Yuva kümeleri | Güvenli alan, sosyal yakınlık ve kaynak maliyeti → yerleşim yoğunluğu. | v0.5; küme büyüklüğü ve yerel kaynak baskısı. |
| F36 | Suyu etkileyen yapı | Malzemenin akış alanını değiştirmesi → üst/alt havza nemi ve geçiş. | v0.4+ araştırma; su modeli ve bütçe doğrulaması gerekli. |

### Grup ilişkileri

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F37 | Birlikte hareket | Yakınlık faydası + ayrışma kuvveti → konumlar; rekabet/zaman bedeli. | v0.5; grup kalıcılığı ve yakınlık dağılımı. |
| F38 | Ortak alarm | Algılanan işaret → tehlike tahmini/kaçış; yanlış alarm maliyeti. | v0.5; alarmdan kaçışa gecikme ve sağkalım. |
| F39 | Yavru yakınlığı/koruma | Bağ ve algılanan tehdit → yakın kalma/müdahale; beslenme fırsatı ve yara bedeli. | v0.5; koruma girişimi, maliyeti, yavru sağkalımı. |
| F40 | Besin paylaşma | Verici rezerv/taşınan stok → alıcı stok; aktarım verimi ve maliyet. | v0.5; net aktarım ve alıcı/verici sonuçları. |
| F41 | Grubun bölünmesi | Kaynak kişi başına azalır → keşif/uzaklaşma faydası artar. | v0.5; bölünme öncesi yoğunluk ve stok. |
| F42 | Komşuluk/çatışma | Ortak erişim ve bağ/tehdit → kaçınma veya mücadele. | v0.5; bölge örtüşmesi, hasar ve enerji. |

### Duyular, iletişim ve öğrenme

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F43 | Koku izi | Bırakma/yayılma/bozulma → yerel sinyal alanı; maliyet ve algı sınırı. | v0.5 araştırma; iz takibi ve yanlış hedefler. |
| F44 | Alarm işareti | Enerji/zamanla yayınlanan yakın sinyal → alıcının risk hesabı. | v0.5; menzil ve gecikme doğrulaması. |
| F45 | Kaynak hafızası | Konum+son görülme+unutma → geçmiş hedefe ziyaret. | v0.5; hatırlanan/gerçekte mevcut stok farkı. |
| F46 | Tehlike hafızası | Kötü karşılaşma → zamanla sönen yer maliyeti. | v0.5; kaçınma ve alternatif yol bedeli. |
| F47 | Tanışıklık | Tekrarlı temas → bağ kaydı; bellek kapasitesi ve unutma. | v0.5; tanıdık/yabancı davranış farkı. |
| F48 | Takip ederek öğrenme | Görülen hareket/iletişim → bireysel yer/eylem belleği. | v0.5 basit takip; v0.8 öğrenme araştırması. |

### Göç ve alan kullanımı

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F49 | Mevsimsel göç | Kaynak/ısı dağılımının yer değiştirmesi → yeni hedefler ve rota. | v0.3 temel, v0.5 hafızalı; döngüsel rotalar. |
| F50 | İlk nehir geçişi | Su kapasitesi ve rezerv yeterliliği → yeni erişilebilir bölge. | v0.1; ilk geçiş ve sonraki altsoy konumu. |
| F51 | Kalabalıktan yayılma | Düşük kişi başı kaynak ve temas → keşif faydası. | v0.1; yerel yoğunluk–ayrılma ilişkisi. |
| F52 | Patika | Tekrarlı geçiş → bitki aşınması/hareket maliyeti azalması; zamanla onarım. | v0.4; iz yoğunluğu ve rota payı. |
| F53 | Dar boğaz | Az geçiş alanı → bekleme, temas ve risk maliyeti. | v0.1; kuyruk, rota ve karşılaşma sayıları. |
| F54 | Yeniden yerleşim | Boş alan ve kaynak toparlanması → komşu nüfustan yayılım. | v0.3; ilk gelenlerin soy payı ve toparlanma. |

### Afetler

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F55 | Kuraklık | Nem/su/üretim azalır → hidrasyon, açlık, erişim. | v0.1; süre/şiddet ve nüfus tepkisi. |
| F56 | Sel | Su alanı/derinliği artar → geçiş kapanır, yuva maruziyeti artar. | v0.3; boğulma/yer değiştirme/yapı hasarı. |
| F57 | Yangın | Yakıt yanar, sıcaklık/hasar ve bitki kaybı oluşur → acil kaçış. | v0.3; yayılan alan ve kaçış maliyeti. |
| F58 | Sıcak/soğuk dalgası | Isı maruziyeti ve su/enerji kaybı → barınma/faaliyet zamanı. | v0.3; sıcaklık–rezerv ve sağkalım eğrisi. |
| F59 | Fırtına | Ağaç/yapı hasarı, yeni engel ve malzeme → erişim ve onarım. | v0.3 basit, v0.4 yapılar; hasar/toparlanma. |
| F60 | Deprem/heyelan | Arazi bağlantısı ve nesne yerleşimi değişir → yollar/izolasyon. | İleri araştırma; yeniden rota ve alan bağlantısı testi. |

### Ekosistem bağlantıları

| Kimlik | Olasılık | Mekanik etki ve karar bağlantısı | İlk sürüm / ölçüm |
|---|---|---|---|
| F61 | Hastalık | Temas/su maruziyeti → yük, sağlık/enerji maliyeti; açık bulaşma kuralı. | v0.5+ araştırma; bulaşma zinciri ve yoğunluk etkisi. |
| F62 | Parazit | Konaktan sınırlı kaynak aktarımı → rezerv/kapasite azalması. | İleri araştırma; konak ve parazit bütçesi. |
| F63 | Karşılıklı yarar | Temizleme/aktarım gibi eylem → iki bireyin açık kaynak/durum değişimi. | v0.5+ araştırma; fayda/maliyet ve eşleşme. |
| F64 | Çürüme/toprağa dönüş | Doku/bitki kaybı → tanımlı toprak besini alanı; üretim girdisiyle tutarlı. | v0.3+; madde hesabı enerjiden ayrı birimle yapılır. |
| F65 | Avcının bitkiye dolaylı etkisi | Otçul konumu/sayısı değişir → otlanma basıncı ve stok. | v0.1 mümkün; v0.2 kontrollü karşılaştırma. |
| F66 | Dışarıdan gelen grup | Açık müdahaleyle yeni kimlik/genomlar → rekabet ve üreme havuzu. | v0.2 müdahale modu; başlangıç ve giriş zamanı kayıtlı. |

### Kullanıcı ve araştırma araçları

Bu altı özellik canlılara biyolojik avantaj sağlamaz; gözlem doğruluğu ve deney tekrarını etkiler. Kamera veya analiz aracı açık diye canlı parametresi değiştirilmez. Kullanıcının “her olasılığın etkisi olsun” ilkesi biyolojik mekaniklerde nedenselliği, gözlem araçlarında gerçek veriye dayanmayı gerektirir.

| Kimlik | Olasılık | Veri/deney etkisi | İlk sürüm / ölçüm |
|---|---|---|---|
| F67 | Yaşam filmi | Seçili birey rotası ve olaylarının kayıtlı geçmişi. | v0.2; kayıt sıklığı ve zaman kapsamı görünür. |
| F68 | Yaşayan soy grafiği | Benzersiz altsoy, yaşayan/toplam ve olay bağlantıları. | v0.1 temel, v0.2 tam; DAG sayım testleri. |
| F69 | Dünya geçmişi | Snapshot+komut replay veya örneklenmiş hızlandırıcı geçmişi. | v0.2; yeniden oynatma biçimi etiketli. |
| F70 | Dünya çatallama | Aynı başlangıç/snapshot'tan yeni yapılandırma ve dal kimliği. | v0.2; anne dal korunur, müdahale kaydedilir. |
| F71 | Tek değişkenli deney | Eşleştirilmiş dış RNG akışları ve yapılandırma farkı. | v0.2; tüm değişken farkları listelenir. |
| F72 | Keşif günlüğü | Gerçek olaylar üzerinde belgeli eşik/sorgu ile özet. | v0.2; hayalî hikâye üretilmez. |

### Daha uzak ufuk: Topluluk ve kültür araştırmaları

Bu araştırmalar çekirdeğin zorunlu yol haritasından ayrıdır; ilgili algı, hafıza ve eylem altyapısı olmadan etkinleştirilmez.

| Fikir | Gereken mekanik | Etki / maliyet | Temel kontrol |
|---|---|---|---|
| Alet kullanma | Taşıma, nesne–işlem uygunluğu, kullanma eylemi | İşlem verimi artar; yük, zaman, dayanıklılık azalır | Aletin kayıtsız enerji üretmemesi. |
| İş bölümü | Tekrarlı görev, bireysel durum, grup/yer bilgisi | Farklı zaman bütçeleri; koordinasyon ve eksik görev riski | Görev dağılımı kodda sabit atamayla mı, karardan mı doğuyor? |
| Takas | Sahip olunan stok, karşılıklı kabul, atomik aktarım | Farklı kaynakların değişimi; yol/iletişim maliyeti | İki tarafın stokları ve işlem tekilliği. |
| Ortak yapı | Çoklu malzeme katkısı, kullanım kapasitesi | Paylaşılan korunak; bakım ve aşırı kullanım | Katkı/kullanım grafiği. |
| Yerel gelenek | Bireysel öğrenme ve gözlenebilir sosyal aktarım | Genom sabitken farklı eylem dağılımları | Ortak çevre veya gen farkının açıklamaması. |
| Göçle bilgi taşıma | Bireye ait kalıcı bellek ve yeni grupta etkileşim | Kaynak/rota bilgisinin farklı bireylere ulaşması | Bilginin hangi gözlenebilir eylemle aktarıldığı. |

<a id="section-25"></a>

## 25. Deney kütüphanesi ve ölçü tanımları

### İlk deney paketleri

| Deney | Değiştirilen şey | Sabit tutulacaklar | Ana çıktılar |
|---|---|---|---|
| D01 Kaynak dağılımı | Dağınık/kümeli besin | Toplam üretim, kurucu dağılımı, seed eşleştirmesi | Yayılım, kümelenme, yol maliyeti. |
| D02 Kuraklık | Süre/şiddet veya kapalı | Harita, başlangıç nüfusu/genler, diğer dış olaylar | Nüfus kaybı, yer değiştirme, toparlanma. |
| D03 Eşeyli/eşeysiz | Üreme modu | Başlangıç, açık enerji bütçeleri, kaynak/afet | Üreme başarısı, çeşitlilik, soy devamı. |
| D04 Mutasyon | 0 ve çok düşük olasılık | Başlangıç genom çeşitliliği, diğer ayarlar | Varyans, yeni değerler, salınım/yok oluş. |
| D05 Su erişimi | Nehir zorluğu / su uyumu başlangıcı | Diğer maliyetler ve harita kaynakları | İlk geçiş, kolonizasyon, iki yaka dağılımı. |
| D06 Avcı etkisi | Etçil başlangıcı/erişimi | Toplam kaynak ve açık nüfus farkı açıklaması | Otlanma dağılımı, otçul ve bitki dinamiği. |
| D07 Korunak | Etki/kapsama/maliyet | Kaynak üretimi ve birey özellikleri | Dinlenme yeri, sağkalım, yoğunluk. |
| D08 İnşaat | Eylem açık/kapalı veya malzeme bedeli | Hazır korunak, hava, genom dağılımı | Yapı sayısı, net enerji, sonraki kuşak etkisi. |
| D09 Sosyal bağ | Bağ eğilimi/iletişim açık-kapalı | Besin ve hareket maliyetleri | Grup kalıcılığı, koruma, rekabet. |
| D10 Bellek | Kapasite/unutma ve kapalı | Duyular, genom, dünya olayları | Yol verimi, yanlış hatırlama, tekrar ziyaret. |
| D11 Denetleyici | Kural/nöroevrim/öğrenme | Açık gözlem/eylem ve bütçe sözleşmesi | Enerji, üreme, genelleme, karar davranışı. |
| D12 Hızlandırıcı | CPU/Burst/GPU | Model/config/input ve tolerans standardı | Hata, olay farkı, duvar saati/güç. |

Tek değişkenli karşılaştırmada tasarım gereği zorunlu yan farklar açıklanır: eşeysiz modda eş arama maliyetinin yok olması mekanizmanın bir parçasıdır. Avcıyı kaldırmak toplam başlangıç nüfusunu da değiştiriyorsa bu fark rapora yazılır; nüfus eşleştirilmiş ek kontrol gerekebilir.

### Ölçüler ve yorumlama

- **Nüfus:** Tick sonunda yaşayan birey sayısı; ceset ve doğum niyeti sayılmaz.
- **Doğum/ölüm oranı:** Açık zaman penceresindeki sayılar ve uygun nüfus paydası; çok küçük örneklerde belirsizlik görünür.
- **Genom dağılımı:** Gen değerlerinin histogramı, ortanca, yayılım ve gerekirse özellikler arası ilişki. Mevcut sağlık/hız ile genom ayrı ölçülür.
- **Nesil:** Soy grafiği derinliği tanımıdır; aynı nesilde doğum tarihleri farklı olabilir.
- **Üreme çıktısı:** Doğan çocuk, yetişkinliğe ulaşan çocuk ve yaşayan altsoy ayrı metriklerdir.
- **Yayılım:** İşgal edilen hücre oranı, bölge başına yoğunluk, merkezden uzaklık ve iki yaka geçişleri. Tek ortalama konum kümeleri gizleyebilir.
- **Göç:** Tanımlı bölgeler arası geçiş; görsel gezinmenin tamamı göç diye etiketlenmez.
- **Enerji verimi:** Kazanılan enerji, hareket/bakım/üreme giderleri ve kayıplar ayrı. Bölme paydası sıfırsa “veri yok” olur.
- **Yakınsama:** Belirlenmiş pencerede özellik dağılımı/değişim hızının kararlılığı; sadece ortalamanın sabit olması yeterli sayılmaz. İki ayrı kümeye ayrışma veya periyodik salınım ayrıca incelenir.
- **Yok oluş:** Son yaşayan bireyin ölüm tick'i ve ana koşullar. İşlemci/bellek sınırı veya kullanıcı durdurması ayrı bitiş nedenidir.
- **İnşaat etkisi:** Yapım enerjisi, kullanım süresi, maruziyet farkı, üreticinin ve başka bireylerin faydası.
- **Öğrenme başarısı:** Eğitimdeki skor yanında görülmemiş seed/koşulda gerçek davranış ve yaşam bütçeleri.

### İstatistik ve kayıt uygulaması

v0.2 karşılaştırmaları küçük bir seed grubu ile hata ayıklamadan sonra en az 20 eşleştirilmiş başlangıçta başlar. Bütün başarılı/başarısız koşular tutulur. Eşik ve karşılaştırma penceresi sonuçları gördükten sonra seçildiyse keşif analizi olarak belirtilir. Daha ciddi çıkarımlar için örneklem sayısı ve belirsizlik ayrıca değerlendirilir.

Sonuçlar model sürümü, ayar farkı, seed seti ve backend bilgisiyle dışa aktarılır. Ham olay listesi ile hesaplanmış metrik sürümü ayrı saklanır; analiz formülü değişince simülasyonu yeniden çalıştırmadan bazı çıktılar yeniden hesaplanabilir. “Bu dünyada şu şartlarla gözlendi” dili kullanılır.

<a id="section-26"></a>

## 26. Rastgelelik, kayıt biçimi ve hata davranışı ayrıntıları

### Rastgele sayı standardı

İlk CPU referans uygulamasında PCG32-XSH-RR ailesinden küçük, kodu proje içinde sabitlenmiş bir üretici kullanılacak. Dış bağımlılığın rastgele sürüm değişimiyle bütün deneyler değişmeyecek. PCG'nin resmî örnekleri sabit başlangıçla beklenen çıktı vektörleri ve ayrı akış durumlarını tanımlar; C# uygulaması bu vektörlerle doğrulanacak. [PCG referans uygulaması ve kullanım sözleşmesi](https://www.pcg-random.org/using-pcg-c-basic.html)

Akış alanları en az `map`, `founders`, `weather`, `reproduction`, `mutation`, `exploration`, `arbitration` ve `cosmetics` olarak ayrılır. Paralel işlerde tek paylaşılan RNG sırası kullanılmaz. Canlı başına veya işlem başına durum/sayaç eşleme yöntemi sürümlenir. Birden fazla dünyanın seed'leri manifestte bulunur; işletim sisteminden rastgele başlangıç seçilirse üretilen seed koşu başlamadan kaydedilir.

Tam sayıya seçimde modulo yanlılığı oluşturan kestirmelerden kaçınılır; aralık örnekleme, Bernoulli ve normal dağılım yöntemleri de RNG sözleşmesinin parçasıdır. Normal dağılım üreticisinin sakladığı ikinci örnek gibi gizli önbellekler checkpoint'e girer. Kozmetik rastgeleliği değiştiren bir shader veya asset güncellemesi biyolojik RNG çağrı sayısını değiştiremez.

### Koşu dizini ve dosyalar

Aşağıdaki düzen uygulama geliştirildiğinde oluşturulacak tasarımdır; bu plan hazırlanırken bu deney dosyaları üretilmiş değildir.

```text
Runs/<run-id>/
  manifest.json             # sürümler, donanım, backend, seed ve config hash
  config.json               # kullanıcı ayarları ve tam ModelProfile
  commands.jsonl            # zaman damgalı kullanıcı müdahaleleri
  events.jsonl              # doğum, ölüm, mutasyon, afet ve yapı olayları
  metrics.csv               # birimleri tanımlı temel zaman serileri
  checkpoints/              # tick etiketli, sürümlü sıkıştırılmış durum
  analysis/                 # ölçüm formülü sürümüyle üretilmiş sonuçlar
  performance.json          # profil, gerçek hız, bellek ve sıcak koşu özeti
  finish.json               # normal süre/yok oluş/kullanıcı/kapasite/hata nedeni
```

JSON Lines ilk sürümde incelenebilir olay biçimidir. Dünya durumunun büyük dizileri için sürümlü ikili snapshot tercih edilebilir; şema, endian/alan biçimleri, sıkıştırma ve checksum açık tanımlanır. Daha sonra sütunlu analiz biçimine geçilmesi ham olay anlamını değiştirmez. Diskte kaydedilen numaralarda kültüre bağlı virgül/nokta farkı yoktur; UTC gerçek saat ile simülasyon tick'i ayrı alanlardır.

### Checkpoint ve replay ayrıntısı

Kayıt yalnız tamamlanmış tick sınırında alınır. Bu noktada commit edilmemiş yaşam işlemi bulunmaz; yine de ileriye zamanlanmış komutlar, rota sırası, eylem durumu, karar fazı, tekrar kullanılabilir slot nesilleri ve bütün RNG durumları saklanır. Yeniden oluşturulabilen türetilmiş önbelleklerin hesap sırası sabittir; bunun doğru olduğu save/load testiyle gösterilir.

Kalıcı kimlikler `run-id + local-id` olarak çözülür. Snapshot'tan dal oluşturulduğunda önceden var olan canlıların köken kimliği korunur; yeni doğumlar dalın kimlik alanında oluşur. Ana koşu ve dalın ortak geçmişi analizde iki ayrı doğum olayı gibi sayılmaz. Ebeveyn referansları kuruculara veya mevcut kimliklere gider; gelecekte doğacak kimliğe başvuru yapılamaz.

Veri yazma kuyruğu sınırlıdır. Kuyruk dolarsa gözlem arayüzü durumu gösterir ve hesap güvenli tick sınırında yavaşlar. Önceliksiz ayrıntılı görsel iz kaydı azaltılabiliyorsa bu eksiklik manifestte görünür; doğum/ölüm/mutasyon veya kullanıcı müdahalesi sessizce atılmaz. Geçerli tamamlanmış son kayıt korunur.

### Hata sınıfları

| Durum | Davranış |
|---|---|
| Geçersiz config veya harita | Başlatma durur; sorunlu değer/alan açıklanır. |
| NaN, sonsuz değer, kimlik tutarsızlığı | Koşu hata etiketiyle durur; son sağlam checkpoint ve dar hata dökümü korunur. |
| Kaynak negatife kaçması | Hata olarak raporlanır; sessiz sıfırlama ile bütçe hatası gizlenmez. |
| Enerji ihtiyacının rezervi aşması | Modelde tanımlı karşılanamayan tüketim/sağlık etkisi uygulanır; aritmetik hata sayılmaz. |
| Disk dolması veya kayıt hatası | Başarı bildirimi verilmez; kontrollü durdurma ve sağlam kaydı koruma. |
| GPU sonucu geç gelmesi | İlgili epoch bariyerinde beklenir; eski alanla biyoloji ilerlemez. |
| GPU/driver kaybı | Koşu teknik hata olarak biter veya sağlam checkpoint'te durur; backend değişimi yeni dal. |
| Nüfus/RAM sınırı | Açık kapasite bitişi; canlı silerek devam edilmez. |
| Canlıların doğal yok oluşu | Başarılı tamamlanmış deney sonucu olabilir; teknik hata sayılmaz. |

Enerji denklemlerinde talep ve gerçekleşen ödeme ayrı tutulur. Rezervden ödenebilen kısım sıfıra kadar düşülür; karşılanamayan zorunlu metabolizma/hidrasyon talebi tanımlı hasar etkisini üretir. Üreme, büyüme ve onarım gibi isteğe bağlı harcamalar yeterli rezerv yoksa başlatılmaz veya izin verilen kısım kadar gerçekleşir. Negatif rezervi sebepsizce sıfırlayıp aynı büyümeyi/yavruyu üretmek yasaktır.

<a id="section-27"></a>

## 27. Test, kalibrasyon ve sürüm kabul stratejisi

### Model değişmezleri

1. Yaş ve tick geri gitmez; doğum tarihinden önce yaşayan birey yoktur.
2. Her bireyin tek kalıcı kimliği vardır; slot yeniden kullanımında eski hedef yeni bireye dönüşmez.
3. Kaynak stokları ve beden dokusu negatif değildir; tüketim mevcut stokla sınırlıdır.
4. Bir kaynak birden çok tüketiciye toplam stoktan fazla aktarılamaz.
5. Doğum ve büyüme açık bütçenin dışında enerji üretmez.
6. Eylem önkoşulları commit sırasında tekrar geçerlidir.
7. Canlı gerçek yarıçapıyla yasak engeli/suyu aşamaz.
8. Algılanmayan hedefin canlı durumu karar hesabına sızmaz.
9. Genom normal koşulda doğumdan sonra yazılmaz; görünüm/yaralanma genom değildir.
10. Mutasyonsuz kalıtım ebeveyn aralığında kalır.
11. Soy grafiğinde döngü ve çift sayım yoktur.
12. Kamera, FPS, görsel kalite ve ses biyolojik sonuçları değiştirmez.

### Somut küçük kontrol sahneleri

| Kontrol | Düzenek | Beklenen sonuç |
|---|---|---|
| Q01 Ön/arka besin | Bir canlı, biri görüşte biri arkada iki kaynak | Yalnız algılanan hedef adaydır; yön değişince durum güncellenir. |
| Q02 Duvar arkası | Görüş mesafesi içinde opak engel arkasında kaynak | LOS yoksa görülmüş sayılmaz. |
| Q03 Kıyı içme | Yüzemeyen susuz canlı ve erişilebilir kıyı | Kara üzerinde durup içer; suya ışınlanmaz. |
| Q04 Dar geçit | Aynı geçitte küçük ve büyük yetişkin | Sadece yarıçapı uygun olan rota kullanır. |
| Q05 Yüksek hız | İnce kaya engeline hızlı ilerleme | Süpürme kontrolü tünellemeyi engeller. |
| Q06 Kaynak çekişmesi | Aynı stoktan eşzamanlı çoklu talep | Oransal tahsis toplam stoku aşmaz; işçi sırası sonucu değiştirmez. |
| Q07 Karşılıklı darbe | İki canlı aynı tick'te ölümcül hasar verir | İkisi de ölebilir; döngü sırası veya aynı tick iyileşme birini kurtarmaz. |
| Q08 Başarısız doğum | Uygun çift ama yavruya yer yok | Enerji kesilmez, kimlik/soy kaydı yarım oluşmaz. |
| Q09 Ölüm ve üreme | Çiftin biri işlem tick'inde ölür | Son geçerlilik koşulu üremeyi engeller. |
| Q10 Kalıtım uçları | Ebeveynler 0.2/0.8, sonra 0.5/0.5 | Mutasyonsuz aralık ve eşitlik; zorlanmış mutasyon aynı ebeveynde de çalışır. |
| Q11 Ortak torun | Akraba dalların tekrar birleştiği elle kurulmuş DAG | Aynı altsoy bir kez sayılır; yaşayan/toplam ayrılır. |
| Q12 Kayıt devam | Ortada snapshot alınan koşu ve kesintisiz kontrol | Aynı referans build'de aynı tamamlanmış tick hash'i. |
| Q13 Hız bağımsızlığı | 1×, 8× ve penceresiz koşuda aynı tick sayısı | Referans olaylar ve durum özeti eşleşir. |
| Q14 Kozmetik bağımsızlık | Farklı görsel seed, çözünürlük, LOD ve kamera | Biyoloji aynı kalır. |
| Q15 Afet RNG ayrımı | Üreme sayısı farklı ama aynı dış olay akışı | Eşleştirilmiş hava olaylarının takvimi değişmez. |
| Q16 Yuva bütçesi | Aynı son malzemeye iki yapım talebi | Tek kullanım ve atomik maliyet. |
| Q17 Hafıza kaybı | Kaynak ortadan kalkar; son görülme kaydı eskir | Canlı gerçek gizli kaynağı canlı izlemeyi sürdürmez. |
| Q18 GPU gecikmesi | Alan hesabı yapay geciktirilir | Biyoloji eski epoch'la ilerlemez; UI son durumu gösterebilir. |

Bu kontroller uygulama için gereklidir; bu plan dosyası hazırlanırken çalıştırılmış test sonuçları değildir. Uygulamada başarısızlık önce küçük sahnede yeniden üretilir, sonra düzeltme ana dünyaya taşınır.

### Rastgelelik ve dağılım kontrolleri

Kalıtımın şekli, özellik/ebeveyn aralıklarına göre örneklenir. Birkaç yavruya bakıp dağılım doğrulandı denmez. Mutasyon sıklığı, önceden belirlenmiş seed ve örneklemle binom dağılımına uygun aralıkta kontrol edilir. Çok düşük olasılığın her kısa koşuda en az bir olay üretmesi beklenmez. İstatistik kontrolünün yanında olayın zorlandığı birim kontrolleri, sıfır olasılık ve sınır yansıtma kontrolleri bulunur.

Normal dağılım ve yansıtmanın uçlarda oluşturduğu sonuçlar incelenir. Kalıtımın çeşitliliği daraltması ile doğal seçilimin aynı ortalamaya yöneltmesi ayrı mekanizmalardır. Gen dağılımı, doğum sayısı ve gerçekleşen mutasyonların tamamı birlikte raporlanır.

### Kalibrasyon sırası

1. Bitki üretim/tüketim ve tek otçul enerji bütçesi.
2. Su erişimi, hareket ve metabolizma nedeniyle yaşama süresi.
3. İki ebeveynli üreme ve yavrunun büyümesi.
4. Otçul nüfusun kaynakla ilişkisi; artış ve çöküş örnekleri.
5. Etçilin besine erişimi, avlanma maliyeti ve ceset ekonomisi.
6. Karışık nüfus, normal ve kuraklık koşulları.
7. Çoklu seed, uzun süre ve farklı yoğunluk.

Bir denge katsayısı değiştirilince etkilediği kontroller çalıştırılır. Testleri geçen düşük etkili görsel değişiklik için bütün uzun deneyler tekrar koşturulmaz. Model veya sayısal backend değişikliği daha geniş doğrulama gerektirir.

### Performans ölçüm protokolü

Önce editör tanılamasıyla darboğaz bulunur, sonra ayrı derlenmiş uygulama/runner ile performans raporu alınır. Donanım, sürücü, güç modu, çözünürlük, build türü, model/config, nüfus ve çalışma süresi raporda yer alır. Dizüstünde kısa soğuk ölçüm ve ısınmış 30 dakikalık koşu ayrı tutulur. Kullanıcının güç/fan ayarları sessizce değiştirilmez.

Tek bir ortalama FPS yeterli değildir. p95/p99 adım süresi, rotaya ayrılan zaman, algı sorgusu sayısı, kaynak arbitrajı, kayıt kuyruğu, GC duraklaması, CPU–GPU aktarımı ve gerçek simülasyon hızına bakılır. İş yükü nüfus arttıkça, belirli bir su noktasında yığılınca ve soy grafiği büyüyünce tekrar ölçülür.

GPU'yu etkinleştirmek için küçük kernel süresinin hızlı olması yetmez. Aynı model ve iş yükünde komut gönderme, veri taşıma, bekleme ve sonuç doğrulama dâhil toplam süre karşılaştırılır. GPU yüzde 100 dolarken kullanıcı arayüzünün tepkisizleşmesi veya toplam deney sayısının düşmesi iyileştirme sayılmaz.

<a id="section-28"></a>

## 28. Bakım, geliştirme düzeni ve risk kaydı

### Kod ve belge düzeni

Önerilen depo:

```text
docs/plan.md
docs/decisions/
docs/model-reference/
src/Evosim.Core/
src/Evosim.Contracts/
src/Evosim.Runner/
unity/                         # Unity projesi; Assets/Packages/ProjectSettings
tests/Core/
tests/Scenarios/
tools/analysis/
tools/art/
benchmarks/profiles/
```

Unity'nin ürettiği Library/Temp gibi dizinler kaynak deposuna girmez. Büyük düzenlenebilir 3D/raster kaynaklar gerektiğinde uygun büyük dosya takibiyle yönetilir; kullanılmayan dev paketler eklenmez. .meta dosyaları ve asset kimlikleri korunur. Her sürümde model değişikliği ile yalnızca sunum değişikliği sürüm notunda ayrılır.

İşler küçük incelenebilir değişiklikler halinde yapılır. Her değişiklik problem, davranış, doğrulama ve bilinen sınırla açıklanır. Bir işi “tamam” saymak için kodun var olması yeterli değildir; ilgili kontrol ve uygulama akışı çalışmalıdır. Bağımlılık sürümü değişikliği normal özellik geliştirmesiyle aynı anda gereksiz yere yapılmaz.

### Öncelikli riskler

| Risk | Erken sinyal | Yanıt |
|---|---|---|
| Çok düşük mutasyon ve daralan çeşitlilik | Gen varyansı azalır, yeni kombinasyonlar az | Başlangıç dağılımını ve kalıtım etkisini ölç; mutasyonu gizlice arttırma. |
| Karar sisteminin aynı eylemde takılması | Uzun hareketsizlik, geçersiz hedef takibi | Durum makinesi, geçerlilik ve histerezis kontrol sahneleri. |
| Açlık/üreme döngüsünde enerji açığı | Bütçe beklenmedik büyür | Talep/ödeme/doku/yavru/ceset defteri. |
| Parametre patlaması | Aynı davranış için çok sayıda bağımsız kaydırıcı | Mevcut durum/bedel zincirine bağla; yeni gen için ayrı karar kaydı. |
| GPU optimizasyonunun modeli değiştirmesi | Olay/dağılım farkı, eski alan kullanımı | Referans, tolerans, epoch bariyeri ve ayrı backend etiketi. |
| Tek seed'e göre dengeleme | Bazı dünyalarda anlamsız sürekli çöküş/taşkın | Eşleştirilmiş seed setleri ve açık başarısızlık sonuçları. |
| Saydam görsel kalabalığı | Üst üste renk çamuru ve GPU piksel maliyeti | Tek gövde katmanı, görsel LOD, kontrast ve yakınlaşma QA. |
| Soy geçmişinin büyümesi | Panel açılırken donma, sınırsız RAM | Olay deposu, kademeli DAG sorgusu, snapshot'ta zaman etiketi. |
| Eğitim paket uyumsuzluğu | CUDA/Unity aktarım hatası | İzole uyumluluk testi ve kilitli sürüm kesişimi. |
| Öğrenilmiş politika açığı | Yüksek ödül ama yanlış yaşam davranışı | Bağımsız seed, ödül denetimi, gerçek enerji/üreme ölçüleri. |
| Çok fazla erken özellik | Hangi mekanik etkilediği anlaşılmaz | Sürüm dilimleri ve kapalı karşı deney. |
| Sanat–fizik ayrışması | Görsel beden engelden geçiyor veya seçim şaşıyor | Ortak beden verisi, pivot ve çarpışma görselleştirmesi. |

### Sürüm tamamlanma kontrol listesi

- [ ] Kapsamdaki mekaniklerin nedensel etki sözleşmesi var.
- [ ] Kalıtsal özellik, durum ve çevre alanları ayrılmış.
- [ ] Gerekli küçük kontrol sahneleri ve kayıt/devam testi geçiyor.
- [ ] Seçili bireyin karar nedeni gerçek hesapla eşleşiyor.
- [ ] Enerji/kaynak/kimlik değişmezleri korunuyor.
- [ ] Model/config/backend sürümleri sonuçlarda bulunuyor.
- [ ] Görsel ve performans kabul ölçümleri raporlanmış.
- [ ] Doğal yok oluş, kapasite durması ve teknik hata ayrılmış.
- [ ] Kaynak dosyalar, değişiklik notu ve bilinen sınırlar tamam.
- [ ] Gelecek iş listesi yeni duruma göre güncellenmiş.

<a id="section-29"></a>

## 29. Kısa terimler sözlüğü

| Terim | EVOSIM'deki anlamı |
|---|---|
| Tick | Sabit süreli bir hesap adımı; ilk modelde 0,05 simülasyon saniyesi. |
| Genom | Doğumda kalıtımla gelen normalize potansiyel/eğilim değerleri. |
| Fenotip / mevcut kapasite | Genomun yaş, beslenme, sağlık ve çevreyle oluşan güncel etkisi. |
| Utility | O anki eylemlerin beklenen fayda ve bedellerinden hesaplanan öncelik. |
| Histerezis | Küçük puan farkları yüzünden eylemlerin sürekli değişmesini önleme kuralı. |
| LOS | Görüş hattının bir engelle kesilip kesilmediği. |
| A* | Geçilebilir hücreler ve maliyetler üzerinden rota arama yöntemi. |
| Steering | Rota yönü, kaçınma ve yavaşlamadan sürekli hareket üretme. |
| SoA | Aynı veri alanlarını ardışık dizilerde saklayan düzen. |
| Shader | Görsel yüzeyin rengini/ışığını/deformasyonunu GPU'da hesaplayan program. |
| Instancing | Benzer şekilleri farklı birey verileriyle topluca çizme. |
| Headless / penceresiz | Görüntü çizmeden simülasyon ve deney çalıştırma. |
| Backend | Aynı hesap sözleşmesinin CPU, Burst veya GPU uygulaması. |
| Epoch | Belirli bir alan güncellemesinin ait olduğu zaman sınırı. |
| Checkpoint | Devam etmek için gereken bütün durumun tutarlı kaydı. |
| Seed | Rastgele başlangıcı tanımlayan değer; mevcut RNG durumunun tamamı değildir. |
| DAG | İki ebeveynli soy ilişkileri gibi yönlü ve döngüsüz grafik. |
| Nöroevrim | Sinir ağına ait kalıtsal değerlerin nesiller boyunca değişmesi. |
| Pekiştirmeli öğrenme | Deneyim ve tanımlı geri bildirimle karar modelinin güncellenmesi. |
| PPO | Daha sonra değerlendirilebilecek bir politika öğrenme algoritması; bireysel öğrenmeyle eş anlamlı değildir. |

<a id="section-30"></a>

## 30. Planın kapanış kararı ve sıradaki işlem

Bu planın hazırlanması tasarım işinin çıktısıdır. Unity projesi, assetler, simülasyon, testler veya GPU eğitimi henüz bu belge nedeniyle tamamlanmış sayılmaz. Bir sonraki uygulama işi **T01–T04: proje iskeleti, aynı çekirdeğin iki çalıştırıcısı, sabit saat ve seçilen görsel dilin küçük sahnesi** olacaktır.

İlk yaşayan dünya v0.1 kapsamıyla sınırlandırılır. Gelecek fikirler F01–F72 kimlikleriyle korunur; her biri zamanı geldiğinde açık etki sözleşmesi ve kabul kontrolüyle geliştirilir. Amaç, her sürümde hem izlemesi keyif veren hem de nedenlerini araştırabileceğimiz bir dünya elde etmektir.
