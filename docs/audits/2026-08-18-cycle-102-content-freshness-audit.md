# Cycle 102 — içerik tazelik çalışma akışı

## Görünür sonuç

Admin editoryal komuta merkezi artık eski yayınları yalnız yaş etiketiyle sıralamaz. Kaynak inceleme borcu, gerçek site içi okur ilgisi ve açık SEO kalite kapısı ayrı gerekçelerle görünür. Ölçüm kaydı yoksa etki tahmin edilmez ve bu durum açıkça belirtilir. Yetkili editör, yayımlanmış Türkçe kaynak sürümden tek eylemle kendisine yüksek öncelikli revizyon görevi açabilir.

## Ürün ve güvenlik sözleşmesi

- Tazelik işi yalnız yayımlanmış `tr-TR` kaynak sürümden başlatılır; çeviri sürümü bağımsız kaynak gibi güncellenmez.
- Görev oluşturma CSRF korumalı, yetkili ve idempotenttir. Aynı yayın için ikinci açık tazelik görevi üretilmez.
- Görev audit kaydı; kaynak locale, politika sürümü ve yeniden dağıtımın editoryal tamamlanma gerektirdiğini taşır.
- Okur ilgisi yalnız mevcut `ArticleEngagement.ViewCount` kanıtından gelir. Veri yoksa sayı uydurulmaz.
- Metadata değişikliği otomatik olarak borcu kapatmaz; bu dilim yayın tarihini veya içeriği değiştirmez.
- Şema ve production verisi değişmedi; migration veya veri rollback'i gerekmez.

## Operasyonel iyileştirme

SYSADMIN incelemesi autonomous runner'ın staging API/Web kohortu doğrulanmadan production'a ilerleyebildiğini buldu. Runner artık değişen tüm bileşenleri staging'e aldıktan sonra `Invoke-StagingHealthCheck.ps1` kapısını çalıştırır. Bu kapı geçmeden production backup, migration veya deploy başlamaz; production kohortu da final sağlık kontrolüyle kapanır. Regresyon testi sıra sözleşmesini doğrular.

## Kalite kanıtı

- Locale consistency: 4/4
- Web testleri: 93/93
- Web lint: PASS
- Web typecheck: PASS
- Next.js production build: PASS, 109 statik sayfa üretildi
- API Release testleri: 187/187
- .NET Release build: PASS, 0 uyarı / 0 hata
- Database backup promotion regression: PASS
- Release promotion regression: PASS

## Director kapısı

FULLSTACK, DESIGNER ve EDITOR ürün hedefinde PASS verdi. SYSADMIN ilk runner sıralamasına REJECT verdi; staging-before-production sağlık kapısı ve testi eklendikten sonra yerel kod kapısı PASS kabul edildi. Gerçek staging/production render ve sağlık kanıtı runner sorumluluğundadır. Bu nedenle `content-freshness` birleşik workstream'i yerel kanıta sahip olsa da canlı kabul tamamlanana kadar `pending` kalır.

## Rollback ve kalan risk

Kod rollback'i bu commit'in geri alınmasıdır; yeni tablo/kolon veya içerik mutasyonu yoktur. Oluşturulmuş editoryal görevler silinmez, normal iş akışında kapatılır. Canlı açık/koyu tema ve mobil render, staging/production sağlık ve gerçek audit kaydı bu worktree içinde doğrulanmadı.
