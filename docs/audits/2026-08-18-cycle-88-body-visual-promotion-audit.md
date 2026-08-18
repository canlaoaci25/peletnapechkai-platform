# Çevrim 88 — bölüm görseli terfi denetimi

## Görünür sonuç

Görsel Yenileme Stüdyosu artık uzun bir yayında tespit ettiği eksik gövde görselini yalnız plan olarak göstermiyor. Seçilen H2/H3 için ayrı, kalıcı ve idempotent inceleme görevi kuruyor. Bütün konu, bölüm, locale, teknik doğruluk, yazısızlık, artefact, crop, lisans ve özgünlük kapıları geçen aday; başlığın hemen altında erişilebilir bir `figure` olarak yayımlanıyor.

## Güvenlik ve kurtarma

- Bölüm başlığı brief oluşturulduktan sonra değiştiyse terfi fail-closed durur.
- Aynı bölüme ikinci görsel yazılmaz.
- Medya ölçüleri olmayan aday yerleştirilmez.
- Makale gövdesi, görev checkpoint'i ve audit kaydı tek veritabanı transaction'ında güncellenir.
- Migration geri alınabilir; `Down` yalnız eklenen hedef ve bölüm başlığı kolonlarını kaldırır.
- Harici veya ücretli sağlayıcı etkinleştirilmedi; secret ya da dış veri aktarımı eklenmedi.

## Kanıt

- Hedefli domain/API testleri: 24/24 geçti.
- Tam API test paketi: 175/175 geçti.
- `npm run lint`: geçti.
- `npm run typecheck`: geçti.
- `npm run build:web`: geçti; 109 statik rota üretildi.
- `dotnet build Peletnapechkai.slnx --configuration Release --no-restore`: 0 uyarı, 0 hata.

## Director kalite kararı

**PASS (yerel ürün dilimi).** Bölüm hedefli görev ve public terfi davranışı testlerle kanıtlandı. Staging/production deploy bu çevrimde yapılmadı; geçici teslim kararı GitHub işlemlerini yasaklıyor, ayrıca canlı deployment kanıtı olmadan visual-service epic'i tamamlanmış sayılmadı.

## Açık riskler

- Sağlayıcı-neutral worker için atomik lease, kalıcı attempt, backoff ve dead-letter sözleşmesi henüz yok; provider otomasyonu kapalı kalmalı.
- Public kart kırpmaları kalıcı odak koordinatı taşımıyor; editör crop onayı bütün yüzeylerde veriyle uygulanabilir değil.
- Gövde yerleşimi HTML içinde transaction ile korunuyor ancak bağımsız revision/rollback varlığı sonraki veri modeli diliminde tamamlanmalı.
