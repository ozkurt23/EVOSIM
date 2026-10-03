# EVOSIM v0.4.0 — Üreme ve ağaç koşulları

15 Eylül 2026. Yerel Linux x64, C# Core + SDL2 gözlemci. Bu sürüm v0.4 yol haritasının ilk dilimidir; büyük harita hâlâ açık iştir.

## Kullanılabilir yenilikler

- Otçul/etçil kurucu soy grubu için bağımsız Kapalı / Eşeyli / Eşeysiz üreme. Eşeysizde ParentA gerçek ebeveyn, ParentB=0; nesil bir artar, yavru/torun/altsoy doğru sayılır. Ayrıntı kartında tek ebeveyn adı gösterilir.
- Grup bazında 0–1 üreme isteği. Hazır olma girdisi, üreme açık ve bekleme bitmişse `2 × istek`, aksi halde sıfır. Varsayılan 0.5 eski girdiyi birebir korur. İstek dünya ayarıdır, kalıtsal yedinci gen değildir. Sıfır istek doğumu yasaklamaz; ağ diğer girdiler/ağırlıklarla sinyal verebilir. Üreme biçimi Kapalı doğumu gerçekten engeller.
- Eşeylide 6 beden geni + 357 ağ ağırlığı iki ebeveyn aralığından örneklenir. Eşeysizde bağımsız dizi kopyaları alınır. Her ikisinde toplam doğum mutasyonu varsayılan %0.1; tek beden/ağ değeri değişir. Beyin belleği kopyalanmaz.
- Yavru maliyeti `(başlangıç doku + enerji) / verim`. Eşeyli ebeveynler yarı yarıya, eşeysiz ebeveyn tamamını öder. Son ödeme karşılanmıyorsa doğum ve enerji transferi commit edilmez. Yavru yerleşimi sınanır; yeni yavru aynı tick'in aday listesine eklenmez.
- Ağaç taç yarıçapının alt/üst sınırı, minimum merkez aralığı, neme bağlı yerleşim katsayısı, meyve uzaklığının taç oranı, kuraklığa karşı meyve sayacı direnci. Gerçek konum/stok/süre etkileri var. Gövde çarpışma hücresi değişmedi. Yerleşim katsayısı 0 ağaç oluşturmaz; dar alan/geniş aralık istenen ağaç adedinin altına düşürebilir.
- Üreme temasına beden yarıçapları dışında eklenen mesafe ayarı. Mevcut 2 birim yakınlık ve görüş hattı sınırı ayrıca korunur.
- Nüfus panelinde üreme engellerinin anlık görünümü. Her canlı ilk engelinde bir kez sayılır: kapalı, olgun değil, bekleme, enerji, su, sağlık, diğer uygunluk, ağ sinyali yok, hazır. JSON raporda çakışabilen ham koşullar ve tekil yakın eş çiftleri de var. Görüntü açıkken yaklaşık bir simülasyon saniyesinde güncellenir. Görüntü geleceği/RNG'yi değiştirmez.
- 84 sayısal dünya ayarı + 6 gen türüne yardım. Yeni üreme modu kontrolü adını gösterir, tıklama/Enter ile değişir. Yeni 0–1 ayarları sürgü, diğerleri sayısal.

## Kayıt geçişi

Yeni Core biçimi `EVOSIM-0.4-WORLD-1`: açık alan adları/türleri, bilinen şemalar, 6 adlandırılmış PCG akışı (state+increment), ağaç X/Y/Radius/FruitRadius ile stok/timer dizileri, canlılar, cesetler, olay ve nüfus geçmişi. İç SHA-256, boyut ve geçerlilik denetimleri var. Henüz herhangi bir saniyeye geri sarılan film değildir.

`EVOSIM-0.2-WORLD-2` eski payload'ı desteklenir. Yeni 11 ayar eski davranışı koruyan belgeli varsayılanlarla eklenir. Mevcut altı gen ve 30→8→5 beyin şeması değişmez. Önceki dünyayı UI'den açmak orijinal dosyanın üzerine yazmaz: ayrı ID ile `· v0.4` devam dünyası yaratır; kaynak kimliği ve formatı metadata'da tutulur. Bu yeni dünyanın otomatik kayıtları yeni ID'ye gider. Eski orijinali tekrar açmak ayrı bir devam kopyası yaratabilir.

Yeni sürümün kayıtları eski uygulamanın okuyacağı iddiası yoktur. v0.3 uygulaması ve paket arşivi korunur. Geçerli bir yeni kayıt tekrar açıldığında gereksiz yeni dal oluşturulmaz.

## Doğrulama

- Derleme/publish: 0 hata, 0 uyarı.
- `artifacts/v0.4/core-checks.json`: 18/18 çekirdek grubu geçti. Ağ yetkisi, eşeyli/eşeysiz tam ödeme, tekil soy, nadir mutasyon, tanının salt okunur geleceği, yeni ağaç ayarları ve eski bütünlük kontrolleri.
- Eski sürüm DLL'sinden üretilen iki checkpoint senaryosu yeni sürümde 400 adım ilerletildi: yalnız özet değil, tam yeni kayıt baytları eski sürümün devam fixture'ı ile eşit. Aile fixture'ı 24 doğum ve 4 mutasyon içerir.
- Yeni ayarlar ve elle farklılaştırılmış ağaç geometri verisi kaydet/yükle ve 400 adım devamında eşit. Bilinmeyen alan/tür, mükerrer alan, bozuk checksum, kesik/aşırı kayıt ve ek veri reddedilir. Ayrıntılar checkpoint-compatibility kanıt klasöründe.
- Beş seed × ilk durum / 100 kuraklık ekoloji adımı = 10 tam terrain digest'i, dondurulmuş v0.3 ile birebir. Yeni varsayılanlar eski ağacı veya RNG sırasını değiştirmedi.
- Yayınlanan uygulama UI smoke: kaydet/aç/özel ayar/isim/ana menü/sil/geri al/slider/tooltip/scroll ve eski fixture'dan ayrı dal. Eski dosya baytları yeni dalın otomatik kaydından sonra değişmedi.
- Gerçek uygulama görüntüleri aynı sürümün capture yolundan üretildi; model kamera/çizim için adımlanmadı.

## Açık işler / sınırlar

Harita 128², CPU simülasyonu ve SDL2 sunum devam ediyor. Büyük harita, parça çizimi, kaynak kimliği ve komşuluk indeksinin genişlemesi sıradaki dilim. Henüz GPU eğitim, Unity sunum, gerçek replay, ağaç yaşam döngüsü/türleşme, mevsimler, sel, yangın veya inşa yok. Güçlü üreme isteği veya eşeysiz seçenek ekosistemin dengede kalmasını ve zeki davranışın evrimleşmesini garanti etmez; bu sonuçlar ölçülecek.
