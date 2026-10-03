# EVOSIM v0.5.5 — Harita katmanları

**··· → Katman** veya **V** ile sırayla canlı yoğunluğu, bitkisel besin, kara nemi ve normal görünüm arasında geç. Sol üstteki açıklamanın **×** düğmesi katmanı kapatır. Varsayılan görünüm kapalıdır; normal simülasyonda üç sabit kontrol korunur. Katman açmak simülasyonu duraklatmaz.

## Gerçek ölçüm

Harita 8×8 dünya birimlik bölgelere ayrılır. Bunlar fizik hücresi veya yeni biyolojik mekanik değil, yalnız gözlem gruplarıdır.

- **Canlı yoğunluğu:** her bölgedeki yaşayan birey sayısı; ölüler sayılmaz. İki beslenme grubu birlikte gösterilir.
- **Bitkisel besin:** bölgedeki ot ve meyvenin ham enerji stoğu. Meyveler gövdeye değil gerçek meyve konumlarına göre sayılır. Cesetler dahil değildir; bu katman sindirim verimi, erişilebilirlik veya canlı başına alınabilecek enerji hesabı değildir.
- **Kara nemi:** su olmayan hücrelerin nem ortalaması. Tamamı su olan bölge boş bırakılır. Bir bölgenin rengi o bölgenin kara ortalamasıdır; kare içinde su da olabilir.

Canlı/besin ölçeği 0 ile **o anda bütün haritadaki en büyük bölge değeri** arasındadır; üst sınır açıklamada gösterilir. Bu nedenle farklı dünyaların veya anların aynı rengi aynı mutlak miktarı ifade etmez. Nem ölçeği sabit0–1'dir. Sıfır canlı/besin bölgeleri saydamdır. Haritada kareler doğrudan ölçülen bölgeleri gösterir; aradaki boşluğa nüfus uyduran bir yumuşatma uygulanmaz.

Katman geometrisi ekrandaki bölgelerden oluşturulur ve tek SDL geometri çağrısıyla çizilir. Canlılar, ağaçlar ve kaynaklar bunun üzerine çizilir. Kamera ve canlıların taç arkasında görünürlüğü korunur. Yenileme dünya/tick/katman/kuraklık değişimine bağlıdır; durmuş aynı durumda tekrar hesaplanmaz. Katman seçimi görsel oturum tercihidir, dünya kaydına veya biyolojik checkpoint'e eklenmez.

## Doğrulama

Yayımlanan Linux uygulamasında UI smoke geçti. Yeni testler:

- 128/192/256 haritada canlı sayısının korunması ve ot+meyve toplamının gerçek stokla eşitliği;
- 8birim sınırının iki tarafındaki bireylerin farklı bölgelere girmesi, ölülerin sayılmaması, boş nüfus ölçeği;
- nem0–1 aralığı ve hesaplama öncesi/sonrası tam checkpoint/RNG eşitliği;
- gerçek V tuşu döngüsü ve × kapatma;
- açıklama metni üzerinde tıklama/tekerleğin haritaya sızmaması;
- geçmiş tick0'da gelecekteki nüfus cache'inin kullanılmaması ve HeadWorld checkpoint'inin korunması.

Önceki kayıt/geçmiş/dallanma/export/soy/kaynak/yaşam paneli kontrolleri de geçti. Beklenen silinmiş kayıt hata testi logda InvalidOperationException üretir; sonuç passed=true'dur.

`artifacts/v0.5.5/layer-1.png`, `layer-2.png`, `layer-3.png`: gerçek yayımlanan uygulama,256harita/500kurucu/350ağaç/seed42/1200tick.480yaşayan,33doğum,53ölüm. Üç görüntü ayrı ayrı incelendi. Yeniden üretim için `--layer 1`, `--layer 2`, `--layer 3` capture seçenekleri kullanılabilir.

Core SHA yine `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`; mutasyon, davranış, dünya ve replay biçimleri değişmedi. Katman varlığı dengeli ekosistem veya öğrenilmiş davranış kanıtı değildir.
