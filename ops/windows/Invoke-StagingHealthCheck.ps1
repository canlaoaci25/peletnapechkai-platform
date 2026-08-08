$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path $PSScriptRoot 'Test-StagingHealth.ps1'
$logDirectory = 'C:\ProgramData\Peletnapechkai\Health\Staging'
$latestPath = Join-Path $logDirectory 'latest.json'

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
try {
    $json = & $scriptPath
    $exitCode = $LASTEXITCODE
    $json | Set-Content -LiteralPath $latestPath -Encoding utf8
    if ($exitCode -ne 0) {
        & eventcreate.exe /T ERROR /ID 110 /L APPLICATION /SO PeletnapechkaiHealth /D 'Staging health check failed. See C:\ProgramData\Peletnapechkai\Health\Staging\latest.json.' | Out-Null
    }
    exit $exitCode
}
catch {
    [pscustomobject]@{ CheckedAt = (Get-Date).ToString('o'); Healthy = $false; Failures = @($_.Exception.Message) } |
        ConvertTo-Json -Depth 3 | Set-Content -LiteralPath $latestPath -Encoding utf8
    exit 1
}
