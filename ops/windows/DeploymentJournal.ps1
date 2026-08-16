function Write-BoeclDeploymentJournal {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('Staging','Production')][string]$Environment,
        [Parameter(Mandatory)][ValidateSet('Web','Api')][string]$Component,
        [Parameter(Mandatory)][ValidateSet('Started','Verifying','Succeeded','Failed','RolledBack','RollbackFailed')][string]$Status,
        [Parameter(Mandatory)][string]$DeploymentId,
        [string]$Commit = '',
        [string]$Message = '',
        [datetimeoffset]$StartedAt = [datetimeoffset]::UtcNow,
        [string]$JournalRoot = 'C:\ProgramData\Peletnapechkai\Deployments'
    )
    $ErrorActionPreference = 'Stop'
    New-Item -ItemType Directory -Path $JournalRoot -Force | Out-Null
    $safeMessage = ($Message -replace '[\r\n\t]+',' ' -replace '(?i)(password|token|secret|key)\s*[=:]\s*\S+','$1=[redacted]').Trim()
    if ($safeMessage.Length -gt 240) { $safeMessage = $safeMessage.Substring(0,240) }
    $now = [datetimeoffset]::UtcNow
    $payload = [ordered]@{
        SchemaVersion = 2; DeploymentId = $DeploymentId; Environment = $Environment
        Component = $Component; Status = $Status; Commit = $Commit; Message = $safeMessage
        StartedAt = $StartedAt.ToString('o'); UpdatedAt = $now.ToString('o')
        DurationSeconds = [math]::Max(0,[math]::Round(($now - $StartedAt).TotalSeconds))
    }
    $target = Join-Path $JournalRoot ("latest-{0}-{1}.json" -f $Environment.ToLowerInvariant(),$Component.ToLowerInvariant())
    if ($DeploymentId -notmatch '^[a-zA-Z0-9-]{1,64}$') { throw 'Deployment id contains unsupported characters.' }
    $historyTarget = Join-Path $JournalRoot ("deployment-{0}.json" -f $DeploymentId)
    # Persist durable evidence before advancing the latest pointer.
    foreach ($destination in @($historyTarget,$target)) {
        $temporary = Join-Path $JournalRoot (".{0}.tmp" -f [guid]::NewGuid().ToString('N'))
        $payload | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath $temporary -Encoding utf8
        Move-Item -LiteralPath $temporary -Destination $destination -Force
    }
}
