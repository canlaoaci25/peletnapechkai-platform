# BOECL Çevrim 42 — İçerik keşfi ve akıllı ev taxonomy merkezi

## Görünür önce / sonra hedefi

Önceden `/tr-TR/topics` bütün kategorileri resimli kartlarla gösteriyor, fakat okur konu
sayfasına girmeden hangi gerçek yayınlarla başlayacağını göremiyordu. Bütün kartlar aynı
ağırlıktaydı; arşiv derinliği editoryal hiyerarşiye dönüşmüyordu.

Bu fazdan sonra en derin locale arşivi büyük bir açılış alanında, üç güncel doğrudan yayın
yoluyla sunulur. Kalan her konu kartı iki gerçek yayını gösterir. Böylece konu merkezi bir
kart kataloğu değil, konu → yayın → derin arşiv yollarını birlikte sunan bir keşif masasıdır.
Mobilde hiyerarşi tek sütuna iner; açık ve koyu tema aynı token sistemini kullanır.

## Kanıt ve ürün kararı

- Production sitemap’teki 201 Türkçe yayın ölçüldü. Akıllı ev güvenliği, robot süpürge,
  akıllı televizyon, ev ağı/IPv6, NAS, ev interneti yedekleme, yerel yapay zekâ, akıllı sayaç
  ve bağlantılı cihaz başlıkları bağımsız, somut bir arşiv kümesi oluşturdu.
- Yeni **Akıllı Ev ve Bağlantılı Yaşam** kategorisi bu sınırlı sinyallerle article-group
  düzeyinde eşleştirilir; çeviri uydurulmaz, yalnız var olan locale yayınları bağlanır.
- Google Search Central, önemli sayfaların normal bağlantılarla kategori ve alt içerik
  yollarından erişilebilir olmasını; W3C WCAG 2.4.5 ise bir konu indeksini geçerli bir ikinci
  bulma yolu olarak tanımlar. Yüzey gerçek `<a href>` yolları ve semantik bölümler kullanır.
- Public archive index içindeki kategori başına sorgu kaldırıldı. Son üç yayın aynı EF Core
  projeksiyonunda alınır; görünür derinlik artarken sorgu sayısı kategori sayısıyla büyümez.

## Veri, SEO, güvenlik ve görsel bütünlük

- Migration dört locale kategorisini kaynak Türkçe taxonomy’ye bağlar, `ON CONFLICT DO
  NOTHING` ile tekrar çalıştırılabilir, append-only audit olayı üretir ve açık rollback sunar.
- Canonical, hreflang ve sitemap üretimi mevcut locale bağlantı modelini kullanır. Taslaklar
  ve locale dışı içerik public projeksiyona girmez.
- Yeni yapay veya dekoratif görsel üretilmedi. Yalnız kalite kapısından geçmiş mevcut yayın
  kapakları kullanılır; görsel bağlantısı yinelenen klavye durağı oluşturmaz.
- Admin taxonomy masası migration sonrası yeni kategoriyi gerçek yayın sayısıyla otomatik
  gösterir; ayrı ve tutarsız bir yönetim görünümü oluşturulmaz.

## Kabul kapıları

- 49 web regresyonu ve 5 locale aracı testi.
- Lint, typecheck, Next.js production build.
- 121 API testi ve .NET Release build.
- 320, 375, 390, 768, 1024 ve 1440 piksel Chromium render; açık/koyu tema, yatay taşma,
  doğrudan yayın bağlantıları ve yeni dört category URL.
- Staging ve production için migration öncesi yedek, atomik deploy, sağlık ve public deneyim
  kontrolleri; canlı URL doğrulanmadan Completed durumu verilmez.

## Sonraki yüksek değerli faz

İzinli kategori ve makale tıklama ölçümüyle konu sırası gerçek ilgi sinyaline bağlanmalı.
Arşiv yeterli olduğunda kategori modeline editoryal olarak yönetilen parent/child ilişkisi
eklenmeli; bu ilişki breadcrumb, topic directory, sitemap ve admin sürükle-bırak sırasını
tek kaynak üzerinden beslemelidir.
