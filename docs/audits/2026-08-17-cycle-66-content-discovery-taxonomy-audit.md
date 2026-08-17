# BOECL Çevrim 66 — İçerik keşfi ve bilgi yönetimi taxonomy

## Görünür önce / sonra hedefi

Canlı Türkçe arşivde not alma, kişisel bilgi yönetimi, araştırma ve okuma arşivi içerikleri geniş Verimlilik kategorisinde kayboluyordu. Faz sonunda `/tr-TR/topics` haritası Verimlilik altında yeni **Bilgi Yönetimi ve Not Alma** yolunu gerçek yayın sayısı ve mevcut yayın kapaklarıyla gösterecek; kategori arşivi üst konuyu görünür breadcrumb ve `BreadcrumbList` ile taşıyacak.

## Kanıt ve karar

- Production sitemap 208 benzersiz Türkçe yayın içeriyor. Oyun, bulut altyapı, geliştirici ve fintech adayları 1–3 açık slug sinyalinde kaldığı için yeni kategoriye dönüştürülmedi.
- Anytype, Capacities, Craft, Day One, Heptabase, Mem, NotebookLM, NotePlan, Obsidian, Readwise Reader, Reflect ve Tana dahil not/bilgi/araştırma kümesi ayrı bir arşiv yolu için yeterli ve tutarlı.
- Eşleşme Türkçe yayımlanmış kaynak localization slug sinyallerinden `article_group_id` çıkarır; yalnız var olan gerçek locale karşılıklarını bağlar. Çeviri veya içerik üretmez.

## Veri, SEO ve geri alma

Migration dört locale kaydını kendi Verimlilik üst kategorisine ve tek Türkçe kaynak kategoriye bağlar. İlişkiler `ON CONFLICT DO NOTHING` ile tekrarlanabilir, append-only audit izi taşır. `Down` önce çevirileri, sonra kaynak kategoriyi kaldırır; mevcut yayın ve üst kategorilere dokunmaz. Staging ve production migration öncesinde mevcut dağıtım kapısı PostgreSQL custom-format yedeği ve checksum üretir.

Kategori API'si üst konu kimliğini public arşiv sözleşmesine ekler. Görünür breadcrumb ile JSON-LD aynı sıralamayı kullanır; canonical ve hreflang sözleşmesi korunur. Yeni görsel üretilmez: konu merkezi ve arşiv yalnız kalite kapısından geçmiş mevcut yayın kapaklarını kullanır.

## Kabul kriterleri

- Migration dört locale, üst kategori, audit, idempotency ve rollback regresyonundan geçer.
- API ve Web build/test kapıları geçer; Türkçe karakterler ve locale sözlükleri korunur.
- Staging ve production yedekli migration/deploy sonrası dört kategori URL'si, üst konu breadcrumb'ı, topic haritası ve health kapıları doğrulanır.
- 390 ve 1440 px açık/koyu gerçek render’da başlık, breadcrumb, kapak grid'i, taşma ve kontrast kontrol edilir.
