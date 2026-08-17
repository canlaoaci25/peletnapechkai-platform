# BOECL Çevrim 40 — Sürüm Güvenilirlik Merkezi

## Görünür önce / sonra hedefi

Önceden admin ana sayfası son dağıtımları listeliyor fakat operatöre başarı eğilimini,
tipik/kötü durum süresini veya sağlam sürüm serisini göstermiyordu. Artık `/{locale}/admin`
içindeki merkez son 50 tamamlanmış kayıttan başarı SLO'su, p50/p95 süre, otomatik kurtarma,
müdahale borcu ve kesintisiz sağlam seri üretir; dört locale ve mobil düzende görünür.

## Otomasyon ve hata kurtarma bütünlüğü

- Ölçüm yalnız tamamlanmış `Succeeded`, `RolledBack`, `RollbackFailed` ve `Failed`
  sonuçlarını kapsar; yarım dağıtımlar yanlış başarı oranı üretmez.
- Örneklem 50 kayıtla sınırlıdır; admin geçmişi son 12 kaydı göstermeyi sürdürür.
- Başarı oranı yalnız doğrudan başarılı sürümleri sayar. Otomatik rollback görünür bir
  kurtarma başarısıdır fakat yayın SLO'sunu yapay olarak yükseltmez.
- Bozuk, büyük veya izin verilmeyen journal girdileri mevcut güvenli okuyucu tarafından
  dışarıda bırakılmaya devam eder. Üretim verisi ve aktif sürüm değiştirilmez.

## Kabul kanıtı

- API birim testi başarı oranı, p50, p95, seri ve risk durumunu doğrular.
- Web regresyonu dört locale, SLO metrikleri ve responsive/risk stillerini doğrular.
- Lint, typecheck, production web build, tüm API testleri ve Release build kapıları çalışır.
- Commit/push sonrasında staging ve production Web/API dağıtımı ile canlı public ve admin
  veri sözleşmesi doğrulanmadan çevrim tamamlanmış sayılmaz.

## Kalan operasyon kilometre taşları

1. Eşik ihlallerinde güvenli, tekrarları bastırılmış uyarı teslimi.
2. İki kişi onaylı, yetkili ve audit izli elle rollback.
3. Journal ve eski release klasörleri için saklama/arşivleme politikası.
4. Web ve API için kademeli/canary trafik kapıları.
