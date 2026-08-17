# BOECL Çevrim 57 — Public sidebar, mobil drawer ve tema bütünlüğü

## Görünür önce / sonra hedefi

Önceden ana yayın navigasyonu iki katmanlı üst masthead içinde yatay kategori sıraları ve
açılır bir bölüm menüsü kullanıyordu. Arşiv büyüdükçe bu yapı kategori adlarını sıkıştırıyor,
ana sayfanın ilk ekranını aşağı itiyor ve mobilde keşif, hesap, dil ve tema araçlarını farklı
noktalara dağıtıyordu.

Bu fazda 1024 px ve üzerindeki public yüzeyler BOECL'e özgü sabit bir sol yayın rayına geçti.
Ray; marka, arama, son yayınlar, konu atlası, kaynak güven merkezi, API'den gelen gerçek
yerelleştirilmiş kategoriler ve yayın sayılarını, üyelik, dil ve tema araçlarını tek bilgi
mimarisinde toplar. Daha dar ekranlarda ince yardımcı bar kalır; aynı ray varsayılan kapalı,
overlay'li bir drawer olarak açılır. Escape, focus trap, açan düğmeye odak dönüşü, body scroll
kilidi ve en az 44 px hedefler klavye/dokunma sözleşmesinin parçasıdır.

## Etki alanı ve kararlar

- Sidebar admin kabuğunu kopyalamaz; public API taxonomy verisini kullanır ve mevcut locale
  arşiv yollarını korur. Statik/sahte kategori eklenmemiştir.
- 1024 ve 1440 px içerik alanı ray genişliği çıkarıldıktan sonra akışkan kalır; 320–768 px
  görünümde ray içerik genişliğini tüketmez.
- Background, surface, elevated surface, foreground, muted, border, accent, overlay, focus ve
  shadow rolleri merkezi tokenlarla açık/koyu temada tanımlanmıştır. Native form color-scheme
  ve mevcut ilk-render tema betiği korunmuştur.
- Canonical, hreflang, JSON-LD, sitemap, LCP görsel preload ve yayın locale izolasyonu değişmez.
  API/veritabanı/migration veya production içerik mutasyonu yoktur.

## Regresyon ve kalite kapıları

- Public drawer sözleşmesi; Escape, Tab döngüsü, scroll kilidi, odak dönüşü, gerçek taxonomy,
  desktop/mobile breakpoint ve semantik tema tokenları için kaynak regresyonuyla korunur.
- Dört locale sözlük eşitliği, lint, Next typecheck, 61 web regresyonu ve production build.
- API testleri ile .NET Release build; staging/production atomik web deploy ve public health.
- 390 ve 1440 px açık/koyu gerçek render; ayrıca 320, 375, 768 ve 1024 px taşma smoke kontrolü.

## Kalan ölçüm fırsatı

Kullanıcı izni bulunan route ölçümü oluştuğunda sidebar kategori sıralaması salt arşiv
derinliği yerine editoryal önem ve gerçek keşif davranışını birlikte kullanabilir. Bu faz yeni
izleme sinyali veya izin kapsamı eklemez.
