# EVOSIM — İlk gözlem

**Sürüm: 0.1.0-observer.1 · Linux x64 · 15 Eylül 2026**

Küçük bir nehir vadisinde canlıları izleyebileceğimiz ilk çalışan prototip. Dünya duraklatılabilir, hızlandırılabilir ve yakınlaştırılabilir. Bu paket uzun plandaki v0.1'in tamamı değildir; ilk gözlem ve davranış denemesi içindir.

## Açılış

Bu klasördeki **`baslat.sh`** dosyasını çalıştır. Hazır uygulama `release/EVOSIM` içindedir; bu bilgisayarda Unity veya .NET kurulumu gerektirmez. Dosya yöneticisi betiği metin olarak açarsa terminalde bu klasörden:

```bash
./baslat.sh
```

Pencereyi kapatmak uygulamayı sonlandırır. Gözlem raporları `observations/` klasörüne yazılır. Raporlar devam ettirilebilir dünya kayıtları değildir.

## Ekran ve kontroller

Harita pencerenin tamamını kullanır. Sürekli açık yan panel yoktur. Üstte yalnızca ad, nüfus ve simülasyon zamanı; altta üç küçük kontrol bulunur. Nüfus grafiği, kuraklık ve rapor araçları **···** menüsündedir. Bir canlıya tıklamak küçük bilgi kartını açar; **Ayrıntı** davranış gerekçesini ve ebeveynleri gösterir.

| İşlem | Kontrol |
|---|---|
| Canlı seç / seçimi kaldır | Sol tık / boş alana tık veya Esc |
| Yakınlaş / uzaklaş | Fare tekerleği |
| Haritayı sürükle | Sağ fare tuşunu basılı tut |
| Duraklat / devam | Boşluk veya Ⅱ / ▶ |
| Hız | Alttaki hız düğmesi; 1 → 2 → 4 → 8 → 1 |
| Tek simülasyon adımı | Sağ ok; otomatik duraklatır |
| Doğrudan hız seç | 1, 2, 4, 8 |
| Seçili canlıyı takip | F veya karttaki Takip |
| Gerçek hedef ve kalan rota | I veya ··· menüsü |
| Tüm haritayı çerçevele | H veya ··· menüsü |
| Kuraklık başlat / bitir | D veya ··· menüsü |
| JSON + CSV raporu | S veya ··· menüsü |
| Aynı tohumla baştan başlat | R; dört saniye içinde tekrar bas |
| Yeni tohumla vadi oluştur | N; dört saniye içinde tekrar bas |

Başlangıçta harita biraz yakındır. Kenarları görmek için uzaklaşabilir veya H'ye basabilirsin. Küçük gövdeler yakından daha iyi okunur.

## Dünyayı okumak

- **Besin:** Canlı yeşil yapraklı küçük bitkiler. Görünüşleri mevcut bitki stokuna bağlıdır; tüketilen alanlar seyrelir.
- **Ağaç:** Gövdeli, büyük ve loblu yeşil taç. Suya yakın nemli alanlarda daha sık yerleşir. Gövde yolu ve görüşü engeller.
- **Taş:** Köşeli gri kaya. Geçişi ve görüşü keser; dolanmak hareket ve enerji maliyetini değiştirir. Henüz toplanıp yapıda kullanılmaz.
- **Su:** Mavi-yeşil nehir ve gölet. Çoğu canlı geçemez; su uyumu yüksek nadir bireyler geçebilir. İçmek için erişilebilir kıyı gerekir.
- **Korunak:** Üç hazır korunak alanı. Yakınında dinlenmek taban enerji giderini azaltır; her taş korunak değildir.
- **Canlı:** Belirgin renkli dış çeper ve yarı saydam iç gövde. Mavi → mor → kırmızı atılganlık eğilimini; beyazlaşan iç kısım yaşı gösterir. Renk o andaki öfkenin ölçüsü değildir. Etçil gövdeler daha uzundur.
- **Yavru:** Daha küçük, henüz yetişkin kapasitesine ulaşmamış beden. Büyük yetişkinler genel olarak güçlü, küçük yetişkinler daha çeviktir; yavrular bu kuralın istisnasıdır.

Çeper okunurluk için çizimde biraz büyütülür; fiziksel hareket hesabı gerçek gövde yarıçapını kullanır. Uzak görünümde bitki kümeleri seyrek çizilir, yakınlaşınca daha ayrıntılı görünür. Çizim kalitesi biyolojiyi değiştirmez.

## Çalışan mekanikler

- 128 × 128 vadi; 150 otçul ve 15 etçil kurucu.
- Suya bağlı nem, bitki üretimi ve stok tüketimi.
- Sınırlı, yönlü algı; kaya/gövde arkasının görülmemesi; statik arazide A* rota arama.
- Sürekli konum, ivmelenme, dönüş, yavaşlayarak varış ve engel köşesini kesmeyen hareket.
- Enerji, su, sağlık, beden büyümesi, yaşlanma ve ölüm; ölümden sonra leş kaynağı.
- Otlanma, içme, avlanma, kaçma, leşten beslenme, dinlenme, keşif ve eş arama.
- İki uygun ebeveynin enerji aktarımıyla tek yavru; altı kalıtsal değer ebeveyn aralığından rastgele alınır.
- **Doğum başına %0,1 mutasyon**: tek özellik değişir; çoğunlukla küçük, seyrek büyük sapma.
- Kimlik, ebeveynler, nesil; tekil yavru, torun ve yaşayan altsoy sayımı.
- Kuraklık, gerçek bitki yenilenmesini, nemi, içme verimini ve su kaybını etkiler.
- Sabit 20 Hz simülasyon; normal kararlar 5 Hz, ekoloji 1 Hz.
- Aynı sürüm/runtime altında aynı seed ile tekrar üretilebilen CPU çekirdeği.

Mutasyon her koşuda hemen görülmeyebilir. Düşük olasılık kasıtlıdır. Model nüfusu kurtarmak için gizlice yeni canlı veya besin eklemez. Popülasyonun azalması veya yok olması mümkün bir sonuçtur.

## Bu ilk paketin sınırları

- **Unity henüz kurulu değil.** Şimdiki gözlemci yerel SDL2 penceresidir. Kalıcı hedef Unity/C# olarak korunmuştur; çekirdek `UnityEngine` bağımlılığı olmadan .NET Standard 2.1'e derlenir.
- Henüz tam SoA, paralel snapshot/commit ve GPU biyoloji sistemi yok. Tek iş parçacıklı, kararlı kimlik sıralı CPU referansı çalışır; görselleştirme ayrı katmandadır. Gelecek performans işi bu referansla karşılaştırılır.
- Gözlemci SDL2 hızlandırılmış çizimi ister; cihaz bunu sağlayamazsa yazılımsal çizime döner. GPU eğitimi veya öğrenen sinir ağı bulunmaz.
- İlk acil kararlar avcıya yakın temas ve kritik susuzluk gibi açık kurallardır; plandaki bütün risk-zaman modeli ve sosyal bakım henüz uygulanmadı.
- Hedef algıyla sınırlıdır; rota servisi statik haritanın tamamını bilir. Uzun süreli bireysel hafıza ve öğrenme yoktur.
- Hareketli bireyler yumuşak biçimde birbirinden uzaklaşır; katı cisimler gibi kusursuz çarpışma çözümü uygulanmaz.
- Ayrı kurucu üreme grupları korunur. Genetik beslenme eğilimi değişse bile grup etiketi otomatik değişmez. Erkek/dişi rolleri ve gebelik ilk modelde yoktur.
- Kuraklık nehir geometrisini değiştirmez. Hazır korunaklar vardır; aktif korunak arama ve yuva inşası sonraki iştir.
- Gözlem dışa aktarımı vardır; dünyayı kapatıp kaldığı yerden devam ettirme henüz yoktur. Olay günlüğü son yaklaşık 1.500–2.000 olayı tutar; tüm bireylerin soy kayıtları koşu boyunca korunur.
- Geniş nüfuslarda performans ve ekolojik denge hâlâ kalibrasyon konusudur. Hız düğmesi hedef hızı belirtir; yetişemeyen makine simülasyon adımını büyütmez.
- Bu yapay yaşam modeli, insan kalıtımının veya gerçek ekosistemlerin birebir bilimsel modeli değildir.

## Açık başlangıç profili

Kurucular 120–360 simülasyon saniyesi yaşında, yetişkin dokusuyla, %84 enerji ve %85–100 suyla başlar. İlk üreme beklemesi 8–43 saniyedir; başarılı doğumdan sonra 60 saniye beklenir. Su uyumu yüksek kurucu olasılığı %2'dir. Başlangıç rezervleri kurucu üretimi sırasında açıkça verilir; koşu içinde gizli takviye yapılmaz.

Yavrular 120 saniyelik yaş olgunluğu ve yeterli beslenmeyle yetişir. Üreme yaşa ek olarak beden gelişimi, enerji, su, sağlık, uyumlu eş ve doğum alanı ister. Üst yaş 1.200 saniyedir. Bu değerler ilk gözlem profili içindir; ana planın daha önceki örnek sayılarını bu paketteki gerçek uygulamadan ayırır.

## Kaynak kod ve geliştirme

```text
src/EvoSim.Core/       Arazi, PCG, kalıtım, yaşam ve kararlar
src/EvoSim.Observer/   Yerel pencere, dünya çizimi, kontroller, rapor
src/EvoSim.Checks/     Bağımsız otomatik kontroller ve headless ölçüm
release/              Hazır Linux uygulaması + .NET çalışma zamanı
artifacts/            Gerçek test sonuçları ve uygulama görüntüleri
docs/                 Ana plan, uygulama durumu ve lisanslar
observations/         Kullanıcının dışa aktardığı raporlar
```

Geliştirme için .NET SDK 10.0.401 kullanıldı. `global.json` bu sürümü sabitler. Core ayrıca .NET Standard 2.1'e hedeflenmiştir.

```bash
dotnet build src/EvoSim.Observer -c Release
dotnet run --project src/EvoSim.Checks -c Release -- --selftest
dotnet run --project src/EvoSim.Checks -c Release -- --ticks 12000 --seed 42
```

`artifacts/selftest.json` ve benchmark dosyalarında kaynak sürümünü ayırt etmek için gerçek Core derlemesinin SHA256 özeti, zaman ve çalıştırma komutu bulunur. Headless süreler ekran çizimini içermez; GPU veya tüm arayüz FPS ölçümü diye yorumlanmamalıdır.

## Varlıkların kaynağı

Ağaç, besin, kaya, canlı, su ve arayüz şekilleri bu proje için kodla çizilen özgün geometridir. Çizim tanımları `WorldView.cs` içinde tutulur; başka bir oyundan asset kopyalanmamıştır. Şu anda raster sprite veya yapay zekâyla üretilmiş görsel paketi yoktur. Yazı tipi DejaVu Sans; çizim SDL2/SDL_ttf üzerinden yapılır. Dağıtım bildirimleri `docs/licenses/` içindedir.

## Teknik kaynaklar

- [Microsoft: .NET kurulum betiği](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-install-script)
- [SDL2 API belgeleri](https://wiki.libsdl.org/SDL2/FrontPage)
- [SDL_ttf API belgeleri](https://wiki.libsdl.org/SDL2_ttf/FrontPage)
- [PCG referans üretici](https://www.pcg-random.org/using-pcg-c-basic.html)
- [Unity 6 destek hattı](https://unity.com/releases/unity-6/support)
