function Get-BoeclAutonomousRoadmap {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Path, [ValidateRange(10, 50)][int]$MinimumFutureItems = 10)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Otonom yol haritasi bulunamadi: $Path" }
    $document = Get-Content -Raw -LiteralPath $Path -Encoding UTF8 | ConvertFrom-Json
    $items = @($document.items)
    if ($items.Count -gt 30) { throw 'Otonom yol haritasi en fazla 30 madde icerebilir.' }
    $allowedStatuses = @('active','queued','blocked','completed')
    $safe = @()
    $ids = @{}
    foreach ($item in $items) {
        $id = ([string]$item.id).Trim()
        $title = ([string]$item.title).Trim()
        $outcome = ([string]$item.outcome).Trim()
        $status = ([string]$item.status).Trim().ToLowerInvariant()
        if ($id -notmatch '^[a-z0-9][a-z0-9-]{2,59}$' -or $ids.ContainsKey($id)) { throw "Gecersiz veya tekrar eden yol haritasi kimligi: $id" }
        if ([string]::IsNullOrWhiteSpace($title) -or $title.Length -gt 100) { throw "Gecersiz yol haritasi basligi: $id" }
        if ([string]::IsNullOrWhiteSpace($outcome) -or $outcome.Length -gt 300) { throw "Gecersiz yol haritasi sonucu: $id" }
        if ($status -notin $allowedStatuses) { throw "Gecersiz yol haritasi durumu: $id" }
        $ids[$id] = $true
        $safe += [pscustomobject]@{ id=$id; title=$title; outcome=$outcome; status=$status }
    }
    $futureCount = @($safe | Where-Object { $_.status -in @('active','queued','blocked') }).Count
    if ($futureCount -lt $MinimumFutureItems) { throw "Otonom yol haritasi en az $MinimumFutureItems gelecek adim icermelidir." }
    if (@($safe | Where-Object status -eq 'active').Count -gt 1) { throw 'Ayni anda en fazla bir yol haritasi maddesi aktif olabilir.' }
    return $safe
}

function Repair-BoeclAutonomousRoadmap {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Path,
        [ValidateRange(10, 50)][int]$MinimumFutureItems = 10,
        [ValidateRange(10, 50)][int]$TargetFutureItems = 12
    )

    if ($TargetFutureItems -lt $MinimumFutureItems) { throw 'Hedef yol haritasi tamponu minimumdan kucuk olamaz.' }
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { throw "Otonom yol haritasi bulunamadi: $Path" }
    $document = Get-Content -Raw -LiteralPath $Path -Encoding UTF8 | ConvertFrom-Json
    $items = @($document.items)
    $allowedStatuses = @('active','queued','blocked','completed')
    $safe = @()
    $ids = @{}
    foreach ($item in $items) {
        $id = ([string]$item.id).Trim()
        $title = ([string]$item.title).Trim()
        $outcome = ([string]$item.outcome).Trim()
        $status = ([string]$item.status).Trim().ToLowerInvariant()
        if ($id -notmatch '^[a-z0-9][a-z0-9-]{2,59}$' -or $ids.ContainsKey($id)) { throw "Gecersiz veya tekrar eden yol haritasi kimligi: $id" }
        if ([string]::IsNullOrWhiteSpace($title) -or $title.Length -gt 100) { throw "Gecersiz yol haritasi basligi: $id" }
        if ([string]::IsNullOrWhiteSpace($outcome) -or $outcome.Length -gt 300) { throw "Gecersiz yol haritasi sonucu: $id" }
        if ($status -notin $allowedStatuses) { throw "Gecersiz yol haritasi durumu: $id" }
        $ids[$id] = $true
        $safe += [pscustomobject]@{ id=$id; title=$title; outcome=$outcome; status=$status }
    }

    $changed = $false
    $active = @($safe | Where-Object status -eq 'active')
    if ($active.Count -gt 1) {
        foreach ($duplicate in @($active | Select-Object -Skip 1)) { $duplicate.status = 'queued' }
        $changed = $true
    } elseif ($active.Count -eq 0) {
        $next = $safe | Where-Object status -eq 'queued' | Select-Object -First 1
        if ($null -ne $next) { $next.status = 'active'; $changed = $true }
    }

    $templates = @(
        @{ id='content-freshness'; title='İçerik tazelik ve güncelleme merkezi'; outcome='Eskiyen yayınları kaynak, trafik ve değişen bilgi sinyalleriyle bul; revizyon, SEO ve yeniden dağıtım akışını tamamla.' },
        @{ id='source-trust'; title='Kaynak güveni ve kanıt görünürlüğü'; outcome='Kaynak otoritesi, güncellik ve iddia bağlantılarını ölç; okura görünür kanıt ve düzeltme yolları sun.' },
        @{ id='structured-discovery'; title='Yapılandırılmış veri ve keşif kapsamı'; outcome='İçerik türlerine uygun schema, breadcrumb, canonical, hreflang ve dahili keşif bağlantılarını uçtan uca doğrula.' },
        @{ id='mobile-accessibility'; title='Mobil erişilebilirlik ve kullanım kalitesi'; outcome='Gerçek dar ekran akışlarında dokunma hedefi, klavye, odak, kontrast, taşma ve performans borçlarını gider.' },
        @{ id='member-personalization'; title='Üye kişiselleştirme ve takip akışı'; outcome='Takip edilen konu, okuma geçmişi ve kayıtlı içeriklerden şeffaf ve yönetilebilir kişisel keşif alanı oluştur.' },
        @{ id='search-intent'; title='Arama niyeti ve sıfır sonuç kurtarması'; outcome='Arama terimlerini konu kümeleriyle eşleştir; yazım toleransı, öneri ve sıfır sonuç kurtarma deneyimini geliştir.' },
        @{ id='editorial-freshness'; title='Editoryal kalite ve tazelik kuyruğu'; outcome='Eksik kaynak, eski bilgi, zayıf SEO ve görsel borcunu öncelikli, atanabilir ve ölçülebilir admin kuyruğuna dönüştür.' },
        @{ id='visual-consistency'; title='Görsel tutarlılık ve özgünlük denetimi'; outcome='Kapak ve gövde görsellerini konu uyumu, yazısızlık, tekrar, telif ve mobil kırpma kapılarıyla sürekli denetle.' },
        @{ id='locale-parity'; title='Locale yayın eşitliği'; outcome='Türkçe kaynak, çeviri, kategori, SEO, görsel alt metni ve yönlendirme kapsamlarını dil bazında eşitle.' },
        @{ id='engagement-learning'; title='İzinli etkileşim öğrenme döngüsü'; outcome='Gizlilik izinli davranış sinyalleriyle ana sayfa, kategori ve öneri sıralamasını ölçülebilir deneylerle iyileştir.' },
        @{ id='release-observability'; title='Canlı sürüm gözlemlenebilirliği'; outcome='Web, API, veritabanı ve worker sürüm uyumunu SLO, hata bütçesi, rollback ve admin kanıtlarıyla görünür kıl.' },
        @{ id='archive-authority'; title='Arşiv ve konu otoritesi derinliği'; outcome='Zayıf kategori arşivlerini rehber, karşılaştırma, seri ve iç bağlantı kümeleriyle güvenilir keşif merkezlerine dönüştür.' }
    )

    $futureCount = @($safe | Where-Object { $_.status -in @('active','queued','blocked') }).Count
    $templateIndex = 0
    while ($futureCount -lt $TargetFutureItems) {
        $template = $templates[$templateIndex % $templates.Count]
        $candidateId = [string]$template.id
        $suffix = 1
        while ($ids.ContainsKey($candidateId)) { $candidateId = "{0}-next-{1}" -f $template.id,$suffix; $suffix++ }
        $safe += [pscustomobject]@{ id=$candidateId; title=[string]$template.title; outcome=[string]$template.outcome; status='queued' }
        $ids[$candidateId] = $true
        $futureCount++
        $templateIndex++
        $changed = $true
    }

    if ($safe.Count -gt 30) {
        $future = @($safe | Where-Object status -ne 'completed')
        $completedCapacity = [Math]::Max(0, 30 - $future.Count)
        $completed = @($safe | Where-Object status -eq 'completed' | Select-Object -Last $completedCapacity)
        $safe = @($completed) + @($future)
        $changed = $true
    }

    if ($changed) {
        $payload = [ordered]@{ updatedAt=[DateTimeOffset]::UtcNow.ToString('o'); items=$safe }
        $temporary = "$Path.$([guid]::NewGuid().ToString('N')).tmp"
        $payload | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath $temporary -Encoding UTF8
        Move-Item -LiteralPath $temporary -Destination $Path -Force
    }
    return @(Get-BoeclAutonomousRoadmap -Path $Path -MinimumFutureItems $MinimumFutureItems)
}
