# Changelog

## Unreleased

### Changed

- Adopt Astra display names, menus, integration terminology and shared documentation conventions. Package IDs and C# APIs remain unchanged.

### Fixed

- Validate definitions and target bindings without building or executing tweens in the inspector.
- Defer built-in From and By values until each tween starts; sequential relative steps now accumulate correctly.
- Restore RectTransform dimensions, Graphic colors/alpha, Image fill and Text content after preview and spawn-point restarts.
- Stop and restore previews before scene saves, assembly reloads and entering Play Mode; repeated Stop calls no longer reapply an old snapshot.
- Resolve snapshot targets using the exact component type, including optional uGUI components.
- Route step bindings by StepId to the corresponding tween callbacks, including interval and callback steps.
- Fire player OnStep once per sequence loop and avoid duplicate OnRewind notifications.
- Apply Single mode consistently to playback, validation, snapshots and preview.
- Add EditMode, optional uGUI and PlayMode regression tests; strengthen the spawn-point restoration assertion.

### Added

- Select event-binding steps by name; preserve selections across renames/reorders and explain missing or inactive steps.
- Generate new step IDs when using Valkyrie's polymorphic clipboard, preserving the original step's bindings.
- Add ValidateTarget and CaptureSnapshot extension points for custom steps and ConfigureValueTween for deferred From/By configuration.
- Retain the legacy value/start helpers as obsolete APIs for source compatibility.

## 2.1.0 - 2026-08-10

### Changed

- Move the addon to `Addons~/` in the repository so it is no longer imported
  as part of the root Valkyrie package; the install path becomes
  `?path=/Addons~/DOTween`.
- Require Valkyrie Inspector 1.5.0.

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
