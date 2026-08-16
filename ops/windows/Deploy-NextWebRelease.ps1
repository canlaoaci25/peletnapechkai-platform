[CmdletBinding()]
param(
    [ValidateSet('Staging','Production')][string]$Environment,
    [string]$BuildRoot = (Join-Path $PSScriptRoot '..\..\apps\web')
)

$ErrorActionPreference = 'Stop'
$settings = if ($Environment -eq 'Production') {
    @{ Service='PeletnapechkaiWeb'; Root='C:\inetpub\peletnapechkai'; Health='Test-ProductionHealth.ps1'; BaseUrl='https://peletnapechkai.com' }
} else {
    @{ Service='BoeclStagingWeb'; Root='C:\inetpub\boecl-staging'; Health='Test-StagingHealth.ps1'; BaseUrl='https://staging.peletnapechkai.com' }
}

$root = [IO.Path]::GetFullPath($settings.Root)
$active = Join-Path $root 'web'
$release = Join-Path $root ('.web-release-' + [guid]::NewGuid().ToString('N'))
$rollback = Join-Path $root ('.web-rollback-' + (Get-Date -Format 'yyyyMMddHHmmss'))
$standalone = [IO.Path]::GetFullPath((Join-Path $BuildRoot '.next\standalone'))
$static = [IO.Path]::GetFullPath((Join-Path $BuildRoot '.next\static'))
$public = [IO.Path]::GetFullPath((Join-Path $BuildRoot 'public'))

foreach ($path in @($active,$release,$rollback)) {
    if (-not ([IO.Path]::GetFullPath($path).StartsWith($root + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase))) {
        throw "Deployment path escaped the expected root: $path"
    }
}
foreach ($required in @($standalone,$static,$public)) {
    if (-not (Test-Path -LiteralPath $required -PathType Container)) { throw "Missing build artifact: $required" }
}
if (-not (Test-Path -LiteralPath (Join-Path $standalone 'server.js'))) { throw 'Standalone server.js is missing.' }

New-Item -ItemType Directory -Path $release -Force | Out-Null
Copy-Item -Path (Join-Path $standalone '*') -Destination $release -Recurse -Force
New-Item -ItemType Directory -Path (Join-Path $release '.next\static') -Force | Out-Null
Copy-Item -Path (Join-Path $static '*') -Destination (Join-Path $release '.next\static') -Recurse -Force
New-Item -ItemType Directory -Path (Join-Path $release 'public') -Force | Out-Null
Copy-Item -Path (Join-Path $public '*') -Destination (Join-Path $release 'public') -Recurse -Force
$currentSitemapText = Join-Path $active 'public\sitemap.txt'
if (Test-Path -LiteralPath $currentSitemapText -PathType Leaf) {
    Copy-Item -LiteralPath $currentSitemapText -Destination (Join-Path $release 'public\sitemap.txt') -Force
}

$serviceStopped = $false
try {
    Stop-Service -Name $settings.Service -Force
    $serviceStopped = $true
    Move-Item -LiteralPath $active -Destination $rollback
    Move-Item -LiteralPath $release -Destination $active
    Start-Service -Name $settings.Service
    $serviceStopped = $false
    $healthy = $false
    for ($attempt = 1; $attempt -le 6; $attempt++) {
        Start-Sleep -Seconds 5
        & (Join-Path $PSScriptRoot $settings.Health) | Out-Null
        if ($LASTEXITCODE -eq 0) { $healthy = $true; break }
    }
    if (-not $healthy) { throw "$Environment health check failed after startup retries." }
    & (Join-Path $PSScriptRoot 'Test-PublicExperience.ps1') -BaseUrl $settings.BaseUrl | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "$Environment public experience check failed." }
    [pscustomobject]@{ Environment=$Environment; Active=$active; Rollback=$rollback; Healthy=$true }
}
catch {
    if (-not $serviceStopped) { Stop-Service -Name $settings.Service -Force -ErrorAction SilentlyContinue }
    if (Test-Path -LiteralPath $active) { Move-Item -LiteralPath $active -Destination ($release + '-failed') }
    if (Test-Path -LiteralPath $rollback) { Move-Item -LiteralPath $rollback -Destination $active }
    Start-Service -Name $settings.Service -ErrorAction SilentlyContinue
    throw
}
