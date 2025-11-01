@echo off
echo Running NetworkScreensaver as Administrator...
echo This will enable tshark packet capture functionality.
echo Press ESC to exit screensaver when running.
echo.

REM Check if already running as admin
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Requesting administrator privileges...
    powershell -Command "Start-Process '%SystemRoot%\System32\NetworkScreensaver.scr' -ArgumentList '/s' -Verb RunAs"
) else (
    echo Already running as administrator, starting screensaver...
    "%SystemRoot%\System32\NetworkScreensaver.scr" /s
)
