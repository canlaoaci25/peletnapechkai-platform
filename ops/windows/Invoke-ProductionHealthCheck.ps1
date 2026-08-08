$ErrorActionPreference = 'Stop'
$scriptPath = Join-Path $PSScriptRoot 'Test-ProductionHealth.ps1'
$logDirectory = 'C:\ProgramData\Peletnapechkai\Health'
$latestPath = Join-Path $logDirectory 'latest.json'
$historyPath = Join-Path $logDirectory 'history.log'

New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null

try {
    $json = & $scriptPath
    $exitCode = $LASTEXITCODE
    $json | Set-Content -LiteralPath $latestPath -Encoding utf8
    "$(Get-Date -Format o) exit=$exitCode $($json -join '')" | Add-Content -LiteralPath $historyPath
    if ($exitCode -ne 0) {
        & eventcreate.exe /T ERROR /ID 100 /L APPLICATION /SO PeletnapechkaiHealth /D 'Production health check failed. See C:\ProgramData\Peletnapechkai\Health\latest.json.' | Out-Null
    }
    exit $exitCode
}
catch {
    $failure = [pscustomobject]@{
        CheckedAt = (Get-Date).ToString('o')
        Healthy = $false
        Failures = @($_.Exception.Message)
    } | ConvertTo-Json -Depth 3
    $failure | Set-Content -LiteralPath $latestPath -Encoding utf8
    "$(Get-Date -Format o) exit=1 $failure" | Add-Content -LiteralPath $historyPath
    & eventcreate.exe /T ERROR /ID 101 /L APPLICATION /SO PeletnapechkaiHealth /D 'Production health check could not complete. See C:\ProgramData\Peletnapechkai\Health\latest.json.' | Out-Null
    exit 1
}
