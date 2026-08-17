# BOECL Çevrim 55 — Görsel özgünlük ve tekrar kapısı

## Görünür hedef

Görsel Yenileme Stüdyosu'nda özgünlük artık editörün yazdığı öznel bir sayı değildir. Aday kapak sunucu tarafında arşivle karşılaştırılır; yönetici en yakın görseli, benzerlik yüzdesini ve türetilen özgünlük puanını masaüstü ve mobilde görür. Yakın tekrar kalite kapısından geçemez ve mevcut kapak transaction dışında değişmez.

## Kök neden ve kapsam

Çevrim 39 aday terfisini ve Çevrim 47 yeniden başlatılabilir operasyonu kurmuştu, ancak `originalityScore` istemciden güvenilerek alınıyordu. Bu, aynı görselin veya küçük bir varyantının yüksek puanla sunulmasına izin veriyordu. Çözüm medya varlığına kalıcı 64 bit fark hash'i ekler, ilk aday denetiminde mevcut optimize arşivi güvenli depolama kökü altında checkpoint olarak parmak izler ve Hamming uzaklığından en yakın eşleşmeyi hesaplar.

## Kalite ve güvenlik sözleşmesi

- Yol çözümleme yapılandırılmış medya kökü dışına çıkamaz.
- Bozuk veya eksik aday reddedilir; arşivdeki bozuk bir kayıt diğer karşılaştırmaları durdurmaz.
- İstemci özgünlük puanı artık göndermez; audit kaydı sunucunun puanı ve eşleşme kimliğini tutar.
- Yakın eşleşme mevcut `>= 85` özgünlük yayın kapısıyla engellenir.
- Migration yalnız nullable alanlar ve indeks ekler; geri alma yolu alanları/indeksi kaldırır, mevcut içerik silmez.

## Sonraki görsel servis adımları

Vision tabanlı konu/yazı/artefact puanları, sağlayıcı adaptörü üzerinden otomatik aday üretimi ve locale'e özgü gövde görseli art direction halen açık önceliklerdir.
