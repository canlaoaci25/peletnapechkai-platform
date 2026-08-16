# BOECL Tam Proje Denetimi

Tarih: 16 Ağustos 2026  
Kapsam: kaynak kod, bağımlılıklar, testler, üretim ve staging, Windows servisleri, zamanlanmış görevler, PostgreSQL verisi, medya, SEO uçları, otomasyon worker'ı ve yedekleme.

## Yönetici özeti

Sistem çalışır ve canlı yayın sağlıklıdır. Dört locale, API, web, veritabanı, medya dosyaları, sitemap ve temel erişilebilirlik kontrolleri geçti. Web bağımlılıklarındaki tek yüksek önem dereceli güvenlik açığı giderildi. Otomasyon worker'ını durduran PowerShell ayrıştırma hatası düzeltilerek canlı görev tekrar çalışır hale getirildi ve gerçek bir otomatik makale; üç çeviri ve dört locale SEO kaydıyla tamamlandı.

Denetim sonunda kritik seviyede açık bulgu yoktur. Buna karşılık güvenlik başlıklarının eksikliği, biçim standardının mevcut kaynakla uyumsuzluğu, içerik görsel/SEO borcu ve otomatik üretimin süre hedefi ile gerçek çalışma süresi arasındaki fark izlenmelidir.

## Uygulanan düzeltmeler

1. `Invoke-BoeclCodexWorker.ps1` içindeki Windows PowerShell tarafından ayrıştırılamayan `$batch:` ifadeleri `${batch}:` biçimine getirildi. Depodaki tüm Windows PowerShell betikleri parser ile doğrulandı.
2. Kurulu worker kopyası güncellendi; görev yeniden etkinleştirilip gerçek iş üzerinde doğrulandı.
3. Web bağımlılık taramasında bulunan `nanoid 3.3.17` yüksek önem dereceli advisory'si `3.3.18` sürümüne yükseltilerek kapatıldı. Son `npm audit` sonucu sıfır güvenlik açığıdır.
4. Web paketi staging ve production'a atomik olarak dağıtıldı; iki ortamın sağlık kapıları geçti.

Bu düzeltmeler `8b6dcc6` commit'iyle GitHub'a gönderildi.

## Doğrulama matrisi

| Kontrol | Sonuç | Kanıt/özet |
|---|---|---|
| Locale bütünlüğü | Başarılı | `tr-TR`, `en-US`, `de-DE`, `fr-FR` |
| Web lint | Başarılı | ESLint hata vermedi |
| Web typecheck | Başarılı | TypeScript hata vermedi |
| Next production build | Başarılı | 93 rota/sayfa üretildi |
| API testleri | Başarılı | 65/65 |
| .NET Release build | Başarılı | 0 hata, 0 uyarı |
| Web güvenlik açığı | Başarılı | `npm audit`: 0 |
| NuGet güvenlik açığı | Başarılı | Bilinen vulnerable paket yok |
| Staging sağlık | Başarılı | Uygulama ve locale kontrolleri geçti |
| Production sağlık | Başarılı | Dört locale ve CSRF ucu geçti |
| Public deneyim | Başarılı | Arama, skip-link/erişilebilirlik ve CSS dosyaları geçti |
| PowerShell parse | Başarılı | `ops/windows/*.ps1` tamamı parse ediliyor |
| Git nesne bütünlüğü | Başarılı | `git fsck` hata vermedi |
| Medya bütünlüğü | Başarılı | 359 kayıt, eksik fiziksel dosya yok |
| Sitemap | Başarılı | `sitemap.txt` HTTP 200, 680 URL |

## Otomatik içerik sistemi

- Otomatik üretim parametresi açıktır; denetim periyodu 3 dakikadır.
- Üç dakika, makalenin üç dakikada tamamlanacağı anlamına gelmez. Worker aynı anda tek Codex işi çalıştırır; araştırma, uzun içerik, görseller, üç çeviri, dört SEO seti ve kalite kapıları tamamlanmadan yeni iş açılmaz.
- İlk düzeltilmiş otomatik iş yaklaşık 59 dakikada tamamlandı: 1 Türkçe yayın, 3 yabancı dil yayını ve 4 locale SEO kaydı.
- Sonraki otomatik iş Yapay Zekâ / Rehber olarak kuyruğa alındı. Denetim sırasında devam eden sonraki iş de içerik, üç çeviri ve kalite kapılarını işledi; zamanlanmış görevin son tamamlanma kodu `0` oldu.
- Eski işlerin toplam durum görünümünde Completed 11, Failed 3, Cancelled 2 ve denetim anında Running 1 vardı; iki saatten eski takılı aktif iş yoktu.
- Son worker hata kaydı 15 Ağustos'taki eski 500 hatalarıdır. Düzeltilmiş akış 16 Ağustos'ta başarıyla sonuçlandı.

### Kapasite değerlendirmesi

Tek makalelik faz yaklaşımı kalite ve kurtarılabilirlik açısından doğrudur; ancak saatlik gerçek üretim kapasitesi 3 dakikalık ayardan değil Codex çalışma süresinden belirlenmektedir. Yaklaşık bir saatlik uçtan uca süreyle 50 kapsamlı makale kısa sürede tamamlanmaz. Paralellik eklemek; benzerlik, kaynak kullanımı, maliyet, veritabanı çakışması ve kalite riskleri nedeniyle ayrı tasarım gerektirir.

## Veri ve içerik bütünlüğü

Yayımdaki içerik sayıları:

| Locale | Published |
|---|---:|
| tr-TR | 177 |
| de-DE | 162 |
| en-US | 162 |
| fr-FR | 162 |

- Aynı grup-locale tekrarı: 0.
- Aynı locale-slug tekrarı: 0.
- Eksik fiziksel medya: 0/359.
- Yayında SEO alanı eksik iki Türkçe test içeriği var: `Merhaba emin test yapıyor` ve `Test emin. S`.
- Yayında kapaksız 208 kayıt var. Bu durum uygulamayı bozmaz, ancak kartların ve liste sayfalarının görsel tutarlılığını azaltır.
- Gövdesinde Markdown benzeri işaret taşıyan 245 yayın kaydı var. Public renderer eski kayıtları dönüştürüyor; buna rağmen veri katmanında HTML/BlockNote standardına toplu normalizasyon borcudur.
- Audit logunda 1.378 kayıt bulunuyor.
- Anime için yenilenen yerel görseller yazısızdır; kategori temalı olmalarına rağmen başlık düzeyinde anlamsal özgünlük garanti etmez. Bu, görsel kalite açısından orta seviyeli kalan risktir.

## Güvenlik incelemesi

Olumlu sonuçlar:

- HTTPS canlı ve sertifikanın yaklaşık 82 günü kaldı.
- `X-Content-Type-Options: nosniff` ve `Referrer-Policy: strict-origin-when-cross-origin` mevcut.
- `robots.txt` ve `ads.txt` HTTP 200 dönüyor.
- Depo desen taramasında gerçek parola, API anahtarı veya token bulunmadı.
- Web ve NuGet bilinen açık taramaları temiz.

Eksik koruyucu başlıklar:

- `Strict-Transport-Security` yok.
- `Content-Security-Policy` yok.
- `X-Frame-Options` veya CSP `frame-ancestors` yok.
- `Permissions-Policy` yok.

Bu eksikler doğrudan ihlal kanıtı değildir; tarayıcı taraflı saldırılara karşı savunma katmanını zayıflatır. Özellikle CSP; GA4, Clarity, AdSense ve medya kaynakları envanteri çıkarılarak önce Report-Only modunda devreye alınmalıdır. HSTS ancak tüm alt alanların HTTPS hazırlığı doğrulandıktan sonra etkinleştirilmelidir.

## Bağımlılık ve bakım durumu

- Web tarafında `next`/`eslint-config-next` 16.3.1 yaması ve BlockNote 0.54 gibi yeni sürümler vardır. TypeScript 7, ESLint 10 ve Node tipleri 26 ana sürüm yükseltmeleridir; otomatik yükseltilmemelidir.
- .NET tarafında 10.0.11 yama ailesi, HtmlSanitizer ve SkiaSharp yamaları bulunuyor. `Microsoft.OpenApi 3.x`, test SDK'sı ve coverlet gibi ana sürümler uyumluluk testi ister.
- `unrs-resolver@1.12.2` kurulum betiği npm `allowScripts` kapsamı dışında uyarı veriyor. Bir güvenlik açığı raporu değildir; tedarik zinciri politikası açıkça kararlaştırılmalıdır.
- `dotnet format --verify-no-changes` 4.678 biçim/satır-sonu bulgusu verdi. Derleme ve testler geçmektedir; sorun ağırlıkla eski sıkıştırılmış C# biçimi ve CRLF/LF tutarsızlığıdır. Tek committe otomatik düzeltme çok geniş diff oluşturacağından ayrı bir format normalizasyon fazı önerilir.
- TODO/FIXME/HACK/XXX taraması sıfır sonuç verdi.

## Operasyon, görevler ve yedekler

- IIS (`W3SVC`), PostgreSQL 18, production web ve staging web servisleri çalışıyor ve otomatik başlangıçta.
- Staging health, saatlik sitemap, haftalık kalite denetimi ve PostgreSQL backup görevlerinin son sonuç kodu `0`.
- Otomasyon görevi başarılı iş sonunda `0` döndürdü. Görev çalışırken görülen `267009` benzeri değerler nihai hata değil Task Scheduler'ın “halen çalışıyor” durumudur.
- Günlük PostgreSQL yedeği başarılı; son doğrulanan yedek 15 Ağustos 02:15 tarihli ve yaklaşık 1,28 MB.
- Yedek adındaki `peletnapechkai_dev` ifadesi production veritabanı için operasyonel olarak yanıltıcıdır; veritabanı adı veya yedek etiketi netleştirilmelidir.
- SFTP/off-site yedek kullanıcı kararıyla ertelendi. Bu nedenle sunucu veya disk kaybında aynı makinedeki yedeklerin birlikte kaybolması halen önemli bir süreklilik riskidir.
- Diskte yaklaşık 161,4 GB boş alan var.

## Önceliklendirilmiş kalan işler

### Yüksek

1. CSP'yi Report-Only ile tasarlayıp GA4, Clarity, AdSense ve uygulama kaynaklarıyla doğrulamak; ardından enforce etmek.
2. HSTS ve clickjacking korumasını IIS/uygulama katmanında devreye almak.
3. Sunucu dışı yedek hedefi belirlemek ve düzenli geri yükleme tatbikatını off-site kopyayla doğrulamak.

### Orta

1. 208 kapaksız yayının öncelikli olanlarına konuya özgü, yazısız görseller üretmek.
2. 245 eski Markdown gövdesini önizlemeli ve geri alınabilir bir migration ile normalize etmek.
3. İki test yayınının SEO alanlarını tamamlamak veya editoryal olarak yayından kaldırılıp kaldırılmayacağına karar vermek.
4. Otomatik üretim için ortalama/p95 faz süreleri, başarısızlık oranı ve günlük üretim kapasitesi göstergeleri eklemek.
5. Görsellerde başlıkla anlamsal eşleşmeyi ölçen insan örneklemesi veya görsel-metin benzerlik kapısı eklemek.
6. C# format normalizasyonunu tek amaçlı bir committe yapmak ve CI'a `dotnet format --verify-no-changes` eklemek.

### Düşük / planlı bakım

1. Yama bağımlılıklarını küçük gruplarla yükseltip tam kalite matrisini çalıştırmak.
2. Ana sürüm yükseltmelerini ayrı dallarda değerlendirmek.
3. Yedek dosya/veritabanı adındaki `_dev` belirsizliğini gidermek.
4. `unrs-resolver` install-script politikasını açık allow/deny kararıyla belgelemek.

## Sonuç

BOECL canlıda işlevsel ve temel kalite kapıları yeşildir. Denetimde üretimi durduran worker ayrıştırma hatası ve yüksek önem dereceli npm açığı giderilmiştir. En önemli açık teknik çalışma hatası değil, savunma başlıkları ile içerik/görsel bakım borcudur. Sistem “iş kalmadı” seviyesinde değildir; fakat kalan işler sınıflandırılmış, ölçülmüş ve güvenli biçimde sıraya alınabilir durumdadır.
