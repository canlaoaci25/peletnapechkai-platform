# BOECL Çevrim 54 — Çeviri kaynak bütünlüğü

## Görünür önce/sonra hedefi

Uluslararası Yayın Sağlığı kuyruğu daha önce bir Türkçe kaynak güncellendiğinde yalnız
“güncel değil” diyordu; editör çevirinin hangi kaynak sürümüne dayandığını ve hangi alanların
değiştiğini göremiyordu. Artık yeni çeviriler kaynak başlık, özet, gövde ve SEO alanlarının
SHA-256 parmak izlerini kaynak sürüm tarihiyle birlikte saklar. Kuyruk kaynak kanıtı tarihini
ve değişen alanları dört yönetim dilinde gösterir. Snapshot öncesi eski çeviriler için geçmiş
uydurulmaz; bunlar açıkça “kaynak sürümü kayıtsız” borcu olarak görünür.

## Veri, güvenlik ve operasyon sınırı

- Snapshot yalnız aynı article group içindeki varsayılan-locale kaynak ile hedef çeviri arasında
  alınabilir; farklı grup veya varsayılan-locale hedef domain tarafından reddedilir.
- Ham kaynak içerik ikinci kez saklanmaz. Sabit uzunluklu alan parmak izleri ve kaynak `UpdatedAt`
  kanıtı tutulur; karşılaştırma sunucuda yapılır.
- Otomatik çeviri teslimi snapshot’ı yayından önce aynı transaction içinde kaydeder. Bu değişiklik
  tek başına içerik oluşturmaz, eski içeriği değiştirmez veya taslağı yayımlamaz.
- Migration beş nullable kolon ekler. Veri backfill yapmaz; `Down` yalnız bu kolonları kaldırır.
  Staging ve production öncesinde ayrı doğrulanmış yedek zorunludur.

## Kabul kanıtı

- Dört locale arayüz kopyası kaynak kanıtı, kayıtsız eski sürüm ve alan farklarını kapsar.
- Domain testleri yalnız değişen başlık/gövdeyi raporladığını ve ilgisiz snapshot’ı reddettiğini doğrular.
- Locale sözleşmesi, lint, typecheck, 59 web testi, 140 API testi, production web build ve Release
  API build geçti.
- Protected admin yüzeyi staging ve production dağıtımından sonra yetkili oturumla masaüstü/mobil
  örneklemde doğrulanmadan çevrim tamamlanmış sayılmaz.

## Kalıcı backlog

1. Yetkili editörün yeniden çeviri tesliminde snapshot’ı atomik yenilemesi ve assignment kapatması.
2. Kaynak revision ile çevrilmiş revision arasında güvenli, satır içi metin karşılaştırma ekranı.
3. Kültürel uyarlama ve doğal alt metin için locale görsel inceleme kuyruğu.
4. Locale bazlı Search Console niyet, sıfır sonuç ve içerik boşluğu ölçümü.
5. Admin tag düzenleyicisinde yetkili manuel source-tag eşleştirme akışı.
