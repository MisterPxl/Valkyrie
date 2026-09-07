using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie;

public sealed class ValkyrieNestedInspectorExample : MonoBehaviour
{
    [Title("Nested attributes", "Expand Settings and Profiles to try conditions and groups.")]
    public NestedExampleSettings settings = new NestedExampleSettings();
    public List<NestedExampleSettings> profiles = new List<NestedExampleSettings> { new NestedExampleSettings() };

    [Title("Polymorphic clipboard", "Right-click a reference header to copy/paste; list elements also offer Duplicate.")]
    [SerializeReference] public INestedExampleAction action = new NestedExampleAction();
    [SerializeReference] public List<INestedExampleAction> actions = new List<INestedExampleAction> { new NestedExampleAction() };
}

[Serializable]
public sealed class NestedExampleSettings
{
    public bool advanced;
    [ShowIf(nameof(advanced))] public float speed = 3f;
    [Required] public GameObject target;
    [ReadOnly] public int version = 1;
    [FoldoutGroup("Limits")] public float minimum;
    [FoldoutGroup("Limits")] public float maximum = 10f;
}

public interface INestedExampleAction { }

[Serializable]
[ManagedReferenceCategory("Examples", "Nested action")]
public sealed class NestedExampleAction : INestedExampleAction
{
    public bool enabled = true;
    [ShowIf(nameof(enabled))] public string message = "Hello";
    public NestedExampleSettings settings = new NestedExampleSettings();
    [SerializeReference] public INestedExampleAction next;
}
