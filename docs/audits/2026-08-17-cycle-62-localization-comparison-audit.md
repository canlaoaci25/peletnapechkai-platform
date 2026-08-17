# BOECL Çevrim 62 — Yerelleştirme karşılaştırma çalışma alanı

## Görünür önce/sonra hedefi

Uluslararası Yayın Sağlığı kuyruğu daha önce değişen kaynak alanlarının yalnız adını gösteriyordu.
Editör, güncel Türkçe kaynak ile hedef çeviriyi aynı bağlamda okuyamıyor ve ilgili çeviri kaydına
doğrudan geçemiyordu. Bu çevrim sonunda her kuyruk kartı, dört yönetim dilinde yerelleştirilmiş
bir karşılaştırma çalışma alanına açılır. Başlık, özet, gövde ve SEO alanları masaüstünde yan yana,
mobilde tek kolon gösterilir; değişen alanlar vurgulanır ve mevcut hedef kayıt doğrudan editöre bağlanır.

## Güvenlik ve editoryal sınır

- Ayrıntı endpoint'i yalnız etkin, varsayılan olmayan hedef locale ile aynı article group içindeki
  yayımlanmış Türkçe kaynağı kabul eder; arşivlenmiş hedefleri göstermez.
- Çalışma alanı salt okunurdur. İçeriği değiştirmez, snapshot yenilemez, taslağı veya yayını otomatik
  yayımlamaz ve mevcut kalite kapılarını atlamaz.
- CMS gövdesi çalıştırılabilir HTML olarak render edilmez; güvenilmeyen içerik metin olarak gösterilir.
- Yeni şema, migration veya veri operasyonu yoktur.

## Kabul kanıtı

- Dört locale için doğal çalışma alanı kopyası ve kuyruktan görünür karşılaştırma eylemi eklendi.
- 800 px altında tek kolon düzeni; geniş ekranda kaynak/hedef iki kolon hiyerarşisi tanımlandı.
- Locale sözleşmesi 4/4, 64 web testi, lint, typecheck, production web build, 145 API testi ve
  Release API build geçti.
- Staging ve production dağıtımı ile yetkili gerçek render doğrulaması tamamlanmadan çevrim operasyonel
  olarak tamamlanmış sayılmaz.

## Kalıcı backlog

1. Yetkili yeniden çeviri tesliminde source snapshot ve assignment durumunu aynı transaction içinde yenileme.
2. Güvenli satır içi metin farkı ve önceki kaynak revizyonunu kalıcı karşılaştırma.
3. Kültürel uyarlama ve doğal alt metin için locale görsel inceleme kuyruğu.
4. Locale bazlı Search Console niyet, sıfır sonuç ve içerik boşluğu ölçümü.
