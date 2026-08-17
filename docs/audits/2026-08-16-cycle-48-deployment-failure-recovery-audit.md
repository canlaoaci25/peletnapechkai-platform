# BOECL Çevrim 48 — Gözlemlenebilir Dağıtım Hata Kurtarması

## Görünür önce / sonra hedefi

Önceden Web veya API dağıtımı artifact hazırlama, `dotnet publish`, IIS modülü yükleme ya
da kopyalama aşamasında erken hata verdiğinde kalıcı kayıt `Started` durumunda kalabiliyor;
Sürüm Kurtarma Merkezi bu sessiz otomasyon arızasını güvenilirlik borcu olarak ayıramıyordu.
Artık `/{locale}/admin` 15 dakikadan uzun süre ilerlemeyen dağıtımları dört etkin dilde
yüksek görünürlüklü bir müdahale uyarısı olarak gösterir. Yeni erken hatalar terminal
`Failed` kaydıyla SLO ve geçmişe girer.

## Otomasyon ve kurtarma bütünlüğü

- Web ve API betiklerinin hazırlık aşamaları dış hata sınırı içindedir; servis değişimi
  öncesindeki bütün istisnalar sanitize edilmiş terminal `Failed` kaydı bırakır.
- Swap başladıktan sonraki mevcut sağlık kapısı ve otomatik rollback akışı korunur;
  `RolledBack` veya `RollbackFailed` sonucu ikinci kez `Failed` ile ezilmez.
- Admin ölçümü yalnız `Started` ve `Verifying` kayıtlarını, son güncellemeden 15 dakika
  geçtiğinde takılı kabul eder. Normal devam eden dağıtım yanlış alarm üretmez.
- Journal okuma boyut, şema ve izin verilen environment/component kontrollerini korur;
  hata mesajında secret sanitizasyonu değişmez.

## Kabul kanıtı ve kalan sınır

PowerShell ayrıştırma/journal regresyonu, API ölçüm testi ve dört locale admin görünüm
sözleşmesi bu davranışı kapsar. Tam lint, typecheck, production build, API testleri ve
Release build sonrasında staging ve production doğrulaması gereklidir. Harici uyarı
teslimi, iki kişi onaylı manuel rollback, journal saklama otomasyonu ve canary trafik
kapısı sonraki operasyon fazlarıdır.
