# Cycle 98 — kalıcı görsel odak noktası denetimi

## Görünür sonuç

Görsel Yenileme Stüdyosu artık aday görselin ana öznesini yalnız merkez kırpmasına mahkûm etmez. Editör görsel üzerinde dokunarak/tıklayarak veya iki klavye uyumlu aralık kontrolüyle odak seçer; hero, lead, mobil, atlas ve akış oranlarını aynı koordinatla önizler. Onaylanan koordinat medya varlığına kaydedilir ve ana sayfa ile makale kapağında güvenli merkez varsayımıyla uygulanır.

## Güvenlik ve veri

- `focal_x` ve `focal_y` nullable, `numeric(5,4)` ve birlikte `0..1` aralığında olma constraint'iyle additive migration olarak eklenir.
- Eski medya kayıtları değiştirilmez; null değer public render'da `%50 %50` olur.
- Aday kanıtı koordinatları audit kaydına dahil eder. Geçersiz API değerleri domain katmanında reddedilir.
- Rollback önce eski uygulama sürümüne dönüş ve kolonları korumadır. Kolon düşürme veri kaybettirebileceği için rutin rollback değildir.

## Director kalite kararı

Yerel ürün dilimi **PASS**. FULLSTACK ve DESIGNER uygulanabilirlikte focal diliminde birleşti. EDITOR, worker generation-context/provenance boşluğu nedeniyle genel `visual-service` için **REJECT** verdi. SYSADMIN, staging backup/restore/migration ve gerçek render kanıtı olmadan production için **REJECT** verdi. Bu nedenle `visual-focal-crops` workstream'i birleşik planda pending kalır; yalnız yerel kanıt eklenmiştir.

## Kalan canlı kabul

Staging'de exact backup restore testi, migration, 375/768/1440 açık-koyu admin/public render; ardından production backup, migration, health ve focal smoke kanıtı gerekir. GitHub/push/remote kimlik doğrulama geçici owner kararı gereği yapılmaz.
