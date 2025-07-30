# Orphaned Screenshot Cleanup Feature

## Overview

The Orphaned Screenshot Cleanup feature addresses a common issue with The Witcher 2 save management: when save files are deleted while the game is running, their associated screenshot files (`.bmp` format) can become "orphaned" and remain on disk, consuming storage space.

## How It Works

### Detection Process
1. **After loading saves**: The system automatically scans for `.bmp` files that don't have corresponding `.save` files
2. **Pattern matching**: Screenshots follow the naming pattern `{SaveName}_640x360.bmp`
3. **Orphan identification**: Any screenshot without a matching save file is considered orphaned

### User Experience

#### Witchery-Themed Notifications
When orphaned screenshots are detected, users see themed messages:

**English**:
```
🐺 Found orphaned screenshot files! 🐺

It's likely their parents were tragically eaten by a kikimora (you deleted their save files while the game was running, so you could also make sure their cloud twins would go away and not return from the dead!)

Would you like to clean up these X orphaned screenshot(s)?
```

**German**:
```
🐺 Verwaiste Screenshot-Dateien gefunden! 🐺

Wahrscheinlich wurden ihre Eltern tragisch von einer Kikimora gefressen (du hast ihre Speicherdateien gelöscht, während das Spiel lief, um sicherzustellen, dass auch ihre Cloud-Zwillinge verschwinden und nicht von den Toten zurückkehren!)

Möchtest du diese X verwaiste(n) Screenshot(s) aufräumen?
```

#### Cleanup Options
- **Yes**: Attempts to delete all orphaned screenshots
- **No**: Leaves orphaned files intact

### Robust Error Handling

#### Locked File Detection
When files are locked (e.g., game still running), the system:
1. **Attempts deletion** of each file individually
2. **Detects lock conditions** and identifies the locking process
3. **Reports partial success** with detailed information about locked files

#### Partial Cleanup Results
If some files couldn't be deleted:

```
🐺 Cleaned up X of Y orphaned screenshots.

Could not delete Z file(s) because they are still locked by cunning spirits:
• filename1.bmp: Process "TheWitcher2" (PID: 1234)
• filename2.bmp: Access denied
```

## Technical Implementation

### Core Components

#### `WitcherSaveFileService`
- `GetOrphanedScreenshots()`: Identifies orphaned screenshot files
- `CleanupOrphanedScreenshotsWithDetails()`: Attempts deletion with detailed error reporting
- `DeleteSaveFile()`: Enhanced to handle both save and screenshot deletion

#### `MainViewModel` 
- `HandleOrphanedScreenshots()`: Orchestrates user interaction and cleanup process
- Localized messaging through `ResourceHelper`

### File Patterns
- **Save files**: `{name}.save`
- **Screenshots**: `{name}_640x360.bmp`
- **Backup detection**: Uses dynamic game extensions via `GameSaveExtensions.GetExtensionForGame()`

### Error Recovery
- **Individual file processing**: Each file deletion is attempted separately
- **Process detection**: Identifies which process is locking files
- **Graceful degradation**: Continues processing even if some files fail
- **User feedback**: Clear reporting of partial success scenarios

## Configuration

### Localization
All messages are stored in `.resx` files:
- `Strings.en.resx`: English messages
- `Strings.de.resx`: German translations

### Resource Keys
- `OrphanedScreenshots_Title`
- `OrphanedScreenshots_Message`
- `OrphanedScreenshots_PartialCleanup_Title`
- `OrphanedScreenshots_PartialCleanup_Message`
- `Status_OrphanedPartialCleanup`
- `Status_OrphanedFullCleanup`

## Benefits

1. **Storage Optimization**: Removes unnecessary screenshot files
2. **User Education**: Explains why orphaned files occur
3. **Non-Destructive**: Always asks user permission before deletion
4. **Robust**: Handles locked files gracefully without crashing
5. **Immersive**: Uses witchery-themed messaging consistent with game lore
6. **Multilingual**: Supports English and German with full localization

## Future Enhancements

- Support for other Witcher game screenshot formats
- Automatic cleanup options (with user preference)
- Batch processing for large numbers of orphaned files
- Integration with cloud sync cleanup
