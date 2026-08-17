# BOECL Çevrim 51 — Üye okuma ritmi

## Görünür önce/sonra hedefi

Üye hesabı daha önce yarım okumaları, kişisel akışı, kayıtları ve takipleri ayrı raflarda
sunuyor; tamamlanan okumayı veya haftalık geri dönüş niyetini görünür kılmıyordu. Hesabın
yeni açılış yüzeyi artık kullanıcının seçtiği 1, 3 veya 5 yazılık haftalık hedefi, tamamlanan
yazı ve aktif gün sayısını, erişilebilir ilerleme göstergesini ve kayıt/takiplerinden henüz
bitirmediği sıradaki okumayı birlikte sunar. Yüzey masaüstünde iki kolon, mobilde tek kolon,
dört locale ve açık/koyu temada aynı sözleşmeyle çalışır.

## Kanıt ve ürün sınırı

- PubMed'deki 141 çalışma ve 16.523 katılımcıyı kapsayan meta-analiz, hedef belirlemenin
  davranış üzerinde küçük ama pozitif bir etkisini raporlar:
  https://pubmed.ncbi.nlm.nih.gov/29189034/
- 28 çalışmalık sistematik inceleme hedef, öz izleme ve davranış geri bildirimini dijital
  etkileşimle en sık ilişkilendirilen teknikler arasında bulur; aynı inceleme bilişsel yükün
  düşük tutulmasını önerir: https://pmc.ncbi.nlm.nih.gov/articles/PMC10545861/
- BBC'nin hesapla cihazlar arası kaydetme ve doğrudan Saved erişimi deseni mevcut BOECL
  temelini doğrular: https://help.bbc.com/hc/en-us/articles/39023397314963-How-do-I-save-content-to-read-later
- BOECL bu kanıtı rekabetçi seri, rozet, sosyal kıyas veya baskıcı bildirim olarak değil;
  kullanıcının kontrol ettiği sade bir haftalık niyet ve geri bildirim yüzeyi olarak uygular.
  Yeni izleme olayı, e-posta, push, profil çıkarımı veya içerik duvarı eklenmez.

## Veri, güvenlik ve kalite

- Tamamlama yüzde 95 eşiğinde bir kez zaman damgalanır; sonraki sayfa başı ölçümü tamamlanmış
  okumayı geriye düşüremez. Eski satırlar uydurma tamamlanma tarihiyle backfill edilmez.
- Hedef yalnız 1, 3 veya 5 olabilir; yazma oturum sahipliği ve antiforgery ile korunur,
  değişiklik audit izine yazılır. Öneri yalnız aktif locale'deki yayımlanmış, kaydedilmiş
  veya takip edilen kategoriye bağlı ve tamamlanmamış içeriği seçer.
- Migration iki nullable/default kolon ve bileşik sorgu indeksi ekler; `Down` yolu vardır.
  Production öncesi yedek, staging migration ve sağlık kapısı zorunludur.
- Hesap rotası noindex kalır; canonical, hreflang, sitemap ve açık içerik davranışı değişmez.

## Kalite kapıları

- Locale tutarlılığı, ESLint, TypeScript ve Next.js production build geçti.
- 133 API testi ve .NET Release build geçti; anonim erişim/yazma ile tamamlamanın gerilememesi
  için regresyonlar eklendi.
- Staging/production deploy, canlı sağlık ve yetkili gerçek render sonucu çevrim kapanışında
  kaydedilecektir.

## Sonraki üyelik backlog'u

1. Açık rızalı özet tercih merkezi yalnız doğrulanmış teslim altyapısıyla.
2. Hesap verisi dışa aktarma ve silme self-service akışı.
3. Mahremiyet korumalı save → return ve hedef tamamlama ölçümü.
4. Kullanıcının kişisel akışta konu sessize alması ve sıralamayı kontrol etmesi.
