param([Parameter(Mandatory=$true)][string]$Key,[string]$SiteUrl='https://peletnapechkai.com')
$ErrorActionPreference='Stop'
if($Key -notmatch '^[A-Za-z0-9-]{8,128}$'){throw 'IndexNow key format is invalid.'}
[xml]$sitemap=(Invoke-WebRequest -Uri "$SiteUrl/sitemap.xml" -UseBasicParsing).Content
$urls=@($sitemap.urlset.url.loc|ForEach-Object{[string]$_}|Where-Object{$_ -like "$SiteUrl/*"}|Select-Object -Unique)
if($urls.Count -eq 0){throw 'No same-origin URLs found in sitemap.'}
$body=@{host=([Uri]$SiteUrl).Host;key=$Key;keyLocation="$SiteUrl/indexnow-key.txt";urlList=$urls}|ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'https://api.indexnow.org/indexnow' -ContentType 'application/json; charset=utf-8' -Body $body
[pscustomobject]@{Submitted=$urls.Count;At=[DateTimeOffset]::Now}
