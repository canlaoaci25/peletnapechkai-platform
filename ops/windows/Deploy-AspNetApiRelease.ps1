[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Staging','Production')][string]$Environment,
    [string]$RepositoryPath = (Join-Path $PSScriptRoot '..\..')
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'DeploymentJournal.ps1')
$deploymentId = [guid]::NewGuid().ToString('N')
$deploymentStartedAt = [datetimeoffset]::UtcNow
$commit = (& git.exe -C $RepositoryPath rev-parse --short=12 HEAD 2>$null)
Write-BoeclDeploymentJournal -Environment $Environment -Component Api -Status Started -DeploymentId $deploymentId -Commit $commit -StartedAt $deploymentStartedAt -Message 'API release is being published and staged.'
$terminalRecorded = $false
try {
Import-Module WebAdministration
$settings = if ($Environment -eq 'Production') {
    @{ Pool='PeletnapechkaiApiPool'; Root='C:\inetpub\peletnapechkai'; Health='Test-ProductionHealth.ps1' }
} else {
    @{ Pool='BoeclStagingApiPool'; Root='C:\inetpub\boecl-staging'; Health='Test-StagingHealth.ps1' }
}
$root = [IO.Path]::GetFullPath($settings.Root)
$active = Join-Path $root 'api'
$stamp = Get-Date -Format 'yyyyMMddHHmmss'
$release = Join-Path $root ".api-release-$stamp"
$rollback = Join-Path $root ".api-rollback-$stamp"
$publish = Join-Path 'C:\ProgramData\Peletnapechkai\Deploy' "autonomous-api-$Environment-$stamp"
foreach ($path in @($active,$release,$rollback)) {
    if (-not ([IO.Path]::GetFullPath($path).StartsWith($root + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase))) {
        throw "Deployment path escaped expected root: $path"
    }
    if ($path -ne $active -and (Test-Path -LiteralPath $path)) { throw "Deployment target already exists: $path" }
}

& dotnet.exe publish (Join-Path $RepositoryPath 'apps\api\Peletnapechkai.Api.csproj') -c Release -o $publish --no-build
if ($LASTEXITCODE -ne 0) { throw 'API publish failed.' }
Copy-Item -LiteralPath $publish -Destination $release -Recurse

$swapped = $false
try {
    Stop-WebAppPool $settings.Pool
    for ($attempt=0; $attempt -lt 30; $attempt++) {
        if ((Get-WebAppPoolState $settings.Pool).Value -eq 'Stopped') { break }
        Start-Sleep -Milliseconds 500
    }
    Start-Sleep -Seconds 2
    Move-Item -LiteralPath $active -Destination $rollback
    Move-Item -LiteralPath $release -Destination $active
    $swapped = $true
    Start-WebAppPool $settings.Pool
    Write-BoeclDeploymentJournal -Environment $Environment -Component Api -Status Verifying -DeploymentId $deploymentId -Commit $commit -StartedAt $deploymentStartedAt -Message 'Release swapped; API health gate is running.'
    Start-Sleep -Seconds 5
    & (Join-Path $PSScriptRoot $settings.Health) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "$Environment API health check failed." }
    Write-BoeclDeploymentJournal -Environment $Environment -Component Api -Status Succeeded -DeploymentId $deploymentId -Commit $commit -StartedAt $deploymentStartedAt -Message 'API health gate passed.'
    $terminalRecorded = $true
    [pscustomobject]@{ Environment=$Environment; Active=$active; Rollback=$rollback; Healthy=$true }
}
catch {
    $failureMessage = $_.Exception.Message
    Stop-WebAppPool $settings.Pool -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
    if ($swapped -and (Test-Path -LiteralPath $rollback)) {
        $failed = Join-Path $root ".api-failed-$stamp"
        Move-Item -LiteralPath $active -Destination $failed -ErrorAction SilentlyContinue
        Move-Item -LiteralPath $rollback -Destination $active
    }
    Start-WebAppPool $settings.Pool -ErrorAction SilentlyContinue
    $rollbackHealthy = $false
    try {
        Start-Sleep -Seconds 3
        & (Join-Path $PSScriptRoot $settings.Health) | Out-Null
        $rollbackHealthy = $LASTEXITCODE -eq 0
    } catch { $rollbackHealthy = $false }
    $recoveryStatus = if ($rollbackHealthy) { 'RolledBack' } else { 'RollbackFailed' }
    Write-BoeclDeploymentJournal -Environment $Environment -Component Api -Status $recoveryStatus -DeploymentId $deploymentId -Commit $commit -StartedAt $deploymentStartedAt -Message $failureMessage
    $terminalRecorded = $true
    throw
}
}
catch {
    if (-not $terminalRecorded) {
        Write-BoeclDeploymentJournal -Environment $Environment -Component Api -Status Failed -DeploymentId $deploymentId -Commit $commit -StartedAt $deploymentStartedAt -Message $_.Exception.Message
    }
    throw
}
