# BOECL Çevrim 32 — Dağıtım Geçmişi ve Kurtarma Kanıtı

Tarih: 16 Ağustos 2026  
Odak: otomasyon, hata kurtarma ve canlı dağıtım güvenilirliği

## Görünür önce / sonra hedefi

Önceden `/{locale}/admin` içindeki Sürüm Kurtarma Merkezi yalnız staging ve production
Web/API bileşenlerinin son durumunu gösteriyor, eski bir hatanın otomatik geri alınıp
alınmadığını veya sürüm güvenilirliği eğilimini kanıtlayamıyordu. Artık yönetici son 12
dağıtımı zaman çizelgesinde görür; başarılı, otomatik kurtarılmış ve müdahale isteyen
sonuçları ayrı sayaçlarla izler ve her kayıtta ortam, bileşen, commit, sonuç ve zamanı
karşılaştırır. Yüzey dört etkin locale için çevrilmiş, masaüstü ve mobil düzenlidir.

## Otomasyon ve hata kurtarma bütünlüğü

- Her dağıtım durumu hem `latest-{environment}-{component}.json` kontrol noktasına hem
  aynı dağıtım kimliğinin kalıcı geçmiş kaydına atomik olarak yazılır.
- Başlangıç, doğrulama, başarı ve rollback aşamaları aynı geçmiş kaydını ilerletir; yarım
  kalan sürüm görünür kalır ve sessiz başarı sayılmaz.
- Mesaj sanitizasyonu geçmiş kayıtlarında da uygulanır; deployment kimliği dosya yolu
  enjeksiyonuna karşı kısıtlanır.
- API yalnız izin verilen ortam/bileşen ve geçerli şema/tarih içeren, 64 KiB altındaki
  kayıtları okur; bozuk günlük admin yanıtını düşürmez.
- Bu faz mevcut otomatik rollback mekanizmasını değiştirmez ve üretim verisine dokunmaz.

## Kabul kanıtı

- PowerShell regresyonu atomik son durum + geçmiş yazımını ve secret sanitizasyonunu kapsar.
- API regresyonu bozuk/aşırı büyük girdiyi reddetmeyi, sıralamayı ve 12 kayıt sınırını kapsar.
- Zorunlu lint, typecheck, web build, API test ve Release build kapıları çalıştırılır.
- Staging ve production Web/API dağıtımları sonrası canlı URL ve admin veri sözleşmesi
  doğrulanmadan çevrim tamamlanmış sayılmaz.

## Kalan operasyon kilometre taşları

1. Süre/başarı SLO trendleri ve uyarı eşikleri.
2. İki kişi onaylı, yetkili ve audit izli elle rollback.
3. Saklama süresi ile eski release klasörü/günlük arşivleme politikası.
4. Web ve API için kademeli/canary trafik doğrulaması.
