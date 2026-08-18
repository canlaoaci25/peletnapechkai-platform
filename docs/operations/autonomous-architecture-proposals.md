# BOECL otonom mimari ve ürün teklifleri

> Çevrim 92 güncellemesi (18 Ağustos 2026): Admin komuta merkezine editoryal üretim ritmi eklendi.
> Ölçüm `UpdatedAt` gibi yeniden atamayla değişen bir alanı kullanmaz; nullable `CompletedAt` kanıtı,
> son 30/90 gün örneklemi, zamanında oran, p50/p95 çevrim süresi ve 13 haftalık tamamlanma eğrisi
> sunar. Geçmiş tamamlanmış görevler tahmini backfill yerine görünür biçimde ölçüm dışında kalır.
> Migration defaultsuz ve geri alınabilir, sorgu partial index ile sınırlıdır. Runner staging backup,
> restore, migration ve gerçek admin render kanıtı sağlamadan canlı tamamlanmış sayılmayacaktır.

> Çevrim 91 güncellemesi (18 Ağustos 2026): Üye okuma merkezi, yarım okumaları, takip edilen
> konulardaki güncel yayınları ve kaydedilenleri aynı locale içinde açıklanabilir haftalık dönüş
> rotasında birleştirdi. Tamamlanan yayınlar dışlanır ve son bölüm anchor'ı korunur. Bu site içi
> özet açık rıza gerektiren e-posta/push teslimini etkinleştirmez; provider, unsubscribe, rate-limit
> ve teslim kanıtı ayrı owner/operasyon kapısı olarak korunur.

> Çevrim 90 güncellemesi (18 Ağustos 2026): Konu merkezi, yeni ve ince kategori üretmek yerine
> yayımlanmış içerik sayısıyla sıralanan gerçek çok dilli etiket arşivlerini Etiket Atlası olarak
> keşfe açtı. Root kategori seçimi deterministik hale geldi. Veri migration'ı yapılmadı. Otonom
> deploy zincirinin production öncesi zorunlu staging taxonomy/public smoke kapısı ayrı P0 operasyon
> borcu olarak korunuyor; bu kapı kapanmadan taxonomy migration terfisi yapılmamalı.

> Çevrim 89 güncellemesi (18 Ağustos 2026): Public sidebar ve ana sayfa Konu Atlası, API'de zaten
> bulunan parent/child taxonomy ilişkilerini koruyacak biçimde düzenlendi. Ana sayfa aynı no-store
> archive yanıtını navigasyonla paylaşarak yinelenen isteği kaldırıyor; daraltılmış ray lokalize
> erişilebilir adları koruyor ve küçük vurgu metni iki temada ayrı kontrast tokenı kullanıyor.
> Veri modeli, production verisi ve harici servis değişmedi.

> Çevrim 88 güncellemesi (18 Ağustos 2026): H2/H3 planı artık kalıcı bölüm hedefli göreve ve public gövde
> görseli terfisine bağlıdır. Başlık değişimi, eksik intrinsic ölçü ve çift yerleşim fail-closed engellenir;
> alt metin, kaynak, görev checkpoint'i ve audit izi atomik güncellenir. Harici sağlayıcı açılmadı.
> Sonraki P1 dilimler provider-neutral lease/attempt/backoff/dead-letter worker sözleşmesi ile kalıcı public
> crop odak modelidir; ücret, lisans ve dış veri aktarımı owner kararı olmadan etkinleştirilmeyecektir.

> Çevrim 87 güncellemesi (18 Ağustos 2026): Görsel Yenileme Stüdyosu artık uzun makaleyi ilk H2'ye
> indirgemiyor; tam gövdeden ilk/orta/son anlamlı H2/H3 sahnelerini en fazla üç özgün, yazısız bölüm
> briefine dönüştürüyor. Dış provider açılmadı ve otomatik yayın yapılmadı. Sonraki uygulanabilir dilim
> stable heading anchor'a bağlı locale-local body asset, provenance ve atomik rollback modelidir.

> Çevrim 81 güncellemesi (18 Ağustos 2026): Public yayın navigasyonu masaüstünde kullanıcı
> tercihini koruyan daraltılabilir raya dönüştürüldü; mobil drawer arka planı erişilebilirlik
> ağacından izole ediyor, breakpoint değişiminde scroll kilidini bırakıyor ve tüm dokunma
> hedefleri en az 44 px. Tema ve navigasyon tercihi paint öncesi uygulanarak görünür sıçrama
> azaltıldı. Web regresyon paketi CI kalite kapısına bağlandı. Harici sistem veya veri değişmedi.

> Çevrim 79 güncellemesi (18 Ağustos 2026): Görsel Yenileme Stüdyosu aday kanıtı tek bir genel
> konu onayından; tam makale, ilgili bölüm, locale/kültür, teknik doğruluk, yazısızlık, artefact
> ve tüm public crop kontrollerini ayrı ayrı gösteren fail-closed sözleşmeye taşındı. Retry veya
> red kararı eski aday ve kanıtlarını terfi edilemez kılar; audit önceki aday kimliğini korur.
> Harici üretim/vision sağlayıcısı ücret, lisans ve veri aktarımı owner kararı olmadan etkinleştirilmedi.

> Çevrim 78 güncellemesi (18 Ağustos 2026): locale yayın bütünlüğü, makale üzerinde yalnız
> gerçekten yayımlanmış karşılıkları gösteren görünür dil baskıları şeridiyle public deneyime
> taşındı. Dört arayüz sözlüğü aynı placeholder sözleşmesiyle doğrulanıyor; mevcut sürüm
> erişilebilir biçimde işaretleniyor ve eksik çeviri için yapay URL üretilmiyor.

> Çevrim 77 güncellemesi (18 Ağustos 2026): public Kaynak ve Güven Merkezi, yalnız kaynak
> listesi göstermekten çıkarılıp yayınların kaynak kapsamı, bağımsız alan adı çeşitliliği ve
> kaynak sınıflandırma/güncellik borcunu ölçen görünür bir kanıt panosuna dönüştürüldü.
> Bu göstergeler doğruluk garantisi olarak sunulmuyor. İddia düzeyi atıf modeli ayrı,
> geri alınabilir bir sonraki faz olarak korunuyor.

> Çevrim 74 güncellemesi (18 Ağustos 2026): taxonomy fazı production arşiv kanıtıyla
> **Zaman, Odak ve Planlama** keşif yolunu seçti. Otonom migration terfisindeki yanlış
> geliştirme-veritabanı yedeği varsayımı kaldırıldı; staging ve production ayrı hedeflenip
> her yedek exact-path izole restore testinden geçiyor. Production deploy ve canlı kanıt
> tamamlanmadan operasyon maddesi tamamlanmış sayılmaz. Sonraki aday, mevcut bağlı tag
> verisini kullanan ölçülebilir bir Etiket Atlası deneyidir.

Son güncelleme: 17 Ağustos 2026 — Çevrim 69. Bu kayıt moda odaklı yeniden yazım listesi değil; depo, canlı sağlık ve ürün kanıtına dayalı kalıcı karar defteridir.

## 1. Görsel sağlayıcı ve vision adaptör katmanı

- **Durum / öncelik:** Araştırılacak / P1
- **Problem ve kanıt:** Görsel kuyruğu, brief, benzerlik ve terfi var; production üretim/vision sağlayıcısı ve sağlık telemetrisi yok.
- **Değişiklik ve fayda:** Değiştirilebilir provider sözleşmesi, kota/timeout, provenance ve fail-closed vision sonucu; konu dışı görsel riski azalır.
- **Yüzey / kapsam:** API worker, admin Visual Studio, runtime secret store ve operasyon metriği.
- **Risk / veri etkisi:** Ücret, lisans, dış veri aktarımı; kalıcı sonuç modeli migration ister.
- **Geri dönüş:** Feature flag kapatılır, manuel aday incelemesi korunur.
- **Kabul:** Owner provider/bütçe kararı, staging hata senaryoları, sağlık bandı, otomatik yayın olmaması ve örneklem kalite PASS.

## 2. Production yedek ve izole geri-yükleme kanıtı

- **Durum / öncelik:** Uygulanacak / P0
- **Problem ve kanıt:** Zamanlanmış iş varsayılan geliştirme veritabanını hedefliyor; production dump ve restore-test kanıtı yok.
- **Değişiklik ve fayda:** Açık DB hedefi, checksum, günlük restore testi ve off-site kopya durumu; migration güvenliği sağlar.
- **Yüzey / kapsam:** PowerShell, Task Scheduler, PostgreSQL, admin operasyon görünümü.
- **Risk / veri etkisi:** Disk ve erişim yetkisi; production verisi değişmez.
- **Geri dönüş:** Eski görev tanımı dışa aktarılıp geri yüklenir.
- **Kabul:** Production adı loga sır sızdırmadan doğrulanır, restore izole DB’de geçer, alarm ve retention kanıtlanır.

## 3. İddia düzeyi kaynak bağlantısı

- **Durum / öncelik:** Araştırılacak / P2
- **Problem ve kanıt:** Makale kaynak listesi var fakat gövde iddiası ile kaynak ilişkisi yok.
- **Değişiklik ve fayda:** Editörde erişilebilir dipnot/atıf modeli ve public satır içi kanıt; güven ve güncelleme hızı artar.
- **Yüzey / kapsam:** İçerik veri modeli, editör, public makale, Article citation.
- **Risk / veri etkisi:** Eski içerik backfill’i ve kötü dipnot UX’i.
- **Geri dönüş:** İlişkiler kaldırılmadan sunum feature flag ile kapanır.
- **Kabul:** Her kritik iddia tekil kaynağa gider; klavye/mobil, canonical ve schema görünür içerikle uyumludur.

## 4. Türkçe pillar ve konu kümesi yenilemesi

- **Durum / öncelik:** Uygulanacak / P2
- **Problem ve kanıt:** Starter içeriklerde ortak altı bölümlü şablon ve tek kaynak kullanımı derinlik riskidir.
- **Değişiklik ve fayda:** Bir pillar + 4–6 destek içerikte benzersiz niyet, resmi kaynak ve çift yönlü iç link; trafik ve otorite artışı hedeflenir.
- **Yüzey / kapsam:** İçerik, taxonomy, editör, homepage keşfi, SEO ölçümü.
- **Risk / veri etkisi:** Cannibalization; yalnız substantive güncellemede tarih değişir.
- **Geri dönüş:** Revision geçmişinden önceki sürüm geri alınır.
- **Kabul:** Editoryal review, en az iki uygun domain, mobil/dark render ve 28 günlük ölçüm tabanı.

## 5. Yapılandırılmış keşif kapsam matrisi

- **Durum / öncelik:** Uygulanacak / P2
- **Problem ve kanıt:** Article/Breadcrumb güçlü; sayfa türü × locale regresyon kapısı merkezi değil.
- **Değişiklik ve fayda:** Canonical, hreflang, robots ve schema sözleşme testi; indeksleme regresyonunu release öncesi yakalar.
- **Yüzey / kapsam:** Next metadata, sitemap, CI/local release gate.
- **Risk / veri etkisi:** Yanlış katı test; veri migration’ı yok.
- **Geri dönüş:** Matris testi gevşetilir, runtime davranış korunur.
- **Kabul:** Dört locale ve tüm indexlenebilir türlerde self-canonical, gerçek alternatif ve görünür içerikle eş schema.

## 6. Arama sorgusu ve içerik boşluğu ölçümü

- **Durum / öncelik:** Kullanıcı kararı gerekli / P2
- **Problem ve kanıt:** Search Console bağlı değil; trafik fırsatı sayısal kanıt yerine proxy ile seçiliyor.
- **Değişiklik ve fayda:** Salt okunur GSC entegrasyonu ve privacy-safe query gap panosu.
- **Yüzey / kapsam:** OAuth bağlantısı, admin trafik, periyodik veri alma.
- **Risk / veri etkisi:** Üçüncü taraf hesap yetkisi; kullanıcı onayı gerekir.
- **Geri dönüş:** Token iptal edilir, dahili ölçüm çalışmaya devam eder.
- **Kabul:** Doğrulanmış property, minimum yetki, token secret store, sayı uydurmayan boş durum.

## 7. Locale yayın bütünlüğü kapısı

- **Durum / öncelik:** Uygulandı / P2
- **Problem ve kanıt:** Dört locale mevcut; çeviri kaynak snapshot’ı olsa da kalite ve kültürel görsel uygunluğu tam otomatik kapı değil.
- **Değişiklik ve fayda:** TranslationReviewed, kaynak snapshot, taxonomy ve locale-görsel kontrolünü tek yayın sözleşmesinde birleştirir.
- **Yüzey / kapsam:** API quality gate, language manager, SEO metadata.
- **Risk / veri etkisi:** Eski taslakların bloklanması; yayınlanmış içeriğe otomatik fallback yok.
- **Geri dönüş:** Yeni kapı yalnız yeni revision’larda uygulanır.
- **Kabul:** Eksik gerçek çeviri indexlenmez; canonical/hreflang yalnız gerçek eşleri gösterir.

## 8. Arama ve öneri kalite servisi

- **Durum / öncelik:** Araştırılacak / P2
- **Problem ve kanıt:** Yazım toleransı ve boş sonuç kurtarma roadmap borcu; içerik genişledikçe keşif kaybı büyür.
- **Değişiklik ve fayda:** Türkçe normalizasyon, kontrollü typo toleransı, topic-cluster önerisi ve açıklanabilir boş sonuçlar.
- **Yüzey / kapsam:** API sorguları/indexler, public arama, analytics.
- **Risk / veri etkisi:** Pahalı sorgu ve alakasız öneri; index migration gerekebilir.
- **Geri dönüş:** Eski exact/prefix arama feature flag ile korunur.
- **Kabul:** Sabit relevancy corpus’u, p95 bütçesi, XSS-safe terimler ve sıfır sonuç iyileşmesi ölçülür.

## 9. Canary sürüm kohortu ve tek eylem geri alma

- **Durum / öncelik:** Araştırılacak / P2
- **Problem ve kanıt:** Atomik deploy/rollback var; sınırlı trafik kohortu yok.
- **Değişiklik ve fayda:** Web+API uyumlu canary, health karşılaştırması ve yetkili rollback; değişiklik yarıçapını küçültür.
- **Yüzey / kapsam:** IIS/reverse proxy, release ledger, admin operasyon.
- **Risk / veri etkisi:** Session ve cache tutarlılığı; DB migration canary’den ayrılmalı.
- **Geri dönüş:** Trafik yüzde 100 son sağlıklı release’e döner.
- **Kabul:** Sentetik akışlar, kohort sürüm başlığı, hata eşiği ve otomatik rollback tatbikatı.

## 10. Üyelik bildirim merkezi ve Web Push

- **Durum / öncelik:** Kullanıcı kararı gerekli / P3
- **Problem ve kanıt:** Takip/kaydetme var; izinli geri dönüş kanalı yok.
- **Değişiklik ve fayda:** Açık opt-in, konu tercihleri, sessiz saatler ve tek tık çıkış; geri dönüşü artırabilir.
- **Yüzey / kapsam:** Service worker, push subscription API/verisi, admin gönderim ve ölçüm.
- **Risk / veri etkisi:** Bildirim izni, VAPID secret, spam ve gizlilik; ürün/iletişim kararı gerekir.
- **Geri dönüş:** Gönderim kapatılır, abonelikler güvenli biçimde pasifleştirilir.
- **Kabul:** Varsayılan kapalı, locale doğal metin, rate limit, unsubscribe ve teslim/etkileşim metriği.

## 11. Düzeltme ve şeffaflık iş akışı

- **Durum / öncelik:** Uygulanacak / P2
- **Problem ve kanıt:** Revision/audit var; okura görünür düzeltme özeti için bütünleşik akış yok.
- **Değişiklik ve fayda:** Editoryal onaylı correction note ve değişiklik özeti; yayın güvenini artırır.
- **Yüzey / kapsam:** Veri modeli, editör, public makale, feed/schema tarihleri.
- **Risk / veri etkisi:** Küçük yazım düzeltmelerinde gürültü.
- **Geri dönüş:** Not geri çekilmez; yeni auditli düzeltme ile düzeltilir.
- **Kabul:** Kim/ne/zaman kaydı, substantive eşik, locale doğrulaması ve public erişilebilir zaman çizgisi.

## 12. Rollback artefaktı retention politikası

- **Durum / öncelik:** Uygulanacak / P3
- **Problem ve kanıt:** Staging ve production’da çok sayıda rollback dizini birikmiş; disk tüketimi büyüyor.
- **Değişiklik ve fayda:** Son sağlıklı N release + yaş/kapasite eşiği ve dry-run raporu.
- **Yüzey / kapsam:** Deploy scriptleri, disk health ve operasyon raporu.
- **Risk / veri etkisi:** Gerekli rollback’in erken silinmesi.
- **Geri dönüş:** İlk faz yalnız raporlar; silme açık retention politikası sonrası açılır.
- **Kabul:** Doğrulanmış absolute path, aktif/son sağlıklı release koruması, dry-run ve disk alarmı.

## Ertelenen yaklaşım

- **Reddedildi/ertelendi:** Platformu yeni framework ile baştan yazmak. Kanıtlanmış ölçülebilir fayda yok; mevcut Next.js/ASP.NET/PostgreSQL mimarisi build, test, locale SEO ve atomik deploy kapılarını karşılıyor.
## Çevrim 85 güncellemesi — görsel sağlayıcı sağlık sözleşmesi

18 Ağustos 2026'da editoryal kütüphane, resmî/doğrulanmış kaynak, lisanslı stok ve temsili AI
sınıfları için yetenek, hak metadata ve editoryal inceleme sözleşmesi uygulandı. Admin Stüdyosu gerçek
API durumunu gösterir. HTTPS endpoint, korumalı credential ve owner etkinleştirmesi olmayan dış
sağlayıcı fail-closed kalır. Dış çağrı, ücret, secret yazımı veya veri aktarımı etkinleştirilmedi.
Production generation/vision adaptörü hâlâ owner kararı gerektirir; sonraki uygulanabilir dilim
locale-local bölüm/gövde görseli yerleşimidir.
