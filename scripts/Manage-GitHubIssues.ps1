param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('create', 'close', 'comment', 'label', 'milestone', 'list', 'branch', 'search', 'status', 'link')]
    [string]$Action = 'create',
    
    [Parameter(Mandatory = $false)]
    [string]$Title,
    
    [Parameter(Mandatory = $false)]
    [string]$Description,
    
    [Parameter(Mandatory = $false)]
    [string]$IssueNumber,
    
    [Parameter(Mandatory = $false)]
    [string]$Comment,
    
    [Parameter(Mandatory = $false)]
    [string]$Label,
    
    [Parameter(Mandatory = $false)]
    [string]$Milestone,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet('feature', 'bug', 'enhancement', 'documentation')]
    [string]$IssueType = 'feature',

    [Parameter(Mandatory = $false)]
    [string]$Repository = "apkasten906/witcherSmartSaveManager",
    
    [Parameter(Mandatory = $false)]
    [string]$Project = "",
    
    [Parameter(Mandatory = $false)]
    [ValidateSet('Todo', 'In Progress', 'Done')]
    [string]$Status = "Todo"
)

$ErrorActionPreference = "Stop"

# Check if GitHub CLI is installed
try {
    $ghVersion = gh --version
    Write-Host "Using GitHub CLI: $ghVersion"
}
catch {
    Write-Error "GitHub CLI not found. Please install it from https://cli.github.com/"
    exit 1
}

# Check authentication status
try {
    gh auth status | Out-Null
    Write-Host "GitHub authentication verified."
}
catch {
    Write-Error "GitHub authentication failed. Please run 'gh auth login' first."
    exit 1
}

# Helper function to create issue templates based on type
function Get-IssueTemplate {
    param(
        [string]$Type,
        [string]$Description
    )
    
    switch ($Type) {
        'feature' {
            return @"
## Feature Request
$Description

## Acceptance Criteria
- [List your acceptance criteria here]

## Notes
- Priority: Medium
- Estimated effort: TBD
"@
        }
        'bug' {
            return @"
## Bug Description
$Description

## Steps to Reproduce
1. [First step]
2. [Second step]
3. [Additional steps as needed]

## Expected Behavior
[Describe what should happen]

## Actual Behavior
[Describe what actually happens]

## Environment
- Windows version: [version]
- .NET version: [version]
- Application version: [version]
"@
        }
        'enhancement' {
            return @"
## Enhancement Description
$Description

## Motivation
[Why is this enhancement needed?]

## Proposed Implementation
[Any ideas on how to implement this]

## Acceptance Criteria
- [List your acceptance criteria here]
"@
        }
        'documentation' {
            return @"
## Documentation Request
$Description

## Areas Affected
- [List documentation areas that need updates]

## Content Suggestions
- [Any specific content that should be included]
"@
        }
        default {
            return $Description
        }
    }
}

# Handle different actions
switch ($Action) {
    'create' {
        if (-not $Title) {
            Write-Error "Title is required for creating an issue."
            exit 1
        }
        
        if (-not $Description) {
            $Description = Read-Host "Enter issue description"
        }
        
        $body = Get-IssueTemplate -Type $IssueType -Description $Description
        
        Write-Host "Creating issue: $Title"
        $command = "gh issue create --repo $Repository --title `"$Title`" --body `"$body`" --assignee `"@me`""
        
        # Only add milestone if specified and not empty
        if (-not [string]::IsNullOrWhiteSpace($Milestone)) {
            $command += " --milestone `"$Milestone`""
        }
        
        # Note: Project will be added in a second step after issue creation
        
        if ($Label) {
            $command += " --label `"$Label`""
        }
        
        # Create the issue with error handling
        try {
            $issueUrl = Invoke-Expression $command
            if (-not $issueUrl) {
                Write-Host "Command was: $command" -ForegroundColor Yellow
                Write-Error "GitHub CLI failed to create issue (no URL returned)"
                exit 1
            }
        }
        catch {
            Write-Host "Command was: $command" -ForegroundColor Yellow
            Write-Error "Error: $_"
            exit 1
        }
        
        # Extract issue number from the URL
        if ($issueUrl -match '\/issues\/(\d+)$') {
            $newIssueNumber = $matches[1]
            Write-Host "Setting status '$Status' for issue #$newIssueNumber"
            
            # Skip the interactive editor for issue edit
            # We're just setting status via comment instead
            
            # Add a comment noting the status
            $statusComment = "Status set by automation: **$Status**"
            $commentCommand = "gh issue comment $newIssueNumber --repo $Repository --body `"$statusComment`""
            Invoke-Expression $commentCommand
            
            # Add to project if specified (in a separate step after issue creation)
            if (-not [string]::IsNullOrWhiteSpace($Project)) {
                Write-Host "Attempting to add issue #$newIssueNumber to project '$Project'..."
                try {
                    # Use the project add command with the correct format
                    # For GitHub's new Projects (v2), use the project number (1)
                    $projectNumber = "1"  # Using hardcoded value for Witcher Smart Save Manager project
                    $projectCommand = "gh project item-add $projectNumber --owner apkasten906 --url $issueUrl"
                    Invoke-Expression $projectCommand
                    Write-Host "Successfully added issue to project '$Project'." -ForegroundColor Green
                }
                catch {
                    Write-Host "Could not add issue to project automatically: $_" -ForegroundColor Yellow
                    Write-Host "You may need to add it manually through the GitHub web interface." -ForegroundColor Yellow
                }
            }
            
            # Inform the user how to set the status in the GitHub UI
            Write-Host "Note: You'll need to manually set the status to '$Status' in the GitHub project board."
        }
    }
    
    'close' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for closing an issue."
            exit 1
        }
        
        if (-not $Comment) {
            $Comment = Read-Host "Enter closing comment (optional)"
        }
        
        Write-Host "Closing issue #$IssueNumber"
        if ($Comment) {
            $command = "gh issue close $IssueNumber --repo $Repository --comment `"$Comment`""
        }
        else {
            $command = "gh issue close $IssueNumber --repo $Repository"
        }
        
        Invoke-Expression $command
    }
    
    'comment' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for adding a comment."
            exit 1
        }
        
        if (-not $Comment) {
            $Comment = Read-Host "Enter comment text"
        }
        
        Write-Host "Adding comment to issue #$IssueNumber"
        $command = "gh issue comment $IssueNumber --repo $Repository --body `"$Comment`""
        Invoke-Expression $command
    }
    
    'label' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for adding a label."
            exit 1
        }
        
        if (-not $Label) {
            $Label = Read-Host "Enter label to add"
        }
        
        Write-Host "Adding label '$Label' to issue #$IssueNumber"
        $command = "gh issue edit $IssueNumber --repo $Repository --add-label `"$Label`""
        Invoke-Expression $command
    }
    
    'milestone' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for setting a milestone."
            exit 1
        }
        
        Write-Host "Setting milestone '$Milestone' for issue #$IssueNumber"
        $command = "gh issue edit $IssueNumber --repo $Repository --milestone `"$Milestone`""
        Invoke-Expression $command
    }
    
    'list' {
        Write-Host "Listing open issues in $Repository"
        $command = "gh issue list --repo $Repository --state open"
        Invoke-Expression $command
    }
    
    'branch' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for creating a branch."
            exit 1
        }
        
        # Get issue details to create a meaningful branch name
        Write-Host "Getting details for issue #$IssueNumber"
        $issueDetails = gh issue view $IssueNumber --repo $Repository --json title
        $issueData = $issueDetails | ConvertFrom-Json
        $issueTitle = $issueData.title
        
        # Create a branch name based on issue number and title
        $branchName = "issue-$IssueNumber/" + ($issueTitle -replace '[^a-zA-Z0-9]', '-').ToLower() -replace '-+', '-' -replace '^-|-$', ''
        
        Write-Host "Creating branch '$branchName' for issue #$IssueNumber"
        
        # Check if we're already on the right branch
        $currentBranch = git rev-parse --abbrev-ref HEAD
        if ($currentBranch -ne "main" -and $currentBranch -ne "master") {
            $checkout = Read-Host "Currently on branch '$currentBranch'. Checkout main first? (Y/n)"
            if ($checkout -ne "n") {
                git checkout main
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Failed to checkout main branch."
                    exit 1
                }
            }
        }
        
        # Create and checkout the new branch
        git checkout -b $branchName
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to create and checkout branch '$branchName'."
            exit 1
        }
        
        # Add comment to the issue linking the branch
        $comment = "Created branch [\`$branchName\`](https://github.com/$Repository/tree/$branchName) for this issue."
        $command = "gh issue comment $IssueNumber --repo $Repository --body `"$comment`""
        Invoke-Expression $command
        
        # Add Development link to the issue (through a comment since API doesn't directly support this)
        $devComment = "Development branch: $branchName"
        $command = "gh issue comment $IssueNumber --repo $Repository --body `"$devComment`""
        Invoke-Expression $command
        
        Write-Host "Branch '$branchName' created and linked to issue #$IssueNumber"
    }
    
    'search' {
        if (-not $Description) {
            $Description = Read-Host "Enter search query"
        }
        
        Write-Host "Searching issues for: $Description"
        $command = "gh issue list --repo $Repository --search `"$Description`""
        Invoke-Expression $command
    }
    
    'status' {
        if (-not $IssueNumber) {
            Write-Error "Issue number is required for updating status."
            exit 1
        }
        
        Write-Host "Setting status '$Status' for issue #$IssueNumber"
        # Add a comment with the status since GitHub CLI doesn't directly support setting project item status
        $statusComment = "## Status Update
Status changed to: **$Status**

*Updated via Manage-GitHubIssues script*"
        $command = "gh issue comment $IssueNumber --repo $Repository --body `"$statusComment`""
        Invoke-Expression $command
        
        Write-Host "Note: You'll need to manually update the status to '$Status' in the GitHub project board."
    }
    
    'link' {
        if (-not $IssueNumber) {
            Write-Error "Source issue number is required for linking issues."
            exit 1
        }
        
        $targetIssue = Read-Host "Enter target issue number to link to #$IssueNumber"
        if (-not $targetIssue) {
            Write-Error "Target issue number is required."
            exit 1
        }
        
        Write-Host "Linking issue #$IssueNumber to issue #$targetIssue"
        $comment = "Linked to #$targetIssue"
        $command = "gh issue comment $IssueNumber --repo $Repository --body `"$comment`""
        Invoke-Expression $command
        
        $reverseComment = "Linked from #$IssueNumber"
        $reverseCommand = "gh issue comment $targetIssue --repo $Repository --body `"$reverseComment`""
        Invoke-Expression $reverseCommand
    }
}
