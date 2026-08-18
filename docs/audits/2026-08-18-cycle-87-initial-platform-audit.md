# Çevrim 87 ilk platform denetimi ve görsel faz kararı

Tarih: 18 Ağustos 2026  
Odak: makale görsellerinin konu uygunluğu ve yazısız özgün tasarımı

## Ziyaretçi / yönetici öncesi-sonrası hedefi

Önce Görsel Yenileme Stüdyosu uzun bir makaleyi tek kapak ve ilk H2 bağlamına indiriyordu. Bu çevrimde yönetici, makalenin tamamından seçilen en fazla üç anlamlı H2/H3 için ayrı görsel türü, gerekçe ve yazısız somut sahne briefi görecek. Bu dilim görseli otomatik yayımlamaz; mevcut fail-closed aday ve editoryal terfi kapılarını korur.

## Mimari ve ürün envanteri

- **Web:** Next.js App Router, TypeScript, locale rotaları ve admin/public UI; `tr-TR`, `en-US`, `de-DE`, `fr-FR` desteklenir.
- **API:** ASP.NET Core minimal API, EF Core ve PostgreSQL; yayın, identity, taxonomy, medya, otomasyon, audit ve görsel review domainleri ayrıdır.
- **Veri:** Migration tabanlı şema; locale-local kapak, optimize medya ölçüleri, perceptual hash ve kalıcı görsel kalite kanıtı bulunur.
- **Auth/güvenlik:** Admin otomasyon uçları sunucu yetki politikası ve antiforgery ister; CMS HTML'i allow-list sanitizer'dan geçer; secret değerleri provider sağlık cevabına çıkmaz.
- **SEO/i18n:** Self-canonical, gerçek yayın karşılıklarına hreflang, Article/Breadcrumb verisi ve locale-aware sitemap vardır; sessiz içerik fallback'i yoktur.
- **Performans/a11y:** Next Image, responsive sizes, intrinsic ölçüler, tema tokenları, focus-visible ve mobil drawer sözleşmeleri vardır.
- **Operasyon:** İzole worktree, canlı durum, release health, backup/restore ve rollback scriptleri vardır. Bu çevrim production/IIS, secret, dış provider veya veriyi değiştirmez.
- **Görsel servis:** Brief, review kuyruğu, provider capability, lisans/alt metin, konu/bölüm/locale, teknik doğruluk, yazısızlık, artefact, crop ve özgünlük kapıları vardır. Gerçek generation worker/provider adaptörü owner kararı olmadan kapalıdır.

## Kritik bulgular

1. **P1:** Builder ilk üç H2/H3'ü bulsa da yalnız ilkini kullanıyordu; gövde görsel yönetmenliği görünür değildi.
2. **P1:** `VisualRenewal` için gerçek aday üreten worker yoktur; dış provider owner kararı gerektirir.
3. **P1:** `missing-body-visual` ölçülürken kalıcı görev/terfi modeli cover odaklıdır.
4. **P1:** Provider “health” canlı probe değil, fail-closed yapılandırma/capability durumudur.
5. **P1:** Stable heading anchor, locale-local body ilişkisi, provenance ve atomik rollback modeli eksiktir.
6. **P2:** Lisans URL/snapshot ve edinim zamanı modellenmelidir.
7. **P2:** Alt metnin locale dili ve doğruluğu server tarafında doğrulanmıyor.
8. **P2:** Taslak/review aşamasında görsel readiness kapısı yoktur.
9. **P2:** Core Web Vitals release bütçesi kuyruktadır.
10. **P3:** Roadmap encoding ve yinelenen maddeler temizlenmelidir.

## Öncelikli ilk 20 geliştirme

| # | Faz | Öncelik | Ölçülebilir kabul |
|---|---|---|---|
| 1 | Tam makaleden bölüm görsel yönetmenliği | P1 | En fazla 3 anlamlı H2/H3, ayrı tür/gerekçe/prompt; bu çevrim |
| 2 | Locale-local gövde görseli modeli | P1 | Stable anchor, asset, alt, hak ve audit ilişkisi |
| 3 | Atomik body figure terfi/rollback | P1 | Stale anchor fail-closed; önceki varlık döner |
| 4 | Provider-neutral candidate worker | P1/karar | Lease, idempotency, retry/dead-letter |
| 5 | Canlı provider probe telemetrisi | P1/karar | Timeout, son başarı, rate-limit, circuit state |
| 6 | Görsel provenance snapshot | P1 | Lisans URL/kapsam, kaynak kimliği, edinim zamanı |
| 7 | Yayın öncesi görsel readiness | P1 | Zorunlu kapı geçmeden indexlenebilir yayın yok |
| 8 | Checkpoint'li tüm yayın backfill'i | P1 | Tüm yayınlar veya raporlu istisnalar |
| 9 | Locale doğal alt metin kapısı | P2 | Dil, uzunluk, tekrar ve iddia kontrolü |
| 10 | Bölüm görseli responsive sunumu | P2 | 390/768/1440, light/dark, CLS, byte bütçesi |
| 11 | İddia düzeyi kaynak atıfları | P2 | Kritik iddia onaylı kaynağa bağlanır |
| 12 | Structured discovery matrisi | P2 | Dört locale × indexlenebilir sayfa |
| 13 | Core Web Vitals bütçesi | P2 | LCP/CLS/INP release eşikleri |
| 14 | Düzeltme/şeffaflık akışı | P2 | Auditli public correction timeline |
| 15 | Canary kohortu ve rollback | P2 | Uyumlu sınırlı trafik ve geri dönüş |
| 16 | Production restore kanıtı | P0 | İzole restore, checksum ve alarm |
| 17 | Zaman/odak otorite merkezi | P2 | Pillar ve çift yönlü cluster bağlantıları |
| 18 | Üye okuma özeti | P3 | Açıklanabilir geri dönüş özeti |
| 19 | Web Push tercih merkezi | P3/karar | Opt-in, sessiz saat ve çıkış |
| 20 | Roadmap veri hijyeni | P3 | UTF-8 ve benzersiz görünür fazlar |

## Director ve uzman kararı

- **FULLSTACK — NEEDS_INTEGRATION:** Gerçek worker/provider adapter eksikliğini kanıtladı; dış provider açılmadı.
- **DESIGNER — PASS:** En değerli güvenli dilim bölüm art-direction; gövde terfisi sonraki atomik veri dilimi olmalı.
- **EDITOR — NEEDS_INTEGRATION:** İlk H2 indirgemesini ve provenance/alt-text borcunu doğruladı.
- **SYSADMIN — HOLD:** Ajan kapasitesi dolu olduğundan Director fallback incelemesi yapıldı. Migration, dış ağ, credential, deployment ve production veri değişimi olmadığı doğrulandı.
- **Director — PASS (uygulama):** Plan ilk/orta/son anlamlı sahneyi seçer, maksimum üçle sınırlar, ince bölümleri dışlar ve mevcut text-free/locale/tür kurallarını her bölüm için uygular.

## Risk ve rollback

Migration, provider çağrısı veya içerik mutasyonu yoktur. API `sectionPlan`, admin bileşeni ve CSS commit geri alınarak kaldırılabilir. Plan henüz body görselini yayımlamadığı için `visual-service` tamamlanmış sayılmaz ve aktif kalır.
