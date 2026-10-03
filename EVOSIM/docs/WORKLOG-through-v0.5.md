# EVOSIM devam günlüğü

## Güncel teslim — v0.5.0 / 15 Eylül 2026

Kullanıcı 16 Eylül 09:00 İstanbul'a kadar geliştirme yetkisi verdi. Mevcut saatlik heartbeat `evosim-geli-tirme-sabaha-kadar` bu görevi sürdürür. Aktif çalışma varsa aynı işi yeniden başlatma. Kullanıcı telefondan izliyor: anlamlı görsel teslimlerde gerçek screenshot, sık bildirim yok.

**Çalışan sürüm:** `baslat.sh` → `release-v0.5`. Kaynak ve binary donma paketi `outputs/EVOSIM-0.5.0-linux-x64.zip`. Önceki v0.4.1/v0.4/v0.3 arşivleri korunur. Açık eski uygulamaları kapatma; gerçek kullanıcı dünyalarını test için kullanma.

**Tamamlandı:** Terra Nil yönünde açık doğa UI, minimal3 kontrol;85 sayısal dünya ayarı/6gen yardım;bol kurulum-scroll-sürgü;üreme modu/istek;ağaç parametreleri;128/192/256harita;dünya ad/sil/geri al;gerçek geçmiş/oynatma/ayrı dal. v0.5 Core SHA `50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033`, v0.4.1 ile aynı. Hazır kaçış/av/eşe gitme politikası YOK. Mutasyon doğum başına toplam.001 varsayılan.

**v0.5 doğrulama:** bağımsız10/10timelinegrubu,ayrı7/7storegrubu,yayınUI smoke;28seek/1800ticktamcheckpoint eşitliği;aynı-tick komutlar,iptal,cachetrim,dal,parentkoruması,legacy333tickbaşlangıç,bozukarşivler,sil/restore. UI regression:panning,popupkatmanı,wheelbirikimi,modaldragtemizliği,kaydetmehatasındakapanmama. `artifacts/v0.5/ui-smoke.log` içindeki InvalidOperationException bilerek silinen izole kayda Save denenmesidir; test uygulamanın açık kaldığını doğrular. NihaiJSON passed=true.

**Kayıt mimarisi:** `EVOSIM-LIBRARY-2`, headCorecheckpoint+timelinearchive tekatomikdosya/.bak. `EVOSIM-TIMELINE-1` gerçekCoreSHA'yalı;origin+fullcommandlogkorunur,600ticksnapshot/32MiBopsiyonelcache. `HeadWorld` gerçeksonan; `DisplayWorld` geçmiş;autosaveyalnızhead. Komutşuankuraklıkaç/kapat. GeçmişmüdahalesiyeniIDdal. LIBRARY1tümimports ayrıkopya,earliest=importtick. ModeldeğişinceeskireplayaynıCoreolmadanyürütülmez;gelecektekiCoregeçişindeaçıkmigrationstratejisigerekiyor.

**Ekranlar:** `artifacts/v0.5/world-timeline.png` yayınbinary'sinden256harita500kurucu/350ağaç;head1800(90sn),view1200(60sn),480canlı/33doğum/53ölüm. Kullanıcıya bu milestone ekranı gönderilecek/gönderildi. Önceki world-256.png tekrar gönderme.

**Sonraki dilim:** v0.5.x gözlem araçları (kaynak inceleme,soy grafiği,gen dağılımları/gerçekolaylar). Bunlar Core'u değiştirmeden yapılabilir. F67/F68/F71henüztamamdeğil. Çevre/mevsim/afet/inşa/GPU ilerikapılar. Bütün planı bitmiş diye sunma. v0.5engine/store/verifierajanlarıtamamlandı,rootyenisahiplikatayacak.

**Araçlar:** SDK `work/toolchain/dotnet/dotnet`; SDLlib `work/toolchain/sdl/usr/lib/x86_64-linux-gnu` veya yayımlanmışklasör. UIcaptureappCLIile:`--capture work/example.bmp --timeline --ticks1800 --seek-tick1200` (argüman/değerlerarasıboşluk);`--ui-smoke --out work/isolated --legacy-fixture work/v041-checkpoint/fixtures/v04-t1201.bin`. BMP→PNG Pillow veview_image. NativeOSUIaracıyok. Yeni releaseayrıklasör;testlergeçmedenlauncher'esdokunma.

## Aktif sahiplik — v0.5.1 gözlem panelleri

v0.5.0 zipi ve yayınbinarysi donduruldu; source şimdi v0.5.1'eilerliyor. Core SHA aynı kalacak, eski geçmişler de açılacak.

- Root: Program/SetupPanel/TimelinePanel/Capture/Native entegrasyonu, ayrıntılıcanlıkartı, yeni ObservationUiSmoke, gerçekekranlar, yayıntesti ve paket.
- visual_architecture_plan: YENİ ResourceInspection.cs + ResourcePanel.cs, puredata/tıklanabilir ağaç/ot/su/kaya/sığınak/ceset incelemesi. KaynakseçimAPI/rootçağrıları. work/v051-resource testleri.
- roadmap_audit: YENİ LineageIndex.cs + LineagePanel.cs,benzersizDAGaltsoy/ebeveyn/çocuk/torunveyaşayanistatistiği; yerelgrafik,pagedçocuklar,tıklanabilirfokus. work/v051-lineage testleri.
- minimal_behavior_review: YENİ PopulationInsights.cs + PopulationInsightsPanel.cs;yaşayangen-dağılımlarıgünceldietgeninesınıflanır,altıhistogramvegerçekaging/maturity;saltokunurtestfixture/checkpointdeğişmezliği. work/v051-insights.

Hiçbir ajan mevcut Core/Timeline/WorldStore dosyalarına dokunmaz. Panellergösterilen `world`üokur,HeadWorldgeleceğinibakmaz. Rapor/arayüzde öğrenmeveadaptasyonkanıtıolarakdağılımdeğişikliğiiddiasıyok. Gerçekgörüntülerdenüfus/soyuydurma;seedkoşularındakigerçekdoğumkullan.

## Önceki çalışma notları (tarihsel; yukarıdaki güncel durum öncelikli)


## 15 Eylül 2026 — v0.4.0 teslim edildi, büyük harita hazırlığı

Kullanıcı 16 Eylül sabahına kadar çalışma yetkisi verdi. Saatlik heartbeat `evosim-geli-tirme-sabaha-kadar`; bitiş 16 Eylül 09:00 İstanbul. Kullanıcı telefondan izliyor: yalnız anlamlı görsel teslimde gerçek screenshot paylaş; sık bildirim yok. Aktif geliştirme varsa aynı işi tekrar başlatma.

Plan: `outputs/plan.md`; F01–F72 + U01–U13 `docs/FEATURE-BACKLOG.md`. Terra Nil sanat yönü kesin. Pusula yönleri iptal: otomatik tüm-canlı taç saydamlığı/kontur uygulanıyor. 0–1 sürgü, gen min/max çift sürgü; ömür/adet numeric. Kurulum bol/scroll, simülasyon minimal. Hazır kaçış/av/çiftleşme yönlendirmesi ekleme: hareket sinir ağına ait. Mutasyon varsayılanı doğum başına toplam .001.

### Çalışan sürüm / tamamlanan kapı

`baslat.sh` release-v0.4 açıyor. v0.4.0 18 Core + 8 checkpoint grubu ve yayın UI smoke geçti. Üreme modu/istek, ağaç koşulları, üreme tanısı ve eski kayda ayrı sürüm dalı hazır. v0.4.0 arşivi kaynak/çalıştırılabilir dondurulmuş geri dönüş noktasıdır. `outputs/EVOSIM-0.3.0-linux-x64.zip` kaynak+çalıştırılabilir dondurulmuş paket. Açık tema, ağaç/taş sanatı, tüm canlı taç saydamlığı+kontur, bilgi balonları, sürgüler/scroll, menü miniharita önizlemeleri, dünya sil/geri al tamamlandı. v0.3 Core v0.2.1 ile aynıydı; v0.4 yeni mekaniklerle genişledi; 40 koşulu 20 eşli-tohum temel ekoloji deneyi arşivde. Gerçek dünya ve ayar screenshotları telefona gönderildi. Aynı görüntüleri tekrar gönderme.

### Aktif sahiplik — v0.5 gerçek geçmiş ve dallanma

v0.4.1 kaynak + çalıştırılabilir arşivi doğrulandı: `outputs/EVOSIM-0.4.1-linux-x64.zip`. Launcher hâlâ release-v0.4.1. Core biyolojisi bu dilimde DEĞİŞMEZ; hash 50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033. Kullanıcıya son gönderilen SS v0.4.1/world-256.png; 500 kurucu, 30 sn sonra496 yaşayan/9doğum/13ölüm.

- Root: yeni Observer/TimelinePanel.cs; Program/SetupPanel/Native/LargeMapUiSmoke entegrasyonu, UI akış testi, paket ve belgeler. Üç sabit düğme korunur; Geçmiş menüden açılır. Playback geçmişte kayıtlı geleceği oynatır; Buradan devam yeni ID oluşturur. SaveCurrent yalnız session.HeadWorld + SerializeArchive kaydeder.
- `/root/roadmap_audit`: YENİ Observer/TimelineSession.cs ve TimelineArchive.cs. Origin + tam komut listesi + 600tick hızlandırıcı snapshot, 32MiB cache, aynı-tick NextCommandIndex. Seek cooperative6ms/100tick; iptal ve ayrı head/display; CreateBranch bağımsız origin/prefix. Sıkıştırma/strict hash/schema; dosya I/O yok. Host TimelineArchive.ConfigureCoreHash ile DLL hash'ini verir.
- `/root/visual_architecture_plan`: yalnız Observer/WorldStore.cs. EVOSIM-LIBRARY-2 tek atomikdosyada head + timelineblob; .bak, sil/geri al aynı dosyayı kapsar. LIBRARY-1 tüm imports ayrıv0.5ID; origin=importtick. Save son optional byte[] timelineArchive; mevcut geçmişsiz overwrite reddedilir. OpenForCurrentVersion tuple(World,Slot,Migrated,TimelineArchive). Storeconstructor gerçekCorehashkonfigureder. AynıID ile headgerilemesini/earliestdeğişmesini reddeder.
- `/root/minimal_behavior_review`: yalnız work/v05-tests bağımsız testharness. FrozenCore1800tick/13komut/22checkpoint fixture; aynı-tickkomutlar, seek/cancel/branch/reopen/cachetrim/corruption/atomikstore testleri. Core/Checks kaynakları değiştirilmez; aynıbinary için23Coretesti tekrar gerekmez.

Mimari kararı: geçmiş ayrıklasörlerde değildir; timelinearchive headcheckpoint ile aynı .evoworld LIBRARY-2 atomik konteynerindedir. Böylece head/log/branch/sil/restore çokdosyalıtransaction'a dönüşmez. Origin ve fullcommandlog cachebütçesinden silinmez; yalnız isteğe bağlı hızlandırıcılar atılır. Core/modelSHAeşleşmeyen geçmiş sessizce yürütülmez. NormalUIyerelde otomatik savesession.HeadWorld; geçmişönizlemehead'i ASLA ezmez.

### Araçlar

SDK `work/toolchain/dotnet/dotnet` (10.0.401). SDL libs `work/toolchain/sdl/usr/lib/x86_64-linux-gnu`. .NET selfcontained publish, eski release assets/font ve SDL libs kopyala. Uygulama `--capture work/name.bmp --ticks 300 --zoom 1.8`, `--setup --setup-tab 1`, `--main-menu`, `--ui-smoke --out work/isolated` seçenekleri var. BMP→PNG Pillow; view_image ile incele. UI hidden capture SDL dummy kullanır. Native computer UI aracı yok; uygulamanın kendi CLI capture/smoke yolunu kullan. Gerçek kayıtları testte silme; çalışan eski release dosyalarını ezme.

### Sonraki kapılar

1. v0.4.1 Core/Observer + yeni büyük harita testleri ve eski fixturelar. Şu an Corebuildtemiz, UIbuild/smokebekliyor.
2. CPU eski/yeni karşılaştırma, actual256harita screenshotları, release-v0.4.1 ayrıpaket. Önceki v0.4 kaynak zipi donduruldu; yeni editler v0.4 yayımlanmışbinary'sinieztirmiyor.
3. Sonraki dilim: gerçek zaman çizelgesi/snapshot+komutlogu/geri sar/ayrıdal. Ardından çevre/mevsim mekanikleri. Roadmaptümünü bitmiş gibi sunma.

Büyük harita geçiş raporu: `work/map-size-audit.md`. Screenshot ensonv0.4world-overview +setup-trees gönderildi. Birsonraki anlamlıSS256harita veya replaydir; küçük değişikliklerde tekrar gönderme.

## Yayın öncesi düzeltme kapısı — 15 Eylül 13:35 İstanbul civarı

Eski128 üreticisinde nehir genişliği .4/2 ile 3 yerine4–9 kara bölgesi oluşan eski birkusur saptandı. Yeni dünyalarda bunu bilerek göndermemek için GeneratorVersion2 varsayılan oluyor. Generator1 yalnız128eski kayıtların tam geleceğini korur; Generator2 tüm128/192/256boyutlarda ince tributaryraster/sliveronarımlarını uygular. Eski kayıtlar1olarak yüklenir; yeniKurulum draftını root2yapar. CheckpointformatnewWORLD-2henüzyayımlanmadığıiçinaynı; v0.4arşiviyinekorunuyor. Önceki23+12test ve CPUsonuçları öncekiCorehashc4685...revizyonuydu; agentlarfinalgen2sonrasıyeniilkkoşumkanıtlarınıkoruyarakyenileyecek. RootNativepencerebaşlığı0.4.1düzeltildi. v0.4.1 henüzpublishyok; launcherhâlâv0.4.

## v0.4.1 teslim kapısı tamamlandı

Launcher release-v0.4.1 oldu. Nihai Core SHA50e63b1ef6fe985e8849d5ac55b27da9db0428f51bd3ca008f79785277459033. 23/23Core +13/13checkpoint,225gen2map +50gen1legacydigest +yayınUISmoke geçti. Yeni256harita500kurucu/350ağaç30simsecSSartifacts/v0.4.1/world-256.png veclose.png:496yaşayan9doğum13ölüm. KısaCPUgen1baseline4.73s→1.15s/2000tick≈4.11×;sharedCPU/renderer-yok ölçümü. Tüm önceki sürümrevizyonkanıtları ayrıklasörlerde; yalnıznihaimanifesthashkullan.

Yeni kaynakdiliminegeçmedenEVOSIM-0.4.1-linux-x64.zip dondurulur. Sonraki işgerçekzamançizelgesi. Tasarımhazır: work/timeline-contract.md; henüzuygulanmadı. SnapshotNextCommandIndexşart;livehead/previewworldayrı;autosavegeçmişleheadiezmez. Origin+fullcommandlogsakla;cachetrimyalnızhızlandırıcılar. GeçmiştenayrıIDyeni dal; eskiimportearliest=importtick. Rootbirsonrakiaktifajanatamasınıgünlüğeekleyecek.
