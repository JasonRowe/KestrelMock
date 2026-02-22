$kestrelInstanceDir = "KestrelMockServerInstance"
$blazorUIDir = "KestrelMock.BlazorUI"

Write-Host "Starting KestrelMock Server..."
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --project $kestrelInstanceDir"

Write-Host "Starting Blazor UI..."
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run --project $blazorUIDir"

Write-Host "Both applications are starting in the background. Press any key to stop them..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host "Stopping applications..."
Stop-Process -Name "KestrelMockServer" -ErrorAction SilentlyContinue
Stop-Process -Name "KestrelMock.BlazorUI" -ErrorAction SilentlyContinue
Write-Host "Done."
