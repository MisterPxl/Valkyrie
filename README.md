# Valkyrie Inspector

Valkyrie is a lightweight inspector helper bundled with this template.

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
