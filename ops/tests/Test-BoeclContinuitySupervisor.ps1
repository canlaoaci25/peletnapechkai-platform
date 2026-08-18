[CmdletBinding()]
param()
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot '..\windows\ContinuitySupervisorCore.ps1')
$now=[DateTimeOffset]::Parse('2026-08-18T07:00:00Z')
if((Get-BoeclContinuityDecision -Kind Service -State Running -Now $now)-ne'Healthy'){throw 'Calisan servis sagliksiz sayildi.'}
if((Get-BoeclContinuityDecision -Kind Service -State Stopped -Now $now)-ne'Start'){throw 'Duran servis baslatilmadi.'}
if((Get-BoeclContinuityDecision -Kind Task -State Ready -Enabled:$false -Now $now)-ne'EnableAndStart'){throw 'Kapali gorev kurtarilmadi.'}
if((Get-BoeclContinuityDecision -Kind Task -State Running -LastResult 1 -Now $now)-ne'Healthy'){throw 'Calisan gorevin eski sonucu aktif hata sayildi.'}
if((Get-BoeclContinuityDecision -Kind Task -State Ready -LastRunAt $now.AddMinutes(-11) -MaximumSilenceMinutes 10 -Now $now)-ne'Start'){throw 'Geciken gorev algilanmadi.'}
$actions=1..3|ForEach-Object{[pscustomobject]@{component='worker';at=$now.AddMinutes(-$_).ToString('o')}}
if((Test-BoeclContinuityRecoveryBudget -Actions $actions -Component worker -Now $now).Allowed){throw 'Kurtarma dongusu sinirlanmadi.'}
if(-not(Test-BoeclContinuityRecoveryBudget -Actions $actions -Component sitemap -Now $now).Allowed){throw 'Baska bilesen gereksiz sinirlandi.'}
Write-Host 'BOECL continuity supervisor testleri basarili.'
