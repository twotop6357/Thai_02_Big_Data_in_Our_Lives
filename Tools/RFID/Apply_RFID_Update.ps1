param([string]$BuildRoot = $PSScriptRoot)
$ErrorActionPreference = 'Stop'
$targetRoot = [IO.Path]::GetFullPath($BuildRoot)
$dataRoot = Join-Path $targetRoot 'Unity-Startup-Template_Data'
$managedRoot = Join-Path $dataRoot 'Managed'
$settingsPath = Join-Path $dataRoot 'StreamingAssets\RFID\RfidSettings.json'
$payloadRoot = Join-Path $PSScriptRoot 'RFID_Update_Files'
$assemblies = @('Assembly-CSharp.dll', 'Exhibition.Blocks.Data.dll')
if (!(Test-Path -LiteralPath $settingsPath)) { throw 'Run this update against the existing Thai_02 build folder.' }
$unityPlayer = Join-Path $targetRoot 'UnityPlayer.dll'
if ((Get-FileHash -LiteralPath $unityPlayer -Algorithm SHA256).Hash -ne 'DC005461655B67912029EEA6BCC303FE244C3A336E2046107F934B901AB9CE2B') {
    throw 'This update requires the original Unity 2022.3.62f3 Windows build.'
}
foreach ($process in Get-Process) {
    try { $processPath = $process.Path } catch { continue }
    if ($processPath -and $processPath.StartsWith($targetRoot.TrimEnd('\') + '\', [StringComparison]::OrdinalIgnoreCase)) {
        throw 'Close the application completely before applying this update.'
    }
}
foreach ($name in $assemblies) {
    if (!(Test-Path -LiteralPath (Join-Path $payloadRoot $name)) -or !(Test-Path -LiteralPath (Join-Path $managedRoot $name))) {
        throw ('Missing required assembly: ' + $name)
    }
}
$configuration = Get-Content -LiteralPath $settingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
if ($configuration.version -ne 2 -or !$configuration.format -or $null -eq $configuration.cards) {
    throw 'The existing RFID settings must be a version 2 server configuration.'
}
# Change framing only. Preserve the site IPs, registered cards, removal values and all other settings.
$configuration.format.framing = 'UidBurst'
$replacement = ($configuration | ConvertTo-Json -Depth 32) + [Environment]::NewLine
$backupRoot = Join-Path $targetRoot ('RFID_Backup_' + (Get-Date -Format 'yyyyMMdd_HHmmss_fff'))
New-Item -ItemType Directory -Path $backupRoot | Out-Null
Copy-Item -LiteralPath $settingsPath -Destination (Join-Path $backupRoot 'RfidSettings.json')
foreach ($name in $assemblies) { Copy-Item -LiteralPath (Join-Path $managedRoot $name) -Destination (Join-Path $backupRoot $name) }
try {
    foreach ($name in $assemblies) { Copy-Item -LiteralPath (Join-Path $payloadRoot $name) -Destination (Join-Path $managedRoot $name) -Force }
    $temporaryPath = $settingsPath + '.rfid-update.tmp'
    [IO.File]::WriteAllText($temporaryPath, $replacement, (New-Object Text.UTF8Encoding($false)))
    [IO.File]::Replace($temporaryPath, $settingsPath, (Join-Path $backupRoot 'RfidSettings.replace.json'))
} catch {
    foreach ($name in $assemblies) { Copy-Item -LiteralPath (Join-Path $backupRoot $name) -Destination (Join-Path $managedRoot $name) -Force }
    Copy-Item -LiteralPath (Join-Path $backupRoot 'RfidSettings.json') -Destination $settingsPath -Force
    throw
}
Write-Output ('RFID update applied. Registered mappings preserved. Restart the application. Backup: ' + $backupRoot)
