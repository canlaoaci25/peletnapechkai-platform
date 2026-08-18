# BOECL Çevrim 85 — ilk platform denetimi

## Görünür hedef

Yönetici, Görsel Yenileme Stüdyosu'nda hangi kaynak sınıfının gerçekten aday sağlayabildiğini,
hangisinin yalnız editoryal incelemeye açık olduğunu ve hangi dış sağlayıcının owner kararı veya
korumalı yapılandırma eksikliği nedeniyle kapalı kaldığını gerçek API durumuyla görecek. Sistem,
kapalı bir sağlayıcıyı sessiz fallback ile kullanılabilir göstermeyecek.

## Mimari ve mevcut durum

- **Teknoloji:** Next.js 16.3, React 19.2 ve TypeScript 5 public/admin web; ASP.NET Core `net10.0`,
  EF Core 10 ve PostgreSQL API/veri katmanı. Kök npm scriptleri lint, typecheck, web build/test ve
  locale sözleşmesini; solution .NET test/build kapılarını çalıştırıyor.
- **Veri ve API:** Locale, taxonomy, yayın, kaynak, medya, üyelik, editoryal görev, otomasyon işi ve
  audit modelleri ilişkisel tutuluyor. Migration geçmişi ve optimistic concurrency mevcut.
- **Kimlik ve güvenlik:** ASP.NET Identity cookie auth, rol politikaları, antiforgery, rate limit,
  HTML sanitization, güvenli medya yolu çözümleme ve secret-store tabanlı ortam sözleşmesi var.
- **Yayın ve admin:** Ana sayfa keşfi, gerçek taxonomy sidebar'ı, arama kurtarması, üyelik akışları,
  editoryal merkez, kaynak/güven görünümü ve görsel yenileme stüdyosu bulunuyor.
- **SEO/i18n:** `tr-TR` kaynak yayın dili; `en-US`, `de-DE`, `fr-FR` gerçek yayın karşılıkları için
  locale route, self-canonical, hreflang, sitemap, robots, Article ve Breadcrumb verileri mevcut.
- **Görsel sistem:** Tam metin/bölüm briefi, yazısız negatif prompt, kalıcı kuyruk, checkpoint,
  perceptual hash, public crop kanıtı, lisans/alt metin ve fail-closed editoryal terfi var. Production
  generation/vision provider ve yönetilen gövde görseli yerleşimi tamamlanmış değil.
- **Operasyon:** İzole worktree, canlı durum kaydı, staging/production health scriptleri, atomik
  release/rollback, backup/restore-test ve continuity supervisor altyapısı var. Bu çevrimde production
  secret veya veri okunmadı; GitHub/push işlemi geçici owner kararıyla kapalı.
- **Tasarım/UX:** Public desktop sidebar ve kapalı mobil drawer tamamlanmış; semantik tema tokenları
  yaygın. Admin görsel stüdyosunda kuyruk ve aday kanıtı güçlü, fakat provider sağlığı görünmüyordu.
- **İçerik/editoryal risk:** Son tarif önceliği görünür trafik fırsatı yaratıyor; buna karşın Recipe
  veri modeli/schema, gıda güvenliği iddia kapısı ve iddia düzeyi atıf tamamlanmadan ölçeklenmemeli.

## Kritik bulgular

1. **P1:** Harici görsel sağlayıcı yeteneği/sağlığı API'de açık bir sözleşme değildi; admin operatörü
   “kapalı”, “inceleme-only” ve “hazır” ayrımını göremiyordu.
2. **P1:** Uzun içerikte `missing-body-visual` ölçülüyor ancak görev/terfi modeli yalnız kapağı yönetiyor.
3. **P1:** Otomatik tarif kampanyası iki farklı kaynak hostunu kontrol ediyor; gıda güvenliği ve
   iddia-kaynak bağı için eşdeğer bir kapı yok.
4. **P1:** Tarifler yalnız Article JSON-LD alıyor; gerçek alanlardan üretilen Recipe modeli yok.
5. **P1:** Çeviri akışı yayın kaynağını doğruluyor; SEO fazı tamamlanmadan oluşabilecek yarım işlerin
   atomik yayın invariantı ayrıca doğrulanmalı.
6. **P2:** İddia düzeyi kaynak bağlantısı ve düzeltme şeffaflığı roadmap borcu.
7. **P2:** Sayfa türü × locale canonical/hreflang/schema kapsam matrisi merkezi release kapısı değil.
8. **P2:** GSC bağlı olmadığı için arama talebi ve içerik boşluğu sayısal olarak kanıtlanamıyor.
9. **P2:** Core Web Vitals release bütçesi roadmap'te bekliyor.
10. **P2:** Görsel servisinin dış provider seçimi ücret, lisans ve veri aktarımı nedeniyle owner kararı istiyor.

## Öncelikli ilk 20 geliştirme

| # | Faz | Öncelik | Etki / kabul özeti |
|---|---|---|---|
| 1 | Görsel sağlayıcı yetenek ve sağlık katmanı | P1 | Fail-closed API + admin durum kartları; bu çevrim |
| 2 | Locale-aware bölüm/gövde görseli art-direction | P1 | Tekil anchor, atomik figure, mobil crop, audit |
| 3 | Production generation/vision adaptörü | P1 / karar | Owner bütçe-lisans-veri kararı sonrası staging kalite örneklemi |
| 4 | Kaynaklı güvenli tarif deneyimi | P1 | Yapısal alanlar, güvenlik kapısı, görünür kaynak izi |
| 5 | Recipe structured data | P1 | Yalnız görünür gerçek alanlardan doğrulanan JSON-LD |
| 6 | Çeviri + SEO atomik yayın kapısı | P1 | Yarım otomasyon işi indexlenmez |
| 7 | Mevcut yayınların checkpoint'li görsel yenilemesi | P1 | Tüm yayınlar veya raporlu istisnalar |
| 8 | İddia düzeyi kaynak atıfları | P2 | Kritik iddia doğrudan editör onaylı kaynağa gider |
| 9 | Yapılandırılmış keşif kapsam matrisi | P2 | Dört locale × indexlenebilir sayfa türü sözleşme testi |
| 10 | Zaman/odak otorite merkezi | P2 | Pillar, kümeler ve çift yönlü iç bağlantılar |
| 11 | İçerik düzeltme ve şeffaflık akışı | P2 | Auditli düzeltme notu ve public zaman çizgisi |
| 12 | Core Web Vitals bütçesi | P2 | LCP/CLS/INP eşikleri release kapısında |
| 13 | Arama niyeti/GSC boşluk panosu | P2 / karar | Minimum OAuth yetkisi ve sayısal fırsat kanıtı |
| 14 | İçerik tazelik sinyallerinin birleştirilmesi | P2 | Kaynak, trafik ve bilgi değişimi tek kuyrukta |
| 15 | Üye haftalık okuma özeti | P3 | Takip, yarım okuma ve yeni kaynaklı yayınlar |
| 16 | Web Push tercih merkezi | P3 / karar | Açık opt-in, sessiz saat, rate limit ve çıkış |
| 17 | Canary kohortu ve tek eylem rollback | P2 | Web/API sürüm uyumu ve hata eşiği |
| 18 | Production backup restore kanıtı | P0 | İzole geri yükleme, checksum ve alarm |
| 19 | Rollback artefakt retention | P3 | Dry-run, aktif/sağlıklı release koruması |
| 20 | Roadmap encoding ve yinelenen faz hijyeni | P3 | Türkçe görünür metin ve benzersiz ürün fazları |

## Director kararı

Aktif `visual-service` korunuyor. EDITOR, tarif güvenliğini en yüksek içerik riski; FULLSTACK ise gövde
görselini en değerli tamamlanabilir dikey dilim olarak işaretledi. Owner'ın acil görsel servis kararı ve
roadmap aktif fazı nedeniyle bu çevrim önce sağlayıcı sağlık sözleşmesini görünür kılıyor. Dış provider
etkinleştirmiyor ve sağlam görselleri değiştirmiyor. Sonraki uygulama adayı, aynı kalite kanıtlarını
koruyan locale-local gövde görseli yerleşimidir.

## Rollback

Bu dilimde migration ve production veri dönüşümü yoktur. API provider health alanı ve admin kartları
commit geri alınarak kaldırılabilir. Dış sağlayıcı çağrısı, credential yazımı veya ücretli işlem yoktur.

## Uzman ve kalite kapısı sonucu

- **FULLSTACK — PASS:** Mevcut görsel omurgası doğrulandı; sonraki dikey dilim locale-local bölüm
  görseli olarak teslim edildi. Provider sağlık sözleşmesi secret değerlerini dışarı vermeden yalnız
  yetenek ve neden kodu döndürüyor.
- **DESIGNER — PASS (ilk REJECT sonrası):** 390 px makale/kategori taşması bulundu. Kategori şeridi
  ve makale baskı kartı viewport sözleşmesine alındı; kesintisiz kaynak URL'si `anywhere` kırılıyor.
  Son ölçümde ana sayfa, kategori ve makale `clientWidth=390 / scrollWidth=390`; 1440 açık/koyu ana
  sayfa `1440/1440`. Yetkili admin oturumu olmadığı için provider paneli gerçek veriyle screenshot
  yerine responsive kod/CSS çapraz kontrolünden geçti.
- **EDITOR — PASS:** Tarif güvenliği, Recipe schema, çeviri+SEO atomik kapısı ve iddia düzeyi atıf
  sonraki yüksek riskler olarak kaydedildi. Dış provider owner kararı olmadan etkinleştirilmedi.
- **SYSADMIN — PASS:** Migration, dış çağrı ve production veri değişimi yok. HTTPS endpoint,
  credential varlığı ve açık enable üçü birlikte olmadıkça dış sağlayıcı aday sağlayamaz. Değerlerin
  kendisi API veya loga yazılmıyor.
- **Director kararı — PASS:** `npm run lint`, `npm run typecheck`, `npm run check:locales`, 76 web
  testi, `npm run build:web`, 170 API testi ve Release .NET build sıfır hata ile geçti. İzole worktree
  teslim kuralı nedeniyle deploy/push yapılmadı; `visual-service` tamamlanmış sayılmayıp aktif kaldı.
