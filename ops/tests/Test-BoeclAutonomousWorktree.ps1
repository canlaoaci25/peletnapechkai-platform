[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot '..\windows\AutonomousWorktree.ps1')
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("boecl-worktree-{0}" -f [guid]::NewGuid().ToString('N'))
$repo = Join-Path $testRoot 'repo'; $worktrees = Join-Path $testRoot 'worktrees'; $failed = $null
New-Item -ItemType Directory -Path $repo -Force | Out-Null
try {
    & git.exe -C $repo init -b main | Out-Null
    & git.exe -C $repo config user.email 'autonomous-test@boecl.local'; & git.exe -C $repo config user.name 'BOECL Test'
    'baseline' | Set-Content -LiteralPath (Join-Path $repo 'proof.txt') -Encoding UTF8
    & git.exe -C $repo add proof.txt; & git.exe -C $repo commit -m baseline | Out-Null
    $baseline = (& git.exe -C $repo rev-parse HEAD).Trim()
    $context = New-BoeclAutonomousWorktree -Repository $repo -WorktreeRoot $worktrees -Cycle 1 -Stamp '20260818-000000' -BaselineCommit $baseline
    'cycle change' | Set-Content -LiteralPath (Join-Path $context.Path 'cycle.txt') -Encoding UTF8
    & git.exe -C $context.Path add cycle.txt; & git.exe -C $context.Path commit -m cycle | Out-Null
    if (@(& git.exe -C $repo status --porcelain).Count -ne 0) { throw 'Yalitilmis cevrim ana dali kirletti.' }
    Merge-BoeclAutonomousWorktree -Repository $repo -Context $context
    if (-not (Test-Path -LiteralPath (Join-Path $repo 'cycle.txt'))) { throw 'Dogrulanan cevrim ana dala birlestirilmedi.' }
    Remove-BoeclAutonomousWorktree -Repository $repo -Context $context
    $failed = New-BoeclAutonomousWorktree -Repository $repo -WorktreeRoot $worktrees -Cycle 2 -Stamp '20260818-000001' -BaselineCommit ((& git.exe -C $repo rev-parse HEAD).Trim())
    'unfinished' | Set-Content -LiteralPath (Join-Path $failed.Path 'unfinished.txt') -Encoding UTF8
    if (@(& git.exe -C $repo status --porcelain).Count -ne 0) { throw 'Basarisiz cevrim ana dali kirletti.' }
    Write-Host 'BOECL otonom worktree devamlılık testleri başarılı.'
} finally {
    if ($null -ne $failed -and (Test-Path -LiteralPath $failed.Path)) { & git.exe -C $repo worktree remove --force ([string]$failed.Path) | Out-Null }
    if (Test-Path -LiteralPath $testRoot) { Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue }
}
