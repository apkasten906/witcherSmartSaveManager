# Uninstallation.Tests.ps1
# Pester test for uninstallation scenario

Describe "Uninstallation" {
    It "Should remove all files and registry entries" {
        # Simulate uninstallation
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/x {8B5A2E4C-3F1D-4B9E-A6D7-2C4E8F9A1B3D} /quiet" -Wait

        # Validate uninstallation
        $defaultPath = "C:\Program Files\WitcherSmartSaveManager"
        Test-Path $defaultPath | Should -Be $false

        # Validate registry cleanup
        $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WitcherSmartSaveManager"
        Test-Path $registryPath | Should -Be $false
    }
}
