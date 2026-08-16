[CmdletBinding()]
param(
    [uri]$SitemapUrl = 'https://peletnapechkai.com/sitemap.xml',
    [string]$OutputPath = 'C:\inetpub\peletnapechkai\web\public\sitemap.txt',
    [string]$LogPath = 'C:\ProgramData\Peletnapechkai\Logs\sitemap-text.log'
)

$ErrorActionPreference = 'Stop'
trap {
    $failureDirectory = Split-Path -Parent $LogPath
    New-Item -ItemType Directory -Path $failureDirectory -Force | Out-Null
    "$(Get-Date -Format o) sitemap.txt update failed: $($_.Exception.Message) | $($_.ScriptStackTrace)" | Add-Content -LiteralPath $LogPath -Encoding UTF8
    exit 1
}
$allowedRoot = [IO.Path]::GetFullPath('C:\inetpub\peletnapechkai\web\public')
$resolvedOutput = [IO.Path]::GetFullPath($OutputPath)
if (-not $resolvedOutput.StartsWith($allowedRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'sitemap.txt output escaped the production public directory.'
}
if ($SitemapUrl.Scheme -ne 'https' -or $SitemapUrl.Host -notin @('peletnapechkai.com','www.peletnapechkai.com')) {
    throw 'Only the BOECL production sitemap may be used.'
}

$response = Invoke-WebRequest -Uri $SitemapUrl.AbsoluteUri -UseBasicParsing -TimeoutSec 45
if ($response.StatusCode -ne 200) { throw "Sitemap returned HTTP $($response.StatusCode)." }
[xml]$document = $response.Content
$urls = @($document.SelectNodes("//*[local-name()='loc']") | ForEach-Object { $_.InnerText.Trim() } | Where-Object {
    $candidate = $null
    [uri]::TryCreate($_, [UriKind]::Absolute, [ref]$candidate) -and
    $candidate.Scheme -eq 'https' -and $candidate.Host -eq 'peletnapechkai.com'
} | Sort-Object -Unique)
if ($urls.Count -lt 4) { throw "Sitemap validation failed: only $($urls.Count) same-origin URLs found." }

$directory = Split-Path -Parent $resolvedOutput
New-Item -ItemType Directory -Path $directory -Force | Out-Null
$temporary = Join-Path $directory ('.sitemap-' + [guid]::NewGuid().ToString('N') + '.tmp')
$backup = Join-Path $directory '.sitemap-previous.tmp'
try {
    [IO.File]::WriteAllText($temporary, (($urls -join "`n") + "`n"), [Text.UTF8Encoding]::new($false))
    if (Test-Path -LiteralPath $resolvedOutput) {
        if (Test-Path -LiteralPath $backup) { Remove-Item -LiteralPath $backup -Force }
        [IO.File]::Replace($temporary, $resolvedOutput, $backup)
        Remove-Item -LiteralPath $backup -Force
    }
    else { [IO.File]::Move($temporary, $resolvedOutput) }
}
finally {
    if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force }
    if (Test-Path -LiteralPath $backup) { Remove-Item -LiteralPath $backup -Force }
}

$logDirectory = Split-Path -Parent $LogPath
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
"$(Get-Date -Format o) sitemap.txt updated with $($urls.Count) URLs." | Add-Content -LiteralPath $LogPath -Encoding UTF8
[pscustomobject]@{ Updated=$true; UrlCount=$urls.Count; Output=$resolvedOutput; CheckedAt=Get-Date }
