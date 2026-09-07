# Valkyrie 2.0 / DOTween integration 3.0 migration candidate

This candidate moves Valkyrie APIs into Astra. Install exact source revisions from
`Tools/Astra/catalog.json` in the framework repository; the working branch is
`codex/astra-conventions`. No new release tag has been created.

## Upgrade

1. Back up custom code, assets, all metadata, project settings and package
   manifests/lockfiles before opening the upgraded project.
2. Upgrade `com.misterpxl.valkyrie` to 2.0 and, when installed,
   `com.misterpxl.valkyrie.dotween` to 3.0 together. DOTween Free is still a separate
   prerequisite: install it and run its setup before adding the integration.
   Removing DOTween requires first removing its integration and dependent assets.
3. If using Aegis's Valkyrie rules, upgrade `com.misterpxl.aegis.valkyrie` to 2.0
   as well. Aegis itself stays at 1.x. Integration 1.x targeted Valkyrie 1.x;
   integration 2.x targets Valkyrie 2.x. Install all Git prerequisites explicitly.
4. Update C# imports and qualified names: `Valkyrie` becomes `Astra.Valkyrie`,
   `Valkyrie.Collections` becomes `Astra.Valkyrie.Collections`, and `Valkyrie.DOTween`
   becomes `Astra.Valkyrie.Integrations.DOTween`. Apply integration prefixes first.
   Update asmdef strings, reflection/type-name strings, linker descriptors and
   any custom generators using those APIs. Recompile prebuilt consumer DLLs.
5. Keep script/assembly GUIDs and serialized assets intact. Open Unity, inspect
   sequences and dictionaries, check missing references, then save and reopen.
   Exercise bindings, callbacks, playback and the inspector in your own content.

`MovedFrom(autoUpdateAPI: false, ...)` records serialized type provenance. It does
not provide automatic C# rewrites or binary compatibility. The annotations
assembly remains engine-free (`noEngineReferences: true`): its 13 public attribute
and enum types are source migrations, without Unity serialization annotations.
Unity-dependent package/sample types carry the appropriate old namespace and
assembly mapping. Generic type names are retained; their class name is not
hard-coded in the annotation.

| Old assembly | New assembly |
|---|---|
| `Valkyrie.DOTween.Editor` | `Astra.Valkyrie.Integrations.DOTween.Editor` |
| `Valkyrie.DOTween.UGUI.Runtime` | `Astra.Valkyrie.Integrations.DOTween.UGUI.Runtime` |
| `Valkyrie.DOTween.Runtime` | `Astra.Valkyrie.Integrations.DOTween.Runtime` |
| `Valkyrie.DOTween.Sample.ReusableSequence` | `Astra.Valkyrie.Integrations.DOTween.Samples.ReusableSequence.Runtime` |
| `Valkyrie.DOTween.Tests.EditMode` | `Astra.Valkyrie.Integrations.DOTween.Tests.EditMode` |
| `Valkyrie.DOTween.Tests.PlayMode` | `Astra.Valkyrie.Integrations.DOTween.Tests.PlayMode` |
| `Valkyrie.DOTween.Tests.UGUI` | `Astra.Valkyrie.Integrations.DOTween.UGUI.Tests.EditMode` |
| `Valkyrie.Editor` | `Astra.Valkyrie.Editor` |
| `Valkyrie.Collections` | `Astra.Valkyrie.Collections.Runtime` |
| `Valkyrie.Runtime` | `Astra.Valkyrie.Annotations` |
| `Valkyrie.Tests.Editor` | `Astra.Valkyrie.Tests.EditMode` |

[identities.json](identities.json) inventories 180 moved public types and the
13 engine-free annotation types. All 136 pre-existing `.meta` files are preserved,
including the renamed asmdefs. The base package has no DOTween/Aegis/Helios or
framework dependency. Collections retain their separate Unity-dependent assembly.

## Existing imported samples

Base sample types previously in the global namespace now use
`Astra.Valkyrie.Samples`; they retain class names and use an empty old namespace
in `MovedFrom`. Their assembly stays consumer-owned (normally `Assembly-CSharp`).
Update the imported sample C# files in place, preserving their existing GUIDs.
The DOTween sample has its own renamed assembly in the table above. Preserve its
asmdef GUID when updating it. Do not install a second copy beside an old sample,
regenerate script metadata, or replace configured prefab/scene data with defaults.

Consumer-defined classes need their own migration annotations if renamed. The
package cannot infer those classes' previous namespaces or assembly placements.

## Stable serialized and diagnostic identities

Serialized field names, generic dictionary entries, sequence step IDs, trigger
kinds, named bindings, UnityEvents and the runtime ID prefix `Valkyrie.DOTween/`
are unchanged. Existing diagnostic enum values, menu paths and preference keys
remain stable. Diagnostic fields that describe a C# type naturally report the new
type name; they are not persistent step identifiers.

The fixtures cover 22 built-in concrete steps, including five uGUI steps; configured
sequence/preset assets; a prefab and scene with an asset reference, target binding,
trigger and persistent event linked to a step ID; inline dictionaries of primitive
and Unity object values; and a shared managed step inside a cyclic consumer graph.
The consumer-owned graph node retains its own identity, while its payload moves
from the old DOTween assembly to the new one.

## Frozen fixtures and verification

`Legacy~` contains project-relative assets captured and re-opened with Valkyrie
`7f643420d29785e46a0c72410711f0db8096bdb8` (base 1.5.0, integration 2.1.0),
Aegis `9c3ebda096c42b5a8e721890171b8e5fc303a454` and Helios
`7201ceb8d671c3b050d027ecc2692aa243ae26c1`. `legacy-sha256.json` freezes their bytes.
The probe explicitly creates every step ID before saving its legacy baseline.
The first experimental capture omitted the last ID until after saving; it was
rejected by legacy-to-legacy verification and replaced before qualification.

Use the framework's `Tools/Astra/Migration/prepare_valkyrie_probe.py` in a new
isolated consumer, with `--api current --fixtures <this-directory>/Legacy~`.
The fixture includes the consumer script and assembly metadata required to retain
its script association; the probe sources come from the framework. Run `Verify`,
`Resave`, then `Verify` in separate Unity processes. The source API changes only;
the old YAML is not rewritten by a search-and-replace operation.

The framework's `Documentation/Astra/ValkyrieMigrationValidation.md` records Unity
suites, sample checks, optional-package checks and the representative Player build.
Scope: Unity 6000.4.0f1, macOS, Mono Development. It does not qualify Unity 6000.0,
IL2CPP, other platforms, release stripping, or arbitrary consumer-owned types.

## Rollback

Restore the old package pins and custom source together. If assets have been
saved with the new identities, restore their backed-up files and metadata too,
including settings and imported sample code. Old package versions are not expected
to read newly saved Astra identities. Do not attempt reverse migration by replacing
namespace strings in YAML.

Unity's [MovedFrom implementation](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Scripting/APIUpdating/UpdatedFromAttribute.cs)
provides the provenance mechanism. The probe checks
[missing managed-reference types](https://docs.unity.cn/ScriptReference/SerializationUtility.HasManagedReferencesWithMissingTypes.html)
as well as actual object data and playback.
