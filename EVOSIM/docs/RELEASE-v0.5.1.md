# EVOSIM v0.5.1 — Canlıyı, ailesini ve kaynakları incele

**15 Eylül 2026.** Bu paket gözlem araçlarını genişletir: canlı kartında altı gen ve güncel beden kapasitesi, tıklanabilir yerel soy grafiği, nüfusun gen dağılımları ve haritadaki kaynaklar için açıklayıcı kartlar. Paneller gerektiğinde açılır; normal gözlemde harita ana alan olarak kalır.

**Biyolojik Core ve zaman çizelgesi biçimleri değişmedi.** Canlı davranışları, ağ ağırlıkları, ödül/beden kuralları ve dünya ayarlarına yeni mekanik eklenmedi. Gözlem yapmak RNG tüketmez; v0.5 kayıtları ve kaydedilmiş geçmiş aynı model kimliğiyle açılır.

## 1. Canlı kartı: özellik ve güncel kapasite

Bir canlıyı seçip **Ayrıntı**'yı açınca altı kalıtsal değer görünür: **Beden, Algı, Beslenme, Su uyumu, Atılganlık, Sosyal bağ**. Her değer, 0–1 aralığını gösteren **salt okunur bir çubuk** ve üç ondalıklı sayıyla sunulur; bu kart genleri değiştirmez. Yanındaki küçük **i** üzerine gelince mevcut ayar açıklamasını kullanan bilgi baloncuğu açılır.

Kart ayrıca modelin hesapladığı **hız kapasitesini**, **görüş yarıçapını/açısını** ve **yaş etkisini** gösterir. Hız kapasitesi, canlının o anda ilerlediği hızla aynı şey değildir; hareketsiz duran bir bireyin de hareket kapasitesi olabilir. Genler ile yaş/beden koşullarından gelen güncel kapasite ayrı görünür. Aynı karttaki yavru/torun sayıları yeni soy indeksiyle hesaplanır.

## 2. Soy: yaşayan ve ölmüş akrabalar

Canlı kartındaki **Soy** düğmesi gözlemi duraklatan sıcak açık bir aile paneli açar. Üstte ebeveynler, ortada seçilen birey, altta dört kişilik sayfalar halinde doğrudan yavrular bulunur. Bağlantı çizgileri ilişkileri gösterir. Düğümde kimlik, otçul/etçil şekli, nesil ve yaşam durumu vardır; yaşayan veya ölmüş bir yakına tıklayarak onun ailesine geçilebilir. Yaşayan seçili bireyde **Haritada göster** paneli kapatıp o bireyi seçer.

Sayım kuralları açıktır:

- Ebeveyn ve doğrudan yavru kimlikleri tekrarlanmaz.
- **Torun**, tam iki ebeveyn-yavru bağıyla erişilen benzersiz bireydir. Sıradışı bir soyda aynı kişi ayrıca doğrudan yavru da olabilir; her ilişki grubunda bir kez sayılır.
- **Toplam altsoy**, bütün kuşaklarda erişilen benzersiz bireylerin birleşimidir. Bir bireye iki aile yolundan erişmek onu iki kez saydırmaz.
- **Yaşayan altsoy**, bu birleşimin görüntülenen anda yaşayan bölümüdür.

Esc/X paneli kapatır. Tab/Shift+Tab ve Enter ile düğümler/kontroller kullanılabilir; oklar ve panel üzerindeki tekerlek yavru sayfalarını değiştirir. Modal açıkken arka planda dünya seçimi, hareket, zoom, simülasyon adımı veya kuraklık komutu çalışmaz.

Bu **yerel aile grafiğidir**. Bütün soy ağını tek ekranda açma, bireyin yaşam filmi ve soylar arasında karşılaştırma sonraki işlerdir.

## 3. Nüfusun gen dağılımları

Araç menüsünden **Nüfus grafiği**, ardından bu karttaki **Genler** düğmesi yeni paneli açar. Altı özellik için on aralıklı histogram, ortanca, en az ve en çok değer gösterilir. Her grubun çubukları kendi nüfusu içindeki yüzdeyi temsil eder; farklı grup büyüklükleri bar yüksekliğini doğrudan belirlemez.

Yalnız yaşayan bireyler sayılır. Gruplar kurucu kimliğine göre değil, **güncel beslenme genine** göre ayrılır: 0,5 altı otçul ağırlıklı, 0,5 ve üzeri etçil ağırlıklıdır. Tam 1 değeri son histogram aralığına girer; boş gruplar sıfır örnek ve boş istatistiklerle gösterilir.

Özet ayrıca su uyumu **≥ 0,85** olan yüzücüleri, yaş ve fiziksel olgunluk koşullarını geçen yetişkinleri ve dünyanın etkin yaşlanma kuralından etkilenen bireyleri sayar. Dağılım tek başına öğrenme veya neden–sonuç kanıtı olarak yorumlanmaz.

## 4. Haritadaki kaynak kartları

Canlıyla çakışmayan bir kaynağa tıklayınca ilgili nesne vurgulanır ve küçük bir kart açılır. Canlı seçimi önceliğini korur.

| Seçilen şey | Gösterilen gerçek durum |
| --- | --- |
| Meyve ağacı | Meyveli yuva sayısı, toplam ham enerji, yerel nem, taç yarıçapı; her meyve yuvasının enerjisi veya yaklaşık yenilenme süresi |
| Ot parçası | Yerel ham enerji, ot/meyve yeme hız sınırları, nem ve yenilenme açıklaması |
| Su | Dünya su düzeyi, içme arzı çarpanı, içme üst hızı ve yüzme eşiği |
| Kaya | Hareketi/görüşü engellemesi; bu sürümde besin veya yapı malzemesi olmaması |
| Doğal barınak | Dinlenmede hareket dışındaki gider çarpanı ×0,72; dışarıda dinlenmenin ×0,90 değerine göre %20 daha düşük gider |
| Ceset | Kalan ham enerji, doğal çürüme hızı ve en geç kaybolacağı süre |

**Ham besin enerjisi, canlının kesin kazanacağı enerji değildir; sindirim beslenme genine bağlıdır.** Ağacın yenilenme tahmini o andaki su düzeyini ve saniyelik ekoloji güncellemesini kullanır; su koşulları değişince süre değişebilir. Su düzeyi yerel derinlik göstergesi değildir. Modelde bulunmayan ağaç yaşı/sağlığı veya gizli ot kapasitesi varmış gibi değer gösterilmez. Ceset yenerek/çürüyerek daha erken tükenebilir; silindiğinde kartı da kapanır.

## Geçmişle tutarlılık ve doğrulama

Bütün araçlar **ekranda görüntülenen World**'ü okur. Geçmişteki aileye henüz doğmamış bireyler eklenmez; nüfus ve kaynak kartları son dünyanın verisini geçmişin üzerine taşımaz. Dünya referansı değiştiğinde ilgili önbellek/seçim yenilenir. Soy topolojisi doğumla, yaşayan özetleri en geç 20 tick'te yenilenir; modal açılışında güncel değer zorunlu alınır.

Üç bağımsız doğrulama paketinin her biri **6/6 grup** geçti:

- **[Soy testleri](../artifacts/v0.5.1/lineage-tests/README.md):** ortak yollar, tam derinlik, 4.000 kişilik DAG, geçmiş dünya değişimi, tam checkpoint ve sonraki 200 adım eşitliği. Gerçek UI kontrolü seed42'nin 24 doğal doğumundan oluşan aileyi kullanır.
- **[Nüfus testleri](../artifacts/v0.5.1/insights-tests/README.md):** bilinen ortancalar, histogram sınırları, boş gruplar ve modeldeki yüzme/olgunluk/yaşlanma kuralları; 400 gözlemin her birinde önce/sonra tam checkpoint ve hash değişmezliği, ardından 400 adımlık gelecek eşitliği.
- **[Kaynak testleri](../artifacts/v0.5.1/resource-tests/README.md):** stok, tahmini yenilenme, tüketim/çürüme sınırları, kaynak türleri ve salt okunur model verisi.

**Son yayın UI smoke geçti:** kaynak ve kart girdileri, nüfus kartında arka plana tıklama sızmaması, bağlamdan soy açma/yavru düğümü seçme, histogram modalı, tam dünya baytlarının korunması ve geçmişte gelecek verisinin görünmemesi doğrulandı. Smoke kaydı ve gerçek ekranlar paket kanıtlarıyla arşivlenir.

Core SHA-256, v0.4.1 ve v0.5.0 ile aynıdır:

```text
50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033
```

v0.5.0'ın kayıt/arama sınırları sürer: 32 MiB yalnız timeline hızlandırıcı önbelleğidir; tam kayıt ve ilk checkpoint açma senkrondur. Çok uzun soyların sorgu maliyeti ayrıca ölçülecek. Kontrollü deney paneli, yaşam filmi, küresel soy görünümü, yoğunluk katmanları, yeni afetler ve öğrenme bu sürüme eklenmiş sayılmaz.
