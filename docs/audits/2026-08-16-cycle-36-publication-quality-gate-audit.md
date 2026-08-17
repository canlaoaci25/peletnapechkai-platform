# BOECL Çevrim 36 — Yayın kalite kapısı

## Görünür önce/sonra hedefi

Önceden sekiz maddelik yayın checklist'i API'de saklanıyor ancak makale çalışma ekranında
gösterilmiyor ve eksik kontroller doğrudan yayınlama, normal yayınlama veya planlama
eylemlerini engellemiyordu. Bu faz makale ayrıntısını hazırlık ilerlemesi, görevler,
yorumlar ve tüm kalite kontrollerinin birlikte yönetildiği bir yayın kokpitine dönüştürür.
Editör tamamlanan kontrol sayısını görür; 8/8 olmadan yayın ve planlama eylemleri hem
arayüzde hem API'de kapalıdır.

## Bütünlük ve güvenlik

- Zorunluluk istemciye güvenmez. Normal yayın, doğrudan yayın ve planlama uçları aynı
  sunucu tarafı kalite kapısını çalıştırır ve eksik kontrol anahtarlarını güvenli bir
  `409 Conflict` yanıtıyla bildirir.
- Checklist bulunmaması sekiz kontrolün de eksik sayılmasıdır; eski içerik yanlışlıkla
  uygun kabul edilmez.
- Checklist değişiklikleri aktör, tamamlanma durumu ve eksik kontrollerle audit izine
  yazılır. Yazma uçları mevcut rol ve antiforgery korumasını korur.
- Şema veya production verisi değişmez; migration ve veri yedeği gerekmemiştir.
- Dört locale için hazırlık metni vardır; semantik durum alanı, görünür ilerleme ve
  disabled eylemler klavye/mobil kullanımda aynı davranır.

## Kalıcı backlog

1. Yayın kuyruğuna toplu kalite özeti ve filtre ekleme.
2. Kontrol maddelerini içerik verisinden otomatik kanıtlarla destekleme.
3. Editör yükü ve yeniden atama görünümü.
4. Tamamlanma süresi ile p50/p95 editoryal SLA trendleri.
5. Locale bazlı çeviri/SEO sahipliği ve SLA.
