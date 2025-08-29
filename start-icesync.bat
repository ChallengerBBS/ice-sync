@echo off
echo Starting IceSync - The Ice Cream Company Workflow Management System
echo.

REM Check if .NET is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET is not installed or not in PATH
    pause
    exit /b 1
)

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Node.js is not installed or not in PATH
    pause
    exit /b 1
)

echo Prerequisites check passed
echo.

echo Starting API server...
start "IceSync API" powershell -NoExit -Command "cd 'IceSync.Api'; dotnet run"

echo Waiting 5 seconds for API to start...
timeout /t 5 /nobreak >nul

echo Starting React frontend...
start "IceSync Frontend" powershell -NoExit -Command "cd 'IceSync.Web'; npm install; npm start"

echo.
echo IceSync is starting up!
echo API will be available at: https://localhost:7041
echo Frontend will be available at: http://localhost:3000
echo.
echo Both applications are launching in separate windows...
echo The React dev server will automatically open your browser.
echo This script will close automatically in 3 seconds...
timeout /t 3 /nobreak >nul

echo Script completed. Applications are running in separate windows.
