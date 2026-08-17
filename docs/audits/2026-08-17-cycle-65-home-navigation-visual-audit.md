# BOECL Çevrim 65 — Ana sayfa, global navigasyon ve görünür dönüşüm

## Görünür önce / sonra hedefi

Production ana sayfasının 1440 px gerçek render'ında manşet başlığı dar bir sütunda aşırı
büyüyor, ilk ekranı tüketiyor ve güncel akış ile manşet arasındaki öncelik ilişkisini
zayıflatıyordu. Edisyon rotası da manşetten önce gereğinden fazla dikey alan kullanıyordu.

Bu faz edisyon rotasını kompakt bir keşif indeksine, ana vitrini dengeli bir yayın masasına
dönüştürür. Manşet görseli, başlık ve beş kayıtlık güncel akış ilk ekranda birlikte okunur;
sidebar gerçek yerelleştirilmiş taxonomy, arama, üyelik, dil ve tema görevini korur. Tablet ve
mobilde mevcut anlamlı tek kolon sırası, drawer erişilebilirliği ve dokunmatik şeritler değişmez.

## Kanıt ve karar

- Production ve staging ana sayfaları, son 20 yerel commit, kalıcı yol haritası ve önceki
  ana sayfa/sidebar denetimleri incelendi. Production 1440 px önce render'ı kalıcı artefaktla
  doğrulandı.
- BBC'nin güncel ana sayfa açıklamasındaki büyük hikâye, zamanlı içerik ve editör seçkisini
  birlikte sunma ilkesi BOECL'in mevcut manşet + güncel akış + konu atlası yapısıyla örtüşür.
  Tasarım veya metin kopyalanmadı.
- W3C'nin görünür odak yaklaşımı ve mevcut iki renkli focus-visible sözleşmesi korunur.
  Google/web.dev'in LCP ve görsel boyutlandırma önerileri doğrultusunda manşet görselinin
  Next.js preload, `sizes` ve ayrılmış aspect-ratio davranışı değiştirilmez.

## Etki alanı ve kabul kapıları

- Değişiklik public ana sayfa CSS kompozisyonu, kaynak regresyon testi, yol haritası ve bu
  karar kaydıyla sınırlıdır. API, veritabanı, içerik, medya, auth ve production verisi değişmez.
- Dört locale aynı veri ve sözlük sözleşmesini kullanır; canonical, hreflang, JSON-LD,
  sitemap ve yayın locale izolasyonu korunur.
- Locale, web test/lint/typecheck/build ve .NET test/Release build kapıları geçmelidir.
- 320, 375, 390, 768, 1024 ve 1440 px; açık/koyu tema; drawer açık/kapalı ve yatay taşma
  gerçek browser render ile doğrulanmalıdır. Staging ve production sağlık kapıları ile canlı
  URL doğrulanmadan faz tamamlanmış sayılmaz.
