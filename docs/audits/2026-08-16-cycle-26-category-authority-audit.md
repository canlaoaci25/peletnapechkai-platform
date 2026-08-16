# BOECL Çevrim 26 — Kategori otorite merkezleri

## Görünür hedef ve kanıt

Production envanterinde 201 Türkçe yayın; mevcut kategorilerde sırasıyla 80, 60, 29, 20,
17, 16 ve 8 yayın ölçüldü. Başlık ve özet taraması yazılım, işletim sistemi, tarayıcı,
mesajlaşma ve üretkenlik uygulaması niyetinde yaklaşık 50 aday gösterirken bu içeriklerin
ayrı bir keşif yolu yoktu. Kategori sayfası da düz kart listesinden ibaretti.

Bu faz `/tr-TR/categories/yazilim-ve-uygulamalar` dahil tüm kategori arşivlerini; toplam
derinlik, içerik türü dağılımı, ilişkili konu rotaları, güçlü bir ana dosya ve görsel yayın
akışıyla otorite merkezine dönüştürür. Yeni taxonomy dört etkin locale’de aynı kaynak
kategoriye bağlıdır; yalnız gerçek yayımlanmış localization ilişkileri public yüzeyde görünür.

## Veri ve güvenlik yaklaşımı

- Migration sabit kimlikler, locale+slug unique kısıtı ve `ON CONFLICT DO NOTHING` ile tekrar çalıştırılabilir.
- Eşleşen Türkçe başlıkların `article_group_id` değeri üzerinden mevcut gerçek çevirilere locale’e özgü kategori atanır; sessiz içerik fallback’i üretilmez.
- Audit kayıtları taxonomy eklenmesini kalıcı olarak izler. `Down` önce çevirileri, sonra kaynak kategoriyi kaldırır; ilişki satırları foreign-key cascade ile temizlenir.
- Uygulama öncesi staging ve production custom-format PostgreSQL yedeği, ardından migration/health doğrulaması zorunludur.

## Sonraki backlog

1. Kategori arşivlerinde cursor tabanlı sayfalama ve toplam aralık bilgisi.
2. Kategorisiz/orphan yayın raporu ve editoryal toplu atama kuyruğu.
3. Dört locale taxonomy paritesi, açıklama kalitesi ve çeviri SLA paneli.
4. Kategori bazlı Search Console görünürlük/CTR ve içerik boşluğu ölçümü.
5. Kanıtlanan derinlikte bilim ve oyun adaylarının editoryal yayın planı.
