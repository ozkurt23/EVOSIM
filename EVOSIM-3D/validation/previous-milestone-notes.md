# EVOSIM — Blender + Unity geçiş denemesi

16 Eylül 2026. **Bu paket bitmiş oyun veya Unity'de doğrulanmış sürüm değildir.**

## Hazır olanlar

- Blender 4.5.9 ile üretilmiş, düzenlenebilir gerçek 3D deneme sahnesi: `blender/EVOSIM-validation.blend`.
- Ortografik kamera, ortak ışık/gölge, yükseltilmiş iki kıyı ve nehir; üç uzun ağaç çeşidi, çalı, saz, kaya ve iki canlı modeli.
- `previews/validation-scene.png` gerçek Blender renderidir; canlı simülasyon ekran görüntüsü değildir.
- Sekiz FBX modelinde birleştirilmiş geometri, korunmuş malzemeler ve zemindeki kök noktası. Blender geri aktarım kontrolü geçti; sonuçlar `validation/fbx-roundtrip.json`.
- Unity 6000.3.24f1 / URP 17.3.0 proje iskeleti. `EvoSimBridge` mevcut Core'u sabit adımla ilerletir, canlı konumlarını görsele aktarır. 16×16 arazi parçaları Core haritasından oluşturulur.
- Runtime C# dosyaları gerçek Unity kütüphanelerine karşı .NET Standard 2.1 derlemesinden sıfır hata/uyarı ile geçti. Editor dosyaları ve gerçek Unity çalışması henüz doğrulanmadı.
- Core DLL, kararlı v0.5.9 ile SHA256 düzeyinde aynıdır. Kullanıcı kayıtları taşınmadı veya değiştirilmedi.

## Unity engeli ve açma

Editor başlatıldı, fakat `No valid Unity Editor license found. Please activate your license.` hatasıyla (198) proje derlemesinden önce durdu. Bu makinede kullanıcı hesabına bağlı geçerli Unity lisansı etkinleştirilmeli. Parola veya lisans anahtarı bu dosyaya yazılmamalı.

Etkinleştirme sonrası Unity Hub üzerinden bu klasörün `Unity` alt klasörünü Unity 6000.3.24f1 ile açın. Paket/model aktarımı tamamlandıktan sonra `EVOSIM > Build Core validation scene` menüsü sahneyi oluşturur; sonra Play. Bu adımlar henüz bu ortamda başarılı olarak denenemedi.

## Üretim ve doğrulama

Blender komutu: `blender --background --factory-startup --python blender/build_scene.py -- /absolute/path/to/EVOSIM-3D`. Kaynak düzenlenebilir modelleri korur; FBX için kopyaları birleştirir. Büyük dünyaya geçmeden LOD seviyeleri ve ekran ölçeğinde okunabilirlik ayarlanmalı.

## Sıradaki işler / kabul koşulları

1. **Sanat:** kıyıların düz duvar görünümünü kır; çimen dudağı/toprak katmanı/su temas gölgesi ekle. Suyu derinlik, akış ve kıyı hareketiyle geliştir. Yaprak ölçeğini küçült ve daha organik taç kütleleri oluştur. Bu ilk render, Terra Nil referansının nihai kalite düzeyine ulaştı sayılmaz.
2. **Kaynak doğruluğu:** Core meyve slotlarını ağaçlarda kırmızı elmalar olarak göster; tüketim/yenilenme görselde aynı anda değişsin. Ot, taş ve ağaç türleri kolay ayırt edilsin.
3. **Canlı okunabilirliği:** renk olmadan da otçul/etçil siluetleri ayrışsın; saldırganlık rengi ve yaş doygunluğu ayrı okunabilsin. Örtülen canlılar için ağaç saydamlığı/iz görünümü uygula, seçim ve zoom ile doğrula.
4. **Core eşitliği:** aynı seed ve adım sayısıyla bağımsız Core ve Unity durumu hash eşitliği; pause/hız/farklı kare hızlarında aynı sonuç. 20×, donanım yetiştiği kadar; mevcut 18 ms işlem bütçesi performans garantisi değildir.
5. **Kayıt:** mevcut dünyayı yükleme, isimlendirme, silme, tarihçe/geri sarma ve dallanmayı kopya kayıtlarla doğrula. Geçmişte gelecek verisi sızmasın. Eski sürüm dosyalarına göç doğrulanmadan yazma.
6. **Arayüz:** takvim; doğum/ölümde otçul/etçil etiketleri; dört konumda sıralı panel yığınları; grafiği canlı seçince açık tutma. Dünya öncesi kapsamlı ayarlar ve açıklamalar, simülasyonda az kontrol.
7. **Performans:** LOD, instancing uyumluluğu, görünmeyen alanları çizmeme, büyük haritada CPU/GPU kare süreleri ve bellek ölçümü. Şu an LOD, GPU Resident Drawer ve animasyon sistemi uygulanmış değildir.
8. **Biyoloji:** hazır kaçma/avlanma kuralları ekleme. Mevcut kalıtsal ağ korunur; bireyin yaşamı boyunca ağırlık öğrenmesi yoktur. Ekolojik sürdürülebilirlik ve strateji evrimi ayrıca deneyle ölçülmeli.

Ana başlatıcı halen v0.5.9'dur. SDL v0.5.10 adayındaki takvim/panel/olay günlüğü çalışmaları kaynakta korunmaktadır; yeni görsel deney sonrasında tam yayın doğrulaması yapılmamıştır.

## Kıyı düzeltmesi

Nehir yanındaki kara köşeleri suyun 0.08 sahne birimi üstüne indirildi; iç araziye eğimle bağlandı. Kayaların tam taban alanı su hücrelerinden uzaklaştırıldı. Bu değişiklik Blender deneme sahnesindedir; Unity arazi adaptörüne henüz taşınmadı.

## Lisans ve Editor doğrulaması — güncelleme

Unity Personal lisansı etkinleşti. Gerçek Unity Editor proje aktarımı ve derlemesi başarıyla tamamlandı (çıkış 0); CoreValidation.unity sahnesi oluşturuldu. Önceki lisans engeli kalktı. Play modu, görsel doğrulama ve deterministik Core eşitliği henüz test edilmedi.
