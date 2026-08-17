# BOECL Çevrim 41 — Günlük edisyon rotası ve global navigasyon derinliği

## Görünür önce / sonra hedefi

Önceden ana sayfa güçlü bir manşet, popüler liste, konu atlası ve son yayınlar sunuyor; ancak
okur ilk ekranda günün edisyon büyüklüğünü veya bu katmanlar arasında doğrudan geçiş yolunu
göremiyordu. Global kategori çubuğu da gerçek arşiv kapsamını yalnız açılır menüde gösteriyordu.

Artık ana sayfa, yalnız o locale'de bulunan benzersiz ve yayımlanmış hikâyelerden oluşan bir
**Edisyon rotası** sunar. Yerelleştirilmiş tarih, edisyondaki özgün hikâye sayısı ve manşet,
popüler, konu atlası, editör seçkisi ile son yayınlara sayfa içi doğrudan yollar aynı editoryal
yüzeyde görünür. Global kategori navigasyonu kategori başına yayın kapsamını masaüstünde
gösterir; mobilde mevcut yatay konu düzenini korumak için yoğunluk rozetlerini gizler. Edisyon
rotası mobilde görünür kaydırma davranışı, 104 piksel dokunma yüzeyleri ve snap hizası kullanır.

## Kanıt ve ürün kararı

- Canlı production ana sayfası 1440 ve 390 pikselde, son 20 commit, kalıcı yol haritası ve
  Çevrim 25/33 raporları incelendi. Önceki manşet ve taxonomy işini tekrarlamak yerine sayfanın
  mevcut güçlü katmanları arasındaki keşif kopukluğu seçildi.
- The Verge'in güncel ana sayfası birincil hikâyeler, en yeni akış ve kalıcı konu kümelerini
  ayrı keşif yolları olarak birlikte sunuyor. WIRED da konu navigasyonu ile güncel yayın akışını
  aynı yayın kabuğunda koruyor. BOECL bu prensibi kendi gerçek locale/taxonomy verisi ve özgün
  “edisyon rotası” diliyle uygular; tasarım veya metin kopyalamaz.
- W3C bağlantı amacı ve birden fazla gezinme yolu ilkelerine uygun olarak bağlantı etiketleri
  hedef bölümü açıklar, yerel anchor davranışı klavye ve yardımcı teknolojilerle çalışır.

## Kapsam, SEO, performans ve güvenlik

- Dört locale sözlüğüne doğal arayüz metni eklendi ve anahtar eşitliği doğrulandı.
- Yeni içerik, görsel, migration veya üretim verisi oluşturulmadı. Yalnız public API'nin locale
  izolasyonlu yayımlanmış içerikleri kullanıldı; görsel kalite ve yazısız kapak politikası değişmedi.
- Anchor yolları canonical/hreflang üretimini, indeks politikasını veya URL mimarisini değiştirmez.
- Yeni istemci bileşeni, JavaScript veya üçüncü taraf istek eklenmedi; yüzey server-rendered HTML
  ve CSS'tir. Mobil navigasyonda geniş metinleri taşıyan yatay kaydırma korunur.

## Kabul kapıları

- Locale eşitliği ve web regresyonları.
- Lint, typecheck, production web build, API testleri ve Release build.
- 320, 375, 390, 768, 1024 ve 1440 piksel gerçek Chromium render; light/dark, yatay taşma,
  odak görünürlüğü ve bölüm hedefleri.
- Commit/push sonrası staging ve production atomik deploy, sağlık ve public-experience kapıları;
  dört locale canlı HTML sözleşmesi doğrulanmadan Completed durumu verilmez.

## Sonraki yüksek değerli faz

Edisyon bağlantılarının anonim ve izinli tıklama ölçümü eklenerek manşet sonrası bölüm sırası
gerçek okur davranışıyla test edilmeli; ölçüm yeterli olduğunda admin ana sayfa motoruna
editoryal öncelik + trafik kanıtı birleşimi eklenmelidir.
