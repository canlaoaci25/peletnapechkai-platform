# Çevrim 92 — Editoryal üretim ritmi

## Görünür önce / sonra

Önce admin komuta merkezi yalnız açık, geciken ve yaklaşan işleri gösteriyordu. Yönetici ekibin
gerçek tamamlanma hızını, teslim güvenilirliğini veya haftalık ritmini göremiyordu.

Sonra aynı yüzey; 30 ve 90 günlük ölçülen görev sayısını, zamanında tamamlanma oranını, p50/p95
çevrim süresini ve 13 haftalık üretim eğilimini gösterir. Üçten az örnek varsa rakamla yönlendirmek
yerine yetersiz kanıt durumu gösterilir.

## Veri ve güvenlik kararı

- `UpdatedAt` yeniden atama ve durum değişimlerinde de güncellendiği için tamamlanma kanıtı değildir.
- Yeni nullable `CompletedAt`, yalnız ilk `Completed` geçişinde yazılır; idempotent tekrar aynı kanıtı
  korur, yeniden açma kanıtı temizler ve sonraki tamamlama yeni zamanı kaydeder.
- Eski tamamlanmış satırlar tahmini backfill edilmez; admin ölçüm dışı kayıt sayısını açıklar.
- Son 90 günlük sorgu yalnız gerekli üç zamanı projekte eder ve partial index kullanır.
- Endpoint mevcut yetkili admin sözleşmesinde kalır; yeni mutasyon veya dış servis eklenmez.

## Rollback ve terfi

Kod rollback'inde nullable kolonun korunması veri kaybını önleyen tercihtir. Migration `Down` geliştirme
ve boş staging geri alma senaryosu içindir; gerçek yeni completion verisi yazıldıktan sonra production'da
kolon düşürülmemelidir. Runner, staging ve production öncesinde ayrı backup/checksum/izole restore
kanıtı ve migration smoke sağlamalıdır.

Bu worktree talimatı staging/production deploy'u runner'a bıraktığı için canlı URL doğrulaması bu
çevrim oturumunda yapılmaz ve yol haritası maddesi bu kanıt gelene kadar `blocked` kalır.
