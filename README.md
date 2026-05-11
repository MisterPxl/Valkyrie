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
Add `[DisableValkyrieInspector]` to any class that should use Unity's default
inspector instead.

```csharp
[DisableValkyrieInspector]
public sealed class ThirdPartyAdapter : MonoBehaviour
{
}
```

Unity's `[HideInInspector]` is respected and hidden fields are not rendered.
