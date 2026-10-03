# EVOSIM v0.3 — Ekoloji başlangıç ölçümü

**Tarih:** 15 Eylül 2026. **Kapsam:** 20 eşleştirilmiş seed, 40 koşu, koşu başına 600 simülasyon saniyesi. **Sonuç:** Bütün koşular mevcut runner'ın durum, hareket, kaynak, doğum bütçesi ve soy kontrollerini geçti. Nüfuslar her iki koşulda da belirgin biçimde küçüldü; bu ufukta tam yok oluş gözlenmedi.

Bu belge v0.3 görsel sürümünün dayandığı **rastgele başlatılan, kalıtsal sinir ağı motorları** için başlangıç ölçümüdür. Eski hazır fayda/eylem seçicisinin sonuçları bu tabloya katılmadı. Çekirdek, ayarlar, paketler, kayıtlar veya plan değiştirilmedi. Kaçış, engelden kaçınma, öğrenme ya da ekolojik denge geliştiği iddia edilmiyor.

## 1. Deney düzeni ve tekrar üretilebilirlik

| Alan | Kullanılan değer |
|---|---|
| Başlangıç | `World(seed)`; derlenmiş `WorldConfig` varsayılanları |
| Dünya | 128 × 128; nehirle ayrılmış 3 kara bölgesi |
| Kurucular | 150 otçul + 15 etçil; her koşuda toplam 165 |
| Karar ağı | 30 girdi, 8 tekrarlayan birim, 5 motor çıktısı; 357 kalıtsal ağırlık |
| Genom | 6 beden özelliği; ağırlıklar da ebeveyn aralığından sürekli kalıtılır |
| Mutasyon | Doğum başına toplam %0,1; değiştirilmedi |
| Ağaç/meyve | 155 ağaç; ağaç başına 7 meyve; meyve başına 18 ham enerji |
| Çayır ayarı | `MeadowCount=13`; nem filtresi nedeniyle bu, 13 gerçekleşmiş alan garantisi değildir |
| Yaş | Kurucu yaşı 120–360 saniye; iki grubun ömrü 1.200 saniye; yaşlanma açık |
| Süre | 12.000 tick × 0,05 saniye = 600 saniye |
| Normal kol | Kuraklık kapalı |
| Kurak kol | 4.000 tick tamamlandıktan sonra, 200. saniyede kuraklık açılır; sonraki tick'ten itibaren etkiler |
| Örnekleme | Her 1.200 tick / 60 saniye |
| Çalıştırma | En fazla 2 eşzamanlı CPU işlemi; mevcut Release runner'ın sabitlenmiş kopyası |

Seed listesi ölçüm öncesinde belirlendi: `42, 7, 135, 2026, 7919, 256, 512, 1024, 4096, 10007, 12345, 16384, 23456, 32768, 45678, 54321, 65535, 77777, 99991, 104729`. İlk beş çiftin koşu süreleri yaklaşık 15–18 saniye olduğundan çalışma 20 çifte genişletildi. Ölçümler 363 saniyede, yaklaşık 6,1 dakikada tamamlandı.

Her çift aynı seed ve aynı ayarlarla başladı. Başlangıç nüfusu, başlangıç kaynak stoku ve kuraklıktan önceki 60, 120, 180. saniye örnekleri **20 çiftin tamamında birebir eşleşti**. Farklı seed'ler haritayı, bedenleri ve ilk ağları birlikte değiştirir; bu deney bunların tek tek etkilerini ayırmaz.

Çalıştırılan Core binary SHA256:

```text
d7a421821220ca2f91da9c734c5d17f4867a2bca496189203b5438c7846c9cc0
```

Binary listesi ve kaynak parmak izleri [deney manifestinde](../artifacts/v0.3/baseline/experiment-manifest.json) saklıdır. Her ham sonuç, komut satırı ve Core hash'i içerir; yanındaki `.meta.json` dosyası başlangıç/bitiş UTC zamanı, çıkış kodu, gerçek süre ve çıktı hash'ini kaydeder. Standart hata çıktıları da korunmuştur.

Eşdeğer runner komutları:

```bash
dotnet EvoSim.Checks.dll --ticks 12000 --seed 42
dotnet EvoSim.Checks.dll --ticks 12000 --seed 42 --drought-at 4000
```

Buradaki `dotnet`, ölçüm sırasında kullanılan .NET 10.0.12 runtime'ını temsil eder. Tam makine yolları her kayıtta mevcuttur. Aynı model için yeniden çalışma yapılırken Core hash'i ve ayarlar eşleştirilmelidir.

## 2. Toplu sonuçlar

Her hücre **20 seed'in medyanı [minimum–maksimum]** biçimindedir. Aralıklar güven aralığı değildir.

| Ölçüm | Normal | Kuraklık |
|---|---:|---:|
| Son yaşayan nüfus | **15,5 [9–21]** | **10,5 [6–18]** |
| Doğum | 22,5 [16–29] | 21,5 [17–29] |
| Ölüm | 173 [162–178] | 176,5 [168–186] |
| En yüksek soy nesli | 1 [1–2] | 1 [1–2] |
| 180. saniyeden sonra doğum | 1 [0–5] | 1,5 [0–5] |
| Son anda 5 saniyeden uzun süredir engellenen yaşayan birey | 0,5 [0–3] | 0 [0–3] |
| Bu bireylerin son yaşayan nüfusa oranı | %2,63 [%0–14,29] | %0 [%0–25] |
| Yaşayanların ortalama yaşam boyu hareket mesafesi | 592,90 [471,20–706,26] | 583,93 [441,96–702,58] |
| Son toplam bitkisel stok: ot + meyve | 16.253,35 [15.837,61–16.841,41] | 15.336,60 [14.555,74–16.034,79] |
| Son meyve stoku | 15.393,25 [14.940,84–15.877,01] | 15.127,80 [14.265,46–15.895,92] |
| Son ot stoku | 888,03 [547,25–1.283,89] | 221,32 [138,87–299,45] |
| Kalan ceset adedi | 4,5 [2–11] | 5 [1–10] |
| Tam nüfus yok oluşu | 0 / 20 | 0 / 20 |

Nesil sayımı kurucuları **0** kabul eder. `maxGeneration`, ölüler dahil bütün soy kayıtları içindeki en yüksek değerdir; son anda yaşayan nesillerin derinliği değildir. Hareket mesafesi harita birimidir. Kaynaklar ham enerji birimidir; sindirimle kazanılan enerjiyle eşdeğer değildir.

Eşleştirilmiş **kurak − normal** son nüfus farkı medyanı **−3,5**, aralığı **−10 ile +2** oldu. Kurak kolda son nüfus 16 seed'de daha düşük, 3 seed'de eşit, 1 seed'de daha yüksekti. Son istisna seed 7919'dur: normal 14, kurak 16 canlı. Doğum farkının eşleştirilmiş medyanı 0, aralığı −3 ile +4'tür. Tek bir seed'deki ters yönlü sonuç genel veya biyolojik bir yarar iddiasına dönüştürülemez.

![Normal ve kurak koşuların nüfus eğrileri ve eşleştirilmiş son nüfusları](../artifacts/v0.3/baseline/population-baseline.png)

Soldaki çizgiler medyanı, boyalı alanlar seed minimum–maksimum aralığını gösterir. Sağda her nokta bir çifttir; aynı sonuçları veren seed'ler üst üste gelebilir. Çapraz kesikli çizgi iki kolun eşit son nüfusunu gösterir.

40 koşuda toplam 891 doğum kaydı ve 0 mutasyon olayı görüldü. Eşleştirilmiş kollar kuraklığa kadar aynı geçmişi paylaştığından bu toplam, 891 bağımsız genetik örnek gibi yorumlanmamalıdır. Bu kısa koşu kümesi bir mutasyon oranı testi değildir; oran değiştirilmedi.

## 3. Verinin söylediği ve söylemediği

**Nüfus yenilenmesi bu ufukta zayıf.** Bütün seed'lerde 600. saniye nüfusu 165 kişilik başlangıcın çok altında kaldı. Doğum var, ancak soylar çoğunlukla ilk yavru neslinin ötesine ilerlemiyor. İlk 180 saniyeden sonraki doğumların azlığı sonraki incelemenin temel işaretidir. Bu, gelecekte bütün koşuların kesin yok olacağını veya uzun vadeli bir dengeye ulaşıldığını göstermez.

**Toplam besin sıfırlanmadığı halde nüfus düşüyor.** Son stokta meyve baskın. Bu gözlem, erişilebilir yerel besin, su, sindirim verimi ve motor davranışının ayrı ölçülmesini gerektiriyor. Haritanın toplamında besin kalması, her bireyin yeterli kullanılabilir besine eriştiğini kanıtlamaz. Mevcut stok ölçümü üretim/tüketim akışını, ceset enerjisini veya kaynaklara uzaklığı içermez.

**Kuraklığın birden fazla etkisi aynı anda açık.** Mevcut model su seviyesi üzerinden nemi, ot kapasitesi/yenilenmesini ve meyve yeniden oluşma hızını değiştirir; ayrıca bedenin su kaybını artırır ve içme hızını düşürür. Su hücrelerinin geometrisi bu modelde değişmez. Bu çalışma toplam kuraklık etkisini karşılaştırır; tek tek etkiler ayrıştırılmadı.

**Engellenme sonucu bir beceri ölçümü değildir.** `BlockedSeconds` yalnız hareket motoru güçlü bir sinyal verirken gerçek adım çok küçükse artar; diğer durumda sıfırlanır. Rapor, sadece son anda hayatta olanlarda kesintisiz 5 saniyeyi aşanları sayar. Daha önce sıkışıp ölenler, kısa temaslar, dönerek aynı yerde kalma ve tüm yaşam boyunca engellenen toplam süre bu sütunda yoktur. Düşük son sayıdan kaçınmanın evrimleştiği sonucu çıkarılamaz. Hayatta kalanların mesafesi de ölenleri dışladığından başarı veya rota verimi değildir.

**Ölüm nedenleri bu runner çıktısında ayrılmıyor.** Varsayılan kurucu yaşı en fazla 360, koşu süresi 600 ve ömür 1.200 saniye olduğundan bu deney ufkunda yaş sınırından ölüm beklenmez; yaşlanmanın hız ve iyileşme etkileri yine açıktır. Açlık, susuzluk ve çatışmanın katkılarını mevcut toplu kayıtlardan sayısal olarak ayıramayız.

## 4. Koddan görülen olası darboğazlar

Aşağıdakiler **kod incelemesine dayanan mekanizma açıklamalarıdır**; bu deney belirli bir kapının toplam nüfus kaybına katkı yüzdesini ölçmedi.

### Üreme: aynı anda birden çok kapının geçilmesi

[`ReadyToBreed` ve `ResolveBirths`](../src/EvoSim.Core/Simulation.cs) yaşayan, olgun, doku gelişimi yeterli, beklemesi bitmiş, enerji oranı en az %67, suyu 35'in üzerinde, sağlığı 65'in üzerinde birey ister. Her iki bireyin üreme motor çıktısı pozitif olmalı; grup uyumu, yakın temas ve görüş şartları da geçmelidir. Sonra yavru için 16 aday kara noktası aranır; doğumun enerji/doku bütçesi iki ebeveynden ödenir. Hareketi veya eşe yaklaşmayı hazır bir politika sağlamaz.

Bu nedenle yalnız bir üreme güdüsü ayarının yükseltilmesi doğumu garanti etmez. Önce **fizyolojik uygunluk → olumlu ağ sinyali → uygun eşle temas → geçerli yavru yeri → fonlanan doğum** zincirinin hangi aşamada daraldığı ölçülmelidir.

### Kaynak ve su: yakın olmak ile stok bulunması farklı

[`Decide` ve `ResolveInteractions`](../src/EvoSim.Core/Simulation.cs) görünür kaynakları algılar; yeme/içme refleksi yalnız fiziksel erişimde çalışır. Besin için yakınlık sınırı yaklaşık 0,9 harita birimidir; içme uygun kıyı noktasına 1,5 birim yakınlık ister. Ağız en yakın yenebilir öğeyi seçer, sindirim bakımından en verimli öğeyi stratejik olarak seçmez. Bitki ve et verimleri beslenme genine göre değişir.

[`Terrain`](../src/EvoSim.Core/Terrain.cs) çayırları sınırlı yamalara, ağaçları nemle ilişkili noktalara dağıtır. Su ve besine ulaşabilen fakat birbirine rastlamayan bireyler yaşayabilir, yine de soy devamı zayıf kalabilir. Bu bir aday açıklamadır; yakınlık/temas ve gerçek enerji alımı kayıtlarıyla sınanmalıdır.

### Ağ: kalıtım var, yaşam içinde eğitim yok

[`Brain`](../src/EvoSim.Core/Brain.cs) motorları 357 ağırlıkla üretir. En yakın görünen bireyin sinyali algıya girer; birden fazla eş adayını veya kaçış rotasını hazır biçimde sıralayan mekanizma yoktur. Genom ve ağ aralıktan kalıtılır, ancak bu kalıtım ebeveyn davranışını korumayı garanti etmez. Geçici ağ belleği, ağırlıkların yaşam boyunca eğitildiği anlamına gelmez.

## 5. v0.4 için önce ölçülecekler

1. **Üreme kapılarını ayrı say.** Uygun birey-zamanı, uygun ve pozitif sinyal veren birey-zamanı, aynı anda uygun olan uyumlu yakın çiftler, yerleşim reddi ve tamamlanan doğumları ayır. Çiftleri aynı tick'te iki kez sayma; nedenleri açık tanımla. Yüksek güdü ayarının etkisini doğrudan bu zincirde göster.
2. **Ölüm ve kaynak bütçesini bağla.** Ölüm nedenlerini ve katkıda bulunan enerji/su/hasar açığını; ot, meyve ve etten gerçekten kazanılan enerjiyi kaydet. Grup bazında son nüfus ve doğum/ölüm de eklensin. Global stok ile bireyin alımı ayrı kalsın.
3. **Temas ve erişimi yaşam boyunca izle.** Bütün bireylerde engellenen toplam süreyi, kaynak/kıyı mesafelerini, kaynakta geçirilen süreyi, eşle temas süresini ve yavrunun ilk besine/suya erişimini ölç. Son anda yaşayan küçük alt küme tek başına değerlendirme olmasın.
4. **Üreme güdüsü ve modlarını ölçülmüş karşı deneylerle ekle.** Planlanan kapalı/eşeyli/eşeysiz modlar, aynı seed ailesi ve açık enerji bütçeleriyle karşılaştırılsın. Güdü ağ girdisi olsun; ağın motor kararı veya fizyolojik kapılar atlanmasın. Yeni ağ girdisi için kayıt/şema geçişi açık olsun.
5. **Kaynak değişikliklerini tek tek sınayıp sonra birleştir.** Ağaç profili, meyve enerjisi/yenilenmesi, çayır yerleşimi ve su maliyetleri birlikte değiştirilirse nedeni ayırmak zorlaşır. Bu 20 seed ve binary hash, yeni sürüm için başlangıç karşılaştırması olarak saklansın. Mutasyon veya kaynaklar nüfusu kurtarmak için sessizce değiştirilmesin.

Bunlar önerilen sonraki işlerdir; bu ölçüm sırasında uygulanmadılar.

## 6. Dosyalar ve sınırlar

- [Makine tarafından okunabilir özet](../artifacts/v0.3/baseline/summary.json)
- [40 koşunun tamamı — CSV](../artifacts/v0.3/baseline/runs.csv)
- [20 eşleştirilmiş fark — CSV](../artifacts/v0.3/baseline/matched-pairs.csv)
- [Deney manifesti ve hash'ler](../artifacts/v0.3/baseline/experiment-manifest.json)
- [Nüfus grafiği — PNG](../artifacts/v0.3/baseline/population-baseline.png) · [SVG](../artifacts/v0.3/baseline/population-baseline.svg)
- Aynı klasörde her `seed-......-normal.json` / `seed-......-drought.json` ham sonucu, işlem metadatası ve stderr dosyası bulunur.

Bu, 20 önceden belirlenmiş seed üzerindeki sonlu bir karşılaştırmadır; nüfusun bütün olası dünyalardaki dağılımı veya bir güven aralığı tahmini değildir. Aynı anda iki işlem, her tick'te denetim ve başka geliştirme işleri çalıştığından kayıtlardaki süreler izole donanım performans testi sayılmaz. Bütün koşuların invariant kontrollerini geçmesi, bütün gelecek mekaniklerinin tamamlandığı veya davranış modelinin biyolojik olarak doğrulandığı anlamına gelmez.
