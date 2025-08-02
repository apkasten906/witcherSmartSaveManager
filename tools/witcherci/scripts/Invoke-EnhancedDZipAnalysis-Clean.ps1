# Enhanced DZIP Decompression with Game Intelligence
# Phase 1: Reference Database Integration

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    [int]${bytes-to-extract} = 1024,
    [string]${output-format} = "console",
    [string]${database-path} = ".\database\witcher_save_manager.db"
)

$ErrorActionPreference = "Stop"

function Write-GameIntelligence {
    param([string]$Message, [string]$Color = "Green")
    Write-Host "[GAME-INTEL] $Message" -ForegroundColor $Color
}

function Write-Status {
    param([string]$Message)
    Write-Host "[DZIP-EXTRACT] $Message" -ForegroundColor Blue
}

function Get-DatabaseConnection {
    param([string]$DatabasePath)
    
    try {
        Import-Module PSSQLite -ErrorAction Stop
        if (Test-Path $DatabasePath) {
            return $DatabasePath
        }
        else {
            Write-Warning "Database not found at: $DatabasePath"
            return $null
        }
    }
    catch {
        Write-Warning "PSSQLite module not available. Running without game intelligence."
        return $null
    }
}

function Get-PatternGameMapping {
    param([string]$DatabasePath, [string]$Pattern)
    
    if (-not $DatabasePath) { return $null }
    
    try {
        $query = "SELECT pattern_text, game_concept, confidence_level, verification_status, data_type, related_entities, related_decisions, context_clues FROM PatternGameMapping WHERE pattern_text LIKE '%$Pattern%' OR pattern_text = '$Pattern';"
        $result = Invoke-SqliteQuery -Query $query -DataSource $DatabasePath
        return $result
    }
    catch {
        Write-Warning "Database query failed for pattern: $Pattern"
        return $null
    }
}

function Get-DecisionContext {
    param([string]$DatabasePath, [string]$VariableName)
    
    if (-not $DatabasePath) { return $null }
    
    try {
        $query = "SELECT decision_name, variable_name, possible_values, impact_level, affects_ending, description FROM DecisionReference WHERE variable_name LIKE '%$VariableName%';"
        $result = Invoke-SqliteQuery -Query $query -DataSource $DatabasePath
        return $result
    }
    catch {
        return $null
    }
}

function Get-QuestContext {
    param([string]$DatabasePath, [string]$QuestPattern)
    
    if (-not $DatabasePath) { return $null }
    
    try {
        $query = "SELECT quest_id, quest_name, act, chapter, quest_type, description FROM QuestReference WHERE quest_id LIKE '%$QuestPattern%' OR quest_name LIKE '%$QuestPattern%';"
        $result = Invoke-SqliteQuery -Query $query -DataSource $DatabasePath
        return $result
    }
    catch {
        return $null
    }
}

function Get-PatternsWithGameIntelligence {
    param([byte[]]$Data, [string]$DatabasePath)
    
    # Convert data to text for pattern analysis
    $text = [System.Text.Encoding]::UTF8.GetString($Data)
    $asciiText = [System.Text.Encoding]::ASCII.GetString($Data)
    
    # Original pattern categories (preserved)
    $observedPatterns = @{
        "GameEngine"     = @("SAVY", "timeInfo", "LogBlock", "facts", "attitude", "playingMusic", "world")
        "QuestSystem"    = @("questSystem", "questThread", "questBlock", "questLog", "questData")
        "StateTracking"  = @("tracked_pin_tag", "activeBool", "flagState", "idTag", "State")
        "NPCSystem"      = @("community", "formations", "attitude", "relationship")
        "DynamicContent" = @("dynamicEn", "LayerGroup", "erSpawn", "Manger")
        "DataStructures" = @("BLCK", "facts", "LogBlock")
    }
    
    # Enhanced analysis with game intelligence
    $categorizedFindings = @{}
    $gameIntelligence = @{}
    $criticalDecisions = @()
    $questData = @()
    $allFoundPatterns = @()
    
    # Check each pattern category
    foreach ($category in $observedPatterns.Keys) {
        $categoryFindings = @()
        foreach ($pattern in $observedPatterns[$category]) {
            if ($text.ToLower().Contains($pattern.ToLower()) -or $asciiText.ToLower().Contains($pattern.ToLower())) {
                $categoryFindings += $pattern
                $allFoundPatterns += $pattern
                
                # Get game intelligence for this pattern
                $gameMapping = Get-PatternGameMapping -DatabasePath $DatabasePath -Pattern $pattern
                if ($gameMapping) {
                    $gameIntelligence[$pattern] = $gameMapping
                    Write-GameIntelligence "Pattern '$pattern' mapped to: $($gameMapping.game_concept) (Confidence: $($gameMapping.confidence_level))"
                }
            }
        }
        if ($categoryFindings.Count -gt 0) {
            $categorizedFindings[$category] = $categoryFindings
        }
    }
    
    # Look for critical decision variables
    $knownDecisionVars = @("aryan_la_valette_fate", "chosen_path", "letho_encounters", "triss_relationship_level", "saskia_dragon_controlled", "political_choice")
    foreach ($decisionVar in $knownDecisionVars) {
        if ($text.ToLower().Contains($decisionVar.ToLower())) {
            $decisionContext = Get-DecisionContext -DatabasePath $DatabasePath -VariableName $decisionVar
            if ($decisionContext) {
                $criticalDecisions += @{
                    Variable = $decisionVar
                    Context  = $decisionContext
                    Found    = $true
                }
                Write-GameIntelligence "CRITICAL DECISION FOUND: $($decisionContext.decision_name)" -Color "Yellow"
            }
        }
    }
    
    # Look for quest-related data
    $questPatterns = @("q101", "q201", "q202", "q203", "q204", "q301", "aryan", "iorveth", "roche", "letho", "vergen")
    foreach ($questPattern in $questPatterns) {
        if ($text.ToLower().Contains($questPattern.ToLower())) {
            $questContext = Get-QuestContext -DatabasePath $DatabasePath -QuestPattern $questPattern
            if ($questContext) {
                $questData += $questContext
                Write-GameIntelligence "Quest data found: $($questContext.quest_name) (Act $($questContext.act))" -Color "Cyan"
            }
        }
    }
    
    return @{
        TextPreview         = $text.Substring(0, [Math]::Min(200, $text.Length))
        CategorizedFindings = $categorizedFindings
        AllFoundPatterns    = $allFoundPatterns
        GameIntelligence    = $gameIntelligence
        CriticalDecisions   = $criticalDecisions
        QuestData           = $questData
        DataSize            = $Data.Length
        PatternCount        = $allFoundPatterns.Count
        IntelligenceEnabled = ($null -ne $DatabasePath)
    }
}

function Get-DZipContent {
    param([string]$FilePath, [int]$BytesToExtract)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # DZIP Header Analysis
    $magic = [System.Text.Encoding]::ASCII.GetString($bytes[0..3])
    if ($magic -ne "DZIP") {
        throw "Not a DZIP file: $magic"
    }
    
    # Parse DZIP header structure
    $version = [BitConverter]::ToUInt32($bytes, 4)
    $compressionType = [BitConverter]::ToUInt32($bytes, 8) 
    $flags = [BitConverter]::ToUInt32($bytes, 12)
    
    Write-Status "DZIP Header parsed - Version: $version, Compression: $compressionType, Flags: $flags"
    
    # Look for compressed data start (after header)
    $headerSize = 16  # Standard DZIP header
    $compressedData = $bytes[$headerSize..($bytes.Length - 1)]
    
    Write-Status "Found $($compressedData.Length) bytes of compressed data"
    
    # Try to decompress using .NET Deflate
    try {
        $memoryStream = New-Object System.IO.MemoryStream(, $compressedData)
        $deflateStream = New-Object System.IO.Compression.DeflateStream($memoryStream, [System.IO.Compression.CompressionMode]::Decompress)
        
        $buffer = New-Object byte[] $BytesToExtract
        $bytesRead = $deflateStream.Read($buffer, 0, $BytesToExtract)
        
        $deflateStream.Close()
        $memoryStream.Close()
        
        if ($bytesRead -eq 0) {
            throw "No data decompressed - possibly wrong compression format"
        }
        
        Write-Status "Successfully decompressed $bytesRead bytes"
        return $buffer[0..($bytesRead - 1)]
    }
    catch {
        Write-Status "Deflate decompression failed: $($_.Exception.Message)"
        
        # Fallback: try raw data extraction
        Write-Status "Attempting raw data extraction..."
        $extractSize = [Math]::Min($BytesToExtract, $compressedData.Length)
        return $compressedData[0..($extractSize - 1)]
    }
}

# Main execution
try {
    Write-Status "Starting Enhanced DZIP Analysis with Game Intelligence"
    Write-Status "Save file: ${save-path}"
    
    # Initialize database connection
    $dbPath = Get-DatabaseConnection -DatabasePath ${database-path}
    if ($dbPath) {
        Write-GameIntelligence "Reference database connected: $dbPath"
    }
    else {
        Write-Status "Running without game intelligence (database unavailable)"
    }
    
    # Extract and decompress data
    $decompressedData = Get-DZipContent -FilePath ${save-path} -BytesToExtract ${bytes-to-extract}
    
    # Enhanced analysis with game intelligence
    $analysis = Get-PatternsWithGameIntelligence -Data $decompressedData -DatabasePath $dbPath
    
    # Display enhanced results
    Write-Host "`n--- ENHANCED DZIP ANALYSIS WITH GAME INTELLIGENCE ---" -ForegroundColor Green
    Write-Host "=" * 60 -ForegroundColor Green
    
    Write-Status "Data Size: $($analysis.DataSize) bytes"
    Write-Status "Total Patterns Found: $($analysis.PatternCount)"
    Write-Status "Game Intelligence: $($analysis.IntelligenceEnabled)"
    
    if ($analysis.CategorizedFindings.Count -gt 0) {
        Write-Host "`nPattern Categories Found:" -ForegroundColor Yellow
        foreach ($category in $analysis.CategorizedFindings.Keys) {
            $patterns = $analysis.CategorizedFindings[$category] -join ', '
            Write-Host "    [$category]: $patterns" -ForegroundColor Cyan
        }
    }
    
    if ($analysis.GameIntelligence.Count -gt 0) {
        Write-Host "`nGAME INTELLIGENCE INSIGHTS:" -ForegroundColor Green
        foreach ($pattern in $analysis.GameIntelligence.Keys) {
            $intel = $analysis.GameIntelligence[$pattern]
            Write-Host "    Pattern: $pattern" -ForegroundColor White
            Write-Host "      -> Game Concept: $($intel.game_concept)" -ForegroundColor Cyan
            Write-Host "      -> Confidence: $($intel.confidence_level)" -ForegroundColor Yellow
            Write-Host "      -> Status: $($intel.verification_status)" -ForegroundColor Magenta
            if ($intel.context_clues) {
                Write-Host "      -> Context: $($intel.context_clues)" -ForegroundColor Gray
            }
        }
    }
    
    if ($analysis.CriticalDecisions.Count -gt 0) {
        Write-Host "`nCRITICAL PLAYER DECISIONS DETECTED:" -ForegroundColor Red
        foreach ($decision in $analysis.CriticalDecisions) {
            if ($decision.Found) {
                $ctx = $decision.Context
                Write-Host "    DECISION: $($ctx.decision_name)" -ForegroundColor Yellow
                Write-Host "      -> Variable: $($ctx.variable_name)" -ForegroundColor White
                Write-Host "      -> Impact: $($ctx.impact_level)" -ForegroundColor Cyan
                Write-Host "      -> Affects Ending: $($ctx.affects_ending)" -ForegroundColor Magenta
                Write-Host "      -> Options: $($ctx.possible_values)" -ForegroundColor Gray
            }
        }
    }
    
    if ($analysis.QuestData.Count -gt 0) {
        Write-Host "`nQUEST PROGRESSION DATA:" -ForegroundColor Blue
        foreach ($quest in $analysis.QuestData) {
            Write-Host "    QUEST: $($quest.quest_name)" -ForegroundColor Cyan
            Write-Host "      -> ID: $($quest.quest_id)" -ForegroundColor White
            Write-Host "      -> Act $($quest.act), Chapter $($quest.chapter)" -ForegroundColor Yellow
            Write-Host "      -> Type: $($quest.quest_type)" -ForegroundColor Gray
        }
    }
    
    Write-Host "`nRaw Text Preview:" -ForegroundColor Gray
    Write-Host "  $($analysis.TextPreview)" -ForegroundColor DarkGray
    
    Write-GameIntelligence "Enhanced DZIP analysis completed with game intelligence!" -Color "Green"
}
catch {
    Write-Error "Enhanced DZIP Analysis failed: $($_.Exception.Message)"
    exit 1
}
