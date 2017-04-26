
Rem @echo off &setlocal enabledelayedexpansion
Rem set "target=ProgressBar.exe"
Rem for /f "delims=, tokens=1,2" %%a in ('tasklist /fo csv /nh') do (
Rem   set "%%~a_pid=%%~b"
Rem )
Rem set "result=!%target%_pid!"
Rem if not "%result%" ==""  call taskkill /f /pid %result%

@echo off
taskkill /f /fi "IMAGENAME eq ProgressBar.exe" /t
Rem pause