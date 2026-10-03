# EVOSIM 3D — çalıştırılabilir Unity önizlemesi

3 Ekim 2026. Blender modelleri ve mevcut C# biyoloji çekirdeği artık ayrı bir Unity uygulamasında birlikte çalışıyor. Bu bir geçiş önizlemesidir; tam oyun göçü tamamlanmış değildir.

## Çalıştırma

Bu klasördeki `baslat.sh` dosyasını çalıştırın. Unity Editor gerekmeden `Build/EVOSIM-3D.x86_64` açılır. Her açılış seed 42 ile yeni bir deneme dünyası oluşturur: 35 otçul, 5 etçil, 80 ağaç. Önizleme mevcut kayıtları açmaz/yazmaz. Eski kararlı sürümün başlatıcısı `../EVOSIM/baslat.sh` olarak korunur.

- Fare tekerleği: yakınlaş/uzaklaş.
- Sağ veya orta tuşla sürükle: haritada gezin.
- Canlıya sol tık: tek canlı gözlem kartı; Escape: kartı kapat.
- Boşluk veya üstteki duraklatma simgesi: duraklat/devam et.
- Hız düğmesi: 1×, 2×, 5×, 10×, 20×.
- Sol üstte gün/yıl/saat, sağında otçul/etçil sayıları yer alır.

## Bu dilimde uygulananlar

- Unity Editor derlemesi, Play modunda Core başlangıcı ve bağımsız Linux uygulaması.
- 16×16 parçalar halinde gerçek Core haritası; alçalan kıyılar. Su/karanın kıyı farkı 0.08 görsel birim; iç kara 0.65. Canlılar ve ağaçlar çizilen üçgenlerle aynı yükseklik hesabını kullanır.
- FBX model kök dönüşümü korunur; canlı/ ağaç ölçeği ayrı üst nesneye uygulanır. İçe aktarımın santimetre/metre dönüşümü artık yanlışlıkla silinmez.
- Gerçek Core ağaçları ve kaya engelleri. Kaya modeli kara hücresinin içinde kalır; ağaç engel hücresine ayrıca kaya konmaz.
- 560 meyve görünümü gerçek Core meyve yuvalarına bağlıdır; besin biterken görünmez, yeniden oluşunca görünür. Ot tutamları örneklenen gerçek besin hücrelerini gösterir; tüm besin hücreleri ayrı modelle çizilmez.
- Canlı silueti beslenme genine göre seçilir. Renk atılganlık geninden, doygunluk yaş/ömür oranından gelir. Atılganlık doğrudan saldırganlıkla eş tutulmaz.
- Ekranda canlı ile örtüşen ağaç yaprakları kademeli saydamlaşır; gövde ve meyveler kalır. Bu ilk sürüm ekran sınırlarıyla yaklaşık örtüşme hesabı yapar; kesin piksel örtüşmesi değildir.
- Minimal gözlem arayüzü, yakınlaştırma, gezinme, 20× hız seçimi; gün/yıl gösterimi. Eski dört konumlu panel sistemi henüz Unity'ye taşınmadı.

## Doğrulama ve sınırlar

`validation/clock-and-shore.txt`: 30/60/144 FPS zaman girdileri ve 1×/20× hız ile 400 adım sonunda bağımsız Core hash eşitliği, duraklatma, işlem bütçesinden kalan adımların korunması ve 233 kıyı köşesi kontrolü geçti. Bunlar FPS performans ölçümü değildir.

`previews/unity-player.png` gerçek bağımsız uygulama ekran görüntüsüdür. Yanındaki JSON, aynı tick sayısında bağımsız Core eşitliği, canlı görünüm sayısı, gerçek model ölçeği ve zemine yerleşimi doğrular. Test dünyası üretim kayıtlarını kullanmaz.

Son test makinesi Unity tarafından **Mesa Intel Graphics (ARL)** olarak tanındı. RTX 5090 üzerinde performans doğrulanmadı. 20× seçilebilir; donanım yetişmezse zaman borcu korunur ve hedef hıza ulaşılamaz. GPU eğitimi eklenmedi; biyoloji CPU'daki mevcut Core'da çalışır.

Editör batch renderinde karanlık çıktı görüldü; teslim görseli bağımsız uygulamanın gerçek karesinden alındı. Editörün SearchDatabase başlangıç istisnası bir denemede görüldü. Başarılı oyuncu uygulaması testinde çalışma zamanı istisnası görülmedi. Editör dışı test raporları görsel kabul için esas alındı.

## Açık işler

1. Terra Nil referansına yaklaşacak kıyı, organik bitki kütlesi, su hareketi ve ışık iyileştirmeleri. Mevcut arazi hâlâ belirgin hücrelidir; görsel çalışma bitmiş değildir.
2. Model LOD, yaprakların daha ucuz çizimi, ot ve meyve sayılarının ölçeğe göre yönetimi. 128/192/256 haritalarda gerçek CPU/GPU kare süreleri ölçülmeli.
3. Yürüme/yeme/içme animasyonları ve ceset görselleri. Core'da ceset/yaşlanma vardır; Unity'de ölen canlı modeli şu anda kaldırılır.
4. Ana menü, kapsamlı dünya ayarları, kayıt açma/isimlendirme/silme ve zaman çizelgesi göçü. Kopya kayıtlarla eşitlik testinden önce gerçek kitaplığa yazılmamalı.
5. Dört konumlu çoklu panel yığınları, nüfus grafikleri, otçul/etçil olay günlüğü ve soy gözlemi. SDL adayındaki çalışmalar korunuyor.
6. Uzun koşu ekoloji ve strateji deneyleri; kalıtsal karar ağına hazır kaçma/avlanma davranışı eklenmeyecek. Yaşam içi ağırlık öğrenmesi uygulanmış değildir.

## Kaynaklar

- `Unity/`: Unity 6000.3.24f1, URP 17.3.0 projesi.
- `blender/EVOSIM-validation.blend`: düzenlenebilir sanat deneme sahnesi.
- `blender/build_scene.py`: modelleri ve FBX dosyalarını yeniden üretir.
- `design-direction.md`: kullanıcının Blender + Unity üretim yönü.
- `Unity/Assets/Editor/EvoSimValidation.cs`: saat/kıyı ve Editor Play testleri.
- `Unity/Assets/Scripts/LiveCapture.cs`: yalnız `--capture /absolute/path.png` ile çalışan izole önizleme kontrolü.
- `EvoSimSceneBuilder.BuildPlayer`: Linux uygulamasını yeniden derleyen Editor metodu.

## 3 Ekim — daha yumuşak kıyılar

Kıyı hücreleri kenar başına 4 alt parçayla çiziliyor; kara/su sınırı üçgenler üzerinde kesilerek büyük kare köşeleri dar (0.28 hücre) bir bantta yuvarlatılıyor. İç bölgeler kaba parçalarda kalıyor. Aynı ince yüzey enterpolasyonu canlıların/bitkilerin yüksekliğinde kullanılıyor. Nehir güzergâhı, hücre merkezlerindeki kara/su sınıflaması ve biyoloji değişmedi. Seyrek küçük kıyı taşları görsel dekor olarak eklendi; bunlar Core'daki engel kayalarından ayrıdır.

Geometri testi 50.293 üçgende geçti; yüzey yüksekliği hesabının en büyük hatası 0.0000241 görsel birimden küçük. Bütün hücre merkezleri ve Core hash'i korundu. Yeni gerçek uygulama görüntüsü: `previews/unity-soft-shore.png`.
