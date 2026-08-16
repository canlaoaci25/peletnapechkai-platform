# BOECL ilk çalıştırma ana denetimi — Çevrim 17

Tarih: 16 Ağustos 2026  
Odak: ana sayfa, global navigasyon ve görünür tasarım dönüşümü  
Kapsam: depo mimarisi, canlı/staging yüzeyleri, son 20 commit, kalıcı yol haritası, veri ve içerik envanteri, SEO, erişilebilirlik, güvenlik, operasyon ve görsel sistem.

## Yönetici özeti

BOECL; Next.js 16 App Router web uygulaması, ASP.NET Core 10 API, PostgreSQL 18, EF Core migrations ve IIS/Windows Service dağıtımıyla çalışan dört dilli bir yayın platformudur. Kimlik, rol tabanlı yönetim, editoryal iş akışı, medya, otomasyon, homepage kürasyonu, sitemap/RSS, canonical/hreflang ve staging/production sağlık kapıları mevcuttur. Son doğrulanan envanterde 663 yayımlanmış lokalizasyon, 359 medya kaydı ve 680 sitemap URL’si vardır; kritik veri bütünlüğü hatası raporlanmamıştır.

Bu çevrimdeki en yüksek getirili güvenli ürün problemi global keşiftir. Canlı ana sayfanın üst menüsü yalnız üç genel arama sorgusuna bağlanmakta; gerçek kategori arşivleri global navigasyonda görünmemekte, mobil masthead araçları kaybolmakta ve 390 px render’da manşet taşmaktadır. Bu durum mevcut güçlü içerik/API altyapısının ziyaretçiye yansımasını engeller. Çevrimin uygulanacak fazı, veri kaybı veya migration gerektirmeden gerçek taxonomy ile beslenen global masthead ve yeniden dengelenmiş responsive ana sayfadır.

## Teknoloji ve klasör mimarisi

- `apps/web`: Next.js 16.3.0, React 19.2.8, TypeScript 5, Tailwind CSS 4; App Router, server components ve locale route grubu.
- `apps/api`: ASP.NET Core 10 minimal API, EF Core/Npgsql, Identity cookie authentication, background workers ve yayın servisleri.
- `tests/api`: kimlik, yayın, otomasyon, sanitization, veri modeli ve PostgreSQL integration testleri.
- `ops/windows`: atomik web/API deploy, staging/production health, PostgreSQL backup/restore, sitemap, IndexNow ve otonom worker betikleri.
- `docs`: mimari, veri, kimlik, SEO, yayın, operasyon, staging ve yol haritası karar kayıtları.

## Database, API ve kimlik

PostgreSQL modeli; article group/localization, category/tag/source/author, SEO metadata, media variants, revision, audit log, homepage placement, engagement, üyelik ve otomasyon varlıklarını kapsar. Unique locale/slug ve grup/locale ilişkileri migration ve testlerle korunur. API public content/homepage/archive uçları ile yetkili admin uçlarını ayırır. ASP.NET Identity; Owner, Admin, Editor, Author, Translator ve SEO görev ayrımını, antiforgery ve audit izini uygular. Bu çevrimde şema değişikliği gerekmemektedir.

## Admin, içerik ve medya

Admin; makale editörü, workflow, taxonomy, medya kütüphanesi, homepage kürasyonu, kullanıcı/dil yönetimi, Knowledge Vault, trafik ve otomasyon merkezlerini içerir. İçerik tarafındaki başlıca borçlar önceki ölçümdeki 208 kapaksız yayın ve eski Markdown benzeri gövdeli 245 lokalizasyondur. Görsel kalite politikası konu eşleşmesi ve optimize variant alanları içerir; ancak başlık-görsel eşleşmesi tüm eski arşiv için insan örneklemesiyle doğrulanmış değildir.

## SEO, performans ve erişilebilirlik

Locale-aware metadata, self-canonical, hreflang/x-default, sitemap, RSS, robots, Article/WebSite/Organization structured data ve draft noindex davranışı vardır. Responsive `next/image`, ölçülü preload ve alt yüzeylerde `content-visibility` kullanılır. Canlı gözlemde mobil manşet taşması, kategori yerine arama bağlantıları ve dar ekranda eksilen keşif araçları P2 ürün/SEO kusurudur. Global navigasyonun sırası tutarlı olmalı, gerçek arşiv URL’leri kullanılmalı ve 44 px dokunma hedefleri korunmalıdır.

## Güvenlik ve operasyon

HTML sanitization, antiforgery, rate limiting, güvenli cookie/authorization, upload doğrulaması, loopback API, HTTPS ve atomik rollback’li deploy mevcuttur. Bilinen npm/NuGet açığı son denetimde sıfırdır. Açık savunma borçları CSP Report-Only tasarımı, HSTS/clickjacking/Permissions-Policy doğrulaması ve makine dışı yedektir. Production verisi, IIS binding’i, secret, DNS ve ödeme kapsam dışıdır.

## Tasarım, içerik ve görsel değerlendirmesi

Mevcut açık/koyu token tabanı güçlü bir başlangıçtır; fakat masthead küçük bir ürün header’ı gibi davranmakta, BOECL’in yayın kimliği ve kategori mimarisi görünmemektedir. Ana sayfanın lead/secondary/trending/picks/latest yüzeyleri veri bakımından yeterlidir; tipografik ölçek ve grid mobilde sınırlandırılmalıdır. Yeni kategori açmak için bu çevrimde kanıt yoktur: mevcut Türkçe arşivde Anime, Dijital Yaşam, Donanım, Siber Güvenlik, Verimlilik ve Yapay Zekâ kategorileri vardır ve önce bunların keşfedilebilirliği çözülmelidir. Yeni toplu içerik veya jenerik görsel üretmek yerine mevcut özgün kapakların daha iyi sunulması daha yüksek getiridir.

## Teknik borç ve güncelleme görünümü

Next/React güncel 2026 ailesindedir. BlockNote/Mantine ve bazı .NET paketlerinde yeni sürümler bulunabilir; major yükseltmeler ayrı uyumluluk fazı ister. C# kaynaklarında geniş format/line-ending borcu vardır ve görünür ürün fazıyla karıştırılmamalıdır. Public header metinlerinin dictionary dışında tutulması ve sıkıştırılmış component kaynakları sürdürülebilirlik borcudur.

## Öncelikli ilk 20 geliştirme

1. **P2 — Global taxonomy navigasyonu:** gerçek, lokalize kategori arşivlerini tüm public yüzeylerde tutarlı masthead’e bağla. **Bu çevrim.**
2. **P2 — Responsive ana sayfa:** 320–1440 px’de taşmayan manşet, belirgin lead/secondary hiyerarşisi ve taranabilir akış. **Bu çevrim.**
3. **P2 — Global dil geçişi:** sayfa eşdeğerlerini koruyan görünür locale seçici; olmayan çeviride kontrollü davranış.
4. **P2 — Arşiv görsel sistemi:** kategori/tag sayfalarına tutarlı responsive kart grid’i ve açıklayıcı üst bilgi.
5. **P2 — Makale okuma yüzeyi:** kaynak, yazar, güncellik ve ilişkili içerik sinyallerini mobil öncelikli güçlendir.
6. **P2 — Kapaksız öncelikli yayınlar:** trafik/değer sırasına göre konuya özgü, yazısız ve lisans/audit kayıtlı kapaklar.
7. **P2 — Homepage kürasyon ölçümü:** lead/pick tıklama ve görünürlük metrikleri; slot bazlı raporlama.
8. **P1 — CSP aşamalı geçişi:** kaynak envanteri, Report-Only telemetrisi ve doğrulama sonrası enforce.
9. **P1 — Transport/clickjacking başlıkları:** staging’de HSTS, `frame-ancestors` ve Permissions-Policy doğrulaması.
10. **P1 — Off-site yedek:** şifreli uzak kopya ve düzenli restore tatbikatı.
11. **P2 — Eski gövde normalizasyonu:** yedekli, önizlemeli, transaction’lı ve geri alınabilir Markdown→kanonik içerik operasyonu.
12. **P2 — Görsel-semantic kalite kapısı:** kapak/başlık eşleşmesi için örnekleme, tekrar algılama ve editoryal ret nedeni.
13. **P2 — İç link grafiği:** orphan içerik, topic cluster ve locale bazında zayıf bağlantıları ölçüp editöre öner.
14. **P2 — Web Vitals telemetrisi:** LCP/CLS/INP’yi route, viewport ve locale kırılımında topla.
15. **P2 — Arama deneyimi:** kategori/tag filtreleri, boş durum önerileri ve typo-tolerant yaklaşım için ölçüm.
16. **P2 — Üyelik değeri:** kaydetme/okuma listesi ve locale tercihlerini gerçek kullanıcı akışına bağla.
17. **P2 — Editoryal güven merkezi:** yöntem, düzeltme, AI kullanımı ve kaynak politikalarını global footer’dan görünür kıl.
18. **P3 — Otomasyon kapasite paneli:** faz süreleri, p95, hata oranı ve günlük üretim kapasitesi.
19. **P3 — Format/CI standardı:** C# normalizasyonunu ayrı commit ve `dotnet format --verify-no-changes` kapısıyla tamamla.
20. **P4 — Kontrollü dependency yamaları:** küçük paket gruplarıyla tam regresyon matrisinden geçir.

## Çevrim 17 kabul kriterleri

- Global header dört locale’de dictionary ve gerçek taxonomy verisiyle çalışır.
- Ana kategori bağlantıları arama yerine lokalize `/categories/{slug}` arşivlerine gider.
- Masaüstü ve mobilde arama, hesap, tema ve keşif menüsü erişilebilir kalır.
- Ana sayfa 320, 375, 390, 768, 1024 ve 1440 px’de yatay taşma üretmez.
- Lead görseli LCP önceliğini, alt görseller lazy/responsive davranışını korur.
- Locale, lint, typecheck, web test/build, API test ve Release build kapıları geçer.
- Staging ve production canlı doğrulaması geçmeden çalışma tamamlanmış sayılmaz.

## Araştırma ilkeleri

The Verge’in kalıcı üst kategori şeridi ve ayrıntılı keşif çekmecesi; global yayınlarda hızlı tarama ile derin keşfin ayrı katmanlarda çözüldüğünü gösterir. WCAG 2.2 tutarlı navigasyon ilkesi, tekrar eden menülerin göreli sırasını korumayı gerektirir. Google Search Central site-name rehberi, ana sayfada tutarlı WebSite/Organization kimliğini destekler. BOECL uygulaması bu prensipleri özgün tipografi, renk ve taxonomy’siyle uygular; herhangi bir yayın tasarımını kopyalamaz.
