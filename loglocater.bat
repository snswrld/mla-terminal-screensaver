@echo off
echo Windows Path Diagnostics
echo =======================
echo.
echo User Profile: %USERPROFILE%
echo AppData: %APPDATA%
echo LocalAppData: %LOCALAPPDATA%
echo.
echo Checking if directories exist:
if exist "%USERPROFILE%" (echo ✓ UserProfile exists) else (echo ✗ UserProfile missing)
if exist "%APPDATA%" (echo ✓ AppData exists) else (echo ✗ AppData missing)
if exist "%LOCALAPPDATA%" (echo ✓ LocalAppData exists) else (echo ✗ LocalAppData missing)
echo.
echo Current NetworkMonitor is trying to use:
echo %USERPROFILE%\Documents\NetworkScreensaverLogs
echo.
echo Should be using:
echo %LOCALAPPDATA%\NetworkScreensaver\Logs
echo.
pause
