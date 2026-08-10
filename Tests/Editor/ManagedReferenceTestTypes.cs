using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Tests.Editor
{
    public interface ITestAction { }

    public interface IActor { }

    public interface INetworkActor : IActor { }

    public interface IProducer<out T> { }

    public interface IConsumer<in T> { }

    [Serializable]
    public class ManagedReferenceTestObject : ScriptableObject
    {
        [SerializeReference]
        public ITestAction action;

        [SerializeReference]
        public List<ITestAction> actions = new List<ITestAction>();

        public NestedManagedReferenceHolder holder = new NestedManagedReferenceHolder();
    }

    [Serializable]
    public class NestedManagedReferenceHolder
    {
        [SerializeReference]
        public ITestAction nestedAction;

        [SerializeReference]
        public List<ITestAction> nestedActions = new List<ITestAction>();
    }

    [Serializable]
    public class ConcreteBaseAction : ITestAction
    {
        public int value;
    }

    [Serializable]
    public sealed class ConcreteDerivedAction : ConcreteBaseAction { }

    [Serializable]
    public sealed class PreserveSourceAction : ITestAction
    {
        public int sharedValue;
        public string sharedName;
    }

    [Serializable]
    public sealed class PreserveTargetAction : ITestAction
    {
        public int sharedValue;
        public string sharedName;
        public int targetOnlyValue = 7;
    }

    [Serializable]
    [ManagedReferenceCategory("Gameplay/Actions", "Categorized Action", -10)]
    public sealed class CategorizedAction : ITestAction { }

    [Serializable]
    public sealed class PrivateConstructorAction : ITestAction
    {
        private PrivateConstructorAction() { }
    }

    [Serializable]
    public abstract class AbstractAction : ITestAction { }

    public sealed class NonSerializableAction : ITestAction { }

    [Serializable]
    public sealed class OpenGenericAction<T> : ITestAction { }

    [Serializable]
    public sealed class UnityObjectAction : ScriptableObject, ITestAction { }

    [Serializable]
    public sealed class ActorProducer : IProducer<IActor> { }

    [Serializable]
    public sealed class NetworkActorProducer : IProducer<INetworkActor> { }

    [Serializable]
    public sealed class ObjectProducer : IProducer<object> { }

    [Serializable]
    public sealed class ActorConsumer : IConsumer<IActor> { }

    [Serializable]
    public sealed class NetworkActorConsumer : IConsumer<INetworkActor> { }
}
