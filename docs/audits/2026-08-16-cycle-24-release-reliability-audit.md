# BOECL Çevrim 24 — Sürüm Kurtarma Merkezi

Admin ana ekranı staging ve production Web/API dağıtımlarının son commit, kalite kapısı,
süre ve otomatik rollback sonucunu gösterir. Her dağıtım atomik checkpoint yazar; başarısız
sürüm geri alındıktan sonra eski sürümün sağlığı ayrıca doğrulanır. Kayıtlar sanitize edilir.

Admin yüzeyi yalnız bilgilendiricidir. Çoklu geçmiş ve iki kişi onaylı elle rollback sonraki fazdır.
