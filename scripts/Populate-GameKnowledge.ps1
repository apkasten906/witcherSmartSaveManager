# Extract Game Knowledge from Codebase
# Phase 0.2: Game Knowledge Consolidation

param(
    [string]$DatabasePath = "witcher_save_manager.db"
)

Write-Host "Extracting Witcher 2 Game Knowledge from Codebase..." -ForegroundColor Green

# Import required modules
try {
    Import-Module PSSQLite -ErrorAction Stop
}
catch {
    Install-Module PSSQLite -Force -Scope CurrentUser
    Import-Module PSSQLite
}

# Database path - use the existing database
$dbPath = ".\database\witcher_save_manager.db"

if (-not (Test-Path $dbPath)) {
    # If not in database folder, try frontend folder
    $dbPath = ".\frontend\witcher_save_manager.db"
    if (-not (Test-Path $dbPath)) {
        Write-Host "Database file not found! Expected at .\database\witcher_save_manager.db or .\frontend\witcher_save_manager.db" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Using database: $dbPath" -ForegroundColor Cyan

# Step 1: Extract Quest Data from Planning Documents and Code Comments
Write-Host "Populating QuestReference table..." -ForegroundColor Cyan

$questData = @(
    @{
        QuestId         = "q001_prologue"
        QuestName       = "Prologue"
        Chapter         = "Prologue"
        QuestType       = "main"
        Description     = "Introduction and Aryan La Valette encounter"
        Prerequisites   = '[]'
        DecisionPoints  = '["aryan_la_valette_fate"]'
        ConsequenceData = '{"aryan_choice": {"spared": "affects_chapter1_reputation", "killed": "changes_npc_reactions"}}'
    },
    @{
        QuestId         = "q101_kayran"
        QuestName       = "The Kayran"
        Chapter         = "Chapter1"
        QuestType       = "main"
        Description     = "Defeat the Kayran in Flotsam"
        Prerequisites   = '["prologue_complete"]'
        DecisionPoints  = '["kayran_strategy", "triss_interaction"]'
        ConsequenceData = '{"strategy_choice": "affects_available_paths"}'
    },
    @{
        QuestId         = "q102_iorveth_path"
        QuestName       = "Iorveth Path Selection"
        Chapter         = "Chapter1"
        QuestType       = "main"
        Description     = "Choose to follow Iorveth's path"
        Prerequisites   = '["kayran_complete"]'
        DecisionPoints  = '["chosen_path"]'
        ConsequenceData = '{"path_choice": {"iorveth": "unlocks_elf_storyline", "roche": "unlocks_human_storyline"}}'
    },
    @{
        QuestId         = "q103_roche_path"
        QuestName       = "Roche Path Selection"
        Chapter         = "Chapter1"
        QuestType       = "main"
        Description     = "Choose to follow Vernon Roche's path"
        Prerequisites   = '["kayran_complete"]'
        DecisionPoints  = '["chosen_path"]'
        ConsequenceData = '{"path_choice": {"roche": "unlocks_human_storyline", "iorveth": "unlocks_elf_storyline"}}'
    },
    @{
        QuestId         = "q201_vergen"
        QuestName       = "The Siege of Vergen"
        Chapter         = "Chapter2"
        QuestType       = "main"
        Description     = "Defend or attack Vergen based on path choice"
        Prerequisites   = '["path_chosen"]'
        DecisionPoints  = '["vergen_strategy", "saskia_fate"]'
        ConsequenceData = '{"vergen_outcome": "affects_chapter3_available_characters"}'
    },
    @{
        QuestId         = "q301_final_choice"
        QuestName       = "Final Confrontation"
        Chapter         = "Chapter3"
        QuestType       = "main"
        Description     = "Final decisions affecting game ending"
        Prerequisites   = '["chapter2_complete"]'
        DecisionPoints  = '["letho_fate", "political_choice"]'
        ConsequenceData = '{"ending_influence": 5, "narrative_weight": "maximum"}'
    }
)

foreach ($quest in $questData) {
    $insertQuest = @"
INSERT OR REPLACE INTO QuestReference 
(QuestId, QuestName, Chapter, QuestType, Description, Prerequisites, DecisionPoints, ConsequenceData)
VALUES 
('$($quest.QuestId)', '$($quest.QuestName)', '$($quest.Chapter)', '$($quest.QuestType)', 
 '$($quest.Description)', '$($quest.Prerequisites)', '$($quest.DecisionPoints)', '$($quest.ConsequenceData)');
"@
    
    Invoke-SqliteQuery -Query $insertQuest -DataSource $db
    Write-Host "  ✓ Added quest: $($quest.QuestName)" -ForegroundColor Green
}

# Step 2: Extract Character and Location Data
Write-Host "`nPopulating GameEntities table..." -ForegroundColor Cyan

$entityData = @(
    # Major Characters
    @{
        EntityId      = "char_geralt"
        EntityType    = "character"
        DisplayName   = "Geralt of Rivia"
        Chapter       = "All"
        RelatedQuests = '["q001_prologue", "q101_kayran", "q201_vergen", "q301_final_choice"]'
        Description   = "Main protagonist, witcher"
    },
    @{
        EntityId      = "char_triss"
        EntityType    = "character"
        DisplayName   = "Triss Merigold"
        Chapter       = "All"
        RelatedQuests = '["q001_prologue", "q101_kayran", "q102_iorveth_path"]'
        Description   = "Sorceress, love interest"
    },
    @{
        EntityId      = "char_iorveth"
        EntityType    = "character"
        DisplayName   = "Iorveth"
        Chapter       = "Chapter1"
        RelatedQuests = '["q102_iorveth_path", "q201_vergen"]'
        Description   = "Scoia'tael commander, path choice leader"
    },
    @{
        EntityId      = "char_roche"
        EntityType    = "character"
        DisplayName   = "Vernon Roche"
        Chapter       = "Chapter1"
        RelatedQuests = '["q103_roche_path", "q201_vergen"]'
        Description   = "Blue Stripes commander, path choice leader"
    },
    @{
        EntityId      = "char_letho"
        EntityType    = "character"
        DisplayName   = "Letho of Gulet"
        Chapter       = "Chapter3"
        RelatedQuests = '["q301_final_choice"]'
        Description   = "Witcher assassin, final antagonist"
    },
    @{
        EntityId      = "char_saskia"
        EntityType    = "character"
        DisplayName   = "Saskia the Dragonslayer"
        Chapter       = "Chapter2"
        RelatedQuests = '["q201_vergen"]'
        Description   = "Leader of Vergen, dragon in human form"
    },
    # Major Locations
    @{
        EntityId      = "loc_flotsam"
        EntityType    = "location"
        DisplayName   = "Flotsam"
        Chapter       = "Chapter1"
        RelatedQuests = '["q101_kayran", "q102_iorveth_path", "q103_roche_path"]'
        Description   = "Harbor town, Chapter 1 main location"
    },
    @{
        EntityId      = "loc_vergen"
        EntityType    = "location"
        DisplayName   = "Vergen"
        Chapter       = "Chapter2"
        RelatedQuests = '["q201_vergen"]'
        Description   = "Dwarven city under siege"
    },
    @{
        EntityId      = "loc_aedirn"
        EntityType    = "location"
        DisplayName   = "Aedirn"
        Chapter       = "Chapter2"
        RelatedQuests = '["q201_vergen"]'
        Description   = "Kingdom involved in political conflict"
    },
    @{
        EntityId      = "loc_kaedwen"
        EntityType    = "location"
        DisplayName   = "Kaedwen"
        Chapter       = "Chapter2"
        RelatedQuests = '["q201_vergen"]'
        Description   = "Kingdom led by King Henselt"
    }
)

foreach ($entity in $entityData) {
    $insertEntity = @"
INSERT OR REPLACE INTO GameEntities 
(EntityId, EntityType, DisplayName, Chapter, RelatedQuests, Description)
VALUES 
('$($entity.EntityId)', '$($entity.EntityType)', '$($entity.DisplayName)', 
 '$($entity.Chapter)', '$($entity.RelatedQuests)', '$($entity.Description)');
"@
    
    Invoke-SqliteQuery -Query $insertEntity -DataSource $db
    Write-Host "  ✓ Added $($entity.EntityType): $($entity.DisplayName)" -ForegroundColor Green
}

# Step 3: Extract Critical Decision Variables
Write-Host "`nPopulating DecisionReference table..." -ForegroundColor Cyan

$decisionData = @(
    @{
        VariableName   = "aryan_la_valette_fate"
        VariableType   = "enum"
        PossibleValues = '["killed", "spared"]'
        QuestContext   = "q001_prologue"
        ImpactLevel    = 3
        Description    = "Fate of Aryan La Valette in prologue"
        Consequences   = '{"killed": "reputation_loss_with_nobles", "spared": "reputation_gain_mercy"}'
    },
    @{
        VariableName   = "chosen_path"
        VariableType   = "enum"
        PossibleValues = '["iorveth", "roche"]'
        QuestContext   = "q102_iorveth_path"
        ImpactLevel    = 5
        Description    = "Major path choice between Iorveth and Roche"
        Consequences   = '{"iorveth": "elf_storyline_unlocked", "roche": "human_storyline_unlocked"}'
    },
    @{
        VariableName   = "letho_encounters"
        VariableType   = "enum"
        PossibleValues = '["killed", "spared", "negotiated"]'
        QuestContext   = "q301_final_choice"
        ImpactLevel    = 4
        Description    = "Final confrontation with Letho"
        Consequences   = '{"killed": "revenge_ending", "spared": "mercy_ending", "negotiated": "diplomatic_ending"}'
    },
    @{
        VariableName   = "triss_relationship"
        VariableType   = "integer"
        PossibleValues = '[0, 1, 2, 3, 4, 5]'
        QuestContext   = "q101_kayran"
        ImpactLevel    = 2
        Description    = "Relationship level with Triss Merigold"
        Consequences   = '{"high": "romance_available", "low": "friendship_only"}'
    },
    @{
        VariableName   = "saskia_fate"
        VariableType   = "enum"
        PossibleValues = '["saved", "controlled", "killed"]'
        QuestContext   = "q201_vergen"
        ImpactLevel    = 4
        Description    = "Outcome for Saskia during Vergen siege"
        Consequences   = '{"saved": "dragon_ally", "controlled": "puppet_ruler", "killed": "vergen_falls"}'
    },
    @{
        VariableName   = "political_allegiance"
        VariableType   = "enum"
        PossibleValues = '["neutral", "aedirn", "kaedwen", "temeria"]'
        QuestContext   = "q201_vergen"
        ImpactLevel    = 3
        Description    = "Political alliance choice affecting ending"
        Consequences   = '{"neutral": "independent_path", "kingdom_choice": "affects_witcher3_world_state"}'
    }
)

foreach ($decision in $decisionData) {
    $insertDecision = @"
INSERT OR REPLACE INTO DecisionReference 
(VariableName, VariableType, PossibleValues, QuestContext, ImpactLevel, Description, Consequences)
VALUES 
('$($decision.VariableName)', '$($decision.VariableType)', '$($decision.PossibleValues)', 
 '$($decision.QuestContext)', $($decision.ImpactLevel), '$($decision.Description)', '$($decision.Consequences)');
"@
    
    Invoke-SqliteQuery -Query $insertDecision -DataSource $db
    Write-Host "  ✓ Added decision: $($decision.VariableName)" -ForegroundColor Green
}

Write-Host "`nPhase 0.2 Complete: Game knowledge extracted and populated!" -ForegroundColor Green
