# Git Hooks for Witcher Smart Save Manager

This repository includes Git hooks to ensure code quality. These hooks run automatically during Git operations to verify that commits meet quality standards.

## Included Hooks

### Pre-commit Hook
The pre-commit hook runs before each commit and performs these checks:
- Compiles the solution to ensure there are no build errors
- Runs unit tests to ensure all tests pass

If either the build fails or tests fail, the commit will be blocked until the issues are fixed.

## Installation

Run the installation script from the repository root:

```powershell
# From the repository root
.\Install-GitHooks.ps1
```

This will configure Git to use the hooks in the `.githooks` directory.

## For Team Members

Please install the Git hooks when you first clone the repository to ensure code quality.

## For Different Environments

- **Windows**: Uses the PowerShell/batch implementation (`pre-commit.bat`)
- **Linux/macOS**: Uses the shell script implementation (`pre-commit`)

The installation script will automatically set up the appropriate hook for your environment.

## Skipping Hooks (Emergency Only)

In rare cases when you need to bypass the hooks, you can use the `--no-verify` flag:

```
git commit --no-verify -m "Your commit message"
```

⚠️ **Warning**: This should be used only in emergency situations as it bypasses quality checks.
