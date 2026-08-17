# BOECL Çevrim 63 — bağlamsal ve yazısız görsel yönü

## Görünür önce / sonra hedefi

Görsel Yenileme Stüdyosu daha önce tam metin ve ilk H2/H3 bağlamından güçlü bir brief
üretiyor, ancak her konuya aynı genel “editorial visual” yaklaşımını uyguluyordu. Artık admin,
her riskli makale için seçilen görsel türünü briefin yanında görür. Prosedür, karşılaştırma,
veri odaklı ve teknik içerikler dekoratif veya jenerik fotoğrafa zorlanmaz; doğal fotoğraf
yalnız gerçek dünya sahnesi konuyu en iyi anlattığında varsayılandır.

## Kalite sözleşmesi

- Seçim başlıkla sınırlı değildir; başlık, özet, ilgili bölüm ve tam gövde birlikte taranır.
- Teknik/güvenlik içeriği; hayali UI, yanlış parça ve güvensiz uygulamayı dışlayan teknik
  illustration yönüne gider.
- Prosedür ve karşılaştırmalar yazı, sayı, etiket, rozet ve ok kullanmadan kompozisyonla
  anlatılır.
- Her brief locale/coğrafya, tek odak, 16:9 mobil-safe crop, gerçekçi ayrıntı ve kapsamlı
  negative prompt taşır.
- Mevcut perceptual tekrar, boyut, format, hak bilgisi ve transaction tabanlı terfi kapıları
  değişmeden korunur; sağlam görsel otomatik değiştirilmez.

## Gerçek üretim kalite örneklemi

Canlı Türkçe arşivdeki Thread teşhis rehberinin tam metni ve bölüm sırası incelenerek yazısız
teknik hero örneği üretildi. İlk aday konu zincirini doğru anlattı fakat Türkiye yerine ABD tipi
duvar prizi ürettiği için reddedildi. İkinci prompt, Türkiye'de kullanılan Type-F Schuko fiziksel
bağlamını ve görünür priz için güvenli fallback'i açıkça tanımladı. Bu örnek, locale bilgisinin
yalnız prompt varlığıyla değil çıktı denetimiyle kalite kapısından geçmesi gerektiğini doğruladı.

## Sınır ve sonraki adım

Bu faz bağlamsal art direction'ı görünür ve testli hale getirir; üretim sağlayıcısının API
anahtarı veya otomatik vision puanı eklenmemiştir. Üretim örnekleri editoryal inceleme kanıtıdır,
otomatik yayın değildir. `visual-service` yol haritası maddesi; sağlayıcı adaptörü, sağlık
telemetrisi ve vision/locale çıktı denetimi tamamlanana kadar aktif kalır.
