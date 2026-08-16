# BOECL ilk çalıştırma ana denetimi — Çevrim 18

Tarih: 16 Ağustos 2026  
Odak: içerik keşfi, kategori mimarisi ve yeni Türkçe taxonomy

## Yönetici özeti ve ölçülen önce/sonra hedefi

BOECL; Next.js 16.3/React 19 web, ASP.NET Core 10 API, EF Core ve PostgreSQL 18 veri katmanı, IIS/Windows Service dağıtımı ve dört etkin locale ile çalışan bir yayın platformudur. Kimlik ve rol ayrımı, antiforgery, audit izi, HTML sanitization, medya optimizasyonu, editoryal durumlar, canonical/hreflang, sitemap/RSS ve atomik deploy/health kapıları mevcuttur.

Canlı Türkçe envanterde 201 yayın olmasına rağmen kategori arşivlerinde toplam 109 ilişki görünmektedir. Altı kategori 8–24 yayın arasında kalırken telefon, Android/iPhone, giyilebilir cihaz ve mobil bağlantı içerikleri geniş “Dijital Yaşam” kovasında kaybolmaktadır. Arşiv kartları kapak göstermediği, bütün konular için bir merkez bulunmadığı ve kategori açıklamalarının çoğu boş olduğu için yeni masthead bağlantıları keşfi tek başına tamamlamamaktadır.

Bu çevrimin görünür hedefi: `/tr-TR/topics` üzerinde içerik sayıları ve gerçek yayın kapaklarıyla tüm konu haritası; görsel kategori arşivleri; dört locale bağlı yeni “Mobil Teknoloji” taxonomy’si ve mevcut uygun article group’larının transaction içindeki idempotent migration ile ilişkilendirilmesidir.

## Mimari ve kalite denetimi

- **Klasörler:** `apps/web` App Router public/admin; `apps/api` minimal API/domain/persistence/workers; `tests/api`; `ops/windows`; `docs`.
- **Veri:** article group/localization, category/tag, medya, yazar/kaynak, kalite kontrolü, revizyon, homepage, engagement, üyelik ve automation ilişkileri. Locale+slug ve source-category+locale unique indeksleri taxonomy bütünlüğünü koruyor.
- **Kimlik/güvenlik:** ASP.NET Identity cookie, Owner/Admin/Editor/Author/Translator/SEO rolleri, antiforgery, rate limit, upload magic-byte/boyut kontrolü, public URL doğrulaması ve append-only audit log var. CSP enforce, dış yedek ve güvenlik başlığı ölçümü açık borçtur.
- **SEO/i18n:** self-canonical, category hreflang ilişkileri, locale route, sitemap, robots ve structured data var. Topic hub canonical eklenmeli; çevrilmemiş makale sessiz fallback yapmıyor.
- **Performans:** responsive Next Image ve optimize WebP mevcut. Topic hub yalnızca ilk üç özet/kapak verisini alır; LCP dışı kart görselleri varsayılan lazy davranır.
- **UX/erişilebilirlik:** açık/koyu token sistemi ve klavye odakları mevcut. 320–1440 px arşiv grid’i, anlamlı heading düzeni ve görsele bitişik konu bağlamı bu fazın kabul kapısıdır.
- **İçerik/görsel:** 201 Türkçe yayının 108’i Guide, 74’ü Review, 11’i Analysis, 8’i News. Bu dağılım güncel haberden çok evergreen/review ağırlığına işaret ediyor. Yeni yapay görsel gerekmez; mevcut konuya özgü, yazısız kapaklar yeniden kullanılır.
- **Operasyon:** staging/production health, yedek, migration, rollback ve atomik deploy betikleri var. Şema/veri değişikliği öncesi PostgreSQL custom-format yedek zorunludur.

## Öncelikli ilk 20 geliştirme

1. **P2 — Konu merkezi + Mobil Teknoloji taxonomy (bu çevrim).**
2. **P2 — Kategorisiz 92 yayının editoryal sınıflandırılması ve kalite raporu.**
3. **P2 — Kategori açıklamalarını admin üzerinden dört locale edit edebilme.**
4. **P2 — Kategori arşivlerinde sayfalama ve toplam sonuç bilgisi.**
5. **P2 — Topic cluster/iç link grafiği ve orphan içerik uyarıları.**
6. **P2 — Bilim başlığı iddiasını arşiv/trend verisiyle doğrulayıp gerçek yayın planına bağlama.**
7. **P2 — Yazılım/geliştirici araçları için ayrı taxonomy aday ölçümü.**
8. **P2 — Kategori başına özgün editoryal seçki ve homepage modülü.**
9. **P2 — Kapaksız 208 eski lokalizasyon için trafik öncelikli görsel programı.**
10. **P2 — Eski Markdown gövdelerinin yedekli ve geri alınabilir normalizasyonu.**
11. **P2 — Arama filtreleri: kategori, içerik türü ve tarih.**
12. **P2 — Üyelikte konu takip etme, kaydetme ve okuma listesi.**
13. **P2 — Kategori/slot bazlı CTR ve görünürlük ölçümü.**
14. **P2 — Route/locale/viewport bazlı LCP, CLS ve INP telemetrisi.**
15. **P1 — CSP Report-Only envanteri ve kontrollü enforce geçişi.**
16. **P1 — HSTS, frame-ancestors ve Permissions-Policy canlı doğrulaması.**
17. **P1 — Şifreli off-site yedek ve düzenli restore tatbikatı.**
18. **P2 — Editoryal güven merkezi: düzeltme, AI ve kaynak politikaları.**
19. **P3 — Automation p95 süre/hata/kapasite paneli.**
20. **P4 — Kontrollü paket güncellemeleri ve C# format CI kapısı.**

## Araştırma dayanağı ve kabul kriterleri

Google Search Central mantıksal site yapısı, açıklayıcı URL, ilgili iç bağlantı ve bağlama yakın kaliteli görsel önerir. WCAG 2.4.5 içerik bulmak için birden fazla yol sunmayı hedefler. Uygulama; header, konu merkezi, kategori arşivi ve arama yollarını birlikte sunar, herhangi bir rakip tasarımı kopyalamaz.

Kabul: yeni taxonomy dört locale bağlı ve yalnız yayınlanmış eş içeriklerle görünür; migration tekrar çalıştırılabilir/rollback’li/audit izli; topic hub ve arşivler mobil-masaüstünde taşmaz; canonical ve semantic heading korunur; locale, web ve API kalite kapıları ile staging/production canlı doğrulaması geçer.
