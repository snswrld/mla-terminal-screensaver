@echo off
echo NetworkScreensaver Cleanup and Reinstall
echo ========================================
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
echo Cleaning up old files...

REM Remove old screensaver files from System32
del "%SystemRoot%\System32\NetworkScreensaver.scr" >nul 2>&1
del "%SystemRoot%\System32\rss_feeds.json" >nul 2>&1

REM Clean project build artifacts
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Remove legacy MLA terminal files
del "mla-terminal.csproj" >nul 2>&1
del "mla-terminal.sln" >nul 2>&1
if exist "obj\mla-terminal.csproj.nuget.dgspec.json" del "obj\mla-terminal.csproj.nuget.dgspec.json" >nul 2>&1
if exist "obj\mla-terminal.csproj.nuget.g.props" del "obj\mla-terminal.csproj.nuget.g.props" >nul 2>&1
if exist "obj\mla-terminal.csproj.nuget.g.targets" del "obj\mla-terminal.csproj.nuget.g.targets" >nul 2>&1

REM Clean old log directories
if exist "%USERPROFILE%\Documents\NetworkScreensaverLogs" rmdir /s /q "%USERPROFILE%\Documents\NetworkScreensaverLogs" >nul 2>&1

echo.
echo Building and installing NetworkScreensaver...

REM Build the project
dotnet clean NetworkScreensaver.csproj
dotnet restore NetworkScreensaver.csproj
dotnet publish NetworkScreensaver.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

if %ERRORLEVEL% NEQ 0 (
    echo Build failed!
    pause
    exit /b 1
)

REM Install screensaver
copy "bin\Release\net8.0-windows\win-x64\publish\NetworkScreensaver.exe" "%SystemRoot%\System32\NetworkScreensaver.scr"
copy "rss_feeds.json" "%SystemRoot%\System32\"

if %ERRORLEVEL% NEQ 0 (
    echo Installation failed!
    pause
    exit /b 1
)

echo.
echo ========================================
echo Cleanup and installation completed successfully!
echo.
echo NetworkScreensaver is now installed and ready to use.
echo Go to Windows Settings > Personalization > Lock screen > Screen saver
echo and select "NetworkScreensaver" from the dropdown.
echo.
echo Press ESC key to safely exit screensaver when running.
echo ========================================
pause
