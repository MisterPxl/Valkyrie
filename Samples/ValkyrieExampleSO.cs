using UnityEngine;
using Valkyrie;

/// <summary>
/// Demonstrates all Valkyrie V1 attributes on a ScriptableObject.
/// Create via Assets > Create > Valkyrie > Example Config.
/// </summary>
[CreateAssetMenu(fileName = "ValkyrieExampleConfig", menuName = "Valkyrie/Example Config")]
public class ValkyrieExampleSO : ScriptableObject
{
    // ── Title + Required ─────────────────────────────────

    [Title("Game Configuration")]
    [InfoBox("Central configuration asset for the game.")]
    public string gameName = "My Game";

    [Required("A player prefab is required for the game to work.")]
    public GameObject playerPrefab;

    // ── FoldoutGroup: Difficulty ──────────────────────────

    [FoldoutGroup("Difficulty")]
    public float enemyDamageMultiplier = 1f;

    [FoldoutGroup("Difficulty")]
    public float playerHealthMultiplier = 1f;

    [FoldoutGroup("Difficulty")]
    [ReadOnly]
    public string difficultyLabel = "Normal";

    // ── FoldoutGroup: Audio ──────────────────────────────

    [FoldoutGroup("Audio")]
    [Range(0f, 1f)]
    public float masterVolume = 0.8f;

    [FoldoutGroup("Audio")]
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;

    [FoldoutGroup("Audio")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    // ── ShowIf / HideIf ──────────────────────────────────

    [Title("Debug")]
    public bool enableCheats;

    [ShowIf("enableCheats")]
    [InfoBox("Cheats are enabled. Disable before shipping!", InfoBoxType.Error)]
    public bool godMode;

    [ShowIf("enableCheats")]
    public int startingLevel = 1;

    [HideIf("enableCheats")]
    [InfoBox("Cheats are disabled. Good.")]
    public string releaseNote = "Production ready";

    // ── Buttons ──────────────────────────────────────────

    [Button("Reset to Defaults")]
    private void ResetDefaults()
    {
        enemyDamageMultiplier = 1f;
        playerHealthMultiplier = 1f;
        difficultyLabel = "Normal";
        masterVolume = 0.8f;
        musicVolume = 0.6f;
        sfxVolume = 1f;
        startingLevel = 1;
        godMode = false;
    }

    [Button("Log Config Summary")]
    private void LogSummary()
    {
        Debug.Log($"[{gameName}] Difficulty: {difficultyLabel} | Cheats: {enableCheats} | God Mode: {godMode}");
    }
}
