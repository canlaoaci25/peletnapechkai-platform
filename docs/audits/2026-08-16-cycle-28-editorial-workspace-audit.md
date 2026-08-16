# BOECL Çevrim 28 — Editoryal çalışma masası

## Görünür önce/sonra hedefi

Önceden admin ana sayfasındaki komuta kuyruğu ekip genelindeki ilk 16 kaydı tek listede
gösteriyor; bir editör kendi görevlerini ayıramıyor ve işi ilerletmek için makale ayrıntısına
gitmek zorunda kalıyordu. Bu faz kontrol merkezini kişisel ve ekip kapsamları, gecikme/48 saat
SLA göstergeleri, operasyon filtreleri ve satır içi görev durumu yönetimi olan günlük bir
çalışma masasına dönüştürür. Yüzey dört locale'de, açık/koyu temada ve mobil/masaüstünde aynı
iş akışını korur.

## Bütünlük ve güvenlik

- Kişisel kapsam oturumdaki ASP.NET Identity kullanıcısından sunucuda hesaplanır; istemci
  kullanıcı kimliği göndermez.
- Görev durumunu yalnız görev sahibi veya Owner/Admin/Editor değiştirebilir. Yazma isteği
  antiforgery korumalıdır ve eski/yeni durum ile makale kimliğini içeren audit olayı üretir.
- Taslak/published içerik görünürlüğü değişmez; admin rotaları indeks dışıdır. Şema veya
  production verisi değiştiren migration gerekmez.
- Kuyruk kişisel işleri önce sıralar; ekip görünümünde risk puanı ve son tarih sırası korunur.

## Sonraki kalıcı backlog

1. Yayın eylemlerinde eksik kalite kapılarını sunucu tarafında zorunlu kılma.
2. Editör başına açık iş yükü ve yeniden atama görünümü.
3. Tamamlanma süresi, gecikme oranı ve p50/p95 editoryal SLA trendleri.
4. Translation/SEO incelemeleri için açık sahiplik ve locale bazlı SLA.
5. Kuyrukta toplu atama; yetki, audit ve geri alma korumasıyla.
