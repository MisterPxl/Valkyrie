using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Valkyrie.Editor;
using Valkyrie.DOTween.Editor;

namespace Valkyrie.DOTween.Tests.EditMode
{
    public sealed class TweenSerializationAndInspectorTests
    {
        [Test]
        public void ManagedReferenceTypeCache_FindsBuiltInTweenSteps()
        {
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(TweenStepDefinition));

            Assert.That(types, Does.Contain(typeof(TransformMoveStepDefinition)));
            Assert.That(types, Does.Contain(typeof(TransformScaleStepDefinition)));
            Assert.That(types, Does.Contain(typeof(CanvasGroupFadeStepDefinition)));
            Assert.That(types, Does.Contain(typeof(IntervalStepDefinition)));
            Assert.That(types, Does.Contain(typeof(CallbackStepDefinition)));
        }

        [Test]
        public void TweenSequenceAsset_StepsPropertyRoutesAsManagedReferenceCollection()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                SerializedObject serializedObject = new SerializedObject(asset);
                SerializedProperty timeline = serializedObject.FindProperty("_timeline");
                SerializedProperty steps = timeline.FindPropertyRelative("_steps");

                Type elementType;
                Assert.That(
                    ManagedReferencePropertyRouter.TryGetManagedReferenceCollectionElementType(steps, out elementType),
                    Is.True);
                Assert.That(elementType, Is.EqualTo(typeof(TweenStepDefinition)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void TweenPlayer_UsesDedicatedValkyrieEditor()
        {
            GameObject owner = new GameObject("TweenPlayer");
            UnityEditor.Editor editor = null;
            try
            {
                TweenPlayer player = owner.AddComponent<TweenPlayer>();
                editor = UnityEditor.Editor.CreateEditor(player);

                Assert.That(editor, Is.TypeOf<TweenPlayerEditor>());
                Assert.That(editor, Is.InstanceOf<ValkyrieEditor>());
            }
            finally
            {
                if (editor != null) UnityEngine.Object.DestroyImmediate(editor);
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void BuiltInStepCategories_AreDeclaredForDesignerPicker()
        {
            ManagedReferenceCategoryAttribute category = (ManagedReferenceCategoryAttribute)Attribute
                .GetCustomAttributes(typeof(TransformPunchPositionStepDefinition), typeof(ManagedReferenceCategoryAttribute))
                .FirstOrDefault();

            Assert.That(category, Is.Not.Null);
            Assert.That(category.Path, Is.EqualTo("Punch"));
            Assert.That(category.Label, Is.EqualTo("Position"));
        }
    }
}
