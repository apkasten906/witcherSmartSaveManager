# Install Git Hooks
#
# This script installs the Git hooks from the .githooks directory
# into the local Git repository.
#
# Usage: ./Install-GitHooks.ps1

Write-Host "Installing Git hooks for Witcher Smart Save Manager..." -ForegroundColor Cyan

# Set the hooks directory
git config --local core.hooksPath .githooks

# Make the hooks executable if we're on a Unix-like system
if ($PSVersionTable.Platform -eq 'Unix') {
    chmod +x .githooks/pre-commit
    Write-Host "Made hooks executable for Unix systems." -ForegroundColor Green
}

# Create a symbolic link for Windows systems to use the .bat file
if ($PSVersionTable.Platform -eq 'Win32NT' -or $null -eq $PSVersionTable.Platform) {
    $preCommitHook = ".git\hooks\pre-commit"
    
    # Remove existing hook if it exists
    if (Test-Path $preCommitHook) {
        Remove-Item $preCommitHook -Force
    }
    
    # Create directory if it doesn't exist
    if (-not (Test-Path ".git\hooks")) {
        New-Item -ItemType Directory -Path ".git\hooks" -Force | Out-Null
    }
    
    # Use bat file for Windows
    Copy-Item ".githooks\pre-commit.bat" $preCommitHook
    Write-Host "Copied Windows-compatible hooks." -ForegroundColor Green
}

Write-Host "Git hooks installed successfully!" -ForegroundColor Green
Write-Host "These hooks will ensure code quality by:" -ForegroundColor Cyan
Write-Host "  1. Building the solution before each commit" -ForegroundColor White
Write-Host "  2. Running unit tests before each commit" -ForegroundColor White
Write-Host "  3. Preventing commits if builds fail or tests fail" -ForegroundColor White
