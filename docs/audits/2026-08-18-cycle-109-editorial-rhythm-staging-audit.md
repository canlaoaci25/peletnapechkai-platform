# Çevrim 109 — editoryal ritim staging terfisi

## Director kararı

FULLSTACK, dış sağlayıcı veya yeni owner kararı gerektirmeyen en güçlü kapanış adımı olarak
`editorial-delivery-rhythm` terfisini önerdi. DESIGNER düzeltme akışının gerçek içerikle görsel
kabulünü, EDITOR ise structured discovery canlı sözleşmesini daha yüksek görünür değerli buldu.
SYSADMIN production sağlık kapısını denetledi. Director, staging envanterinde yayımlanmış makale
olmaması nedeniyle iki public adayın gerçek içerikle kanıtlanamayacağını; editoryal ritmin ise az
örnek durumunu dürüstçe sunabildiğini dikkate alarak staging terfisini seçti.

Karar: **PASS — staging checkpoint / HOLD — production**.

## Yerel kalite kanıtı

- Dört locale sözlük ve placeholder kontrolü geçti.
- 95 web testi, lint ve TypeScript kontrolü geçti.
- Next.js production build 110 rotayı üretti.
- 196 API testi ve .NET Release build sıfır uyarıyla geçti.
- PowerShell parse, otomasyon kurtarma, deployment journal, web release budget, release promotion
  ve database promotion regresyonları geçti.
- İlk denemede worktree bağımlılıkları ve NuGet assets dosyası yoktu. Bunlar uygulama hatası olarak
  gizlenmedi; mevcut lockfile ile web kurulumu ve `dotnet restore` sonrasında kapılar yeniden koşuldu.

## Staging veri ve yayın kanıtı

- Staging PostgreSQL yedeği alındı ve SHA-256 yan dosyası üretildi.
- Aynı yedek exact path ile izole veritabanına geri yüklendi: 49 migration ve dört locale doğrulandı.
- EF migration kapısı staging veritabanının güncel olduğunu bildirdi; yeni migration uygulanmadı.
- API ve Web atomik staging release dizinlerine terfi etti; iki bileşenin kendi rollback yolu korundu.
- Staging sağlık kapısı PASS verdi.
- `/tr-TR/admin` anonim isteği locale-correct `/tr-TR/admin/login` rotasına 307 döndü.
- Kaynak merkezi, yasal sayfa ve sitemap 200 döndü; staging yanıtları `noindex, nofollow, noarchive`
  sözleşmesini korudu.

## Açık kabul ve production engeli

Yetkili admin oturumu sağlanmadığı için üretim ritmi kartlarının gerçek 0/1–2/3+ örnek durumları,
klavye akışı ve açık/koyu ekran görüntüleri henüz kanıtlanmadı. Bu nedenle workstream `completed`
yapılmadı.

Production C: boş alanı yaklaşık 17,2 GB; sağlık kapısı en az 20 GB ister. Kalite eşiği düşürülmedi,
retention kararı olmadan rollback/deploy artefaktı silinmedi ve production veritabanı ya da uygulaması
mutasyona uğratılmadı. Terfi, disk kapasitesi güvenli biçimde düzeltildikten ve production preflight
PASS verdikten sonra aynı backup/restore/migration/health sırasıyla devam etmelidir.

## Rollback

Staging API ve Web deploy betikleri önceki aktif dizinleri ayrı rollback yollarında korur. Bu çevrimde
production rollback gerektirecek bir işlem yapılmadı. `CompletedAt` kolonu yeni canlı kanıt yazıldıktan
sonra uygulama rollback'inde korunmalıdır; production'da kolon düşürmek güvenli geri dönüş değildir.
