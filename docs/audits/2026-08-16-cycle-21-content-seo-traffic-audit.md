# BOECL ilk çalıştırma ana denetimi — Çevrim 21

Tarih: 16 Ağustos 2026  
Odak: Türkçe içerik, SEO, kaynak kalitesi ve trafik büyümesi

## Yönetici özeti ve görünür hedef

BOECL; Next.js 16.3/React 19 istemcisi, ASP.NET Core 10 API, EF Core/PostgreSQL,
IIS/Windows Service ve `tr-TR`, `en-US`, `de-DE`, `fr-FR` yayınlarından oluşan çalışan
bir platformdur. Canlı ana sayfa, konu haritası, makale, arşiv, arama, üyelik ve admin
yüzeyleri 16 Ağustos 2026 tarihinde HTTP 200 döndürmektedir. Depo temizdir ve son 20
commit; ana sayfa keşfi, mobil teknoloji taxonomy'si, üyelik ve editoryal komuta
kuyruğunun yakın zamanda tamamlandığını gösterir.

Bu çevrimin önce/sonra hedefi makale sayfasıdır: okur bugün başlık, tek tarih, gövde ve
sayfa sonundaki yalın kaynak listesini görüyor. Faz sonunda kategori yolu, yayın ve
güncelleme ayrımı, tahmini okuma süresi, yazar/kaynak güven özeti, kaynak alan adları ve
kapaklı ilgili okuma kartları tek responsive **Makale Güven ve Keşif Katmanı** içinde
sunulacaktır. Değişiklik mevcut 177 Türkçe yayının tamamına ve gerçek çevirilerine
uygulanır; yapay içerik veya doğrulanmamış veri üretmez.

## Tam sistem analizi

- **Mimari ve sürümler:** `apps/web` locale-aware App Router; `apps/api` minimal API,
  domain, EF ve worker katmanı; `tests/api`; `ops/windows`; kalıcı `docs` kayıtları.
  Web paketi Next 16.3.0, React 19.2.8 ve TypeScript 5; API .NET 10 ailesindedir.
- **Veri ve ilişkiler:** locale, article group/localization, kategori/etiket, yazar,
  kaynak, medya varyantı, revizyon, SEO, checklist, görev, homepage, engagement,
  üyelik ve append-only audit izi vardır. Kaynak URL'si benzersizdir ve private ağ,
  kimlik bilgisi, `file:` gibi güvensiz hedefler domain katmanında reddedilir.
- **Auth ve güvenlik:** Identity cookie, rol/politika ayrımı, antiforgery, rate limit,
  loopback API ve admin proxy vardır. Public kaynak sunumu ayrıca URL'yi yeniden
  doğrular. Açık kalan savunma borçları kontrollü CSP, HSTS/clickjacking başlıkları ve
  off-site restore tatbikatıdır.
- **Admin ve içerik:** editör, workflow, revizyon, taxonomy, kaynak, medya, homepage,
  trafik, kullanıcı/dil, Knowledge Vault ve otomasyon yüzeyleri vardır. Kaynak ilişkisi
  denetlenebilir güncelleme komutuyla yönetilir. 208 kapaksız ve 245 eski Markdown gövdeli
  yayın önceki denetimde ölçülmüştür; toplu yayın mutasyonu bu fazın güvenli kapsamı değildir.
- **SEO:** canonical, hreflang/x-default, Open Graph, Article ve Breadcrumb JSON-LD,
  sitemap/RSS, robots ve draft noindex temelleri vardır. Article şeması kaynakları
  `citation`, taxonomy'yi `articleSection/keywords` olarak taşır. Görünür sayfada ise
  `dateModified`, kaynak bağlamı ve breadcrumb yeterince belirgin değildir.
- **Performans:** public istekler salt okunur ve `AsNoTracking`; görseller Next Image
  üzerinden responsive sunulur. Makale LCP kapağı preload edilir. Yeni faz ek API çağrısı
  açmadan mevcut payload'u kullanmalı; ilgili kartlarda doğru `sizes` ve lazy davranışı
  korunmalıdır.
- **Tasarım/UX ve erişilebilirlik:** açık/koyu/sistem tokenları, semantik başlıklar,
  skip-link, odak görünümü ve responsive temel vardır. Makale 780 px okuma kolonudur;
  güven bilgisi dağınık, ilgili içerikler görselsiz ve kaynaklar küçük metin listesidir.
  320–1440 px aralığında bilgi yoğunluğu ile okunabilirlik birlikte çözülmelidir.
- **İçerik ve görsel kalite:** canlıda 177 Türkçe, diğer üç dilde 162'şer yayın bulunduğu
  önceki tam denetimde doğrulanmıştır. Konuya özel kapaklar mevcut olduğunda alt metin,
  kredi ve kaynak sayfası sunulur. Bu faz yeni görsel uydurmaz; mevcut doğrulanmış
  kapakları keşif kartlarında yeniden kullanır.
- **Operasyon:** atomik web/API deploy, staging/production health, rollback, günlük
  PostgreSQL yedeği ve restore testi vardır. Bu faz şema/veri değiştirmediği için yeni
  yedek veya migration gerektirmez; staging ve production doğrulaması yine zorunludur.
- **Araştırma ilkeleri:** Google, insanların yararı için üretilen içeriğin açık kaynak,
  uzmanlık/yazar bağlamı ve güven sinyalleri taşımasını; Discover için yanıltıcı olmayan
  başlıklar ve en az 1200 px kaliteli görselleri önerir. Google ayrıca önemli sayfaların
  taranabilir iç bağlantılarla bağlanmasını ister. Faz bu ilkeleri kaynak görünürlüğü,
  güncellik ve ilgili okuma akışına dönüştürür. Kaynaklar:
  [Helpful content](https://developers.google.com/search/docs/fundamentals/creating-helpful-content),
  [Google Discover](https://developers.google.com/search/docs/appearance/google-discover),
  [Link best practices](https://developers.google.com/search/docs/crawling-indexing/links-crawlable).

## Öncelikli ilk 20 geliştirme

1. **P2 — Makale Güven ve Keşif Katmanı:** güncellik, okuma süresi, yazar, kaynak ve kapaklı ilgili okumaları bütünleştir. **Bu çevrim.**
2. **P2 — Kaynak sağlık merkezi:** kırık URL, son doğrulama tarihi, kaynak türü ve editör sahibi.
3. **P2 — Türkçe içerik sağlık puanı:** kaynak, kapak, taxonomy, güncellik ve gövde derinliği.
4. **P2 — Kapaksız Türkçe arşiv kuyruğu:** trafik/değer sıralı, lisans ve konu uygunluğu kapılı.
5. **P2 — Eski Markdown gövde normalizasyonu:** önizleme, transaction, rollback ve audit.
6. **P2 — Category authority hub:** alt konu navigasyonu, öne çıkan rehber ve güncel akış.
7. **P2 — Orphan içerik ve iç link önerileri:** cluster bağlamlı, insan onaylı.
8. **P2 — Arama niyeti/content-gap paneli:** Search Console sorgusu ile gerçek arşiv eşleştirmesi.
9. **P2 — Türkçe evergreen tazelik kuyruğu:** yaş, trafik kaybı ve değişen kaynak sinyalleri.
10. **P2 — Yazar uzmanlık profilleri:** bio, uzmanlık alanları, doğrulanmış yayın geçmişi.
11. **P2 — Kaynak türü ve birincil kaynak işaretleme:** admin + API + görünür açıklama.
12. **P2 — Locale üretim dengesi:** eksik çeviri, gecikme ve dil bazlı yayın ritmi.
13. **P2 — Görsel benzersizlik/konu uygunluğu örneklemesi:** tekrar ve anlamsal sapma kuyruğu.
14. **P2 — Görsel metadata kalite kapısı:** kredi, lisans, alt metin ve crop doğrulaması.
15. **P2 — Arama sonuçlarında kategori/kapak/güncellik bağlamı:** daha güçlü keşif kartları.
16. **P2 — Homepage slot ölçümü:** CTR/engagement ve içerik çeşitliliği karar desteği.
17. **P2 — Editoryal düzeltme ve güncelleme notu:** public güven izi ve audit zinciri.
18. **P1 — CSP Report-Only'den kontrollü enforce'a geçiş.**
19. **P1 — HSTS, frame-ancestors ve Permissions-Policy canlı doğrulaması.**
20. **P1 — Şifreli off-site yedek ve düzenli bağımsız restore tatbikatı.**

## Kabul kriterleri ve risk

- Makale üstünde kategori yolu, yayın/güncelleme, okuma süresi, yazar ve kaynak sayısı görünürdür.
- Kaynaklar güvenli dış bağlantı, alan adı ve açıklayıcı bağlamla sunulur.
- İlgili içerik kartları mevcut konuya özgü kapakları responsive ve yazısız biçimde kullanır.
- Article/Breadcrumb JSON-LD, canonical/hreflang ve public API sözleşmesi gerilemez.
- Dört locale metni; açık/koyu tema; 320, 375, 390, 768, 1024 ve 1440 px renderları doğrulanır.
- Lint, locale, typecheck, web test/build, API test ve Release build geçmeden deploy edilmez.

Kalan risk: tahmini okuma süresi dil bağımsız sözcük sayımına dayanır; gerçek okuma davranışı
ölçümü değildir. Kaynakların güven düzeyi veri modelinde henüz sınıflandırılmadığı için arayüz
bir kaynağı “birincil” veya “doğrulanmış” diye iddia etmeyecektir.
