# BOECL Çevrim 22 — Uluslararası yayın deneyimi

Tarih: 16 Ağustos 2026  
Odak: çeviri, locale bütünlüğü ve uluslararası deneyim

## Görünür hedef ve karar

Önceden admin dil listesi yalnız toplam kayıt sayısını gösteriyor, makale dil menüsü ise
çevirisi olmayan bir dil seçildiğinde okuru ilgisiz dil ana sayfasına gönderiyordu. Bu
çevrimde tek bir Uluslararası Yayın Sağlığı yüzeyi oluşturuldu: dil başına yayın/taslak,
Türkçe kaynak arşive göre kapsama, eksik eşdeğer ve insan incelemesi borcu görünürdür.
Public makalede yalnız yayımlanmış eşdeğerler bağlantıdır; eksik çeviri doğal ve
yerelleştirilmiş bir durum metniyle açıklanır.

## Kök neden ve güvenlik sınırı

Public API zaten yalnız etkin ve yayımlanmış çevirileri döndürüyordu fakat header bu
listenin yokluğunu locale ana sayfasına fallback olarak yorumluyordu. Admin API ise
ilişkisel veriye sahip olduğu halde farkları hesaplamıyordu. Çözüm yeni veri veya otomatik
yayın üretmez; mevcut salt-okunur ilişkileri ölçer, taslakları indexlenebilir yapmaz ve
şema/migration gerektirmez.

## Kalıcı backlog

1. Çeviri görevlerine sahip, son tarih ve SLA atama akışı.
2. Locale bazlı kaynak, SEO, kapak ve gövde kalite skoru.
3. Kategori/etiket çeviri eşitliği ve orphan taxonomy raporu.
4. Kültürel uyarlama gerektiren kapak/alt metin inceleme kuyruğu.
5. Dil bazlı Search Console arama niyeti ve içerik boşluğu.
6. Çeviri güncellik sapması: kaynak makale değişince eşdeğerleri yeniden inceleme.
7. Locale bazlı yazar uzmanlığı ve kaynak güven sınıflandırması.

## Kabul kriterleri

- Dört sözlük aynı anahtar ve placeholder sözleşmesini taşır.
- Makale dil menüsü yalnız gerçek yayımlanmış eşdeğer URL'leri bağlar.
- Admin metrikleri enabled locale, published/draft, missing ve reviewed checklist verisinden gelir.
- 320–1440 px açık/koyu tema hiyerarşisi, lint/typecheck/test/build ve canlı URL doğrulanır.
