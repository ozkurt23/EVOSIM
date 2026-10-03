# EVOSIM v0.5.7 — Oyun içinde bireysel yaşam izi

Bir canlıyı seçip **Ayrıntı → Yaşam izi** aç. Enerji/kapasite, su ve sağlık eğrileri ile örneklenen konumlar aynı panelde görünür. İnceleme için simülasyon duraklar. **Vazgeç / Esc** hesaplamayı iptal eder; tamamlandığında **Kapat / Esc** paneli kapatır. Kapanıştan sonra simülasyon duraklamış kalır; normal oynat düğmesiyle devam edilir.

Panel kayıtlı geçmişten **görüntülenen ana kadar, en fazla son20simülasyon dakikasını** yeniden oynatır. Geçmiş izlenirken gelecekteki ölüm veya durum gösterilmez. Yaklaşık1saniyelik örnekler, doğum/ölüm geçişleri ve son nokta ile tamamlanır. Doğumdan önce çizgi yoktur; ölüm sonrası durumlar çizgide uzatılmaz. Pencerenin ilk noktası doğum anı olmak zorunda değildir.

Konum çizgisi örnekleri birleştirir; tam güzergâh, engel haritası veya davranış niyeti değildir. Eylem/temas nedenleri için yeni olay uydurulmaz. Ölüm nedeni Core'un son etiketidir; bütün hasarların payını açıklamaz.

## Uygulama

LifeTraceJob kaynak head checkpoint'i ve arşivini sabit kopyaya alır; replay kendi özel dünyasında çalışır. Biyolojik ilerleme kare başına6ms hedef/100adım üst sınırıyla bölünür. Hazırlık sonrası gerçek panel kullanılabilir ve iptal girişi işlenir. İlk snapshot serileştirme, arşiv yükleme ve checkpoint açma bölünemez işlemlerdir; çok büyük kayıtta6ms bütçesini aşabilir. Bu sürüm bütün hazırlık aşamalarına sert gecikme garantisi vermez. Uzun geçmişte başlangıca arama da ek süre gerektirir.

İptal özel dünya/arşiv/nokta referanslarını bırakır. Panel kapanması, dünya değişimi, diğer gözlem panelleri ve ana menüyle aynı modal kapatma akışı kullanılır. Kullanıcının gösterdiği dünya, kamera zamanı ve HeadWorld değiştirilmez. İzler şu anda kalıcı ayrı ölçüm dosyası olarak saklanmaz; panel açıldığında yeniden hesaplanır. Ayrı CLI aracı daha uzun/parçalı JSON çıkarımı için `artifacts/individual-trace` içinde korunur.

## Doğrulama

Yayımlanan Linux uygulamasında UI smoke geçti.6000adımlık bağımsız ileri koşuda yavru166'nın beden ve konumu her adımda kaydedildi. Küçük pompa bütçeleriyle elde edilen panel noktaları bu ölçümlerle birebir eşleşti. Tam doğum/ölüm geçişleri, pompa başına7adım sınırı, gerçek kart düğmesi, modal altında müdahale/oynatma engeli, tamamlanmış paneli kapatma, başlatılmış işi iptal etme ve özel verinin temizlenmesi doğrulandı. Aynı anda kuraklık aç/kapa ve sonraki iklim komutları test edildi.

Geçmiş1200tick'ten açılan iz, gelecekteki ölümü göstermedi ve1200tick sınırını aşmadı. Görüntülenen dünya hash'i, orijinal HeadWorld'ün tam checkpoint/RNG'si ve timeline arşivi aynı kaldı. Önceki kayıt, geçmiş, dışa aktarım, katman, kitaplık, soy ve kaynak UI kontrolleri de geçti. Silinmiş kayıtta beklenen save hata testi logdaki InvalidOperationException'ın nedenidir.

`artifacts/v0.5.7/trace.png` gerçek yayımlanan uygulama, seed42/6000tick/165kurucu/37yaşayan/24doğum/152ölüm; seçilen166numaralı yavru. Bu capture'da kuraklık komutu yoktur; önceki CLI deneyindeki iklim müdahaleli örnekle karıştırılmamalı. Bu dünyada yavrunun ölüm nedeni açlıktır. Görüntü incelendi. Yeniden üretim: `--capture çıktı.bmp --ticks 6000 --select 166 --trace`.

Core SHA değişmedi: `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`.
