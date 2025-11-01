@echo off
echo NetworkScreensaver Log File Locations
echo =====================================
echo.
echo Current log directory (old path):
echo %USERPROFILE%\Documents\NetworkScreensaverLogs
echo.
echo Expected log directory (Windows 11 compliant):
echo %LOCALAPPDATA%\NetworkScreensaver\Logs
echo.
echo Checking for log files...
echo.
if exist "%USERPROFILE%\Documents\NetworkScreensaverLogs" (
    echo Found logs in Documents folder:
    dir "%USERPROFILE%\Documents\NetworkScreensaverLogs" /b
) else (
    echo No logs found in Documents folder
)
echo.
if exist "%LOCALAPPDATA%\NetworkScreensaver\Logs" (
    echo Found logs in LocalAppData folder:
    dir "%LOCALAPPDATA%\NetworkScreensaver\Logs" /b
) else (
    echo No logs found in LocalAppData folder
)
echo.
pause
