$ErrorActionPreference = 'Stop'
$programs = [Environment]::GetFolderPath('Programs')
$app = Join-Path $PSScriptRoot 'CuePane.exe'
$shortcut = Join-Path $programs 'CuePane.lnk'
$registration = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\CuePane'

try {
    if (Test-Path -LiteralPath $app) { Remove-Item -LiteralPath $app }
    if (Test-Path -LiteralPath $shortcut) { Remove-Item -LiteralPath $shortcut }
    if (Test-Path -LiteralPath $registration) { Remove-Item -LiteralPath $registration }
    Remove-Item -LiteralPath $PSCommandPath
    if (@(Get-ChildItem -LiteralPath $PSScriptRoot -Force).Count -eq 0) {
        Remove-Item -LiteralPath $PSScriptRoot
    }
} catch {
    Add-Type -AssemblyName System.Windows.Forms
    [void][System.Windows.Forms.MessageBox]::Show(
        "Close CuePane and retry if it is running. $($_.Exception.Message)", 'CuePane Uninstall')
    exit 1
}
