# 重新编译 DSH 启动器（改完 src\DSHLauncher.cs 后运行本脚本）
# 依赖：Windows 自带的 .NET Framework csc.exe（v4.0.30319），无需安装 .NET SDK。
# 注意：编译前先关闭正在运行的 DSH.exe，否则会报 CS0016。
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) { $csc = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe" }
if (-not (Test-Path $csc)) { Write-Output "BUILD_FAIL: csc.exe not found"; exit 1 }

$args0 = @(
  "/nologo","/target:winexe","/codepage:65001","/platform:anycpu","/optimize+",
  "/out:$root\DSH.exe",
  "/resource:$root\build\avatar.png,WhaleAvatar.png",
  "/resource:$root\build\av_open.png,av_open.png",
  "/resource:$root\build\av_update.png,av_update.png",
  "/resource:$root\build\av_check.png,av_check.png",
  "/resource:$root\build\av_voice.png,av_voice.png",
  "/resource:$root\build\av_close.png,av_close.png",
  "/reference:System.dll","/reference:System.Drawing.dll","/reference:System.Windows.Forms.dll",
  "$root\src\DSHLauncher.cs"
)
$ico = Join-Path $root "build\whale-dsh.ico"
if (Test-Path $ico) { $args0 += "/win32icon:$ico" }

& $csc $args0
if (Test-Path "$root\DSH.exe") { Write-Output ("BUILD_OK " + (Get-Item "$root\DSH.exe").Length + " bytes") } else { Write-Output "BUILD_FAIL" }
