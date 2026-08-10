# Changelog

## 1.4.0 - 2026-08-01

### Added

- Add `[ManagedReferenceCategory]` to group concrete `[SerializeReference]` types in Valkyrie's picker.

## 1.3.0 - 2026-07-30

### Added

- Add supported inheritance and composition paths for type-specific custom editors.
- Add a custom-editor integration sample and EditMode coverage.

### Changed

- Register Valkyrie's global editors as fallbacks so type-specific editors take priority.

## 1.2.0 - 2026-07-30

### Added

- Add EditMode coverage for managed-reference type discovery, mutation, multi-editing, nested fields, and Undo.

### Fixed

- Add the missing Unity metadata file for `CHANGELOG.md`.
- Include concrete base types and generic variance matches in `[SerializeReference]` type dropdowns.
- Preserve matching serialized fields when switching managed-reference concrete types.
- Route nested `[SerializeReference]` fields and collections through Valkyrie's polymorphic renderers.
- Show a native object-field picker affordance on `[SerializeReference]` selectors.

## 1.1.0 - 2026-07-29

### Added

- Add the `Usage Examples` sample with a showcase scene and prefab.

### Fixed

- Keep object reference key detection compatible with Unity 6.3.
- Keep editor state cache keys compatible with Unity 6.0 to 6.2.
