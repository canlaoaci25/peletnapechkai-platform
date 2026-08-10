[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$failures = [Collections.Generic.List[string]]::new()

foreach ($name in 'BoeclStagingWeb', 'W3SVC', 'postgresql-x64-18') {
    $service = Get-Service -Name $name -ErrorAction SilentlyContinue
    if (-not $service -or $service.Status -ne 'Running') {
        $failures.Add("Service is not running: $name")
    }
}

foreach ($uri in 'https://staging.peletnapechkai.com/tr-TR',
    'https://staging.peletnapechkai.com/en-US',
    'https://staging.peletnapechkai.com/de-DE',
    'https://staging.peletnapechkai.com/fr-FR',
    'https://staging.peletnapechkai.com/api/admin/auth/csrf') {
    try {
        $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -ne 200) { $failures.Add("Unexpected status for ${uri}: $($response.StatusCode)") }
        if ($response.Headers['X-Robots-Tag'] -notmatch 'noindex') { $failures.Add("Staging noindex header is missing: $uri") }
    }
    catch { $failures.Add("Endpoint failed: $uri - $($_.Exception.Message)") }
}

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString('o')
    Healthy = $failures.Count -eq 0
    Failures = $failures
}
$result | ConvertTo-Json -Depth 3
if ($failures.Count -gt 0) { exit 1 }
exit 0
