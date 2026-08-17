# BOECL Çevrim 67 — Üye ilgi alanı aktivasyonu

## Görünür önce/sonra hedefi

Yeni üye daha önce kayıt sonrasında genel ana sayfaya dönüyor ve boş kişisel akışın nasıl
oluşacağını kendi başına keşfetmek zorunda kalıyordu. Bu faz kayıt sonrasında dört dilde,
gerçek yayınlı kök taxonomy verisini ve konuya ait gerçek yayın kapağını kullanan bir ilgi alanı
seçimi açar. Üye 1–5 konu seçerek kişisel akışını ilk oturumda oluşturabilir veya zorlanmadan
adımı geçebilir.

## Güvenlik, veri ve yayın bütünlüğü

- Seçimler istemcide serbest metin olarak güvenilmez; API locale, kök kategori, yayın varlığı ve
  1–5 sınırını yeniden doğrular.
- Toplu takip kurulumu oturum ve CSRF gerektirir, tek `SaveChanges` işlemiyle atomik kalır ve
  yalnız kullanıcı kimliği ile konu sayısını içeren append-only audit izi oluşturur.
- Var olan takipler korunur ve tekrar çalıştırma yinelenen ilişki üretmez; production içerik
  mutasyonu yoktur.
- Staging gerçek akış kapısı runtime rolünün üyelik ilişki tablolarında eksik yetkisini buldu.
  Geri alınabilir migration kaydetme, takip ve okuma ilerlemesi tablolarına gerekli en dar CRUD
  yetkisini verir; şema veya üye verisi silmez.
- Aynı kapı haftalık ritim sorgusundaki yerel saat ofseti dönüşümünü yakaladı. Haftanın başlangıcı
  artık PostgreSQL `timestamptz` sözleşmesine uygun açık UTC `DateTimeOffset` olarak hesaplanır.
- Onboarding ve hesap yüzeyleri açık içeriğin canonical/hreflang sözleşmesini değiştirmez.

## Deneyim ve erişilebilirlik

- Kartlar gerçek kategori adı, yayın sayısı, açıklama ve varsa gerçek yayın kapağını gösterir.
- Seçimler semantik düğme ve `aria-pressed`, canlı seçim sayacı, görünür focus ve en az 44 px
  eylem hedefleri kullanır.
- Merkezi yüzey/foreground/muted/accent/overlay/shadow tokenları açık ve koyu temada kullanılır;
  3→2→1 kolon düzeni masaüstünden telefona uyarlanır.
- Açık rıza banner’ı görünürken sabit onboarding eylemi banner’ın üstüne güvenli biçimde taşınır;
  iki kritik kontrol birbirinin pointer veya klavye erişimini engellemez.

## Kabul kapıları

- ESLint, TypeScript, 66 web testi ve dört locale tutarlılık kontrolü geçti.
- 150 API testi geçti; anonim onboarding isteği için 401 regresyonu eklendi.
- Next.js production build ve .NET Release build uyarısız geçti.
