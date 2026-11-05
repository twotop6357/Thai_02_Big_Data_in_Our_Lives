param([string]$PlayerDirectory='Builds/ComfyAutostart')
$ErrorActionPreference='Stop'
$playerPath=(Resolve-Path (Join-Path $PlayerDirectory 'Thai02.exe')).Path
$settingsPath=(Resolve-Path (Join-Path $PlayerDirectory 'Thai02_Data/StreamingAssets/Settings.json')).Path
$original=[IO.File]::ReadAllText($settingsPath)
$settings=$original|ConvertFrom-Json
$output=(New-Item -ItemType Directory -Force 'docs/validation/content_log').FullName
$recordPath=(Resolve-Path 'Temp/ContentLog/requests.jsonl').Path
$ownedPlayer=$null
$checks=[Collections.Generic.List[string]]::new()
$report=[ordered]@{task='LOG-003';realWindowsPlayer=$true;receiver='loopback fixture';checks=$checks;passed=$false}
function Records([string]$path){
 $rows=@(Get-Content -LiteralPath $recordPath | ForEach-Object {try{$_|ConvertFrom-Json}catch{}} | Where-Object {$_.path -eq $path})
 return ,$rows
}
function Check([bool]$ok,[string]$label){if(!$ok){throw $label};$checks.Add($label)}
function Wait-Records([string]$path,[int]$count,[int]$timeout=30){
 $deadline=[DateTime]::UtcNow.AddSeconds($timeout)
 do{ $rows=Records $path;if($rows.Count -ge $count){return ,$rows};Start-Sleep -Milliseconds 200 }while([DateTime]::UtcNow -lt $deadline)
 throw "Expected $count messages for $path"
}
function Launch([string]$path,[bool]$normal){
 $settings.apiUrl="http://127.0.0.1:18196$($path)?message="
 [IO.File]::WriteAllText($settingsPath,($settings|ConvertTo-Json -Depth 12))
 $args=@('-batchmode','-nographics','-logFile',"`"$(Join-Path $output ($path.TrimStart('/')+'.log'))`"")
 if($normal){$args+='-validate-content-log-exit'}
 $script:ownedPlayer=Start-Process -FilePath $playerPath -ArgumentList $args -WindowStyle Hidden -PassThru
}
try{
 $null=Invoke-RestMethod 'http://127.0.0.1:18196/health'
 $runId=[Guid]::NewGuid().ToString('N').Substring(0,8)
 $normalPath="/player_normal_$runId";Launch $normalPath $true
 $normal=Wait-Records $normalPath 2
 $null=$ownedPlayer.WaitForExit(15000)
 Check $ownedPlayer.HasExited 'Normal Application.Quit finishes without a stuck exit barrier'
 Start-Sleep -Milliseconds 1500
 $normal=Records $normalPath
 Check (($normal.message -join ',') -eq 'start,end') 'Normal Player sends exactly start,end and no end_kill'
 $ownedPlayer=$null
 $killedPath="/player_killed_$runId";Launch $killedPath $false
 $null=Wait-Records $killedPath 1
 Start-Sleep -Milliseconds 1500
 $identity=Get-Process -Id $ownedPlayer.Id
 Check ($identity.Path -eq $playerPath) 'Forced-exit test targets only the test Player executable'
 Stop-Process -Id $ownedPlayer.Id
 $killed=Wait-Records $killedPath 2
 Start-Sleep -Milliseconds 500
 $killed=Records $killedPath
 Check (($killed.message -join ',') -eq 'start,end_kill') 'Independent watcher sends exactly end_kill after a killed Player'
 Check (@($normal+$killed | Where-Object {$_.message -like 'Call API*' -or $_.message -like 'Return*'}).Count -eq 0) 'Comfy startup/probes do not produce external API telemetry'
 $report.normalMessages=@($normal.message);$report.killedMessages=@($killed.message);$report.passed=$true
}catch{$report.error=$_.Exception.ToString();throw}
finally{
 if($ownedPlayer -and !$ownedPlayer.HasExited){$identity=Get-Process -Id $ownedPlayer.Id -ErrorAction SilentlyContinue;if($identity -and $identity.Path -eq $playerPath){Stop-Process -Id $identity.Id}}
 [IO.File]::WriteAllText($settingsPath,$original)
 [IO.File]::WriteAllText((Join-Path $output 'player.json'),($report|ConvertTo-Json -Depth 10))
}
$report|ConvertTo-Json -Depth 10
