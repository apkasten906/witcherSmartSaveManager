# ErrorHandling.Tests.ps1
# Pester test for error handling scenario

Describe "Error Handling" {
    It "Should provide appropriate error messages for insufficient permissions" {
        # Simulate installation without admin rights
        Start-Process -FilePath "msiexec.exe" -ArgumentList "/i WitcherSmartSaveManagerInstaller.msi" -NoNewWindow -PassThru | Wait-Process

        # Validate error message
        $errorMessage = "Insufficient permissions to install to Program Files"
        # Assuming the installer logs errors to a file
        $logFile = "C:\ProgramData\WitcherSmartSaveManager\install.log"
        (Get-Content $logFile) -contains $errorMessage | Should -Be $true
    }
}
