# Production Environment Configuration

## Overview
The `production` environment is used for stable releases on the `main` branch.

## Protection Rules
- **Required Reviewers**: 1 (can be configured in GitHub Settings > Environments)
- **Wait Timer**: None (can be added for deployment delays)
- **Deployment Branches**: Only `main` branch

## Environment Variables
Set these in GitHub Settings > Environments > production:

| Variable | Value | Description |
|----------|-------|-------------|
| `ENVIRONMENT_NAME` | `production` | Environment identifier |
| `RELEASE_CHANNEL` | `stable` | Release channel for telemetry |
| `BUILD_CONFIGURATION` | `Release` | .NET build configuration |

## Secrets
Required secrets for production deployments:
- `GITHUB_TOKEN` (automatically provided)
- Additional secrets can be added as needed

## Features
- ✅ Automated semantic versioning
- ✅ Git tag creation
- ✅ Release generation with installers
- ✅ Comprehensive release notes
- ✅ Production-grade error handling

## Usage
This environment is automatically used when:
- Pushing to `main` branch
- Creating pull requests to `main` branch

The workflow will:
1. Build and test the application
2. Generate semantic version based on commits
3. Create Git tags
4. Build Windows installer
5. Create GitHub release with installer
