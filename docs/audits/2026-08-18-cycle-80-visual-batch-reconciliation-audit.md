# Çevrim 80 — Görsel batch checkpoint uzlaştırma denetimi

## Önce / sonra hedefi

Önce, editör görsel adaylarını onaylayıp reddetse bile arşiv yenileme işi kendi
`CompletedItems` ve `FailedItems` checkpointlerini güncellemiyordu. Bütün görevler terminal
duruma geldiğinde batch `Running` kalıyor, 15 dakika sonra yanlış biçimde bayat görünüyordu.

Sonra, her aday ekleme ve editoryal durum değişimi aynı veri birimi içindeki batch ile
uzlaştırılır. Başarılı, reddedilen ve kalan toplamları batch toplamına eşit olmak zorundadır.
Son açık görev de sonuçlandığında iş otomatik `Completed` olur; admin operasyon kartı doğru
mesajı ve terminal durumu gösterir. Cancelled işler yeniden açılmaz.

## Rol denetimleri

- **FULLSTACK — PASS:** Kök neden, görev sonucu ile batch aggregate'ının ayrık yazılmasıydı.
  Domain invariant ve endpoint uzlaştırması eklendi.
- **DESIGNER — PASS:** Mevcut admin operasyon kartı yeni durum ve checkpoint mesajını ek UI
  yaması olmadan görünür kılıyor; yanlış bayat alarmı ortadan kalkıyor.
- **EDITOR — PASS:** `Approved` başarı, `Rejected` editoryal istisna olarak korunuyor;
  `InReview`, `Pending` ve `RetryRequested` kalan iş sayılıyor. Yayına otomatik kalite düşüşü yok.
- **SYSADMIN — PASS:** Uzlaştırma aynı EF Core unit-of-work/transaction sınırında kalıyor,
  toplam uyuşmazlığında fail-closed davranıyor ve cancelled işi değiştirmiyor.

## Kabul ve geri dönüş

- Ara checkpoint başarı/red sayılarını korur.
- Son terminal karar batch'i tamamlar ve tamamlanma zamanını kaydeder.
- Tutarsız toplam kabul edilmez.
- Rollback, bu commit'in geri alınmasıdır; şema veya production veri dönüşümü yoktur.

Harici görsel/vision sağlayıcısı ücret, lisans, secret ve veri aktarımı owner kararı gerektirdiği
için etkinleştirilmedi. Görsel servisinin roadmap maddesi bu nedenle `active` kalır.
