[CmdletBinding()]
param(
    [string]$ConfigPath = 'C:\ProgramData\Peletnapechkai\Secrets\automation-worker.json'
)

$ErrorActionPreference = 'Stop'
$logRoot = 'C:\ProgramData\Peletnapechkai\Logs\AutomationWorker'
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Codex-Automation-Worker')
if (-not $mutex.WaitOne(0)) { exit 0 }

try {
    $config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
    $env:CODEX_HOME = [string]$config.codexHome
    $headers = @{ 'X-BOECL-Worker-Token' = [string]$config.workerToken }
    try {
        $job = Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/claim" -Headers $headers
    }
    catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 204) { exit 0 }
        throw
    }
    if (-not $job.id) { exit 0 }

    $jobId = [string]$job.id
    $jobLog = Join-Path $logRoot "$jobId.jsonl"
    $stderrLog = Join-Path $logRoot "$jobId-stderr.log"
    $lastMessage = Join-Path $logRoot "$jobId-result.txt"
    $codexArguments = @(
        'exec', '--ephemeral', '--json', '--sandbox', 'danger-full-access',
        '--cd', [string]$config.repositoryPath, '--output-last-message', $lastMessage, '-'
    )
    $savedErrorPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    [string]$job.prompt | & ([string]$config.codexPath) @codexArguments 2> $stderrLog | Set-Content -LiteralPath $jobLog -Encoding utf8
    $codexExitCode = $LASTEXITCODE
    $ErrorActionPreference = $savedErrorPreference
    if ($codexExitCode -ne 0) {
        $stderrTail = if (Test-Path -LiteralPath $stderrLog) { (Get-Content -LiteralPath $stderrLog -Tail 8) -join ' ' } else { '' }
        throw "Codex exited with code $codexExitCode. $stderrTail"
    }

    $result = if (Test-Path -LiteralPath $lastMessage) { Get-Content -LiteralPath $lastMessage -Raw } else { 'Codex işi tamamladı.' }
    if ($result.Length -gt 1800) { $result = $result.Substring(0, 1800) }
    $body = @{ message = $result } | ConvertTo-Json
    $bodyBytes = [Text.Encoding]::UTF8.GetBytes($body)
    Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/complete" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $bodyBytes | Out-Null
}
catch {
    $message = $_.Exception.Message
    if ($jobId) {
        try {
            if ($message.Length -gt 1800) { $message = $message.Substring(0, 1800) }
            $body = @{ message = $message } | ConvertTo-Json
            $bodyBytes = [Text.Encoding]::UTF8.GetBytes($body)
            Invoke-RestMethod -Method Post -Uri "$($config.apiUrl)/api/v1/internal/automation-worker/$jobId/fail" -Headers $headers -ContentType 'application/json; charset=utf-8' -Body $bodyBytes | Out-Null
        }
        catch { }
    }
    "$(Get-Date -Format o) job=$jobId $message" | Add-Content -LiteralPath (Join-Path $logRoot 'worker-errors.log')
    exit 1
}
finally {
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
