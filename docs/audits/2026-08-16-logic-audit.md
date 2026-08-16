# BOECL İş Mantığı Denetimi

Tarih: 16 Ağustos 2026

Bu denetim sözdizimi, derleme veya paket açığından farklı olarak sistemin geçerli girdiler karşısında yanlış, çelişkili ya da kullanıcı beklentisine uymayan sonuç üretme ihtimalini inceler. Bulgular kod yolları çapraz okunarak sınıflandırılmıştır. Canlı veriye zarar verecek deneyler yapılmamıştır.

## Yönetici özeti

Doğrulanmış 8 mantık kusuru, 6 tasarım riski ve önemli test boşlukları tespit edildi. En yüksek öncelik yeni locale yaşam döngüsü, ülke-locale tekillik kuralı, çalışan otomasyon işinin gerçek iptali ve ana sayfa yerleşim doğrulamasıdır.

## Doğrulanmış mantık kusurları

### L-01 — Yönetimden eklenen yeni dil public arayüzde otomatik çalışmıyor

Önem: Yüksek

Kanıt:

- API herhangi bir geçerli `dil-BÖLGE` kültürünü `Locale` olarak ekleyip etkinleştirebiliyor.
- Web `hasLocale` kontrolü yalnız derleme zamanındaki `config/supported-locales.json` içindeki dört locale'i kabul ediyor.
- Proxy veritabanından aldığı dilleri tekrar `hasLocale` ile süzüyor; sayfa layout'ları da desteklenmeyen locale'i 404 yapıyor.
- Başlıklar, yasal metinler, üyelik, admin ve diğer bileşenlerde dört locale'e özel statik sözlükler var.

Etki: Admin panelinde yeni dil eklemek başarılı görünür; kategori/çeviri otomasyonu bu dili hedefleyebilir, ancak ziyaretçi o dilde public siteyi açamaz. “Yeni dil eklediğimde sistem çalışsın” beklentisi mevcut mimariyle karşılanmıyor.

Çözüm: Locale için iki ayrı durum tanımlanmalı: `ContentEnabled` ve `UiReady/PublicEnabled`. Yeni dil ekleme işi sözlük, rota smoke testi, yasal metinler ve bileşen kopyaları tamamlanmadan public etkinleşememeli. Alternatif olarak arayüz metinleri veritabanı tabanlı dinamik paket haline getirilmeli.

### L-02 — Bir ülke birden fazla etkin locale'e bağlanabiliyor

Önem: Yüksek

Kanıt:

- Yeni locale oluşturulurken o dilin konuşulduğu tüm bölgeler etkin `LocaleCountry` olarak ekleniyor.
- Aynı ülkenin başka bir locale'de etkin olmasını engelleyen domain, endpoint veya veritabanı kuralı yok.
- Proxy hedef dili `directory.locales.find(...)` ile ilk eşleşmeden seçiyor.

Etki: Örneğin ileride `en-GB` eklenirse GB hem `en-US` hem `en-GB` altında etkin olabilir. Sonuç editörün niyetine göre değil API sıralamasına göre belirlenir. Yeni locale eklemek mevcut ziyaretçilerin yönlendirmesini sessizce değiştirebilir veya hiç değiştirmeyebilir.

Çözüm: Etkin ülke eşlemesi için veritabanında tekillik kuralı kurulmalı ya da açık öncelik alanı eklenmeli. Locale oluşturulurken yalnız ana bölge varsayılan açık, diğer bölgeler kapalı gelmeli. Etkinleştirme sırasında çakışma kullanıcıya gösterilmeli.

### L-03 — “Duraklat” çalışan Codex işini gerçekten duraklatmıyor

Önem: Yüksek

Kanıt:

- Admin `Pause` yalnız veritabanındaki durumu `Paused` yapıyor.
- Windows worker/Codex alt sürecine iptal sinyali, process kimliği veya kooperatif kontrol gönderilmiyor.
- Çalışan süreç üretime devam ediyor; sonraki heartbeat/teslim çağrıları yalnız `Running` kabul ettiği için `Conflict` alıyor.

Etki: Kullanıcı işin durduğunu sanırken CPU, süre ve model kullanımı devam edebilir. Üretilen sonuç teslim edilemez ve yeniden denemede aynı iş tekrar maliyet yaratabilir.

Çözüm: Worker lease + cancellation token modeli eklenmeli. Her paket arasında durum kontrol edilmeli; işletim sistemi process kimliği saklanıp kontrollü sonlandırma yapılmalı. UI “duraklatma istendi” ve “gerçekten durdu” durumlarını ayırmalı.

### L-04 — “İptal” çalışan süreci durdurmuyor ve yeni işle çakışabiliyor

Önem: Yüksek

Kanıt:

- `Cancel` yalnız işi `Cancelled` yapıyor.
- Otomatik scheduler `Cancelled` işi meşgul saymıyor ve yeni iş açabiliyor.
- Eski Codex süreci halen çalışıyor olabilir; zamanlanmış görev tek-instance olsa da veritabanı yeni işi kuyrukta gösterir ve kullanıcı kaynak kullanımını doğru okuyamaz.

Etki: Hayalet çalışma, boşa model kullanımı, yanıltıcı kuyruk durumu ve teslim sırasında 409 oluşabilir.

Çözüm: L-03 ile aynı lease/iptal protokolü; iptal tamamlanana kadar `Cancelling` ara durumu ve yeni otomatik iş açılmasını engelleme.

### L-05 — Otomasyon retry sayaçlarını sıfırlamıyor

Önem: Orta

Kanıt:

- `Retry` durumu tekrar `Queued` yapıyor fakat `FailedItems`, `CompletedItems` ve `CurrentPhase` değerlerini sıfırlamıyor veya açık bir checkpoint politikası uygulamıyor.
- `Complete`, `CompletedItems = TotalItems - FailedItems` hesaplıyor.

Etki: Önceki denemede başarısız öğe sayısı raporlandıysa sonraki başarılı deneme tamamlanmış olsa bile eksik tamamlanmış sayılabilir. UI eski faz/sayaçları yeni denemeye aitmiş gibi gösterebilir.

Çözüm: Retry semantiği seçilmeli: tam yeniden başlatmada sayaçları sıfırla; checkpoint devamında ise ayrı `Attempt` kayıtları ve deneme-bazlı sayaç tut.

### L-06 — Çalışan Hazır İçerik işinde Türkçe kaynak arşivlenirse iş kilitlenebilir

Önem: Orta

Kanıt:

- Üretim teslim endpoint'i `alreadyCreated` hesabında aynı işe bağlı varsayılan dildeki tüm durumları sayıyor.
- Tamamlanma sayacı yalnız `Published` kaynakları sayıyor.
- İş sırasında üretilen kaynak arşivlenirse yeni kaynak üretimi `alreadyCreated` nedeniyle engellenir, tamamlanma ise yayındaki kaynak olmadığı için sürekli eksik görür.

Etki: İş retry ile kendini iyileştiremez; manuel veritabanı/iş müdahalesi gerekebilir.

Çözüm: Checkpoint sayımı tek bir ortak kurala bağlanmalı. Archived kaynak ya açıkça yeniden üretilebilir olmalı ya da iş açıklayıcı terminal hataya alınmalı.

### L-07 — Kategori çeviri tamamlanması gerçek kayda değil audit loguna bağlı

Önem: Orta

Kanıt:

- Eksik kategori çevirisi `automation.category_localized` audit kayıtlarından hesaplanıyor.
- Çevrilmiş kategori sonradan silinse veya bozulsa bile audit kaydı durduğu için sistem onu tamamlanmış kabul ediyor.
- Var olan bir kategori audit izi olmadan eklenmişse otomasyon onu eksik sayıp ikinci kategori oluşturabilir; yalnız slug çakışınca son ek ekler.

Etki: Yeni kategori çevirileri eksik kalabilir veya anlamsal olarak yinelenen kategoriler oluşabilir.

Çözüm: Kaynak kategori kimliği ile hedef kategori arasında kalıcı ilişki/tablo oluşturulmalı. Audit yalnız tarihçe olmalı, güncel doğruluğun kaynağı olmamalı.

### L-08 — Ana sayfa manuel yerleşimi geçerli görünen girdide 500 üretebilir

Önem: Yüksek

Kanıt:

- Kaydetme endpoint'i ilk beş yerleşimi kabul ediyor; `Lead` sayısını veya `Editors` üst sınırını doğrulamıyor.
- Beş `Editors` kaydı kaydedilebilir.
- Okuma sırasında `Take(4 - editors.Count)` çağrılır. `editors.Count` 5 olduğunda negatif değer oluşur ve LINQ `Take` hata verir.
- Aynı anda birden fazla `Lead`, yinelenen pozisyon ve anlamsız sıralamalar da engellenmiyor.

Etki: Admin tarafından kaydedilebilen bir yerleşim public ana sayfa API'sini 500'e düşürebilir.

Çözüm: En fazla bir Lead, en fazla dört Editors, benzersiz makale ve bölüm içinde benzersiz pozisyon doğrulaması eklenmeli; okuma tarafı da `Math.Max(0, 4-editors.Count)` ile savunmalı olmalı.

## Doğrulanmış davranış tutarsızlıkları

### B-01 — “Her 3 dakikada üretim” metni gerçek davranışı yanlış anlatıyor

Scheduler 15 saniyede bir kontrol eder. `NextRunAt` iş kuyruğa alınırken üç dakika ileri alınır; fakat iş bir saat sürerse süre çoktan geçmiş olur ve iş biter bitmez yaklaşık 15 saniye içinde yenisi açılır. Dolayısıyla davranış “önceki iş bittikten üç dakika sonra” değil, “sistem boşsa ve son kuyruklamadan üç dakika geçtiyse hemen” şeklindedir.

Öneri: UI metnini “Boş kaldığında otomatik üret” olarak değiştirin veya `NextRunAt` değerini iş tamamlanmasından itibaren hesaplayın.

### B-02 — Admin ana sayfa istatistiklerinde pay ve payda farklı veri kümelerinden geliyor

- Sistem durumundaki `total/published` tüm veritabanını sayıyor.
- Admin makale listesi yalnız son 100 kaydı getiriyor.
- Taslak, inceleme, planlanan ve tür dağılımı bu 100 kayıt üzerinden hesaplanıyor; yayın oranı ve toplam ise bütün sistemden.

Etki: Ekrandaki dağılımlar birbirini toplamaz; kullanıcı yanlış operasyonel karar verebilir.

Öneri: API ayrı aggregate istatistik endpoint'i vermeli; liste sayfalı olmalı.

### B-03 — Otomatik ana sayfanın lideri trend puanıyla seçilmiyor

Kod trend listesini `Score` ile sıralıyor, fakat manuel lider yoksa `articles[0]` kullanıyor. `articles` yayın tarihine göre sıralı olduğundan lider “en yeni”, trend puanı en yüksek yayın değildir. “Otomatik trend motoru” etiketi bu davranışı tam karşılamıyor.

### B-04 — Trend metriği istemci tarafından sınırsız şişirilebilir

Public engagement endpoint'inde özel rate limit, ziyaretçi/oturum tekilleştirmesi veya tekrar engeli yok. Her istek view artırabilir; `engaged` isteği başına 300 saniyeye kadar eklenebilir. Global limiter tanımlı değil, yalnız kimlik endpoint'leri özel politika kullanıyor.

Etki: Bot veya tekrar çağrısı ana sayfa trend sırasını manipüle edebilir.

Öneri: Oturum+makale bazlı idempotency, zaman penceresi, IP hash/anonim anahtar ve rate limit eklenmeli. Ham sayaç yerine güvenilir olaylardan türetilen skor kullanılmalı.

### B-05 — Yeni locale mevcut otomasyon işlerine sonradan dahil olmuyor

İş oluşturulurken hedef locale dizisi snapshot alınır. Yeni dil etkinleştirilse bile çalışan veya kuyruktaki 50'lik iş o dili üretmez. Bu teknik olarak tutarlı, ancak admin ekranında açıklanmıyor ve kullanıcı yeni dilin otomatik yakalanmasını bekliyor.

Öneri: “İş oluşturulduğundaki hedef diller” açıkça gösterilmeli; yeni locale sonrası eksik çeviri tamamlayıcı işi otomatik açılmalı.

### B-06 — Çeviriler kategori ve etiket ilişkilerini taşımıyor

Çeviri aynı ArticleGroup'a ekleniyor fakat locale'e özel kategori ve etiket ataması yapılmıyor. İçerik public olur; ancak hedef dil kategori sayfalarında görünmeyebilir ve navigasyon/keşif ağı zayıflar. Kategori çeviri otomasyonu ile makale çeviri otomasyonu arasında kaynak-hedef taxonomy bağı bulunmadığı için otomatik eşleme yapılamıyor.

## Test boşlukları

Mevcut 65 API testi temel domain ve güvenlik akışlarını doğruluyor; aşağıdaki kritik senaryolar için doğrudan regresyon testi bulunamadı:

1. Yeni locale ekleme → UI paketi hazır değilken public etkinleşmeme.
2. Aynı ülkenin iki etkin locale'e atanması.
3. Çalışan işte pause/cancel sonrası gerçek worker davranışı.
4. Retry sonrası sayaç ve attempt bütünlüğü.
5. Hazır içerik kaynağının iş sırasında arşivlenmesi.
6. Kategori çevirisinin silinmesi veya audit kaydının eksik olması.
7. Beş Editors / birden fazla Lead ana sayfa yerleşimi.
8. Engagement tekrar çağrısı ve trend manipülasyonu.
9. Admin aggregate istatistiklerinin 100'den fazla içerikte doğruluğu.
10. Yeni dil eklenince sözlük, rota, SEO, sitemap ve health kapsamının atomik genişlemesi.

## Önerilen düzeltme sırası

### Faz 1 — Veri ve public doğruluk

1. Locale için `UiReady/PublicEnabled` kapısı.
2. Ülke-locale tekillik/öncelik kuralı.
3. Ana sayfa placement doğrulaması ve negatif `Take` savunması.

### Faz 2 — Otomasyon durum makinesi

1. Worker lease ve attempt modeli.
2. Gerçek pause/cancel protokolü.
3. Retry sayaçları ve archived checkpoint kuralı.

### Faz 3 — İçerik ilişkileri ve ölçüm

1. Kaynak-hedef kategori eşleme tablosu.
2. Çevirilerde kategori/etiket aktarımı.
3. Güvenilir ve limitli engagement ölçümü.
4. Admin için gerçek aggregate istatistik endpoint'i.

## Sonuç

Sistemin test ve derleme kapılarının yeşil olması bu iş kuralı kusurlarını dışlamıyor. En büyük mimari çelişki, locale yönetiminin dinamik görünmesine rağmen web arayüzünün dört dilde statik olmasıdır. En acil çalışma riski ise ana sayfa yerleşiminin public 500 üretebilmesi ve çalışan otomasyonun admin durum değişiklikleriyle gerçekten kontrol edilememesidir.
