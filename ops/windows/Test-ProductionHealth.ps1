[CmdletBinding()]
param(
    [int]$MinimumFreeDiskGb = 20,
    [int]$MinimumCertificateDays = 21
)

$ErrorActionPreference = 'Stop'
$failures = [Collections.Generic.List[string]]::new()
$serviceNames = @('W3SVC', 'postgresql-x64-18', 'PeletnapechkaiWeb')

$services = foreach ($name in $serviceNames) {
    $service = Get-Service -Name $name -ErrorAction SilentlyContinue
    if (-not $service -or $service.Status -ne 'Running') {
        $failures.Add("Service is not running: $name")
    }
    [pscustomobject]@{
        Name = $name
        Status = if ($service) { $service.Status.ToString() } else { 'Missing' }
    }
}

$endpoints = foreach ($uri in @(
    'https://peletnapechkai.com/tr-TR',
    'https://peletnapechkai.com/en-US',
    'https://peletnapechkai.com/de-DE',
    'https://peletnapechkai.com/api/admin/auth/csrf'
)) {
    try {
        $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -ne 200) { $failures.Add("Unexpected HTTP status for ${uri}: $($response.StatusCode)") }
        [pscustomobject]@{ Uri = $uri; Status = $response.StatusCode }
    }
    catch {
        $failures.Add("Endpoint failed: $uri - $($_.Exception.Message)")
        [pscustomobject]@{ Uri = $uri; Status = 0 }
    }
}

$disk = Get-Volume -DriveLetter C
$freeDiskGb = [math]::Round($disk.SizeRemaining / 1GB, 2)
if ($freeDiskGb -lt $MinimumFreeDiskGb) { $failures.Add("Free disk is below threshold: $freeDiskGb GB") }

$certificate = Get-ChildItem Cert:\LocalMachine\WebHosting |
    Where-Object Subject -eq 'CN=peletnapechkai.com' |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1
$certificateDays = if ($certificate) { [math]::Floor(($certificate.NotAfter - (Get-Date)).TotalDays) } else { -1 }
if ($certificateDays -lt $MinimumCertificateDays) { $failures.Add("Certificate lifetime is below threshold: $certificateDays days") }

$result = [pscustomobject]@{
    CheckedAt = (Get-Date).ToString('o')
    Healthy = $failures.Count -eq 0
    Services = $services
    Endpoints = $endpoints
    FreeDiskGb = $freeDiskGb
    CertificateDaysRemaining = $certificateDays
    Failures = $failures
}

$result | ConvertTo-Json -Depth 5
if ($failures.Count -gt 0) { exit 1 }
exit 0
