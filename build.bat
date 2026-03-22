@echo off
set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist %CSC_PATH% (
    echo [.NET Framework 4.5+ not found!]
    echo Please install .NET Framework or find csc.exe manually.
    pause
    exit /b
)

echo Compiling Recorder.cs...
%CSC_PATH% /target:winexe /r:System.Windows.Forms.dll /r:System.Drawing.dll /out:Recorder.exe Recorder.cs

if %ERRORLEVEL% EQU 0 (
    echo [Build Successful!]
    echo Run Recorder.exe to start.
) else (
    echo [Build Failed!]
)

pause
