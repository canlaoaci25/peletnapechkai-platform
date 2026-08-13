[CmdletBinding()]
param([string]$Root = 'C:\ProgramData\Peletnapechkai\SearchConsole')
$ErrorActionPreference = 'Stop'
$clientPath = Join-Path $Root 'oauth-client.json'
$tokenPath = Join-Path $Root 'oauth-token.json'
if (-not (Test-Path -LiteralPath $clientPath)) { throw 'OAuth client file is missing.' }
$client = (Get-Content -Raw -LiteralPath $clientPath | ConvertFrom-Json).installed
if (-not $client.client_id -or -not $client.client_secret) { throw 'Invalid desktop OAuth client.' }
$port = Get-Random -Minimum 49152 -Maximum 65000
$redirect = "http://localhost:$port/"
$state = [guid]::NewGuid().ToString('N')
$scope = 'https://www.googleapis.com/auth/webmasters.readonly'
$parameters = @(
  ('client_id=' + [uri]::EscapeDataString($client.client_id))
  ('redirect_uri=' + [uri]::EscapeDataString($redirect))
  'response_type=code',
  ('scope=' + [uri]::EscapeDataString($scope))
  'access_type=offline'
  'prompt=consent'
  ('state=' + $state)
)
$authorize = 'https://accounts.google.com/o/oauth2/v2/auth?' + [string]::Join('&', $parameters)
$listener = [Net.HttpListener]::new(); $listener.Prefixes.Add($redirect); $listener.Start()
try {
  & 'C:\Program Files\Google\Chrome\Application\chrome.exe' $authorize
  $context = $listener.GetContext()
  $query = $context.Request.QueryString
  $message = if ($query['code'] -and $query['state'] -eq $state) { 'BOECL Search Console yetkilendirmesi alindi. Bu sekmeyi kapatabilirsiniz.' } else { 'Yetkilendirme tamamlanamadi.' }
  $bytes = [Text.Encoding]::UTF8.GetBytes("<!doctype html><meta charset=utf-8><title>BOECL</title><p>$message</p>")
  $context.Response.ContentType='text/html; charset=utf-8';$context.Response.OutputStream.Write($bytes,0,$bytes.Length);$context.Response.Close()
  if (-not $query['code'] -or $query['state'] -ne $state) { throw "OAuth authorization failed: $($query['error'])" }
  $response = Invoke-RestMethod -Method Post -Uri 'https://oauth2.googleapis.com/token' -ContentType 'application/x-www-form-urlencoded' -Body @{
    code=$query['code'];client_id=$client.client_id;client_secret=$client.client_secret;redirect_uri=$redirect;grant_type='authorization_code'
  }
  if (-not $response.refresh_token) { throw 'Google did not return a refresh token.' }
  [ordered]@{refresh_token=$response.refresh_token;scope=$scope;created_at=(Get-Date).ToUniversalTime().ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $tokenPath -Encoding utf8
  icacls $tokenPath /inheritance:r /grant:r 'SYSTEM:F' 'Administrators:F' 'IIS AppPool\PeletnapechkaiApiPool:R' | Out-Null
  [pscustomobject]@{Connected=$true;TokenStored=$true;Scope=$scope}
} finally { $listener.Stop();$listener.Close() }
