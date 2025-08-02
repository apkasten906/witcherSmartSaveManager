# Witcher Save Manager - Database Schema Documentation

## 🎯 Database Purpose

The `witcher_save_manager.db` SQLite database stores parsed and analyzed data from Witcher game save files. It serves as the authoritative source for:

- **Quest progression tracking** across save files
- **Character variables and flags** for game state analysis  
- **Inventory management** and item tracking
- **Save file metadata** and relationships
- **Localization resources** for multi-language support
- **Cross-referencing patterns** discovered through DZIP decompression

## 📊 Database Tables Overview

### Core Save File Tables

#### `SaveFileMetadata`
**Purpose**: Primary save file information and metadata
```sql
CREATE TABLE SaveFileMetadata (
    Id INTEGER PRIMARY KEY,
    FileName TEXT NOT NULL,           -- Save file name (e.g., "AutoSave_0039.sav")
    GameKey TEXT NOT NULL,            -- Game identifier ("Witcher1", "Witcher2", "Witcher3")
    FullPath TEXT NOT NULL,           -- Complete file system path
    ScreenshotPath TEXT,              -- Associated screenshot file path
    FileSize INTEGER,                 -- File size in bytes
    LastModified DATETIME NOT NULL,   -- File modification timestamp
    ModifiedTimeIso TEXT,             -- ISO format modification time
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    SaveFileId INTEGER                -- Self-referencing relationship
);
```

#### `SaveFiles` (Legacy)
**Purpose**: Simplified save file tracking (being migrated to SaveFileMetadata)
```sql
CREATE TABLE SaveFiles (
    Id INTEGER PRIMARY KEY,
    FileName TEXT NOT NULL,
    Timestamp DATETIME NOT NULL,
    GameState TEXT
);
```

### Game Data Analysis Tables

#### `QuestInfo`
**Purpose**: Quest progression and state tracking from DZIP-parsed save data
```sql
CREATE TABLE QuestInfo (
    Id INTEGER PRIMARY KEY,
    SaveFileId INTEGER NOT NULL REFERENCES SaveFileMetadata(Id),
    QuestName TEXT,                   -- Quest identifier from save file
    QuestPhase TEXT,                  -- Current quest phase/step
    QuestDescription TEXT,            -- Human-readable quest description
    IsCompleted BOOLEAN DEFAULT 0,    -- Quest completion status
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Data Sources**: 
- DZIP patterns: `questSystem`, `questThread`, `questBlock`
- Save file variables: quest flags and progression markers

#### `CharacterVariables`
**Purpose**: Game variables, flags, and character state from save files
```sql
CREATE TABLE CharacterVariables (
    Id INTEGER PRIMARY KEY,
    SaveFileId INTEGER NOT NULL REFERENCES SaveFileMetadata(Id),
    VariableName TEXT NOT NULL,       -- Variable/flag name from save file
    VariableValue TEXT,               -- Current value
    VariableType TEXT,                -- Data type (boolean, integer, string)
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

**Data Sources**:
- DZIP patterns: `facts`, `flagState`, `activeBool`, `tracked_pin_tag`
- Character progression variables and game state flags

#### `InventoryItems`
**Purpose**: Player inventory tracking and item management
```sql
CREATE TABLE InventoryItems (
    Id INTEGER PRIMARY KEY,
    SaveFileId INTEGER NOT NULL REFERENCES SaveFileMetadata(Id),
    ItemName TEXT NOT NULL,           -- Item name/identifier
    ItemId TEXT,                      -- Unique item identifier from save
    Quantity INTEGER DEFAULT 1,       -- Item quantity
    ItemType TEXT,                    -- Item category (weapon, armor, consumable)
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### `GameDetails`
**Purpose**: General game state information and metadata
```sql
CREATE TABLE GameDetails (
    Id INTEGER PRIMARY KEY,
    SaveFileId INTEGER NOT NULL REFERENCES SaveFiles(Id),
    DetailKey TEXT NOT NULL,          -- Detail category/key
    DetailValue TEXT NOT NULL         -- Detail value
);
```

**Data Sources**:
- DZIP patterns: `world`, `playingMusic`, `timeInfo`, `attitude`
- General game engine state and configuration

### Support Tables

#### `LanguageResources`
**Purpose**: Localization strings for quest names, descriptions, and UI elements
```sql
CREATE TABLE LanguageResources (
    Id INTEGER PRIMARY KEY,
    Key TEXT NOT NULL,                -- Resource key identifier
    Value TEXT NOT NULL,              -- Localized text value
    Language TEXT NOT NULL            -- Language code (en, de, pl)
);
```

**Current Data**: 7,153 localization entries supporting English, German, and Polish

#### `DatabaseVersion`
**Purpose**: Schema version tracking for migrations
```sql
CREATE TABLE DatabaseVersion (
    Version TEXT PRIMARY KEY,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### New Reference Tables (Phase 1 Addition)

#### `QuestReference`
**Purpose**: Authoritative Witcher 2 quest database for pattern validation
```sql
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
```

#### `GameEntities`
**Purpose**: Characters, locations, and items reference for context validation
```sql
CREATE TABLE GameEntities (
    Id INTEGER PRIMARY KEY,
    EntityId TEXT UNIQUE NOT NULL,      -- 'char_triss', 'loc_flotsam'
    EntityType TEXT NOT NULL,           -- 'character', 'location', 'item'
    DisplayName TEXT NOT NULL,          -- 'Triss Merigold', 'Flotsam'
    Chapter TEXT,                       -- When entity is available
    RelatedQuests TEXT,                 -- JSON array of quest IDs
    Description TEXT
);
```

#### `DecisionReference`
**Purpose**: Known decision variables and their game impact
```sql
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

#### `PatternGameMapping`
**Purpose**: Map DZIP patterns to game concepts for intelligent analysis
```sql
CREATE TABLE PatternGameMapping (
    Id INTEGER PRIMARY KEY,
    DZipPattern TEXT NOT NULL,          -- 'questSystem', 'activeBool'
    GameConcept TEXT NOT NULL,          -- 'quest_state', 'boolean_flag'
    ExpectedDataType TEXT,              -- 'string', 'boolean', 'integer'
    SampleValues TEXT,                  -- JSON examples from DZIP
    RelatedEntities TEXT,               -- JSON: quest IDs, character names
    AnalysisNotes TEXT,
    ConfidenceLevel INTEGER DEFAULT 1   -- 1-5 validation confidence
);
```

## 🔗 REVISED Data Flow Architecture

### 1. Reference-First Approach: Knowledge → Analysis → Storage
```
Codebase Game Knowledge (comments, planning docs)
    ↓ Extraction & Consolidation
Reference Database Population (QuestReference, GameEntities, etc.)
    ↓ Authoritative Game Context
Enhanced DZIP Analysis with Validation
    ↓ Contextual Pattern Recognition
Intelligent Save File Interpretation
    ↓ Meaningful Data Storage
Structured Game Data in Analysis Tables
```

### 2. Enhanced Pattern Recognition with Game Context
Based on our **Reference-First** strategy:

- **GameEngine**: `SAVY`, `timeInfo`, `LogBlock` → Validated against game version data
- **QuestSystem**: `questSystem`, `questThread` → Cross-referenced with `QuestReference` table
- **StateTracking**: `activeBool`, `flagState` → Mapped to known `DecisionReference` variables
- **NPCSystem**: `community`, `formations` → Validated against `GameEntities` character data
- **DynamicContent**: `LayerGroup`, `erSpawn` → Contextualized with location/chapter data

### 3. Intelligent Cross-Reference Validation
The enhanced database enables **contextual** validation of DZIP parsing results:

```sql
-- Validate DZIP quest patterns against authoritative quest database
SELECT 
    pm.DZipPattern,
    pm.GameConcept,
    qr.QuestName,
    qr.Chapter
FROM PatternGameMapping pm
JOIN QuestReference qr ON pm.RelatedEntities LIKE '%' || qr.QuestId || '%'
WHERE pm.DZipPattern IN ('questSystem', 'questThread', 'questBlock');

-- Cross-reference discovered variables with known decision database
SELECT 
    cv.VariableName,
    cv.VariableValue,
    dr.Description,
    dr.ImpactLevel,
    dr.Consequences
FROM CharacterVariables cv
JOIN DecisionReference dr ON cv.VariableName = dr.VariableName
JOIN SaveFileMetadata sm ON cv.SaveFileId = sm.Id;

-- Contextual entity validation for discovered game elements
SELECT 
    ge.DisplayName,
    ge.EntityType,
    ge.Chapter,
    pm.DZipPattern,
    pm.SampleValues
FROM GameEntities ge
JOIN PatternGameMapping pm ON pm.RelatedEntities LIKE '%' || ge.EntityId || '%'
WHERE ge.EntityType IN ('character', 'location');
```

## 🛠️ REVISED Integration with WitcherCI

### Reference-First Database Population Workflow
1. **Knowledge Extraction**: Extract game data from existing codebase (parser comments, planning docs)
2. **Reference Database Population**: Populate `QuestReference`, `GameEntities`, `DecisionReference` tables
3. **Pattern Mapping Creation**: Build `PatternGameMapping` from our DZIP discoveries + game knowledge
4. **Enhanced DZIP Analysis**: Modify scripts to validate patterns against reference database
5. **Contextual Data Storage**: Store interpreted data (not raw patterns) in analysis tables
6. **Intelligent Validation**: Cross-reference all findings against authoritative game reference

### Current Database State (Revised Assessment)
- **1 SaveFileMetadata entry**: `test_manual.w2gamesave` (1024 bytes)
- **7,153 LanguageResources**: Excellent foundation for localization validation
- **Empty analysis tables**: Waiting for reference-enhanced data population
- **Missing reference tables**: **Phase 1 Priority** - Need to create and populate game knowledge foundation

### Next Immediate Actions
1. **Create reference table schemas** in database
2. **Extract game knowledge** from codebase comments and planning documents  
3. **Populate reference tables** with authoritative Witcher 2 data
4. **Enhance DZIP scripts** with reference validation capabilities
5. **Implement contextual pattern interpretation** using game knowledge

## 🎮 Witcher Game Specifics

### Witcher 2 Save Structure
Based on DZIP analysis of `.sav` files:
- **Polish language elements**: CD Projekt RED's native language support
- **Quest system integration**: Complex state tracking with phases
- **Screenshot relationships**: `_640x360.bmp` files linked to saves
- **Custom DZIP format**: Version 2, Compression 1, Flags 1

### Pattern Recognition Accuracy
Our DZIP analysis has identified consistent patterns across save files:
- Quest-related strings consistently appear in `questSystem` contexts
- Character variables follow `facts` and `flagState` patterns  
- Game world state tracked through `world` and `timeInfo` structures

## 📋 REVISED PLAN: Reference-First Database Strategy

### **Strategic Shift: Build Game Knowledge Foundation First**

Based on codebase analysis revealing extensive parsing infrastructure and scattered game knowledge, we're adopting a **Reference-First** approach:

### Phase 1: Expand Database Schema for Game Reference Data
1. **Add Reference Tables**: Create `QuestReference`, `GameEntities`, `DecisionReference`, `PatternGameMapping` tables
2. **Extract Existing Knowledge**: Consolidate game data scattered in codebase comments and planning docs
3. **Populate Reference Database**: Load authoritative Witcher 2 quest, character, and decision data

### Phase 2: Enhanced Pattern Recognition with Context
4. **Contextual DZIP Analysis**: Modify `Invoke-NativeDZipDecompression.ps1` to validate against reference database
5. **Smart Pattern Mapping**: Transform "Found questSystem" into "Found Chapter 1 quest progression data"
6. **Reference Validation**: Cross-reference DZIP discoveries with known game entities

### Phase 3: Intelligent Save File Analysis  
7. **Context-Aware Parsing**: Use reference data to interpret save file patterns meaningfully
8. **Quest Progression Intelligence**: Map binary data to actual quest states using game knowledge
9. **Decision Impact Analysis**: Connect save file variables to known critical choices and consequences

### Phase 4: Database Integration with Intelligence
10. **Smart Database Population**: Insert contextually-interpreted data rather than raw patterns
11. **Cross-Reference Validation**: Validate all discoveries against authoritative game reference
12. **Save Comparison Engine**: Enable meaningful diff analysis using game context

## 🔍 Database Queries for Analysis

### Quest Progression Analysis
```sql
-- Find all quests across save files
SELECT sm.FileName, qi.QuestName, qi.QuestPhase, qi.IsCompleted
FROM QuestInfo qi
JOIN SaveFileMetadata sm ON qi.SaveFileId = sm.Id
ORDER BY sm.LastModified;
```

### Variable Change Tracking  
```sql
-- Track variable changes across saves
SELECT cv.VariableName, cv.VariableValue, sm.FileName, sm.LastModified
FROM CharacterVariables cv
JOIN SaveFileMetadata sm ON cv.SaveFileId = sm.Id
WHERE cv.VariableName = 'specific_quest_flag'
ORDER BY sm.LastModified;
```

### Localization Cross-Reference
```sql
-- Find localized names for discovered patterns
SELECT DISTINCT lr.Key, lr.Value, lr.Language
FROM LanguageResources lr
WHERE lr.Key LIKE '%quest%' OR lr.Value LIKE '%Flotsam%'
ORDER BY lr.Language, lr.Key;
```

This database represents the **destination** for our DZIP analysis work - turning raw save file bytes into structured, queryable game data.

## 🚀 Implementation Status

### Phase 0: Reference Database Foundation ✅ COMPLETE
- **Status**: ✅ Complete - Foundation Established  
- **Completion Date**: August 2, 2025
- **Database Schema**: ✅ Created (4 new reference tables)
- **Core Game Knowledge**: ✅ Populated (38 reference records)
- **Pattern Mappings**: ✅ Complete (10 DZIP patterns mapped)

#### Phase 0 Results Summary:
- **QuestReference**: 12 quests mapped with dependencies and progression
- **GameEntities**: 10 characters/locations with relationships and attributes  
- **DecisionReference**: 6 critical choice variables affecting endings
- **PatternGameMapping**: 10 DZIP patterns with confidence levels (5 confirmed)

#### Key Achievements:
- ✅ High-confidence pattern mapping for `chosen_path`, `aryan_la_valette_fate`, `questSystem`
- ✅ Complete ending-affecting decision variable mapping  
- ✅ Reference-first architecture enables intelligent save file interpretation
- ✅ Database foundation ready for Phase 1 live save file analysis

### Next Phase: Phase 1 - Live Save File Analysis
- **Goal**: Apply reference database to actual save file DZIP decompression
- **Method**: Cross-reference discovered patterns against game knowledge base
- **Expected Outcome**: Intelligent interpretation of save file data with game context
