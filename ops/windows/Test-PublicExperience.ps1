[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://staging.peletnapechkai.com',
    [string]$SearchTerm = 'verification'
)

$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')
$localeConfig = Get-Content (Join-Path $PSScriptRoot '..\..\config\supported-locales.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$locales = $localeConfig.locales.PSObject.Properties.Name
foreach ($locale in $locales) {
    $homeResponse = Invoke-WebRequest "$base/$locale" -UseBasicParsing -TimeoutSec 30
    if ($homeResponse.StatusCode -ne 200 -or $homeResponse.Content -notmatch 'class="skip-link"') {
        throw "Public accessibility check failed for $locale."
    }
    $stylesheets = [regex]::Matches($homeResponse.Content, 'href="([^"]+\.css[^"]*)"') | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique
    if (-not $stylesheets) { throw "No stylesheet was rendered for $locale." }
    foreach ($stylesheet in $stylesheets) {
        $assetUrl = if ($stylesheet -match '^https?://') { $stylesheet } else { "$base$stylesheet" }
        $asset = Invoke-WebRequest $assetUrl -Method Head -UseBasicParsing -TimeoutSec 30
        if ($asset.StatusCode -ne 200 -or $asset.Headers['Content-Type'] -notmatch 'text/css') {
            throw "Stylesheet integrity check failed for $locale at $assetUrl."
        }
    }
    $search = Invoke-WebRequest "$base/$locale/search?q=$([uri]::EscapeDataString($SearchTerm))" -UseBasicParsing -TimeoutSec 30
    if ($search.StatusCode -ne 200 -or $search.Content -notmatch 'role="search"') {
        throw "Public search check failed for $locale."
    }
    $feed = Invoke-WebRequest "$base/$locale/feed.xml" -UseBasicParsing -TimeoutSec 30
    if ($feed.StatusCode -ne 200 -or $feed.Headers['Content-Type'] -notmatch 'application/rss\+xml') {
        throw "RSS check failed for $locale."
    }
}

$sitemap = Invoke-WebRequest "$base/sitemap.xml" -UseBasicParsing -TimeoutSec 30
if ($sitemap.StatusCode -ne 200) { throw 'Sitemap check failed.' }
[pscustomobject]@{ BaseUrl = $base; Locales = $locales.Count; Search = 'Success'; Accessibility = 'Success'; Stylesheets = 'Success'; Result = 'Success' }
