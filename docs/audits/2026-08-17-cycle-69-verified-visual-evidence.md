# BOECL Çevrim 69 — doğrulanmış görsel aday kanıtı

## Görünür önce / sonra

Admin Görsel Yenileme Stüdyosu daha önce konu, yazısızlık ve mobil crop için tarayıcıdan serbest
sayısal puan kabul ediyor; boş provenance alanlarını “BOECL Original” diye dolduruyordu. Artık editör
konu-bölüm eşleşmesi ile yazı/logo/filigran/sahte UI yokluğunu açık onay kutularıyla ve kimliği/zamanı
audit edilen bir kanıt olarak verir. Crop puanı gerçek medya oranından, özgünlük puanı arşiv perceptual
karşılaştırmasından yalnız sunucuda hesaplanır. Sağlayıcı ve lisans boşsa doğrulanmış gibi gösterilmez;
tüm kanıtlar geçmeden transaction tabanlı terfi kapalıdır.

## Sınır ve operasyon kararı

Bu faz gerçek production görsel/vision sağlayıcısı olduğunu iddia etmez. Provider seçimi, ücret, secret,
kota ve veri işleme kararı olmadığı için harici aktivasyon fail-closed kalır. Production veritabanı
backup/restore kanıtı yetersiz olduğundan migration tasarımı reddedildi; mevcut audit alanları kullanıldı
ve veri şeması değişmedi.

## Kabul kanıtı

- Dört locale için editoryal kanıt dili ve koyu/açık admin tokenları.
- İstemciden serbest topic/text/crop puanı kaldırıldı.
- Eksik editoryal onay `CandidatePasses=false`; crop ve originality server-derived.
- Hedefli API testi, lint, typecheck, Next production build, tüm API testleri ve Release build.
- Staging/production deploy sonrası yetkili admin render ve canlı health ayrıca kaydedilir.
