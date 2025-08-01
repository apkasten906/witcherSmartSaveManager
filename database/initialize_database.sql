-- Enable verbose output
PRAGMA foreign_keys = ON;

-- Create SaveFiles table
CREATE TABLE SaveFiles (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FileName TEXT NOT NULL,
    Timestamp DATETIME NOT NULL,
    GameState TEXT
);
SELECT 'SaveFiles table created successfully' AS Message;

-- Create LanguageResources table
CREATE TABLE LanguageResources (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL,
    Value TEXT NOT NULL,
    Language TEXT NOT NULL
);
SELECT 'LanguageResources table created successfully' AS Message;

-- Create GameDetails table
CREATE TABLE GameDetails (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    SaveFileId INTEGER NOT NULL,
    DetailKey TEXT NOT NULL,
    DetailValue TEXT NOT NULL,
    FOREIGN KEY (SaveFileId) REFERENCES SaveFiles(Id)
);
SELECT 'GameDetails table created successfully' AS Message;