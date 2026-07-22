using UnityEngine;
using Valkyrie;

/// <summary>
/// Demonstrates all Valkyrie V1 attributes on a MonoBehaviour.
/// Attach to any GameObject to see the inspector in action.
/// </summary>
public class ValkyrieExample : MonoBehaviour
{
    // ── Title + InfoBox + Required ───────────────────────

    [Title("Identity", "Core character setup")]
    [InfoBox("Configure the character name and assign a target.")]
    public string characterName = "Hero";

    [Required("A target transform must be assigned!")]
    public Transform target;

    [Required]
    public string faction;

    // ── FoldoutGroup: Stats (with ReadOnly) ──────────────

    [FoldoutGroup("Stats")]
    [ReadOnly]
    public int currentLevel = 1;

    [FoldoutGroup("Stats")]
    [ReadOnly]
    public float playTime = 42.5f;

    [FoldoutGroup("Stats")]
    public int hp = 100;

    [FoldoutGroup("Stats")]
    public int attack = 15;

    // ── FoldoutGroup: Movement (with ShowIf) ─────────────

    [FoldoutGroup("Movement")]
    public float speed = 5f;

    [FoldoutGroup("Movement")]
    public float jumpHeight = 2f;

    [FoldoutGroup("Movement")]
    [ShowIf("HasTarget")]
    public float followDistance = 3f;

    // ── ShowIf / HideIf ──────────────────────────────────

    [Title("Advanced Settings")]
    public bool enableAdvanced;

    [ShowIf("enableAdvanced")]
    [InfoBox("These options are experimental.", InfoBoxType.Warning)]
    public float advancedMultiplier = 2f;

    [ShowIf("enableAdvanced")]
    public bool debugTrails;

    [HideIf("enableAdvanced")]
    public string simpleMode = "Standard mode is active";

    // ── Condition via method ─────────────────────────────

    [ShowIf("IsMaxLevel")]
    [InfoBox("Congratulations! Max level reached!")]
    public string maxLevelReward = "Golden Crown";

    // ── Buttons ──────────────────────────────────────────

    [Button("Reset Stats")]
    private void ResetStats()
    {
        currentLevel = 1;
        playTime = 0f;
        hp = 100;
        attack = 15;
    }

    [Button]
    private void LevelUp()
    {
        currentLevel++;
        attack += 3;
        hp += 20;
    }

    [Button("Log Character Info")]
    private void LogInfo()
    {
        Debug.Log($"[{characterName}] Level {currentLevel} | HP {hp} | ATK {attack}");
    }

    // ── Condition helpers ────────────────────────────────

    private bool IsMaxLevel() => currentLevel >= 99;
    private bool HasTarget => target != null;
}
