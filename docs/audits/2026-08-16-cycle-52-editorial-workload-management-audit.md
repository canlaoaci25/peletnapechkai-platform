# BOECL Çevrim 52 — Editoryal iş yükü yönetimi

## Görünür önce / sonra hedefi

Kontrol merkezi ekip yükünü ve tekil sorumlu değişimini gösteriyordu; yoğun bir masayı dengelemek
için her görev ayrı ayrı ve önizlemesiz taşınıyordu. Artık yetkili kullanıcı ekip kuyruğunda en
fazla 25 açık görevi seçer, etkilenecek içerikleri ve eski/yeni sorumluyu tek listede inceler ve
tek onayla topluca yeniden atar. Başarılı işlem ekranda geri alma eylemiyle birlikte görünür.

## Güvenlik ve veri bütünlüğü

- Toplu atama `ManageEditorial`, antiforgery ve aktif hedef kullanıcı kontrolleri altındadır.
- İstemcinin gönderdiği görev kümesi sunucuda benzersizlik, sınır, varlık ve açık durum açısından
  yeniden doğrulanır. En fazla 25 görev sınırı kötüye kullanım ve kilit süresini sınırlar.
- Tüm görevler tek veritabanı transaction'ında değişir; eksik veya kapanmış görev varsa işlem
  başlamadan `409 Conflict` döner ve kısmi atama oluşmaz.
- Her değişiklik batch kimliği, önceki/yeni sorumlu ve makale kimliğiyle append-only audit izine
  yazılır. Geri alma veriyi istemciden değil bu audit kaydından kurar.
- Geri alma yalnız işlemi yapan kullanıcıya, on dakika boyunca ve görevlerin hiçbiri daha sonra
  değişmemişse açıktır. Bir çakışmada hiçbir görev geri alınmaz.
- Şema veya üretim verisi topluca değiştirilmedi; migration ve veri yedeği gerektiren bir operasyon
  yapılmadı.

## Deneyim ve erişilebilirlik

Toplu yönetim yalnız ekip kuyruğunda görünür. Seçim sayısı, hedef sorumlu, önizleme diyaloğu,
başarı/çatışma mesajı ve geri alma eylemi Türkçe, İngilizce, Almanca ve Fransızca sunulur. Masaüstü
araç şeridi mobilde tek sütuna iner; görev seçimleri doğal checkbox ve açıklayıcı erişilebilir ad
kullanır. Açık/koyu admin tokenları korunur.

## Kalıcı backlog

1. Tamamlanan görevlerden p50/p95 çevrim süresi ve gecikme oranı trendleri.
2. Ölçülmüş throughput'a dayalı yapılandırılabilir kapasite eşikleri.
3. İş türü, locale ve yayın masası becerisine göre açıklanabilir sorumlu önerisi.
4. Audit geçmişinden uzun vadeli toplu işlem raporu ve Owner kapsamlı geri yükleme akışı.
