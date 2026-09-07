using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Astra.Valkyrie.Editor;

namespace Astra.Valkyrie.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class CustomEditorIntegrationTests
    {
        private FallbackEditorTestObject _fallbackTarget;
        private SpecificEditorTestObject _specificTarget;
        private IntegratedEditorTestObject _integratedTarget;
        private UnityEditor.Editor _editor;

        [SetUp]
        public void SetUp()
        {
            _fallbackTarget = ScriptableObject.CreateInstance<FallbackEditorTestObject>();
            _specificTarget = ScriptableObject.CreateInstance<SpecificEditorTestObject>();
            _integratedTarget = ScriptableObject.CreateInstance<IntegratedEditorTestObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_editor != null)
                Object.DestroyImmediate(_editor);

            if (_fallbackTarget != null)
                Object.DestroyImmediate(_fallbackTarget);

            if (_specificTarget != null)
                Object.DestroyImmediate(_specificTarget);

            if (_integratedTarget != null)
                Object.DestroyImmediate(_integratedTarget);
        }

        [Test]
        public void ScriptableObjectWithoutCustomEditor_UsesValkyrieFallback()
        {
            _editor = UnityEditor.Editor.CreateEditor(_fallbackTarget);

            Assert.That(_editor, Is.TypeOf<ValkyrieScriptableObjectEditor>());
        }

        [Test]
        public void TypeSpecificCustomEditor_TakesPriorityOverValkyrieFallback()
        {
            _editor = UnityEditor.Editor.CreateEditor(_specificTarget);

            Assert.That(_editor, Is.TypeOf<SpecificEditorTestObjectEditor>());
        }

        [Test]
        public void TypeSpecificEditor_CanIntegrateThroughValkyrieEditor()
        {
            _editor = UnityEditor.Editor.CreateEditor(_integratedTarget);

            Assert.That(_editor, Is.TypeOf<IntegratedEditorTestObjectEditor>());
            Assert.That(_editor, Is.InstanceOf<ValkyrieEditor>());
        }
    }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class FallbackEditorTestObject : ScriptableObject { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class SpecificEditorTestObject : ScriptableObject { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class IntegratedEditorTestObject : ScriptableObject { }

    [CustomEditor(typeof(SpecificEditorTestObject))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class SpecificEditorTestObjectEditor : UnityEditor.Editor { }

    [CustomEditor(typeof(IntegratedEditorTestObject))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class IntegratedEditorTestObjectEditor : ValkyrieEditor { }
}
