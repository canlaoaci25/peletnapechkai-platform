[CmdletBinding()]
param([string]$StateRoot='C:\ProgramData\Peletnapechkai\Continuity')

$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'ContinuitySupervisorCore.ps1')
$statePath=Join-Path $StateRoot 'state.json'
$logPath=Join-Path $StateRoot 'events.jsonl'
$lockStream=$null
$now=[DateTimeOffset]::UtcNow
$components=[Collections.Generic.List[object]]::new()
$newActions=[Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $StateRoot -Force | Out-Null
$previous=if(Test-Path -LiteralPath $statePath){try{Get-Content -Raw -LiteralPath $statePath -Encoding UTF8|ConvertFrom-Json}catch{$null}}else{$null}
$history=if($null -ne $previous -and $previous.PSObject.Properties.Name -contains 'actions'){@($previous.actions)}else{@()}

function Add-Component([string]$Name,[string]$Kind,[string]$Status,[string]$Decision,[string]$Message){
    $components.Add([ordered]@{name=$Name;kind=$Kind;status=$Status;decision=$Decision;message=$Message})
}
function Invoke-Recovery([string]$Name,[scriptblock]$Action){
    $budget=Test-BoeclContinuityRecoveryBudget -Actions $history -Component $Name -Now $now
    if(-not $budget.Allowed){return 'Throttled'}
    & $Action
    $entry=[ordered]@{at=$now.ToString('o');component=$Name;result='Recovered'}
    $newActions.Add($entry); $entry|ConvertTo-Json -Compress|Add-Content -LiteralPath $logPath -Encoding UTF8
    'Recovered'
}

try{
    try{$lockStream=[IO.File]::Open((Join-Path $StateRoot 'supervisor.lock'),[IO.FileMode]::OpenOrCreate,[IO.FileAccess]::ReadWrite,[IO.FileShare]::None)}catch [IO.IOException]{exit 0}

    foreach($name in @('W3SVC','postgresql-x64-18','PeletnapechkaiWeb','BoeclStagingWeb')){
        $service=Get-Service -Name $name -ErrorAction SilentlyContinue
        if($null -eq $service){Add-Component $name 'Service' 'Missing' 'Manual' 'Servis bulunamadi.';continue}
        $decision=Get-BoeclContinuityDecision -Kind Service -State ([string]$service.Status)
        $result=if($decision -eq 'Start'){Invoke-Recovery $name {Start-Service -Name $name -ErrorAction Stop}}else{'Healthy'}
        Add-Component $name 'Service' ([string](Get-Service -Name $name).Status) $result 'Windows servisi denetlendi.'
    }

    Import-Module WebAdministration
    foreach($name in @('Default Web Site','Peletnapechkai API','BOECL Staging API','BOECL Staging')){
        $site=Get-Website -Name $name -ErrorAction SilentlyContinue
        if($null -eq $site){Add-Component $name 'Site' 'Missing' 'Manual' 'IIS sitesi bulunamadi.';continue}
        $decision=Get-BoeclContinuityDecision -Kind Site -State ([string]$site.State)
        $result=if($decision -eq 'Start'){Invoke-Recovery $name {Start-Website -Name $name}}else{'Healthy'}
        Add-Component $name 'Site' ([string](Get-Website -Name $name).State) $result 'IIS sitesi denetlendi.'
    }
    foreach($name in @('DefaultAppPool','PeletnapechkaiApiPool','BoeclStagingApiPool')){
        $pool=Get-Item "IIS:\AppPools\$name" -ErrorAction SilentlyContinue
        if($null -eq $pool){Add-Component $name 'AppPool' 'Missing' 'Manual' 'IIS uygulama havuzu bulunamadi.';continue}
        $state=[string]$pool.State
        $decision=Get-BoeclContinuityDecision -Kind AppPool -State $state
        $result=if($decision -eq 'Start'){Invoke-Recovery $name {Start-WebAppPool -Name $name}}else{'Healthy'}
        Add-Component $name 'AppPool' ([string](Get-Item "IIS:\AppPools\$name").State) $result 'IIS uygulama havuzu denetlendi.'
    }

    $workerTask=Get-ScheduledTask -TaskName 'BOECL Codex Automation Worker' -ErrorAction SilentlyContinue
    $workerInstallRoot='C:\ProgramData\Peletnapechkai\AutomationWorker'
    $workerFiles=@('Invoke-BoeclCodexWorker.ps1','BoeclAutomationRecovery.ps1')
    $integrityHealthy=$true
    foreach($file in $workerFiles){
        $source=Join-Path $PSScriptRoot $file; $installed=Join-Path $workerInstallRoot $file
        if(-not(Test-Path -LiteralPath $source -PathType Leaf)){continue}
        $matches=(Test-Path -LiteralPath $installed -PathType Leaf) -and ((Get-FileHash -LiteralPath $source).Hash -eq (Get-FileHash -LiteralPath $installed).Hash)
        if($matches){continue}
        if($null -ne $workerTask -and $workerTask.State -notin @('Running','Queued')){
            $result=Invoke-Recovery "Worker file: $file" {Copy-Item -LiteralPath $source -Destination $installed -Force}
            if($result -ne 'Recovered'){$integrityHealthy=$false}
        }
        else{$integrityHealthy=$false}
    }
    Add-Component 'Codex worker dosya butunlugu' 'FileSet' $(if($integrityHealthy){'Healthy'}else{'Mismatch'}) $(if($integrityHealthy){'Healthy'}else{'Manual'}) 'Worker ve kurtarma bagimliliklari hash ile denetlendi.'

    $taskPolicies=@(
        @{Name='BOECL Autonomous Watchdog';Max=6},@{Name='BOECL Codex Automation Worker';Max=5},
        @{Name='BOECL - Staging Health';Max=10},@{Name='Peletnapechkai - Production Health';Max=10},
        @{Name='BOECL Hourly Sitemap Text';Max=90},@{Name='Peletnapechkai - PostgreSQL Backup';Max=1560},
        @{Name='BOECL Weekly Quality Audit';Max=11520}
    )
    foreach($policy in $taskPolicies){
        $task=Get-ScheduledTask -TaskName $policy.Name -ErrorAction SilentlyContinue
        $info=Get-ScheduledTaskInfo -TaskName $policy.Name -ErrorAction SilentlyContinue
        if($null -eq $task -or $null -eq $info){Add-Component $policy.Name 'Task' 'Missing' 'Manual' 'Zamanlanmis gorev bulunamadi.';continue}
        $last=if($info.LastRunTime -gt [datetime]::MinValue){[DateTimeOffset]$info.LastRunTime}else{$null}
        $next=if($info.NextRunTime -gt [datetime]::MinValue){[DateTimeOffset]$info.NextRunTime}else{$null}
        $decision=Get-BoeclContinuityDecision -Kind Task -State ([string]$task.State) -Enabled ([bool]$task.Settings.Enabled) -LastRunAt $last -NextRunAt $next -LastResult ([long]$info.LastTaskResult) -MaximumSilenceMinutes ([int]$policy.Max) -Now $now
        $result='Healthy'
        if($decision -in @('Start','EnableAndStart')){
            $result=Invoke-Recovery $policy.Name {
                if($decision -eq 'EnableAndStart'){Enable-ScheduledTask -TaskName $policy.Name|Out-Null}
                Start-ScheduledTask -TaskName $policy.Name -ErrorAction Stop
            }
        }
        Add-Component $policy.Name 'Task' ([string](Get-ScheduledTask -TaskName $policy.Name).State) $result "Son sonuc: $([long]$info.LastTaskResult)"
    }

    $recentActions=@(foreach($action in @($history)+@($newActions)){
        $parsed=[DateTimeOffset]::MinValue
        if([DateTimeOffset]::TryParse([string]$action.at,[ref]$parsed) -and ($now-$parsed).TotalHours -le 24){$action}
    })
    $allActions=@($recentActions|Select-Object -Last 100)
    $unhealthy=@($components|Where-Object{$_.status -in @('Missing','Stopped') -or $_.decision -in @('Manual','Throttled')})
    $payload=[ordered]@{checkedAt=$now.ToString('o');healthy=$unhealthy.Count -eq 0;componentCount=$components.Count;recoveredCount=$newActions.Count;components=$components;actions=$allActions}
    $temporary="$statePath.$([guid]::NewGuid().ToString('N')).tmp"
    $payload|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $temporary -Encoding UTF8
    Move-Item -LiteralPath $temporary -Destination $statePath -Force
    if($unhealthy.Count -gt 0){exit 2}
}
catch{
    [ordered]@{at=$now.ToString('o');component='supervisor';result='Failed';message=$_.Exception.Message}|ConvertTo-Json -Compress|Add-Content -LiteralPath $logPath -Encoding UTF8
    exit 1
}
finally{if($null -ne $lockStream){$lockStream.Dispose()}}
