@echo off
set LOGDIR=%1
set TIMESTAMP=%2

if "%LOGDIR%"=="" goto :error
if "%TIMESTAMP%"=="" set TIMESTAMP=%date:~-4,4%%date:~-10,2%%date:~-7,2%_%time:~0,2%%time:~3,2%%time:~6,2%

set TIMESTAMP=%TIMESTAMP: =0%
set PCAPFILE=%LOGDIR%\capture_%TIMESTAMP%.pcap

REM Run tshark indefinitely until killed
tshark -i any -w "%PCAPFILE%" -q
exit /b 0

:error
echo Usage: tshark_capture.bat LOGDIR [TIMESTAMP]
exit /b 1
