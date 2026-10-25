> Güncellik notu (25 Ekim): Bu dosyanın v14 bölümü tarihsel test kaydıdır. Yaşayan orman, kayıtlar, çoklu paneller ve yüzme hareketleri daha sonra eklendi. Güncel kapsam için `docs/RELEASE_ROADMAP.md` ve `IMPLEMENTATION_STATUS.md` son bölümlerini kullanın.

# Bütünsel görünüm ve durum matrisi — v14

Bu matris, sonsuz olası simülasyon geçmişinin test edildiği iddiası değildir. Kapsamı açıkça sınırlar: gerçek çekirdek durumları, kontrollü senaryolar, sentetik animasyon örnekleri ve henüz uygulanmamış durumlar. Sanat yönü VISUAL_SPEC içindeki UI01–08 / ENV01–10 kurallarını kullanır. Yeni yağmur/animasyon süreleri özgün uygulama tercihleridir; sabit Terra Nil ekran görüntülerinden ölçülmüş hareketler gibi sunulmaz.

## Tek tasarım dili

- İklim: aynı yeşil/kehribar/toprak paleti; kuraklık su erişimi ile dereceli sararma; yağmur hafif soğuk ışık, ince damlalar, su halkaları ve ıslak yüzey. Harita geçiş kuralları ve nehir sınırı değişmez.
- Canlı: otçul uzun kulak/boynuz, daha dik baş ve açık karın; avcı yatay uzun burun, sivri kulak, koyu sırt/yele, uzun kuyruk. Yaş doygunluğu azaltır; yavru boyu gerçek çekirdek yarıçapını izler.
- Hareket: dört ayrı bacak, çapraz çift yürüyüş; kat edilen mesafeye bağlı faz; yeme/içme/ısırma baş eklemi. Hareket veya hedef seçen yeni karar kuralı yok.
- Ölüm: çekirdekteki ceset varken yana yatmış, matlaşmış beden; çekirdek kaldırdığında görünüm de kaldırılır. Kalıcı iz yok.
- UI: gerçek eylem kısa açıklaması, yavru/yaralı bilgisi, aynı çerçeve ve çubuklar. İklim bir durum göstergesi; yeni karar düğmeleri eklenmez.

## Senaryolar ve kabul

| Durum | Veri kaynağı / örnek | Kabul / kanıt türü |
|---|---|---|
| Ilıman dünya | normal seed42 | Yağmur yok; su erişimi1; canlılar, meyve ve otlar görünür; gerçek ekran + hash |
| İleri kuraklık | --scenario=drought, gerçek Core2400tick kurak ısınma | Su erişimi düşük, ot miktarı azalmış, toprak ve bitki sararmış; gerçek ekran + hash |
| Kuraklık sonrası yağmur | --scenario=rain, aynı kurak ısınma ardından SetDrought(false) | Yağmur, kademeli su toparlanması; aniden yemyeşil olmaz; gerçek ekran + hash |
| Boş dünya | --scenario=empty, tek kısa ömürlü kurucunun gerçek ölümü sonrası | Sayaçlar0, yüzde hesapları sonlu, seçim yok, hata yok; 720p ekran |
| Yürüyüş / durma | DistanceTravelled, hız, tick | Çapraz bacak çiftleri, duraklatınca eklemler sabit; sentetik hareket galerisi ve çalışan dünya |
| Yeme / içme / meyve / ısırma | Core Action | Yalnız mevcut eyleme uygun baş pozu; sentetik galeri; gerçek örnekler olay gerçekleştiğinde |
| Avcı / otçul | gerçek diyet geni | Renk olmasa da burun/kulak/kuyruk silueti ayırt edilir; galeri |
| Yavru / yaşlı | Radius, Maturity, Aging | Boy ve doygunluk; sentetik galeri + çalışma anındaki gerçek veriye bağ |
| Yaralanma | RecentDamage / Health | Hafif beden tonu, sağlık30altında kart uyarısı; canlı kararı değişmez |
| Ceset / çürüme | Core Corpses / lifetime | Yana yatış, doygunluk kaybı, çekirdekten silinince kaldırma; galeri ve kurak senaryo |
| Yüzme | Core Map.IsWater | Beden suya alçalır, aynı bacak fazı; özel yüzme animasyonu henüz yok |
| 1× / 20× / duraklama | bridge | Sabit adım hash ve6sabit-adım testi;20× sürdürülebilir performans ayrı hedef |
| Yakın/uzak, örtülme, seçme | ortografik kamera, canopy reveal | Kamera ve seçme smoke testleri; yerel ağaç açılması |
| Düşük/yüksek kalite | aynı dünya | Yağmur1200→500örnek, mevcut ot/SSAO azaltımı; durum ve sayaçlar aynı |

## İklim davranışının sınırı

Godot kesitine240dünya günlük örnek döngü eklendi:80–160kuraklık,160–190yağış görünümü/toparlanma, kalanılıman. Döngü mevcut World.SetDrought girdisini kullanır; su/yiyecek/biyoloji formülleri değiştirilmedi. Her iklim girdisi tick numarasıyla köprüde kaydedilir ve doğrulama yeniden oynatılır. Bu kayıt henüz kalıcı dünya kayıt sistemi değildir. Yağmur damlaları ayrı bir hidroloji sistemi değildir; mevcut kuraklık-sonrası toparlanmayı görselleştirir. CLItest senaryolarında döngü kapalıdır.

## Henüz kapsam dışı / doğrulanmamış

Yangın, sel, kar, hastalık salgını, barınak yapımı ve afet tahliyesi mevcut çekirdekte yok. Sadece efekt çizip çalışıyormuş gibi gösterilmez. Gerçek doğum sıklığı/avlanma stratejisinin öğrenilmesi animasyon galerisinden çıkarılamaz. Kalıcı kayıt menüsü ve çoklu panel sistemi Godot kesitine henüz taşınmadı. Her türün her gen kombinasyonu veya bütün donanımlar test edilmiş değildir.

## Çalıştırılmış sonuçlar

Sonuç dosyası: `validation/state-tests-16.json`. Sekiz uygulama koşusu ve sekiz sentetik poz kontrolü geçti. Normal/kurak/yağışlı, gerçek ölüm sonrası sıfır nüfus, tam çevre döngüsü, dengeli kalite/uzak kamera,20×hedef ve yakın gözlem çalıştırıldı.20×koşusundaki “passed” davranış eşitliğidir; hedef hıza yetişme testi başarısızdır. Geçersiz ilk iki boş-dünya testinin nedenleri VISUAL_QA içinde korunur; finaltest boş dünyayı gerçek çekirdek ölümüyle oluşturur.
