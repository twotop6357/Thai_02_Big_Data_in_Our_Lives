param([switch]$Diagnose, [switch]$PrepareOnly)
$ErrorActionPreference = 'Stop'
$runtimeDirectory = $PSScriptRoot
$packageDirectory = Split-Path -Parent $runtimeDirectory
$portablePython = Join-Path $runtimeDirectory 'python\python.exe'
try {
    $arguments = @('-I', (Join-Path $runtimeDirectory 'portable_boot.py'), '--root', $packageDirectory)
    if ($Diagnose) { $arguments += '--diagnose' }
    & $portablePython @arguments
    if ($LASTEXITCODE -ne 0) { throw "Runtime preparation/diagnostic failed. Exit code: $LASTEXITCODE" }
    if (-not $Diagnose -and -not $PrepareOnly) {
        $packageManifest = Get-Content -LiteralPath (Join-Path $runtimeDirectory 'package_manifest.json') -Raw -Encoding UTF8 | ConvertFrom-Json
        # The exhibit itself is interactive; its Comfy child process remains hidden through the existing launcher.
        Start-Process -FilePath (Join-Path $packageDirectory $packageManifest.player) -WorkingDirectory $packageDirectory -WindowStyle Normal
    }
    exit 0
} catch {
    $failure = $_ | Out-String
    $failure | Out-File -LiteralPath (Join-Path $runtimeDirectory 'logs\startup_error.txt') -Encoding utf8
    Write-Error $failure -ErrorAction Continue
    exit 1
}
