# Enhanced DZIP Analysis - Decision Variable Hunter
# This version specifically searches for critical decision variables

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    
    [Parameter(Mandatory = $false)]
    [int]${bytes-to-extract} = 32768,
    
    [Parameter(Mandatory = $false)]
    [string]${output-format} = "detailed",
    
    [Parameter(Mandatory = $false)]
    [string]${database-path} = "database\witcher_save_manager.db"
)

Write-Host "[DECISION-HUNTER] Starting Decision Variable Hunt" -ForegroundColor Cyan
Write-Host "[DECISION-HUNTER] Save file: ${save-path}"

# Read the save file
try {
    $saveData = [System.IO.File]::ReadAllBytes(${save-path})
    Write-Host "[DECISION-HUNTER] File size: $($saveData.Length) bytes"
}
catch {
    Write-Error "Failed to read save file: $($_.Exception.Message)"
    return
}

# Extract larger chunk for decision hunting
$extractSize = [Math]::Min(${bytes-to-extract}, $saveData.Length - 16)
$dataChunk = $saveData[16..($extractSize + 15)]
$textData = [System.Text.Encoding]::ASCII.GetString($dataChunk)

Write-Host "[DECISION-HUNTER] Extracted $($dataChunk.Length) bytes for analysis"

# Hunt for critical decision variables
$decisionPatterns = @(
    @{Pattern = "aryan"; Context = "Aryan La Valette fate decision" },
    @{Pattern = "roche"; Context = "Vernon Roche path choice" },
    @{Pattern = "iorveth"; Context = "Iorveth path choice" },
    @{Pattern = "chosen_path"; Context = "Critical storyline path" },
    @{Pattern = "la_valette"; Context = "La Valette family decisions" },
    @{Pattern = "spared|killed"; Context = "Character fate outcomes" },
    @{Pattern = "q101|q201|q301"; Context = "Major quest identifiers" },
    @{Pattern = "act1|act2|act3"; Context = "Story act progression" }
)

$foundDecisions = @()

foreach ($decision in $decisionPatterns) {
    $regexMatches = [regex]::Matches($textData, $decision.Pattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    if ($regexMatches.Count -gt 0) {
        $foundDecisions += @{
            Pattern   = $decision.Pattern
            Context   = $decision.Context
            Matches   = $regexMatches.Count
            Positions = $regexMatches | ForEach-Object { $_.Index }
        }
        Write-Host "[DECISION-FOUND] $($decision.Pattern): $($regexMatches.Count) matches - $($decision.Context)" -ForegroundColor Green
    }
}

# Look for facts that might contain decisions
$factsMatches = [regex]::Matches($textData, "facts.*?(?=\x00|\xFF|\x01)", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
foreach ($factMatch in $factsMatches) {
    $factText = $factMatch.Value
    Write-Host "[FACTS-BLOCK] Found facts block: $($factText.Substring(0, [Math]::Min(100, $factText.Length)))..." -ForegroundColor Yellow
    
    # Check if facts contain decision keywords
    foreach ($decision in $decisionPatterns) {
        if ($factText -match $decision.Pattern) {
            Write-Host "  -> Contains decision pattern: $($decision.Pattern)" -ForegroundColor Magenta
        }
    }
}

# Output summary
Write-Host "`n=== DECISION VARIABLE HUNT RESULTS ===" -ForegroundColor Cyan
Write-Host "Save File: ${save-path}"
Write-Host "Bytes Analyzed: $($dataChunk.Length)"
Write-Host "Decision Patterns Found: $($foundDecisions.Count)"

if ($foundDecisions.Count -gt 0) {
    Write-Host "`nDETECTED DECISIONS:" -ForegroundColor Green
    foreach ($decision in $foundDecisions) {
        Write-Host "  $($decision.Pattern) ($($decision.Matches) matches): $($decision.Context)" -ForegroundColor White
    }
}
else {
    Write-Host "No decision variables detected in this save file extract" -ForegroundColor Yellow
}

Write-Host "`n[DECISION-HUNTER] Analysis complete!" -ForegroundColor Cyan
