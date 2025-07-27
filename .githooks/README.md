# Git Hooks for Witcher Smart Save Manager

This repository includes Git hooks to ensure code quality. These hooks run automatically during Git operations to verify that commits meet quality standards.

## Included Hooks

### Pre-commit Hook
The pre-commit hook runs before each commit and performs these checks:
1. **Branch Naming Convention**: Validates that feature branches follow the pattern `feat/{issue-number}-{description}` to ensure proper GitHub issue linking
2. **Code Compilation**: Compiles the solution to ensure there are no build errors
3. **Unit Tests**: Runs unit tests to ensure all tests pass

If any of these checks fail, the commit will be blocked until the issues are fixed.

#### Branch Naming Requirements
- **Feature branches**: Must follow `feat/{issue-number}-{description}` format
  - ✅ Valid: `feat/56-link-branch-to-issue`, `feat/123-add-new-feature`
  - ❌ Invalid: `feat/my-feature`, `feature/56-something`, `feat/no-number`
- **Special branches**: `main`, `dev`, `master`, and `hotfix/*` are exempt from naming validation
- **Purpose**: Ensures automatic linking between branches and GitHub issues for better project tracking

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
