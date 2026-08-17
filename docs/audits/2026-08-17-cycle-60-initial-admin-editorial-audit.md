# Çevrim 60 — ilk tam denetim ve admin/editoryal faz seçimi

Tarih: 2026-08-17. Kapsam: depo, son 20 yerel commit, kalıcı yol haritası, web/API/veri modeli, kimlik, SEO, operasyon ve test envanteri. Bu belge kullanıcı master talimatındaki ilk çalıştırma denetimidir.

## Mimari ve mevcut durum

- Frontend: Next.js App Router, React 19 ve TypeScript; locale köklü public/admin rotaları, sunucu bileşenleri ve BFF proxy. Dört locale `tr-TR`, `en-US`, `de-DE`, `fr-FR` merkezi katalogla yönetiliyor.
- Backend: ASP.NET Core `net10.0`, EF Core/PostgreSQL, minimal API endpoint modülleri, worker servisleri ve katmanlı domain/persistence düzeni.
- Veri: locale bazlı makaleler; taxonomy, kaynak, medya, SEO, checklist, revizyon, görev, audit, üyelik ve otomasyon ilişkileri migration ile izleniyor.
- Kimlik/güvenlik: ASP.NET Identity, rol/policy tabanlı yetki, antiforgery, HTML temizleme ve upload doğrulaması var. Secret değerleri depo dışında.
- Admin: birleşik sidebar, içerik editörü, taxonomy, yerelleştirme, homepage, otomasyon, görsel kalite, trafik ve geliştirme yüzeyleri var. Komuta merkezi görevleri ve kapasiteyi gösteriyor.
- SEO/public: locale-aware URL/canonical/hreflang, robots, yapılandırılmış veri, sitemap ve yayın kalite kapıları var. Public sidebar ve dark tema önceki fazda tamamlanmış.
- Operasyon: IIS staging/production deployment, health/smoke, deployment journal, PostgreSQL backup/restore ve canlı durum kaydı var.
- Test: Node testleri, lint/typecheck/build; xUnit ve PostgreSQL entegrasyon testleri; PowerShell operasyon testleri.

## Kanıtlanan açık

En yüksek getirili açık, admin ana sayfasındaki kalite borcunun yalnız toplam sayı olmasıdır. Checklist verisi hangi içeriğin hangi yayın kapısında takıldığını içerdiği halde komuta kuyruğuna taşınmıyor. Yol haritasındaki aktif push fazı bu çevrimin açık admin/editoryal odağıyla uyuşmuyor. Remote işlemleri 2026-08-20 17:45:33 UTC'ye kadar kullanıcı kararıyla yasaklıdır.

## Öncelikli ilk 20 geliştirme

1. P1 — Kalite checklist borcunu admin iş kuyruğunda kapı bazında yönetilebilir yap.
2. P1 — Görsel servisinin sağlayıcı sağlığı, retry ve kalite raporunu gerçek sayılarla tamamla.
3. P1 — Görsel backfill işini checkpoint/rollback ile yayımlanmış arşive uygula.
4. P1 — Editoryal görevlerde sahipsiz iş atama ve SLA eskalasyonunu tamamla.
5. P1 — Admin kritik eylemleri için audit geçmişi ve geri alma görünümü ekle.
6. P2 — Homepage keşif bloklarını gerçek performans ve editör seçkisiyle yönet.
7. P2 — Arama yazım toleransı, boş sonuç kurtarma ve konu önerilerini geliştir.
8. P2 — Locale çeviri kapsamı ve kaynak revizyon sapmasını yayın kapısına bağla.
9. P2 — Canonical/hreflang/taxonomy tutarsızlıklarını admin SEO borç kuyruğuna taşı.
10. P2 — İç bağlantı önerilerini konu kümesi ve kaynak otoritesiyle puanla.
11. P2 — Admin mobil editör deneyimini gerçek 390/768 render ile iyileştir.
12. P2 — Core Web Vitals bütçelerini release kalite kapısına bağla.
13. P2 — Üyelik kişisel akış ve bildirim tercihlerini tamamla.
14. P2 — Web Push için konu tercihleri, sessiz saatler ve abonelik iptali ekle.
15. P2 — Arşiv/trend kanıtıyla Türkçe taxonomy ve evergreen boşluklarını doldur.
16. P2 — Kaynak güveni ve güncelliği editör öncelik puanına kat.
17. P3 — Admin sorgularını ölç, pagination ve indeksleri doğrula.
18. P3 — API rate-limit, IDOR ve güvenlik regresyon matrisini genişlet.
19. P3 — Staging admin görsel regresyon/erişilebilirlik otomasyonunu kalıcılaştır.
20. P3 — Backup/restore ve deployment rollback kanıtını tek raporda birleştir.

## Bu çevrimin önce/sonra hedefi

Önce: editör yalnız kalite borcu toplamını görür. Sonra: gerçek checklist borçları ayrı filtreyle görünür; eksik kaynak, SEO, taxonomy, kapak/alt metin, çeviri ve hukuk kontrolleri kart üzerinde okunur ve içerik doğrudan açılır.
