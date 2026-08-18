# BOECL Çevrim 79 — konuya uygun, yazısız görsel kanıt kapısı

## Görünür önce / sonra hedefi

Görsel Yenileme Stüdyosu daha önce editörden yalnız genel konu eşleşmesi ve yazı/logo yokluğu
onayı alıyordu. Yönetici artık adayın tam makaleye ve belirtilen bölüme uyumunu, locale/kültürel
bağlamını, teknik/fiziksel doğruluğunu, görünür AI artefact yokluğunu, yazısızlığını ve beş public
kırpmada ana öznenin korunmasını ayrı ayrı görür ve onaylar. Tek bir eksik kanıt yayına terfiyi kapatır.

## Yayın bütünlüğü

- Kanıt sürümü `editorial-attestation-v2` olarak audit edilir; istemci sayısal skor belirleyemez.
- Konu skoru makale + bölüm + locale + teknik doğruluk; güvenlik skoru yazısızlık + artefact
  yokluğu birlikte doğrulanınca oluşur.
- Crop skoru sunucu oran ölçümünün yanında editörün gerçek public crop matrisini onaylamasını ister.
- Retry veya red eski aday, lisans/atıf, alt metin, skor, benzerlik ve attestation kanıtını
  geçersiz kılar. Önceki aday kimliği append-only audit kaydında kalır.
- Ham `approve` kestirmesi kaldırılmıştır; yalnız tüm kapıları geçen `promote` işlemi yayına alabilir.
- Eşzamanlı eski editör işlemleri optimistic concurrency ile HTTP 409 alır.

## Sınır ve rollback

Bu çevrim bir üretim veya vision sağlayıcısı etkinleştirmez; ücret, lisans, secret ve dış veri
aktarımı owner kararı gerektirir. Şema migration'ı yoktur. Kod geri alınabilir; yeni audit kayıtları
silinmez ve v2 kanıtları v1 gibi yorumlanmaz. Geçici teslim kararı nedeniyle push ve deploy yapılmaz.
