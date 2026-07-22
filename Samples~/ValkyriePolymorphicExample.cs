using System;
using UnityEngine;
using Valkyrie;

// ═══════════════════════════════════════════════════════
//  Interface-based polymorphism
// ═══════════════════════════════════════════════════════

public interface ICondition
{
    bool Evaluate();
}

[Serializable]
public class HealthCondition : ICondition
{
    public int minHealth = 50;
    public int maxHealth = 100;

    public bool Evaluate() => true;
}

[Serializable]
public class TagCondition : ICondition
{
    public string requiredTag = "Player";

    public bool Evaluate() => true;
}

[Serializable]
public class DistanceCondition : ICondition
{
    public float maxDistance = 10f;
    public bool useSquaredDistance;

    public bool Evaluate() => true;
}

// ═══════════════════════════════════════════════════════
//  Abstract class-based polymorphism
// ═══════════════════════════════════════════════════════

[Serializable]
public abstract class RewardBase
{
    public string rewardName = "Reward";
}

[Serializable]
public class GoldReward : RewardBase
{
    public int amount = 100;
}

[Serializable]
public class ItemReward : RewardBase
{
    public string itemId = "sword_01";
    public int quantity = 1;
}

[Serializable]
public class ExperienceReward : RewardBase
{
    public int xpAmount = 250;
    public float multiplier = 1f;
}

// ═══════════════════════════════════════════════════════
//  Demo MonoBehaviour
// ═══════════════════════════════════════════════════════

/// <summary>
/// Demonstrates Valkyrie's SerializeReference support.
/// Attach to a GameObject to see polymorphic type selectors.
/// </summary>
// CS0414: sample fields are inspected through the editor only, not read in code.
#pragma warning disable 0414
public class ValkyriePolymorphicExample : MonoBehaviour
{
    // ── Interface field ──────────────────────────────────

    [Title("Condition System", "Interface-based polymorphism")]
    [InfoBox("Select a condition type from the dropdown.")]
    [SerializeReference]
    private ICondition _condition;

    [SerializeReference]
    private ICondition _secondCondition;

    // ── Abstract class field ─────────────────────────────

    [Title("Reward System", "Abstract class-based polymorphism")]
    [SerializeReference]
    private RewardBase _primaryReward;

    [SerializeReference]
    private RewardBase _bonusReward;

    // ── Combined with other Valkyrie attributes ──────────

    [Title("Conditional Polymorphism")]
    public bool enableOptionalReward;

    [ShowIf("enableOptionalReward")]
    [InfoBox("This reward only appears when the toggle is on.", InfoBoxType.Warning)]
    [SerializeReference]
    private RewardBase _optionalReward;

    // ── Buttons ──────────────────────────────────────────

    [Button("Log Current Setup")]
    private void LogSetup()
    {
        Debug.Log($"Condition: {_condition?.GetType().Name ?? "None"}");
        Debug.Log($"Primary Reward: {_primaryReward?.rewardName ?? "None"}");
        Debug.Log($"Bonus Reward: {_bonusReward?.rewardName ?? "None"}");
    }

    [Button("Clear All References")]
    private void ClearAll()
    {
        _condition = null;
        _secondCondition = null;
        _primaryReward = null;
        _bonusReward = null;
        _optionalReward = null;
    }
}
#pragma warning restore 0414
