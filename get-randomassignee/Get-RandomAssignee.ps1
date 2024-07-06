# Get a random assignee from a list of assignees
Param(
    [string]
    [Parameter(Mandatory=$false)]
    $Assignees = "",

    [switch]
    [Parameter(Mandatory=$false)]
    $Help
)

function Show-Usage {
    Write-Output "    This returns a randomly selected assignee from the given collection of assignees

    Usage: $(Split-Path $MyInvocation.ScriptName -Leaf) ``
            [-Assignees <List of assignees>] ``

            [-Help]

    Options:
        -Assignees: A comma-delimited or line-break-delimited list of assignees

        -Help:      Show this message.
"

    Exit 0
}

# Show usage
$needHelp = $Help -eq $true
if ($needHelp -eq $true) {
    Show-Usage
    Exit 0
}

if ([String]::IsNullOrWhiteSpace($Assignees) -eq $true) {
    Show-Usage
    Exit 0
}

# Split the assignees into an array
$assigneesCollection = $Assignees -split "[,`r`n]" | `
    Where-Object { [String]::IsNullOrWhiteSpace($_) -eq $false } | `
    ForEach-Object { $_.Trim() }

# Get a random number
$random = Get-Random -Minimum 0 -Maximum $assigneesCollection.Length

# Get the random assignee
$randomAssignee = $assigneesCollection[$random]

# Output the random assignee
Write-Output $randomAssignee
