@echo off
setlocal
powershell.exe -NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -File "%~dp0ComfyRuntime\portable_start.ps1" -Diagnose
echo.
echo Diagnostics: ComfyRuntime\logs\diagnostics.json
pause
endlocal
