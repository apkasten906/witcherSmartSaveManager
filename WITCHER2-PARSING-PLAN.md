# Witcher 2 Save File Parsing Implementation Plan

> **NOTE:** This file is now maintained in `docs/WITCHER2-PARSING-PLAN.md` and is the canonical, up-to-date plan for Witcher 2 save parsing and analysis. All future updates and documentation should be made in the `docs/` folder.


## 🎯 Mission: Smart Save Manager with Decision Impact Analysis

### REVISED STRATEGY: Reference-First Database Architecture

Based on database analysis revealing comprehensive game knowledge infrastructure, we're adopting a **Reference-First** approach that builds on our DZIP decompression foundation.


---

## 📁 Documentation Location Policy

All planning, architecture, and technical documentation for Witcher 2 save parsing is now maintained in the `docs/` folder. This ensures a single source of truth and easier onboarding for contributors.

---

### Phase 0: Reference Database Foundation (NEW PRIORITY)

#### 0.1 Database Schema Extension
- [x] Analyze existing database structure with 7,153 language resources
- [x] Create `QuestReference`, `GameEntities`, `DecisionReference`, `PatternGameMapping` tables
- [x] Extract game knowledge from existing codebase comments and planning docs
- [x] Populate reference tables with authoritative Witcher 2 data

#### 0.2 Game Knowledge Consolidation
- [x] Extract quest data from parser code comments (`WitcherSaveFileAnalyzer.cs`)
- [x] Consolidate character/location data from DZIP analysis scripts
- [x] Map critical decisions from this planning document to reference database
- [x] Create pattern-to-game-concept mappings from DZIP discoveries

#### 0.3 Enhanced WitcherCI Integration
- [x] Modify `Invoke-NativeDZipDecompression.ps1` to validate against reference database
- [x] Transform pattern recognition from "Found questSystem" to "Found Chapter 1 quest data"
- [x] Enable contextual interpretation using game knowledge foundation

### Phase 1: Save File Structure Discovery (ENHANCED)

#### 1.1 Binary Analysis Setup (REVISED)
- [x] Create `SaveFileAnalyzer` service in WitcherCore
- [x] Implement binary file reader with structured parsing
- [x] Add hex dump utilities for manual inspection (WitcherCI DZIP analysis)
- [x] Enhance analysis with reference database validation
- [ ] Document save file header structure with game context

#### 1.2 Quest Data Location (CONTEXTUAL)
- [x] Identify quest state storage format in .sav files (DZIP patterns discovered)
- [x] Map quest IDs to human-readable names using `QuestReference` table
- [x] Locate quest phase/progress indicators (`questSystem`, `questThread`, `questBlock`)
- [x] Find decision variables using `DecisionReference` validation

#### 1.3 Test Data Collection (DATABASE-DRIVEN)
- [x] Create multiple save files at key decision points referenced in `DecisionReference`:
  - Prologue choices (Aryan La Valette fate) → `aryan_la_valette_fate` variable
  - Chapter 1 path selection (Iorveth vs Roche) → `chosen_path` variable
  - Key moral decisions throughout → Cross-reference with `DecisionReference`
- [x] Store findings in database for pattern validation and learning

### Phase 2: Core Parsing Engine (DATABASE-ENHANCED)

#### 2.1 Binary Parser Implementation (CONTEXTUAL)
- [x] `WitcherSaveFileParser` class for .sav file reading (implemented in WitcherCore)
- [x] Quest data extraction using `QuestReference` validation
- [x] Character variable parsing with `DecisionReference` context
- [x] Inventory state analysis using `GameEntities` item reference

#### 2.2 Decision Impact Mapping (REFERENCE-DRIVEN)
- [x] **DATABASE**: Populate `DecisionReference` with decision taxonomy (moral, political, personal)
- [x] **ENHANCED**: Map quest states to narrative consequences using reference data
- [x] **DATABASE**: Use `PatternGameMapping` to identify branching points and outcomes
- [x] **COMPLETED**: Decision impact database schema already designed

#### 2.3 Smart Analysis Features (INTELLIGENCE-BASED)
- [x] **ENHANCED**: Detect critical decision moments using `DecisionReference.ImpactLevel`
- [x] **NEW**: Analyze save file "quality" based on choices using game knowledge
- [x] **ENHANCED**: Predict narrative consequences using `DecisionReference.Consequences`
- [x] **NEW**: Suggest optimal save points using quest progression data

### Phase 3: ML Foundation

#### 3.1 Data Structure for ML
- [ ] Feature extraction from quest states
- [ ] Decision vector representation
- [ ] Outcome classification schema
- [ ] Training data format definition

#### 3.2 Analysis Engine
- [ ] Smart save recommendation system
- [ ] Decision impact scoring
- [ ] Narrative path analysis
- [ ] Player behavior insights

### Phase 4: UI Integration

#### 4.1 Enhanced Save Display
- [ ] Current quest with context
- [ ] Recent decisions and their impact
- [ ] Critical choice indicators
- [ ] Narrative branch visualization

#### 4.2 Smart Features
- [ ] "Point of No Return" warnings
- [ ] Alternative path suggestions
- [ ] Decision consequence preview
- [ ] Save file "smartness" rating

## 🔬 Technical Implementation

### Core Classes Needed:
```csharp
// Save file parsing
public class WitcherSaveFileParser
public class QuestStateExtractor  
public class DecisionAnalyzer

// Smart analysis
public class NarrativePathAnalyzer
public class DecisionImpactCalculator
public class SaveFileIntelligence

// ML preparation
public class DecisionFeatureExtractor
public class OutcomePredictor
```

### Database Schema Extensions (REVISED - ALREADY DESIGNED):
```sql
-- Reference tables for game knowledge (NEW PRIORITY)
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

-- Enhanced decision tracking (UPDATED)
CREATE TABLE IF NOT EXISTS Decisions (
    Id INTEGER PRIMARY KEY,
    SaveFileId INTEGER,
    DecisionId TEXT,
    DecisionType TEXT, -- moral, political, personal
    ChoiceMade TEXT,
    ImpactLevel INTEGER, -- 1-5 scale
    NarrativeConsequences TEXT,
    -- NEW: Reference to authoritative decision data
    DecisionReferenceId INTEGER REFERENCES DecisionReference(Id)
);

-- Quest branches (ENHANCED)
CREATE TABLE IF NOT EXISTS QuestBranches (
    Id INTEGER PRIMARY KEY,
    QuestId TEXT,
    BranchPoint TEXT,
    Conditions TEXT,
    Outcomes TEXT,
    -- NEW: Reference to authoritative quest data
    QuestReferenceId INTEGER REFERENCES QuestReference(Id)
);
```

## 🎮 Witcher 2 Specific Focus Areas (REFERENCE DATABASE TARGETS)

### Critical Decision Points to Track (→ DecisionReference Table):
1. **Prologue**: Aryan La Valette's fate → `aryan_la_valette_fate` variable
2. **Chapter 1**: Iorveth vs Roche path choice → `chosen_path` variable
3. **Letho encounters**: Kill vs spare decisions → `letho_encounters` variable  
4. **Triss relationship**: Romance choices → `triss_relationship` variable
5. **Political choices**: King's fate, political alliances → `political_allegiance` variables

### Quest Impact Categories (→ QuestReference.ConsequenceData):
- **Immediate**: Affects current chapter → `"immediate_impact": true`
- **Long-term**: Changes available quests/characters → `"long_term_consequences": ["quest_ids"]`
- **Ending**: Influences game conclusion → `"ending_impact": 1-5`
- **Moral**: Character development impact → `"moral_weight": 1-5`

### Character/Location Entities (→ GameEntities Table):
- **Characters**: Geralt, Triss, Iorveth, Roche, Letho, Saskia, Philippa, Vernon, Foltest
- **Locations**: Flotsam, Vergen, Aedirn, Kaedwen, Temeria, Pontar Valley
- **Items**: Witcher medallions, formulas, quest items

### DZIP Pattern Mappings (→ PatternGameMapping Table):
- **questSystem** → active quest tracking → `QuestReference` validation
- **questThread** → quest progression state → Chapter/phase mapping  
- **questBlock** → quest completion data → Boolean completion flags
- **activeBool** → decision boolean flags → `DecisionReference` variables
- **flagState** → variable state tracking → Character progression markers
- **facts** → game world facts → Environmental/story state

## 📊 Success Metrics

### What Makes Our Manager "Smart":
1. **Decision Awareness**: Knows what choices were made
2. **Impact Understanding**: Predicts consequences
3. **Optimal Timing**: Suggests when to save
4. **Alternative Paths**: Shows what could have been
5. **Player Insights**: Learns from decision patterns

This positions us as the **definitive smart save manager** for story-driven RPGs!
