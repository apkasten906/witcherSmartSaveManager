# Changelog

All notable changes to the Witcher Smart Save Manager project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2025-07-30

### Added
- **Orphaned Screenshot Cleanup** - Detection and cleanup of screenshot files left behind when save files are deleted during gameplay
  - Automatic detection of orphaned screenshots when loading saves
  - Witcher-themed user notifications with lore-friendly messaging
  - Graceful handling of locked files with detailed error reporting
  - User choice to clean up orphaned files or leave them intact
- **Persistent File Counters** - Real-time display of loaded saves and backup file counts in the UI
  - Live updating counters for total loaded save files
  - Live updating counters for backed up files in backup folder
  - Localized counter labels in English and German
- **Enhanced Multi-language Support** - Complete localization for new features
  - German translations for all new orphaned screenshot messages
  - Localized status messages for cleanup operations
  - Resource-based localization system using .resx files

### Fixed
- **Backup File Counting Accuracy** - Fixed issue where backup counters showed 0 despite existing backup files
  - Updated backup counting to use dynamic game-specific file extensions instead of hardcoded patterns
  - Ensures consistent file detection logic between save loading and backup counting
- **ImageSource Conversion Errors** - Resolved WPF data binding errors for save file screenshots
  - Added `StringToImageSourceConverter` to properly handle empty/null screenshot paths
  - Eliminates `System.Windows.Data Error: 23` ImageSourceConverter exceptions
- **Resource Loading Warnings** - Fixed missing resource key warnings
  - Added all missing resource keys to English and German .resx files
  - Eliminated `System.Windows.ResourceDictionary Warning: 9` resource not found errors

### Technical Improvements
- **Code Cleanup** - Removed excessive debug logging while preserving important operational logs
- **MVVM Pattern** - Enhanced separation of concerns with proper ViewModel property binding
- **Error Handling** - Improved exception handling for file I/O operations during cleanup
- **Performance** - Optimized file enumeration and resource loading patterns

### Developer Experience
- **Comprehensive Documentation** - Updated README with feature descriptions and architectural notes
- **Resource Management** - Streamlined localization workflow with consistent resource key naming
- **Testing Support** - Enhanced logging for troubleshooting file operation issues

## Previous Versions

For changes prior to this release, see the git commit history.
