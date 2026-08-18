Set-StrictMode -Version Latest

function New-BoeclAutonomousWorktree {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Repository,[Parameter(Mandatory)][string]$WorktreeRoot,[Parameter(Mandatory)][int]$Cycle,[Parameter(Mandatory)][string]$Stamp,[Parameter(Mandatory)][string]$BaselineCommit)
    $repositoryPath = [IO.Path]::GetFullPath($Repository)
    $rootPath = [IO.Path]::GetFullPath($WorktreeRoot)
    New-Item -ItemType Directory -Path $rootPath -Force | Out-Null
    $safeStamp = $Stamp -replace '[^0-9A-Za-z-]', '-'
    $branch = "autonomous/cycle-$Cycle-$safeStamp"
    $path = [IO.Path]::GetFullPath((Join-Path $rootPath "cycle-$Cycle-$safeStamp"))
    if (-not $path.StartsWith($rootPath + [IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)) { throw 'Otonom worktree yolu izin verilen kokun disina cikti.' }
    if (Test-Path -LiteralPath $path) { throw "Otonom worktree zaten var: $path" }
    & git.exe -C $repositoryPath worktree add -b $branch $path $BaselineCommit | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Otonom worktree olusturulamadi.' }
    [pscustomobject]@{Path=$path;Branch=$branch;BaselineCommit=$BaselineCommit}
}

function Merge-BoeclAutonomousWorktree {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Repository,[Parameter(Mandatory)][psobject]$Context)
    if ((& git.exe -C $Repository rev-parse HEAD).Trim() -ne [string]$Context.BaselineCommit) { throw 'Ana dal cevrim sirasinda degisti; birlestirme durduruldu.' }
    if (@(& git.exe -C $Repository status --porcelain).Count -gt 0) { throw 'Ana dal temiz degil; birlestirme durduruldu.' }
    & git.exe -C $Repository merge --ff-only ([string]$Context.Branch) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Otonom dal fast-forward birlestirilemedi.' }
}

function Remove-BoeclAutonomousWorktree {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Repository,[Parameter(Mandatory)][psobject]$Context)
    & git.exe -C $Repository worktree remove ([string]$Context.Path) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Tamamlanan otonom worktree kaldirilamadi.' }
    & git.exe -C $Repository branch -d ([string]$Context.Branch) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Tamamlanan otonom dal kaldirilamadi.' }
}
