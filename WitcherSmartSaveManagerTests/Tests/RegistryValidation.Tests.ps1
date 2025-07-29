# RegistryValidation.Tests.ps1
# Pester test for registry validation scenario

Describe "Registry Validation" {
    It "Should create correct registry entries during installation" {
        # Validate registry entries
        $registryPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\WitcherSmartSaveManager"
        Test-Path $registryPath | Should -Be $true

        $displayName = (Get-ItemProperty -Path $registryPath).DisplayName
        $displayName | Should -Be "Witcher Smart Save Manager"
    }
}
