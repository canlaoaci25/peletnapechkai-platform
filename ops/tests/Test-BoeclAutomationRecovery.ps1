[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\windows\BoeclAutomationRecovery.ps1')

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
    $legacy = Find-BoeclRecoveredResult -LogRoot $testRoot -JobId 'job-2' -CurrentResultPath (Join-Path $testRoot 'current.json') -RequestFingerprint $fingerprintA
    if ($null -ne $legacy) { throw 'Parmak izi metadata kaydı olmayan eski sonuç yeniden kullanıldı.' }

    Write-Host 'BOECL otomasyon kurtarma regresyon testleri başarılı.'
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
