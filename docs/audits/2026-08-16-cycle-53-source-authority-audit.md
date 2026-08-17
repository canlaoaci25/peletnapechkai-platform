# BOECL Çevrim 53 — Kaynak Otoritesi Katmanı

## Görünür önce / sonra hedefi

Kaynak ve Güven Merkezi daha önce yalnız alan adı, atıf ve yayın sayısı gösteriyordu. Okur bir
kaynağın resmî kurum, birincil araştırma, sektör verisi veya haber yayını olup olmadığını; kaydın
insan editör tarafından ne zaman incelendiğini ayırt edemiyordu. Yeni yüzey kaynak türünü ve son
editoryal inceleme tarihini kart düzeyinde gösterir. İncelenmemiş eski kayıtlar güven iddiası
üretmeden açıkça “Henüz sınıflandırılmadı” olarak kalır.

## Veri, güvenlik ve editoryal bütünlük

- `SourceKind` kontrollü enum'u ve nullable `LastReviewedAt` kalıcı veri modeline eklendi.
- Migration mevcut kayıtları güvenli `Unclassified` durumunda tutar; `Down` yolu iki yeni sütunu
  geri alır. Production uygulamasından önce mevcut yedekli deploy hattı kullanılır.
- Kaynak incelemesi yalnız mevcut editoryal yönetim politikası içinde, antiforgery doğrulamasıyla
  yapılır. Sunucu izin verilen enum değerini yeniden doğrular.
- Her inceleme `supporting.source_reviewed` olayıyla append-only audit izine yazılır. İstemci tarih
  veya doğrulanmışlık iddiası gönderemez; zaman damgasını sunucu üretir.
- Public API yalnız yayımlanmış, etkin locale içeriklerinden kaynak özeti üretme kuralını korur.

## SEO, yerelleştirme ve trafik etkisi

Google'ın insanlar için yararlı içerik rehberi açık kaynaklandırma ve güven kanıtını önerir. Faz,
arama motoru için yapay bir puan üretmek yerine okura gerçek editoryal bağlam verir ve kaynak
arşivlerine giden mevcut taranabilir iç bağlantıları korur. Merkez açıklaması, türler, inceleme
durumu ve admin kontrolleri Türkçe, İngilizce, Almanca ve Fransızca tamamlandı.

Araştırma: https://developers.google.com/search/docs/fundamentals/creating-helpful-content

## Kalıcı backlog

1. Kaynak sahibi ve periyodik yeniden inceleme SLA'sı.
2. DNS rebinding savunmalı, SSRF güvenli periyodik bağlantı sağlık kontrolü.
3. Kaynak sınıfı çeşitliliğini içerik otoritesi kuyruğuna dahil etmek.
4. Search Console sorgu fırsatlarını Türkçe kaynak otoritesi kümeleriyle eşlemek.
