# GitHub Issue Management Script

This PowerShell script (`Manage-GitHubIssues.ps1`) provides a command-line interface for managing GitHub issues for the Witcher Smart Save Manager project.

## Prerequisites

- [GitHub CLI](https://cli.github.com/) must be installed
- You must be authenticated with GitHub (`gh auth login`)

## Usage

```powershell
.\Manage-GitHubIssues.ps1 -Action <action> [parameters]
```

## Available Actions

### Create a new issue

```powershell
.\Manage-GitHubIssues.ps1 -Action create -Title "Issue title" -Description "Detailed description" [-IssueType feature|bug|enhancement|documentation] [-Label "label"] [-Status "Todo|In Progress|Done"]
```

- Creates a new issue with appropriate template based on issue type
- Adds the issue to the project board
- Sets initial status (default: Todo)

### Comment on an existing issue

```powershell
.\Manage-GitHubIssues.ps1 -Action comment -IssueNumber 42 -Comment "Your comment text"
```

### Add a label to an issue

```powershell
.\Manage-GitHubIssues.ps1 -Action label -IssueNumber 42 -Label "enhancement"
```

### Set milestone for an issue

```powershell
.\Manage-GitHubIssues.ps1 -Action milestone -IssueNumber 42 -Milestone "Complete Witcher 2 Base Features"
```

### List all open issues

```powershell
.\Manage-GitHubIssues.ps1 -Action list
```

### Create a development branch for an issue

```powershell
.\Manage-GitHubIssues.ps1 -Action branch -IssueNumber 42
```

- Creates a branch named `issue-42/issue-title` (based on issue title)
- Adds a comment to the issue with a link to the branch
- Checkout the new branch

### Search for issues

```powershell
.\Manage-GitHubIssues.ps1 -Action search -Description "search query"
```

### Update issue status

```powershell
.\Manage-GitHubIssues.ps1 -Action status -IssueNumber 42 -Status "In Progress"
```

- Adds a comment indicating the status change
- Note: Status must be manually updated in the GitHub project board

### Link related issues

```powershell
.\Manage-GitHubIssues.ps1 -Action link -IssueNumber 42
```

- Prompts for the target issue number
- Creates a bidirectional link between the issues through comments

### Close an issue

```powershell
.\Manage-GitHubIssues.ps1 -Action close -IssueNumber 42 [-Comment "Optional closing comment"]
```

## Parameters

| Parameter    | Description                                      | Default Value                   |
|--------------|--------------------------------------------------|--------------------------------|
| Action       | The action to perform                            | create                         |
| Title        | Title for new issues                             | (required for create)          |
| Description  | Description text                                 | (prompts if not provided)      |
| IssueNumber  | Issue number for operations on existing issues   | (required for most actions)    |
| Comment      | Comment text                                     | (prompts if not provided)      |
| Label        | Label to apply                                   | (prompts if not provided)      |
| Milestone    | Milestone to set                                 | "Complete Witcher 2 Base Features" |
| IssueType    | Type of issue when creating                      | feature                        |
| Repository   | GitHub repository                                | apkasten906/witcherSmartSaveManager |
| Project      | GitHub project name                              | Witcher Smart Save Manager     |
| Status       | Status to set                                    | Todo                           |

## Examples

See [Examples-GitHubIssues.ps1](./Examples-GitHubIssues.ps1) for more usage examples.

## Notes

- This script requires PowerShell 5.1 or later
- All operations are performed through the GitHub CLI
- Project status updates require manual intervention in the GitHub UI
