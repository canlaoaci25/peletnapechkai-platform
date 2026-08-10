[CmdletBinding()]
param(
    [string]$RepositoryPath = 'C:\Users\Administrator\Desktop\peletnapechkai-platform',
    [string]$LogRoot = 'C:\ProgramData\Peletnapechkai\Logs\QualityAudit'
)

$ErrorActionPreference = 'Stop'
$repository = (Resolve-Path -LiteralPath $RepositoryPath).Path
if ($repository -ne 'C:\Users\Administrator\Desktop\peletnapechkai-platform') { throw "Unexpected repository path: $repository" }
New-Item -ItemType Directory -Path $LogRoot -Force | Out-Null
$log = Join-Path $LogRoot "quality-$((Get-Date).ToString('yyyyMMdd-HHmmss')).log"
$mutex = [Threading.Mutex]::new($false, 'Global\BOECL-Weekly-Quality-Audit')
if (-not $mutex.WaitOne(0)) { exit 0 }

try {
    Push-Location $repository
    $checks = @(
        @{ Name = 'Locale consistency'; Command = 'npm.cmd'; Arguments = @('run', 'check:locales') },
        @{ Name = 'Web lint'; Command = 'npm.cmd'; Arguments = @('run', 'lint') },
        @{ Name = 'Web typecheck'; Command = 'npm.cmd'; Arguments = @('run', 'typecheck') },
        @{ Name = 'Web production build'; Command = 'npm.cmd'; Arguments = @('run', 'build:web') },
        @{ Name = 'API tests'; Command = 'dotnet.exe'; Arguments = @('test', 'Peletnapechkai.slnx', '--configuration', 'Release') },
        @{ Name = '.NET Release build'; Command = 'dotnet.exe'; Arguments = @('build', 'Peletnapechkai.slnx', '--configuration', 'Release') },
        @{ Name = 'Staging health'; Command = 'powershell.exe'; Arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'ops\windows\Test-StagingHealth.ps1') },
        @{ Name = 'Production health'; Command = 'powershell.exe'; Arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', 'ops\windows\Test-ProductionHealth.ps1') }
    )
    foreach ($check in $checks) {
        "[$(Get-Date -Format o)] START $($check.Name)" | Add-Content -LiteralPath $log -Encoding UTF8
        $arguments = $check.Arguments
        $savedErrorPreference = $ErrorActionPreference
        $ErrorActionPreference = 'Continue'
        & $check.Command @arguments 2>&1 | Add-Content -LiteralPath $log -Encoding UTF8
        $checkExitCode = $LASTEXITCODE
        $ErrorActionPreference = $savedErrorPreference
        if ($checkExitCode -ne 0) { throw "Quality audit failed: $($check.Name) (exit $checkExitCode)" }
        "[$(Get-Date -Format o)] PASS $($check.Name)" | Add-Content -LiteralPath $log -Encoding UTF8
    }
    "[$(Get-Date -Format o)] COMPLETE commit=$((& git.exe rev-parse HEAD).Trim())" | Add-Content -LiteralPath $log -Encoding UTF8
}
catch {
    "[$(Get-Date -Format o)] FAILURE $($_.Exception.Message)" | Add-Content -LiteralPath $log -Encoding UTF8
    Write-EventLog -LogName Application -Source 'BOECL Quality Audit' -EntryType Error -EventId 4101 -Message $_.Exception.Message -ErrorAction SilentlyContinue
    throw
}
finally {
    Pop-Location -ErrorAction SilentlyContinue
    $mutex.ReleaseMutex()
    $mutex.Dispose()
}
