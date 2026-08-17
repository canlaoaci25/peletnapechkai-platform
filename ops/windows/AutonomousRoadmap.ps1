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
