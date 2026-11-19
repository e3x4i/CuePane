param([string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'))
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw '.NET Framework compiler not found.' }
$directory = (New-Item -ItemType Directory -Force -Path $OutputDirectory).FullName
$app = Join-Path $directory 'CuePane.exe'
$setup = Join-Path $directory 'CuePane-Setup.exe'
$icon = Join-Path $PSScriptRoot 'assets\cuepane.ico'
$source = Join-Path $PSScriptRoot 'CuePane.cs'
$setupSource = Join-Path $PSScriptRoot 'Setup.cs'
$uninstall = Join-Path $PSScriptRoot 'Uninstall.ps1'

& $compiler /nologo /optimize+ /platform:x64 /target:winexe "/win32icon:$icon" "/out:$app" /reference:System.Security.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Net.Http.dll /reference:System.Web.Extensions.dll $source
if ($LASTEXITCODE -ne 0) { throw 'CuePane compilation failed.' }

& $compiler /nologo /optimize+ /platform:x64 /target:winexe "/win32icon:$icon" "/out:$setup" /reference:System.Windows.Forms.dll /reference:Microsoft.CSharp.dll "/resource:$app,CuePane.exe" "/resource:$uninstall,Uninstall.ps1" $setupSource
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }

