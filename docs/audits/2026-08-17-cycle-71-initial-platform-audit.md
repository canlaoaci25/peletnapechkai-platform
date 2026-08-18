# BOECL Çevrim 71 — ilk platform denetimi ve öncelikli 20 geliştirme

## Yönetici özeti

BOECL; Next.js App Router public/admin arayüzü, ASP.NET Core minimal API, EF Core/PostgreSQL,
Identity tabanlı rol yönetimi ve Windows/IIS operasyon betikleri olan çok dilli bir yayın platformudur.
Son çevrimler public sidebar, editoryal merkez, üyelik, kaynak güveni ve görsel inceleme kuyruğunda
güçlü temeller kurmuştur. Bu denetimin en kritik kanıtı, görsel yenileme işinin genel Codex worker
tarafından claim edilebilmesi fakat o worker'ın `VisualRenewal` promptu üretememesidir. En yüksek
görünür kalite açığı ise adminin yalnız 16:9 önizleme gösterirken public yüzeyin aynı görseli 1:1,
4:3, 16:10 ve manşet oranlarında kırpmasıdır.

## Mimari ve klasörler

- `apps/web`: Next.js 16 / React 19 App Router; public yayın, üyelik ve admin yüzeyleri.
- `apps/api`: .NET 10 ASP.NET Core API; Identity, EF Core, yayın ve otomasyon domainleri.
- `tests/api`: xUnit domain/API/persistence regresyonları.
- `ops/windows` ve `ops/tests`: IIS dağıtım, backup, health, worker, watchdog ve recovery sistemi.
- `docs`: mimari, editoryal, SEO, dağıtım, operasyon ve çevrim denetimleri.
- Paket yönetimi npm; sunucu bağımlılıkları NuGet. Desteklenen locale'ler `tr-TR`, `en-US`,
  `de-DE`, `fr-FR`; Türkçe kaynak locale'dir.

## Veri, API ve kimlik

PostgreSQL modeli makale grubu/localization, locale/region, kategori/tag ilişkileri, medya,
kaynak, editoryal görev, üyelik etkileşimi, otomasyon işi ve görsel inceleme görevlerini kapsar.
Migration ve indeks geçmişi mevcuttur. Identity cookie, rol politikaları ve antiforgery yönetim
mutasyonlarında uygulanır. Worker uçları ayrı token ile fail-closed çalışır. Bu çevrim production
verisi veya şeması değiştirmemiştir; backup gerektiren bir migration yoktur.

## Admin, içerik ve medya

Admin; yayın masası, taxonomy, locale, kütüphane, otomasyon ve Görsel Yenileme Stüdyosu sunar.
Görsel brief tam başlık/özet/gövde/H2-H3/kategori/locale bağlamından tür seçer. Aday terfisi konu,
yazısızlık, crop, özgünlük, lisans ve alt metin kapılarıyla transaction içinde yapılır. Buna karşın
otomatik hazır içerik yolu stok görseli AI adıyla kaydedebilir; inline provenance eksiktir ve yayın
öncesi görsel review görevini atlayabilir. Bu P1 editoryal bütünlük borcudur.

## SEO, performans ve erişilebilirlik

Locale route, self-canonical, hreflang, metadata, structured data, sitemap/robots ve responsive
`next/image` temelleri vardır. Makale hero'sunda boyut ve preload; gövde görsellerinde lazy/async
politikası bulunur. Public kartlarda görsel dekoratif, başlık bağlantısı erişilebilir kalır. Ana açık:
tek 16:9 crop puanı gerçek public odak kaybını ölçmez; AVIF/art-direction ve gerçek viewport kanıtı
tamamlanmamıştır.

## Güvenlik ve operasyon

HTML sanitize, upload doğrulama, path sınırı, authorization, CSRF, audit ve sabit zamanlı worker
token karşılaştırması olumlu. Pexels indirmesinde host allowlist/streaming byte limiti; generic worker
claim'inde atomik lease; provider retry/backoff/dead-letter ve provider health telemetry eksiktir.
IIS release/health/rollback ve autonomous watchdog mevcuttur. Bu izole worktree runner'ın merge ve
dağıtım sorumluluğunu devralmaz; GitHub ve canlı sistemlere dokunulmamıştır.

## Tasarım, içerik ve görsel kalite

Semantik light/dark token sistemi ve desktop sidebar/mobile drawer kararı uygulanmıştır. Görsel
stüdyo before/after, skorlar ve kalıcı batch sayıları gösterir. Çevrim 71 öncesinde gerçek public
oran matrisi yoktur. Konu eşleşmesi alt metin kelime ortaklığına, yazısızlık ise editör beyanına
fazla bağımlıdır; OCR/vision ikinci kapısı yoktur. Gerçek varlık, stok, özgün diagram ve AI kökeni
yapılandırılmış tek provenance sözleşmesine bağlanmalıdır.

## Teknik borç ve güncellik

Kod tabanı güncel ana platform sürümlerindedir; ölçülebilir gerekçe olmadan framework yükseltme veya
yeniden yazım önerilmez. Öncelik; mevcut akışlardaki yayın bypass'ı, worker sahipliği, provider
gözlemlenebilirliği, crop kanıtı ve içerik/provenance bütünlüğüdür. Bazı eski dokümanlarda “branded
cover” ve plain-text body ifadeleri mevcut davranış ve yazısız owner kararıyla çelişmektedir.

## Öncelikli ilk 20 geliştirme

| # | Öncelik | Faz | Kanıtlanabilir kabul sonucu |
|---:|:---:|---|---|
| 1 | P0 | VisualRenewal worker sahipliği | Genel worker görsel işi claim/time-out etmez; özel worker gelene dek fail-closed kalır. |
| 2 | P1 | Public görsel crop kanıt matrisi | Admin 16:9, manşet, 1:1, 4:3 ve 16:10 current/candidate önizlemesini gösterir. |
| 3 | P1 | Doğrulanmış görsel provenance | Köken, sağlayıcı, lisans, kaynak URL, atıf ve audit zamanı kapak/gövde için kalıcıdır. |
| 4 | P1 | Otomatik yayın görsel kapısı | Hazır içerik görselleri review tamamlanmadan published olmaz. |
| 5 | P1 | Ayrı görsel provider worker | Atomik lease, checkpoint, pause/resume/cancel ve idempotent item işleme. |
| 6 | P1 | Provider güvenliği | HTTPS/host allowlist, streaming boyut ve piksel limitleri, content-type/decode kontrolü. |
| 7 | P1 | Retry/dead-letter | Sınıflı hata, exponential backoff+jitter, üst sınır ve manuel müdahale durumu. |
| 8 | P1 | Vision/OCR kalite kanıtı | Konu, yazı/logo/filigran ve fiziksel artefact sinyali editör kararını destekler. |
| 9 | P2 | Focal point/art direction | Tek asset odağı tüm public componentlerde tutarlı uygulanır. |
| 10 | P2 | Locale görsel kanıtı | Doğal alt metin ve kültürel uygunluk her gerçek localization için ayrı doğrulanır. |
| 11 | P2 | Inline görsel provenance | Her gövde görseli H2/H3, amaç, lisans ve atıfla ilişkilidir. |
| 12 | P2 | Provider health paneli | Auth/config, kota, latency, hata oranı ve son başarı secret sızmadan görünür. |
| 13 | P2 | Görsel batch tamamlama | Sayaçlar item transaction'ıyla güncellenir; terminal durum idempotent oluşur. |
| 14 | P2 | Gerçek viewport regresyonu | 390/768/1440 light+dark screenshot matrisi crop ve taşmayı doğrular. |
| 15 | P2 | AVIF/WebP bütçesi | Public varyantlar ölçülen byte ve LCP bütçesiyle sunulur. |
| 16 | P2 | Public schema/image SEO | ImageObject, alt, credit ve image sitemap gerçek varlık metadata'sıyla uyumludur. |
| 17 | P2 | Arama/öneri kalitesi | Yazım toleransı ve boş sonuç kurtarma ölçülür. |
| 18 | P2 | Locale yayın bütünlüğü | Çeviri, taxonomy ve hreflang açıkları otomatik kapıda görünür. |
| 19 | P2 | Core Web Vitals release bütçesi | LCP/CLS/INP ölçümü release kararına bağlanır. |
| 20 | P3 | Doküman ve runbook doğruluğu | Public body ve yazısız görsel kuralları güncel kodla eşleşir. |

## Bu çevrimin seçilen fazı

Önce: editör yalnız 16:9 current/candidate karşılaştırması görüyordu; mobil veya kart kırpmasında ana
öznenin kesildiğini terfi öncesi fark edemiyordu. Sonra: aynı iki asset gerçek public oranlarında tek
kanıt matrisinde görünür. Destekleyici güvenlik düzeltmesi, özel görsel işi generic worker kuyruğundan
ayırır. Bu faz dış provider varmış veya otomatik vision tamamlanmış iddiasında bulunmaz.
