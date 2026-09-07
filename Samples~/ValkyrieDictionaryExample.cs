using System;
using UnityEngine;
using Astra.Valkyrie;
using Astra.Valkyrie.Collections;

namespace Astra.Valkyrie.Samples
{

// ═══════════════════════════════════════════════════════
//  Supporting types for the demo
// ═══════════════════════════════════════════════════════

[UnityEngine.Scripting.APIUpdating.MovedFrom(false, "")]
public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[Serializable]
[UnityEngine.Scripting.APIUpdating.MovedFrom(false, "")]
public class ItemConfig
{
    public string displayName = "Item";
    public Color color = Color.white;
    [Range(0f, 1f)]
    public float dropRate = 0.1f;
}

// ═══════════════════════════════════════════════════════
//  Demo MonoBehaviour
// ═══════════════════════════════════════════════════════

/// <summary>
/// Demonstrates SerializableDictionary with 3 different type combinations.
/// Attach to a GameObject to test in the inspector.
/// </summary>
[UnityEngine.Scripting.APIUpdating.MovedFrom(false, "")]
public class ValkyrieDictionaryExample : MonoBehaviour
{
    // ── string → int ─────────────────────────────────────

    [Title("Scores", "Simple string-to-int dictionary")]
    [SerializeField]
    private SerializableDictionary<string, int> _scores = new();

    // ── string → GameObject ──────────────────────────────

    [Title("Prefab Registry", "Map names to prefabs")]
    [SerializeField]
    private SerializableDictionary<string, GameObject> _prefabs = new();

    // ── enum → serializable class ────────────────────────

    [Title("Item Configuration", "Map rarity to config")]
    [SerializeField]
    private SerializableDictionary<ItemRarity, ItemConfig> _itemConfigs = new();

    // ── Runtime usage demo ───────────────────────────────

    [Button("Log All Scores")]
    private void LogScores()
    {
        Debug.Log($"--- Scores ({_scores.Count}) ---");
        foreach (var kvp in _scores)
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
    }

    [Button("Add Test Score")]
    private void AddTestScore()
    {
        string key = $"Player_{_scores.Count}";
        _scores[key] = UnityEngine.Random.Range(0, 1000);
        Debug.Log($"Added {key}");
    }

    [Button("Log Item Configs")]
    private void LogItemConfigs()
    {
        Debug.Log($"--- Item Configs ({_itemConfigs.Count}) ---");
        foreach (var kvp in _itemConfigs)
            Debug.Log($"  {kvp.Key}: {kvp.Value.displayName} (drop: {kvp.Value.dropRate:P0})");
    }

    [Button("Lookup Prefab 'Player'")]
    private void LookupPlayer()
    {
        if (_prefabs.TryGetValue("Player", out var go))
            Debug.Log($"Found prefab: {go.name}");
        else
            Debug.Log("No prefab registered for key 'Player'");
    }
}

}
