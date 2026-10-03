# EVOSIM v0.5.8 — Yaşam izinden gerçek ana geçiş

**Canlı → Ayrıntı → Yaşam izi** grafiğinde fareyle gez:

- En yakın gerçek örneğin zamanı, enerji yüzdesi, su ve sağlık değeri görünür.
- Grafikte dikey işaret ve üç durum noktası; konum çizgisinde aynı örneğin halkası gösterilir.
- Doğum öncesinde “Henüz mevcut değil”; ölüm sonrasında “Ölü · son beden durumu” yazılır. Ölü bedenin saklanan değeri yaşayan ölçüm gibi sunulmaz.
- Grafiğe tıklandığında panel kapanır, zaman çizelgesi açılır ve seçilen örneğin tam tick'ine gidilir. Dünya duraklatılmış kalır; kayıtlı gelecek korunur.

Gösterilen değerler interpolasyonla üretilmez; yakın örneğe tutunulur. Grafik sağ kenarı son örneğe erişir. İz penceresi hâlâ görüntülenen ana kadar en fazla20simülasyon dakikasıdır. Geçmişten açılan iz gelecekteki birey bilgilerini göstermez. Hazırlık ve örnekleme sınırları v0.5.7 notlarında geçerlidir.

## Doğrulama

Yayımlanan uygulamanın UI smoke testi genişletildi: doğum öncesi hover, ölüm sonrası son örnek, grafik sağ kenarı, geçmiş penceresinde canlı örneği, gerçek tıklamayla timeline açılması ve tam örnek tick'ine gitme doğrulandı. Kaynak HeadWorld tam checkpoint'i aynı kaldı. Önceki bağımsız ileri ölçümle eşitlik, iptal, modal girdi, kayıt/replay/export/katman/kitaplık/soy/kaynak kontrolleri de geçti.

`artifacts/v0.5.8/hover.png` gerçek yayımlanan uygulamadan, seed42/6000tick/166numaralı yavru.130.00.sn örneği enerji%2.2, su100.0, sağlık74.2 gösterir. Konum halkası aynı örneğe aittir. Görüntü incelendi. Yeniden üretim: `--capture çıktı.bmp --ticks 6000 --select 166 --trace --trace-hover-tick 2600`.

Biyolojik Core SHA değişmedi: `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`. Mutasyon, dünya şeması ve kayıtlı geçmişin biyolojisi aynı. Tıklama yeni bir dünya dalı oluşturmaz; yalnız mevcut geçmişi görüntüler. Geçmişe müdahale etmek veya oradan farklı devam etmek için mevcut dal oluşturma akışı kullanılır.
