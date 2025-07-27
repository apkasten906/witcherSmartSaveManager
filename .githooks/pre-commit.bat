@echo off
REM Pre-commit hook for Windows
REM Prevents commits if compilation fails or tests fail

echo Running pre-commit hook to verify code quality...

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
