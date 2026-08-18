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

Staging backup/restore/migration, gerçek 375/768/1440 açık-koyu render ve production disk sağlık
kapısının düzelmesi beklenir. Bu nedenle birleşik faz ve `visual-focal-crops` henüz Completed değildir.
