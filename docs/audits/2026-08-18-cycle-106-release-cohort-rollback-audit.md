# Cycle 106 — release cohort rollback denetimi

## Hedef

Production Web ve API sürümlerinin kısmi terfi sonrasında farklı commit kohortlarında kalmasını önlemek; staging kapısını zorunlu tutmak ve önceki sağlıklı uygulama kohortuna tek eylemle, yol doğrulamalı geri dönüş sağlamak.

## Önce / sonra

- Önce: API ve Web ayrı deployment kimliğiyle sırayla terfi ediyor; Web veya birleşik production sağlık kapısı başarısız olursa API yeni sürümde kalabiliyordu.
- Sonra: İki bileşen aynı `cohort-*` kimliğini taşır. Web terfisi ya da son sağlık kapısı başarısız olduğunda koordineli rollback önceki artefaktları geri yükler ve birleşik sağlık kapısını çalıştırır. `-SkipStaging` production için fail-closed reddedilir.

## Güvenlik ve geri dönüş

Rollback yalnız açıkça verilen, `C:\inetpub\peletnapechkai` altında kalan ve `.web-rollback-*` / `.api-rollback-*` ad sözleşmesine uyan mevcut klasörleri kabul eder. En az bir geçerli artefakt olmadan servis mutasyonu başlamaz. Mevcut başarısız sürüm ayrı cohort karantinasına taşınır. Veritabanı şeması otomatik geri alınmaz; bu dilim yalnız uygulama artefakt kohortunu kapsar.

## Kanıt

- `ops/tests/Test-ReleasePromotion.ps1`: staging zorunluluğu, ortak kohort kimliği, koordineli rollback çağrısı ve root dışı artefakt reddi.
- `ops/tests/Test-DeploymentJournal.ps1`: bileşen bazlı kalıcı journal ve mesaj redaksiyonu.
- `ops/windows/Test-PowerShellScripts.ps1`: bütün operasyon PowerShell dosyaları parse kapısından geçti.
- `git diff --check`: geçti.

## Director kalite kararı

Yerel dilim **PASS**. `canary-cohort-rollback` workstream'i henüz Completed değildir: sınırlı gerçek trafik kohortu, staging rollback tatbikatı, production sağlık/audit kanıtı ve veritabanı uyumluluk checkpoint'i gereklidir. Bu çevrimde staging veya production mutasyonu yapılmadı.
