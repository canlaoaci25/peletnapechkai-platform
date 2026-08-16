[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('Staging','Production')][string]$Environment,
    [string]$RepositoryPath = (Join-Path $PSScriptRoot '..\..')
)

$ErrorActionPreference = 'Stop'
$site = if ($Environment -eq 'Production') { 'Peletnapechkai API' } else { 'BOECL Staging API' }
$appcmd = Join-Path $env:windir 'System32\inetsrv\appcmd.exe'
$raw = (& $appcmd list config "$site/" /section:system.webServer/aspNetCore /xml) -join "`n"
$match = [regex]::Match($raw, 'name="ConnectionStrings__Database"\s+value="([^"]+)"')
if (-not $match.Success) { throw "Database connection setting is missing for $site." }
try {
    $env:ConnectionStrings__Database = [Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
    & dotnet.exe ef database update --project (Join-Path $RepositoryPath 'apps\api\Peletnapechkai.Api.csproj') `
        --startup-project (Join-Path $RepositoryPath 'apps\api\Peletnapechkai.Api.csproj') --no-build
    if ($LASTEXITCODE -ne 0) { throw "$Environment database migration failed with exit code $LASTEXITCODE." }
}
finally { Remove-Item Env:\ConnectionStrings__Database -ErrorAction SilentlyContinue }

