# BOECL Çevrim 44 — Editoryal kapasite merkezi

## Görünür önce/sonra hedefi

Admin kontrol merkezi önceden yalnız kişisel ve ekip iş sıralarını gösteriyordu; yönetici
işin kimde yığıldığını, pasif kullanıcıda kalan sorumluluğu veya yeniden dengeleme
seçeneğini göremiyordu. Yeni kapasite yüzeyi aktif ekibi açık, gecikmiş ve 48 saat içinde
yaklaşan görev sayılarıyla karşılaştırır. Owner, Admin ve Editor rollerindeki kullanıcılar
aynı ekranda aktif bir ekip üyesini seçerek görevi yeniden atayabilir.

## Bütünlük ve güvenlik

- İş yükü açık görevlerden sunucuda hesaplanır; istemci tarafından gönderilen sayılara
  güvenilmez. Pasif kullanıcıya bağlı açık görevler ayrı sahiplik borcu olarak görünür.
- Yeniden atama yalnız `ManageEditorial` politikasıyla, CSRF doğrulamasıyla ve aktif hedef
  kullanıcı kontrolüyle yapılır.
- Eski ve yeni sorumlu kimliği `editorial.task_reassigned` audit olayına yazılır. Görev
  içeriği, yayın durumu veya son tarihi değişmez.
- Şema ve production verisi topluca değiştirilmedi; migration, yedek veya rollback veri
  operasyonu gerekmedi.
- Yeni arayüz metinleri Türkçe, İngilizce, Almanca ve Fransızca sağlandı; kapasite kartları
  masaüstünde üç, tablette iki, mobilde tek sütuna iner.

## Kalıcı backlog

1. Tamamlanan görev sürelerinden p50/p95 ve gecikme oranı trendleri.
2. Ölçülmüş ekip throughput'una dayalı, yapılandırılabilir kapasite eşikleri.
3. Seçili görevler için önizlemeli ve geri alınabilir toplu yeniden atama.
4. İş türü/locale/yayın masası becerisine göre önerilen sorumlu.
