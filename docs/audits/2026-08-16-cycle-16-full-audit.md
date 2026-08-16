# BOECL Çevrim 16 — Tam Proje Denetimi ve İlk 20

Tarih: 16 Ağustos 2026  
Odak: otomasyon, hata kurtarma ve canlı dağıtım güvenilirliği

## Yönetici özeti

BOECL; Next.js 16 App Router, React 19, ASP.NET Core 10, EF Core 10 ve PostgreSQL 18 üzerinde çalışan, `tr-TR`, `en-US`, `de-DE` ve `fr-FR` yayınlarını aynı içerik ilişkileriyle yöneten bir platformdur. Production ve staging Windows/IIS mimarisi, atomik web/API dizin değişimi, rollback klasörü, sağlık kontrolleri, günlük PostgreSQL yedeği, audit logu ve otomatik içerik worker'ı mevcuttur. Çalışma ağacı denetim başlangıcında temizdir; `main`, `origin/main` ile eşittir.

En yüksek getirili güvenli bulgu şudur: production sağlık görevi servisleri, locale URL'lerini, admin CSRF ucunu, disk alanını ve TLS ömrünü ölçüp `C:\ProgramData\Peletnapechkai\Health\latest.json` dosyasına yazıyor; ancak yönetici paneli bu sonucu okumuyor. Paneldeki “Canlı sistem” beyanı yalnızca anlık veritabanı ve depolama sayımlarına dayanıyor. Operatör son kontrolün bayatladığını, bir public endpoint'in düştüğünü veya TLS eşiğinin yaklaştığını arayüzden göremiyor. Çevrim 16'nın uygulama fazı bu kopukluğu uçtan uca kapatacaktır.

## Mimari ve klasörler

- `apps/web`: Next.js 16.3.0 standalone output, React 19.2.8, TypeScript 5, Tailwind 4; public site, üyelik ve admin.
- `apps/api`: ASP.NET Core 10 minimal API; Identity, antiforgery, rate limiting, EF Core ve background worker'lar.
- `tests/api`: domain, auth, persistence, yayın ve otomasyon testleri.
- `ops/windows`: staging/production deploy, rollback, health, backup/restore, autonomous worker ve recovery betikleri.
- `config/supported-locales.json`: dört etkin locale için ortak kaynak; web tarafında üretilen locale kataloğuna dönüştürülüyor.
- `docs`: mimari, yayın, SEO, kimlik, operasyon ve kalıcı yol haritası.

## Veri, kimlik ve yayın modeli

PostgreSQL şeması locale, region/country, article group/localization, revision, SEO, taxonomy, source, author, media, homepage placement, engagement, knowledge vault, editorial collaboration, automation job ve schedule ilişkilerini kapsar. Migration geçmişi izlenebilir; eşzamanlı edit için concurrency alanları ve audit kayıtları vardır. ASP.NET Core Identity; Owner/Admin/Editor/SEO/Translator gibi politikalar, güvenli cookie, antiforgery ve rate limiting ile korunur. CMS HTML'i sunucuda sanitize edilir; taslak/önizleme içeriği public indeksleme yoluna karıştırılmaz.

## SEO, performans, erişilebilirlik ve görsel sistem

Locale-aware canonical, hreflang, sitemap/feed, robots, structured data ve çevrilmiş kategori arşivleri bulunur. Next standalone sunucu IIS reverse proxy arkasındadır. Responsive image yardımcıları, hero önceliği, gövde görsellerinde lazy loading, ertelenmiş entegrasyonlar ve mobil admin iyileştirmeleri vardır. Skip link, klavye navigasyonu ve semantic outline için testler mevcuttur. Açık içerik borcu: çok sayıda eski yayında konuya özgü kapak bulunmaması ve eski Markdown-benzeri gövdelerin runtime dönüşümüne dayanmasıdır. Gerçek varlık/haber görsellerinde lisans ve teknik doğruluk kalite kapısı korunmalıdır.

## Otomasyon ve hata kurtarma

Automation job/schedule modeli, heartbeat ve worker endpoint'leri; istek parmak iziyle aynı girdiye ait yarım kalmış sonucu yeniden kullanabilen PowerShell recovery katmanı ve parser/regresyon testleri vardır. Bununla birlikte gerçek process iptali ile veritabanı `Pause/Cancel` durumları tam bağlı değildir; retry deneme sayaçları ayrı attempt modeli taşımamaktadır. Bunlar kaynak tüketimi ve operatör güveni açısından P1'dir.

## Canlı dağıtım ve operasyon

Web ve API deploy betikleri release dizini oluşturur, aktif dizini rollback'e taşır, hizmeti başlatır ve health kapısından sonra sürümü kabul eder; hata halinde eski dizini geri getirir. Production health görevi servis, dört locale, CSRF, disk ve sertifika ömrünü denetler. Günlük custom-format PostgreSQL yedeği ve izole restore testi vardır. Off-site kopya henüz etkin değildir. Deploy sonucu için kalıcı, ortak bir release ledger/commit kimliği bulunmaması ve mevcut health JSON'unun admin/API'de görünmemesi temel operasyon boşluklarıdır.

## Güvenlik ve teknik borç

Auth/authorization, CSRF, rate limiting, forwarded-header sınırı, HTML sanitization, upload doğrulaması ve loopback servisleri olumlu katmanlardır. CSP, HSTS ve clickjacking/Permissions-Policy tamamlanmamıştır; CSP önce Report-Only telemetrisiyle açılmalıdır. Off-site backup yokluğu makine kaybında RPO riskidir. Yeni locale'in admin tarafından etkinleştirilmesi ile build-time UI sözlüğünün hazır olması aynı lifecycle ile korunmamaktadır. Format borcu geniştir fakat ölçülebilir ürün faydası vermeyen toplu refactor bu çevrimin konusu değildir.

## Öncelikli ilk 20 geliştirme

| Sıra | Öncelik | Geliştirme | Kabul kanıtı |
|---:|:---:|---|---|
| 1 | P1 | Production health sonucunu tazelik ve kapı ayrıntılarıyla admin ana ekranına bağla | Bozuk/eksik/bayat snapshot fail-safe; API ve responsive UI testleri |
| 2 | P1 | Deploy release ledger'ı: environment, bileşen, commit, faz, health, rollback sonucu | Atomik kayıt; admin geçmişi; sır içermeyen audit |
| 3 | P1 | Pause/Cancel için worker lease ve kooperatif process sonlandırma | `Cancelling` ara durumu ve yarış koşulu regresyonları |
| 4 | P1 | Retry attempt modelini sayaç/checkpoint semantiğiyle ayır | Eski sayaç sızıntısı olmayan retry testleri |
| 5 | P1 | Web ve API'yi tek promotion transaction/orchestrator ile staging→production geçir | Bir bileşen hata verirse belgeli rollback ve tutarlı release |
| 6 | P1 | Health scheduler bayatlığı, art arda hata ve TLS eşiği için uyarı kanalı | Dedup/throttle ve teslim kanıtı |
| 7 | P1 | Off-site şifreli yedek ve düzenli restore tatbikatı | İkinci failure domain'den doğrulanmış restore |
| 8 | P1 | Locale lifecycle'a `UiReady/PublicEnabled` kapısı ekle | Sözlük/rota/yasal metin hazır değilse etkinleştirme reddi |
| 9 | P1 | Ülke-locale yönlendirmesinde tekillik/öncelik kuralı | DB constraint veya açık priority ve çakışma testi |
| 10 | P2 | CSP Report-Only telemetrisi, ardından enforce; frame ve permissions policy | Entegrasyon regresyonu olmadan header testi |
| 11 | P2 | Otomasyon p50/p95 süre, başarı oranı, retry ve recovery metrikleri | Admin trend görünümü ve ölçülebilir SLO |
| 12 | P2 | Takılı job watchdog ve güvenli otomatik recovery | Lease süresi aşımında tek recovery, çift çalışma yok |
| 13 | P2 | Deploy sonrası sentetik public yolculuk: locale, arama, makale, medya, üyelik | Staging ve production smoke raporu |
| 14 | P2 | Öncelikli kapaksız yayınları konuya özel, yazısız ve lisans izli görsellerle tamamla | Mobil crop, alt text, boyut ve tekrar kalite kapısı |
| 15 | P2 | Eski Markdown gövdelerini yedekli, önizlemeli, rollback'li normalize et | Transaction, idempotency ve render karşılaştırması |
| 16 | P2 | SEO/kapak eksik yayımlar için yayın öncesi bloklayıcı kalite politikası | Dört locale regresyonu ve noindex güvenliği |
| 17 | P2 | Ana sayfa modüllerinde başarısız API için stale-safe/empty/error sunumu | Mobil ve masaüstü gerçek render kontrolü |
| 18 | P2 | Core Web Vitals ve API latency ölçümünü release kimliğiyle ilişkilendir | Önce/sonra dashboard ve regresyon eşiği |
| 19 | P3 | PostgreSQL yavaş sorgu/N+1 gözlemi ve kanıtlı indeks iyileştirmeleri | Query plan ve entegrasyon testi |
| 20 | P3 | CI kalite kapılarını PowerShell parser, locale, audit ve release artifact doğrulamasıyla birleştir | Tekrarlanabilir temiz pipeline |

## Çevrim 16 görünür hedefi

Önce: yönetici “Canlı sistem” yazısını görür fakat public locale uçlarının, servislerin, TLS'nin ve health görevinin tazeliğini göremez.  
Sonra: admin kontrol merkezinde canlı yayın güveni; sağlıklı/riskli/ulaşılamıyor durumu, son kontrol yaşı, çalışan servis ve başarılı endpoint sayıları, disk, TLS ömrü ve güvenli hata özetiyle masaüstü ve mobilde görünür.

## Araştırma dayanağı

Next.js self-hosting kılavuzu reverse proxy, graceful shutdown ve tutarlı deployment varlıklarını; Microsoft güvenli deployment rehberi ise her rollout fazından önce health kapısı, staging ve geri dönüş yolunu önerir. Mevcut BOECL atomik dizin swap yaklaşımı bu prensiplere uygundur; eksik olan health kanıtının operatöre görünürlüğü ve release kimliğiyle sürekliliğidir.

