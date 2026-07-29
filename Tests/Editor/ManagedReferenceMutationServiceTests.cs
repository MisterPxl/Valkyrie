using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Valkyrie.Editor;

namespace Valkyrie.Tests.Editor
{
    public sealed class ManagedReferenceMutationServiceTests
    {
        private ManagedReferenceTestObject _targetA;
        private ManagedReferenceTestObject _targetB;

        [SetUp]
        public void SetUp()
        {
            _targetA = ScriptableObject.CreateInstance<ManagedReferenceTestObject>();
            _targetB = ScriptableObject.CreateInstance<ManagedReferenceTestObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_targetA != null)
                Object.DestroyImmediate(_targetA);

            if (_targetB != null)
                Object.DestroyImmediate(_targetB);
        }

        [Test]
        public void AssignType_PreservesFieldsWithMatchingNames()
        {
            _targetA.action = new PreserveSourceAction
            {
                sharedValue = 42,
                sharedName = "kept"
            };

            var serializedObject = new SerializedObject(_targetA);
            ManagedReferenceMutationService.AssignType(
                serializedObject,
                nameof(ManagedReferenceTestObject.action),
                typeof(PreserveTargetAction),
                preserveExistingValues: true);

            var result = (PreserveTargetAction)_targetA.action;
            Assert.That(result.sharedValue, Is.EqualTo(42));
            Assert.That(result.sharedName, Is.EqualTo("kept"));
            Assert.That(result.targetOnlyValue, Is.EqualTo(7));
        }

        [Test]
        public void ResetCurrentType_CreatesFreshInstance()
        {
            _targetA.action = new PreserveTargetAction
            {
                sharedValue = 99,
                sharedName = "discard"
            };

            var serializedObject = new SerializedObject(_targetA);
            ManagedReferenceMutationService.ResetCurrentType(serializedObject, nameof(ManagedReferenceTestObject.action));

            var result = (PreserveTargetAction)_targetA.action;
            Assert.That(result.sharedValue, Is.EqualTo(0));
            Assert.That(result.sharedName, Is.Null.Or.Empty);
            Assert.That(result.targetOnlyValue, Is.EqualTo(7));
        }

        [Test]
        public void AssignType_AppliesToEverySelectedObject()
        {
            var serializedObject = new SerializedObject(new Object[] { _targetA, _targetB });

            ManagedReferenceMutationService.AssignType(
                serializedObject,
                nameof(ManagedReferenceTestObject.action),
                typeof(PreserveSourceAction),
                preserveExistingValues: true);

            Assert.That(_targetA.action, Is.TypeOf<PreserveSourceAction>());
            Assert.That(_targetB.action, Is.TypeOf<PreserveSourceAction>());
        }

        [Test]
        public void AppendAndRemove_AppliesToEverySelectedObject()
        {
            var serializedObject = new SerializedObject(new Object[] { _targetA, _targetB });

            ManagedReferenceMutationService.AppendInstance(
                serializedObject,
                nameof(ManagedReferenceTestObject.actions),
                typeof(PreserveSourceAction));

            Assert.That(_targetA.actions, Has.Count.EqualTo(1));
            Assert.That(_targetB.actions, Has.Count.EqualTo(1));
            Assert.That(_targetA.actions[0], Is.TypeOf<PreserveSourceAction>());
            Assert.That(_targetB.actions[0], Is.TypeOf<PreserveSourceAction>());

            ManagedReferenceMutationService.RemoveAt(
                serializedObject,
                nameof(ManagedReferenceTestObject.actions),
                0);

            Assert.That(_targetA.actions, Is.Empty);
            Assert.That(_targetB.actions, Is.Empty);
        }

        [Test]
        public void Router_ResolvesNestedManagedReferenceBaseTypes()
        {
            var serializedObject = new SerializedObject(_targetA);

            SerializedProperty nestedReference = serializedObject.FindProperty("holder.nestedAction");
            SerializedProperty nestedCollection = serializedObject.FindProperty("holder.nestedActions");

            bool hasReferenceType = ManagedReferencePropertyRouter.TryGetManagedReferenceBaseType(
                nestedReference,
                out System.Type referenceType);
            bool hasCollectionType = ManagedReferencePropertyRouter.TryGetManagedReferenceCollectionElementType(
                nestedCollection,
                out System.Type collectionType);

            Assert.That(hasReferenceType, Is.True);
            Assert.That(referenceType, Is.EqualTo(typeof(ITestAction)));
            Assert.That(hasCollectionType, Is.True);
            Assert.That(collectionType, Is.EqualTo(typeof(ITestAction)));
        }

        [Test]
        public void SerializedObjectMutation_IsUndoable()
        {
            var serializedObject = new SerializedObject(_targetA);
            Undo.IncrementCurrentGroup();

            ManagedReferenceMutationService.AssignType(
                serializedObject,
                nameof(ManagedReferenceTestObject.action),
                typeof(PreserveSourceAction),
                preserveExistingValues: true);

            Assert.That(_targetA.action, Is.TypeOf<PreserveSourceAction>());

            Undo.PerformUndo();
            serializedObject.Update();

            Assert.That(_targetA.action, Is.Null);
        }
    }
}
