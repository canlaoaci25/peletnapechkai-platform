# BOECL Çevrim 35 — Okuma sürekliliği

## Görünür önce/sonra hedefi

Üyeler daha önce yazı kaydedebiliyor ve konu takip edebiliyordu; yarım bıraktıkları bir
yazıya nereden döneceklerini göremiyordu. Bu faz, makalede ince bir canlı ilerleme göstergesi
ve hesap sayfasının en üstünde kapaklı, yüzdeli, son H2/H3 bölümüne bağlı **Kaldığın yerden
devam et** rafı ekler. Raf dört locale, açık/koyu tema ve mobil yatay kart akışıyla çalışır.

## Veri, güvenlik ve operasyon

- `article_reading_progress`, yalnız oturumdaki kullanıcı ile yayımlanmış locale makalesini
  bağlar. İstemciden kullanıcı kimliği alınmaz; `(user_id, article_localization_id)` benzersizdir.
- Yüzde 0–100, bölüm kimliği 160 karakter ile sınırlıdır. Liste yalnız yüzde 5–94 arasındaki
  son sekiz yarım okumayı döndürür; hesap dışı ve taslak içerik görünmez.
- Yazma CSRF korumalıdır, ilk okuma ilişkisi audit edilir. Her kaydırmada audit üretilmez;
  istemci yalnız anlamlı on puanlık farklarda checkpoint yazar.
- Migration yalnız yeni tablo ve indeksler ekler, geri alınabilir. Production uygulamasından
  önce custom-format yedek ve staging migration/health kapısı zorunludur.
- Üyelik yüzeyi `noindex` kalır; açık makale canonical/hreflang ve public render yolu değişmez.

## Kabul kanıtı

- Web regresyonları, locale kontrolü, lint ve typecheck geçti.
- 113 API testi geçti; anonim listeleme ve yazma için 401 regresyonları eklendi.
- Next.js production build ve .NET Release build geçti. Mevcut Skia obsolete uyarıları bu
  fazdan önce vardır ve hata değildir.
- Staging/production yedek, migration, dağıtım, sağlık ve canlı URL sonucu çevrim kapanışında
  ayrıca doğrulanacaktır.

## Sonraki üyelik backlog'u

1. Açık rızalı özet tercih merkezi ancak doğrulanmış teslim altyapısıyla.
2. Hesap verisi dışa aktarma ve silme self-service akışı.
3. Kaydedilenler ve devam listesinde arama/filtreleme.
4. Veri minimizasyonlu save→return funnel ölçümü.
