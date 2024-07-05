# Gets the GitHub access token
Param(
    [string]
    [Parameter(Mandatory = $false)]
    $AccessTokenIDs = "",

    [string]
    [Parameter(Mandatory = $false)]
    $ApimBaseUrl = "",

    [string]
    [Parameter(Mandatory = $false)]
    $ApimApiKey = "",

    [switch]
    [Parameter(Mandatory = $false)]
    $Help
)

function Show-Usage {
    Write-Output "    This gets the GitHub access token

    Usage: $(Split-Path $MyInvocation.ScriptName -Leaf) ``
            [-AccessTokenIDs    <Comma-delimited access token IDs>] ``
            [-ApimBaseUrl       <Base URL to APIM endpoint>] ``
            [-ApimApiKey        <API key to APIM request>] ``

            [-Help]

    Options:
        -AccessTokenIDs:    Comma-delimited access token IDs.
        -ApimBaseUrl:       Base URL to APIM endpoint.
        -ApimApiKey:        API key to APIM request.

        -Help:          Show this message.
"

    Exit 0
}

# Show usage
$needHelp = $Help -eq $true
if ($needHelp -eq $true) {
    Show-Usage
    Exit 0
}

if ([string]::IsNullOrWhiteSpace($AccessTokenIDs) -or [string]::IsNullOrWhiteSpace($ApimBaseUrl) -or [string]::IsNullOrWhiteSpace($ApimApiKey)) {
    Write-Host "All 'AccessTokenIDs', 'ApimBaseUrl' and 'ApimApiKey' must be provided" -ForegroundColor Red
    Show-Usage
    Exit 0
}

$ids = $AccessTokenIDs -split ","
  
# Get a random number
$random = Get-Random -Minimum 0 -Maximum $ids.Length

# Get the random assignee
$id = $ids[$random]

$tokenUrl = "$ApimBaseUrl/github/access-token?id=$id"
$token = $(Invoke-RestMethod $tokenUrl -Headers @{ "Ocp-Apim-Subscription-Key" = "$ApimApiKey" })

Write-Output $token

Remove-Variable token
Remove-Variable tokenUrl
Remove-Variable id
Remove-Variable random
Remove-Variable ids
