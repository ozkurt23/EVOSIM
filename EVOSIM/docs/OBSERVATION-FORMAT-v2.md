# Gözlem paketi biçimi v2

Uygulama v0.5.4, JSON `schema = evosim.observation.v2`.

## Dosyalar

Menüdeki mevcut dışa aktarım komutu `observations/evosim-TOHUM-tTICK-TARİH-KİMLİK/` klasörü oluşturur:

| Dosya | İçerik |
|---|---|
| report.json | Dünya ayarları, görüntülenen durum, tüm kayıtlı bireyler, kaynaklar, üreme/ölüm teşhisi, nüfus örnekleri ve saklanan olaylar |
| population.csv | Önceki CSV ile aynı sütunlar: sim_seconds, herbivores, carnivores, births, deaths |
| creatures.csv | Her kayıtlı birey için bir satır; yaşayanlar ve ölenler dahil |
| integrity.json | Diğer üç dosyanın SHA-256 özetleri |

Önce bütün dosyalar aynı üst klasörde `.partial` geçici klasörüne yazılır. Başarıdan sonra klasör tek taşıma işlemiyle tamamlanmış ada geçer. Yakalanan yazım/serileştirme hatasında geçici klasör temizlenir. İşlem veya bilgisayar zorla kapanırsa `.partial` kalabilir; bu tamamlanmış rapor değildir. Aynı anda yeniden dışa aktarmak önceki raporun üzerine yazmaz. Elektrik kesintisine karşı disk kalıcılığı garantisi verilmez.

v1'in doğrudan yan yana JSON/CSV dosyaları korunur. Yeni v2 çıktısı klasör paketidir; dosya arayan eski analiz betikleri bu yolu ve yeni şema etiketini dikkate almalıdır. Dünya kaydı ve zaman çizelgesi biçimleri değişmedi.

## Hangi an?

- `viewTick`: bütün birey, kaynak, sayaç, tarihçe ve olay verisinin ait olduğu fizik adımı.
- `headTick`: kaynak dünyanın ulaştığı son adım. Geçmişten rapor alındığında viewTick'ten büyük olabilir.
- `earliestTick`: mevcut kaynak geçmişinin erişilebilir başlangıcı; eski içe aktarımlarda 0 olmak zorunda değildir.
- `historyPreview`: viewTick < headTick.
- `sourceWorldId`, `worldName`: varsa mevcut kaydın kimliği ve adı; kayıtsız dünyada null.
- `coreSha256`: raporu üreten biyolojik DLL kimliği. `stateHash`: görüntülenen Core durumunun özeti.

Tick başına .05 simülasyon saniyesi. Takvim zamanı yalnız `createdAtUtc` alanındadır. Geçmişten rapor, gelecekte doğacak bireyleri veya ölümleri içermez; headTick yalnız arşiv aralığını bildirir. Rapor devam ettirilebilir checkpoint değildir. Harita ve simülasyonu yeniden başlatmak için dünyayı uygulamanın normal kayıt sistemiyle sakla.

## Birey sütunları

CSV UTF-8, virgül ayracı, noktalı ondalık ve CRLF satır sonu kullanır. Virgül, çift tırnak veya satır sonu içeren metinler standart CSV tırnak kaçışıyla yazılır. `alive` true/false; bilinmeyen/uygulanmayan ölüm alanları boş hücredir.

| Sütunlar | Anlam |
|---|---|
| view_tick, id | Gözlem adımı ve dünya içindeki kalıcı birey kimliği |
| founder_group | Başlangıç kategori kodu; üreme modu/ömür ayarlarının seçildiği Group |
| current_diet | Görüntülenen bireyin beslenme genine göre herbivorous (< .5) veya carnivorous (≥ .5) |
| generation, parent_a, parent_b | Nesil ve ebeveyn kimlikleri; kurucu nesli 0, ebeveyn yoksa ID 0; eşeysiz doğumda parent_b=0 |
| alive, age_seconds, x, y | Yaşam durumu, biyolojik yaş, harita konumu |
| energy, max_energy, hydration, health | Anlık enerji ve kapasitesi; su ve sağlık düzeyi. Enerji oranı energy/max_energy ile bulunur |
| death_seconds, death_cause | Ölü bireyin ölüm anı ve Core son neden etiketi; yaşayanlarda boş |
| body, perception, diet, aquatic, boldness, bond | Altı kalıtsal genin 0–1 değeri |
| maturity, reproduction_cooldown | Büyüme olgunluğu ve kalan üreme bekleme saniyesi |
| speed_capacity, radius | Mevcut bedenin hız kapasitesi ve fiziksel yarıçapı; hız kapasitesi anlık hareket hızı değildir |
| distance_travelled, blocked_seconds, action | Birikimli yol, mevcut sıkışma süresi ve son eylem etiketi |

Ölü bireylerin beden alanları ölüm sonrası güncellenen bir yaşam ölçümü değildir; son saklanan durumlarıdır. `action` veya motor çıktısı canlının niyetinin açıklaması değildir. Birey CSV'si sürekli zaman serisi oluşturmaz; başka bir an için ayrı rapor al.

JSON `creatures` önceki gen, beyin ağırlığı/belleği ve motor alanlarını korur; ölüm zamanı/nedeni, currentDiet, MaxEnergy ve beden/üreme durumu ekler. JSON yaşayan ölüm alanları null'dır. Önceki alan adlarının büyük/küçük harfleri korunur: örneğin Id, ParentA, Alive; ek alanlarda deathTime ve deathCause kullanılır.

## Teşhis ve sınırlar

`breeding` ve `populationHealth.Breeding` aynı gözlemi taşır. Doğrudan koşul sayımları örtüşebilir; `ExclusiveFirstBlocker` yaşayan nüfusu sırayla bölümlendirir. Hazır çift sayısı gerçekleşmiş doğum değildir. `populationHealth.DeathCauses` yalnız kayıtlı ölü bireylerin son neden etiketlerini sayar; boş neden Bilinmiyor grubuna alınır. Bütün geçmiş hasarların nedensel payı hesaplanmaz.

`history` nüfus örneklerini, `events` Core'un hâlâ sakladığı olayları içerir. Core çok uzun olay listesinin başını kırpabilir. Bu dosyalar bütün bireylerin kesintisiz yaşam filmi veya bütün olayların sınırsız arşivi olarak sunulmaz.
