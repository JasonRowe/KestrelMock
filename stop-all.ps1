Write-Host "Stopping all KestrelMock related dotnet runs..." -ForegroundColor Yellow

# Try to stop gracefully by matching window titles, otherwise kill by process name
Stop-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -match "KestrelMock" }

# Also stop the KestrelMockServerInstance executable if spawned directly
Stop-Process -Name "KestrelMockServerInstance" -Force -ErrorAction SilentlyContinue

Write-Host "Done." -ForegroundColor Green
