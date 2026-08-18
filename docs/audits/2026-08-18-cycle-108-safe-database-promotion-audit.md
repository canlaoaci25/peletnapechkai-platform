# Cycle 108 — Güvenli veritabanı terfisi denetimi

## Amaç ve görünür hedef

`visual-focal-crops` dahil migration bekleyen görünür ürün dilimlerini staging üzerinden güvenle
terfi ettirmek. Rutin release komutu artık veritabanı değişimini yedek ve geri yükleme kanıtı olmadan
başlatamaz; production sağlıksızsa production verisi veya uygulaması değiştirilmeden durur.

## Değişiklik

- Staging veritabanı ortam adıyla yedeklenir, üretilen tam yedek yolu izole veritabanında geri
  yüklenir, ardından migration ve API/Web staging kohortu çalışır.
- Staging sağlık kapısından sonra production için salt-okunur sağlık ön kontrolü çalışır.
- Production sağlıklıysa aynı yedek, tam-yol restore testi ve migration sırası production için de
  uygulanır; Web ve API ortak cohort rollback sözleşmesini korur.
- Regresyon testi sıra ve exact-backup sözleşmesini fail-closed doğrular.

## Uzman kapısı

- **FULLSTACK — PASS:** additive focal migration ve merkez fallback yerelde hazır; canlı migration/render gerekir.
- **DESIGNER — PASS / canlı kanıt gerekli:** 375, 768 ve 1440 px açık/koyu admin ve public crop matrisi gerekir.
- **EDITOR — PASS_WITH_SCOPE:** yazısızlık, locale ve bölüm uygunluğu aynı aday kanıtına bağlı kalmalıdır.
- **SYSADMIN — REJECT (production):** production uçları yanıt verse de disk boşluğu 19,28 GB ile 20 GB sağlık
  eşiğinin altındadır. Açık retention kararı olmadan eski rollback artefaktları silinmedi.
- **DIRECTOR — HOLD:** kod kalite kapıları PASS; staging kanıtı üretilebilir, production mutasyonu preflight
  düzelene kadar yasaktır.

## Test ve rollback

Locale 4/4, 95 web testi, lint, typecheck, 110 rotalık production build, 196 API testi, uyarısız
Release build ve tam otomasyon regresyon paketi PASS. Kod rollback'i bu commit'in geri alınmasıdır.
Yedekler retention politikasına göre korunur; başarısız preflight hiçbir production migration veya deploy
başlatmaz.

## Açık kanıt

İlk gerçek staging terfi denemesi yedeği izole geri yükledi ve kalıcı veri tabanında yalnız üç locale
bulunduğunu kanıtladı; migration ve deploy başlamadan durdu. Kök neden başlangıç seed'inde `fr-FR` ve
Fransa bölgesinin hiç bulunmamasıydı. Ayrı, idempotent ve planlama taxonomy migration'ından önce
çalışan locale parity migration'ı eklendi; onarım 4/4 assertion geçmezse yine durur.

İkinci terfi denemesinde eski yedek 3 locale ile eksiksiz geri yüklendi; parity migration önce
çalıştı, ardından focal crop ve kalan migration'lar uygulandı. Staging API/Web dağıtımı ve sağlık
kapısı PASS oldu. Migration sonrası alınan yeni staging yedeği izole veritabanında **49 migration ve
4 locale** ile geri yüklendi.

Staging public ana sayfa 375, 768 ve 1440 px açık/koyu headless browser görüntülerinde sidebar/drawer,
tema ve taşma açısından PASS verdi; kanıtlar `C:\ProgramData\Peletnapechkai\Evidence\cycle-108`
altındadır. Staging yayın envanteri Türkçe içerik döndürmediği ve seçilen article URL'si 404 olduğu
için gerçek merkez-dışı focal crop public kanıtı üretilemedi. Admin kanıtı da yetkili oturum olmadan
üretilmedi. Production disk sağlık kapısının düzelmesi ve bu iki gerçek render kanıtı beklenir. Bu
nedenle birleşik faz ve `visual-focal-crops` henüz Completed değildir.
