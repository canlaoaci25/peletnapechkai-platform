# BOECL Çevrim 56 — Otonom hata kurtarma ve yeniden deneme güvenliği

## Görünür önce / sonra hedefi

Önceden canlı geliştirme ekranı çalışan ve çöktükten sonra `Running` durumunda kalmış bir
çevrimi ayıramıyor; ardışık hata ve güvenli yeniden deneme zamanını göstermiyordu. Artık
`/{locale}/admin/development` heartbeat sağlığını, ardışık hata sayısını, otomatik kurtarma
sayısını ve sonraki güvenli denemeyi dört etkin dilde, responsive bir operasyon kartında
gösterir.

## Otomasyon ve kurtarma bütünlüğü

- Codex alt süreci çalışırken durum kaydı 15 saniyede bir atomik heartbeat ile yenilenir.
- Önceki süreç çökerse terk edilmiş global mutex güvenli biçimde devralınır; 10 dakikadan
  eski veya eksik heartbeat kalıcı bir otomatik kurtarma olayı olarak sayılır.
- Başarısız çevrimler 1, 2, 4, 8, 16, 32 ve en fazla 60 dakikalık geri çekilme uygular.
  Başarılı çevrim hata sayacını ve beklemeyi sıfırlar.
- CLI başlat/durdur akışı kurtarma telemetrisini korur. Durum dosyaları geçici dosya ve
  atomik taşıma düzenini sürdürür; sır veya ham komut çıktısı yeni alana yazılmaz.

## Kabul ve kalan sınır

PowerShell regresyonu retry sınırlarını, deadline davranışını ve heartbeat eşiğini kapsar;
web kontratı dört locale ile risk/sağlık yüzeyini doğrular. Tam lint, typecheck, web build,
API test/build, staging ve production doğrulaması tamamlanmadan çevrim tamamlanmış değildir.
Harici alarm teslimi, iki kişi onaylı manuel rollback ve kademeli canary trafik sonraki
operasyon fazlarıdır.
