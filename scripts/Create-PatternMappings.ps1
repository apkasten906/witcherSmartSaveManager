# Map DZIP Patterns to Game Concepts
# Phase 0.3: Enhanced WitcherCI Integration

param(
    [string]$DatabasePath = "witcher_save_manager.db"
)

Write-Host "Creating DZIP Pattern to Game Concept Mappings..." -ForegroundColor Green

# Import required modules
try {
    Import-Module PSSQLite -ErrorAction Stop
}
catch {
    Install-Module PSSQLite -Force -Scope CurrentUser
    Import-Module PSSQLite
}

$db = $DatabasePath

# Step 1: Map DZIP patterns discovered in our analysis to game concepts
Write-Host "Populating PatternGameMapping table..." -ForegroundColor Cyan

$patternMappings = @(
    # Quest System Patterns
    @{
        DZipPattern      = "questSystem"
        GameConcept      = "active_quest_tracker"
        ExpectedDataType = "string"
        SampleValues     = '["q101_kayran", "q102_iorveth_path", "q103_roche_path"]'
        RelatedEntities  = '["q001_prologue", "q101_kayran", "q201_vergen", "q301_final_choice"]'
        AnalysisNotes    = "Core quest system identifier - indicates active quest tracking data"
        ConfidenceLevel  = 5
    },
    @{
        DZipPattern      = "questThread"
        GameConcept      = "quest_progression_state"
        ExpectedDataType = "enum"
        SampleValues     = '["active", "completed", "failed", "inactive"]'
        RelatedEntities  = '["q101_kayran", "q102_iorveth_path", "q201_vergen"]'
        AnalysisNotes    = "Quest progression tracking - indicates current state of quest threads"
        ConfidenceLevel  = 4
    },
    @{
        DZipPattern      = "questBlock"
        GameConcept      = "quest_completion_data"
        ExpectedDataType = "complex"
        SampleValues     = '["objective_states", "completion_flags", "reward_data"]'
        RelatedEntities  = '["q101_kayran", "q201_vergen", "q301_final_choice"]'
        AnalysisNotes    = "Quest completion data structure - contains objective and reward information"
        ConfidenceLevel  = 3
    },
    
    # State Tracking Patterns
    @{
        DZipPattern      = "activeBool"
        GameConcept      = "boolean_game_flags"
        ExpectedDataType = "boolean"
        SampleValues     = '["true", "false"]'
        RelatedEntities  = '["aryan_la_valette_fate", "chosen_path", "letho_encounters"]'
        AnalysisNotes    = "Boolean decision flags - tracks binary choices and states"
        ConfidenceLevel  = 5
    },
    @{
        DZipPattern      = "flagState"
        GameConcept      = "variable_state_tracker"
        ExpectedDataType = "enum"
        SampleValues     = '["set", "unset", "pending", "locked"]'
        RelatedEntities  = '["aryan_la_valette_fate", "saskia_fate", "political_allegiance"]'
        AnalysisNotes    = "Flag state management - tracks variable states and transitions"
        ConfidenceLevel  = 4
    },
    @{
        DZipPattern      = "tracked_pin_tag"
        GameConcept      = "quest_objective_markers"
        ExpectedDataType = "string"
        SampleValues     = '["pin_kayran_lair", "pin_roche_camp", "pin_iorveth_hideout"]'
        RelatedEntities  = '["loc_flotsam", "loc_vergen", "q101_kayran"]'
        AnalysisNotes    = "Quest pin tracking - map markers and objective locations"
        ConfidenceLevel  = 3
    },
    
    # Character/NPC Patterns
    @{
        DZipPattern      = "community"
        GameConcept      = "npc_faction_data"
        ExpectedDataType = "complex"
        SampleValues     = '["scoia_tael", "blue_stripes", "town_guard"]'
        RelatedEntities  = '["char_iorveth", "char_roche", "loc_flotsam"]'
        AnalysisNotes    = "NPC community and faction tracking - affects reputation and interactions"
        ConfidenceLevel  = 4
    },
    @{
        DZipPattern      = "formations"
        GameConcept      = "party_formation_data"
        ExpectedDataType = "complex"
        SampleValues     = '["combat_formation", "travel_formation", "stealth_formation"]'
        RelatedEntities  = '["char_geralt", "char_triss", "char_roche"]'
        AnalysisNotes    = "Party formation and companion positioning data"
        ConfidenceLevel  = 2
    },
    @{
        DZipPattern      = "attitude"
        GameConcept      = "character_relationship_data"
        ExpectedDataType = "integer"
        SampleValues     = '[-100, -50, 0, 50, 100]'
        RelatedEntities  = '["char_triss", "char_iorveth", "char_roche", "triss_relationship"]'
        AnalysisNotes    = "Character attitude and relationship values - affects dialogue and story options"
        ConfidenceLevel  = 4
    },
    
    # Game Engine Patterns
    @{
        DZipPattern      = "SAVY"
        GameConcept      = "save_file_identifier"
        ExpectedDataType = "string"
        SampleValues     = '["SAVY_v2", "SAVE_FORMAT"]'
        RelatedEntities  = '[]'
        AnalysisNotes    = "Save file format identifier - indicates Witcher 2 save file type"
        ConfidenceLevel  = 5
    },
    @{
        DZipPattern      = "timeInfo"
        GameConcept      = "game_time_tracking"
        ExpectedDataType = "complex"
        SampleValues     = '["playtime_minutes", "game_date", "time_of_day"]'
        RelatedEntities  = '[]'
        AnalysisNotes    = "Game time and playtime tracking data"
        ConfidenceLevel  = 4
    },
    @{
        DZipPattern      = "world"
        GameConcept      = "world_state_data"
        ExpectedDataType = "complex"
        SampleValues     = '["current_location", "weather", "day_night_cycle"]'
        RelatedEntities  = '["loc_flotsam", "loc_vergen", "loc_aedirn"]'
        AnalysisNotes    = "World state information - location, environment, and global flags"
        ConfidenceLevel  = 3
    },
    @{
        DZipPattern      = "playingMusic"
        GameConcept      = "audio_state_data"
        ExpectedDataType = "string"
        SampleValues     = '["combat_music", "ambient_flotsam", "quest_theme"]'
        RelatedEntities  = '["loc_flotsam", "loc_vergen"]'
        AnalysisNotes    = "Current music and audio state tracking"
        ConfidenceLevel  = 2
    },
    
    # Data Structure Patterns
    @{
        DZipPattern      = "facts"
        GameConcept      = "game_world_facts"
        ExpectedDataType = "key_value_pairs"
        SampleValues     = '["fact_aryan_spared", "fact_path_chosen", "fact_kayran_defeated"]'
        RelatedEntities  = '["aryan_la_valette_fate", "chosen_path", "q101_kayran"]'
        AnalysisNotes    = "Game world facts system - persistent story state tracking"
        ConfidenceLevel  = 5
    },
    @{
        DZipPattern      = "LogBlock"
        GameConcept      = "debug_logging_data"
        ExpectedDataType = "complex"
        SampleValues     = '["debug_info", "error_logs", "performance_data"]'
        RelatedEntities  = '[]'
        AnalysisNotes    = "Debug and logging information - development data"
        ConfidenceLevel  = 1
    },
    @{
        DZipPattern      = "BLCK"
        GameConcept      = "data_block_identifier"
        ExpectedDataType = "string"
        SampleValues     = '["QUEST_BLCK", "CHAR_BLCK", "ITEM_BLCK"]'
        RelatedEntities  = '[]'
        AnalysisNotes    = "Data block identifiers - section markers in save file structure"
        ConfidenceLevel  = 3
    }
)

foreach ($mapping in $patternMappings) {
    $insertMapping = @"
INSERT OR REPLACE INTO PatternGameMapping 
(DZipPattern, GameConcept, ExpectedDataType, SampleValues, RelatedEntities, AnalysisNotes, ConfidenceLevel)
VALUES 
('$($mapping.DZipPattern)', '$($mapping.GameConcept)', '$($mapping.ExpectedDataType)', 
 '$($mapping.SampleValues)', '$($mapping.RelatedEntities)', '$($mapping.AnalysisNotes)', $($mapping.ConfidenceLevel));
"@
    
    Invoke-SqliteQuery -Query $insertMapping -DataSource $db
    Write-Host "  ✓ Mapped pattern: $($mapping.DZipPattern) → $($mapping.GameConcept)" -ForegroundColor Green
}

Write-Host "`nPhase 0.3 Complete: DZIP patterns mapped to game concepts!" -ForegroundColor Green

# Step 2: Generate summary report
Write-Host "`nGenerating Reference Database Summary..." -ForegroundColor Yellow

$questCount = (Invoke-SqliteQuery -Query "SELECT COUNT(*) as count FROM QuestReference" -DataSource $db).count
$entityCount = (Invoke-SqliteQuery -Query "SELECT COUNT(*) as count FROM GameEntities" -DataSource $db).count
$decisionCount = (Invoke-SqliteQuery -Query "SELECT COUNT(*) as count FROM DecisionReference" -DataSource $db).count
$patternCount = (Invoke-SqliteQuery -Query "SELECT COUNT(*) as count FROM PatternGameMapping" -DataSource $db).count

Write-Host "`n📊 Reference Database Population Summary:" -ForegroundColor Cyan
Write-Host "  Quest Reference Entries: $questCount" -ForegroundColor White
Write-Host "  Game Entities: $entityCount" -ForegroundColor White
Write-Host "  Decision References: $decisionCount" -ForegroundColor White
Write-Host "  Pattern Mappings: $patternCount" -ForegroundColor White

Write-Host "`n🎯 Ready for Phase 1: Enhanced DZIP Analysis!" -ForegroundColor Green
Write-Host "The reference database now provides context for intelligent pattern recognition." -ForegroundColor White
