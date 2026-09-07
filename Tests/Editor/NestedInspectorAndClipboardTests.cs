using System;
using System.Collections;
using UnityEngine.TestTools;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Astra.Valkyrie.Editor;
using Object = UnityEngine.Object;

namespace Astra.Valkyrie.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface IClipboardNode { }
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class ClipboardNode : IClipboardNode
    {
        public int value;
        public Object asset;
        [SerializeReference] public IClipboardNode left;
        [SerializeReference] public IClipboardNode right;
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class NestedInspectorData
    {
        public bool show;
        [ShowIf("show")] public int conditional;
        [HideIf("show")] public int inverse;
        [Required] public string required;
        [ReadOnly] public DrawerData locked = new DrawerData();
        [FoldoutGroup("Details")] public int grouped;
        [Title("Title", "Subtitle"), InfoBox("Information")] public int decorated;
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    [Serializable] public class DrawerData { [ShowIf("missing")] public int number; }
    [CustomPropertyDrawer(typeof(DrawerData))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class TestNestedDrawer : PropertyDrawer
    {
        public static bool DrewDisabled;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => 61f;
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            DrewDisabled |= !GUI.enabled;
            EditorGUI.LabelField(rect, label);
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class NestedInspectorHost : ScriptableObject
    {
        public bool show = true;
        public NestedInspectorData nested = new NestedInspectorData();
        public List<NestedInspectorData> list = new List<NestedInspectorData> { new NestedInspectorData() };
        [SerializeReference] public NestedInspectorData polymorphic = new NestedInspectorData();
        [SerializeReference] public IClipboardNode node;
        [SerializeReference] public List<IClipboardNode> nodes = new List<IClipboardNode>();
        [SerializeReference] public NestedInspectorData incompatible;
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class NestedInspectorTestWindow : EditorWindow
    {
        public SerializedObject Owner;
        private void OnGUI()
        {
            if (Owner == null) return;
            Owner.Update();
            PropertyRenderer.Draw(new Rect(0, 0, 300, 61), Owner.FindProperty("nested.locked"));
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class NestedInspectorAndClipboardTests
    {
        private NestedInspectorHost _a;
        private NestedInspectorHost _b;
        [SetUp] public void SetUp()
        {
            _a = ScriptableObject.CreateInstance<NestedInspectorHost>();
            _b = ScriptableObject.CreateInstance<NestedInspectorHost>();
            ManagedReferenceClipboard.Clear();
            EditorStateCache.Clear();
        }
        [TearDown] public void TearDown()
        {
            Object.DestroyImmediate(_a);
            Object.DestroyImmediate(_b);
            ManagedReferenceClipboard.Clear();
            EditorStateCache.Clear();
        }

        [TestCase("nested")]
        [TestCase("list.Array.data[0]")]
        [TestCase("polymorphic")]
        public void ConditionsUseTheNestedOwner(string parent)
        {
            using var so = new SerializedObject(_a);
            var field = so.FindProperty(parent + ".conditional");
            Assert.That(PropertyRenderer.IsVisible(field, out string warning), Is.False);
            Assert.That(warning, Is.Null);
            Assert.That(PropertyRenderer.GetHeight(field), Is.Zero);
            Assert.That(PropertyRenderer.IsVisible(so.FindProperty(parent + ".inverse"), out _), Is.True);
            ((NestedInspectorData)SerializedPropertyContext.GetValue(_a, parent)).show = true;
            so.Update();
            Assert.That(PropertyRenderer.IsVisible(field, out _), Is.True);
        }

        [Test] public void NestedVisibilityMustMatchEverySelectedObject()
        {
            _a.nested.show = true;
            _b.nested.show = false;
            using var so = new SerializedObject(new Object[] { _a, _b });
            Assert.That(PropertyRenderer.IsVisible(so.FindProperty("nested.conditional"), out _), Is.False);
        }

        [Test] public void NestedRequiredAndDecorationsContributeToHeight()
        {
            using var so = new SerializedObject(_a);
            var required = so.FindProperty("nested.required");
            Assert.That(PropertyRenderer.GetRequiredMessage(required, new RequiredAttribute()), Is.Not.Null);
            Assert.That(PropertyRenderer.GetHeight(required), Is.GreaterThan(EditorGUIUtility.singleLineHeight));
            Assert.That(PropertyRenderer.GetHeight(so.FindProperty("nested.decorated")), Is.GreaterThan(EditorGUIUtility.singleLineHeight * 3));
            _a.nested.required = "set";
            so.Update();
            Assert.That(PropertyRenderer.GetRequiredMessage(required, new RequiredAttribute()), Is.Null);
        }

        [Test] public void NestedGroupsUseTheFullPropertyPath()
        {
            using var so = new SerializedObject(_a);
            var ordinary = so.FindProperty("nested");
            var polymorphic = so.FindProperty("polymorphic");
            ordinary.isExpanded = polymorphic.isExpanded = true;
            float before = PropertyRenderer.GetHeight(ordinary);
            float otherBefore = PropertyRenderer.GetHeight(polymorphic);
#if UNITY_6000_3_OR_NEWER
            string id = _a.GetEntityId().ToString();
#else
            string id = _a.GetInstanceID().ToString();
#endif
            EditorStateCache.Set(id + ":nested:Details", true);
            Assert.That(PropertyRenderer.GetHeight(ordinary), Is.GreaterThan(before));
            Assert.That(PropertyRenderer.GetHeight(polymorphic), Is.EqualTo(otherBefore));
        }

        [UnityTest] public IEnumerator CustomDrawersKeepTheirHeightAndReadOnlyScope()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("This GUI integration test requires a graphics device; run Unity without -nographics.");
            using var so = new SerializedObject(_a);
            Assert.That(PropertyRenderer.GetHeight(so.FindProperty("nested.locked")), Is.EqualTo(61f));
            var window = ScriptableObject.CreateInstance<NestedInspectorTestWindow>();
            try
            {
                TestNestedDrawer.DrewDisabled = false;
                window.Owner = so;
                window.Show();
                yield return null;
                window.SendEvent(new Event { type = EventType.Repaint });
                Assert.That(TestNestedDrawer.DrewDisabled, Is.True);
            }
            finally { window.Close(); }
        }

        [Test] public void CopyPastePreservesCyclesSharedNodesAndUnityReferences()
        {
            var shared = new ClipboardNode { value = 12, asset = _a };
            var root = new ClipboardNode { left = shared, right = shared };
            shared.left = root;
            _a.node = root;
            using var source = new SerializedObject(_a);
            using var destination = new SerializedObject(_b);
            Assert.That(ManagedReferenceClipboard.Copy(source, "node"), Is.True);
            shared.value = 99; // Copy captures a snapshot, not a live reference.
            Assert.That(ManagedReferenceClipboard.Paste(destination, "node"), Is.True);
            var clone = (ClipboardNode)_b.node;
            var child = (ClipboardNode)clone.left;
            Assert.That(clone, Is.Not.SameAs(root));
            Assert.That(clone.left, Is.SameAs(clone.right));
            Assert.That(child.left, Is.SameAs(clone));
            Assert.That(child.value, Is.EqualTo(12));
            Assert.That(child.asset, Is.SameAs(_a));
        }

        [Test] public void IncompatiblePasteDoesNotChangeTheDestination()
        {
            _a.node = new ClipboardNode();
            _b.incompatible = new NestedInspectorData { required = "keep" };
            using var source = new SerializedObject(_a);
            using var destination = new SerializedObject(_b);
            ManagedReferenceClipboard.Copy(source, "node");
            Assert.That(ManagedReferenceClipboard.CanPaste(destination, "incompatible"), Is.False);
            Assert.That(ManagedReferenceClipboard.Paste(destination, "incompatible"), Is.False);
            Assert.That(_b.incompatible.required, Is.EqualTo("keep"));
        }

        [Test] public void PasteCreatesIndependentGraphsAcrossSelectionAndSupportsUndo()
        {
            _a.node = new ClipboardNode { left = new ClipboardNode { value = 7 } };
            using var source = new SerializedObject(_a);
            ManagedReferenceClipboard.Copy(source, "node");
            using var both = new SerializedObject(new Object[] { _a, _b });
            Undo.IncrementCurrentGroup();
            Assert.That(ManagedReferenceClipboard.Paste(both, "node"), Is.True);
            Assert.That(_a.node, Is.Not.SameAs(_b.node));
            Assert.That(((ClipboardNode)_a.node).left, Is.Not.SameAs(((ClipboardNode)_b.node).left));
            Undo.PerformUndo();
            Assert.That(_b.node, Is.Null);
        }

        [Test] public void DuplicateUsesEachTargetsOwnValueAndInsertsAfterTheSource()
        {
            _a.nodes.Add(new ClipboardNode { value = 1, left = new ClipboardNode() });
            _b.nodes.Add(new ClipboardNode { value = 2, left = new ClipboardNode() });
            using var both = new SerializedObject(new Object[] { _a, _b });
            Undo.IncrementCurrentGroup();
            Assert.That(ManagedReferenceClipboard.Duplicate(both, "nodes", 0), Is.True);
            Assert.That(_a.nodes.Count, Is.EqualTo(2));
            Assert.That(((ClipboardNode)_a.nodes[1]).value, Is.EqualTo(1));
            Assert.That(((ClipboardNode)_b.nodes[1]).value, Is.EqualTo(2));
            Assert.That(((ClipboardNode)_a.nodes[1]).left, Is.Not.SameAs(((ClipboardNode)_a.nodes[0]).left));
            Undo.PerformUndo();
            Assert.That(_a.nodes.Count, Is.EqualTo(1));
            Assert.That(_b.nodes.Count, Is.EqualTo(1));
        }
    }
}
