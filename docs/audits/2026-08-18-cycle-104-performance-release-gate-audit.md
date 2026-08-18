# Çevrim 104 — Performans release kapısı

## Görünür sonuç

Admin trafik merkezi artık yalnız mobil değil masaüstü Core Web Vitals kohortlarını da locale, route,
metrik, örneklem ve bütçe kararıyla gösterir. Türkçe, Almanca ve Fransızca panel metinlerindeki bozuk
karakterler düzeltildi. Yetersiz örnek, gerçek bütçe ihlaliyle aynı kritik renk sinyalini kullanmaz.

## Dağıtım kapısı

`Test-WebReleaseBudget.ps1` tamamlanmış Next.js build manifestini ve gerçek artefaktları ölçer. Eksik
veya bozuk manifest, eksik client asset ya da sabit kök JS, tek chunk ve CSS bütçesi ihlali non-zero
sonuç üretir. Kapı hem rutin promotion hem otonom staging adımından önce çalışır; böylece başarısız
aday hiçbir ortamı değiştirmez. Bu deterministik artefakt kapısı gerçek LCP/CLS/INP garantisi değildir;
açık rızalı saha panelini tamamlayan erken regresyon korumasıdır.

## Yerel kanıt

- Gerçek build: root JS 437.466 / 655.360 byte; en büyük chunk 932.925 / 1.048.576 byte; en büyük CSS 211.059 / 262.144 byte — PASS.
- Web release budget PASS/aşım/eksik asset fixture testi: PASS.
- Promotion sırası ve PowerShell parse regresyonları: PASS.
- Locale sözleşmesi: 4 locale PASS.
- Web: 93 test, lint, typecheck ve 110 sayfalık production build PASS.
- API: 195 Release test PASS; Release build 0 warning, 0 error.

## Sınır ve rollback

Migration, secret, dış servis, IIS veya production verisi değiştirilmedi. Rollback bu commit'in geri
alınmasıdır. Staging/production migration, izinli beacon, açık/koyu 390/1440 render, retention ve canlı
promotion kanıtı bulunmadığından `performance-budget` workstream'i blocked kalır.
