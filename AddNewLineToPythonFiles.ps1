$files = Get-ChildItem -Path . -Recurse -Include *.py -File
$fixed = 0

foreach ($file in $files) {
    try {
        $lines = Get-Content $file.FullName -Raw -Encoding UTF8
        if ($null -eq $lines) {
            continue
        }

        # Normalize all line endings
        $lines = $lines -replace "`r`n", "`n" -replace "`r", "`n"
        $linesArray = $lines -split "`n"

        # Trim trailing blank lines
        while ($linesArray.Count -gt 0 -and ($linesArray[-1] -match '^\s*$')) {
            $linesArray = $linesArray[0..($linesArray.Count - 2)]
        }

        # Add exactly one newline at the end
        $fixedText = ($linesArray -join "`n") + "`n"

        # Only write if content changed
        $originalBytes = [System.IO.File]::ReadAllBytes($file.FullName)
        $newBytes = [System.Text.Encoding]::UTF8.GetBytes($fixedText)

        if (-not ($originalBytes.SequenceEqual($newBytes))) {
            [System.IO.File]::WriteAllBytes($file.FullName, $newBytes)
            Write-Host "Fixed: $($file.FullName)"
            $fixed++
        }
    }
    catch {
        Write-Warning "Failed to process $($file.FullName): $_"
    }
}

Write-Host "DONE: $fixed Python files cleaned."
