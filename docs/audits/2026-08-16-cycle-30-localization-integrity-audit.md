# BOECL Çevrim 30 — Yerelleştirme bütünlüğü

Tarih: 16 Ağustos 2026  
Odak: çeviri, locale bütünlüğü ve uluslararası deneyim

## Görünür önce/sonra hedefi

Uluslararası Yayın Sağlığı daha önce yalnız eksik kayıtları ve insan inceleme borcunu
gösteriyordu. Kaynak Türkçe makale bir çeviriden sonra değiştiğinde editör bu sapmayı
göremiyor; yerelleştirilmemiş kategori yolları da yayın kapsamının içinde saklanıyordu.
Bu çevrim sonunda aynı yönetim yüzeyi her etkin locale için güncelliğini yitiren çeviri
sayısını ve yayımlanmış kaynak kategorilere bağlı kategori oranını gösterir. Özet kartları
toplam güncellik ve taxonomy borcunu önceliklendirir.

## Veri ve güvenlik sınırı

- Güncellik farkı yalnız yayımlanmış varsayılan-locale kaynağının `UpdatedAt` değeri hedef
  taslak/yayından yeni olduğunda oluşur. Başka article group veya locale karışmaz.
- Kategori eşitliği yalnız yayımlanmış Türkçe içeriği bulunan kaynak kategoriler ile açık
  `SourceCategoryId` ilişkilerini karşılaştırır. Slug veya ada bakarak sahte ilişki kurulmaz.
- Endpoint mevcut yönetici yetki politikasını korur; yeni mutation, migration veya otomatik
  yayınlama eklenmez. Taslaklar public/indexlenebilir hale gelmez.
- Tag modelinde doğrulanmış çeviri ilişkisi bulunmadığından tag eşitliği uydurulmadı; bu,
  veri modeli ve migration gerektiren açık backlog öğesidir.

## Kabul kriterleri

- Yönetim özeti güncellik farkını ve eksik kategori sayısını gösterir.
- Her hedef locale bağlı/toplam kaynak kategori oranını, eksik çeviri, güncellik ve inceleme
  borcuyla birlikte gösterir.
- Dört locale sözlüğü anahtar ve placeholder sözleşmesini korur.
- Web regresyonları, lint, typecheck, production build, API test/build ve staging sorgusu geçer.
- Dar ekran iki kolon; geniş ekran üç kolon özet hiyerarşisiyle taşmadan çalışır.

## Kalıcı backlog

1. Çeviri işi sahibi, son tarih ve SLA eskalasyonu.
2. Locale bazlı kaynak, SEO, kapak ve gövde kalite skoru.
3. Tag çeviri kaynak ilişkisi, güvenli migration ve orphan raporu.
4. Kaynak revizyon farkını alan bazında gösteren karşılaştırma ve yeniden inceleme aksiyonu.
5. Kültürel uyarlama gerektiren kapak ve alt metin inceleme kuyruğu.
6. Locale bazlı Search Console arama niyeti ve içerik boşluğu.
