# CustomPathInstallation.Tests.ps1
# Pester test for custom path installation scenario

Describe "Custom Path Installation" {
    It "Should install to the specified custom path" {
        # Define custom path
        $customPath = "C:\Custom\WitcherSmartSaveManager"

        # Simulate installation with detailed logging
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i WitcherSaveManagerInstaller.exe INSTALLFOLDER=$customPath /quiet /l*v C:\Windows\Temp\custom_install.log" -Wait

        # Validate installation
        Test-Path "$customPath\WitcherSmartSaveManager.exe" | Should -Be $true
    }
}
