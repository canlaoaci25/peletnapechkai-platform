# BOECL posta sunucusu

## Mevcut durum

- Stalwart Mail Server 0.16.17, Windows hizmeti `Stalwart` olarak otomatik başlar.
- Başlangıç veri deposu `C:\Program Files\Stalwart\data` altındaki RocksDB'dir.
- `peletnapechkai.com` yerel posta alanı ve `noreply@peletnapechkai.com` uygulama hesabı tanımlıdır.
- Yönetim, SMTP ve posta istemcisi dinleyicileri yalnız `127.0.0.1` adresine bağlıdır.
- Yönetim arayüzü yalnız sunucudan `http://127.0.0.1:8080/admin` adresindedir.
- Yönetici ve uygulama parolaları Git deposunda veya bu belgede tutulmaz; Windows korumalı kimlik bilgisi deposundadır.

## Yerel dinleyiciler

| Protokol | Adres |
|---|---|
| SMTP | `127.0.0.1:25` |
| SMTPS | `127.0.0.1:465` |
| IMAPS | `127.0.0.1:993` |
| POP3S | `127.0.0.1:995` |
| ManageSieve | `127.0.0.1:4190` |
| HTTP yönetim/JMAP | `127.0.0.1:8080` |
| HTTPS yönetim/JMAP | `127.0.0.1:8443` |

## Doğrulama

```powershell
Get-Service Stalwart
Get-NetTCPConnection -State Listen |
  Where-Object OwningProcess -eq (Get-Process stalwart).Id |
  Sort-Object LocalPort
```

Yerel SMTP kabul testi `noreply@peletnapechkai.com` adresinden yerel yönetici kutusuna başarıyla kuyruklandı. İnternet teslimatı henüz etkin değildir.

## İnternet postasına geçiş kapısı

Genel erişim açılmadan önce aşağıdakilerin tümü tamamlanmalıdır:

1. `mail.peletnapechkai.com` A kaydını bu sunucunun genel IP adresine taşı.
2. Alanın MX kaydını yeni posta adına taşı.
3. Genel IP için PTR/rDNS kaydını aynı posta adına ayarlat.
4. SPF kaydını yeni gönderici IP'siyle güncelle.
5. Stalwart'ın ürettiği DKIM kayıtlarını ve bir DMARC politikasını DNS'e ekle.
6. Geçerli TLS sertifikasını al ve dış SMTP/IMAP dinleyicilerini kontrollü aç.
7. Spam, açık relay ve harici teslimat testlerini tamamla.

Bu kapı tamamlanmadan dinleyiciler genel arayüze bağlanmamalıdır.
