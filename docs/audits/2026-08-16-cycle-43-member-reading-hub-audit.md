# BOECL Çevrim 43 — Kişisel okuma merkezi

## Görünür önce/sonra hedefi

Hesap daha önce devam edilenler, kişisel akış, takipler ve kayıtları uzun ve birbirinden
kopuk bölümler halinde gösteriyordu. Üye artık dört alanın gerçek sayısını tek bakışta görür,
doğrudan ilgili rafa geçer ve okuma listesini başlık, özet veya gerçek içerik türüyle
locale-duyarlı biçimde arayıp süzer. Masaüstünde dört, mobilde iki sütunlu merkez navigasyonu;
mobil filtrelerde tek sütun kullanılır.

## Kanıt ve ürün kararı

- Önceki üyelik backlog'u kaydedilenlerde arama/filtrelemeyi açık iş olarak tanımlıyordu.
- BBC Help'in güncel hesapla cihazlar arası kayıt ve doğrudan Saved erişimi deseni incelendi;
  BOECL bunu kişisel akış, takip ve okuma sürekliliğiyle tek merkezde birleştirdi.
- Teslim altyapısı doğrulanmadığı için e-posta özeti veya örtük bildirim rızası eklenmedi.
- Yeni veri toplama, profil çıkarımı, indekslenebilir hesap yüzeyi veya içerik duvarı yoktur.

## Kalite kapıları

- Arama native `search`, tür filtresi native `select`, sonuç sayısı `aria-live` output kullanır.
- Dört locale tüm yeni metinleri içerir; Türkçe eşleştirme aktif locale ile yapılır.
- Mevcut CSRF korumalı kayıt kaldırma ve takipten çıkma yolları değişmemiştir.
- Web testleri, locale kontrolü, lint, typecheck ve production build geçti.
- API testleri, Release build, staging/production ve gerçek ortam doğrulama sonuçları çevrim
  kapanışında kaydedilecektir.

## Sonraki üyelik backlog'u

1. Doğrulanmış gönderim altyapısıyla açık rızalı özet tercih merkezi.
2. Hesap verisi dışa aktarma ve silme self-service akışı.
3. Veri minimizasyonlu save → return ölçümü ve üye kohortları.
4. Üyenin kontrol ettiği kişisel akış sıralaması ve konu sessize alma.
