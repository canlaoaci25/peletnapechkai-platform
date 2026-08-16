# BOECL ilk çalıştırma ana denetimi — Çevrim 19

Tarih: 16 Ağustos 2026  
Odak: üyelik, etkileşim ve ziyaretçiyi geri getiren ürün özellikleri

## Yönetici özeti ve görünür hedef

BOECL; Next.js 16.3/React 19, ASP.NET Core 10, EF Core/PostgreSQL 18, IIS/Windows Service ve `tr-TR`, `en-US`, `de-DE`, `fr-FR` locale’leriyle çalışan çok dilli yayın platformudur. Kimlik, roller, antiforgery, rate limit, audit, editoryal workflow, medya, taxonomy, canonical/hreflang, sitemap/RSS, sağlık ve rollback kapıları vardır. Çevrim 17 global navigasyonu, çevrim 18 konu merkezini tamamlamıştır.

Üyelik yüzeyi yalnızca profil ve parola ayarlarından oluşmaktadır. Anonim view/engaged ölçümü vardır fakat ziyaretçinin değer verdiği içeriğe dönmesini sağlayan kullanıcı ilişkisi yoktur. Bu çevrimin önce/sonra hedefi: dört dilde makale üstünde görünür ve klavyeyle çalışan **Kaydet** eylemi; hesapta kapaklı, responsive **Okuma listesi**; cihazlar arası kalıcılık; kullanıcı sahipliğini her istekte sunucuda doğrulayan ve ekleme/çıkarma işlemlerini audit eden API/veri modelidir. İçerik açık kalır; SEO’ya üyelik duvarı eklenmez.

## Tam sistem denetimi

- **Mimari/klasör:** `apps/web` public+admin App Router; `apps/api` minimal API/domain/EF/workers; `tests/api`; `ops/windows`; `docs`.
- **Veri/API:** article group/localization, locale, category/tag/author/source, media, revision, homepage, aggregate engagement, Identity ve audit ilişkileri olgun. Eksik olan üye→makale kalıcı ilişkisidir.
- **Auth/güvenlik:** Identity cookie, Member dahil görev rolleri, Strict SameSite antiforgery, lockout/rate limit ve append-only audit vardır. Yeni kayıtlar yalnızca oturumdaki `UserId` ile sorgulanmalı; istemciden kullanıcı kimliği alınmamalıdır.
- **Admin/içerik:** editoryal, medya, taxonomy, homepage, trafik, Knowledge Vault ve automation yönetimi vardır. Bu faz yeni içerik veya kategori gerektirmez.
- **SEO:** açık makaleler self-canonical, hreflang, Article/Breadcrumb structured data, sitemap ve RSS ile korunur. Hesap sayfaları robots tarafından dışlanır; kaydetme içeriğin index durumunu değiştirmez.
- **Performans:** kaydetme durumu küçük, kullanıcıya özel ve `no-store` istektir; makalenin server render/LCP yolunu bloke etmez. Okuma listesi optimize responsive kapakları kullanır.
- **UX/erişilebilirlik:** açık/koyu token sistemi vardır. Toggle için native button ve `aria-pressed`, kalıcı etiket, görünür focus ve 44 px hedef gerekir. Boş, bekleme, başarı ve hata durumları sunulmalıdır.
- **Görsel/içerik:** okuma listesi mevcut editoryal, yazısız ve konuya özgü kapakları kullanır; yeni jenerik AI görseli üretmek bilgi değeri katmaz.
- **Operasyon:** migration ileri/geri çalışır; production şeması öncesi custom-format yedek, staging uygulaması, sağlık kontrolü ve ardından kontrollü production gerekir.
- **Teknik borç:** CSP enforce, off-site restore tatbikatı, eski gövde/kapak borcu ve route/locale Web Vitals telemetrisi açıktır. Skia API obsolete uyarıları mevcut ancak bu görünür fazla karıştırılmamalıdır.
- **Teknoloji güncelliği:** çekirdek Next/React/.NET 2026 ailesindedir. Major dependency yükseltmeleri ayrı uyumluluk çevrimi gerektirir.

## Araştırma kararı

BBC’nin güncel yardım akışı, oturum açmış kullanıcının makale başlığının altından kaydetmesini ve hesabındaki Saved listesinden farklı cihazda geri dönmesini güçlü bir yayın deseni olarak doğrular. MDN, iki durumlu toggle için native `button` üzerinde `aria-pressed` kullanılmasını ve etiketin durumla değiştirilmemesini önerir. OWASP, her istekte sahiplik kontrolü, deny-by-default ve tahmin edilebilir kimliklere güvenmeme gereğini vurgular. BOECL uygulaması istemciden `UserId` kabul etmez; ilişkiyi oturum sahibinden kurar.

## Öncelikli ilk 20 geliştirme

1. **P2 — Makale kaydetme + hesap okuma listesi (bu çevrim).**
2. **P2 — Takip edilen konular ve kişisel keşif modülü.**
3. **P2 — Açık rızalı haftalık özet tercih merkezi ve doğrulanmış e-posta teslimi.**
4. **P2 — Okuma ilerlemesi ve “kaldığın yerden devam et”.**
5. **P2 — Kaydedilenler içinde arama/filtre/sıralama.**
6. **P2 — Hesap silme/veri dışa aktarma self-service akışı.**
7. **P2 — Üye dönüşü, save ve revisit funnel ölçümü; kişisel veri minimizasyonu.**
8. **P2 — Homepage’de oturum sahibine son kaydedilenler kısayolu.**
9. **P2 — Kategorisiz yayınların editoryal sınıflandırılması.**
10. **P2 — Topic cluster ve orphan iç link grafiği.**
11. **P2 — Arama kategori/tür/tarih filtreleri.**
12. **P2 — Trafik öncelikli eksik kapak programı.**
13. **P2 — Eski gövdelerin yedekli normalizasyonu.**
14. **P2 — Route/locale/viewport Web Vitals telemetrisi.**
15. **P1 — CSP Report-Only envanteri ve kontrollü enforce.**
16. **P1 — HSTS, frame-ancestors ve Permissions-Policy canlı doğrulaması.**
17. **P1 — Şifreli off-site yedek ve restore tatbikatı.**
18. **P2 — Editoryal güven merkezi ve düzeltme politikası.**
19. **P3 — Automation p95/hata/kapasite paneli.**
20. **P4 — Kontrollü paket ve obsolete API güncellemeleri.**

## Kabul kriterleri

- Kaydetme/listeleme/çıkarma yalnızca authenticated kullanıcıya açık ve CSRF korumalıdır.
- Aynı kullanıcı aynı makaleyi yalnızca bir kez kaydedebilir; başka kullanıcının kaydını göremez/değiştiremez.
- Yalnızca yayımlanmış, istenen locale’deki makale kaydedilir ve listelenir.
- Makale eylemi ve hesap listesi dört locale’de, açık/koyu temada ve 320–1440 px’de kullanılabilir.
- Migration yedek sonrası uygulanabilir, rollback’i vardır; audit izi tutulur.
- Locale, lint, typecheck, web test/build, API test ve Release build kapıları geçmeden deploy edilmez.
