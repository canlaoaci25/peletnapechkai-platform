# BOECL Çevrim 27 — Konu takipleri ve kişisel keşif

## Görünür hedef

Çevrim 19 hesaplar arası kalıcı okuma listesini tamamladı; ancak üyeler ilgi alanlarını
belirleyemiyor ve geri döndüklerinde yeni içerikten kendilerine ayrılmış bir akış
göremiyordu. Bu faz kategori otorite merkezine erişilebilir **Konuyu takip et** eylemi,
hesaba **Takip ettiğin konular** yönetimi ve takip edilen konuların en yeni gerçek
yayımlarından oluşan kapaklı **Senin için** akışı ekler. Deneyim dört etkin locale'de,
açık/koyu temada ve mobil/masaüstünde aynı ürün sözleşmesini korur.

## Veri, güvenlik ve yayın bütünlüğü

- `followed_categories` ilişkisi kullanıcı ve locale kategorisini bağlar; istemci kullanıcı
  kimliği göndermez. `(user_id, category_id)` benzersiz indeksi tekrarları engeller.
- Liste, durum, takipten çıkarma ve kişisel akış her istekte oturum sahibine göre filtrelenir.
  Yazma işlemleri antiforgery korumalı ve audit kayıtlıdır.
- Kişisel akış yalnız seçili locale'deki `Published` içerikleri döndürür; çeviri fallback'i,
  taslak sızıntısı veya üyelik duvarı oluşturmaz. Hesap rotaları index dışı kalır.
- Migration geri alınabilir. Staging ve production öncesinde custom-format PostgreSQL
  yedeği, migration, sağlık kapısı ve otomatik rollback yolu zorunludur.

## Kabul kriterleri

- Anonim kullanıcı takip API'sine erişemez; kategori yüzeyinden yerelleştirilmiş giriş
  akışına yönlendirilir.
- Aynı üye aynı konuyu yalnız bir kez takip eder ve başka üyelerin ilişkisini göremez.
- Kategori CTA'sı native button, sabit etiket mantığı, `aria-pressed`, durum mesajı,
  görünür focus ve en az 44 px hedef sunar.
- Hesap yüzeyi 320–1440 px'te kapaklı kişisel akış, konu yönetimi, boş/hata durumları ve
  mevcut okuma listesini birlikte sunar.
- Locale, web test/lint/typecheck/build, API test/Release build, staging ve production
  canlı kapıları geçmeden faz tamamlanmış sayılmaz.

## Sonraki kalıcı backlog

1. Açık rızalı haftalık özet tercih merkezi ve doğrulanmış e-posta teslimi.
2. Okuma ilerlemesi ve “kaldığın yerden devam et”.
3. Kaydedilenler ve kişisel akışta filtre/sıralama.
4. Üye dönüşü, takip ve revisit funnel ölçümü; kişisel veri minimizasyonu.
5. Hesap silme/veri dışa aktarma self-service akışı.
