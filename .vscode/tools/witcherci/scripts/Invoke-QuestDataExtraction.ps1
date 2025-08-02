# Quest Data Extraction from DZIP Save Files
# Phase 2.1 - Enhanced quest parsing and structure analysis

param(
    [Parameter(Mandatory = $true)]
    [string]${save-path},
    [int]${bytes-to-extract} = 4096,
    [string]${quest-filter} = "",
    [string]${output-format} = "structured"
)

$ErrorActionPreference = "Stop"

function Write-Status {
    param([string]$Message)
    Write-Host "[QUEST-EXTRACT] $Message" -ForegroundColor Blue
}

function Get-DZipRawData {
    param([string]$FilePath, [int]$BytesToExtract)
    
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    
    # DZIP Header validation
    $magic = [System.Text.Encoding]::ASCII.GetString($bytes[0..3])
    if ($magic -ne "DZIP") {
        throw "Not a DZIP file: $magic"
    }
    
    # Skip header and return raw compressed data for analysis
    $headerSize = 16
    $compressedData = $bytes[$headerSize..($bytes.Length - 1)]
    
    # Return specified amount for deep analysis
    return $compressedData[0..[Math]::Min($BytesToExtract - 1, $compressedData.Length - 1)]
}

function Find-QuestStructures {
    param([byte[]]$Data)
    
    # Convert to searchable text
    $text = [System.Text.Encoding]::UTF8.GetString($Data)
    
    # Enhanced quest patterns for Witcher 2
    $questKeywords = @(
        # Core quest system
        "quest", "zadanie", "objective", "cel", "progress", "postep",
        "questSystem", "questThread", "questBlock", "questLog", "questData",
        
        # Quest states
        "completed", "failed", "active", "inactive", "pending", "started",
        "ukonczone", "nieudane", "aktywne", "nieaktywne", "oczekujace",
        
        # Major characters (main quest indicators)
        "geralt", "triss", "iorveth", "roche", "letho", "saskia", "philippa",
        "vernon", "foltest", "radovid", "henselt", "dethmold", "stennis",
        "dandelion", "zoltan", "yarpen", "cedric", "malena", "loredo",
        
        # Major locations (quest progression markers)
        "flotsam", "vergen", "aedirn", "kaedwen", "temeria", "pontar",
        "loc_", "area_", "region_", "map_", "zone_",
        
        # Main quest storylines
        "kingslayer", "assassin", "conspiracy", "medallion", "formula",
        "tournament", "troll", "nekker", "endrega", "kayran", "dragon",
        
        # Quest mechanics
        "tracked", "pin", "marker", "waypoint", "compass", "journal",
        "activeBool", "flagState", "questFlag", "condition", "trigger"
    )
    
    $foundStructures = @()
    
    # Look for quest system markers with enhanced detection
    $questPatterns = @{
        "QuestSystem"   = @("questSystem", "quest_system")
        "QuestThread"   = @("questThread", "quest_thread") 
        "QuestBlock"    = @("questBlock", "quest_block")
        "QuestLog"      = @("questLog", "quest_log", "LogBlock")
        "QuestData"     = @("questData", "quest_data")
        "QuestFlags"    = @("questFlag", "quest_flag", "activeBool")
        "QuestTracking" = @("tracked_pin", "waypoint", "marker")
        "GameFacts"     = @("facts", "factSystem", "gameState")
        "CharacterData" = @("attitude", "relationship", "npcState")
        "WorldState"    = @("world", "worldState", "environment")
    }
    
    foreach ($structureType in $questPatterns.Keys) {
        foreach ($pattern in $questPatterns[$structureType]) {
            $index = $text.IndexOf($pattern)
            if ($index -ge 0) {
                $contextStart = [Math]::Max(0, $index - 100)
                $contextLength = [Math]::Min(300, $text.Length - $contextStart)
                $context = $text.Substring($contextStart, $contextLength)
                
                $foundStructures += @{
                    Type        = $structureType
                    Pattern     = $pattern
                    Position    = $index
                    Context     = $context
                    ByteContext = Get-ByteContext $Data $index
                }
            }
        }
    }
    
    # Extract all meaningful strings
    $strings = Get-NullTerminatedStrings $Data
    
    # Filter for quest-related strings
    $questStrings = @()
    foreach ($str in $strings) {
        $lowerStr = $str.ToLower()
        foreach ($keyword in $questKeywords) {
            if ($lowerStr.Contains($keyword.ToLower())) {
                $questStrings += @{
                    String  = $str
                    Keyword = $keyword
                    Length  = $str.Length
                }
                break
            }
        }
    }
    
    return @{
        QuestStructures = $foundStructures
        QuestStrings    = $questStrings
        AllStrings      = $strings[0..[Math]::Min(19, $strings.Length - 1)]
        DataAnalyzed    = $Data.Length
    }
}

function Get-ByteContext {
    param([byte[]]$Data, [int]$Position)
    
    $contextSize = 32
    $start = [Math]::Max(0, $Position - $contextSize)
    $end = [Math]::Min($Data.Length - 1, $Position + $contextSize)
    
    $contextBytes = $Data[$start..$end]
    $hexString = ($contextBytes | ForEach-Object { "{0:X2}" -f $_ }) -join " "
    
    return @{
        HexDump  = $hexString
        StartPos = $start
        EndPos   = $end
        Length   = $contextBytes.Length
    }
}

function Get-NullTerminatedStrings {
    param([byte[]]$Data)
    
    $strings = @()
    $currentString = ""
    $minStringLength = 4
    
    for ($i = 0; $i -lt $Data.Length; $i++) {
        $byte = $Data[$i]
        
        if ($byte -eq 0) {
            if ($currentString.Length -ge $minStringLength) {
                $strings += $currentString
            }
            $currentString = ""
        }
        elseif ($byte -ge 32 -and $byte -le 126) {
            $currentString += [char]$byte
        }
        elseif ($byte -ge 128) {
            $currentString += [char]$byte
        }
        else {
            if ($currentString.Length -ge $minStringLength) {
                $strings += $currentString
            }
            $currentString = ""
        }
    }
    
    if ($currentString.Length -ge $minStringLength) {
        $strings += $currentString
    }
    
    return $strings
}

function Show-QuestData {
    param([hashtable]$QuestData, [string]$Filter)
    
    Write-Status "Quest Data Analysis Results:"
    Write-Host "  Total Data Analyzed: $($QuestData.DataAnalyzed) bytes" -ForegroundColor White
    Write-Host "  Quest Structures Found: $($QuestData.QuestStructures.Count)" -ForegroundColor Green
    Write-Host "  Quest-Related Strings: $($QuestData.QuestStrings.Count)" -ForegroundColor Yellow
    Write-Host "  Total Strings Extracted: $($QuestData.AllStrings.Count)" -ForegroundColor Cyan
    
    if ($QuestData.QuestStructures.Count -gt 0) {
        Write-Host "`n🗡️ Quest Structures Found:" -ForegroundColor Magenta
        foreach ($structure in $QuestData.QuestStructures) {
            Write-Host "    [$($structure.Type)] '$($structure.Pattern)' at position $($structure.Position)" -ForegroundColor White
            Write-Host "      Context: $($structure.Context.Substring(0, [Math]::Min(150, $structure.Context.Length)))" -ForegroundColor Gray
            
            if ($structure.ByteContext) {
                Write-Host "      Hex Context: $($structure.ByteContext.HexDump.Substring(0, [Math]::Min(64, $structure.ByteContext.HexDump.Length)))..." -ForegroundColor DarkGray
            }
            Write-Host ""
        }
    }
    
    if ($QuestData.QuestStrings.Count -gt 0) {
        Write-Host "`n📜 Quest-Related Strings by Category:" -ForegroundColor Green
        
        # Group quest strings by category
        $categories = @{
            "Characters"  = @("geralt", "triss", "iorveth", "roche", "letho", "saskia", "philippa", "vernon", "foltest")
            "Locations"   = @("flotsam", "vergen", "aedirn", "kaedwen", "temeria", "loc_", "area_", "region_")
            "QuestSystem" = @("quest", "zadanie", "objective", "questSystem", "questThread", "questBlock")
            "States"      = @("completed", "failed", "active", "inactive", "tracked", "activeBool", "flagState")
            "Storylines"  = @("kingslayer", "assassin", "conspiracy", "medallion", "formula", "tournament")
        }
        
        $filteredStrings = if ($Filter) { 
            $QuestData.QuestStrings | Where-Object { $_.String.ToLower().Contains($Filter.ToLower()) }
        }
        else { 
            $QuestData.QuestStrings 
        }
        
        foreach ($category in $categories.Keys) {
            $categoryStrings = $filteredStrings | Where-Object { 
                $keyword = $_.Keyword.ToLower()
                $categories[$category] | ForEach-Object { $keyword.Contains($_.ToLower()) } | Where-Object { $_ -eq $true }
            }
            
            if ($categoryStrings.Count -gt 0) {
                Write-Host "    [$category]:" -ForegroundColor Yellow
                foreach ($questString in $categoryStrings) {
                    Write-Host "      - [$($questString.Keyword)]: $($questString.String)" -ForegroundColor White
                }
            }
        }
        
        # Show uncategorized strings
        $uncategorizedStrings = $filteredStrings | Where-Object {
            $keyword = $_.Keyword.ToLower()
            $isInCategory = $false
            foreach ($categoryKeywords in $categories.Values) {
                if ($categoryKeywords | ForEach-Object { $keyword.Contains($_.ToLower()) } | Where-Object { $_ -eq $true }) {
                    $isInCategory = $true
                    break
                }
            }
            -not $isInCategory
        }
        
        if ($uncategorizedStrings.Count -gt 0) {
            Write-Host "    [Other]:" -ForegroundColor Yellow
            foreach ($questString in $uncategorizedStrings) {
                Write-Host "      - [$($questString.Keyword)]: $($questString.String)" -ForegroundColor White
            }
        }
    }
    
    Write-Host "`n🔍 All Extracted Strings (sample):" -ForegroundColor Cyan
    foreach ($str in $QuestData.AllStrings) {
        Write-Host "    - $str" -ForegroundColor White
    }
}

try {
    Write-Status "Starting Quest Data Extraction"
    Write-Status "Save Path: '${save-path}'"
    Write-Status "Bytes to Extract: ${bytes-to-extract}"
    if (${quest-filter}) {
        Write-Status "Filter: '${quest-filter}'"
    }
    
    # Navigate to project root and resolve path
    $projectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)))
    Set-Location $projectRoot
    
    $fullSavePath = Join-Path $projectRoot ${save-path}
    if (-not (Test-Path $fullSavePath)) {
        throw "Save file not found: $fullSavePath"
    }
    
    Write-Status "Extracting quest data from: $fullSavePath"
    $rawData = Get-DZipRawData $fullSavePath ${bytes-to-extract}
    
    $questData = Find-QuestStructures $rawData
    
    Show-QuestData $questData ${quest-filter}
    
    Write-Status "Quest data extraction completed successfully"
}
catch {
    Write-Error "Quest Data Extraction failed: $($_.Exception.Message)"
    exit 1
}
