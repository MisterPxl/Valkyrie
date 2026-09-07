# Astra Valkyrie — DOTween Integration

An optional **Astra integration**. Install its prerequisites explicitly; the base
packages remain usable independently. This is the **3.0 migration candidate** for
Valkyrie 2.x. Follow the [migration guide](../../Documentation~/Migration-2.0/README.md).
Use the exact revisions from the Astra catalog; no new release tag has been created.

Optional no-code DOTween integration for Valkyrie Inspector.

## Prerequisites

- Unity 6 or newer.
- Valkyrie Inspector 2.x (this integration does not target the old 1.x API).
- DOTween Free installed separately. DOTween Pro is not required.

Install DOTween in the consuming Unity project and run DOTween setup before
installing this integration.

## Installation

```json
{
  "dependencies": {
    "com.misterpxl.valkyrie": "https://github.com/misterpxl/Valkyrie.git#codex/astra-conventions",
    "com.misterpxl.valkyrie.dotween": "https://github.com/misterpxl/Valkyrie.git?path=/Addons~/DOTween#codex/astra-conventions"
  }
}
```

## What It Adds

- `TweenPlayer`, a designer-facing component with `Single`, `Sequence`, and
  `Asset` modes.
- `TweenSequenceAsset`, a reusable ScriptableObject containing the same
  `TweenTimeline` data used by the component.
- Polymorphic tween steps organized with Valkyrie's
  `[ManagedReferenceCategory]` picker categories.
- Hybrid targeting: `Self`, direct object reference, or named key binding for
  reusable assets.
- Edit Mode preview with Play, Pause, Stop, and scrub.
- Spawn point capture and `RestartFromSpawnPoint()`.
- Project and built-in presets through `TweenPresetLibrary`.
- Lifecycle triggers and UnityEvents at player and step-binding level.

## Built-In Steps

Runtime steps include transform move, rotate, scale, punch, shake, CanvasGroup
fade, material color, sprite color, camera FOV/orthographic size/background
color, interval, and callback.

The optional `Astra.Valkyrie.Integrations.DOTween.UGUI.Runtime` assembly adds uGUI steps when
`com.unity.ugui` is installed: `Graphic` color/fade, `Image.fillAmount`,
`RectTransform.sizeDelta`, and legacy `Text` typewriter.

## Basic Usage

1. Add **Astra/Valkyrie/DOTween/Tween Player** to a GameObject.
2. Keep `Mode = Single` for one animation, or switch to `Sequence` for a list.
3. Add a step from the categorized picker.
4. Use `Self` for the current GameObject, assign an object directly, or use a
   named key when the animation comes from a shared asset.
5. Preview in Edit Mode, then wire triggers or call `Play()` at runtime.

Use `TweenSequenceAsset` when several scene objects should reuse the same
animation. Put scene-specific targets on each `TweenPlayer` via bindings.

`Single` uses only the first list entry for playback, validation and preview.
`Sequence` and `Asset` use all enabled entries. A disabled first entry in Single
mode produces an empty-sequence diagnostic.

`From` and `By` values are evaluated when their step starts. Building or
validating a built-in animation does not apply its starting value to the target.
Step event bindings match `StepId`; absent and disabled steps do not emit events.
Their start, update, loop-complete (`OnStep`), complete and rewind events follow
the corresponding tween. Player `OnStep` follows the loops of the whole sequence.
Seeking with `Goto` uses DOTween's seek callback behavior rather than normal playback.

In **Events**, each step binding has a **Step** dropdown showing the animation's
step names. Selections keep their IDs when steps are renamed or reordered.
A missing or inactive step displays a warning; choose another step or **None**
to change the binding. Asset mode lists the selected asset's steps.

Right-click a step header to copy/paste its configuration or duplicate a list
element. Pasted and duplicated steps receive a new ID; existing bindings continue
to refer to the original step. Bind the new step explicitly through the dropdown.

## Custom Steps

Create a serializable subclass of `TweenStep` or `TimedTweenStep`. Add
`[ManagedReferenceCategory("Category/Subcategory", "Display Name", order)]` to
control where it appears in the designer picker. No central enum or registry is
needed.

Keep `ValidateDefinition` and `ValidateTarget` free of target mutations. Validation
in the inspector calls these methods without building a sequence. Steps implementing
`ITweenTargetStep` get component-type validation automatically; override
`ValidateTarget` for additional requirements such as shader properties.

For value tweens, pass the authored value directly to `DOTween.To`, then call
`ConfigureValueTween(tween)` and `TryPlaceTween(sequence, tween, context)`.
`ConfigureValueTween` applies timing and defers From/By initialization. The older
`Resolve*EndValue`/`Apply*StartValue` helpers are obsolete because they evaluate or
write target values during construction. Use `ConfigureTween` for timing-only
steps such as punch and shake. `TryPlaceTween` also registers the tween for step events;
group multiple internal tweens into a single tween/sequence when exposing one step.

Override `CaptureSnapshot(context, snapshot)` for additional animated properties.
Resolve the target, capture its original values and register a null-safe restoration
callback through `snapshot.AddRestoreAction`. This is used by both Edit Mode preview
and `CaptureSpawnPoint`. The uGUI steps provide examples while keeping uGUI optional.

## Tests

The integration ships EditMode and PlayMode tests guarded by `UNITY_INCLUDE_TESTS`.
The core package's GUI integration test requires a graphics device (omit
`-nographics`); it is skipped in runs without a graphics device.
Add the integration package to `testables` when running them from a consuming project:

```json
"testables": [
  "com.misterpxl.valkyrie",
  "com.misterpxl.valkyrie.dotween"
]
```

## Prerequisites and tests

The manifest declares these package versions:

- `com.misterpxl.valkyrie`: `1.5.0`.

Install the Astra base packages explicitly in the consumer manifest, using the
Git URLs from their READMEs. Git packages are not fetched transitively from
version-only dependencies. Unity registry dependencies resolve normally.

Add `com.misterpxl.valkyrie.dotween` to the consumer manifest’s `testables` and run
its suites in Unity Test Runner. Keep DOTween installed while running these tests.

## Removal

Remove project components, assets or code that reference this integration before
removing it through Package Manager. The base packages can remain installed.
