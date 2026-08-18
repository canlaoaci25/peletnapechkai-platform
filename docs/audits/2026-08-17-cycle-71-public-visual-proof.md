# BOECL Çevrim 71 — public görsel kanıt matrisi

## Görünür önce / sonra

Görsel Yenileme Stüdyosu artık mevcut ve aday kapağı yalnız 16:9 olarak değil; makale hero 16:9,
masaüstü manşet, mobil 1:1, konu kartı 4:3 ve güncel akış 16:10 kadrajlarında yan yana gösterir.
Editör, yazısız ve konuya özel tasarımın public yüzeylerde ana öznesini kaybetmesini aday terfisinden
önce görebilir. Matris adminin mevcut light/dark semantik tokenlarını ve gerçek `object-fit: cover`
davranışını kullanır; mobilde tek kolona iner.

## Operasyon güvenliği

`VisualRenewal` kalıcı batch'i editor kontrollü checkpoint akışıdır. Generic Codex worker bu işi
üretecek provider sözleşmesine sahip değildir. Claim ve stale-timeout sorguları artık bu türü dışlar;
böylece unsupported prompt hatasıyla HTTP 500 sonrası 30 dakika `Running` kalması ve FIFO kuyruğunu
bloke etmesi önlenir. Ayrı provider worker uygulanana kadar davranış bilinçli olarak fail-closed'dur.

## Sınırlar

Bu faz crop varlığını görünür kılar; focal point seçimi, OCR/vision değerlendirmesi, provider adaptörü,
otomatik crop onayı ve production görsel backfill'i tamamlanmış sayılmaz. Ücretli sağlayıcı veya secret
aktivasyonu yapılmamıştır. Sağlam mevcut görseller otomatik değiştirilmez.
