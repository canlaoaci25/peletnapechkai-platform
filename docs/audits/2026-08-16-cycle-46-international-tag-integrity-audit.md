# BOECL Çevrim 46 — Uluslararası etiket bütünlüğü

## Görünür önce/sonra hedefi

Etiket arşivleri daha önce yalnız Türkçe yayında vardı ve farklı locale sürümleri arasında
doğrulanmış bir ilişki bulunmadığından dil geçişi, reciprocal `hreflang` ve sitemap alternatifi
üretilemiyordu. Bu çevrim sonunda yedi gerçek yayın etiketi İngilizce, Almanca ve Fransızca
doğal ad/slug karşılıklarına sahip olur; çevrilmiş makaleler aynı tag graph’ına bağlanır ve
ziyaretçi yalnız gerçekten yayımlanmış eşdeğer arşivler arasında geçer.

## Veri ve güvenlik sınırı

- `source_tag_id`, kaynak Türkçe tag + hedef locale için tekildir; slug benzerliğinden ilişki
  tahmin edilmez.
- Migration 21 hedef tag’i transaction içinde oluşturur, aynı article group içindeki mevcut
  çevirileri kaynak tag ilişkisine göre bağlar ve her oluşturulan tag için audit izi bırakır.
- Rollback yalnız migration audit izi bulunan tag’leri kaldırır; kullanıcı tarafından sonradan
  oluşturulan tag ilişkilerine dokunmaz.
- Public API ve SEO yalnız etkin locale’de yayımlanmış makalesi bulunan karşılıkları açığa çıkarır.
  Taslak veya boş arşiv hreflang içine girmez.

## Kabul kriterleri

- Tag arşivi self-canonical ve reciprocal locale alternates üretir; Türkçe `x-default` olur.
- XML sitemap aynı doğrulanmış tag graph’ını kullanır.
- Admin yayın sağlığı, hedef locale başına bağlı ve eksik tag sayısını gerçek veriden gösterir.
- Dört locale sözlük/test kapısı, web regresyonları, lint, typecheck, production build, API
  testleri ve Release build geçer.
- Staging ve production migration öncesinde yedeklenir; canlı sayfa ve head metadata doğrulanır.

## Kalıcı backlog

1. Çeviri oluşturulduğu anda kaynak alan snapshot’ı kaydedip güvenilir alan bazlı source diff.
2. Kültürel uyarlama ve doğal alt metin için locale görsel inceleme kuyruğu.
3. Locale bazlı Search Console niyet ve sıfır sonuç ölçümü.
4. Admin tag düzenleyicisinde yetkili manuel source-tag eşleştirme akışı.
