# Güncel uygulama: v0.2.0-neuro.1

Güncel sözleşme `DESIGN-v0.2.md` içindedir. Eski v0.1 utility ve otomatik kaçış sistemi kaynak koddan çıkarılmıştır; üretim kararları `Brain.Evaluate` üzerinden doğrudan motor komutlarına dönüşür.

- 30 girdi, 8 tekrarlayan gizli birim, 5 motor çıktısı, 357 kalıtsal ağırlık.
- Dünya içinde gerçek iki ebeveynli üreme ve gen aktarımı; başlangıç politikaları rastgele/eğitimsiz.
- Erken açlık/susuzluk basıncı ve temasla tüketme refleksi kullanıcı tarafından özellikle istenmiş beden kurallarıdır.
- Ayrı meyve stokları/sayaçları ve sınırlı düşük enerjili çayırlar.
- 90 saniyelik ceset sınırı, doygunlukla yaş, yaşa bağlı hız/ivme/iyileşme düşüşü.
- Eski araç API'sinin A* yardımcıları arazi kontrolleri için mevcut; ağın hedefini seçmezler.
- Gözlemci yerel SDL2; Unity entegrasyonu henüz yapılmadı. Bütün testler CPU referansı içindir.

Kabul raporları `artifacts/v0.2/` içindedir. Kaçış becerisinin gerçekten evrimleştiğinin gösterilmesi, ayrı çoklu seed ve davranış karşılaştırmaları gerektirir; bu sürüm bunu olmuş gibi sunmaz.

## Kurulum, kayıt ve ömür

WorldConfig: dokuz sekme, min/max gen aralıkları, dört preset, rastgele harita ve 1–5 kara bölgesi. Her soy grubu için bağımsız ömür (0=sınırsız) ve yaşlanma (0=kapalı). Checkpoint, RNG ve beyin belleği dahil tam durum ve ayarlar; WorldStore, adlandırılmış kayıtlar, SHA-256 kontrolü, atomik yazım, önceki yedek. 30 saniyede/normal kapanışta kayıt. 20× hedefi sabit adımla uygulanır.

14 çekirdek test grubu ve SDL olaylarıyla arayüz smoke testi son paketi doğrular. Kaydet-aç sonrası aynı geleceği üretme, farklı ömür kuralları ve rastgele harita bölge sayısı dahildir.
