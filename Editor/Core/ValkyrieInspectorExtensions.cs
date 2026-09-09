using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Astra.Valkyrie.Editor
{
    /// <summary>Per-draw information handed to <see cref="IValkyrieInspectorExtension"/> callbacks.</summary>
    public readonly struct ValkyrieInspectorContext
    {
        public ValkyrieInspectorContext(SerializedObject serializedObject, UnityEngine.Object[] targets)
        {
            SerializedObject = serializedObject;
            Targets = targets ?? Array.Empty<UnityEngine.Object>();
        }

        public SerializedObject SerializedObject { get; }
        public UnityEngine.Object[] Targets { get; }
        public UnityEngine.Object Target => Targets.Length > 0 ? Targets[0] : null;
    }

    /// <summary>
    /// Composition point of the Valkyrie inspector. Extensions draw additional IMGUI content
    /// at the top of an inspector, right after each top-level field, and at the bottom, without
    /// replacing the inspector or registering a second global editor.
    /// </summary>
    public interface IValkyrieInspectorExtension
    {
        /// <summary>Lower values draw first.</summary>
        int Order { get; }
        void OnBeginInspector(ValkyrieInspectorContext context);
        void OnAfterField(ValkyrieInspectorContext context, SerializedProperty property, InspectedField field);
        void OnEndInspector(ValkyrieInspectorContext context);
    }

    /// <summary>
    /// Registry of <see cref="IValkyrieInspectorExtension"/> instances consulted by the Valkyrie
    /// renderers. A failing extension is logged once per session and skipped, so one integration
    /// cannot break the inspector. Registration returns a handle whose disposal unregisters.
    /// </summary>
    public static class ValkyrieInspectorExtensions
    {
        private static readonly List<IValkyrieInspectorExtension> Extensions = new List<IValkyrieInspectorExtension>();
        private static readonly HashSet<IValkyrieInspectorExtension> Faulted = new HashSet<IValkyrieInspectorExtension>();

        public static int Count => Extensions.Count;
        public static IReadOnlyList<IValkyrieInspectorExtension> Registered => Extensions;

        public static IDisposable Register(IValkyrieInspectorExtension extension)
        {
            if (extension == null) throw new ArgumentNullException(nameof(extension));
            if (!Extensions.Contains(extension))
            {
                Extensions.Add(extension);
                Extensions.Sort((a, b) => a.Order.CompareTo(b.Order));
            }
            Faulted.Remove(extension);
            return new Registration(extension);
        }

        public static bool Unregister(IValkyrieInspectorExtension extension)
        {
            Faulted.Remove(extension);
            return extension != null && Extensions.Remove(extension);
        }

        public static bool IsRegistered(IValkyrieInspectorExtension extension) => extension != null && Extensions.Contains(extension);

        public static void Clear()
        {
            Extensions.Clear();
            Faulted.Clear();
        }

        /// <summary>Called by Valkyrie renderers before the first field.</summary>
        public static void NotifyBegin(ValkyrieInspectorContext context)
        {
            for (int i = 0; i < Extensions.Count; i++)
                Invoke(Extensions[i], context, (e, c) => e.OnBeginInspector(c));
        }

        /// <summary>Called by Valkyrie renderers after each top-level field.</summary>
        public static void NotifyAfterField(ValkyrieInspectorContext context, SerializedProperty property, InspectedField field)
        {
            if (Extensions.Count == 0) return;
            for (int i = 0; i < Extensions.Count; i++)
            {
                IValkyrieInspectorExtension extension = Extensions[i];
                if (Faulted.Contains(extension)) continue;
                try { extension.OnAfterField(context, property, field); }
                catch (Exception exception) { Fault(extension, exception); }
            }
        }

        /// <summary>Called by Valkyrie renderers after the last field and buttons.</summary>
        public static void NotifyEnd(ValkyrieInspectorContext context)
        {
            for (int i = 0; i < Extensions.Count; i++)
                Invoke(Extensions[i], context, (e, c) => e.OnEndInspector(c));
        }

        private delegate void Callback(IValkyrieInspectorExtension extension, ValkyrieInspectorContext context);

        private static void Invoke(IValkyrieInspectorExtension extension, ValkyrieInspectorContext context, Callback callback)
        {
            if (Faulted.Contains(extension)) return;
            try { callback(extension, context); }
            catch (ExitGUIException) { throw; }
            catch (Exception exception) { Fault(extension, exception); }
        }

        private static void Fault(IValkyrieInspectorExtension extension, Exception exception)
        {
            if (!Faulted.Add(extension)) return;
            Debug.LogError("[Valkyrie] Inspector extension '" + extension.GetType().FullName + "' threw and is disabled until re-registered.\n" + exception);
        }

        private sealed class Registration : IDisposable
        {
            private IValkyrieInspectorExtension _extension;
            public Registration(IValkyrieInspectorExtension extension) { _extension = extension; }
            public void Dispose() { if (_extension != null) { Unregister(_extension); _extension = null; } }
        }
    }
}
