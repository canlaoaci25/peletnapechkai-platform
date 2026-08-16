function Get-BoeclRequestFingerprint {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$RequestJson)

    $bytes = [Text.Encoding]::UTF8.GetBytes($RequestJson)
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try { return ([BitConverter]::ToString($sha256.ComputeHash($bytes))).Replace('-', '').ToLowerInvariant() }
    finally { $sha256.Dispose() }
}

function Save-BoeclRecoveryMetadata {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$ResultPath,
        [Parameter(Mandatory)][string]$RequestFingerprint
    )

    $metadataPath = "$ResultPath.request.json"
    $temporaryPath = "$metadataPath.tmp"
    @{ requestFingerprint = $RequestFingerprint } | ConvertTo-Json -Compress |
        Set-Content -LiteralPath $temporaryPath -Encoding UTF8
    Move-Item -LiteralPath $temporaryPath -Destination $metadataPath -Force
}

function Find-BoeclRecoveredResult {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$LogRoot,
        [Parameter(Mandatory)][string]$JobId,
        [Parameter(Mandatory)][string]$CurrentResultPath,
        [Parameter(Mandatory)][string]$RequestFingerprint
    )

    $results = Get-ChildItem -LiteralPath $LogRoot -Filter "$JobId-*-batch-*-result.json" -File -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -ne $CurrentResultPath } |
        Sort-Object LastWriteTimeUtc -Descending
    foreach ($result in $results) {
        $metadataPath = "$($result.FullName).request.json"
        if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) { continue }
        try {
            $metadata = Get-Content -LiteralPath $metadataPath -Raw -Encoding UTF8 | ConvertFrom-Json
            if ([string]$metadata.requestFingerprint -ceq $RequestFingerprint) { return $result.FullName }
        }
        catch { continue }
    }
    return $null
}
