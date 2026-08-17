# BOECL Çevrim 37 — İçerik Otoritesi ve Kaynak Kalitesi Merkezi

## Görünür önce/sonra hedefi

Önceden yönetici trafik ekranı düşük görüntülenmeyi tek başına iyileştirme sinyali sayıyor,
kaynak kapsamını veya kaynak çeşitliliğini göstermiyordu. Bu faz `/{locale}/admin/traffic`
yüzeyini; arşiv otorite dağılımı, kaynaksız/tek kaynaklı yayın borcu, en çok kullanılan
kaynak alan adları ve makale bazlı açıklanabilir kalite kuyruğuyla yeniden kurar.

## Kanıt ve güvenlik sınırı

- Puan; kaynak sayısı, bağımsız alan adı, HTTPS, SEO alanları, kapak, kategori ve etiketten
  hesaplanır. Bir kaynağın doğrulandığını, birincil olduğunu veya doğru olduğunu iddia etmez.
- Kuyruk önce en düşük puanı, eşitlikte mevcut görüntülenmeyi ve yayın tarihini kullanır;
  böylece yüksek etkili Türkçe iyileştirmeler görünür olur.
- Kaynak URL'leri mevcut domain kuralıyla public HTTP(S) adresleridir. Yeni uç yazma yapmaz,
  dış URL çağırmaz, secret okumaz ve production verisini değiştirmez.
- Arayüz Türkçe, İngilizce, Almanca ve Fransızca tamamlanmıştır; dar ekranda puan, kanıt
  bantları ve eylemler tek kolonlu okunabilir akışa geçer.

## Test ve kabul

- İçerik otoritesi politikasına eksiksiz bağımsız kanıt, aynı domain/HTTP ve kaynaksız
  senaryolar için birim regresyonları eklendi.
- Locale eşitliği, lint, typecheck, Next production build, 117 API testi ve .NET Release
  build geçmelidir.
- Staging API gerçek arşiv sayılarını döndürmeli; staging ve production health ile public
  experience kapıları geçmeden faz tamamlanmış sayılmaz.

## Kalıcı backlog

1. Kaynak kaydına tür, editör sahibi, son doğrulama zamanı ve kontrollü sağlık sonucu ekle.
2. Search Console sorgularını ilgili Türkçe içerik ve taxonomy boşluklarıyla eşleştir.
3. Kaynak çeşitliliği kapısını yayın checklist'ine otomatik kanıt olarak bağla.
4. Trafik değişimini içerik güncelleme olayıyla karşılaştıran 28/90 günlük etki görünümü.
5. İnsan onaylı cluster-içi bağlantı önerileri ve orphan içerik kuyruğu.
