# BOECL Çevrim 31 — Görsel Yenileme Stüdyosu

Tarih: 16 Ağustos 2026  
Odak: makale görsellerinin konu uygunluğu ve yazısız özgün tasarımı

## Görünür önce / sonra hedefi

Önceden yönetici riskli kapakları görebiliyor ancak bu tespitler kalıcı bir işe dönüşmüyor,
makalenin ilgili bölüm bağlamı görünmiyor ve editoryal karar kaydedilemiyordu. Artık
`/{locale}/admin/automation/visual-quality` yüzeyi riskli yayımlanmış makaleleri idempotent
bir kuyruğa alır; özet, tam gövde, ilk H2/H3, taxonomy ve locale/ülke bağlamından türetilen
yazısız 16:9 özgün görsel briefini gösterir. Yönetici briefi incelemeye alır, gerekçeli
onay/red verir veya yeni deneme ister.

## Güvenlik ve veri bütünlüğü

- Kuyruk ve durum işlemleri Owner/Admin yetkisi ve antiforgery koruması altındadır.
- Aynı makale ve aynı risk kümesi unique idempotency anahtarıyla tekrar eklenmez.
- Kararlar append-only audit kaydı üretir; retry sayacı ve karar notu kalıcıdır.
- Bu faz doğrulanmamış bir sağlayıcı çıktısını yayımlamaz ve mevcut kapağı değiştirmez.
- Migration geri alınabilir; makale ve mevcut medya ilişkileri korunur.

## Kabul kanıtı

- Yeni domain kuralları ve full-context brief üretimi birim testleriyle kapsandı.
- Lint, typecheck, web build, API test ve Release build kapıları çalıştırıldı.
- Responsive stüdyo; iki kolonlu masaüstü, tek kolonlu tablet/mobil ve 400 px altı özet
  düzenlerini kapsar; light/dark admin tokenlarını kullanır.

## Açık servis kilometre taşları

1. Lisans metadata sözleşmeli resmî/stock/diagram/AI sağlayıcı adaptörleri.
2. Vision konu-yazı-artefact puanı ve perceptual hash/embedding tekrar kapısı.
3. Yeni varlık için önce/sonra önizleme, optimize AVIF/WebP varyantları ve mobil crop.
4. Onaylanan varlığın transaction ile yayıma alınması ve tek tık rollback.
5. Staging onayından sonra checkpointli tüm arşiv yeniden görselleştirme worker'ı.
