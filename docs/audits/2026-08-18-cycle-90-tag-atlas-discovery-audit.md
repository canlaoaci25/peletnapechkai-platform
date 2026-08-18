# BOECL Çevrim 90 — Etiket Atlası ve konu seçimi bütünlüğü

## Görünür önce / sonra hedefi

Konu merkezi daha önce yalnız kategori hiyerarşisini gösteriyor, doğrulanmış çok dilli etiket
arşivleri ise sitemap ve doğrudan URL dışında keşfedilemiyordu. Ayrıca öne çıkan konu, API'nin ilk
kaydından root filtresinden önce seçildiği için yayın sayısı yüksek bir alt konu ana alan gibi
sunulabiliyordu.

Bu fazda `/tr-TR/topics` ve gerçek karşılıkları, en çok yayımlanmış içeriğe sahip ilk 12 etiketi
yayın sayısıyla gösteren bir **Etiket Atlası** kazanır. Her kart doğrudan locale-local etiket arşivine
gider. Öne çıkan konu yalnız gerçek ana kategori kümesinden seçilir. Yeni kategori, etiket, içerik,
çeviri veya görsel üretilmez.

## Kanıt ve ürün kararı

- Çevrim 74 production envanteri yeni kategori adaylarının çoğunu 0–3 açık sinyal nedeniyle elemiş,
  bağlı tag verisini kullanan Etiket Atlası'nı sonraki güvenli keşif adayı olarak kaydetmiştir.
- Çevrim 46 yedi gerçek etiketin dört locale arasında `source_tag_id` ile bağlandığını; yalnız
  yayımlanmış karşılıkların public API, hreflang ve sitemap'te açıldığını kanıtlamıştır.
- Editör denetimi `alisveris` etiketindeki dört mevcut içerik sinyalinin kategori açmak için ince,
  etiket tabanlı keşif pilotu için uygun olduğunu doğrulamıştır.
- API etiketleri artık alfabetik düz liste yerine yayımlanmış ilişki sayısına göre sıralar ve bu
  sayıyı response'a taşır. Sıfır yayınlı veya taslak-only etiket public atlasına giremez.

## SEO, erişilebilirlik ve güvenlik

- Etiket kartları server-rendered, crawl edilebilir locale URL'leridir; yeni ince URL üretilmez.
- Var olan tag archive self-canonical, gerçek sibling hreflang ve sitemap sözleşmesi korunur.
- Atlas semantik `section`/`h2`, açıklayıcı erişilebilir ad, görünür yayın sayısı, focus-visible ve
  semantik tema tokenları kullanır.
- Grid 3 → 2 → 1 kolon kırılır; 320 px'te yatay taşma yaratmaz. Hareket yalnız renk geçişidir.
- Veri migration'ı, secret, dış servis, production yazımı veya deploy yoktur; rollback bu commit'in
  geri alınmasıdır.

## Uzman kalite kapısı

- DESIGNER: root lead kusurunu ve gerçek browser görsel test açığını doğruladı; ürün yönü PASS.
- EDITOR: yeni kök kategori için kanıt olmadığını, etiket atlasının küçük ve geri alınabilir doğru
  deney olduğunu doğruladı; PASS.
- SYSADMIN: migration/deploy hattında production öncesi staging-smoke açığı nedeniyle migration'lı
  teslimi REJECT etti. Director bu fazı migration'sız tuttu ve çevrim talimatına göre deploy yapmadı.
- FULLSTACK: public archive sorgusu, tip sözleşmesi, SSR görünüm ve regresyon testleri birlikte
  güncellendi.

## Kabul kriterleri

- En fazla 12 etiket yayımlanmış makale sayısına göre kararlı sıralanır.
- Her kart aktif locale'in `/tags/{slug}` arşivine gider; sıfır yayınlı etiket gösterilmez.
- Öne çıkan kategori bir child olamaz.
- Dört locale arayüz metni doğal ve eksiksizdir.
- Web test, locale kontrolü, lint, typecheck, production build, API test ve Release build geçer.
- 390 ve 1440 px açık/koyu render'da taşma, kontrast ve okunabilirlik doğrulanır.

