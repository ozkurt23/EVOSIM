# EVOSIM 0.5.9 — Arka planda yaşam izi

16 Eylül 2026.

Yaşam izi hazırlanırken kayıt çözme ve geçmişi yeniden oynatma artık ayrı bir iş parçacığında çalışır. Arayüz yalnız hazır durum bilgisini okur; biten örnekler salt okunur bir koleksiyon olarak tek seferde aktarılır. Esc/Vazgeç hesaplamayı iptal eder. Kapanmış panelin sonuçları geri gelemez. Uygulamadan çıkış da iptal ister.

Kaynak dünya ve arşiv kopyası panel açılırken ana iş parçacığında alınır. Önceki ölçümde bu aşamanın ortancası koşula göre 9–67 ms idi; büyük kayıtlarda daha uzun sürebilir. Kopya alma ve tek bir çözme/fizik adımı ortasından kesilmez; iptal sonraki kontrol noktasında işlenir. Bu sürüm toplam hesaplama maliyetini ortadan kaldırmaz ve FPS/20× hız garantisi vermez. GPU kullanılmaz.

## Doğrulama

- Yayınlanan Linux uygulamasının tam UI testi geçti.
- Replay iş parçacığı UI iş parçacığından farklı.
- Tamamlanmamış işin değişebilir örnekleri UI'ya verilmez.
- Başlatılmış arka plan işi iptal sonrası sonlanır; sonuç tekrar yayımlanmaz.
- Birey 166'nın 6000 tick bağımsız ileri koşusuyla örnekler, doğum ve ölüm geçişleri birebir eşleşir.
- Kaynak checkpoint/RNG ve arşiv baytları aynı kalır; geçmiş görünümü gelecekteki ölüm bilgisini içermez.
- Grafik hover ve örnek anına gitme testleri geçti.

Core DLL ve kayıt formatı değişmedi. Kalıtsal 357 ağ ağırlığı, .001 doğum başına mutasyon ve mevcut fizik korunur. Testler davranışların başarılı evrimleştiğini kanıtlamaz.

Önceki sürümün dört koşul × üç tekrar performans ölçümü `artifacts/trace-performance/REPORT.md` içindedir. Bu ölçüm bir önce/sonra hızlanma iddiası değildir. Yeni ekran düzeni yoktur; mevcut yaşam izi görünümü korunmuştur.
