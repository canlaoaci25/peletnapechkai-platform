# BOECL Çevrim 50 — İçerik keşfi, kategori mimarisi ve mobilite taxonomy

## Görünür önce / sonra hedefi

Önceden `/tr-TR/topics` on kategoriyi arşiv büyüklüğüne göre aynı seviyede sıralıyordu.
Okur Donanım, Mobil Teknoloji ve Akıllı Ev arasındaki ilişkiyi ya da Yazılım ile Yapay
Zekâ arasındaki keşif yolunu göremiyordu. Admin de yalnız düz kategori adlarını yönetiyordu.

Bu fazdan sonra konu merkezi ana alanları ve onların alt uzmanlıklarını tek görsel haritada
gösterir. Her alt konu gerçek arşiv sayısıyla doğrudan bağlantıdır. Admin, Türkçe kategoriyi
güvenli biçimde bir ana alana bağlayabilir; API aynı locale, tek seviye ve silme bütünlüğünü
zorunlu kılar. Yeni **Otomobil Teknolojileri ve Mobilite** konusu Donanım altında açılır.

## Kanıt ve karar

- Production Türkçe envanterindeki 201 yayın tarandı. OBD-II, araç kazası dijital kanıtı,
  Android Automotive, eCall, dijital otomobil anahtarı, kiralık otomobil verisi ve akıllı EV
  şarjı bağımsız, somut bir mobilite kümesi oluşturuyor.
- Mevcut kategoriler üç doğal teknoloji yolu altında bağlandı: Dijital Yaşam → Verimlilik ve
  Gizlilik; Yazılım ve Uygulamalar → Yapay Zekâ ve Siber Güvenlik; Donanım → Mobil Teknoloji,
  Akıllı Ev ve yeni Mobilite. Anime bağımsız yayın alanı olarak korunur.
- Google Search Central, mantıklı site yapısı ile önemli sayfalara ilgili sayfalardan kısa ve
  açıklayıcı bağlantılar verilmesini önerir. W3C WAI, site haritasının yapıyı anlatmasını ve
  karmaşık hiyerarşilerde üst seviyelerin açıkça sunulmasını önerir.

## Veri, SEO, güvenlik ve operasyon

- `parent_category_id` aynı tabloya `Restrict` ilişkisi ve parent/name indeksiyle eklenir.
  Domain kuralı aynı locale'i ve self-reference engelini uygular; admin yalnız üst düzey bir
  parent seçebilir ve child sahibi kategori silinemez.
- Migration dört locale taxonomy kaydını var olan çeviri ilişkisine bağlar, yalnız mevcut
  article-group çevirilerini sınıflandırır, `ON CONFLICT DO NOTHING` kullanır ve append-only
  audit olayı üretir. Down yolu önce ilişkileri çözer ve yalnız yeni taxonomy kayıtlarını siler.
- Canonical, hreflang, sitemap ve yayın izolasyonu mevcut archive sözleşmesinde kalır. Yeni
  görsel üretilmez; konu merkezi kalite kapısından geçmiş mevcut yayın kapaklarını kullanır.

## Kabul kapıları

- Locale eşitliği, lint, typecheck, web regresyonları ve Next.js production build.
- API model/regresyon testleri ve .NET Release build.
- Migration öncesi staging/production yedeği, atomik deploy, health ve public experience.
- 320, 375, 390, 768, 1024 ve 1440 px gerçek render; açık/koyu tema, taşma, parent/child
  bağlantıları ve dört yeni locale archive URL doğrulaması.

## Sonraki yüksek değerli faz

İzinli kategori tıklama ölçümü ile ana alan ve alt konu sıralaması kanıta bağlanmalı. Parent
ilişkisi kategori archive breadcrumb ve Article/BreadcrumbList structured data katmanına da
taşınmalı; iki seviyeden daha derin yapı ancak gerçek arşiv yoğunluğu kanıtlarsa açılmalıdır.
