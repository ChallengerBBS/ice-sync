# IceSync Startup Script
# This script starts both the API and frontend applications

Write-Host "Starting IceSync - The Ice Cream Company Workflow Management System" -ForegroundColor Green
Write-Host ""

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host ".NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host ".NET is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "Node.js is not installed or not in PATH" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting API server..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'IceSync.Api'; dotnet run --launch-profile https" -WindowStyle Normal

Write-Host "Waiting 5 seconds for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host "Starting React frontend..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd 'IceSync.Web'; npm install; npm start" -WindowStyle Normal

Write-Host ""
Write-Host "IceSync is starting up!" -ForegroundColor Green
Write-Host "API will be available at: https://localhost:7041 (HTTPS)" -ForegroundColor Cyan
Write-Host "Frontend will be available at: http://localhost:3000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Both applications are launching in separate windows..." -ForegroundColor Yellow
Write-Host "The React dev server will automatically open your browser." -ForegroundColor Cyan
Write-Host "This script will close automatically in 3 seconds..." -ForegroundColor Gray
Start-Sleep -Seconds 3

Write-Host "Script completed. Applications are running in separate windows." -ForegroundColor Green
