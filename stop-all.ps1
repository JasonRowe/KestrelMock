Write-Host "Stopping all KestrelMock related dotnet runs..." -ForegroundColor Yellow

# Try to stop them gracefully by matching the window titles if possible, otherwise forcefully stop the process
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -match "KestrelMock" }

# Since dotnet run sometimes spawns a child process for the actual executing app, we should also look for those specific executable names if they exist (produced by the build).
Stop-Process -Name "KestrelMockServerInstance" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "KestrelMock.BlazorUI" -Force -ErrorAction SilentlyContinue

Write-Host "Done." -ForegroundColor Green
