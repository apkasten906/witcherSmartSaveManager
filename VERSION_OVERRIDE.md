# Version Override: v1.1.0

## Decision Date: 2025-07-30

## Override Details:
- **Requested Version**: v1.1.0
- **Automatic Version**: v2.0.0
- **Override Reason**: Semantic versioning detected major changes, but v2.0.0 is premature

## Justification:

### Why v2.0.0 Was Suggested:
- Large batch of commits (>20) since v1.0.0
- Comprehensive CI/CD pipeline implementation
- Multiple feature additions and infrastructure changes

### Why v1.1.0 Is More Appropriate:
- This is the **first automated release** with the new CI/CD system
- Changes are primarily **tooling and infrastructure** (CI/CD, workflows, documentation)
- **No breaking changes** to user-facing functionality
- **No API changes** that would affect end users
- v1.1.0 better reflects the **minor feature** nature (automated releases + infrastructure)

### Future Versioning:
- v2.0.0 reserved for actual **breaking changes** to save file handling or UI
- Semantic versioning will resume normally after this override
- This establishes a baseline for future automated versioning

## Manual Override Applied:
Using `version: 1.1.0` in commit message to trigger manual version override logic in CI/CD pipeline.
