[CmdletBinding()]
param(
    [string]$BaseUrl = 'https://staging.peletnapechkai.com',
    [string]$SearchTerm = 'verification'
)

$ErrorActionPreference = 'Stop'
$base = $BaseUrl.TrimEnd('/')
foreach ($locale in 'tr-TR', 'en-US', 'de-DE', 'fr-FR') {
    $homeResponse = Invoke-WebRequest "$base/$locale" -UseBasicParsing -TimeoutSec 30
    if ($homeResponse.StatusCode -ne 200 -or $homeResponse.Content -notmatch 'class="skip-link"') {
        throw "Public accessibility check failed for $locale."
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
[pscustomobject]@{ BaseUrl = $base; Locales = 4; Search = 'Success'; Accessibility = 'Success'; Result = 'Success' }
