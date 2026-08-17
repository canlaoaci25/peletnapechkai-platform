# BOECL Çevrim 47 — Görsel Üretim Operasyon Merkezi

## Görünür önce / sonra hedefi

Görsel Yenileme Stüdyosu daha önce riskli makaleleri tekil kalıcı görevlere alıyor fakat
arşiv işinin bütünü için güvenilir ilerleme veya kontrol sunmuyordu. Artık yönetici tek bir
arşiv yenileme operasyonunda toplam, işlenen, kalan, başarılı ve reddedilen sayıları; aktif
makaleyi ve checkpoint durumunu görür, işi duraklatabilir, kaldığı yerden sürdürebilir veya
güvenle iptal edebilir. Yüzey Türkçe, İngilizce, Almanca ve Fransızca çalışır.

## Ürün ve veri bütünlüğü

- Her toplu iş `AutomationJob` üzerinde kalıcıdır; görsel görevleri nullable dış anahtarla
  kaynak işe bağlanır ve durum indeksinden raporlanır.
- Aynı anda ikinci aktif arşiv işi açılamaz. Görev idempotency anahtarları aynı riskli
  makalenin gereksiz tekrar kuyruğa alınmasını önler.
- Başlatma, duraklatma, devam ve iptal işlemleri Owner/Admin yetkisi, antiforgery ve audit
  izi altındadır.
- Toplu iş hiçbir kapağı otomatik terfi ettirmez. Mevcut transaction tabanlı aday kalite
  kapısı ve önceki kapak audit kaydı korunur.
- Lisanslı sağlayıcı konuya uygun sonuç vermezse kapak ve gövde görseli için soyut/geometrik
  fallback artık üretilmez; içerik hattı güvenli biçimde hata verir ve yanlış görsel yayınlamaz.
- Migration ileri/geri çalışabilir; iş silinirse görev geçmişi korunup yalnız batch bağı
  `SetNull` olur.

## Doğrulama ve kalan sınır

Bu çevrim arşiv çapında kalıcı orchestration, görünür yönetim ve kalite düşüren fallback'in
kaldırılmasını tamamlar. Sağlayıcı anahtarları repoya eklenmemiştir. Otomatik vision puanı,
perceptual hash/embedding benzerliği ve gerçek AI sağlayıcı adaptörü hâlâ dış servis
yapılandırması gerektiren sonraki kilometre taşlarıdır; bunlar olmadan aday otomatik
yayınlanmaz.
