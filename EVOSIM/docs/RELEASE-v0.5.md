# EVOSIM v0.5.0 — Gerçek geçmiş ve ayrı gelecekler

**15 Eylül 2026.** Kaydettiğin dünyanın erişilebilir geçmişinde istediğin ana gidebilir, kayıtlı geleceği tekrar izleyebilir ve bir geçmiş anından bağımsız bir dünya oluşturabilirsin. Önceki dünyanın geleceği korunur. Biyolojik çekirdek v0.4.1 ile aynıdır; canlıların karar politikası, kalıtımı, mutasyon oranı, kaynakları ve üreme modeli bu pakette değişmedi.

## Kullanım

1. Gözlem ekranındaki küçük araç menüsünden **Geçmiş**'i aç. Zaman çubuğu gerektiğinde görünür; harita ana alan olarak kalır.
2. Çubuğu sürükleyip bırak veya çizelge açıkken **sol ok** ile bir fizik adımı geri git. Çubuğun üzerindeki fare tekerleği bir simülasyon saniyesi değiştirir.
3. Seçilen an hazırlanırken **Vazgeç** veya Esc aramayı iptal eder. O anda gördüğün önceki görüntü ve dünyanın son durumu korunur.
4. Geçmişte **Oynat** veya tek adım ile kayıtlı geleceği izle. **Son ana dön** doğrudan en son durumu gösterir.
5. **Buradan devam**, seçilen anı yeni kimlikli bir dünya olarak kaydeder. Bu dal kendi geleceğini üretir; önceki dünya aynı kayıtlı geleceğe sahip olmaya devam eder. Yeni dal başlangıçta duraklatılır.
6. Geçmişteki kuraklık kontrolü **Yeni dalda kuraklık** diye görünür. Bu işlem önce ayrı dünya oluşturur, sonra müdahaleyi ona uygular. Kaydedilmiş geçmişin içine komut yazılmaz.

Geçmişe bakarken otomatik kayıt ve **Dünyadan çık** en son canlı durumu saklar. Dünya yeniden açıldığında son an gösterilir. Gözlem dışa aktarımı ise incelenen anı verir; raporda görüntülenen tick, son tick ve geçmiş önizlemesi bilgisi bulunur.

## Kayıt güvenliği ve eski dünyalar

Yeni **EVOSIM-LIBRARY-2**, tam son durum checkpoint'i ve **EVOSIM-TIMELINE-1** geçmiş arşivini tek `.evoworld` dosyasında atomik olarak saklar. Uzunluk ve SHA-256 kontrolleri yanlış eşleştirilmiş, yarım veya bozuk veriyi reddeder. Geçerli önceki kayıt yedekte korunur. Sil/geri al, gömülü geçmişi ve yedeği dünya ile birlikte yönetir; bir dalın kendi dosyası başka dalın dosyasına bağımlı değildir.

**LIBRARY-1 eski dünya ilk açıldığında ayrı `· v0.5` devam kopyası oluşturulur.** Eski dosya değiştirilmez. Geçmişin ilk erişilebilir anı o eski kaydın gerçek tick'idir. Örneğin yalnız 333. adımı saklanmış bir dünyada 0–332 adımlarının geçmişi varmış gibi gösterilmez; 333'ten sonraki yeni yaşam izlenebilir.

Replay kimliği, çalışan gerçek **Core DLL SHA-256** ve checkpoint şemasını içerir. Bu sürümün Core özeti:

```text
50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033
```

Core checkpoint biçimi **EVOSIM-0.4-WORLD-2** olarak kalır. Başka Core modeline ait timeline açık hata ile reddedilir; aynı eski komutları sessizce farklı biyolojiyle yeniden hesaplama yapılmaz. Buradaki birebir eşitlik kanıtı kullanılan CPU/.NET ortamına aittir; farklı runtime veya GPU yürütümü için ayrıca doğrulama gerekir.

## Zaman çizelgesi nasıl çalışır?

- İlk checkpoint ve **tam müdahale günlüğü** korunur. Şimdiki müdahale türü kuraklığı aç/kapatmaktır.
- **600 fizik adımında bir**, yani 30 simülasyon saniyesinde, GZip ile sıkıştırılmış hızlandırıcı checkpoint hazırlanır. **32 MiB** bütçe yalnız hızlandırıcılar içindir. Eski hızlandırıcılar bütçeyle çıkarılabilir; başlangıç ve komutlar silinmez, erişilebilir geçmiş aralığı küçülmez.
- Her checkpoint sonraki uygulanacak komutun indeksini taşır. Aynı adımda kuraklığı birkaç kez değiştirmek gibi durumlar kesin sırayla ve birer kez uygulanır.
- Arama ayrı bir dünya üzerinde hesaplanır. Tamamlanmadan ekrandaki dünya değiştirilmez. Her UI döngüsünde 6 ms hedefle en çok 100 fizik adımı hesaplanır; bir fizik adımı bölünemediğinden bu hedef bir adım kadar aşılabilir.

## Doğrulanan kapsam

**10/10 bağımsız timeline test grubu geçti.** Referans veriler timeline motoru kullanılmadan sabit v0.4.1 Core ile üretildi; yalnız nüfus sayıları değil, tam kaydedilebilir dünya baytları ve durum özeti karşılaştırıldı.

- 28 farklı geçmiş hedefi; 600 ve 1.200 checkpoint sınırlarındaki aynı-tick komutlar dahil.
- 1.800 adımlık kayıtlı geleceği oynatma ve 22 referans checkpoint eşitliği.
- Yedi adımlık arama parçasından sonra iptal; önceki 901. adım görünümü ve 1.800. adım son durumunun korunması.
- 601. adımda dal oluşturma, yeni müdahale ile bağımsız 200 adımlık gelecek; ebeveyn arşivinin değişmemesi.
- 1 KiB test bütçesinde hızlandırıcıları kırptıktan sonra başlangıçtan 1.733. adıma doğru replay.
- Geçmiş görünürken kaydet/aç, eski dosyayı koruyan içe aktarma, sil/geri al ve bağımsız dal dosyaları.
- 15 farklı timeline bozulması, beş LIBRARY-2 bozulması ve yanlış son dünya/arşiv eşleşmesinin reddedilmesi.

Yerel SDL2 UI smoke, zaman çubuğu/iptal/oynatma/dallanma/otomatik kayıt/geri alma ve eski fixture açma akışlarını da doğrular. Son yayın smoke kaydı, paketle birlikte arşivlenir. Bağımsız yöntem, ilk koşum ve fixture'lar **[timeline-tests](../artifacts/v0.5/timeline-tests/README.md)** içinde bulunur.

## Performans ve sınırlar

Temsilî paylaşılan makine CPU ölçümünde 256 harita / 500 kurucu / 350 ağaç koşulunda başlangıç checkpoint'ini açmak **14,6–20,4 ms**, başlangıçtan 599 adım replay **0,98–1,01 saniye**, en uzun replay iş parçası **7,9 ms** sürdü. Periyodik checkpoint içeren adım yaklaşık 13 ms'ye çıktı. Kaynak ve değiştirilmemiş ilk sonuçlar **[timeline-cost](../artifacts/v0.5/timeline-cost/README.md)** içinde. Bu ölçümler GPU/FPS veya azami yük garantisi değildir.

Checkpoint açma ve tam dünya dosyasını yazma **senkrondur**; ilk arama/yükleme veya uzun kayıtta kısa bekleme olabilir. 32 MiB bütün uygulamanın RAM bütçesi değildir: başlangıç, komutlar, son dünya, geçici yükleme kopyaları ve Core soy/geçmiş verisi ayrıca yer kullanır. Arşiv üst sınırı 512 MiB, komut sınırı 2.000.000'dur; sınır aşımı açık hata verir, geçmiş sessizce silinmez.

Yaşam filmi, soy grafiği, deney/karşılaştırma panelleri, önemli an sabitleme, yeni afetler, ağaç yaşam döngüsü ve yaşam içinde öğrenme sonraki dilimlerde kalır. v0.5.0 bu hedeflerin tamamlandığı anlamına gelmez.
