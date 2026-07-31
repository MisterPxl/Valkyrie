# Changelog

## 2.0.0 - 2026-08-01

### Added

- Add the unified `TweenPlayer` component with Single, Sequence, and Asset modes.
- Add hybrid target references (`Self`, direct object, or named binding).
- Add Edit Mode preview with manual scrub, scene-state snapshots, and spawn-point restart.
- Add categorized built-in steps for transform, punch, shake, camera, renderer, UI, interval, and callbacks.
- Add lifecycle triggers, player/step events, timeline clipboard actions, and preset libraries.

### Changed

- Rework `TweenSequenceAsset` and inline data around a shared `TweenTimeline`.
- Require Valkyrie Inspector 1.4.0 for `[ManagedReferenceCategory]`.
- Simplify the `TweenPlayer` inspector with contextual foldouts and a sequence-only viewer.

### Fixed

- Match `TweenPlayer` script filenames so Unity exposes the component in the Add Component menu.
- Use Unity's opaque `EntityId` API where available while retaining Unity 6.0 and 6.1 compatibility.

## 1.0.2 - 2026-07-31

### Added

- Add a package-local MIT license.

### Fixed

- Validate step definitions without constructing tweens, including custom constraints.
- Read custom timeline duration and placement through `ITweenTimelineStepDefinition`.
- Keep root assembly isolation checks out of the add-on directory.
- Stop PlayMode tests from suppressing unrelated Unity errors.

## 1.0.1 - 2026-07-31

### Fixed

- Reject sequences without enabled tween-producing steps.
- Release player ownership when recyclable sequences auto-kill.
- Align editor validation with disabled and custom non-timeline steps.

## 1.0.0 - 2026-07-31

### Added

- Add extensible DOTween sequence assets with managed-reference step definitions.
- Add typed target bindings, structured build diagnostics, and runtime playback controls.
- Add move, rotation, scale, CanvasGroup fade, and interval steps.
