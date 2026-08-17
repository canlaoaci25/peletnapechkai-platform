# BOECL Çevrim 34 — İçerik keşfi ve taxonomy bütünlüğü

Tarih: 16 Ağustos 2026  
Odak: içerik keşfi, kategori mimarisi ve yeni Türkçe taxonomy

## Görünür önce / sonra hedefi

Önceden 24 yayından derin kategori arşivleri ilk sayfada kesiliyor, okur kalan yayınlara
ulaşamıyordu. Kategori yönetimi yalnız ad ve slug düzenliyor; yayın kapsamını veya
kategorisiz içerik borcunu göstermiyordu. Arşivde gizlilik, takip, şifreleme, dijital
kimlik ve veri hakları için güçlü bir içerik kümesi bulunmasına rağmen bunlar geniş
Siber Güvenlik ve Dijital Yaşam başlıklarında dağılıyordu.

Bu fazdan sonra kategori arşivleri kararlı ve locale-aware önceki/sonraki sayfalar,
self-canonical URL ve gerçek toplam sayfa bilgisi sunar. Yönetici kategori ekranı canlı
Türkçe yayın kapsamını, kategori hacimlerini ve doğrudan düzenlenebilir kategorisiz yayın
kuyruğunu gösterir. Yeni **Gizlilik ve Dijital Haklar** merkezi; doğal açıklama ve slug ile
`tr-TR`, `en-US`, `de-DE`, `fr-FR` arasında bağlıdır.

## Kanıt ve karar

- Production sitemap içindeki 201 Türkçe makale ve görünür kategori atlası ölçüldü.
  Gizlilik/dijital haklar adayı 41 başlık sinyali üretirken bilim ve sürdürülebilir
  teknoloji adayları henüz bağımsız otorite merkezi için yeterli arşiv derinliği vermedi.
- Sınıflandırma slug içindeki açık gizlilik, parola/şifreleme, izin, takip, dijital kimlik
  ve veri koruma sinyalleriyle sınırlandı. Aynı article group içindeki gerçek çeviriler
  locale'e ait eş kategoriye bağlanır; yayın içeriği veya çeviri üretilmez.
- Arşiv sorgusu tarih eşitliğinde kimlik ile kararlı sıralanır. Geçersiz yüksek sayfa 404
  döndürür; birinci sayfa temiz canonical kullanır, sonraki sayfalar kendine referans verir.
- Admin ölçümü yetkili editoryal uçta kalır; public API editoryal borç veya taslak sızdırmaz.

## Veri ve geri alma güvenliği

Migration tekrar çalıştırılabilir `ON CONFLICT DO NOTHING` ilişkileri, dört locale bağı ve
append-only audit kayıtları içerir. `Down` önce çeviri kategorilerini, sonra kaynak
kategoriyi kaldırır; ilişkiler foreign-key cascade ile temizlenir. Staging ve production
migration öncesinde PostgreSQL custom-format yedeği ve SHA-256 checksum zorunludur.

## Kabul kapıları

- Web regresyonları, locale eşitliği, lint, typecheck ve production Next.js build.
- 111 API testi ve .NET Release build.
- Staging migration/deploy sonrası yeni dört archive URL, sayfa 2 canonical/prev/next,
  admin render ve 320/375/390/768/1024/1440 px light/dark gerçek Chromium kontrolü.
- Production yedek, migration, API/Web atomik deploy, dört locale sağlık, sitemap ve canlı
  URL doğrulaması geçmeden Completed sayılmaz.

## Sonraki yüksek değerli faz

Search Console ve yerel arama verisiyle kategori/içerik türü/tarih filtreleri eklenmeli;
taxonomy parite borcu owner/SLA ile yönetilmeli ve konu atlası sıralaması gerçek CTR ile
editoryal kürasyonu birleştirmelidir.
