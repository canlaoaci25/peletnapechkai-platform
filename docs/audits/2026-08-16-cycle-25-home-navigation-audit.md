# BOECL Çevrim 25 — Ana sayfa ve global navigasyon

## Ziyaretçi sonucu

Ana sayfa, BOECL'in yayın kimliğini marka kilidi içinde açıklayan; gündem, konu, arama,
hesap, dil ve tema yollarını tutarlı katmanlara ayıran yeni bir global başlığa kavuştu.
Manşet alanı masaüstünde konuya özel kapak, ana hikâye ve dört ikincil başlığı tek editoryal
kompozisyonda sunuyor; mobilde aynı anlam sırası tek kolona dönüşüyor.

## Kanıt ve kararlar

- Önceki canlı başlıkta dil ve kategori yolları vardı ancak yayın vaadi görünmüyor, tema
  kontrolü içerikten kopuk biçimde ekran köşesinde yüzüyor ve mobil yardımcılar kayboluyordu.
- BBC, The Verge ve Wired ana sayfalarındaki marka, bölüm, gündem ve yardımcı eylem
  katmanları desen olarak incelendi; görsel veya metinsel tasarım kopyalanmadı.
- W3C WAI, tipik web navigasyonunda karmaşık `menubar` rolü yerine semantik bağlantılar ve
  disclosure desenini öneriyor. Mevcut yerel `details` temeli korunarak bilgi mimarisi ve
  görünür durumlar güçlendirildi.
- İlk 390 px render denetimi bir CSS öncelik hatasıyla yatay taşmayı gösterdi. Tek kolon
  kırılımı son katmanda yeniden tanımlanarak deploy öncesinde giderildi.

## Kapsam

- Dört locale için yayın vaadi, menü açıklaması, gündem rotası ve manşet yardımcı metinleri.
- Header içine alınan tema kontrolü; semantik arama ikonu; dokunmatik ve klavye hedefleri.
- Yeniden oranlanan LCP manşet görseli, okunabilir ana başlık, belirgin okuma bağlantısı ve
  sayılı ikincil haber masası.
- 320, 375, 390, 768, 1024 ve 1440 px gerçek Chromium render kapısı.

## Sonraki yüksek değerli faz

Global başlık artık güçlü bir temel sağlıyor. Sonraki görünür faz, kategori arşivlerini aynı
editoryal hiyerarşiye taşımak ve category authority hub yapısını gerçek içerik yoğunluğu,
SEO açıklaması ve konu ilişkileriyle tamamlamaktır.
