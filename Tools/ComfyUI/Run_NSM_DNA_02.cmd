@echo off
setlocal
cd /d "%~dp0"
if not exist "NSM_DNA_02.exe" (
  echo NSM_DNA_02.exe is missing. Run this file from the NSM_DNA_02 folder.
  pause
  exit /b 1
)
if not exist "ComfyRuntime\python\python.exe" (
  echo ComfyRuntime is missing. Move the ComfyRuntime folder from the old Thai_02 folder into this folder.
  pause
  exit /b 1
)
"ComfyRuntime\python\python.exe" -I "Launcher\prepare_player.py" "%~dp0."
if errorlevel 1 (
  echo Startup failed. See Launcher\startup_error.txt
  pause
  exit /b 1
)
start "" "NSM_DNA_02.exe" %*
endlocal
