@echo off
echo NetworkScreensaver Uninstaller
echo ==============================
echo.

REM Check for admin privileges
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: This script must be run as Administrator!
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo Stopping any running screensaver processes...
taskkill /f /im NetworkScreensaver.exe >nul 2>&1
taskkill /f /im NetworkScreensaver.scr >nul 2>&1

echo.
echo Removing screensaver files from System32...
del "%SystemRoot%\System32\NetworkScreensaver.scr" >nul 2>&1
del "%SystemRoot%\System32\rss_feeds.json" >nul 2>&1
del "%SystemRoot%\System32\tshark_capture.bat" >nul 2>&1

echo.
echo Cleaning up log directories...
if exist "%USERPROFILE%\Documents\NetworkScreensaverLogs" (
    echo Found logs in Documents folder
    set /p cleanup="Delete log files? (y/n): "
    if /i "%cleanup%"=="y" (
        rmdir /s /q "%USERPROFILE%\Documents\NetworkScreensaverLogs"
        echo Log files deleted
    ) else (
        echo Log files preserved
    )
)

if exist "%LOCALAPPDATA%\NetworkScreensaver" (
    echo Found logs in LocalAppData folder
    set /p cleanup2="Delete LocalAppData logs? (y/n): "
    if /i "%cleanup2%"=="y" (
        rmdir /s /q "%LOCALAPPDATA%\NetworkScreensaver"
        echo LocalAppData logs deleted
    ) else (
        echo LocalAppData logs preserved
    )
)

echo.
echo Refreshing Windows screensaver list...
rundll32.exe desk.cpl,InstallScreenSaver

echo.
echo ==============================
echo NetworkScreensaver uninstalled successfully!
echo.
echo NOTE: You may need to:
echo 1. Change your screensaver setting in Windows
echo 2. Restart Windows Explorer if the old screensaver still appears in the list
echo.
echo External dependencies (Wireshark, .NET) were NOT removed.
echo ==============================
pause
