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

    [string]
    [Parameter(Mandatory = $false)]
    $DueDate = "",

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
            [-DueDate           <Due date>] ``

            [-Help]

    Options:
        -IssueNumber:       GitHub issue number. If the event is 'workflow_dispatch', it must be provided.
        -GitHubPayload:     GitHub event payload.
        -GitHubAccessToken: GitHub access token. If not provided, it will look for the 'GH_TOKEN' environment variable.
        -DueDate:           Due date.
        
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
    $title = $GitHubPayload.title
    $githubID = $GitHubPayload.user.login
    $assignee = $GitHubPayload.assignee
    $createdAt = $GitHubPayload.created_at.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
} else {
    $IssueNumber = $GitHubPayload.event.issue.number
    $body = $GitHubPayload.event.issue.body
    $title = $GitHubPayload.event.issue.title
    $githubID = $GitHubPayload.event.issue.user.login
    $assignee = $GitHubPayload.event.issue.assignee
    $createdAt = $GitHubPayload.event.issue.created_at.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
}

$sections = $body.Split("###", [System.StringSplitOptions]::RemoveEmptyEntries)

$segments = $sections[0].Split("`n", [System.StringSplitOptions]::RemoveEmptyEntries)

$issue = @{}
$issue.Add("title", $segments[1].Trim())
$issue.title = if ($issue.title -eq "클라우드 스킬 챌린지") {
    "Cloud Skills Challenge"
} elseif ($issue.title -eq "사전 워크샵") {
    "Workshop"
} elseif ($issue.title -eq "팀 주제 제출") {
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
    if ($issue.title -eq "Cloud Skills Challenge") {
        switch ($segments[0].Trim()) {
            "챌린지 코드" {
                $issue.Add("challengeCode", $segments[1].Trim())
            }
            "GitHub 프로필 URL" {
                $issue.Add("githubProfile", $segments[1].Trim())
            }
            "Microsoft Learn 프로필 URL" {
                $issue.Add("microsoftLearnProfile", $segments[1].Trim())
            }
        }
    } elseif ($issue.title -eq "Workshop") {
        switch ($segments[0].Trim()) {
            "GitHub 프로필 URL" {
                $issue.Add("githubProfile", $segments[1].Trim())
            }
            "GitHub 리포지토리 URL" {
                $issue.Add("githubRepository", $segments[1].Trim())
            }
            "프론트엔드 앱 URL" {
                $issue.Add("frontendUrl", $segments[1].Trim())
            }
            "백엔드 앱 URL" {
                $issue.Add("backendUrl", $segments[1].Trim())
            }
            "대시보드 앱 URL" {
                $issue.Add("dashboardUrl", $segments[1].Trim())
            }
        }
    } else {
        switch ($segments[0].Trim()) {
            "팀 이름" {
                $issue.Add("teamName", $segments[1].Trim())
            }
            "팀 리포지토리" {
                $issue.Add("teamRepository", $segments[1].Trim())
            }
        }
    }
}

$issueType = switch ($issue.title) {
    "Cloud Skills Challenge" { "CSC" }
    "Workshop" { "WORKSHOP" }
    "Team Topic" { "TOPIC" }
    "Team App 1" { "APP1" }
    "Team App 2" { "APP2" }
    "Team Pitch" { "PITCH" }
    default { $null }
}

$isValidChallengeCode = $title.Contains($issue.challengeCode, [StringComparison]::InvariantCultureIgnoreCase)

$tz = [TimeZoneInfo]::FindSystemTimeZoneById("Asia/Seoul")

$dateSubmitted = [DateTimeOffset]::Parse($createdAt)
$offset = $tz.GetUtcOffset($dateSubmitted)
$dateSubmitted = $dateSubmitted.ToOffset($offset)

$dateDue = [DateTimeOffset]::Parse($DueDate)
$isOverdue = "$($dateSubmitted -gt $dateDue)".ToLowerInvariant()

$dateSubmittedValue = $dateSubmitted.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")
$dateDueValue = $dateDue.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz")

if (($issueType -eq "TOPIC") -or ($issueType -eq "APP1") -or ($issueType -eq "APP2") -or ($issueType -eq "PITCH")) {
    $isValidTeamRepository = $($issue.teamRepository).StartsWith("https://github.com/hackersground-kr/") -eq $true

    $segments = $issue.teamRepository.Split('/', [System.StringSplitOptions]::RemoveEmptyEntries)
    $issue.teamRepository = $segments[$segments.Length - 1]
} else {
    $issue.githubProfile = $issue.githubProfile.Split('?')[0] 
    $isValidGitHubProfile = $($($issue.githubProfile).StartsWith("https://github.com/") -eq $true) -and $($($issue.githubProfile).TrimEnd("/").EndsWith($githubID) -eq $true)
    $isValidMicrosoftLearnProfile = if ($issueType -eq "CSC") {
        $($issue.microsoftLearnProfile).StartsWith("https://learn.microsoft.com/ko-kr/users/") -eq $true
    } else {
        $false
    }
    $isValidGitHubRepository = if ($issueType -eq "WORKSHOP") {
        $issue.githubRepository = $issue.githubRepository.Split('?')[0]
        $($($issue.githubRepository).StartsWith("https://github.com/") -eq $true) -and $($($issue.githubRepository).Contains("/$gitHubID/") -eq $true)
    } else {
        $false
    }
    $isValidFrontendUrl = if ($issueType -eq "WORKSHOP") {
        $($($issue.frontendUrl).StartsWith("https://")) -and $($($issue.frontendUrl).TrimEnd("/").EndsWith(".azurecontainerapps.io"))
     } else {
        $false
    }
    $isValidBackendUrl = if ($issueType -eq "WORKSHOP") {
        $($($issue.backendUrl).StartsWith("https://")) -and $($($issue.backendUrl).TrimEnd("/").EndsWith(".azurecontainerapps.io"))
    } else {
        $false
    }
    $isValidDashboardUrl = if ($issueType -eq "WORKSHOP") {
        $($($issue.dashboardUrl).StartsWith("https://")) -and $($($issue.dashboardUrl).TrimEnd("/").EndsWith(".azurecontainerapps.io"))
    } else {
        $false
    }
    $isValidDashboardUrl = if ($issueType -eq "WORKSHOP") {
        $($($issue.dashboardUrl).StartsWith("https://")) -and $($($issue.dashboardUrl).TrimEnd("/").EndsWith(".azurecontainerapps.io"))
    } else {
        $false
    }
}

$result = @{
    issueNumber = $IssueNumber;
    issueType = $issueType;
    createdAt = $createdAt;
    title = $issue.title;
    challengeCode = $issue.challengeCode;
    isValidChallengeCode = $isValidChallengeCode;
    githubID = $githubID;
    githubProfile = $issue.githubProfile;
    isValidGitHubProfile = $isValidGitHubProfile;
    microsoftLearnProfile = $issue.microsoftLearnProfile;
    isValidMicrosoftLearnProfile = $isValidMicrosoftLearnProfile;
    dateSubmitted = $dateSubmittedValue;
    dateDue = $dateDueValue;
    isOverdue = $isOverdue;
    githubRepository = $issue.githubRepository;
    isValidGitHubRepository = $isValidGitHubRepository;
    frontendUrl = $issue.frontendUrl;
    isValidFrontendUrl = $isValidFrontendUrl;
    backendUrl = $issue.backendUrl;
    isValidBackendUrl = $isValidBackendUrl;
    dashboardUrl = $issue.dashboardUrl;
    isValidDashboardUrl = $isValidDashboardUrl;
    teamName = $issue.teamName;
    teamRepository = $issue.teamRepository;
    isValidTeamRepository = $isValidTeamRepository;
    assignee = $assignee;
}

Write-Output $($result | ConvertTo-Json -Depth 100)

# Remove-Variable result
# Remove-Variable isValidTeamRepository
# Remove-Variable teamRepository
# Remove-Variable teamName
# Remove-Variable isValidDashboardUrl
# Remove-Variable isValidBackendUrl
# Remove-Variable isValidFrontendUrl
# Remove-Variable isValidGitHubRepository
# Remove-Variable isValidMicrosoftLearnProfile
# Remove-Variable isValidGitHubProfile
# Remove-Variable dateDueValue
# Remove-Variable dateSubmittedValue
# Remove-Variable isOverdue
# Remove-Variable dateDue
# Remove-Variable dateSubmitted
# Remove-Variable offset
# Remove-Variable tz
# Remove-Variable isValidChallengeCode
# Remove-Variable issueType
# Remove-Variable issue
# Remove-Variable segments
# Remove-Variable sections
# Remove-Variable createdAt
# Remove-Variable githubID
# Remove-Variable title
# Remove-Variable body
# Remove-Variable accessToken
# Remove-Variable eventName
# Remove-Variable needHelp
