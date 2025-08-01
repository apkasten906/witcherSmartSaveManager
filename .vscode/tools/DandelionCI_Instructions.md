# DandelionCI Wiki

## 📖 What is DandelionCI?

**DandelionCI** is a **secure file-based task runner framework** designed to safely bridge the gap between AI agents and .NET build operations. It provides a controlled environment where AI systems can trigger build, test, and deployment tasks without direct access to sensitive commands.

### 🎯 Key Features
- **File-Based Communication** - AI agents drop JSON task files, system processes them automatically
- **Security-First Design** - Multiple layers of validation prevent command injection attacks
- **Multi-Project Support** - Works with both individual `.csproj` files and complete `.sln` solutions
- **Real-Time Processing** - 5-second polling ensures responsive workflows
- **Comprehensive Logging** - Full audit trail of all operations and security events

### 🔒 Security Model
DandelionCI acts as a **security gateway** that:
- ✅ **Whitelists** only approved commands from `allowed_commands.json`
- ✅ **Validates** all input parameters against injection attacks
- ✅ **Blocks** path traversal attempts and unauthorized file access
- ✅ **Logs** all security violations for audit purposes

---

## 🚀 Quick Setup Guide

### Option 1: Professional Windows Installer (Recommended)
1. **Download** the latest `DandelionCI-Setup.exe` from releases
2. **Run installer** (will request admin privileges for Program Files installation)
3. **Follow setup wizard** - choose installation directory, enable PATH integration
4. **Start immediately** - Use desktop shortcut or Start Menu to launch DevAgent

### Option 2: Manual Installation (Developers)

#### Prerequisites
- **Windows** with PowerShell 5.1+
- **.NET SDK** (any recent version)
- **Git** (for cloning the repository)

#### Step 1: Clone the Repository
```bash
git clone https://github.com/apkasten906/DandelionCI.git
cd DandelionCI
```

#### Step 2: Verify Your Environment
```powershell
# Check PowerShell version (should be 5.1+)
$PSVersionTable.PSVersion

# Check .NET SDK
dotnet --version

# Verify you're in the project root
ls allowed_commands.json
```

#### Step 3: Test the Installation
```powershell
# Run the integration test suite
.\run-integration-tests.bat

# Expected output: "All 13 tests passed successfully!"
```

### Step 4: Start the DevAgent

#### For Installed Version (via installer):
```powershell
# Use Start Menu shortcuts or desktop icon
# OR use the convenient batch file:
start-dandelion.bat
```

#### For Manual Installation:
```powershell
# Option 1: Use the batch wrapper (recommended)
.\start-devagent.bat

# Option 2: Start manually from scripts directory
cd scripts
.\devagent-watcher.ps1
```

### Step 5: Test with a Simple Task
```powershell
# In a new terminal, create a test task
'{"task": "list-projects"}' | Out-File -FilePath "task-test.json"

# Check the result (appears within 5 seconds)
Get-Content "result-task-test.json" | ConvertFrom-Json
```

---

## 📋 Available Commands

| Task | Description | Requires Project | Example |
|------|-------------|------------------|---------|
| `build` | Build project or solution | ✅ | `{"task": "build", "project": "MyApp/MyApp.csproj"}` |
| `test` | Run unit tests | ✅ | `{"task": "test", "project": "Tests/MyTests.csproj"}` |
| `clean` | Clean build artifacts | ✅ | `{"task": "clean", "project": "MySolution.sln"}` |
| `restore` | Restore NuGet packages | ✅ | `{"task": "restore", "project": "MyApp/MyApp.csproj"}` |
| `publish` | Publish application | ✅ | `{"task": "publish", "project": "MyApp/MyApp.csproj"}` |
| `list-projects` | Discover available projects | ❌ | `{"task": "list-projects"}` |

---

## 🔧 Usage Examples

### Discover Available Projects
```powershell
'{"task": "list-projects"}' | Out-File -FilePath "task-discover.json"
# Wait 5 seconds, then check:
Get-Content "result-task-discover.json" | ConvertFrom-Json
```

### Build a Specific Project
```powershell
'{"task": "build", "project": "TestProjects/TestApp/TestApp.csproj"}' | Out-File -FilePath "task-build.json"
# Result appears in result-task-build.json
```

### Run Tests
```powershell
'{"task": "test", "project": "TestProjects/Tests/TestApp.Tests/TestApp.Tests.csproj"}' | Out-File -FilePath "task-test.json"
# Check result-task-test.json for test results
```

### Build Entire Solution
```powershell
'{"task": "build", "project": "TestProjects/TestSolution.sln"}' | Out-File -FilePath "task-build-all.json"
# Builds all projects in the solution
```

---

## 🛠️ Project Structure

```
DandelionCI/
├── allowed_commands.json      # Whitelist of permitted tasks
├── scripts/
│   ├── devagent.ps1          # Core task executor
│   ├── devagent-watcher.ps1  # File monitoring service
│   ├── devagent.bat          # Batch wrapper
│   └── devagent-watcher.bat  # Batch wrapper for watcher
├── TestProjects/             # Sample .NET projects for testing
│   ├── TestApp/              # Console application
│   ├── TestLibrary/          # Class library
│   ├── Tests/TestApp.Tests/  # Unit test project
│   └── TestSolution.sln      # Complete solution file
├── start-devagent.bat        # Quick start script
├── run-integration-tests.bat # Test suite runner
└── README.md                 # Detailed documentation
```

---

## 🔍 Troubleshooting

### DevAgent Won't Start
```powershell
# Check PowerShell execution policy
Get-ExecutionPolicy

# If Restricted, use batch wrappers:
.\start-devagent.bat
```

### Tasks Not Processing
```powershell
# Check if devagent is running
Get-Process | Where-Object {$_.ProcessName -like "*powershell*"}

# Check the log file
Get-Content devagent.log -Tail 10
```

### Invalid Task Errors
```powershell
# Verify task name exists in allowed_commands.json
Get-Content allowed_commands.json | ConvertFrom-Json | Select-Object -ExpandProperty PSObject.Properties | Select-Object Name

# Check project file exists
Test-Path "../YourProject/YourProject.csproj"
```

### Security Violations
All security violations are logged with details:
```powershell
# Check security log entries
Select-String "SECURITY" devagent.log
```

---

## 🚨 Security Guidelines

### ✅ Safe Patterns
```json
{"task": "build", "project": "MyApp/MyApp.csproj"}
{"task": "test", "project": "Tests/MyTests/MyTests.csproj"}
{"task": "list-projects"}
```

### ❌ Blocked Patterns
```json
{"task": "build; rm -rf /", "project": "test.csproj"}     // Command injection
{"task": "build", "project": "../../../etc/passwd"}      // Path traversal
{"task": "build", "project": "C:/Windows/system32/"}     // Absolute paths
{"task": "build", "project": "malicious.exe"}            // Wrong extension
```

### 🛡️ Security Layers
1. **Task Name Validation** - Only alphanumeric, dash, underscore allowed
2. **Project Path Validation** - Relative paths only, correct extensions
3. **Command Template Validation** - Pre-approved command structures
4. **Parameter Escaping** - Safe substitution prevents injection
5. **File Existence Verification** - Projects must exist before execution
6. **Output Logging** - All operations recorded for audit

---

## 🎯 AI Agent Integration

### Workflow Pattern
1. **Discover Projects** - Use `list-projects` to find available targets
2. **Validate Paths** - Ensure project files exist and have correct extensions
3. **Submit Tasks** - Create JSON task files with validated parameters
4. **Monitor Results** - Poll for result files or watch for completion
5. **Handle Errors** - Parse result status and output for error handling

### Best Practices for AI Agents
- Always use `list-projects` before targeting specific projects
- Validate project paths against the discovery results
- Handle both `.csproj` and `.sln` file types appropriately
- Implement proper error handling for failed tasks
- Clean up task/result files after processing (optional)

---

## 📊 Testing & Validation

### Integration Test Suite
```powershell
# Run all tests (functional + security)
.\run-integration-tests.bat

# Quick validation test
.\quick-test.ps1
```

### Test Coverage
- **13 Total Tests** (5 functional, 8 security)
- **Functional Tests** - Build, test, clean, restore, list operations
- **Security Tests** - All known attack vectors validated and blocked
- **Real Projects** - Tests use actual .NET projects for authenticity

---

## 📈 Version History

- **v1.0.0** - Initial release with basic task execution
- **v1.1.0** - Added batch wrappers for execution policy handling  
- **v1.2.0** - Major enhancement with multi-project support, comprehensive security, and testing framework
- **v1.3.0** - Easy installation with automated setup, professional Windows installer, and enhanced user experience

---

## 💾 Installation Options

### Professional Windows Installer
- **File**: `DandelionCI-Setup.exe` (2MB)
- **Location**: Installs to `C:\Program Files\DandelionCI`
- **Features**: 
  - ✅ Prerequisites checking (.NET SDK)
  - ✅ Start Menu integration
  - ✅ PATH environment variable setup
  - ✅ Desktop shortcuts (optional)
  - ✅ Automatic configuration
  - ✅ Professional uninstaller

### Manual Installation
- **For**: Developers who want source code access
- **Requirements**: Git, PowerShell, .NET SDK
- **Benefits**: Full customization, development environment

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feat/amazing-feature`)
3. Run the test suite (`.\run-integration-tests.bat`)
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feat/amazing-feature`)
6. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

- **Issues** - Report bugs via GitHub Issues
- **Discussions** - General questions via GitHub Discussions
- **Security** - Report security issues privately to the maintainers

---

*DandelionCI - Safely bridging AI agents and .NET build operations* 🌻
