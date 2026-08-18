# BOECL Çevrim 74 — Zaman, odak ve planlama keşif yolu

## Görünür önce / sonra hedefi

Production Türkçe arşivinde zaman planlama, takvim, görev akışı ve dikkat yönetimi üzerine
çok sayıda yayın geniş **Verimlilik** alanında kayboluyordu. Faz sonunda `/tr-TR/topics`,
Verimlilik altında yeni **Zaman, Odak ve Planlama** yolunu gerçek yayın sayısı ve mevcut,
kalite kapısından geçmiş yayın kapaklarıyla gösterecek. Dört locale'deki gerçek yayın
karşılıkları kendi yerelleştirilmiş kategori arşivlerine bağlanacak.

Admin taxonomy masasında görünen ana alan seçimi daha önce güncelleme isteğine eklenmiyordu;
editör seçim kaydedildi sanabiliyordu. Oluşturma ve güncelleme artık aynı parent sözleşmesini
kullanır. Public archive ana içeriği de global skip-link hedefini yeniden sağlar.

## Kanıt ve karar

- 18 Ağustos 2026 production sitemap'i 217 benzersiz Türkçe yayın içeriyor.
- Oyun, bulut, geliştirici, fintech, ses ve giyilebilir adayları 0–3 açık slug sinyalinde
  kaldığı için yeni kategori yapılmadı.
- Akiflow, Amazing Marvin, Clockify, Cold Turkey, Endel, Fantastical, FlowSavvy, Focusmate,
  Forest, Freedom, Llama Life, Morgen, Motion, Notion Calendar, Reclaim, RescueTime, Rize,
  Routine, Sunsama, Things, TickTick, Todoist, Toggl Track ve Vimcal dahil 26 açık yayın
  sinyali aynı planlama/odak niyetini oluşturuyor. Migration yalnız bu açık slug kümesinin
  `article_group_id` değerlerini kullanır; metin benzerliğinden geniş ve belirsiz eşleme yapmaz.
- Etiket atlası ayrıca değerli bir sonraki keşif adayıdır; yeni kategori yerine mevcut bağlı
  tag arşivlerini görünür kılmak için roadmap'te araştırma girdisi olarak korunur.

## Veri, SEO, güvenlik ve geri alma

- Veri migration'ı dört deterministic kategori kimliği, locale'e ait Verimlilik parent'ı,
  Türkçe source ilişkisi, `ON CONFLICT DO NOTHING`, çakışmada fail-closed doğrulama ve
  append-only audit olayı içerir.
- Yalnız mevcut gerçek localization'lar yeni kategoriye bağlanır. Yeni çeviri, yayın veya
  görsel üretilmez; canonical/hreflang yalnız yayımlanmış gerçek arşiv sözleşmesinden gelir.
- `Down` yalnız dört yeni kategori kimliğini kaldırır. Production geri dönüşünde yazı almış
  taxonomy üzerinde körlemesine Down çalıştırmak yerine doğrulanmış yedek veya düzeltici ileri
  migration tercih edilir.
- Otonom migration terfisi artık staging ve production hedeflerini ayrı ayrı IIS ayarından
  çözer, her ortam için ayrı custom-format yedek alır ve üretilen tam yedek dosyasını migration
  öncesi izole restore testine verir. Secret değerleri loglanmaz veya commit edilmez.

## Kabul kapıları

- Locale, migration, admin parent ve skip-link regresyonları; PowerShell parser ve backup
  promotion regresyonu.
- Lint, typecheck, Next.js production build, .NET test ve Release build.
- Staging/production öncesi ortam bazlı yedek + restore; migration ve atomik API/Web deploy.
- Dört locale category URL, parent breadcrumb, topic haritası, admin parent yönetimi, health
  ve public experience kontrolü.
- 390 ve 1440 px açık/koyu gerçek browser render; topic child yolu, kategori arşivi, admin
  taxonomy, taşma, kontrast ve klavye odağı.

## Sonraki yüksek değerli faz

`search-recommendation` active yapıldı. Türkçe normalizasyon, açıklanabilir boş sonuç kurtarması
ve ölçülebilir relevancy corpus'u tamamlanmalı. Etiket Atlası, mevcut Alışveriş tag kümesiyle
küçük ve geri alınabilir bir keşif deneyi olarak bu fazın aday girdisidir.

## Dağıtım notu

Staging custom-format yedeği izole restore edildiğinde yalnız 3 locale bulundu. Desteklenen
locale sözleşmesi 4 olduğu için migration ve deploy fail-closed durduruldu; production'a
geçilmedi. Restore kapısı artık locale sayısını tam olarak 4 ister. Staging veri bütünlüğü
onarılıp yeni yedek aynı kapıdan geçmeden faz canlıda tamamlanmış sayılmaz.
