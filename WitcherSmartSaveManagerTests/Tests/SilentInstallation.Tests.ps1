# SilentInstallation.Tests.ps1
# Pester test for silent installation scenario

Describe "Silent Installation" {
    It "Should install silently without user interaction" {
        # Simulate silent installation
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i WitcherSmartSaveManagerInstaller.msi /quiet" -Wait

        # Validate installation
        $defaultPath = "C:\Program Files\WitcherSmartSaveManager\WitcherSmartSaveManager.exe"
        Test-Path $defaultPath | Should -Be $true
    }
}
