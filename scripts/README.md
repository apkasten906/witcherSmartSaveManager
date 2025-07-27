# GitHub Issue Management Scripts

This directory contains scripts to help manage GitHub issues for the Witcher Smart Save Manager project.

## Manage-GitHubIssues.ps1

This PowerShell script provides a convenient way to create and manage GitHub issues through the command line.

### Prerequisites

- [GitHub CLI](https://cli.github.com/) installed and authenticated (`gh auth login`)
- PowerShell 5.1 or later

### Usage

```powershell
# Create a new feature request
.\Manage-GitHubIssues.ps1 -Action create -Title "Your issue title" -Description "Detailed description" -IssueType feature

# Create a bug report
.\Manage-GitHubIssues.ps1 -Action create -Title "Bug report title" -Description "Bug details" -IssueType bug -Milestone "Complete Witcher 2 Base Features"

# Close an issue with comment
.\Manage-GitHubIssues.ps1 -Action close -IssueNumber 42 -Comment "Closing as completed in branch feat/example"

# Add a comment to an issue
.\Manage-GitHubIssues.ps1 -Action comment -IssueNumber 42 -Comment "Implementation in progress"

# Add a label to an issue
.\Manage-GitHubIssues.ps1 -Action label -IssueNumber 42 -Label "enhancement"

# Set milestone for an issue
.\Manage-GitHubIssues.ps1 -Action milestone -IssueNumber 42 -Milestone "Complete Witcher 2 Base Features"

# List all open issues
.\Manage-GitHubIssues.ps1 -Action list

# Create a branch for an issue (will link the branch to the issue)
.\Manage-GitHubIssues.ps1 -Action branch -IssueNumber 42

# Search for issues
.\Manage-GitHubIssues.ps1 -Action search -Description "language support"

# Update status of an issue
.\Manage-GitHubIssues.ps1 -Action status -IssueNumber 42 -Status "In Progress"

# Link two issues together
.\Manage-GitHubIssues.ps1 -Action link -IssueNumber 42
```

### Parameters

| Parameter | Description | Required | Default |
|-----------|-------------|----------|---------|
| Action | Action to perform: create, close, comment, label, milestone, list, branch, search, status, or link | No | create |
| Title | Issue title | Yes (for create) | - |
| Description | Issue description | No | Interactive prompt |
| IssueNumber | Number of the issue to update | Yes (for close, comment, label, milestone) | - |
| Comment | Comment text | No | Interactive prompt |
| Label | Label to add | No | - |
| Milestone | Milestone to assign | No | Complete Witcher 2 Base Features |
| IssueType | Template type: feature, bug, enhancement, or documentation | No | feature |
| Repository | GitHub repository in owner/repo format | No | apkasten906/witcherSmartSaveManager |
| Project | GitHub project to assign the issue to | No | Witcher Smart Save Manager |
| Status | Initial status for the issue: Todo, In Progress, Done | No | Todo |

### Templates

The script includes templates for different issue types:
- **feature**: Standard feature request
- **bug**: Bug report with steps to reproduce
- **enhancement**: Improvement to existing functionality
- **documentation**: Documentation request

### Examples

#### Creating a feature request

```powershell
.\Manage-GitHubIssues.ps1 -Action create -Title "Add cloud backup integration" -Description "Allow users to back up their saves to cloud storage providers" -IssueType feature
```

#### Creating a bug report

```powershell
.\Manage-GitHubIssues.ps1 -Action create -Title "App crashes when deleting last save" -Description "Application crashes when deleting the last remaining save file" -IssueType bug
```

#### Closing an issue

```powershell
.\Manage-GitHubIssues.ps1 -Action close -IssueNumber 42 -Comment "Fixed in PR #45"
```

### Troubleshooting

If you encounter errors:

1. Ensure GitHub CLI is installed and in your PATH
2. Verify you're authenticated with `gh auth status`
3. Check that you have proper permissions for the repository
