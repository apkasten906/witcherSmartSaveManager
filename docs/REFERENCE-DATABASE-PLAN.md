# Witcher 2 Reference Database Development Plan

## 🎯 Strategic Approach: Reference-First Architecture

### **The Problem We're Solving**
Our DZIP analysis discovers patterns like `questSystem`, `flagState`, and `activeBool`, but we lack the **context** to interpret them meaningfully. We need to build authoritative reference data before we can properly decode save files.

## 📊 Phase 1: Expand Reference Database

### 1.1 Enhanced Language Resources
**Current**: 7,153 entries (English only)
**Target**: Comprehensive multi-language quest/character reference

```sql
-- Expand LanguageResources with game-specific data
INSERT INTO LanguageResources (Key, Value, Language) VALUES
('quest.prologue.aryan_choice', 'Spare or Kill Aryan La Valette', 'en'),
('quest.ch1.path_choice', 'Choose Iorveth or Roche Path', 'en'),
-- ... continue with known quest structure
```

### 1.2 Game Knowledge Tables
Create new tables to store authoritative game data:

```sql
-- Witcher 2 quest reference data
CREATE TABLE QuestReference (
    Id INTEGER PRIMARY KEY,
    QuestId TEXT UNIQUE NOT NULL,       -- 'q001_prologue', 'q101_kayran'
    QuestName TEXT NOT NULL,            -- 'The Kayran'
    Chapter TEXT NOT NULL,              -- 'Prologue', 'Chapter1', 'Chapter2'
    QuestType TEXT,                     -- 'main', 'side', 'character'
    Description TEXT,
    Prerequisites TEXT,                 -- JSON of required quest states
    DecisionPoints TEXT,                -- JSON of critical choices
    ConsequenceData TEXT                -- JSON of outcomes/impacts
);

-- Character and location reference
CREATE TABLE GameEntities (
    Id INTEGER PRIMARY KEY,
    EntityId TEXT UNIQUE NOT NULL,      -- 'char_triss', 'loc_flotsam'
    EntityType TEXT NOT NULL,           -- 'character', 'location', 'item'
    DisplayName TEXT NOT NULL,          -- 'Triss Merigold', 'Flotsam'
    Chapter TEXT,                       -- When entity is available
    RelatedQuests TEXT,                 -- JSON array of quest IDs
    Description TEXT
);

-- Known decision variables and their meanings
CREATE TABLE DecisionReference (
    Id INTEGER PRIMARY KEY,
    VariableName TEXT UNIQUE NOT NULL,  -- 'aryan_la_valette_fate'
    VariableType TEXT NOT NULL,         -- 'boolean', 'enum', 'integer'
    PossibleValues TEXT NOT NULL,       -- JSON: ['killed', 'spared']
    QuestContext TEXT NOT NULL,         -- 'q001_prologue'
    ImpactLevel INTEGER,                -- 1-5 scale
    Description TEXT,
    Consequences TEXT                   -- JSON of narrative outcomes
);
```

### 1.3 Pattern Mapping Tables
Connect our DZIP discoveries to meaningful game data:

```sql
-- Map DZIP patterns to game concepts
CREATE TABLE PatternGameMapping (
    Id INTEGER PRIMARY KEY,
    DZipPattern TEXT NOT NULL,          -- 'questSystem', 'activeBool'
    GameConcept TEXT NOT NULL,          -- 'quest_state', 'boolean_flag'
    ExpectedDataType TEXT,              -- 'string', 'boolean', 'integer'
    SampleValues TEXT,                  -- JSON examples from DZIP
    RelatedEntities TEXT,               -- JSON: quest IDs, character names
    AnalysisNotes TEXT
);
```

## 📚 Phase 2: Populate Reference Data

### 2.1 Extract from Existing Codebase
From our code analysis, we can extract:

**Quest Knowledge**:
```csharp
// From WitcherSaveFileAnalyzer.cs and parser comments
var knownQuests = new[] {
    ("q001_prologue", "Prologue", "Aryan La Valette Choice"),
    ("q101_kayran", "Chapter 1", "The Kayran"),
    // Extract from parser code comments
};

var knownCharacters = new[] {
    ("geralt", "Geralt of Rivia", "protagonist"),
    ("triss", "Triss Merigold", "main_character"),
    ("iorveth", "Iorveth", "path_choice_leader"),
    ("roche", "Vernon Roche", "path_choice_leader"),
    // From DZIP analysis and parser code
};
```

**Critical Decisions**:
```csharp
// From WITCHER2-PARSING-PLAN.md analysis
var criticalDecisions = new[] {
    ("aryan_la_valette_fate", new[] {"killed", "spared"}, "prologue"),
    ("chosen_path", new[] {"iorveth", "roche"}, "chapter1"),
    ("letho_encounters", new[] {"killed", "spared"}, "various"),
    // Extract from planning documents
};
```

### 2.2 Cross-Reference with DZIP Findings
Connect our discovered patterns to game concepts:

```csharp
var patternMappings = new[] {
    ("questSystem", "active_quest_tracker", "string"),
    ("questThread", "quest_progression_state", "enum"),
    ("questBlock", "quest_completion_data", "complex"),
    ("activeBool", "boolean_game_flags", "boolean"),
    ("flagState", "variable_state_tracker", "enum"),
    ("facts", "game_world_facts", "key_value_pairs"),
    // Map our DZIP discoveries to meaning
};
```

## 🔧 Phase 3: Reference Database Integration Tools

### 3.1 Database Population Script
Create PowerShell script to populate reference tables:

```powershell
# Populate-WitcherReferenceDatabase.ps1
param(
    [string]$DatabasePath = "witcher_save_manager.db"
)

# Populate QuestReference from known game data
# Populate GameEntities from character/location knowledge  
# Populate DecisionReference from critical choice analysis
# Populate PatternGameMapping from our DZIP discoveries
```

### 3.2 Enhanced DZIP Analysis with Reference Validation
Modify our DZIP scripts to cross-reference discoveries:

```powershell
# Enhanced pattern recognition with database lookup
function Validate-PatternWithReference {
    param($Pattern, $Context, $DatabaseConnection)
    
    # Query reference database for known patterns
    $knownPattern = Invoke-SqliteQuery -Query @"
        SELECT GameConcept, ExpectedDataType, SampleValues 
        FROM PatternGameMapping 
        WHERE DZipPattern = '$Pattern'
    "@
    
    if ($knownPattern) {
        Write-Host "Validated pattern: $Pattern maps to $($knownPattern.GameConcept)"
        return $true
    } else {
        Write-Host "Unknown pattern discovered: $Pattern - adding to research queue"
        return $false
    }
}
```

## 🎯 Phase 4: Smart Save Analysis Engine

### 4.1 Context-Aware Pattern Recognition
With reference database in place, transform DZIP analysis:

```powershell
# Instead of: "Found questSystem at position 1234"
# Provide: "Found active quest tracker - likely contains: Prologue, Chapter1, or Chapter2 quest data"

# Instead of: "Found activeBool patterns"  
# Provide: "Found boolean flags - checking for: aryan_fate, path_choice, relationship_flags"
```

### 4.2 Intelligent Save File Intelligence
Create smart analysis that references game knowledge:

```csharp
public class SaveFileIntelligenceEngine
{
    public SaveIntelligenceReport AnalyzeWithGameContext(byte[] saveData)
    {
        var patterns = ExtractDZipPatterns(saveData);
        var gameContext = _referenceDb.GetGameContext();
        
        return new SaveIntelligenceReport
        {
            LikelyQuests = MatchPatternsToQuests(patterns, gameContext),
            CriticalDecisions = IdentifyDecisionPoints(patterns, gameContext),
            GameProgression = EstimateChapterProgress(patterns, gameContext),
            SmartSaveRecommendation = GenerateRecommendation(patterns, gameContext)
        };
    }
}
```

## 🚀 **Immediate Next Steps**

1. **Create reference database population script**
2. **Extract game knowledge from existing codebase**
3. **Populate QuestReference, GameEntities, DecisionReference tables**
4. **Enhance DZIP analysis with reference validation**
5. **Build context-aware pattern recognition**

This approach gives us **authoritative game knowledge** to properly interpret our DZIP discoveries, rather than guessing what patterns mean!
