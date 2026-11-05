param(
 [Parameter(Mandatory=$true)][int]$TargetProcessId,
 [Parameter(Mandatory=$true)][long]$StartTimeUtcTicks,
 [Parameter(Mandatory=$true)][string]$Endpoint,
 [Parameter(Mandatory=$true)][string]$NormalExitMarker
)
# Monitors only the supplied Player. It never terminates a process or shuts down Windows.
$ErrorActionPreference='Stop'
try {
 $target=[Diagnostics.Process]::GetProcessById($TargetProcessId)
 if($target.StartTime.ToUniversalTime().Ticks -eq $StartTimeUtcTicks){
  $null=$target.Handle
  $target.WaitForExit()
 }
} catch [ArgumentException] { # The Player may have exited before the watcher obtained its handle.
} catch [InvalidOperationException] { # The process may exit between acquiring its ID and its handle.
} finally { if($target){$target.Dispose()} }
if(Test-Path -LiteralPath $NormalExitMarker){
 Remove-Item -LiteralPath $NormalExitMarker
 exit 0
}
$endpointUri=[Uri]$Endpoint
if(($endpointUri.Scheme -ne 'https' -and $endpointUri.Scheme -ne 'http') -or !$Endpoint.EndsWith('message=')){exit 2}
[Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12
for($attempt=0;$attempt -lt 2;$attempt++){
 try {
  $null=Invoke-WebRequest -Uri ($Endpoint+[Uri]::EscapeDataString('end_kill')) -UseBasicParsing -TimeoutSec 3
  exit 0
 }catch{if($attempt -eq 0){Start-Sleep -Milliseconds 500}}
}
exit 1
