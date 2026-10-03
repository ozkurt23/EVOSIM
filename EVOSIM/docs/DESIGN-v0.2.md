# EVOSIM — Güncel tasarım sözleşmesi

## 1. Esas düzeltme

**Canlının kararını hazır bir kaç/avlan/eş ara sistemi vermeyecek.** Biz bedenin olanaklarını, duyuları, kaynakları ve fiziksel sonuçları tanımlıyoruz. Kalıtsal karar ağı motor komutlarını üretir. Gerçek yaşam ve üreme sonuçları, hangi karar yapılarının sonraki nesillere geçeceğini belirler.

Bu tasarım, eski planın utility öncelikleri, otomatik kaçış, nöroevrimi ileri sürüme erteleme, beyaz yaşlılık ve her yere yayılan besin varsayımlarının yerine geçer. Önceki ayrıntılı plan tarihsel arşivde saklanır; çelişkide **bu belge** geçerlidir.

## 2. Beden, karar ve öğrenme birbirinden ayrılır

| Katman | Uygulama |
|---|---|
| Dünya | Arazi, nehir, meyve, ot, ceset, kuraklık ve kaynak yenilenmesi |
| Beden | Enerji, su, sağlık, büyüme, yaşlanma, hareket/saldırı maliyeti ve üreme uygunluğu |
| Duyular | Sınırlı görüşte nesne yönü/yakınlığı, beden silueti, yakın engeller ve kendi ihtiyaçları |
| Kalıtsal beyin | 30 girdi → 8 tekrarlayan gizli birim → 5 motor çıktısı; 357 ağırlık |
| Geçici bellek | Önceki gizli aktivasyonlar; doğumda sıfır, genetik ağırlıklardan ayrı |
| Seçilim | Dünyada yaşayabilen ve gerçekten yavru bırakabilen soyların devamı |

Başlangıç ağları çeşitli rastgele ağırlıklarla kurulur; önceden eğitilmiş kaçış becerisi yoktur. Yaşam sırasında ağırlık güncellemesi, PPO, elle yazılmış puan veya ödül optimizasyonu yapılmaz. Geçici aktivasyon belleği öğrenilmiş ağırlık anlamına gelmez.

**Bu sürüm kaçışın başarıyla evrimleştiğini iddia etmez.** Kaçış dahil davranışların evrimleşebileceği mekanizmayı kurar. Kötü hareket eden, dönen, bekleyen veya duvara yürüyen bir başlangıç bireyi mümkün bir model sonucudur. Fizik/navigasyon hatasıyla kötü politikanın farkı motor çıktıları ve engelde kalma süresiyle incelenir.

## 3. Ağın karar alanı

Ağ çıktıları:

1. İleri/geri hareket isteği.
2. Sağa/sola dönüş.
3. Ağız/temas etkileşimi.
4. Isırma girişimi.
5. Çiftleşmeye açıklık sinyali.

Ağ `avcı`, `kaç`, `avı kovala` gibi etiketlenmiş davranış seçmez. Yakındaki başka bedenin görünür beslenme silueti algılanabilir, fakat o siluete hangi tepkiyi vereceği kodlanmaz. Karşı saldırı da otomatik değildir. Uygun eylemin gerçekleşmesi fiziksel temasa, kaynak ve beden sınırlarına tabidir.

Duyular kendi enerji/su/sağlığını, yakın zamanda alınan hasarı, olgunluğu, üreme beklemesini, bitki/meyve–su kıyısı–leş–başka beden yönlerini ve yakınlıklarını, ön/sol/sağ engel yakınlığını, gerçekleşen hareketi ve görünür bedensel işaretleri içerir. Gizli rakip sağlığı, geleceği ve soy sayacı beyne verilmez. Varlık görünmüyorsa yön/yakınlık girdileri sıfırdır.

Eski A* yardımcı kodu arazi testleri için korunur; **canlıların kararını veya gideceği hedefi artık A* seçmez.** Canlılar ağdan gelen dönüş ve hareketi uygular. Arayüzde gösterilen işaret, hareket niyetidir; stratejik rota değildir.

## 4. Kullanıcının istediği ihtiyaç refleksi

Hazır kaçış politikası kaldırılırken, kullanıcı erken açlık/susuzluk ve temasla tüketme refleksini özellikle istedi. Bu **açık beden kuralıdır**:

- Enerji oranı %60'ın altına düşünce açlık baskısı başlar.
- Su oranı %65'in altına düşünce susuzluk baskısı başlar.
- Her ikisinde %25'in altında sinyal daha hızlı büyür.
- Formül: `clamp((başlangıç-oran)/başlangıç,0,1) + 2*clamp((0.25-oran)/0.25,0,1)`.
- Baskı, kalıtsal ağın girdisidir. Daha yüksek baskının hangi yöne dönüşeceği ağ bağlantılarına bağlıdır; iyi bir arama stratejisi garanti edilmez.
- İhtiyaç varken fiziksel erişimindeki besin/su tüketilebilir. Bu refleks **uzaktaki besini seçmez, canlıyı oraya götürmez, avcıdan kaçırmaz**.
- Ağ, ihtiyaç başlamadan da ağız çıktısıyla etkileşebilir. Dolu rezervden fazlası gizlice biriktirilmez.

Bu istisna açıklanmadan bütün kararların öğrenildiği söylenemez.

## 5. Kalıtım ve mutasyon

Altı beden özelliği korunur: beden, algı, beslenme, su uyumu, atılganlık/renk girdisi ve sosyal bağ girdisi. Son iki değer karar ağına girdidir; eski sabit davranış kurallarını etkinleştirmez.

Yavrunun her beden özelliği ve her ağ ağırlığı, ilgili iki ebeveyn değeri arasından sürekli uniform örneklenir. Böylece motor kararının genetik yapısı gerçekten yavruya geçer. Topoloji sabittir; gizli aktivasyon belleği geçmez.

**Toplam mutasyon olasılığı doğum başına %0,1.** Mutasyon olduğunda 6 beden + 357 beyin değerinden tek konum seçilir. Ağ boyutu arttı diye doğum başına mutasyon olasılığı katlanmaz. Ağ ağırlıklarında çoğunlukla küçük, seyrek büyük sapma uygulanır. Ebeveynler aynı değere sahipken de yeni değer oluşabilir. Beden değerleri 0–1, ağ ağırlıkları -3–3 sınırında kalır.

Bir ağın ağırlıklarını aralıkla birleştirmek ebeveyn becerisini korumak zorunda değildir. Bu kullanıcı tarafından seçilen ilk kalıtım modelidir; davranış etkileri ayrı deneylerde karşılaştırılmalıdır. Başarı puanıyla en iyi bireyi gizlice klonlama yoktur.

## 6. Besin ekolojisi

### Meyve ağacı

- Ağaç artık yalnız dekor/engel değildir: **ana bitkisel besin kaynağıdır**.
- Her ağaçta yedi gerçek meyve konumu vardır. Kırmızı elma işaretleri bu konumların stoklarını gösterir.
- Tam meyve 18 soyut besin enerjisi içerir; bedene geçen miktar sindirim verimine bağlıdır.
- Meyveye yaklaşıp fiziksel erişimden tüketilir. Gövde yolu ve görüşü kesmeye devam eder.
- Tüketilen meyve kaybolur; aynı konum 45–69 simülasyon saniyelik beklemeden sonra yenilenir.
- Kuraklık bekleme sayacının ilerlemesini yavaşlatır. Yeni meyve tüketimle aynı adımda aniden gelmez.
- Kısmen yenmiş meyvenin kalan stoku korunur; tükenmeden yeni tam meyve eklenmez.

### Çayır

- Ot bütün karaya homojen yayılmaz. Suyla ilişkili uygun bölgelerde ayrı, sınırlı çayır lekeleri oluşturulur.
- Çayır dışının ot kapasitesi **sıfırdır**; zaman geçince bütün harita yeniden otla kaplanmaz.
- Ot daha düşük stok ve tüketim verimi/süresiyle ikincil besindir: ağız ot için en fazla 1,8, meyve için 12 ham enerji/saniye alabilir.
- Otun nemle sınırlı yavaş yenilenmesi ve kuraklıkla seyrelmesi vardır.
- Meyve ve ot stokları ortak kaynağa gelen eşzamanlı ağız taleplerinde oransal paylaşılır.

### Et ve ceset

- Ölümün ardından gerçek kalan enerji/dokudan ceset oluşur; ücretsiz sonsuz besin yoktur.
- Et ağızla temas halinde tüketilir, sindirim verimi beslenme özelliğinden gelir.
- Tamamen yenmiş ceset kaldırılır.
- Kalan ceset en geç **90 simülasyon saniyesinde** kaldırılır; son 25 saniyede görünürlüğü azalır.
- Yok edilen artık, ekosistemden çıkan enerji olarak yorumlanır. Yerde kalıcı iz tutulmaz.

## 7. Yaşlanma ve görsel kimlik

- Yaşlılık **beyazlaşma ile gösterilmez**. Kalıtsal rengin doygunluğu giderek azalır; parlaklığı beyaza çekilmez.
- Gençlerde doygun renk, yaşlılarda soluk renk. Renk tek başına etçil/otçul ayrımı değildir.
- Otçul: yuvarlak gövde ve iki yuvarlak kulak/lop.
- Etçil: uzun, köşeli kama başı ve çatallı kuyruk; farklı dış siluet.
- Önceki kalın dış çeper korunur. Yaş rengi iç ve dış gövdede tutarlı uygulanır.
- 350–1.200 saniye aralığında yetişkin azami hızı kademeli olarak başlangıç hızının %40'ına kadar düşer.
- Aynı dönemde ivmelenme ve iyileşme kapasitesi de azalır. Daha önceki algı yaşlanması korunur.
- Yavrunun büyümesi yaşlılıktan ayrı mekaniktir: küçük yavru hızlı yetişkin sayılmaz.

## 8. Hareket ve köşe davranışı

- 20 Hz sabit zaman adımı, 5 Hz dağıtılmış ağ değerlendirmesi, 1 Hz ekoloji.
- Ağ doğrudan dönüş/hareket komutu verir; ömürlü son hedef, hedefe kilitlenme ve sınır dışı kaçış hedefi yoktur.
- İvme, enerji maliyeti, arazi geçiş yeteneği ve gövde yarıçapı korunur.
- Tam hareket segmenti engellerle sınanır. Çarpışma sırasında nüfuz önlenir; fizik canlının yerine yeni bir kaçış yönü seçmez.
- Üç engel sensörü ve gerçekleşmeyen hareket sinyali, duvara basmayı ağ için gözlenebilir yapar.
- `BlockedSeconds` ve kat edilen mesafe rapora yazılır. Kötü ağ politikasını gizli otomatik dönüşle örtmek yoktur.
- Eski sürümdeki sınırda hedefe takılma yolu kaldırılmıştır; bu, evrilen her bireyin duvarı başarıyla aşacağı vaadi değildir.

## 9. Deney ve teknik mimari

C# .NET Standard 2.1 Core ve ayrı yerel SDL2 gözlemci korunur. Unity ana hedefidir; mevcut makinede editör kurulmadığı için bu sürüm Unity build'i olarak sunulmaz. Görsel değişiklikler biyolojik rastgelelik akışlarını tüketmez.

RNG akışları: harita, kurucu beden, kurucu ağ, beden kalıtımı, ağ kalıtımı ve mutasyon ayrı. Aynı sürüm/runtime, seed ve müdahale zamanlarıyla deterministik tekrar kontrol edilir. Hash artık meyve stok/sayaçlarını, ağ ağırlıkları/bellek/çıktılarını ve engelde bekleme durumunu içerir.

Gözlem raporları: bireyler, beden genleri, ağ genleri, motor çıktıları, soy, olaylar, ağaç meyveleri, cesetler ve zaman serisi. Tam dünya kaydı ve kaldığı yerden devam vardır; RNG, kaynaklar, beyin belleği ve dünya ayarları da kaydedilir. Eski testler eski sürümü temsil eder; yeni kayıtlar `artifacts/v0.2/` altındadır.

357 ağırlıklı küçük ağ için şu anda CPU ölçülür. GPU'ya geçiş yalnız toplam deney/karar hızında ölçülmüş yarar verirse yapılır; yüksek GPU kullanım yüzdesi kendi başına hedef değildir. Tam paralel snapshot/commit ve veri odaklı depolama sonraki teknik iş olarak korunur.

## 10. Bu revizyonun kabul kontrolleri

1. Sıfır motor üreten ağ, yakın etçil var diye hareket ettirilmez; otomatik karşı saldırı olmaz.
2. Ağ motor çıktısı hareketi değiştirir; ağırlıklar ömür içinde gizlice değişmez.
3. Gerçek yavrular ağ genleri taşır; mutasyonsuz değerler ebeveyn aralığındadır.
4. Ot alanı sınırlıdır; çayır dışı zamanla ot üretmez.
5. Meyve tüketimi gerçekten enerji sağlar; meyve sayacı tamamlanmadan meyve çıkmaz; kuraklık beklemeyi uzatır.
6. Ceset 90 saniye sınırını aşmaz.
7. İhtiyaç baskısı eşikler altında monoton artar; kritik eşik daha güçlüdür.
8. Aynı bedenin yaşlı kopyası daha yavaştır.
9. Kaynak, sağlık, doğum bütçesi, ölüm kilidi, kimlik/soy ve engel kontrolleri geçer.
10. Sade arayüz korunur; kırmızı elma, farklı siluet ve doygunluk gerçek çizimden incelenir.

## 11. Güncellenmiş yol haritası

- **v0.2:** Kalıtsal karar ağı temel sürüm; meyve/çayır ekolojisi; yeni görsel kimlik; ihtiyaç reflekslerinin açık ayrımı.
- **v0.3:** Zaman çizelgesinde geçmişe sarma/yeniden oynatma, çoklu seed ve kontrol dünyaları; davranışın gerçekten evrimleşip evrimleşmediğinin ölçümü.
- **v0.4:** Daha uzun koşular, ortak kaynak ve üreme arbitrajı, profil ve koşullu GPU/Jobs hızlandırması.
- **v0.5:** Mevsimler, afetler, yapı ve korunak davranışları; her mekanik için enerji/erişim maliyeti.
- **Sonraki araştırma:** Yaşam içinde öğrenme, daha gelişmiş bellek, iletişim ve sosyal bakım; kalıtsal evrimden ayrı deney kolları.
- **Görsel kol:** Unity/URP bağlantısı, hafif 3D ve yürüyüş animasyonları; kamera üstten kalır, okunurluk bozulmaz.

Önceki plandaki ayrıntılı gelecek fikirleri tarihsel arşivde korunur; hangi sırada yapılacaklarını güncel deney sonuçları belirler.


## 14. Yapılandırılabilir dünya ve kayıt — uygulandı

Açılışta kayıtlı dünyaları gösteren ana menü vardır. Yeni dünya oluştur düğmesi parametre özetini ve kurulum formunu açar. Dünyadan çık, kayıt başarılıysa ana menüye döner; ana menüde simülasyon ilerlemez. Simülasyon başladıktan sonra form kapanır, harita ana alanı kaplar. Dokuz sekme; sayısal aralık doğrulama; dört referans profil; bağımsız harita tohumu vardır. `WorldConfig` bu sürümün ayar sözleşmesidir; değerler checkpoint ve durum hash'ine dahildir. Önceki bölümlerdeki rakamlar referans profilin varsayılanlarıdır, kullanıcı değiştirilebilir değerlerdir.

Harita: 128 × 128 sabit alan; 1–5 suyla ayrılmış kara bölgesi, nehir genişliği, 0–350 ağaç, kaya kümeleri, çayır sayısı/yarıçapı ve rastgele tohum. Kullanıcı harita çizmez. Aynı tohum ve ayarlar aynı başlangıcı verir. Kaynak adedi ve şekil kısıtları, kıyı/engel güvenliğiyle uygulanır.

Otçul ve etçil kurucu grupları için altı genin min/max aralıkları ayrı ayarlanır. Ömürleri ve yaşlanma anahtarları da ayrıdır: ömür 0, yaşlılıktan ölümün kapatılmasıdır; yaşlanma 0, yaş etkilerinin kapatılmasıdır. Grup yavruya aktarılır; bunlar evrilen bir ömür geni değildir, deneyin kurallarıdır. Açlık, susuzluk ve yaralanma ölümü devam eder. Yaşlanma bitiş yaşı ömür sınırından bağımsızdır. Ortak beden, metabolizma, kaynak, üreme ve temas katsayıları kendi sekmelerinden değişir.

Kayıt: `.evoworld` dosyası sürüm işareti, ad/tarih/kamera/hız meta verisi, SHA-256 kontrolü ve tam ikili dünya durumundan oluşur. Geçici dosya + atomik değişim; önceki kayıt `.bak`. 30 gerçek saniyede ve normal kapanışta otomatik kayıt; yeni kayıt ayrı bir dünya dalıdır. Açılan kayıt duraklatılır. Sabit adımlı devamın gelecekteki hash eşitliği test edilir. Henüz herhangi bir geçmiş saniyeye sarma yoktur; kullanıcı ayrı kayıtlarına döner.

Hız: 1/2/4/8/12/20×. Fizik dt=.05 korunur, render başına işlem bütçesi arayüzün tepki vermesini sağlar. Ulaşılan hız düşükse ekranda belirtilir. GPU yürütücüsü henüz yoktur; CPU referansı ilerideki GPU sonuçları için doğruluk tabanıdır.
