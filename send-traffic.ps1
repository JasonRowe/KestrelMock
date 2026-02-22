# send-traffic.ps1
$kestrelUrl = "http://localhost:5000"

Write-Host "Sending test traffic to KestrelMock server at $kestrelUrl..."
Write-Host "Press Ctrl+C to stop."
Write-Host "--------------------------------------------------------"

while ($true) {
    # Pick a random request type
    $action = Get-Random -Minimum 1 -Maximum 4

    try {
        if ($action -eq 1) {
            Write-Host "[GET] /api/v1/users - " -NoNewline
            $response = Invoke-RestMethod -Method Get -Uri "$kestrelUrl/api/v1/users"
            Write-Host "Success (200)" -ForegroundColor Green
        }
        elseif ($action -eq 2) {
            Write-Host "[POST] /api/v1/auth/login - " -NoNewline
            $body = @{ username = "testuser"; password = "password123" } | ConvertTo-Json
            $response = Invoke-RestMethod -Method Post -Uri "$kestrelUrl/api/v1/auth/login" -Body $body -ContentType "application/json"
            Write-Host "Success (200)" -ForegroundColor Green
        }
        elseif ($action -eq 3) {
            $userId = Get-Random -Minimum 1 -Maximum 100
            Write-Host "[DELETE] /api/v1/users/$userId - " -NoNewline
            $response = Invoke-WebRequest -Method Delete -Uri "$kestrelUrl/api/v1/users/$userId"
            Write-Host "Success (204)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "Failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    $delay = Get-Random -Minimum 500 -Maximum 2500
    Start-Sleep -Milliseconds $delay
}
