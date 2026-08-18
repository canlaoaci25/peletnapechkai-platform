# Çevrim 103 — Core Web Vitals saha bütçesi

## Görünür sonuç

Admin trafik merkezi artık ölçüm izni veren ziyaretçilerden gelen LCP, CLS ve INP saha örneklerini
locale, sabit yayın şablonu ve viewport kohortunda gösterir. Mobil satırlarda p75 değer, bütçe,
örneklem ve `Bütçe içinde`, `Bütçe dışında` veya `Yeterli örnek yok` kararı birlikte görünür.

## Güvenlik, gizlilik ve veri yaşam döngüsü

- Ölçüm yalnız `boecl-consent=granted` sonrasında gönderilir.
- Tam URL, slug, sorgu, kullanıcı, cihaz veya IP kimliği veritabanına yazılmaz.
- Locale, route, viewport, metric ve değer allowlist/sınır denetiminden geçer; public endpoint rate-limitlidir.
- Ham örnekler 90 gün sonra günlük worker ile silinir; silme sayısı yapılandırılmış loga yazılır.
- Migration geri alınabilir ve runtime role yalnız SELECT/INSERT/DELETE tablo yetkisi verir.

## Yerel kanıt

- Web test: 93/93 PASS.
- Web lint: PASS.
- Web typecheck: PASS.
- Web production build: 110 statik sayfa ve `/api/web-vitals` rotasıyla PASS.
- API Release test: 195/195 PASS.
- API Release build: 0 warning, 0 error.

## Açık canlı kabul kapıları

Bu worktree production veya staging hedefini değiştirmez. Runner önce staging backup/restore ve
migration uygulamasını, sonra açık/koyu temada 390/1440 admin renderını ve gerçek izinli beacon
smoke testini kanıtlamalıdır. Yeterli mobil örnek olmadan bütçe `PASS` sayılmaz. Üretim terfisinden
sonra veri alımı, retention logu ve rollback doğrulanmadan birleşik workstream tamamlanmış değildir.
