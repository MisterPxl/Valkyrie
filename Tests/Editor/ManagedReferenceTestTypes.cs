using System;
using System.Collections.Generic;
using UnityEngine;

namespace Astra.Valkyrie.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface ITestAction { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface IActor { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface INetworkActor : IActor { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface IProducer<out T> { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface IConsumer<in T> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class ManagedReferenceTestObject : ScriptableObject
    {
        [SerializeReference]
        public ITestAction action;

        [SerializeReference]
        public List<ITestAction> actions = new List<ITestAction>();

        public NestedManagedReferenceHolder holder = new NestedManagedReferenceHolder();
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class NestedManagedReferenceHolder
    {
        [SerializeReference]
        public ITestAction nestedAction;

        [SerializeReference]
        public List<ITestAction> nestedActions = new List<ITestAction>();
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class ConcreteBaseAction : ITestAction
    {
        public int value;
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class ConcreteDerivedAction : ConcreteBaseAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class PreserveSourceAction : ITestAction
    {
        public int sharedValue;
        public string sharedName;
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class PreserveTargetAction : ITestAction
    {
        public int sharedValue;
        public string sharedName;
        public int targetOnlyValue = 7;
    }

    [Serializable]
    [ManagedReferenceCategory("Gameplay/Actions", "Categorized Action", -10)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class CategorizedAction : ITestAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class PrivateConstructorAction : ITestAction
    {
        private PrivateConstructorAction() { }
    }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public abstract class AbstractAction : ITestAction { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class NonSerializableAction : ITestAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class OpenGenericAction<T> : ITestAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public struct StructAction : ITestAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class UnityObjectAction : ScriptableObject, ITestAction { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class ActorProducer : IProducer<IActor> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class NetworkActorProducer : IProducer<INetworkActor> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class ObjectProducer : IProducer<object> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class ActorConsumer : IConsumer<IActor> { }

    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public sealed class NetworkActorConsumer : IConsumer<INetworkActor> { }
}
