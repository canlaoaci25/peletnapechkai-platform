# BOECL Çevrim 45 — Kaynak ve Güven Merkezi

## Görünür önce/sonra hedefi

BOECL kaynakları daha önce yalnız tekil makalelerin altındaki dış bağlantılarda görünüyordu.
Okur hangi kaynak alanlarının arşivde ne ölçüde kullanıldığını göremiyor, aynı kaynakla
hazırlanan diğer yayınları keşfedemiyor ve arama motorları bu kanıt ilişkisini kalıcı bir
iç bağlantı mimarisinde izleyemiyordu.

Yeni `/{locale}/sources` yüzeyi gerçek yayımlanmış içerikten kaynak alanı, atıf ve yayın
sayısını üretir. Her alan adı `/{locale}/sources/{domain}` altında kapaklı yayın akışına,
yerelleştirilmiş metadata'ya ve kendine referans veren canonical URL'ye sahiptir. Makale
kaynak kutuları dış kanıt bağlantısını korurken alan adını BOECL kaynak arşivine bağlar.

## SEO, içerik ve güven bütünlüğü

- Yalnız etkin locale'deki yayımlanmış içerikler sayılır; taslaklar ve başka dildeki
  içerikler sonuçlara karışmaz.
- Alan adları güvenli HTTP(S) URL'lerinden türetilir, küçük harfe çevrilir ve `www` eşleri
  tek kanonik alanda birleştirilir. Geçersiz rota girdileri 404 olur.
- Kaynak merkezi dört etkin locale için doğal arayüz metni, locale-aware tarih biçimi,
  canonical metadata ve sitemap girdileri sunar.
- Kaynak arşivi editoryal doğrulama iddiasında bulunmaz; yalnız BOECL'in gerçek atıf
  kullanımını şeffaflaştırır. Dış bağlantıların `nofollow noopener noreferrer` politikası
  değişmedi.
- Masaüstünde üç, tablette iki, mobilde tek sütunlu kaynak kataloğu; kaynak detayında
  görsel öncelikli yayın akışı ve klavye ile erişilebilir bağlantılar sağlandı.
- Şema veya uygulama verisi değiştirilmedi; migration, yedek ya da rollback veri işlemi
  gerekmedi.

## Kalıcı backlog

1. Kaynak sahibi, son editoryal kontrol zamanı ve doğrulama notunu audit iziyle yönetmek.
2. Sunucu taraflı, SSRF güvenli periyodik HTTP sağlık kontrolü ve bozuk kaynak kuyruğu.
3. Search Console sorgularını kaynak gücü yüksek/zayıf Türkçe içerik kümeleriyle eşlemek.
4. Kaynak türü sınıflandırması: resmi kurum, birincil araştırma, sektör verisi ve ikincil
   haber kaynağı.
5. Alan adı değişimleri ve birleştirmeleri için geri alınabilir kanonik kaynak yönetimi.
