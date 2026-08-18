# BOECL Çevrim 72 — Görsel operasyon kurtarma ve gerçek test kapısı

## Görünür önce / sonra hedefi

Görsel Yenileme Stüdyosu daha önce bir arşiv işi `Running` durumunda takıldığında son sinyalin
güncel olup olmadığını göstermiyor ve checkpoint'ten güvenli kurtarma sunmuyordu. Yönetici artık
operasyon kalp atışının güncel/bayat durumunu ve locale-aware son güncelleme zamanını görür. On
beş dakika ilerleme kaydetmeyen iş, işlenen/başarısız sayaçları ile mevcut faz korunarak tek
eylemle yeniden kuyruğa alınabilir.

## Güvenlik ve kurtarma kanıtı

- Kurtarma yalnız `VisualRenewal + Running` ve sunucu tarafından hesaplanan 15 dakikalık eşik
  sonrasında kabul edilir; istemci eşik veya durumu belirleyemez.
- `updated_at` optimistic concurrency token'dır. Kurtarma sonrasında eski state ile heartbeat veya
  progress yazmaya çalışan worker'ın kaydı `DbUpdateConcurrencyException` ile reddedilir.
- İşlem Owner/Admin yetkisi, antiforgery ve append-only audit izi altındadır. Audit; önceki/yeni
  durum, önceki sinyal zamanı, eşik, tamamlanan/başarısız sayaçları ve faz checkpoint'ini içerir.
- Kurtarma hiçbir adayı terfi ettirmez, sağlam kapağı değiştirmez ve görsel kalite kapılarını
  gevşetmez.
- UI; light/dark admin tokenları, renk dışı metinli durum, `alert/status` semantiği ve mobilde
  44px kurtarma eylemi kullanır. Dört desteklenen locale için metin sağlanmıştır.

## Orkestrasyon kalite düzeltmesi

`dotnet test` daha önce test projesinde `IsTestProject` olmadığı için exit code 0 ile yalnız build
yapıyor, hiçbir xUnit testi keşfetmiyordu. Test projesi açıkça işaretlendi. Filtreli ilk gerçek
koşuda 13 `AutomationJobTests` testi keşfedildi ve geçti; yeni checkpoint kurtarma ve geçersiz
durum regresyonları buna dahildir.

## Sınır ve rollback

Bu faz provider seçmez, secret eklemez ve otomatik görsel yayımlamaz. Özel provider worker lease,
sınıflı retry/backoff, dead-letter, yapılandırılmış provenance ve yayın öncesi locale/alt-text
kapısı `visual-service` maddesinin sonraki dilimleridir; aktif yol haritası bu nedenle tamamlanmış
sayılmamıştır. Kod rollback'i şemasızdır; append-only audit kayıtları silinmez ve kurtarılmış aktif
bir iş rollback öncesi operatörce uzlaştırılır.
