using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Valkyrie.Editor;

namespace Valkyrie.Tests.Editor
{
    public sealed class ManagedReferenceTypeCacheTests
    {
        [SetUp]
        public void SetUp()
        {
            ManagedReferenceTypeCache.Clear();
        }

        [Test]
        public void ConcreteBaseType_IsIncludedWithDerivedTypes()
        {
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(ConcreteBaseAction));

            Assert.That(types, Does.Contain(typeof(ConcreteBaseAction)));
            Assert.That(types, Does.Contain(typeof(ConcreteDerivedAction)));
        }

        [Test]
        public void InvalidManagedReferenceTypes_AreExcluded()
        {
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(ITestAction));

            Assert.That(types, Does.Contain(typeof(PreserveSourceAction)));
            Assert.That(types, Does.Contain(typeof(PrivateConstructorAction)));
            Assert.That(types, !Does.Contain(typeof(AbstractAction)));
            Assert.That(types, !Does.Contain(typeof(NonSerializableAction)));
            Assert.That(types, !Does.Contain(typeof(OpenGenericAction<>)));
            Assert.That(types, !Does.Contain(typeof(UnityObjectAction)));
            Assert.That(types.Length, Is.EqualTo(types.Distinct().Count()));
        }

        [Test]
        public void GenericCovariance_IsSupported()
        {
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(IProducer<IActor>));

            Assert.That(types, Does.Contain(typeof(ActorProducer)));
            Assert.That(types, Does.Contain(typeof(NetworkActorProducer)));
            Assert.That(types, !Does.Contain(typeof(ObjectProducer)));
        }

        [Test]
        public void GenericContravariance_IsSupported()
        {
            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(typeof(IConsumer<INetworkActor>));

            Assert.That(types, Does.Contain(typeof(NetworkActorConsumer)));
            Assert.That(types, Does.Contain(typeof(ActorConsumer)));
        }

        [Test]
        public void ManagedReferenceCategoryAttribute_CustomizesDropdownMetadata()
        {
            Type typeMenuItemType = typeof(ManagedReferenceTypeDropdown).GetNestedType(
                "TypeMenuItem",
                BindingFlags.NonPublic);
            MethodInfo fromTypeMethod = typeMenuItemType.GetMethod(
                "FromType",
                BindingFlags.Public | BindingFlags.Static);

            object menuItem = fromTypeMethod.Invoke(null, new object[] { typeof(CategorizedAction) });

            Assert.That(GetProperty<string>(menuItem, "Path"), Is.EqualTo("Gameplay/Actions"));
            Assert.That(GetProperty<string>(menuItem, "Label"), Is.EqualTo("Categorized Action"));
            Assert.That(GetProperty<int>(menuItem, "Order"), Is.EqualTo(-10));
            Assert.That(GetProperty<bool>(menuItem, "UsesCategoryPath"), Is.True);
        }

        private static T GetProperty<T>(object instance, string propertyName)
        {
            PropertyInfo property = instance.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance);
            return (T)property.GetValue(instance);
        }
    }
}
