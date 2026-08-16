# BOECL ilk çalıştırma ana denetimi — Çevrim 20

Tarih: 16 Ağustos 2026  
Odak: admin paneli, editoryal verimlilik ve yönetilebilirlik

## Yönetici özeti ve görünür hedef

BOECL; Next.js 16.3/React 19 istemcisi, ASP.NET Core 10 API, EF Core/PostgreSQL, IIS/Windows Service ve `tr-TR`, `en-US`, `de-DE`, `fr-FR` yayınlarıyla çalışan çok dilli platformdur. Kimlik/roller, antiforgery, rate limit, audit izi, revizyon, workflow, taxonomy, medya, otomasyon, üyelik, canonical/hreflang, sitemap/RSS ve kontrollü deploy temelleri vardır.

Admin kontrol merkezi bugün içerik adetlerini ve sistem sağlığını gösteriyor; fakat editörün sıradaki eylemini belirlemiyor. İncelemedeki içerik, geciken görev ve eksik kalite kapıları yazı detaylarına dağılmış durumda. Bu çevrimin önce/sonra hedefi: kontrol merkezinin üstünde geciken işleri, 48 saat içinde bitecek görevleri, editoryal/SEO incelemelerini ve eksik kalite kontrollerini tek, responsive, doğrudan eyleme açılan bir **Editoryal Komuta Kuyruğu** olarak sunmak.

## Tam sistem analizi

- **Mimari:** `apps/web` locale-aware App Router public/admin UI; `apps/api` minimal API, domain, EF ve worker katmanı; `tests/api`; `ops/windows`; kalıcı `docs` kayıtları.
- **Veri:** article group/localization, locale, taxonomy, author/source, media/variant, revision, checklist, task/comment, homepage, engagement, saved article, automation ve append-only audit modeli var. Görevler için `(assignee,status,due)` indeksi mevcut; bu faz migration gerektirmiyor.
- **Auth/API:** Identity cookie ve Owner/Admin/Editor/Author/Translator/SEO görev ayrımı; antiforgery ile yazma uçları; loopback API ve admin proxy var. Yeni komuta uç noktası yalnızca `WriteContent` politikasına açık olmalı.
- **Admin/editoryal:** yazı editörü, workflow, revizyon, işbirliği, yayın kuyruğu, taxonomy, medya, homepage, trafik, kullanıcı/dil, Knowledge Vault ve otomasyon mevcut. Ana eksik, bu araçları günlük önceliğe bağlayan çapraz operasyon yüzeyi.
- **SEO/i18n:** public canonical/hreflang, structured data, sitemap, RSS ve draft noindex korunuyor. Admin indexlenmiyor. Yeni UI dört locale için gerçek arayüz metni sağlıyor ve makale locale bilgisini görünür tutuyor.
- **Performans:** komuta sorgusu salt okunur, `AsNoTracking`, sınırlı sonuç ve mevcut indeksleri kullanıyor. Dört küçük aggregate ve en fazla 16 satır döndürüyor; public render yoluna eklenmiyor.
- **Güvenlik:** sorgu kullanıcı girdisi almıyor, yazma yapmıyor ve yetki politikası deny-by-default davranıyor. HTML üretmiyor; XSS/CSRF yüzeyi eklemiyor.
- **Tasarım/UX:** token tabanlı açık/koyu admin teması ve responsive sidebar var. Mevcut dashboard pasif metrik ağırlıklı; mobilde de taranabilir eylem sırası eksik. Yeni yüzey 320–1440 px, klavye bağlantıları, semantik başlıklar, zaman etiketleri, boş durum ve yüksek risk vurgusu ister.
- **İçerik/görsel:** bu operasyon fazında yeni yayın veya görsel için kanıt yoktur. Dekoratif AI görsel editoryal karar hızını artırmayacağı için üretilmemelidir; mevcut konuya özgü kapaklar editör detayında korunur.
- **Operasyon:** atomik web/API deploy, staging/production health ve rollback betikleri var. Şema değişmediğinden veri yedeği/migration gerekmiyor; kalite kapıları ve iki ortam doğrulaması yine zorunlu.
- **Teknik borç/güncellik:** Next/React/.NET çekirdeği güncel 2026 ailesinde. Sıkıştırılmış eski component/C# kaynakları, CSP enforce, off-site restore tatbikatı, Web Vitals ayrıntısı ve eski içerik/görsel borcu sürüyor. Görünür fazla ilgisiz major paket yükseltmesi yapılmamalı.

## Öncelikli ilk 20 geliştirme

1. **P2 — Editoryal Komuta Kuyruğu:** geciken/atanmış görev, review ve kalite riskini tek eylem yüzeyinde birleştir. **Bu çevrim.**
2. **P2 — Kişisel “Bana atananlar” görünümü:** oturum sahibi görevleri ve bekleyen mention/yorumlar.
3. **P2 — Checklist’i workflow geçiş kapısına bağlama:** eksik zorunlu kontrollerde gerekçeli engel.
4. **P2 — Toplu ama güvenli queue eylemleri:** seçili görev atama/öncelik/tarih; antiforgery ve audit.
5. **P2 — Locale üretim dengesi:** çeviri eksikleri, yaş ve yayın ritmi göstergesi.
6. **P2 — Editoryal takvim:** zamanlanmış yayın, kapasite ve çakışma görünümü.
7. **P2 — İçerik sağlık puanı:** kaynak, kapak, taxonomy, SEO, tazelik ve ilişki sinyalleri.
8. **P2 — Kapaksız ve bozuk görselli arşiv iş kuyruğu:** trafik/değer sıralı, lisans auditli.
9. **P2 — Orphan içerik/iç link önerileri:** cluster bağlamı ve insan onayı.
10. **P2 — Arşiv gövde normalizasyon konsolu:** önizleme, transaction, rollback ve audit.
11. **P2 — Editoryal arama:** locale, yazar, kategori, görev, kalite ve tarih filtreleri.
12. **P2 — Revizyon karşılaştırma:** alan ve blok düzeyinde okunabilir diff.
13. **P2 — Düzeltme/geri çekme workflow’u:** public güven notu ve audit zinciri.
14. **P2 — Kaynak tazelik merkezi:** kırık URL, doğrulama tarihi ve güven düzeyi.
15. **P2 — Homepage slot karar desteği:** CTR/engagement ve çeşitlilik uyarıları.
16. **P2 — Editoryal SLA ölçümü:** review bekleme, görev gecikmesi ve yayın süresi trendi.
17. **P1 — CSP Report-Only → kontrollü enforce.**
18. **P1 — HSTS/frame-ancestors/Permissions-Policy canlı doğrulaması.**
19. **P1 — Şifreli off-site yedek ve düzenli restore tatbikatı.**
20. **P4 — Kontrollü dependency ve obsolete medya API güncellemesi.**

## Kabul kriterleri ve risk

- Yetkili editör dashboard açılışında risk sıralı kuyruğu görür; kart doğrudan doğru yazı alanına gider.
- Geciken, 48 saatlik, review ve eksik checklist sayıları sunucudan gelir; en fazla 16 eylem döner.
- Dört locale metni, açık/koyu tema, 320/375/390/768/1024/1440 px ve klavye kullanımı çalışır.
- API yetkisiz erişimi reddeder; veri mutasyonu ve yeni secret yoktur.
- Lint, locale, typecheck, web test/build, API test ve Release build geçmeden deploy edilmez.

Kalan risk: ilk sürüm kuyruk sıralaması kural tabanlıdır; gerçek SLA ve editör davranışı telemetrisi henüz yoktur. Sonraki ölçüm çevrimi sıralama ağırlıklarını gerçek gecikme/yayın süresiyle kalibre etmelidir.
