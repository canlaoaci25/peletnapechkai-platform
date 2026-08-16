# BOECL Çevrim 29 — Arama ve Keşif Merkezi

## Görünür önce/sonra hedefi

Canlı `/tr-TR/search` yüzeyi daha önce yalnız başlık ve özetten oluşan düz bir sonuç
listesiydi. Çevrim sonunda arşiv araması; konuya özel mevcut kapak, kategori yolu, içerik
türü, yayın tarihi ve kaynak sayısını tek responsive kartta gösterir. Başlık ve özet
eşleşmeleri yalnız gövdede geçen sözcüklerden önce sıralanır. Aynı deneyim dört etkin
locale'de doğal arayüz metniyle sunulur.

## SEO, içerik ve bütünlük

- Arama sayfaları kişiye/sorguya bağlı ve düşük değerli URL çoğalmasını önlemek için
  `noindex, follow` kalır; sonuçlardaki makale ve kategori bağlantıları taranabilirdir.
- API yalnız etkin locale'deki yayımlanmış içerikleri döndürür. Draft veya başka dilde
  sessiz fallback yoktur.
- Yeni veya yapay içerik/görsel üretilmez. Yalnız makalenin editoryal olarak atanmış,
  optimize edilmiş mevcut kapağı kullanılır; dekoratif yazılı görsel eklenmez.
- Kaynak sayısı doğrulanmış ilişki kaydından, taxonomy ise gerçek locale kategorisinden
  hesaplanır. İstemci güven niteliği uydurmaz.
- Görsel bağlantısı ikinci klavye durağı oluşturmaz; metin başlığı erişilebilir ana
  bağlantıdır. Kartlar 640 px altında tek kolona iner ve gerçek responsive `sizes`
  sözleşmesi kullanır.

## Kalıcı backlog

1. Türkçe PostgreSQL full-text/trigram indeksli arama ve ölçülmüş sorgu planı.
2. Yazım hatası toleransı, eşanlamlılar ve editör yönetimli arama sözlüğü.
3. Sonuç tıklaması ve sıfır-sonuç sorgularının gizlilik uyumlu ölçümü.
4. Sıfır-sonuç sorgularından Search Console verisiyle içerik boşluğu önerileri.
5. Cursor tabanlı arama ve kategori arşivi sayfalaması.
