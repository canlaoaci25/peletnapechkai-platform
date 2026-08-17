# BOECL Çevrim 39 — Görsel Aday Kalite ve Yayın Terfisi

## Görünür önce / sonra hedefi

Önceden Görsel Yenileme Stüdyosu yalnız tam metinden brief üretip editoryal karar kaydediyor,
gerçek aday görseli göstermiyor ve güvenli biçimde yayına alamıyordu. Artık yönetici mevcut ve
aday kapağı yan yana görür; sağlayıcı, lisans, atıf, doğal alt metin ile konu, yazısızlık, mobil
crop ve özgünlük puanlarını kalıcı kanıta dönüştürür. Tüm kapılar geçerse aday tek transaction
içinde yayımlanır.

## Güvenlik ve veri bütünlüğü

- Aday bağlama ve yayın terfisi Owner/Admin, antiforgery ve audit koruması altındadır.
- API optimize edilmiş, yaklaşık 16:9, en az 1200 px ve 500 KB altı adayı zorunlu tutar.
- Konu ≥80, yazısızlık ≥95, crop ≥80 ve özgünlük ≥85 olmadan terfi reddedilir.
- Lisans ve locale doğal alt metni zorunludur; eski kapak audit ayrıntısında korunur.
- Migration geri alınabilir; aday silme davranışı `Restrict`, mevcut kapak ilişkisi `SetNull` kalır.

## Kalan sınır

Bu faz insan tarafından doğrulanan sağlayıcı/kalite kanıtı ve güvenli terfi hattını tamamlar.
Model sağlayıcı anahtarları eklenmemiştir; vision ve perceptual similarity puanlarının otomatik
üretilmesi ile checkpointli tüm arşiv worker'ı sonraki kilometre taşıdır.
