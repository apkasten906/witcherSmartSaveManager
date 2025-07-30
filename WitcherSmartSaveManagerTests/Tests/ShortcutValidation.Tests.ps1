# ShortcutValidation.Tests.ps1
# Pester test for shortcut validation scenario

Describe "Shortcut Validation" {
    It "Should create Start Menu and Desktop shortcuts" {
        # Validate Start Menu shortcut
        $startMenuShortcut = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\WitcherSmartSaveManager.lnk"
        Test-Path $startMenuShortcut | Should -Be $true

        # Validate Desktop shortcut
        $desktopShortcut = "C:\Users\Public\Desktop\WitcherSmartSaveManager.lnk"
        Test-Path $desktopShortcut | Should -Be $true
    }
}
