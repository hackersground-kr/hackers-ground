# Check issue details
param(
    [string]
    [Parameter(Mandatory = $false)]
    $IssueNumber = "",

    [psobject]
    [Parameter(Mandatory = $false)]
    $GitHubPayload = $null,

    [string]
    [Parameter(Mandatory = $false)]
    $GitHubAccessToken = "",

    [switch]
    [Parameter(Mandatory = $false)]
    $Help
)

function Show-Usage {
    Write-Output "    This checks the issue details from the event payload

    Usage: $(Split-Path $MyInvocation.ScriptName -Leaf) ``
            [-IssueNumber       <GitHub issue number>] ``
            [-GitHubPayload     <GitHub event payload>] ``
            [-GitHubAccessToken <GitHub access token>] ``

            [-Help]

    Options:
        -IssueNumber:       GitHub issue number. If the event is 'workflow_dispatch', it must be provided.
        -GitHubPayload:     GitHub event payload.
        -GitHubAccessToken: GitHub access token. If not provided, it will look for the 'GH_TOKEN' environment variable.
        
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

if ($GitHubPayload -eq $null) {
    Write-Host "'GitHubPayload' must be provided" -ForegroundColor Red
    Show-Usage
    Exit 0
}

$eventName = $GitHubPayload.event_name
if (($eventName -eq "workflow_dispatch") -and ([string]::IsNullOrWhiteSpace($IssueNumber))) {
    Write-Host "'IssueNumber' must be provided for the 'workflow_dispatch' event" -ForegroundColor Red
    Show-Usage
    Exit 0
}

$accessToken = [string]::IsNullOrWhiteSpace($GitHubAccessToken) ? $env:GH_TOKEN : $GitHubAccessToken
if (($eventName -eq "workflow_dispatch") -and ([string]::IsNullOrWhiteSpace($accessToken))) {
    Write-Host "'GitHubAccessToken' must be provided through either environment variable or parameter" -ForegroundColor Red
    Show-Usage
    Exit 0
}

if ($eventName -eq "workflow_dispatch") {
    $GitHubPayload = $(gh api /repos/$($GitHubPayload.repository)/issues/$IssueNumber | ConvertFrom-Json)
    $body = $GitHubPayload.body
    $githubID = $GitHubPayload.user.login
    $assignee = $GitHubPayload.assignee
} else {
    $IssueNumber = $GitHubPayload.event.issue.number
    $body = $GitHubPayload.event.issue.body
    $githubID = $GitHubPayload.event.issue.user.login
    $assignee = $GitHubPayload.event.issue.assignee
}

$sections = $body.Split("###", [System.StringSplitOptions]::RemoveEmptyEntries)

$segments = $sections[0].Split("`n", [System.StringSplitOptions]::RemoveEmptyEntries)

$issue = @{}
$issue.Add("title", $segments[1].Trim())
$issue.title = if ($issue.title -eq "팀 주제 제출") {
    "Team Topic"
} elseif ($issue.title -eq "팀 앱 제출 1차") {
    "Team App 1"
} elseif ($issue.title -eq "팀 앱 제출 2차") {
    "Team App 2"
} elseif ($issue.title -eq "팀 발표자료 제출") {
    "Team Pitch"
} else {
    $issue.title
}

$sections | ForEach-Object {
    $segments = $_.Split("`n", [System.StringSplitOptions]::RemoveEmptyEntries)
    switch ($segments[0].Trim()) {
        "팀 이름" {
            $issue.Add("teamName", $segments[1].Trim())
        }
        "팀 리포지토리" {
            $issue.Add("teamRepository", $segments[1].Trim())
        }
    }
}

$issueType = switch ($issue.title) {
    "Team Topic" { "TOPIC" }
    "Team App 1" { "APP1" }
    "Team App 2" { "APP2" }
    "Team Pitch" { "PITCH" }
    default { $null }
}

$result = @{
    issueNumber = $IssueNumber;
    issueType = $issueType;
    githubID = $githubID;
    assignee = $assignee;
    title = $issue.title;
    teamName = $issue.teamName;
    teamRepository = $issue.teamRepository;
}

Write-Output $($result | ConvertTo-Json -Depth 100)

Remove-Variable result
Remove-Variable issueType
Remove-Variable issue
Remove-Variable segments
Remove-Variable sections
Remove-Variable assignee
Remove-Variable githubID
Remove-Variable body
Remove-Variable accessToken
Remove-Variable eventName
Remove-Variable needHelp
