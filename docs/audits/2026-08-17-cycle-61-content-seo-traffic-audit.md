# Çevrim 61 — Türkçe içerik, SEO, kaynak kalitesi ve trafik denetimi

Tarih: 17 Ağustos 2026. Kapsam: canlı/staging public yüzeyler, admin, son 20 commit, kalıcı roadmap, Next.js web, ASP.NET Core API, PostgreSQL veri modeli, kimlik, SEO, içerik, medya, test ve Windows/IIS operasyonları.

## Mimari ve mevcut durum

- Web: Next.js 16.3 App Router, React 19.2 ve TypeScript 5; locale köklü public/admin rotaları, sunucu bileşenleri ve API BFF katmanı.
- API/veri: ASP.NET Core 10, EF Core 10 ve PostgreSQL 18; makale grubu/lokalizasyon, taxonomy, yazar, kaynak, medya, SEO, checklist, revision, audit, üyelik ve otomasyon ilişkileri migration ile izleniyor.
- Kimlik/güvenlik: ASP.NET Identity cookie, rol/policy yetkileri, antiforgery, rate limit, HTML temizleme, güvenli upload ve public medya erişim kapısı mevcut.
- SEO: locale-aware canonical/hreflang, sitemap, robots, RSS, Article/Breadcrumb JSON-LD, kaynak `citation` alanları ve draft noindex mevcut.
- İçerik/admin: dört dilde editoryal iş akışı; kaynak türü/inceleme, trafik ve otorite kuyruğu, çeviri revizyon izi ve görsel kalite masası mevcut.
- Tasarım/performans: public sidebar/drawer, açık-koyu semantik tokenlar, responsive Next Image, LCP preload ve ekran altı içerik görünürlüğü uygulanmış.
- Operasyon/test: atomik IIS staging/production deploy, rollback, health/public smoke, PostgreSQL backup/restore, deployment journal; web, API ve PowerShell regresyon kapıları mevcut.

## Kanıtlanan ürün açığı ve önce/sonra hedefi

Kaynak Merkezi ve makale güven katmanı mevcut olsa da ana sayfa seçkileri kaynak çeşitliliğini görünür kılmıyor; güçlü kaynaklı Türkçe yayınlar yalnız kronoloji, manuel seçim veya etkileşim skoruyla keşfediliyor.

Önce: okur manşet, popüler, konu atlası ve editör seçkisini görür; hangi içeriğin birden fazla bağımsız kaynak alanına ve kayıtlı kaynak incelemesine dayandığını ana sayfada ayırt edemez. Sonra: API gerçek kaynak ilişkilerinden en az iki kaynak ve iki bağımsız alan koşuluyla güvenli bir seçki üretir; ana sayfa kaynak/alan/inceleme kanıtını dört locale’de gösterir ve Kaynak ve Güven Merkezi’ne taranabilir iç bağlantı verir. İçerik uydurulmaz ve “doğrulanmış” iddiası yapılmaz.

## Öncelikli ilk 20 geliştirme

1. P2 — Ana sayfada kaynak derinliği kanıtlı yayın seçkisi. **Bu çevrim.**
2. P1 — Bağımsız görsel servisinde production sağlayıcı, vision kalite ve sağlık raporu.
3. P1 — Yayın arşivi görsel backfill/checkpoint ve istisna raporu.
4. P2 — Türkçe evergreen tazelik kuyruğu ve public güncelleme notları.
5. P2 — Search Console sorgu-content gap eşleme merkezi.
6. P2 — Konu kümeli, editör onaylı iç bağlantı önerileri.
7. P2 — Kaynak URL sağlık taraması ve son erişim kanıtı.
8. P2 — Birincil kaynak oranı ve kaynak çeşitliliği hedefleri.
9. P2 — Kategori authority hub’larında pillar/rehber ayrımı.
10. P2 — Türkçe arama yazım toleransı ve boş sonuç kurtarma.
11. P2 — Locale çeviri kapsamı ve kaynak revizyon sapması yayın kapısı.
12. P2 — Yazar uzmanlık profilleri ve doğrulanabilir yayın geçmişi.
13. P2 — Homepage seçkisi CTR/engagement çeşitlilik ölçümü.
14. P2 — Kapaksız Türkçe içerik için trafik öncelikli görsel kuyruğu.
15. P2 — Gövde görsellerinde bölüm eşleşmesi ve locale alt metin kapısı.
16. P2 — Mobil LCP/CLS/INP release bütçesi.
17. P1 — CSP enforce ve security-header canlı matrisi.
18. P1 — Off-site şifreli yedek ve bağımsız restore tatbikatı.
19. P3 — Public sorguların plan/index ve pagination ölçümü.
20. P3 — Admin ve public gerçek browser görsel regresyon otomasyonu.

## Kabul ve risk

Seçki yalnız yayımlanmış locale içeriğinden ve mevcut kaynak ilişkilerinden oluşmalı; en az iki kaynak ve iki canonical alan koşulunu korumalıdır. Dört locale sözlükleri, semantik başlık, açık/koyu tema, mobil/masaüstü render, API sözleşmesi, lint/typecheck/build ve .NET test/build geçmelidir. Staging ve production canlı doğrulaması olmadan tamamlanmış sayılmaz.

Kalan risk: kaynak türü ve editoryal inceleme kaydı, kaynağın her iddiasının bağımsız fact-check sonucu değildir. Arayüz bu nedenle yalnız “kaynak incelemesi kaydı” gösterir; “doğrulanmış içerik” iddiası yapmaz.
