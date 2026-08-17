[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\windows\BoeclAutomationRecovery.ps1')
. (Join-Path $PSScriptRoot '..\windows\AutonomousCycleRecovery.ps1')
. (Join-Path $PSScriptRoot '..\windows\AutonomousRoadmap.ps1')

$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("boecl-recovery-{0}" -f [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $requestA = '{"kind":"generation","requestedCount":1,"category":"science","includeImages":true}'
    $requestB = '{"kind":"generation","requestedCount":1,"category":"travel","includeImages":true}'
    $fingerprintA = Get-BoeclRequestFingerprint -RequestJson $requestA
    $fingerprintB = Get-BoeclRequestFingerprint -RequestJson $requestB
    if ($fingerprintA -eq $fingerprintB) { throw 'Farklı istekler aynı parmak izini üretti.' }

    $matchingResult = Join-Path $testRoot 'job-1-run-batch-1-result.json'
    '{"items":[{"slug":"safe-result"}]}' | Set-Content -LiteralPath $matchingResult -Encoding UTF8
    Save-BoeclRecoveryMetadata -ResultPath $matchingResult -RequestFingerprint $fingerprintA

    $currentResult = Join-Path $testRoot 'job-1-new-batch-1-result.json'
    $found = Find-BoeclRecoveredResult -LogRoot $testRoot -JobId 'job-1' -CurrentResultPath $currentResult -RequestFingerprint $fingerprintA
    if ($found -ne $matchingResult) { throw 'Aynı isteğe ait doğrulanabilir kurtarma sonucu bulunamadı.' }
    $wrongRequest = Find-BoeclRecoveredResult -LogRoot $testRoot -JobId 'job-1' -CurrentResultPath $currentResult -RequestFingerprint $fingerprintB
    if ($null -ne $wrongRequest) { throw 'Farklı isteğe ait sonuç yeniden kullanılmak üzere seçildi.' }

    $legacyResult = Join-Path $testRoot 'job-2-run-batch-1-result.json'
    '{"items":[{"slug":"legacy-result"}]}' | Set-Content -LiteralPath $legacyResult -Encoding UTF8
    $delays = 1..8 | ForEach-Object { Get-BoeclRetryDelayMinutes -ConsecutiveFailures $_ }
    if (($delays -join ',') -ne '1,2,4,8,16,32,60,60') { throw 'Otonom yeniden deneme geri cekilmesi beklenen sinirda degil.' }
    $now = [DateTimeOffset]::Parse('2026-08-16T12:00:00Z')
    if (-not (Test-BoeclUtcDeadlinePending -Deadline '2026-08-16T12:01:00Z' -Now $now)) { throw 'Gelecek yeniden deneme zamani bekleme olarak algilanmadi.' }
    if (Test-BoeclUtcDeadlinePending -Deadline '2026-08-16T11:59:00Z' -Now $now) { throw 'Gecmis yeniden deneme zamani bekleme olarak algilandi.' }
    if (-not (Test-BoeclHeartbeatStale -Heartbeat '2026-08-16T11:49:59Z' -Now $now)) { throw 'Terk edilmis heartbeat algilanmadi.' }
if (Test-BoeclHeartbeatStale -Heartbeat '2026-08-16T11:55:00Z' -Now $now) { throw 'Saglikli heartbeat terk edilmis sayildi.' }

$roadmap = @(Get-BoeclAutonomousRoadmap -Path (Join-Path $PSScriptRoot '..\..\docs\operations\autonomous-roadmap.json'))
if ($roadmap.Count -lt 10) { throw 'Otonom yol haritasi en az 10 gelecek adim sunmuyor.' }
if (@($roadmap | Where-Object status -eq 'active').Count -ne 1) { throw 'Otonom yol haritasinda tek aktif adim bulunmali.' }

$eventLog = Join-Path $testRoot 'events.jsonl'
'{"type":"item.completed"}' | Set-Content -LiteralPath $eventLog -Encoding UTF8
if (Test-BoeclTurnCompletedEvent -EventPath $eventLog) { throw 'Normal olay tamamlanmis tur sayilmamali.' }
'{"type":"turn.completed"}' | Add-Content -LiteralPath $eventLog -Encoding UTF8
if (-not (Test-BoeclTurnCompletedEvent -EventPath $eventLog)) { throw 'Tamamlanmis tur olayi algilanamadi.' }

    $legacy = Find-BoeclRecoveredResult -LogRoot $testRoot -JobId 'job-2' -CurrentResultPath (Join-Path $testRoot 'current.json') -RequestFingerprint $fingerprintA
    if ($null -ne $legacy) { throw 'Parmak izi metadata kaydı olmayan eski sonuç yeniden kullanıldı.' }

    Write-Host 'BOECL otomasyon kurtarma regresyon testleri başarılı.'
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
