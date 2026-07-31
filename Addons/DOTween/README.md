# Valkyrie DOTween

Optional, asset-driven DOTween sequences for Valkyrie. A
`TweenSequenceAsset` stores polymorphic steps once, while each
`TweenSequencePlayer` supplies scene-local target bindings.

## Prerequisites

- Unity 6 or newer.
- Valkyrie Inspector.
- **DOTween Free installed separately.** DOTween is not bundled and DOTween Pro
  is not required.

Install DOTween Free from the Unity Asset Store or Demigiant, then open
**Tools > Demigiant > DOTween Utility Panel**:

1. Run **Setup DOTween...**.
2. Generate DOTween's ASMDEF files from the Utility Panel when the project uses
   optional DOTween modules.
3. Verify that Unity can resolve the `DOTween.dll` precompiled reference.

The add-on runtime explicitly references `DOTween.dll`. Built-in steps avoid a
hard dependency on `DOTween.Modules`, so CanvasGroup fade remains usable without
bringing the optional uGUI module assembly into the package graph.

## Installation

Add both packages to `Packages/manifest.json`. In a Git dependency with a
subdirectory, `?path=` must come before `#revision`:

```json
{
  "dependencies": {
    "com.misterpxl.valkyrie": "https://github.com/misterpxl/Valkyrie.git#v1.3.0",
    "com.misterpxl.valkyrie.dotween": "https://github.com/misterpxl/Valkyrie.git?path=/Addons/DOTween#dotween-v1.0.0"
  }
}
```

DOTween remains a project asset and is not expressible as a normal dependency
of this Git package. Install and set it up before adding the add-on if possible.

The add-on is published from dedicated `addon/dotween` tags because Unity
imports a root Git package recursively. Keep Valkyrie on a normal root release
tag and the add-on on a `dotween-v*` tag. Do not install the root package from
the add-on revision: that revision contains `Addons/DOTween` as source and would
make Unity import the add-on twice.

## Basic usage

1. Create an asset with **Assets > Create > Valkyrie > DOTween > Tween
   Sequence**.
2. Add concrete step types to its `Steps` managed-reference list.
3. Add a `TweenSequencePlayer` to a scene object and assign the asset.
4. Add one local binding for each non-`Self` key used by the steps.
5. Call `Play()`, or enable **Play On Enable** in the serialized inspector.

The same asset can be assigned to any number of players. Only keys and animation
data live in the asset; scene objects live on each player.

```csharp
using UnityEngine;
using Valkyrie.DOTween;

public sealed class OpenPanelAnimation : MonoBehaviour
{
    [SerializeField] private TweenSequencePlayer _player;
    [SerializeField] private CanvasGroup _panel;

    private void Awake()
    {
        _player.Bindings.Clear();
        _player.Bindings.Add(new TweenTargetBinding("Panel", _panel));
    }

    public void Play()
    {
        _player.Play();
    }
}
```

Built-in steps cover world/local transform movement, world/local rotation,
scale, `CanvasGroup` alpha, and intervals. Timed steps support delay, ease,
finite loops, relative values, and `Append`, `Join`, or absolute `Insert`
placement.

## Binding rules

- `Self` is implicit and resolves to `TweenSequencePlayer.TargetRoot`.
  `TargetRoot` falls back to the player's own transform.
- An empty/whitespace key and any case variation of `Self` also mean `Self`.
  The binding list cannot override it.
- Other keys are trimmed and compared with ordinal, case-sensitive matching.
- Duplicate keys are build errors. Null entries and attempts to bind `Self` are
  warnings.
- A bound object can be the requested object directly. For component targets,
  it can also be a `GameObject` or another component on the same object.
- Missing, null, or wrong-type bindings produce diagnostics and prevent the
  entire sequence from being returned.

Inspect `TweenSequencePlayer.Diagnostics` or subscribe to
`DiagnosticsChanged` when configuration errors need to be shown in custom UI.

## Playback and lifecycle

`Play()` kills any previous player-owned sequence, builds a fresh sequence, and
starts it. `TryBuildSequence(out Sequence)` builds the same sequence in a paused
state for explicit control. The player also exposes `Pause`, `Resume`, `Rewind`,
`Complete`, and `Kill`.

Sequence assets default to `AutoKill = false`, so completed sequences remain
controllable until killed. Each built sequence receives:

- a readable DOTween ID from `IdOverride`, or a generated player/asset ID;
- a DOTween target from `TargetOverride`, or a
  `TweenSequenceRuntimeIdentity` containing the player and asset.

Disable and destruction cleanup are configured independently:

- `Kill` (default) kills and clears the owned sequence;
- `CompleteAndKill` applies the end state, then kills;
- `None` leaves the sequence alive when the player is disabled. On destruction,
  `None` is deliberately treated as `Kill` so the player cannot orphan a tween.

## Custom steps

Add a serializable subclass next to your game code. No global enum, identifier,
or central registration needs updating; Valkyrie's managed-reference type
discovery finds compatible concrete classes.

```csharp
using System;
using DG.Tweening;
using UnityEngine;
using Valkyrie.DOTween;

[Serializable]
public sealed class LightIntensityStepDefinition : TimedTweenStepDefinition
{
    [SerializeField] private string _targetKey = TweenTargetBinding.SelfKey;
    [Min(0f)]
    [SerializeField] private float _endIntensity = 1f;

    public override bool TryAddTo(Sequence sequence, TweenBuildContext context)
    {
        if (!ValidateTiming(context))
        {
            return false;
        }

        Light target;
        if (!context.TryResolve(_targetKey, out target))
        {
            return false;
        }

        if (_endIntensity < 0f)
        {
            context.ReportError(
                TweenDiagnosticCode.InvalidValue,
                "Light intensity must be greater than or equal to zero.");
            return false;
        }

        Tweener tween = DG.Tweening.DOTween.To(
            () => target.intensity,
            value => target.intensity = value,
            _endIntensity,
            Duration);
        ConfigureTween(tween);
        return TryPlaceTween(sequence, tween, context);
    }
}
```

Place the custom step in an assembly that references
`Valkyrie.DOTween.Runtime` and `DOTween.dll`. Add `DOTween.Modules` only when
the custom step calls an extension supplied by that assembly.

## Sample

`Samples~/Reusable Sequence/ReusableTweenSequenceSample.prefab` is an importable
prefab artifact. Copy the `Reusable Sequence` sample folder into the project's
`Assets` folder, drop the prefab into a scene, and enter Play Mode. It creates
one shared `TweenSequenceAsset`, assigns that same instance to two local
players, and binds each player's `Animated` key to a different transform. Press
the component's **Replay** context-menu action during Play Mode to rebuild and
replay both players.

The sequence is assembled in
`ReusableTweenSequenceSample.cs` rather than hand-authoring Unity's
version-sensitive managed-reference YAML. In production, create the same shared
asset from the Create menu and assign it to each player.

## Inspector and preview

The add-on includes dedicated inspectors for both `TweenSequenceAsset` and
`TweenSequencePlayer`. The asset inspector uses Valkyrie's managed-reference
type discovery and shows a timeline summary. The player inspector reports
pre-play validation diagnostics and exposes playback controls while the editor
is in Play Mode.

There is intentionally no EditMode tween preview. Playback and preview are
**Play Mode only** unless a future optional preview integration is installed.

## Tests

The package contains separate EditMode and PlayMode test assemblies guarded by
`UNITY_INCLUDE_TESTS`. Add the add-on package name to the consuming project's
testables list when needed:

```json
"testables": [
  "com.misterpxl.valkyrie.dotween"
]
```

Run both groups from **Window > General > Test Runner**. EditMode tests cover
managed-reference serialization/type discovery, the dedicated inspector
routing, diagnostics, and root-package isolation. PlayMode tests manually seek
sequences to cover all built-in steps, timeline placement, controls, identity,
and cleanup without depending on frame timing.

## Root package isolation

Valkyrie's normal root release branch contains no add-on files, and its
`Runtime` and `Editor` assemblies do not reference DOTween. Only dedicated
`dotween-v*` revisions contain this source package. Projects that install only
`com.misterpxl.valkyrie` from a normal root release therefore do not need
DOTween.
