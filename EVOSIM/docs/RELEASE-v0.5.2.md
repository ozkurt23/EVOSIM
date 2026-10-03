# EVOSIM v0.5.2 — Olay günlüğü

Araç menüsündeki **Olay günlüğü** doğum, ölüm, mutasyon ve çevre olaylarını en yeniden eskiye gösterir. Tümü, Doğumlar, Ölümler, Mutasyonlar ve Çevre filtreleri vardır. Mutasyonlu doğum iki ilgili filtrede de bulunur. Tekerlek, yukarı/aşağı, Page Up/Down ve Home/End ile gezinilir. Esc veya × kapatır. Panel açıkken simülasyon durur.

Doğum satırındaki **Soy**, gerçek yavrunun yerel aile grafiğini açar. Erişilebilir geçmişteki **Anı gör**, olayın fizik adımını zaman çizelgesinde açar. O andaki bütün işlemler tamamlanmış dünya gösterilir; aynı adım içindeki ara durumlar ayrı zaman noktaları değildir. Kayıtlı son dünya korunur.

Günlük yalnız görüntülenen dünyanın mevcut olaylarını okur. Eski bir anda gelecekte doğacak bireyler veya gerçekleşecek olaylar görünmez. Biyolojik Core ve kayıt/timeline biçimleri değişmedi; doğum başına varsayılan mutasyon %0,1.

## Sınır

Bu, Core'un sakladığı olay penceresidir; eksiksiz ve sınırsız bir olay arşivi değildir. Mevcut model 2.000 satırı aşınca en eski 500 satırı çıkarır. Daha eski kayıtlı anlara zaman çizelgesinden giderek o anın olayları incelenebilir. Otomatik keşif/zekâ yorumu veya yeni olay üretimi eklenmedi. Ölüm açıklamaları modelin mevcut nedenleridir.

## Doğrulama

Yayın uygulamasında UI smoke geçti: gerçek ters olay sırası, filtre sınırları ve mutasyonlu doğumun kapsamı, tıklanabilir filtre, tekerlek, doğumdan gerçek soy açılması, olaydan zaman çizelgesine geçiş, Escape, modal girdi yalıtımı, tam checkpoint baytlarının değişmemesi ve başlangıç anında gelecek olaylarının görülmemesi. Önceki dünya, kurulum, kaynak, soy, histogram ve geçmiş test akışları da geçti. Core SHA v0.5.1 ile aynı kaldığından biyoloji testleri gereksiz tekrarlanmadı.

Kanıt: `artifacts/v0.5.2/ui-smoke.json`, günlük ve gerçek yayın ekranı. Logdaki kaydetme istisnası başarısız kayıt sırasında uygulamanın kapanmadığını sınayan kasıtlı regresyon testidir.

Ekran üretimi: `./baslat.sh --capture work/journal.bmp --timeline --journal --map-size 256 --herbivores 480 --carnivores 20 --trees 350 --ticks 1200 --zoom 1.2`. Yakalama dünyası kullanıcı kayıtlarını kullanmaz.
