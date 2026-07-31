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
        private const string TemporaryAssetPath = "Assets/__ValkyrieDOTweenSerializationTest.asset";
        private const string TemporaryCopyPath = "Assets/__ValkyrieDOTweenSerializationCopy.asset";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TemporaryAssetPath);
            AssetDatabase.DeleteAsset(TemporaryCopyPath);
        }

        [Test]
        public void SerializeReferenceSteps_RoundTripWithConcreteTypesAndValues()
        {
            AssetDatabase.DeleteAsset(TemporaryAssetPath);
            AssetDatabase.DeleteAsset(TemporaryCopyPath);

            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            TransformMoveStepDefinition move = new TransformMoveStepDefinition
            {
                TargetKey = "Mover",
                EndValue = new Vector3(2f, 3f, 4f),
                Duration = 1.25f,
                Local = true
            };
            IntervalStepDefinition interval = new IntervalStepDefinition
            {
                Duration = 0.75f
            };
            asset.Steps.Add(move);
            asset.Steps.Add(interval);

            AssetDatabase.CreateAsset(asset, TemporaryAssetPath);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            Assert.That(AssetDatabase.CopyAsset(TemporaryAssetPath, TemporaryCopyPath), Is.True);
            AssetDatabase.ImportAsset(TemporaryCopyPath, ImportAssetOptions.ForceSynchronousImport);

            TweenSequenceAsset reloaded = AssetDatabase.LoadAssetAtPath<TweenSequenceAsset>(TemporaryCopyPath);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded, Is.Not.SameAs(asset));
            Assert.That(reloaded.Steps, Has.Count.EqualTo(2));

            TransformMoveStepDefinition reloadedMove = reloaded.Steps[0] as TransformMoveStepDefinition;
            Assert.That(reloadedMove, Is.Not.Null);
            Assert.That(reloadedMove.TargetKey, Is.EqualTo("Mover"));
            Assert.That(reloadedMove.EndValue, Is.EqualTo(new Vector3(2f, 3f, 4f)));
            Assert.That(reloadedMove.Duration, Is.EqualTo(1.25f));
            Assert.That(reloadedMove.Local, Is.True);

            IntervalStepDefinition reloadedInterval = reloaded.Steps[1] as IntervalStepDefinition;
            Assert.That(reloadedInterval, Is.Not.Null);
            Assert.That(reloadedInterval.Duration, Is.EqualTo(0.75f));
        }

        [Test]
        public void ManagedReferenceTypeDiscovery_FindsEveryBuiltInStep()
        {
            ManagedReferenceTypeCache.Clear();
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(TweenStepDefinition));

            Assert.That(types, Does.Contain(typeof(TransformMoveStepDefinition)));
            Assert.That(types, Does.Contain(typeof(TransformRotationStepDefinition)));
            Assert.That(types, Does.Contain(typeof(TransformScaleStepDefinition)));
            Assert.That(types, Does.Contain(typeof(CanvasGroupFadeStepDefinition)));
            Assert.That(types, Does.Contain(typeof(IntervalStepDefinition)));
            Assert.That(types.All(IsConcreteSerializableStep), Is.True);
        }

        [Test]
        public void StepsProperty_IsRecognizedAsManagedReferenceCollection()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            try
            {
                SerializedObject serializedObject = new SerializedObject(asset);
                SerializedProperty steps = serializedObject.FindProperty("_steps");

                Type elementType;
                bool recognized = ManagedReferencePropertyRouter.TryGetManagedReferenceCollectionElementType(
                    steps,
                    out elementType);

                Assert.That(recognized, Is.True);
                Assert.That(elementType, Is.EqualTo(typeof(TweenStepDefinition)));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void DedicatedDOTweenInspectors_HandleAssetAndPlayer()
        {
            TweenSequenceAsset asset = ScriptableObject.CreateInstance<TweenSequenceAsset>();
            GameObject gameObject = new GameObject("DOTween Inspector Test");
            TweenSequencePlayer player = gameObject.AddComponent<TweenSequencePlayer>();
            UnityEditor.Editor assetEditor = null;
            UnityEditor.Editor playerEditor = null;

            try
            {
                assetEditor = UnityEditor.Editor.CreateEditor(asset);
                playerEditor = UnityEditor.Editor.CreateEditor(player);

                Assert.That(assetEditor, Is.TypeOf<TweenSequenceAssetEditor>());
                Assert.That(playerEditor, Is.TypeOf<TweenSequencePlayerEditor>());
                Assert.That(assetEditor, Is.InstanceOf<ValkyrieEditor>());
                Assert.That(playerEditor, Is.InstanceOf<ValkyrieEditor>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(assetEditor);
                UnityEngine.Object.DestroyImmediate(playerEditor);
                UnityEngine.Object.DestroyImmediate(asset);
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static bool IsConcreteSerializableStep(Type type)
        {
            return type != null &&
                   typeof(TweenStepDefinition).IsAssignableFrom(type) &&
                   !type.IsAbstract &&
                   type.IsSerializable;
        }
    }
}
