# BOECL Çevrim 23 — Görsel Kalite Masası

Tarih: 16 Ağustos 2026  
Odak: makale görsellerinin konu uygunluğu ve yazısız özgün tasarımı

## Görünür hedef ve sonuç

Önceden medya kütüphanesi yalnız dosya boyutu ve kullanım sayısını, üretim hattı ise yalnız
üretim öncesi arama sorgusu uygunluğunu gösteriyordu. Yönetici yayımdaki görsel borcunu
makale bağlamında göremiyordu. Bu çevrim, admin otomasyon alanına responsive Görsel Kalite
Masası ekler: tüm yayımlanmış makaleler gerçek veriden puanlanır, en riskli kayıtlar kapak
önizlemesi ve doğrudan makale/editör bağlantılarıyla sıraya konur.

## Kalite sözleşmesi

Kapı; kapak varlığı, doğal ve konuya temas eden alt metin, yazı/logo/filigran riski, bilinen
boyutlar, 16:9 mobil kırpma toleransı, WebP optimizasyonu, 450 KB bütçesi, kaynak/hak bilgisi
ve uzun içerikte gövde görseli varlığını ölçer. Puan 80'in altında veya herhangi bir risk
varken kayıt otomatik olarak temiz kabul edilmez. Sağlam görsel otomatik değiştirilmez.

## Güvenlik ve operasyon sınırı

Tarama salt okunurdur ve yalnız Owner/Admin rolüne açıktır. Yeni endpoint mevcut yetki
grubunu kullanır; dosya veya veritabanı değiştirmez, üretim anahtarı istemez ve kullanıcı
girdisini HTML olarak render etmez. Gerçek varlık üretimi ve toplu yeniden görselleştirme,
sağlayıcı/lisans metadata modeli ve transaction/checkpoint kuyruğu tamamlanmadan otomatik
yayına bağlanmayacaktır.

## Sonraki görsel-servis backlog'u

1. Kalıcı visual job/item/attempt veri modeli, idempotency anahtarı ve checkpoint migration'ı.
2. Resmî/lisanslı stok, doğrulanmış diagram ve temsili AI sağlayıcı adaptörleri.
3. Vision konu/artefact/yazı denetimi ile perceptual hash tekrar kapısı.
4. Taslak üzerinde önce/sonra onayı, rollback ve audit izi.
5. Locale'e doğal alt metin ve kültürel uyarlama kuyruğu.
6. Staging onayından sonra yeniden başlatılabilir yayımlanmış arşiv taraması.
