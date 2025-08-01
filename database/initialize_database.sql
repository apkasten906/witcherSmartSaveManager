-- Enable verbose output
PRAGMA foreign_keys = ON;

-- Create SaveFiles table
CREATE TABLE IF NOT EXISTS SaveFiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName TEXT NOT NULL,
    Timestamp DATETIME NOT NULL,
    GameState TEXT
);

SELECT
    'SaveFiles table created successfully' AS Message;

-- Create LanguageResources table
CREATE TABLE IF NOT EXISTS LanguageResources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL,
    Value TEXT NOT NULL,
    Language TEXT NOT NULL
);

SELECT
    'LanguageResources table created successfully' AS Message;

-- Create GameDetails table
CREATE TABLE IF NOT EXISTS GameDetails (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL,
    DetailKey TEXT NOT NULL,
    DetailValue TEXT NOT NULL,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFiles (Id)
);

SELECT
    'GameDetails table created successfully' AS Message;

-- Initialize database schema for Witcher Smart Save Manager
-- SQLite database for storing save file metadata and analysis results
-- Table for save file metadata
CREATE TABLE IF NOT EXISTS SaveFileMetadata (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL, -- Links to SaveFiles table
    FileName TEXT NOT NULL,
    GameKey TEXT NOT NULL, -- 'Witcher1', 'Witcher2', 'Witcher3'
    FullPath TEXT NOT NULL,
    ScreenshotPath TEXT,
    FileSize INTEGER,
    LastModified DATETIME NOT NULL,
    ModifiedTimeIso TEXT,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFiles (Id) ON DELETE CASCADE
);

-- Table for parsed quest information (Witcher 2 specific)
CREATE TABLE IF NOT EXISTS QuestInfo (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL,
    QuestName TEXT,
    QuestPhase TEXT,
    QuestDescription TEXT,
    IsCompleted BOOLEAN DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFileMetadata (Id) ON DELETE CASCADE
);

-- Table for character stats and variables
CREATE TABLE IF NOT EXISTS CharacterVariables (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL,
    VariableName TEXT NOT NULL,
    VariableValue TEXT,
    VariableType TEXT, -- 'bool', 'int', 'float', 'string'
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFileMetadata (Id) ON DELETE CASCADE
);

-- Table for inventory items (future enhancement)
CREATE TABLE IF NOT EXISTS InventoryItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL,
    ItemName TEXT NOT NULL,
    ItemId TEXT,
    Quantity INTEGER DEFAULT 1,
    ItemType TEXT, -- 'weapon', 'armor', 'potion', 'book', etc.
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFileMetadata (Id) ON DELETE CASCADE
);

-- Indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_savefile_game ON SaveFileMetadata (GameKey);

CREATE INDEX IF NOT EXISTS idx_savefile_modified ON SaveFileMetadata (LastModified);

CREATE INDEX IF NOT EXISTS idx_quest_savefile ON QuestInfo (SaveFileId);

CREATE INDEX IF NOT EXISTS idx_variables_savefile ON CharacterVariables (SaveFileId);

CREATE INDEX IF NOT EXISTS idx_inventory_savefile ON InventoryItems (SaveFileId);

-- Indexes for the relationship between SaveFiles and SaveFileMetadata
CREATE INDEX IF NOT EXISTS idx_metadata_savefile ON SaveFileMetadata (SaveFileId);

-- Indexes for existing tables
CREATE INDEX IF NOT EXISTS idx_gamedetails_savefile ON GameDetails (SaveFileId);

CREATE INDEX IF NOT EXISTS idx_languageresources_key ON LanguageResources (Key, Language);

-- Insert version info
CREATE TABLE IF NOT EXISTS DatabaseVersion (
    Version TEXT PRIMARY KEY,
    AppliedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

INSERT OR REPLACE INTO
    DatabaseVersion (Version)
VALUES
    ('1.0.0');