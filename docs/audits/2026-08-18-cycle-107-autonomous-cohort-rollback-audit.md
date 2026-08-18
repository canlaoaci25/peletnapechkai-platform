# Çevrim 107 — otonom cohort rollback denetimi

## Görünür operasyon sonucu

Otonom teslimatın gerçek yürütme yolu artık production Web ve API sürümlerini tek cohort kimliğiyle
ilişkilendirir. API dağıtımından sonra Web dağıtımı veya birleşik production sağlık kapısı başarısız
olursa, yalnız deploy betiklerinin döndürdüğü ve rollback betiğinin kök/ad sözleşmesiyle yeniden
doğruladığı artefaktlar koordineli biçimde geri alınır. Böylece admin deployment journal aynı sürüm
kohortunu bileşen bazında izleyebilir ve split-release riski azalır.

## Kök neden ve kapsam

Çevrim 106 koordineli rollback'i `Promote-BoeclRelease.ps1` içinde uygulamıştı. Ancak zamanlanmış
otonom sistemin gerçek yolu `Invoke-BoeclAutonomousCycle.ps1` içinde API ve Web deploy betiklerini
ayrı kimliklerle çağırıyor, Web/final-health hatasında cohort rollback çalıştırmıyordu. Bu çevrim,
korumayı gerçek yürütme yoluna taşıdı ve regresyon kapısını aynı dosyaya bağladı.

Değişiklik yalnız uygulama artefaktlarını kapsar. Database migration otomatik geri alınmaz; gerçek
sınırlı trafik canary'si, ileri/geri şema uyumluluk kapısı, staging rollback tatbikatı ve production
sağlık/journal kanıtı tamamlanmadan `canary-cohort-rollback` workstream'i completed değildir.

## Güvenlik ve rollback

- Rollback yalnız deploy sonucunda dönen mevcut artefakt için denenir.
- Repo tarafından yönetilen rollback betiği path'i deployment root altında ve beklenen isimle
  doğrulamadan IIS/service mutasyonu yapmaz.
- Staging kohortu sağlık kontrolünü geçmeden production backup, migration veya deploy başlamaz.
- GitHub, remote kimlik doğrulama, push, IIS, staging veya production mutasyonu yapılmadı.

## Yerel kanıt

- `ops/tests/Test-DatabaseBackupPromotion.ps1`: gerçek autonomous runner'da staging sırası, ortak
  production cohort ve deploy sonrası koordineli rollback sözleşmesi.
- `ops/tests/Test-ReleasePromotion.ps1`: yardımcı promotion yolu ve unsafe rollback path reddi.
- `ops/windows/Test-PowerShellScripts.ps1`: tüm operasyon PowerShell dosyaları parse PASS.
- `git diff --check`: PASS.

## Uzman kalite kapısı

- FULLSTACK: Görsel service provenance dilimini sonraki bağımlı P1 olarak belirledi; dış sağlayıcı
  owner kararı olmadan kapalı kalmalı.
- DESIGNER: Public planlama merkezinin canlı render kanıtı eksik; bu çevrimde yeni UI yazımı önermedi.
- EDITOR: Release sözleşme kaymasının locale, canonical/hreflang, yayın durumu ve görsel metadata
  bütünlüğünü etkilediğini doğruladı; runner düzeltmesini önceliklendirdi.
- SYSADMIN: Yardımcı promotion ile gerçek autonomous yol arasındaki split-release açığını REJECT
  bulgusu olarak verdi; bu düzeltme bulgunun yerel kod kısmını kapattı.
- DIRECTOR: Yerel checkpoint için **PASS**; workstream tamamlanması için **HOLD**.
