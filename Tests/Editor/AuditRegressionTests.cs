using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using Astra.Valkyrie.Editor;
using Object = UnityEngine.Object;

namespace Astra.Valkyrie.Tests.EditMode
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public interface IAction { }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    [Serializable] public class ActionA : IAction { public int value; }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    [Serializable] public class ActionB : IAction { public string text; }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class Holder : ScriptableObject { [SerializeReference] public IAction action; }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    [Serializable] public class IntProducer : IProducer<int> { }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    [Serializable] public class ObjectConsumer : IConsumer<object> { }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class BaseConditions
    {
        private bool visible = false;
        private bool VisibleProperty => visible;
        private bool VisibleMethod() => visible;
        [ShowIf("visible")] public int detail;
        [ShowIf("VisibleProperty")] public int propertyDetail;
        [ShowIf("VisibleMethod")] public int methodDetail;
    }
    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class DerivedConditions : BaseConditions { }

    [UnityEngine.Scripting.APIUpdating.MovedFrom(false, "Valkyrie.Tests.Editor", "Valkyrie.Tests.Editor")]
    public class AuditRegressionTests
    {
        [TestCase("detail")]
        [TestCase("propertyDetail")]
        [TestCase("methodDetail")]
        public void InheritedPrivateConditionShouldBeResolved(string fieldName)
        {
            var f = TypeData.Build(typeof(DerivedConditions)).GetField(fieldName);
            bool draw = ConditionResolver.ShouldDraw(new DerivedConditions(), f, out string warning);
            Assert.That(draw, Is.False, warning);
        }
        [Test] public void ValueTypeCovarianceShouldBeExcluded()
        {
            Assert.That(typeof(IProducer<object>).IsAssignableFrom(typeof(IntProducer)), Is.False);
            Assert.That(ManagedReferenceTypeCache.GetCompatibleTypes(typeof(IProducer<object>)), Has.No.Member(typeof(IntProducer)));
            Assert.That(ManagedReferenceTypeCache.GetCompatibleTypes(typeof(IProducer<int>)), Has.Member(typeof(IntProducer)));
        }
        [Test] public void ValueTypeContravarianceShouldBeExcluded()
        {
            Assert.That(ManagedReferenceTypeCache.GetCompatibleTypes(typeof(IConsumer<int>)), Has.No.Member(typeof(ObjectConsumer)));
        }
        [Test] public void ResetShouldPreserveEachSelectedObjectsType()
        {
            var a = ScriptableObject.CreateInstance<Holder>();
            var b = ScriptableObject.CreateInstance<Holder>();
            var empty = ScriptableObject.CreateInstance<Holder>();
            try
            {
                var oldA = new ActionA { value = 99 };
                var oldB = new ActionB { text = "old" };
                a.action = oldA; b.action = oldB;
                using (var so = new SerializedObject(new Object[] { a, b, empty }))
                    ManagedReferenceMutationService.ResetCurrentType(so, "action");
                Assert.That(a.action, Is.TypeOf<ActionA>());
                Assert.That(b.action, Is.TypeOf<ActionB>());
                Assert.That(a.action, Is.Not.SameAs(oldA));
                Assert.That(b.action, Is.Not.SameAs(oldB));
                Assert.That(((ActionA)a.action).value, Is.Zero);
                Assert.That(((ActionB)b.action).text, Is.Null.Or.Empty);
                Assert.That(empty.action, Is.Null);
            }
            finally { Object.DestroyImmediate(a); Object.DestroyImmediate(b); Object.DestroyImmediate(empty); }
        }
    }
}
