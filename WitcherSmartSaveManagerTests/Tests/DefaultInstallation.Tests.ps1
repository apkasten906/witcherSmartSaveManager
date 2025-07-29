# DefaultInstallation.Tests.ps1
# Pester test for default installation scenario

Describe "Default Installation" {
    It "Should install to the default path" {
        # Simulate installation
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i WitcherSmartSaveManagerInstaller.msi /quiet" -Wait

        # Validate installation
        $defaultPath = "C:\Program Files\WitcherSmartSaveManager\WitcherSmartSaveManager.exe"
        Test-Path $defaultPath | Should -Be $true
    }
}
