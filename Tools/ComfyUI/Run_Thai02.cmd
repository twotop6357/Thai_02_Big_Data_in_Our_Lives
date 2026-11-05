@echo off
setlocal
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden -File "%~dp0ComfyRuntime\portable_start.ps1"
if errorlevel 1 (
  echo Startup failed. See ComfyRuntime\logs\startup_error.txt
  pause
)
endlocal
