$gateway = "http://localhost:7000/lb"
$counts  = @{ X = 0; Y = 0; Z = 0 }

function Send-Requests($method, $n) {
    for ($i = 0; $i -lt $n; $i++) {
        $response = Invoke-RestMethod -Uri $gateway -Method $method
        $counts[$response.nick]++
    }
}

Send-Requests "GET"    50
Send-Requests "POST"   50
Send-Requests "PUT"    50
Send-Requests "DELETE" 50

Write-Host "`nResults:"
Write-Host "X handled: $($counts['X']) requests"
Write-Host "Y handled: $($counts['Y']) requests"
Write-Host "Z handled: $($counts['Z']) requests"