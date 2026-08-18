[CmdletBinding()] param()
$ErrorActionPreference='Stop';$root=Join-Path ([IO.Path]::GetTempPath()) ('boecl-web-budget-'+[guid]::NewGuid().ToString('N'))
try{$next=Join-Path $root '.next';New-Item -ItemType Directory -Path (Join-Path $next 'static\chunks'),(Join-Path $next 'static\css') -Force|Out-Null
'{"rootMainFiles":["static/chunks/root.js"]}'|Set-Content -LiteralPath (Join-Path $next 'build-manifest.json') -Encoding UTF8
[IO.File]::WriteAllBytes((Join-Path $next 'static\chunks\root.js'),[byte[]]::new(32));[IO.File]::WriteAllBytes((Join-Path $next 'static\chunks\route.js'),[byte[]]::new(48));[IO.File]::WriteAllBytes((Join-Path $next 'static\css\public.css'),[byte[]]::new(16))
$gate=Join-Path $PSScriptRoot '..\windows\Test-WebReleaseBudget.ps1';$passed=& $gate -BuildRoot $root -MaximumRootJavaScriptBytes 64 -MaximumChunkBytes 64 -MaximumCssBytes 32|ConvertFrom-Json
if($LASTEXITCODE-ne 0-or-not $passed.Passed){throw 'Valid web release fixture did not pass.'}
& $gate -BuildRoot $root -MaximumRootJavaScriptBytes 16 -MaximumChunkBytes 64 -MaximumCssBytes 32|Out-Null;if($LASTEXITCODE-eq 0){throw 'Oversized root JavaScript fixture was accepted.'}
Remove-Item -LiteralPath (Join-Path $next 'static\chunks\root.js');try{& $gate -BuildRoot $root|Out-Null;throw 'Missing manifest asset was accepted.'}catch{}
Write-Host 'Web release budget regression tests passed.'}finally{Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue}
