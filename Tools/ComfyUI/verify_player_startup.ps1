param([string]$PlayerDirectory = 'Builds/ComfyAutostart', [int]$Port = 18194)
$ErrorActionPreference = 'Stop'
$playerPath = (Resolve-Path (Join-Path $PlayerDirectory 'Thai02.exe')).Path
$settingsPath = Join-Path $PlayerDirectory 'Thai02_Data/StreamingAssets/BlockGeneration/ComfyUISettings.json'
$reportDirectory = (New-Item -ItemType Directory -Force 'docs/validation/comfy_autostart').FullName
$settingsOriginal = [IO.File]::ReadAllText((Resolve-Path $settingsPath))
$settings = $settingsOriginal | ConvertFrom-Json
$settings.serverArguments = $settings.serverArguments.Replace('--port 8188', "--port $Port")
$settings.serverUrl = "http://127.0.0.1:$Port"
$settings.launchServerInEditor = $true
$ownedPlayer = $null
$ownedServers = [Collections.Generic.List[int]]::new()
$checks = [Collections.Generic.List[string]]::new()
$desktopBefore = @(Get-Process -Name 'Comfy Desktop' -ErrorAction SilentlyContinue | ForEach-Object Id)
$report = [ordered]@{task='COMFY-AUTOSTART-002'; realWindowsPlayer=$true; headlessPlayer=$true; port=$Port; checks=$checks; passed=$false}
function Save-Settings { [IO.File]::WriteAllText($settingsPath, ($settings | ConvertTo-Json -Depth 10)) }
function Check([bool]$condition, [string]$label) { if(!$condition){throw $label}; $checks.Add($label) }
function Start-Player([string]$name) {
 $script:playerLog = Join-Path $reportDirectory "$name.log"
 if(Test-Path -LiteralPath $script:playerLog){Remove-Item -LiteralPath $script:playerLog}
 $script:ownedPlayer = Start-Process -FilePath $playerPath -ArgumentList @('-batchmode','-nographics','-logFile',"`"$script:playerLog`"") -WindowStyle Hidden -PassThru
}
function Stop-Player {
 if($script:ownedPlayer -and !( $script:ownedPlayer.HasExited)) {
  $process = Get-Process -Id $script:ownedPlayer.Id -ErrorAction Stop
  if($process.Path -ne $playerPath){throw 'Player identity changed'}
  Stop-Process -Id $process.Id
  $script:ownedPlayer.WaitForExit(10000) | Out-Null
 }
 $script:ownedPlayer=$null
}
function Wait-Log([string]$pattern, [int]$seconds = 120) {
 $deadline = [DateTime]::UtcNow.AddSeconds($seconds)
 do {
  if(Test-Path $script:playerLog) {
   $stream=[IO.File]::Open($script:playerLog,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
   $reader=[IO.StreamReader]::new($stream)
   try {$body=$reader.ReadToEnd()} finally {$reader.Dispose()}
   if($body -match $pattern){return $body}
  }
  if($script:ownedPlayer.HasExited){throw 'Player exited before expected server state'}
  Start-Sleep -Milliseconds 250
 } while([DateTime]::UtcNow -lt $deadline)
 throw "Expected player log was not observed: $pattern"
}
function Stop-OwnedServers {
 foreach($ownedId in $script:ownedServers) {
  $process=Get-Process -Id $ownedId -ErrorAction SilentlyContinue
  if($process -and $process.ProcessName -eq 'python'){Stop-Process -Id $ownedId}
 }
 $script:ownedServers.Clear()
}
try {
 Check (!(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)) 'Isolated test port is unused; production server is preserved'
 Save-Settings
 $watch=[Diagnostics.Stopwatch]::StartNew(); Start-Player 'player_cold'
 $body=Wait-Log 'ComfyUI server is ready\.'
 $report.coldReadySeconds=$watch.Elapsed.TotalSeconds
 Check ($body -match 'Started server process (\d+)\.') 'Player directly starts Python backend without launching Desktop'
 $startedId=[int]$Matches[1]; $ownedServers.Add($startedId)
 $listener=Get-NetTCPConnection -LocalPort $Port -State Listen
 if($listener.OwningProcess -ne $startedId){$ownedServers.Add($listener.OwningProcess)}
 $child=Get-CimInstance Win32_Process -Filter "ProcessId=$($listener.OwningProcess)"
 $parent=Get-CimInstance Win32_Process -Filter "ProcessId=$startedId"
 Check ($parent.ParentProcessId -eq $ownedPlayer.Id -and ($child.ProcessId -eq $startedId -or $child.ParentProcessId -eq $startedId)) 'Backend parent chain originates in the actual Unity Player'
 $stats=Invoke-RestMethod "$($settings.serverUrl)/system_stats"
 Check ($null -ne $stats.system) 'Cold backend responds to the ComfyUI API'
 $playerTcp=@(Get-NetTCPConnection -OwningProcess $ownedPlayer.Id -State Listen -ErrorAction SilentlyContinue)
 $playerUdp=@(Get-NetUDPEndpoint -OwningProcess $ownedPlayer.Id -ErrorAction SilentlyContinue)
 $report.playerListeningTcpPorts=@($playerTcp | ForEach-Object LocalPort)
 $report.playerUdpPorts=@($playerUdp | ForEach-Object LocalPort)
 Check ($listener.LocalAddress -eq '127.0.0.1') 'Backend accepts local connections only'
 $report.backendVersion=$stats.system.comfyui_version
 $backendId=$listener.OwningProcess; Stop-Player
 Check ([bool](Get-Process -Id $backendId -ErrorAction SilentlyContinue)) 'Server survives Player exit for reuse'
 Start-Player 'player_reuse'; $body=Wait-Log 'ComfyUI server is already running\.' 30
 Check ($body -notmatch 'Started server process') 'Second Player run reuses the running server'
 Check ((Get-NetTCPConnection -LocalPort $Port -State Listen).OwningProcess -eq $backendId) 'Reuse preserves the existing backend process'
 Stop-Player; Stop-OwnedServers
 $originalExecutable=$settings.serverExecutable
 $settings.serverExecutable=Join-Path $reportDirectory 'missing-python.exe'; Save-Settings
 Start-Player 'player_failure'; $null=Wait-Log 'Failed to start ComfyUI:' 30
 Check (!(Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)) 'Missing Python is reported without a server or crash'
 Check (!$ownedPlayer.HasExited) 'Player remains responsive after launch failure'
 Stop-Player
 $settings.serverExecutable=$originalExecutable
 $desktopAfter=@(Get-Process -Name 'Comfy Desktop' -ErrorAction SilentlyContinue | ForEach-Object Id)
 Check (@($desktopAfter | Where-Object {$_ -notin $desktopBefore}).Count -eq 0) 'No Desktop GUI process was launched during any test'
 $report.passed=$true
} catch { $report.error=$_.Exception.ToString(); throw }
finally {
 Stop-Player; Stop-OwnedServers
 [IO.File]::WriteAllText($settingsPath,$settingsOriginal)
 [IO.File]::WriteAllText((Join-Path $reportDirectory 'player.json'),($report|ConvertTo-Json -Depth 10))
}
$report | ConvertTo-Json -Depth 10
