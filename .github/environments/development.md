# Development Environment Configuration

## Overview
The `development` environment is used for feature development on the `dev` branch.

## Protection Rules
- **Required Reviewers**: None (fast feedback for developers)
- **Wait Timer**: None
- **Deployment Branches**: Only `dev` branch and feature branches

## Environment Variables
Set these in GitHub Settings > Environments > development:

| Variable | Value | Description |
|----------|-------|-------------|
| `ENVIRONMENT_NAME` | `development` | Environment identifier |
| `RELEASE_CHANNEL` | `dev` | Development channel |
| `BUILD_CONFIGURATION` | `Release` | .NET build configuration |

## Secrets
Required secrets for development builds:
- `GITHUB_TOKEN` (automatically provided)

## Features
- ✅ Fast build and test cycle
- ✅ No versioning/releases (development only)
- ✅ Multiple developer support
- ✅ Quick feedback on code quality

## Usage
This environment is automatically used when:
- Pushing to `dev` branch
- Creating pull requests to `dev` branch

The workflow will:
1. Build and test the application
2. Run unit tests
3. Provide quick feedback to developers
4. No releases or versioning (development focus)
