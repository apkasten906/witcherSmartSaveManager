# Create Reference Database Schema for Witcher Save Manager
# Phase 0: Reference Database Foundation

param(
    [string]$DatabasePath = "witcher_save_manager.db"
)

Write-Host "Creating Reference Database Schema..." -ForegroundColor Green

# Check if SQLite module is available
try {
    Import-Module PSSQLite -ErrorAction Stop
    Write-Host "PSSQLite module loaded successfully" -ForegroundColor Green
}
catch {
    Write-Host "PSSQLite module not found. Installing..." -ForegroundColor Yellow
    Install-Module PSSQLite -Force -Scope CurrentUser
    Import-Module PSSQLite
}

# Create database connection
$db = $DatabasePath

# Create QuestReference table
Write-Host "Creating QuestReference table..." -ForegroundColor Cyan
$questReferenceSchema = @"
CREATE TABLE IF NOT EXISTS QuestReference (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    QuestId TEXT UNIQUE NOT NULL,
    QuestName TEXT NOT NULL,
    Chapter TEXT NOT NULL,
    QuestType TEXT,
    Description TEXT,
    Prerequisites TEXT,
    DecisionPoints TEXT,
    ConsequenceData TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
"@

Invoke-SqliteQuery -Query $questReferenceSchema -DataSource $db

# Create GameEntities table
Write-Host "Creating GameEntities table..." -ForegroundColor Cyan
$gameEntitiesSchema = @"
CREATE TABLE IF NOT EXISTS GameEntities (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityId TEXT UNIQUE NOT NULL,
    EntityType TEXT NOT NULL,
    DisplayName TEXT NOT NULL,
    Chapter TEXT,
    RelatedQuests TEXT,
    Description TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
"@

Invoke-SqliteQuery -Query $gameEntitiesSchema -DataSource $db

# Create DecisionReference table
Write-Host "Creating DecisionReference table..." -ForegroundColor Cyan
$decisionReferenceSchema = @"
CREATE TABLE IF NOT EXISTS DecisionReference (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VariableName TEXT UNIQUE NOT NULL,
    VariableType TEXT NOT NULL,
    PossibleValues TEXT NOT NULL,
    QuestContext TEXT NOT NULL,
    ImpactLevel INTEGER,
    Description TEXT,
    Consequences TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
"@

Invoke-SqliteQuery -Query $decisionReferenceSchema -DataSource $db

# Create PatternGameMapping table
Write-Host "Creating PatternGameMapping table..." -ForegroundColor Cyan
$patternMappingSchema = @"
CREATE TABLE IF NOT EXISTS PatternGameMapping (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    DZipPattern TEXT NOT NULL,
    GameConcept TEXT NOT NULL,
    ExpectedDataType TEXT,
    SampleValues TEXT,
    RelatedEntities TEXT,
    AnalysisNotes TEXT,
    ConfidenceLevel INTEGER DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
"@

Invoke-SqliteQuery -Query $patternMappingSchema -DataSource $db

# Create indexes for performance
Write-Host "Creating indexes..." -ForegroundColor Cyan
$indexes = @(
    "CREATE INDEX IF NOT EXISTS idx_quest_reference_id ON QuestReference(QuestId);",
    "CREATE INDEX IF NOT EXISTS idx_quest_reference_chapter ON QuestReference(Chapter);",
    "CREATE INDEX IF NOT EXISTS idx_game_entities_type ON GameEntities(EntityType);",
    "CREATE INDEX IF NOT EXISTS idx_decision_reference_variable ON DecisionReference(VariableName);",
    "CREATE INDEX IF NOT EXISTS idx_pattern_mapping_pattern ON PatternGameMapping(DZipPattern);"
)

foreach ($index in $indexes) {
    Invoke-SqliteQuery -Query $index -DataSource $db
}

Write-Host "Reference database schema created successfully!" -ForegroundColor Green
Write-Host "Database location: $DatabasePath" -ForegroundColor White

# Verify tables were created
Write-Host "`nVerifying table creation..." -ForegroundColor Yellow
$tables = Invoke-SqliteQuery -Query "SELECT name FROM sqlite_master WHERE type='table' AND (name LIKE '%Reference' OR name='GameEntities' OR name='PatternGameMapping');" -DataSource $db
foreach ($table in $tables) {
    Write-Host "  ✓ $($table.name)" -ForegroundColor Green
}

Write-Host "`nPhase 0.1 Complete: Database schema ready for population!" -ForegroundColor Green
