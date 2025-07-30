# GitHub Environments Setup Guide

## Overview
This project uses GitHub Environments to separate production and development deployments with different configurations and protection rules.

## Setting Up Environments

### 1. Navigate to Repository Settings
1. Go to your GitHub repository
2. Click **Settings** tab
3. In the left sidebar, click **Environments**

### 2. Create Production Environment
1. Click **New environment**
2. Name: `production`
3. Configure the following settings:

#### Protection Rules:
- ✅ **Required reviewers**: Add 1 reviewer (repository owner)
- ✅ **Deployment branches**: Only `main` branch
- ⚠️ **Wait timer**: Optional (e.g., 5 minutes for safety)

#### Environment Variables:
```
ENVIRONMENT_NAME = production
RELEASE_CHANNEL = stable
BUILD_CONFIGURATION = Release
```

#### Environment Secrets:
- `GITHUB_TOKEN` is automatically provided
- Add additional secrets as needed

### 3. Create Development Environment
1. Click **New environment** 
2. Name: `development`
3. Configure the following settings:

#### Protection Rules:
- ❌ **Required reviewers**: None (fast feedback)
- ✅ **Deployment branches**: `dev` branch and feature branches
- ❌ **Wait timer**: None

#### Environment Variables:
```
ENVIRONMENT_NAME = development
RELEASE_CHANNEL = dev
BUILD_CONFIGURATION = Release
```

## Environment Usage

### Production Environment (`main` branch)
- **Triggers**: Push to `main`, PR to `main`
- **Features**: Full release pipeline with versioning
- **Protection**: Requires review, controlled deployment
- **Outputs**: Tagged releases, installers, release notes

### Development Environment (`dev` branch)
- **Triggers**: Push to `dev`, PR to `dev` 
- **Features**: Build and test only
- **Protection**: No restrictions (fast feedback)
- **Outputs**: Build validation, test results

## Verification

After setup, verify environments are working:

1. **Check Environment List**: Should see `production` and `development` environments
2. **Test Development**: Push to `dev` branch should use development environment
3. **Test Production**: Push to `main` branch should use production environment
4. **Monitor Deployments**: Check environment deployment history

## Benefits

✅ **Separation of Concerns**: Different rules for different branches
✅ **Protection**: Production requires review, dev doesn't  
✅ **Visibility**: Clear deployment tracking per environment
✅ **Flexibility**: Different variables and secrets per environment
✅ **Compliance**: Audit trail for production deployments

## Next Steps

With environments configured, you can:
- ✅ **Add approval gates for production** - See [APPROVAL_GATES.md](APPROVAL_GATES.md)
- Set up environment-specific secrets
- Configure deployment notifications
- Add deployment protection rules
- Monitor deployment history and success rates
