# EVOSIM v0.5.6 — Dünya kitaplığında sıralama ve filtre

Ana menü ve dünya içindeki **Dünyalar** kitaplığında iki küçük kontrol bulunur:

- **Sıralama:** son kaydedilen → ad (Türkçe alfabetik) → nüfus (çoktan aza) → simülasyon süresi (uzundan kısaya).
- **Durum:** tüm dünyalar → canlı var → canlı yok.

Kontrole her basış bir sonraki seçeneğe geçer. Sıra veya durum değişince ilk sayfaya dönülür ve açık kayıt eylemleri kapanır. Ana menüde eşleşen/toplam kayıt adedi görünür. Filtre sonucunda kayıt kalmazsa bunun filtre nedeniyle olduğu belirtilir. Aynı kontroller silinenler listesine de uygulanır; geri alma özelliği korunur.

Sıralama ve filtreleme **son kaydedilmiş metadata** üzerinden yapılır. Kayıtlar açılmaz, simülasyonlar ilerletilmez ve disk üzerindeki kayıtlar değiştirilmez. Henüz otomatik kaydedilmemiş canlı dünyadaki değişiklikler kitaplıkta anında görünmek zorunda değildir. “Canlı yok” başlangıçta boş oluşturulan dünyaları da kapsar; otomatik olarak neslin tükendiği sonucunu çıkarmaz.

Ad sıralaması Türkçe harfleri dikkate alır (ör. Ç, I, İ). Eşit değerlerde kimlik ile kararlı sıra kullanılır. Görünüm tercihleri oturum içindedir, dünya kaydına eklenmez. Normal simülasyon kontrol sayısı değişmedi.

## Doğrulama

Yayımlanan Linux uygulamasında UI smoke geçti. Yeni bağımsız sorgu kontrolleri Türkçe ad sırası, nüfus/süre/kayıt tarihi sırası, eşitlik bağları, yaşayan/boş filtre ve boş listeyi kapsar. Girdi listesinin sırası ve metadata alanları değişmeden kalır. Gerçek ana menü ve dünya içi kitaplık kontrollerine tıklama; filtre döngüsü; simülasyon hash'i ve kayıt metadata'sının değişmezliği doğrulandı. Önceki kayıt/geçmiş/export/katman/soy/kaynak/yaşam kontrolleri de geçti.

`artifacts/v0.5.6/library.png` gerçek yayımlanan uygulamanın izole UI test kayıtlarıyla ürettiği ana menüdür; kullanıcı dünyaları testte kullanılmadı. Görüntüde üç kayıt ve yeni iki kontrol vardır. Görsel kontrol yapıldı.

Core SHA aynı `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`; dünya metadata şeması, checkpoint ve replay biçimleri değişmedi. Kaynaklar, DLL, test ve paket ayrı v0.5.6 klasöründe; önceki çalışan sürüm korunur.
