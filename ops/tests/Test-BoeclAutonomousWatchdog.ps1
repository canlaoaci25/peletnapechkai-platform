$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\windows\AutonomousWatchdogCore.ps1')

$now = [DateTimeOffset]::Parse('2026-08-17T12:00:00Z')
function Assert-Equal($Expected, $Actual, [string]$Message) { if ($Expected -ne $Actual) { throw "$Message Beklenen=$Expected Gercek=$Actual" } }

Assert-Equal 'Disabled' (Get-BoeclWatchdogDecision -Enabled $false -TaskState Ready -CurrentStatus Completed -Heartbeat $now.ToString('o') -Now $now) 'Kapali mod baslatilmamali.'
Assert-Equal 'Healthy' (Get-BoeclWatchdogDecision -Enabled $true -TaskState Running -CurrentStatus Running -Heartbeat $now.AddMinutes(-1).ToString('o') -Now $now) 'Guncel calisma saglikli olmali.'
Assert-Equal 'RecoverStaleRun' (Get-BoeclWatchdogDecision -Enabled $true -TaskState Running -CurrentStatus Running -Heartbeat $now.AddMinutes(-11).ToString('o') -Now $now) 'Eski heartbeat kurtarilmali.'
Assert-Equal 'Start' (Get-BoeclWatchdogDecision -Enabled $true -TaskState Ready -CurrentStatus Running -Heartbeat $now.AddMinutes(-1).ToString('o') -Now $now) 'Kayip surec yeniden baslatilmali.'
Assert-Equal 'Backoff' (Get-BoeclWatchdogDecision -Enabled $true -TaskState Ready -CurrentStatus Failed -Heartbeat $now.ToString('o') -NextRetryAt $now.AddMinutes(5).ToString('o') -Now $now) 'Ana geri cekilme suresi korunmali.'
$budget = Test-BoeclRecoveryBudget -Recoveries @($now.AddMinutes(-5).ToString('o'),$now.AddMinutes(-10).ToString('o'),$now.AddMinutes(-20).ToString('o')) -Now $now
Assert-Equal $false $budget.Allowed 'Yeniden baslatma dongusu sinirlanmali.'
$expired = Test-BoeclRecoveryBudget -Recoveries @($now.AddMinutes(-31).ToString('o')) -Now $now
Assert-Equal $true $expired.Allowed 'Eski kurtarma kaydi butceyi tuketmemeli.'

'BOECL autonomous watchdog tests passed.'
