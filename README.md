# Valkyrie Inspector

Valkyrie is a lightweight inspector helper for Unity projects.

## Installation

Add the package to the project's `Packages/manifest.json`:

```json
"com.misterpxl.valkyrie": "https://github.com/misterpxl/Valkyrie.git#v1.1.0"
```

## Sample

Import `Usage Examples` from the Package Manager, then open
`ValkyrieShowcase.unity`. Select the showcase prefab's child objects to inspect
the configured attributes, dictionaries, and polymorphic references.

`ValkyrieShowcase.prefab` can also be dropped into another test scene.

## Supported attributes

- `[Title]`, `[InfoBox]`, `[ReadOnly]`, `[Required]`
- `[ShowIf]`, `[HideIf]`
- `[FoldoutGroup]`
- `[Button]`
- `[SerializeReference]` type dropdowns and lists
- `SerializableDictionary<TKey, TValue>`

## Opt out

Valkyrie installs global editors for `MonoBehaviour` and `ScriptableObject`.

**Per-type:** add `[DisableValkyrieInspector]` to any class that should use Unity's
default inspector instead.

**Project-wide:** add the `VALKYRIE_DISABLE_GLOBAL_INSPECTOR` scripting define
(Project Settings > Player > Scripting Define Symbols) to disable the global
takeover entirely and fall back to Unity's default inspectors everywhere. The
attributes and `SerializableDictionary` remain usable via explicit custom editors.

```csharp
[DisableValkyrieInspector]
public sealed class ThirdPartyAdapter : MonoBehaviour
{
}
```

Unity's `[HideInInspector]` is respected and hidden fields are not rendered.
