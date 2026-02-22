# add-mocks.ps1
$kestrelUrl = "http://localhost:5000"

$mocks = @(
    @{
        Id = "get-users"
        Request = @{
            Methods = @("GET")
            PathStartsWith = "/api/v1/users"
        }
        Response = @{
            Status = 200
            Body = "[{'id': 1, 'name': 'Alice'}, {'id': 2, 'name': 'Bob'}]"
            Headers = @(
                @{"Content-Type" = "application/json"}
            )
        }
        Watch = @{
            Id = "11111111-1111-1111-1111-111111111111"
            RequestLogLimit = 100
        }
    },
    @{
        Id = "post-login"
        Request = @{
            Methods = @("POST")
            PathStartsWith = "/api/v1/auth/login"
        }
        Response = @{
            Status = 200
            Body = "{'token': 'super-secret-jwt'}"
            Headers = @(
                @{"Content-Type" = "application/json"}
            )
        }
        Watch = @{
            Id = "22222222-2222-2222-2222-222222222222"
            RequestLogLimit = 100
        }
    },
    @{
        Id = "delete-user"
        Request = @{
            Methods = @("DELETE")
            PathMatchesRegex = "^/api/v1/users/([0-9]+)$"
        }
        Response = @{
            Status = 204
        }
        Watch = @{
            Id = "33333333-3333-3333-3333-333333333333"
            RequestLogLimit = 100
        }
    }
)

Write-Host "Registering Mocks..."

foreach ($mock in $mocks) {
    $jsonBody = $mock | ConvertTo-Json -Depth 5
    Write-Host "Adding mock: $($mock.Id)"
    Invoke-RestMethod -Method Post -Uri "$kestrelUrl/kestrelmock/mocks" -Body $jsonBody -ContentType "application/json" | Out-Null
}

Write-Host "Successfully registered 3 observed mocks!"
