$ErrorActionPreference = 'Stop'

$projectDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$compilerCandidates = @(
    "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe",
    "$env:WINDIR\Microsoft.NET\Framework\v4.0.30319\csc.exe"
)
$compiler = $compilerCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1

if (-not $compiler) {
    throw '找不到 Windows .NET Framework C# 编译器。'
}

& $compiler /nologo /target:winexe /optimize+ /platform:anycpu `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    "/resource:$projectDirectory\assets\break-background.jpg,BreakReminder.Background.jpg" `
    "/win32manifest:$projectDirectory\app.manifest" `
    "/out:$projectDirectory\BreakReminder.exe" `
    "$projectDirectory\BreakReminder.cs"

if ($LASTEXITCODE -ne 0) {
    throw "编译失败，退出码：$LASTEXITCODE"
}

Write-Host "已生成：$projectDirectory\BreakReminder.exe"
