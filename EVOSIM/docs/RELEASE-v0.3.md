# v0.3 teslimi — 15 Eylül 2026

İlk görsel ve kullanıcı deneyimi paketi tamamlandı. Yeni mekaniklerin tamamı yapılmış sayılmaz: üreme güdüsü, ağaç profilleri, büyük harita ve replay sonraki dilimlerdir.

Doğrulama: full packaged UI smoke PASS; 73 açıklama/6 gen, yüzde dönüşümü, sürgü sürükleme ve ince adım, aralık kesişmeme, çakışan tutamaçları ayırma, tooltip gecikmesi/pin/Esc ve kapalı alan giriş engeli, scroll, dünya silme/geri alma, silinen kaydın otomatik kayıttan korunması. Renderer kontrolü: biyolojik hash değişmez, seçilmeyen otçul/etçil taç altında görünür/seçilebilir, geçişli saydamlık ve dünya değişiminde cache temizliği.

Core binary v0.2.1 ile aynı: d7a421821220ca2f91da9c734c5d17f4867a2bca496189203b5438c7846c9cc0. Art değişiklikleri biyolojiyi etkilemedi. Aynı Core'un 40 kontrollü koşusunun tamamı invariant kontrollerini geçti; nüfus azalması bir teknik hata veya iyi davranışın evrimleştiği kanıtı diye sunulmadı. Rapor ECOLOGY-BASELINE-v0.3.md'de.

Paket release-v0.3; baslat.sh oraya yönelir. Önceki release klasörleri ve gerçek worlds klasörü korundu. Screenshotlar artifacts/v0.3 altında. GPU donanım FPS sonucu henüz raporlanmadı; yazılımsal screenshot zamanı GPU performansı sayılmaz.
