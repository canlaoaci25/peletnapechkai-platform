# Cycle 110 public focal contract audit

## Karar

Director kalite kapısı **yerel dilim için PASS**, `visual-service` ve birleşik tamamlama fazı için **HOLD** verdi. Kalıcı odak noktası artık ana sayfa ve makale hero'suyla sınırlı değildir; kategori/etiket/yazar arşivleri, konu merkezi, arama, kaynak arşivi, ilişkili yayınlar, üye kişisel akışı, okuma listesi, devam etme ve onboarding kartları aynı merkez-fallback sözleşmesini kullanır.

## Kanıt

- Üye API projeksiyonları focal X/Y değerlerini kaybetmeden istemciye taşır.
- Public ve üye kartları `focalPointStyle` ile 0..1 aralığına sıkıştırılmış `object-position` uygular; eski/null kayıtlar güvenli biçimde merkezde kalır.
- Kaynak sözleşme regresyon testi tüm değişen yüzeylerde focal stil kullanımını doğrular.
- 96 web testi, lint, typecheck, 110 rotalı production Web build, 196 API testi ve API Release build geçti.

## Rollback

Değişiklik şema veya veri mutasyonu içermez. Uygulama rollback'i önceki Web/API artefaktına dönerek yapılabilir; API'ye eklenen JSON alanları geriye uyumludur. Veritabanındaki mevcut focal koordinatları korunur.

## Açık kapılar

- Staging'de merkez dışı, hakları doğrulanmış gerçek bir görselle 390/768/1440 açık-koyu render ve yetkili admin crop matrisi henüz kanıtlanmadı.
- Bağımsız provider worker/completion zinciri, immutable asset provenance, yayın öncesi hak/görsel kapısı ve vision değerlendirmesi eksik.
- Production preflight önceki kanıtta 20 GB disk eşiğinin altında kaldı. Owner retention kararı olmadan artefakt temizlenmedi ve bu çevrim talimatı uyarınca deploy yapılmadı.
