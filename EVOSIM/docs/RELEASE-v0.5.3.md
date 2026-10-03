# EVOSIM v0.5.3 — Nüfusun yaşam durumu

Nüfus azalırken nedenlerini gözlemlemek için **··· → Nüfus grafiği → Genler → Yaşam** yolunda yeni sekme bulunur. Panel açıldığında gözlem duraklar. Genler sekmesine geri dönülebilir; Esc veya × paneli kapatır. Normal simülasyonun üç sabit kontrolü korunur.

## Gösterilen gerçek veriler

- Görüntülenen ana kadar doğum, ölüm ve mutasyon toplamları.
- Ölü bireylerin kayıtlarından susuzluk, açlık, avlanma/çatışma, yaşlılık ve diğer/bilinmeyen nedenler. Her çubuk kayıtlı ölü bireyler içindeki paydır. Neden kaydı adedi ile toplam ölüm ayrı gösterilir; eksik eski kayıtlar için neden uydurulmaz.
- Yaşayan bireyler için üreme önündeki **ilk engel**: kapalı mod, yetişkinlik, bekleme, enerji, su, sağlık, diğer uygunluk koşulları, olumlu motor sinyali yokluğu; ardından hazır ve sinyal verenler. Her birey yalnız bir satırda yer alır. Satır sırası Core'un mevcut teşhis sırasıdır, yeni bir karar politikası değildir.
- Temas mesafesinde uygun hazır eşeyli çiftler ve hazır eşeysiz bireyler. Hazırlık gerçek doğum garantisi değildir.

Ölüm sayımları birikimlidir, üreme tablosu ise yalnız görüntülenen anın durumudur. Son ölüm nedeni tüm birikmiş hasarı veya nedensel payı açıklamaz. İlk üreme engeli başka engellerin olmadığı anlamına gelmez.

## Doğrulama

Yayımlanan Linux uygulamasında UI smoke geçti: gerçek sekme tıklamaları, geri dönüş, üreme gruplarının yaşayan nüfusu eksiksiz bölmesi, ölümlerin gerçek kayıtlarla muhasebesi, modal altında müdahale/adım engeli, tam checkpoint ve RNG değişmezliği, geçmiş tick0'da gelecekteki doğum/ölümlerin görünmemesi. Önceki kayıt, sil/geri al, büyük harita, geçmiş/dallanma, soy, kaynak ve olay günlüğü UI regresyonları da geçti. Bilerek silinmiş kayıtta kayıt hatası test logunda beklenen InvalidOperationException üretir; testin sonucu passed=true.

`artifacts/v0.5.3/life.png` gerçek yayımlanan uygulamadan, seed42/6000tick/300sn:37yaşayan,24doğum,152ölüm. Görüntü incelendi; metinler ve satırlar panel içinde okunuyor. `--ecology` CLI seçeneği bu görüntüyü tekrar üretir.

Core SHA256: `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`.
Core, .001 mutasyon ve biyolojik davranışlar değişmedi; v0.5 dünya/timeline kimliği korunur. UI dışından aynı dünyayı değiştiren eklentiler desteklenmez; gözlem paneli açılırken ve görüntülenen dünya/tick değişince veri yenilenir.

Önceki uzun koşu çalışması `artifacts/ecology-2026-09-15/` içinde pakete dahil edildi. Bu çalışma rastgele kurucularda henüz sürdürülebilir ekosistem veya öğrenilmiş beceri göstermiyor. Yeni sekme bu sorunu çözmüş olarak sunulmaz; darboğazı gözlemlemeyi sağlar.
