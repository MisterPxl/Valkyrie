using System.Collections.Generic;
using UnityEngine;
using Valkyrie;

/// <summary>
/// Demonstrates Valkyrie's <c>[SerializeReference]</c> support on collections.
/// Each entry in a list / array is itself a polymorphic managed reference and is
/// drawn through <c>ManagedReferenceListRenderer</c>:
/// <list type="bullet">
///   <item>Header with element count + "+ Add" dropdown of compatible types</item>
///   <item>Per-element type selector, reorder (▲ / ▼), individual remove (×)</item>
///   <item>Clear-all button on the header</item>
/// </list>
///
/// <para>Reuses <c>ICondition</c> and <c>RewardBase</c> declared in
/// <c>ValkyriePolymorphicExample.cs</c> to avoid duplicating sample types.</para>
/// </summary>
public sealed class ValkyriePolymorphicCollectionExample : MonoBehaviour
{
    [Title("Conditions (List)", "Interface-based polymorphism in a List<T>")]
    [InfoBox("Click '+ Add' on the list header to insert a HealthCondition, TagCondition, or DistanceCondition.")]
    [SerializeReference]
    private List<ICondition> _conditions = new List<ICondition>();

    [Title("Rewards (Array)", "Abstract-class-based polymorphism in a T[]")]
    [InfoBox("Same UX as the list above — works on arrays too.")]
    [SerializeReference]
    private RewardBase[] _rewards = new RewardBase[0];

    [Title("Conditional Collection")]
    public bool enableBonusRewards;

    [ShowIf("enableBonusRewards")]
    [SerializeReference]
    private List<RewardBase> _bonusRewards = new List<RewardBase>();

    [Button("Log Conditions")]
    private void LogConditions()
    {
        if (_conditions == null || _conditions.Count == 0)
        {
            Debug.Log("No conditions configured.");
            return;
        }

        for (int i = 0; i < _conditions.Count; i++)
        {
            var c = _conditions[i];
            Debug.Log($"[{i}] {c?.GetType().Name ?? "null"}");
        }
    }

    [Button("Log Rewards")]
    private void LogRewards()
    {
        if (_rewards == null || _rewards.Length == 0)
        {
            Debug.Log("No rewards configured.");
            return;
        }

        for (int i = 0; i < _rewards.Length; i++)
        {
            var r = _rewards[i];
            Debug.Log($"[{i}] {r?.GetType().Name ?? "null"} — name='{r?.rewardName}'");
        }
    }

    [Button("Clear All")]
    private void ClearAll()
    {
        _conditions?.Clear();
        _rewards = new RewardBase[0];
        _bonusRewards?.Clear();
    }
}
