@echo off
echo Testing Network Screensaver...

REM Build the project first
dotnet clean NetworkScreensaver.csproj
dotnet restore NetworkScreensaver.csproj
dotnet build NetworkScreensaver.csproj -c Release

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Build successful! Starting screensaver test...
echo Press ESC key to safely exit the screensaver.
echo.

REM Run the screensaver in test mode
"bin\Release\net8.0-windows\NetworkScreensaver.exe" /s

echo Test completed.
echo If network connections were detected, you should see an alert.
pause