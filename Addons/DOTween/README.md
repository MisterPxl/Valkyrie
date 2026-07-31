# Valkyrie DOTween

Optional no-code DOTween addon for Valkyrie Inspector.

## Prerequisites

- Unity 6 or newer.
- Valkyrie Inspector 1.4.0 or newer.
- DOTween Free installed separately. DOTween Pro is not required.

Install DOTween in the consuming Unity project and run DOTween setup before
installing this addon.

## Installation

```json
{
  "dependencies": {
    "com.misterpxl.valkyrie": "https://github.com/misterpxl/Valkyrie.git#v1.4.0",
    "com.misterpxl.valkyrie.dotween": "https://github.com/misterpxl/Valkyrie.git?path=/Addons/DOTween#dotween-v2.0.0"
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

The optional `Valkyrie.DOTween.UGUI.Runtime` assembly adds uGUI steps when
`com.unity.ugui` is installed: `Graphic` color/fade, `Image.fillAmount`,
`RectTransform.sizeDelta`, and legacy `Text` typewriter.

## Basic Usage

1. Add **Valkyrie/DOTween/Tween Player** to a GameObject.
2. Keep `Mode = Single` for one animation, or switch to `Sequence` for a list.
3. Add a step from the categorized picker.
4. Use `Self` for the current GameObject, assign an object directly, or use a
   named key when the animation comes from a shared asset.
5. Preview in Edit Mode, then wire triggers or call `Play()` at runtime.

Use `TweenSequenceAsset` when several scene objects should reuse the same
animation. Put scene-specific targets on each `TweenPlayer` via bindings.

## Custom Steps

Create a serializable subclass of `TweenStep` or `TimedTweenStep`. Add
`[ManagedReferenceCategory("Category/Subcategory", "Display Name", order)]` to
control where it appears in the designer picker. No central enum or registry is
needed.

## Tests

The addon ships EditMode and PlayMode tests guarded by `UNITY_INCLUDE_TESTS`.
Add the addon package to `testables` when running them from a consuming project:

```json
"testables": [
  "com.misterpxl.valkyrie",
  "com.misterpxl.valkyrie.dotween"
]
```
