param(
    [string]$ProjectRoot = 'C:\Users\Administrator\Desktop\peletnapechkai-platform'
)

$checks = [System.Collections.Generic.List[pscustomobject]]::new()

function Add-Check {
    param(
        [string]$Name,
        [string]$Result,
        [string]$Details
    )
    $checks.Add([pscustomobject]@{ Name=$Name; Result=$Result; Details=$Details })
}

function Test-ServiceState {
    param([string]$Name)
    $svc = Get-Service -Name $Name -ErrorAction SilentlyContinue
    if (-not $svc) { return @{Result='PASS';Details='Not installed'} }
    if ($svc.Status -eq 'Stopped') { return @{Result='PASS';Details='Stopped'} }
    return @{Result='FAIL';Details="Status=$($svc.Status)"}
}

function Test-TaskState {
    param([string]$Name)
    $task = Get-ScheduledTask -TaskName $Name -ErrorAction SilentlyContinue
    if (-not $task) { return @{Result='PASS';Details='Not installed'} }
    if ($task.State -in @('Disabled','Ready')) {
        return @{Result='PASS';Details="State=$($task.State)"}
    }
    return @{Result='WARNING';Details="State=$($task.State)"}
}

function Add-PathCheck {
    param([string]$Path)
    if (Test-Path $Path) {
        Add-Check "Path exists: $Path" 'WARNING' 'REVIEW REQUIRED for shared resources before deletion'
    }
    else {
        Add-Check "Path exists: $Path" 'PASS' 'Not present'
    }
}

function Test-WebSiteState {
    param([string]$Name)
    try {
        $obj = Get-Website -Name $Name -ErrorAction Stop
        if ($obj) {
            if ($obj.State.ToString() -eq 'Started') { return @{ Result='FAIL'; Details=\"State=$($obj.State)\" } }
            return @{ Result='PASS'; Details=\"State=$($obj.State)\" }
        }
        return @{ Result='PASS'; Details='Not installed' }
    } catch [Microsoft.IIs.PowerShell.Provider.IisConfigurationException] {
        return @{ Result='PASS'; Details='Not installed' }
    } catch {
        return @{ Result='REVIEW REQUIRED'; Details=$_.Exception.Message }
    }
}

function Test-AppPoolState {
    param([string]$Name)
    try {
        $stateObj = Get-WebAppPoolState -Name $Name -ErrorAction Stop
        if ($stateObj) {
            if ($stateObj.Value.ToString() -ne 'Stopped') {
                return @{ Result='WARNING'; Details=\"State=$($stateObj.Value)\" }
            }
            return @{ Result='PASS'; Details=\"State=$($stateObj.Value)\" }
        }
        return @{ Result='PASS'; Details='Not installed' }
    } catch {
        $itemNotFoundType = 'System.Management.Automation.ItemNotFoundException'
        if ($_.Exception.GetType().FullName -eq $itemNotFoundType) {
            return @{ Result='PASS'; Details='Not installed' }
        }

        return @{ Result='REVIEW REQUIRED'; Details=$_.Exception.Message }
    }
}

Add-Check 'Windows Service PeletnapechkaiWeb' (Test-ServiceState 'PeletnapechkaiWeb').Result (Test-ServiceState 'PeletnapechkaiWeb').Details
Add-Check 'Windows Service BoeclStagingWeb' (Test-ServiceState 'BoeclStagingWeb').Result (Test-ServiceState 'BoeclStagingWeb').Details
Add-Check 'Windows Service PostgreSQL' (Test-ServiceState 'postgresql-x64-18').Result (Test-ServiceState 'postgresql-x64-18').Details
Add-Check 'Windows Service W3SVC' (Test-ServiceState 'W3SVC').Result (Test-ServiceState 'W3SVC').Details

$taskNames = @(
    'BOECL Autonomous Improvement',
    'BOECL Autonomous Watchdog',
    'BOECL Codex Automation Worker',
    'BOECL Continuity Supervisor',
    'BOECL Hourly Sitemap Text',
    'BOECL Weekly Quality Audit',
    'BOECL - Staging Health',
    'Peletnapechkai - Production Health',
    'Peletnapechkai - PostgreSQL Backup'
)
foreach($task in $taskNames){
    $c = Test-TaskState $task
    Add-Check "Scheduled Task: $task" $c.Result $c.Details
}

try {
    Import-Module WebAdministration -ErrorAction Stop

    foreach($site in @('Default Web Site','Peletnapechkai API','BOECL Staging','BOECL Staging API')) {
        $c = Test-WebSiteState -Name $site
        Add-Check "IIS Site: $site" $c.Result $c.Details
    }

    foreach($pool in @('PeletnapechkaiApiPool','BoeclStagingApiPool')) {
        $c = Test-AppPoolState -Name $pool
        Add-Check "IIS AppPool: $pool" $c.Result $c.Details
    }

    foreach($path in @(
        'C:\inetpub\peletnapechkai',
        'C:\inetpub\boecl-staging',
        'C:\ProgramData\Peletnapechkai',
        'C:\ProgramData\BOECL'
    )) {
        Add-PathCheck -Path $path
    }
} catch {
    Add-Check 'IIS state checks' 'REVIEW REQUIRED' "IIS block failed: $($_.Exception.GetType().FullName): $($_.Exception.Message)"
}

try {
    $ports = @(3000,3001,3002,3003,3100,3101,3102,3103,5080,80,443)
    $listeners = Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue
    foreach($p in $ports){
        $l = $listeners | Where-Object LocalPort -eq $p
        if ($l) {
            $pids = [string]::Join(',', ($l.OwningProcess | Select-Object -Unique))
            Add-Check "Port listening: $p" 'WARNING' "Listening PIDs: $pids"
        } else {
            Add-Check "Port listening: $p" 'PASS' 'No listeners'
        }
    }
} catch {
    Add-Check 'Network listeners' 'REVIEW REQUIRED' $_.Exception.Message
}

$envScope = @('Machine','User')
$envKeys = @(
    'NEXT_PUBLIC_SITE_URL',
    'NEXT_PUBLIC_API_URL',
    'ASPNETCORE_ENVIRONMENT',
    'ASPNETCORE_URLS',
    'ConnectionStrings__Database',
    'ConnectionStrings__DatabaseMigration',
    'DataProtection__KeysPath'
)

foreach($scope in $envScope){
    foreach($key in $envKeys){
        if ($scope -eq 'Machine') {
            $v = [Environment]::GetEnvironmentVariable($key, [EnvironmentVariableTarget]::Machine)
        } else {
            $v = [Environment]::GetEnvironmentVariable($key, [EnvironmentVariableTarget]::User)
        }
        if ($v) {
            Add-Check "$scope env: $key" 'WARNING' 'Value present on host'
        }
        else {
            Add-Check "$scope env: $key" 'PASS' 'Not present'
        }
    }
}

$recentDeploy = Join-Path $ProjectRoot '.artifacts'
if (Test-Path $recentDeploy) {
    Add-Check '.artifacts folder' 'PASS' "Present: $recentDeploy"
}
else {
    Add-Check '.artifacts folder' 'PASS' 'Not present'
}

foreach($c in $checks){
    Write-Output ("{0} | {1} | {2}" -f $c.Name,$c.Result,$c.Details)
}

$counts = $checks | Group-Object -Property Result
$fail = $counts | Where-Object Name -eq 'FAIL'
$warn = $counts | Where-Object Name -eq 'WARNING'
$review = $counts | Where-Object Name -eq 'REVIEW REQUIRED'

"`nSummary:"
if ($fail) { "FAIL: $($fail.Count)" } else { 'FAIL: 0' }
if ($warn) { "WARNING: $($warn.Count)" } else { 'WARNING: 0' }
if ($review) { "REVIEW REQUIRED: $($review.Count)" } else { 'REVIEW REQUIRED: 0' }
