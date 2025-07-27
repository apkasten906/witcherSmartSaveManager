# Examples for using the Manage-GitHubIssues script

# Create a basic feature request
.\Manage-GitHubIssues.ps1 -Action create -Title "Example feature request" -Description "This is a test feature request" -IssueType feature -Status "Todo" -Project "Witcher Smart Save Manager"

# Create a bug report with detailed description
$bugDescription = @"
The application crashes when performing the following actions:
1. Open the app
2. Load Witcher 2 saves
3. Try to delete multiple saves at once
"@
.\Manage-GitHubIssues.ps1 -Action create -Title "Example bug report" -Description $bugDescription -IssueType bug -Status "In Progress"

# Create an enhancement request for an existing feature
.\Manage-GitHubIssues.ps1 -Action create -Title "Improve language switching" -Description "Add ability to switch languages without restarting the application" -IssueType enhancement

# List all open issues
.\Manage-GitHubIssues.ps1 -Action list

# Comment on issue #57 
.\Manage-GitHubIssues.ps1 -Action comment -IssueNumber 57 -Comment "Starting implementation in branch feat/handle-orphaned-images"

# Add a label to issue #57
.\Manage-GitHubIssues.ps1 -Action label -IssueNumber 57 -Label "enhancement"

# Create a development branch for issue #57
.\Manage-GitHubIssues.ps1 -Action branch -IssueNumber 57

# Update the status of issue #57
.\Manage-GitHubIssues.ps1 -Action status -IssueNumber 57 -Status "In Progress"

# Link issue #57 to a related issue
.\Manage-GitHubIssues.ps1 -Action link -IssueNumber 57

# Search for orphaned image related issues
.\Manage-GitHubIssues.ps1 -Action search -Description "orphaned image"

# Close issue #57 when complete
.\Manage-GitHubIssues.ps1 -Action close -IssueNumber 57 -Comment "Fixed in commit abc123. Implemented detection and cleanup of orphaned image files."
