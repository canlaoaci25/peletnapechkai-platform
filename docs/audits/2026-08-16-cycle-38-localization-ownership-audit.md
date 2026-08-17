# BOECL Çevrim 38 — Yerelleştirme sahipliği ve SLA

## Görünür önce/sonra hedefi

Önceden `/{locale}/admin/languages` eksik, güncelliğini yitirmiş ve inceleme bekleyen
çevirileri yalnız toplam sayılarla gösteriyordu. Artık aynı yüzey her gerçek borcu kaynak
başlığı ve hedef locale ile listeler; yönetici sorumlu ve son tarih atar, ekran sahipsiz,
gecikmiş, yaklaşan ve planlı SLA durumunu açıkça gösterir. Kuyruk masaüstünde iki kolon,
mobilde tek kolon çalışır; arayüz Türkçe, İngilizce, Almanca ve Fransızcadır.

## Veri, güvenlik ve operasyon sınırı

- `localization_assignments` kaynak article group + hedef locale üzerinde tekildir; tekrar
  atama yeni satır üretmez. Owner/SLA indeksi operasyon kuyruğunu destekler.
- Atama yalnız etkin kullanıcı, etkin hedef locale ve yayımlanmış Türkçe kaynak için kabul
  edilir. Geçmiş tarih reddedilir; mutation antiforgery ve yönetici yetkisiyle korunur.
- Her değişiklik `localization.assignment_updated` audit olayı üretir. Atama içerik üretmez,
  yayımlamaz ve taslağı indexlenebilir hale getirmez.
- Migration yalnız yeni tablo/indeks ekler; `Down` tabloyu kaldırır. Production öncesi iki
  veritabanı yedeği ve staging doğrulaması zorunludur.

## Kabul kriterleri

- Dört locale kopyası, filtre, sorumlu, son tarih ve SLA durumları görünürdür.
- Eksik, stale ve insan incelemesi borcu aynı açık kurallarla hesaplanır.
- Locale eşitliği, web testleri, lint, typecheck, production build, API testleri ve Release
  build geçer; staging/production health ve gerçek URL doğrulanmadan tamamlanmış sayılmaz.

## Kalıcı backlog

1. Kaynak revizyon farkını alan bazında gösteren çeviri karşılaştırması.
2. Tag çeviri kaynak ilişkisi ve orphan raporu.
3. Locale bazlı kaynak, SEO, kapak ve gövde kalite skoru.
4. Kültürel uyarlama ve doğal alt metin için görsel inceleme kuyruğu.
5. Locale bazlı Search Console arama niyeti ve içerik boşluğu.
