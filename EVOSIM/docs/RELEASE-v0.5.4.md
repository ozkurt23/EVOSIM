# v0.5.4 — Eksiksiz gözlem dışa aktarımı

Mevcut dışa aktarım komutu artık tek klasörde `report.json`, `population.csv`, `creatures.csv` ve `integrity.json` üretir. Ölüm nedenleri/zamanları, ebeveynler, nesiller, güncel beslenme sınıfı, enerji kapasitesi ve üreme teşhisi dosyada yer alır. Dünya adı/kimliği, görüntülenen an ve kaynak geçmiş aralığı raporun hangi dünyadan ve zamandan geldiğini gösterir.

Yeni şema `evosim.observation.v2`; eski v1 raporları değiştirilmez. Dosya arayan analiz betikleri yeni paket klasörünü kullanmalıdır. Detaylar `OBSERVATION-FORMAT-v2.md` içinde.

## Güvenilirlik

- Hazırlık klasörü tamamlanmadan nihai rapor görünmez; yakalanan hata temizlenir. Zorla kapatmada `.partial` kalabilir.
- Benzersiz paket adları önceki raporu korur. Üç veri dosyası için SHA-256 manifesti vardır.
- Türkçe sistem ayarında da CSV sayıları noktalı ondalıktır; tırnak/virgül/satır sonları doğru kaçırılır.
- Geçmiş araması sürerken dışa aktarım bekletilir. Hazır geçmiş anından export gelecekteki birey durumlarını eklemez.

## Kanıt

Yayımlanan Linux uygulamasında tam UI smoke ve yeni export grubu geçti. Gerçek seed42/1800tick dünyasında doğum/ölüm ve ebeveynler doğrulandı. Tam checkpoint/RNG eşitliği, aynı adımda tekrar export'un önceki dosyayı koruması, tr-TR altında bağımsız CSV ayrıştırıcıyla tüm satır/sütun ve çok satırlı UTF-8 metin dönüşümü, veri dosyası hashleri, kasıtlı NaN serileştirme hatasında hiçbir tamamlanmış paket yayımlanmaması ve staging temizliği, geçmiş tick0'da gelecekteki doğum/ölümlerin bulunmaması test edildi. Geçmiş aramasından sonra HeadWorld tam checkpoint'i de korundu.

UI smoke önceki dünya sil/geri al, kayıt, büyük harita, timeline/dallanma, soy, kaynak, olay ve yaşam paneli kontrollerini içerir. Bilerek silinen kayıtta save hata mesajı beklenen test çıktısıdır.

Biyolojik Core SHA aynı `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`. Davranış, mutasyon, kayıt ve replay biçimleri değişmedi. Bireysel sürekli enerji/su geçmişi henüz uygulanmadı; bu sürüm seçilen anın raporudur. Yeni harita/asset değişikliği yok; yinelenen ekran görüntüsü paylaşılmaz.
