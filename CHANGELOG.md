# Changelog

## 1.5.0 - 2026-08-10

### Added

- Add a root MIT license and the `license` field in `package.json`.

### Changed

- Move the optional DOTween addon to the `addon/dotween` branch
  (`Addons~/DOTween`) so it is no longer imported with the root package;
  install it separately via `?path=/Addons~/DOTween#dotween-v2.1.0`.
- Evaluate `[ShowIf]`/`[HideIf]` conditions on every selected object during
  multi-editing, and show a mixed-value dash on managed-reference headers.
- Support enum and numeric compare values in `[ShowIf]`/`[HideIf]`
  (e.g. `[ShowIf("mode", 1)]` against an enum member).

### Fixed

- Fix nested `[SerializeReference]` type selection and Reset/Clear targeting
  the wrong slot when triggered from deferred dropdown or context-menu
  callbacks.
- Fix nested managed-reference lists rebuilding their `ReorderableList` every
  frame, which broke drag-reorder and selection state.
- Exclude value types from the `[SerializeReference]` type picker; Unity
  cannot assign them and the selection failed silently.
- Cache managed-reference type-name and field lookups that previously ran
  reflection and `Assembly.Load` per child per frame.
- Align the duplicate-key highlight of `SerializableDictionary` with Unity 6's
  array drawer geometry.
- Record prefab-instance overrides after `[Button]` invocations.
- Dispose per-target `SerializedObject` instances in mutation operations.
- Keep foldout state keys collision-free by using the full `EntityId`.
- Warn and deduplicate shadowed serialized fields with the same name instead
  of rendering the same property twice.
- Keep `EndFoldoutHeaderGroup` balanced when drawing a grouped field throws.

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
