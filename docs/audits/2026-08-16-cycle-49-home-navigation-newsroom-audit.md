# BOECL Çevrim 49 — Ana sayfa ve global navigasyon dönüşümü

Tarih: 16 Ağustos 2026  
Odak: ana sayfa, global navigasyon ve görünür yayın vitrini

## Görünür önce / sonra hedefi

Önceden ziyaretçi büyük manşeti ve ikincil başlıkları görebiliyor, ancak kronolojik yayın
akışına ulaşmak için ana sayfanın alt katmanlarına ilerliyordu. Global bölüm satırı da konu
arşivlerini gösterirken BOECL'in gerçek kaynak ve güven merkezine doğrudan yol vermiyordu.

Artık ilk yayın vitrini üç görevi tek kompozisyonda birleştirir: büyük görsel manşet, beş
yayınlık saatli güncel akış ve görsel ikincil dosya şeridi. Masaüstünde manşet ile koyu
“Güncel akış” masası yan yana çalışır; tablet ve mobilde anlamlı okuma sırasına iner, ikincil
dosyalar dokunmatik snap şeridine dönüşür. Global navigasyon, mevcut locale-aware Kaynaklar
ve Güven Merkezi'ni bağımsız bir üst düzey keşif yolu olarak görünür kılar.

## Kanıt ve ürün kararları

- Production, staging ve admin yanıtları; son 20 commit, kalıcı roadmap, Çevrim 33 ve 48
  denetimleri incelendi. Önceki manşet, atlas ve edisyon rotası korunarak tekrar iş yerine
  ilk ekrandaki kronolojik keşif boşluğu seçildi.
- [W3C WAI menü rehberi](https://www.w3.org/WAI/tutorials/menus/) mevcut konumun belirgin,
  menü yapısının semantik, odağın anlamlı sırada ve içerik keşfinin birden fazla yolla
  erişilebilir olmasını ister. Yeni kaynak yolu gerçek bir `Link`, güncel masa `aside`, akış
  ise sıralı listedir; görsel bağlantılar yinelenen klavye durağı oluşturmaz.
- [W3C reflow açıklaması](https://www.w3.org/WAI/WCAG21/Understanding/reflow.html) dar
  görünümde içeriğin kaybolmadan yeniden akmasını gerektirir. Üst vitrin 1050 px altında tek
  kolona, ikincil dosyalar 700 px altında açık yatay kaydırma davranışına geçer.
- [BBC'nin içerik yüzeyi açıklaması](https://help.bbc.com/hc/en-us/articles/39027623773331-What-types-of-news-content-will-be-available)
  büyük hikâyeler, editör seçkisi ve kalıcı bölüm keşfini birlikte sunar. BOECL bu prensibi
  kendi kaynaklı teknoloji/bilim arşivi ve özgün token sistemiyle uygular; metin veya tasarım
  kopyalanmamıştır.

## Veri, SEO, performans ve güvenlik sınırı

- Yalnız mevcut public homepage ve archive API verisi kullanılır. Yeni içerik, taxonomy,
  görsel, migration veya production veri mutasyonu yoktur.
- Dört locale'in arayüz metinleri eksiksizdir. Canonical, hreflang, sitemap, JSON-LD ve
  yayın izolasyonu değişmemiştir.
- Manşet LCP görselinin mevcut `preload` sözleşmesi ve Next.js optimizasyonu korunur. Yeni
  güncel masa metin tabanlıdır; ilk yüklemeye yeni istemci JavaScript'i veya görsel isteği
  eklemez. Aşağıdaki son yayınlar ilk beş kayıtla yinelenmez.
- Kaynak bağlantısı mevcut public arşive gider; auth, admin, cookie veya API yetkisi
  değişmez. Koyu/açık tema tokenları ve reduced-motion davranışı korunur.

## Kabul kapıları

- 4 locale sözlük eşitliği ve 56 web regresyonu.
- ESLint, Next.js type generation/TypeScript ve production build.
- 130 API testi ve .NET Release build.
- Staging ve production atomik deploy; 320, 375, 390, 768, 1024 ve 1440 px gerçek render,
  açık/koyu tema, yatay taşma, menü, manşet ve güncel akış doğrulaması.

## Sonraki yüksek değerli faz

Güncel akış ve konu atlası tıklamaları izinli route ölçümüyle ayrıştırılmalı; trafik kanıtı
oluştuğunda ana sayfa sırası editoryal sabitleme ile davranış verisini birlikte kullanmalıdır.
Mobil art-direction crop ve otomatik görsel-anlam/benzerlik puanı, görsel servisinin kalan
öncelikli kalite kilometre taşlarıdır.
