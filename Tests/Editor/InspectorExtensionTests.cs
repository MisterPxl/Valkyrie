using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Astra.Valkyrie.Editor;

namespace Astra.Valkyrie.Tests.EditMode
{
    public sealed class InspectorExtensionTests
    {
        private ScriptableObject _target;

        [SetUp] public void SetUp() { ValkyrieInspectorExtensions.Clear(); _target = ScriptableObject.CreateInstance<FallbackEditorTestObject>(); }
        [TearDown] public void TearDown() { ValkyrieInspectorExtensions.Clear(); if (_target != null) UnityEngine.Object.DestroyImmediate(_target); }

        [Test]
        public void RegistrationOrdersByOrderAndHandleUnregisters()
        {
            var late = new Recorder(10); var early = new Recorder(-5);
            using (ValkyrieInspectorExtensions.Register(late))
            using (ValkyrieInspectorExtensions.Register(early))
            {
                ValkyrieInspectorExtensions.Register(late);
                Assert.That(ValkyrieInspectorExtensions.Count, Is.EqualTo(2));
                Assert.That(ValkyrieInspectorExtensions.Registered[0], Is.SameAs(early));
                using (var so = new SerializedObject(_target))
                {
                    var context = new ValkyrieInspectorContext(so, new UnityEngine.Object[] { _target });
                    ValkyrieInspectorExtensions.NotifyBegin(context);
                    ValkyrieInspectorExtensions.NotifyAfterField(context, so.FindProperty("m_Script"), null);
                    ValkyrieInspectorExtensions.NotifyEnd(context);
                }
                Assert.That(early.Calls, Is.EqualTo(new[] { "begin", "field:m_Script", "end" }));
                Assert.That(late.Calls, Is.EqualTo(new[] { "begin", "field:m_Script", "end" }));
                Assert.That(early.LastTarget, Is.SameAs(_target));
            }
            Assert.That(ValkyrieInspectorExtensions.Count, Is.Zero);
            Assert.That(ValkyrieInspectorExtensions.IsRegistered(early), Is.False);
        }

        [Test]
        public void FailingExtensionIsLoggedOnceAndSkippedWithoutBlockingOthers()
        {
            var healthy = new Recorder(0); var failing = new Throwing();
            ValkyrieInspectorExtensions.Register(failing); ValkyrieInspectorExtensions.Register(healthy);
            using (var so = new SerializedObject(_target))
            {
                var context = new ValkyrieInspectorContext(so, new UnityEngine.Object[] { _target });
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Throwing' threw and is disabled"));
                ValkyrieInspectorExtensions.NotifyBegin(context);
                ValkyrieInspectorExtensions.NotifyBegin(context);
                ValkyrieInspectorExtensions.NotifyEnd(context);
            }
            Assert.That(failing.Attempts, Is.EqualTo(1), "a faulted extension is not called again");
            Assert.That(healthy.Calls, Is.EqualTo(new[] { "begin", "begin", "end" }));
            ValkyrieInspectorExtensions.Register(failing);
            using (var so = new SerializedObject(_target))
            {
                LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Throwing' threw and is disabled"));
                ValkyrieInspectorExtensions.NotifyBegin(new ValkyrieInspectorContext(so, new UnityEngine.Object[] { _target }));
            }
            Assert.That(failing.Attempts, Is.EqualTo(2), "re-registration re-enables the extension");
        }

        private sealed class Recorder : IValkyrieInspectorExtension
        {
            public readonly List<string> Calls = new List<string>();
            public UnityEngine.Object LastTarget;
            public Recorder(int order) { Order = order; }
            public int Order { get; }
            public void OnBeginInspector(ValkyrieInspectorContext context) { Calls.Add("begin"); LastTarget = context.Target; }
            public void OnAfterField(ValkyrieInspectorContext context, SerializedProperty property, InspectedField field) => Calls.Add("field:" + property.propertyPath);
            public void OnEndInspector(ValkyrieInspectorContext context) => Calls.Add("end");
        }

        private sealed class Throwing : IValkyrieInspectorExtension
        {
            public int Attempts;
            public int Order => 0;
            public void OnBeginInspector(ValkyrieInspectorContext context) { Attempts++; throw new InvalidOperationException("boom"); }
            public void OnAfterField(ValkyrieInspectorContext context, SerializedProperty property, InspectedField field) { }
            public void OnEndInspector(ValkyrieInspectorContext context) { }
        }
    }
}
