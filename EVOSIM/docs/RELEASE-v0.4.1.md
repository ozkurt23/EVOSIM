# EVOSIM v0.4.1 — Daha büyük dünyalar

15 Eylül 2026. Yerel Linux x64, C# Core + SDL2 gözlemci. v0.4.0 üzerine büyük harita ve kaynak araması dilimi.

## Kullanılabilir yenilikler

- **Dünya → Harita boyutu:** 128 × 128, 192 × 192 veya 256 × 256. Düğme bu üç seçenek arasında döner. 256 haritanın alanı 128'in dört katıdır.
- Her dünyanın boyutu bağımsızdır. Aynı süreçte farklı büyüklükte dünyalar oluşturulabilir, açılabilir ve karşılaştırılabilir. Değerler bir global değişken üzerinden paylaşılmaz.
- Nüfus, ağaç, kaya ve çayır adetleri kullanıcının yazdığı sayılardır. Alan büyüyünce kendiliğinden çoğalmazlar. Referans profil seçmek mevcut harita boyutunu korur.
- Kamera gerçek dünya sınırlarına bağlanır. **Tüm harita** gerçek merkeze döner; maksimum yakınlaşma 128/192/256 için 5/7.5/10 olur, böylece büyüyen haritada aynı canlı ayrıntısına erişilir. Ana menüde boyut yazılır; 64² önizleme bütün dünyadan örneklenir.
- Büyük haritanın genel görünümünde kırmızı meyve işaretleri küçük taçları bütünüyle kaplamayacak ölçüde küçülür; yakın görünüm ayrıntısı ve gerçek stoklar korunur. Canlıların asgari görünür silueti fiziksel bedenlerini büyütmez.

## Harita üreticisi ve eski dünyalar

Yeni kurulumlar **üretici sürümü 2** kullanır. Eski 128 üreticisinde özellikle dar/geniş üç kollu nehirlerin birleşiminde küçük kapalı kara artıkları veya ince su boşlukları oluşabiliyordu. Yeni üretici, nehir merkez çizgisini sürekli kaplar ve birleşim içindeki pikselleşme artıklarını suya birleştirir; bunlar kaynaklar ve canlılar oluşturulmadan önce uygulanır. Bu düzeltme rastgele sayı tüketmez.

Yeni üreticide 128/192/256 × 5 tohum × 1–5 kara bölgesi × 0.4/1/2 nehir genişliği = **225 kombinasyon** beklenen bölge sayısını verdi. Eski kaydın arazisini değiştirmemek için **üretici sürümü 1**, yalnız eski 128 dünyalarının yüklenmesi/aynı koşulla yeniden başlaması için korunur. 50 eski terrain özeti, kuraklık altında 100 ekoloji adımı dahil, dondurulmuş v0.4 ile birebir eşleşir.

**Yeni dünya kur**, eski bir dünya açık olsa bile düzeltilmiş sürüm 2'yi kullanır. **Aynı vadiyi başlat**, o vadinin tohumunu ve bütün kurallarını, eskiyse üretici sürümü 1'i de korur. Kaydı açmak araziyi yeniden tasarlamaz.

Büyük haritalarda nehir/lake/sığınak yerleşim noktaları ve geniş biçim dalgaları alanla ölçeklenir. Bedenler, hareket hızı, görüş, kaynak enerjisi, nem etki mesafesi ve ayarlanmış fiziksel nehir genişliği aynı birimlerde kalır.

## Çekirdek ve performans

- Beslenme talepleri **ot / meyve / ceset** türü, nesne indeksi ve meyve konumu ile temsil edilir. Önceki 16384 tabanlı ceset numaraları, büyük haritanın ot hücreleriyle çakışmaz.
- Canlı komşuluk ızgarası, sekiz dünya birimlik hücrelerle gerçek boyuta göre kurulur.
- Meyve noktaları da yerel ızgarada aranır. Canlının yakın çevresi taranır; bütün ağaçlarda tekrar tekrar trigonometrik konum hesaplanmaz. Eşit uzaklıkta seçim sonucu değişmesin diye eski ağaç/konum sırası korunur.
- Ağaç geometrisi değişirse indeks yenilenir; yükleme sonunda indeks gerçek yüklenmiş ağaç nesnelerini kullanır. Tükenmiş meyve yerleri indeksten silinmez; yenilenince aynı yerde yeniden bulunur.
- Yoğun başlangıçta 100 rastgele konum denemesi biterse üst üste kurucu kabul edilmez. Boş hücre aranır; yer yoksa açıklanmış kurulum hatası döner.
- Kayıt yüklerken sonradan silinecek geçici kurucular üretilmez. Kayıttaki canlılar ve RNG durumları doğrudan geri konur.
- Hazır kaçış, avlanma veya eşe gitme politikası eklenmedi. Beyin ve gen şeması, fizik adımı, ihtiyaç refleksi ve mutasyon olasılığı bu performans çalışmasında değişmedi.

Kısa CPU karşılaştırması ve ham ölçümler `artifacts/v0.4.1/cpu-benchmark/` içindedir. Aynı 128 üretici sürümü 1 koşulu eski/yeni DLL ile karşılaştırılır; yeni büyük haritalar sürüm 2 kullanır. Bu ölçümler paylaşılan makinede, çizim kapalı yapılır. GPU/FPS veya her koşulda 20× gözlem hızı ölçümü değildir.

## Kayıt biçimi

Yeni Core biçimi **EVOSIM-0.4-WORLD-2**, harita boyutunu ve üretici sürümünü açıkça saklar. v0.4 WORLD-1 ve v0.2/v0.3 WORLD-2 biçimleri desteklenir; eksik boyut/üretici bilgileri 128/1 ile tamamlanır. Yeni sürümde eski kaydı açmak ayrı `· v0.4.1` devam dünyası oluşturur. Eski dosya korunur.

Config ve kayıt başlığındaki boyut eşleşmelidir; desteklenmeyen boyut/üretici veya sürüm 1 ile büyük harita reddedilir. Gerçek zaman çizelgesi bu dilimin parçası değildir; sıradaki pakettir.

## Doğrulama kanıtları

- **23/23 Core grubu:** kaynak çakışması, büyük koordinatlarda komşular, üç boyutun bağımsız ilerlemesi, büyük harita kayıtlarının geleceği ve önceki mekanikler.
- `grass[16384]`, `corpse[0]`, `fruit[0]` aynı gerçek fizik adımında doğru kaynaklardan bağımsız tüketildi.
- 98 canlı ve 84 meyve sorgusu, tüm nesneleri tarayan referansla aynı adayları ve sırayı verdi.
- 192/256 kayıtları yüklenip 200 adım ilerletildiğinde asıllarıyla aynı hash'e ulaştı.
- Eski iki kayıt biçimi ve yeni boyut/üretici doğrulaması için ayrı checkpoint testleri; tam kayıt baytlarıyla gelecek eşitliği ve bozuk veri reddi.
- Yayın UI smoke: üç boyutta kurulum, güneydoğu canlı seçimi, tekerlek yakınlaşma merkezi, kamera kaydı, önizleme örneklemesi, büyükten küçüğe geçiş. Önceki menü/sil/geri al/ayar/tooltip ve eski kayıt dalı akışları da korunur.

Önceki üretici geliştirme revizyonlarının kanıtları ayrı klasörlerde tutulur; nihai manifestteki Core hash'i bu sürümü tanımlar.

## Açık işler

384/512 haritalar henüz menüde yok. Zemin hâlâ tek 1024² görsel doku kullanır; ayrıntılı parça çizimi ileriki ölçekleme işidir. Ölü bireylerin soy/beyin verisi, nüfus geçmişi ve senkron kayıtların uzun koşuda büyümesi ayrıca ele alınacak. Simülasyon CPU'dadır. Gerçek geçmiş/geri sarma, mevsimler, daha kapsamlı ağaç yaşamı, afetler ve inşa sonraki dilimlerdir.
