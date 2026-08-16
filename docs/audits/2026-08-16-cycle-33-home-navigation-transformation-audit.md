# BOECL Çevrim 33 — Ana sayfa ve global navigasyon dönüşümü

Tarih: 16 Ağustos 2026  
Odak: ana sayfa, global navigasyon ve görünür tasarım dönüşümü

## Görünür önce / sonra hedefi

Önceden BOECL ana sayfası güçlü bir manşetle açılıyor ancak arşiv derinliğini aşağıdaki
bloklarda içerik türü aramalarına indirgiyordu. Ziyaretçi gerçek kategori mimarisini,
hangi konuda ne kadar yayın bulunduğunu ve o konunun öne çıkan dosyasını aynı yüzeyde
göremiyordu. Global menü de kategori adlarını listeliyor fakat kapsam sinyali vermiyordu.

Artık ana sayfa gerçek taxonomy verisiyle çalışan bir **BOECL konu atlası** sunar. İlk altı
kategori; yayın sayısı, yerelleştirilmiş açıklama, konuya özel gerçek yayın kapağı, öne
çıkan hikâye ve doğrudan kategori yolu ile tek editoryal kompozisyonda görünür. Masaüstünde
12 kolonlu yayın masası, mobilde yatay kaydırılabilir ve snap davranışlı konu kartları
kullanılır. Global menü kategori yoğunluklarını gösterir; ana sayfa bağlantısı hem menüde
hem birincil navigasyonda semantik ve görsel mevcut-konum işareti taşır.

## Kanıt ve kararlar

- Canlı production ana sayfası, staging, admin yüzeyi, son 20 commit, kalıcı yol haritası
  ve Çevrim 25 ana sayfa denetimi incelendi. Çevrim 25'in manşet/header işini tekrarlamak
  yerine keşif mimarisindeki taxonomy kopukluğu seçildi.
- [W3C WAI menü rehberi](https://www.w3.org/WAI/tutorials/menus/) mevcut konumun açık
  işaretlenmesini, anlamlı grup etiketlerini, klavye sırasını ve bir içeriği bulmak için
  birden fazla yol sunmayı önerir. Yerel `details` temeli ve link sırası korunarak
  `aria-current` ile kapsam göstergeleri eklendi.
- [W3C tutarlı navigasyon açıklaması](https://www.w3.org/WAI/WCAG21/Understanding/consistent-navigation.html)
  tekrarlanan navigasyonun sırasının öngörülebilir kalmasını ister; mobil ve masaüstü aynı
  bilgi sırasını korur.
- [BBC'nin güncel içerik yüzeyi açıklaması](https://help.bbc.com/hc/en-us/articles/39027623773331-What-types-of-news-content-will-be-available)
  büyük gündem, editör seçkisi ve kalıcı konu bölümlerini farklı keşif yolları olarak
  birlikte sunar. BOECL bu prensibi kendi taxonomy verisi ve özgün görsel diliyle uygular.
- [web.dev responsive images rehberi](https://web.dev/articles/responsive-images) görsel
  indirme adaylarının gerçek render genişliklerine göre tanımlanmasını önerir; atlasın ana
  ve ikincil kapakları için ayrı `sizes` sözleşmeleri eklendi.

## Kapsam ve güvenlik

- Dört etkin locale'e eksiksiz ve doğal arayüz metni eklendi; locale anahtar eşitliği
  otomatik testle doğrulandı.
- Yeni içerik, translation veya veritabanı kaydı üretilmedi. Yayında ve editoryal onaylı
  gerçek kategori/kapak verisi kullanıldı; veri yedeği veya migration gerekmedi.
- Görsel linkler yinelenen klavye durağı yaratmaz; yazısız kapaklar Next.js optimizasyon
  hattı ve belirlenmiş boyut adayları üzerinden sunulur. Birden çok kategoriye bağlı aynı
  makale atlas içinde ikinci kez seçilmez; her kategori sıradaki benzersiz öne çıkan yayını
  kullanır veya sağlıklı biçimde görselsiz taxonomy kartına düşer.
- Canonical, hreflang, JSON-LD, locale routing, noindex politikaları ve admin yetkileri
  değişmedi.

## Kabul kapıları

- Locale eşitliği, web regresyonları, lint, typecheck ve production build.
- API testleri ve Release build.
- 320, 375, 390, 768, 1024 ve 1440 px Chromium render; light/dark tema, yatay taşma,
  başlık kırılımı, konu atlası ve dokunma hedefleri.
- Staging ve production atomik deploy, sağlık/public-experience kapıları ve canlı dört
  locale HTML sözleşmesi doğrulanmadan çevrim tamamlanmış sayılmaz.

## Sonraki yüksek değerli faz

Konu atlasındaki gerçek trafik ve tıklama dağılımı ölçülerek kategori sırası editoryal
kürasyonla birleştirilmeli; ardından manşet/atlas için mobil art-direction crop desteği ve
görsel kalite servisinin önerilen varlık önizlemesi tamamlanmalıdır.
