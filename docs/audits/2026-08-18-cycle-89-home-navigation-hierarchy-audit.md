# Çevrim 89 — ana sayfa ve yayın navigasyonu hiyerarşisi

## Görünür hedef

Ziyaretçi masaüstü sidebar ve mobil drawer içinde düz bir kategori yığını yerine üst konuları ve bunlara bağlı alt konuları görür. Ana sayfadaki Konu Atlası en çok yayını olan rastgele alt konular yerine gerçek pillar/root kategorilerden kurulur. Daraltılmış masaüstü rayı ekran okuyucu adlarını korur; küçük vurgu metinleri açık ve koyu temada okunabilir kalır.

## Kanıt ve karar

- Public archive API locale'e göre filtrelenmiş gerçek taxonomy verisinde `parent` ve `children` bağlarını zaten sağlıyordu; `SiteHeader` bu alanları atıyor ve navigasyon düz liste üretiyordu.
- Ana sayfa `archives.categories.slice(0, 6)` kullandığı için parent ve child aynı atlas içinde yarışıyordu.
- Daraltılmış CSS görünür kategori metnini kaldırınca bağlantıda yalnız CSS pseudo-dot kalıyordu.
- Açık tema accent rengi küçük metinde yaklaşık 3.94:1 kontrast veriyordu; ayrı `--accent-text` tokenı daha koyu bir değerle tanımlandı.

Director kararı: API veya migration eklemeden mevcut doğrulanmış ilişkileri frontend'de korumak; ana sayfanın aynı archive yanıtını `SiteHeader` ile paylaşmak; root kategori yoksa eski düz listeyi güvenli fallback olarak kullanmak.

## Kabul kriterleri

- Sidebar parent/child yapısını gerçek archive payload'ından üretir ve yapay kategori oluşturmaz.
- Ana sayfa atlası root kategorileri tercih eder; root yoksa yayın keşfi kaybolmaz.
- Collapsed ray ana ve kategori bağlantılarında lokalize erişilebilir ad taşır.
- Mobil drawer varsayılan kapalı, Escape/focus trap/focus return/body lock davranışlarını korur.
- Dört locale sözlük kapısı, lint, typecheck, web testleri ve production build geçer.
- 320/390/768/1024/1440 light/dark gerçek render ile taşma ve tema kontrolü yapılır.

## Rollback

Frontend commit'i geri alınır. Şema, veri veya harici servis değişikliği olmadığı için veri rollback'i yoktur.

## Doğrulama sonucu

- `npm run test:web`: 79/79 PASS.
- `npm run lint`, `npm run typecheck`, `npm run check:locales`: PASS; dört locale tutarlı.
- `npm run build:web`: Next.js production build PASS, 109 statik sayfa üretildi.
- `dotnet test Peletnapechkai.slnx`: 175/175 PASS.
- `dotnet build Peletnapechkai.slnx --configuration Release`: 0 uyarı, 0 hata.
- Headless Chromium: 320, 390, 768, 1024 ve 1440 açık tema; 390 ve 1440 koyu tema screenshot PASS. Yatay taşma veya okunmayan yüzey gözlenmedi. Yerel API fallback'i boş içerik döndürdüğü için taxonomy metinleri bu screenshot setinde görünmedi; hiyerarşi payload sözleşmesi, kaynak regresyonu, typecheck ve production build ile doğrulandı.

Director kalite kapısı: kod ve yerel ürün kanıtı **PASS**. İzole worktree teslim kuralı nedeniyle staging/production deploy yapılmadı; canlı doğrulama runner handoff'udur.
