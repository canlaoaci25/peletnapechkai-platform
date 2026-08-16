[CmdletBinding()]
param(
    [string]$StatePath = 'C:\ProgramData\Peletnapechkai\Autonomous\state.json',
    [int]$RefreshSeconds = 2
)

$host.UI.RawUI.WindowTitle = 'BOECL AI - Canli Otonom Sistem'
function Select-FirstValue { param([object[]]$Values) foreach ($value in $Values) { if ($null -ne $value -and "$value" -ne '') { return $value } } return '-' }
while ($true) {
    Clear-Host
    Write-Host 'BOECL AI CANLI OTONOM SISTEM' -ForegroundColor Cyan
    Write-Host ('Guncelleme: ' + (Get-Date -Format 'dd.MM.yyyy HH:mm:ss')) -ForegroundColor DarkGray
    Write-Host ('-' * 90)
    if (-not (Test-Path -LiteralPath $StatePath)) {
        Write-Host 'Durum dosyasi bekleniyor...' -ForegroundColor Yellow
        Start-Sleep -Seconds $RefreshSeconds
        continue
    }
    try { $state = Get-Content -Raw -LiteralPath $StatePath -Encoding UTF8 | ConvertFrom-Json }
    catch {
        Write-Host 'Durum dosyasi guncelleniyor...' -ForegroundColor Yellow
        Start-Sleep -Seconds $RefreshSeconds
        continue
    }
    $color = if (-not $state.enabled) { 'Red' } elseif ($state.currentStatus -eq 'Running') { 'Green' } elseif ($state.currentStatus -eq 'Failed') { 'Red' } else { 'Yellow' }
    Write-Host ("Otonom mod : " + $(if ($state.enabled) { 'ACIK' } else { 'KAPALI' })) -ForegroundColor $color
    Write-Host ("Cevrim      : " + (Select-FirstValue @($state.currentCycle,$state.cycle,0)))
    Write-Host ("Durum       : " + (Select-FirstValue @($state.currentStatus,$state.lastResult,'Bekliyor'))) -ForegroundColor $color
    Write-Host ("Odak        : " + (Select-FirstValue @($state.currentFocus,'Siradaki cevrim bekleniyor')))
    Write-Host ("Son sonuc   : " + (Select-FirstValue @($state.lastResult,'-')))
    Write-Host ('-' * 90)
    $eventPath = [string]$state.currentEventLog
    if ($eventPath -and (Test-Path -LiteralPath $eventPath)) {
        Write-Host 'SON CODEX OLAYLARI' -ForegroundColor Cyan
        $events = Get-Content -LiteralPath $eventPath -Tail 18 -Encoding UTF8 -ErrorAction SilentlyContinue
        foreach ($line in $events) {
            try {
                $event = $line | ConvertFrom-Json
                if ($event.type -eq 'item.completed' -and $event.item.type -eq 'agent_message') {
                    Write-Host ('AI: ' + $event.item.text) -ForegroundColor White
                }
                elseif ($event.type -in @('item.started','item.completed') -and $event.item.type -eq 'command_execution') {
                    $prefix = if ($event.type -eq 'item.started') { 'KOMUT BASLADI' } else { "KOMUT BITTI ($($event.item.exit_code))" }
                    Write-Host ("${prefix}: " + $event.item.command) -ForegroundColor DarkCyan
                }
                elseif ($event.type -in @('turn.completed','turn.failed')) {
                    Write-Host ("CEVRIM: " + $event.type) -ForegroundColor $color
                }
            } catch { }
        }
    } else {
        Write-Host 'Yeni cevrim olaylari bekleniyor...' -ForegroundColor DarkGray
    }
    Write-Host ('-' * 90)
    Write-Host 'Bu pencereyi kapatmak sistemi durdurmaz. Cikis: Ctrl+C' -ForegroundColor DarkGray
    Start-Sleep -Seconds $RefreshSeconds
}
