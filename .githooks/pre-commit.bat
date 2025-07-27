@echo off
setlocal enabledelayedexpansion
REM Pre-commit hook for Windows
REM Prevents commits if compilation fails or tests fail

echo Running pre-commit hook to verify code quality...

REM Check branch naming convention
echo Checking branch naming convention...
for /f "tokens=*" %%i in ('git rev-parse --abbrev-ref HEAD') do set BRANCH_NAME=%%i

REM Check if it's a special branch (main, dev, master, hotfix)
echo %BRANCH_NAME% | findstr /r "^main$ ^dev$ ^master$ ^hotfix/" >nul
if %ERRORLEVEL% EQU 0 (
    echo ✅ Special branch '%BRANCH_NAME%' - skipping naming convention check.
    goto :build
)

REM Check if it follows feat/{number}-{description} pattern
echo %BRANCH_NAME% | findstr /r "^feat/[0-9][0-9]*-[a-zA-Z0-9-][a-zA-Z0-9-]*$" >nul
if %ERRORLEVEL% EQU 0 (
    REM Extract issue number for display
    for /f "tokens=2 delims=/" %%a in ("%BRANCH_NAME%") do (
        for /f "tokens=1 delims=-" %%b in ("%%a") do set ISSUE_NUMBER=%%b
    )
    echo ✅ Branch naming convention valid: '%BRANCH_NAME%' (references issue #!ISSUE_NUMBER!)
    goto :build
)

REM If we get here, the branch name is invalid
echo ❌ Commit failed: Branch name '%BRANCH_NAME%' does not follow the required convention.
echo    Required format: feat/{issue-number}-{description}
echo    Examples: feat/56-link-branch-to-issue, feat/123-add-new-feature
echo    Special branches allowed: main, dev, master, hotfix/*
exit /b 1

:build

REM Build the solution to ensure it compiles
echo Building solution to verify compilation...
dotnet build
if %ERRORLEVEL% NEQ 0 (
    echo [91m❌ Commit failed: Code does not compile. Please fix the errors and try again.[0m
    exit /b 1
)
echo [92m✅ Compilation successful![0m

REM Run unit tests
echo Running unit tests...
dotnet test WitcherSmartSaveManagerTests\WitcherGuiApp.Tests.csproj
if %ERRORLEVEL% NEQ 0 (
    echo [91m❌ Commit failed: Unit tests failed. Please fix the failing tests and try again.[0m
    exit /b 1
)
echo [92m✅ All tests passed![0m

REM If we got here, everything succeeded
echo [92m✅ Pre-commit checks passed! Proceeding with commit.[0m
exit /b 0
