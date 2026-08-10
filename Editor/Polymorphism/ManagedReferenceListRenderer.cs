using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Valkyrie.Editor
{
    /// <summary>
    /// Renderer for <c>[SerializeReference]</c> collections (<c>List&lt;T&gt;</c> or <c>T[]</c>).
    /// Built on top of <see cref="ReorderableList"/> so reordering uses the native
    /// drag handle that ships with Unity (consistent UX with everything else in the editor).
    ///
    /// <para>Layout matches Odin:</para>
    /// <list type="bullet">
    ///   <item>Foldout-style header with item count (or "Empty") and a "+" button on the right.</item>
    ///   <item>Clicking "+" opens a searchable type picker
    ///         (<see cref="ManagedReferenceTypeDropdown"/>).</item>
    ///   <item>Each element renders through <see cref="ManagedReferenceRenderer.DrawElement"/>
    ///         with its own foldout, type picker and child properties.</item>
    /// </list>
    /// </summary>
    public static class ManagedReferenceListRenderer
    {
        // ReorderableLists are stateful and must be reused across draw calls for a given
        // SerializedProperty path, otherwise reorder/select state is lost on every repaint.
        // We keep a weak reference to the SerializedObject so we can detect when it has
        // been disposed (inspector switch, recompile, asset reselect…) and rebuild safely.
        private sealed class CacheEntry
        {
            public ReorderableList List;
            public WeakReference<SerializedObject> SerializedObjectRef;
            public string PropertyPath;
        }

        private static readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private static readonly Dictionary<Type, string> _typeKeyCache = new Dictionary<Type, string>();
        private static readonly List<string> _elementLabels = new List<string>();

        public static void Draw(SerializedProperty listProperty, InspectedField field)
        {
            Draw(listProperty, field.ManagedReferenceBaseType);
        }

        public static void Draw(SerializedProperty listProperty, Type baseType)
        {
            if (listProperty == null) return;
            if (!listProperty.isArray)
            {
                EditorGUILayout.PropertyField(listProperty, true);
                return;
            }

            ReorderableList list = GetOrBuildList(listProperty, baseType);
            list.DoLayoutList();
        }

        public static float GetHeight(SerializedProperty listProperty, Type baseType)
        {
            if (listProperty == null || !listProperty.isArray)
                return EditorGUI.GetPropertyHeight(listProperty, true);

            return GetOrBuildList(listProperty, baseType).GetHeight();
        }

        public static void Draw(Rect rect, SerializedProperty listProperty, Type baseType)
        {
            if (listProperty == null) return;
            if (!listProperty.isArray)
            {
                EditorGUI.PropertyField(rect, listProperty, true);
                return;
            }

            GetOrBuildList(listProperty, baseType).DoList(rect);
        }

        private static ReorderableList GetOrBuildList(SerializedProperty listProperty, Type baseType)
        {
            string key = BuildCacheKey(listProperty, baseType);

            if (key != null && _cache.TryGetValue(key, out CacheEntry entry) && IsEntryValid(entry, listProperty))
            {
                return entry.List;
            }

            // Build against a stable copy: `listProperty` may be a shared iterator that
            // advances after this call, while the ReorderableList and its callbacks keep
            // referencing the property across frames and deferred events.
            SerializedProperty stableProperty = listProperty.Copy();
            ReorderableList list = BuildList(stableProperty, baseType);

            if (key != null)
            {
                _cache[key] = new CacheEntry
                {
                    List = list,
                    SerializedObjectRef = new WeakReference<SerializedObject>(stableProperty.serializedObject),
                    PropertyPath = stableProperty.propertyPath
                };
            }

            return list;
        }

        private static bool IsEntryValid(CacheEntry entry, SerializedProperty listProperty)
        {
            if (entry == null || entry.List == null) return false;

            // Did the underlying SerializedObject change identity (different inspector, reselect)?
            if (!entry.SerializedObjectRef.TryGetTarget(out SerializedObject cachedSO)) return false;
            if (!ReferenceEquals(cachedSO, listProperty.serializedObject)) return false;

            // The cached property may reference a disposed native object (e.g. after a
            // domain reload or an inspector switch that left the cache stale). Verify
            // safely — Unity has no public IsValid API, so try/catch is the only option.
            try
            {
                if (entry.PropertyPath != listProperty.propertyPath) return false;
                return SerializedProperty.EqualContents(entry.List.serializedProperty, listProperty);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Stable key per (target instances + property path + base type), or
        /// <c>null</c> when the underlying object has been destroyed — in which
        /// case the caller builds a throwaway list without polluting the cache.
        /// </summary>
        private static string BuildCacheKey(SerializedProperty property, Type baseType)
        {
            try
            {
                UnityEngine.Object[] targets = property.serializedObject.targetObjects;
                string ids = string.Empty;
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null) ids += GetStableObjectKey(targets[i]) + "|";
                }
                return ids + "::" + property.propertyPath + "::" + GetTypeKey(baseType);
            }
            catch
            {
                return null;
            }
        }

        private static string GetTypeKey(Type baseType)
        {
            if (baseType == null)
                return string.Empty;

            if (!_typeKeyCache.TryGetValue(baseType, out string key))
            {
                key = baseType.AssemblyQualifiedName;
                _typeKeyCache[baseType] = key;
            }
            return key;
        }

        private static string GetStableObjectKey(UnityEngine.Object target)
        {
#if UNITY_6000_3_OR_NEWER
            // Full EntityId (not its 32-bit hash) so distinct objects cannot collide.
            return target.GetEntityId().ToString();
#else
            return target.GetInstanceID().ToString();
#endif
        }

        private static string GetElementLabel(int index)
        {
            while (_elementLabels.Count <= index)
                _elementLabels.Add("Element " + _elementLabels.Count);
            return _elementLabels[index];
        }

        private static ReorderableList BuildList(SerializedProperty listProperty, Type baseType)
        {
            string headerLabel = listProperty.displayName;

            var list = new ReorderableList(
                listProperty.serializedObject, listProperty,
                draggable: true, displayHeader: true,
                displayAddButton: true, displayRemoveButton: true);

            list.headerHeight = EditorGUIUtility.singleLineHeight + 2f;

            list.drawHeaderCallback = rect =>
            {
                Rect labelRect = new Rect(rect.x, rect.y, rect.width - 60f, rect.height);
                Rect countRect = new Rect(rect.xMax - 60f, rect.y, 60f, rect.height);
                EditorGUI.LabelField(labelRect, headerLabel, EditorStyles.boldLabel);
                string countLabel = listProperty.arraySize == 0 ? "Empty" : $"{listProperty.arraySize} items";
                EditorGUI.LabelField(countRect, countLabel, EditorStyles.miniLabel);
            };

            list.elementHeightCallback = index =>
            {
                if (index < 0 || index >= listProperty.arraySize) return EditorGUIUtility.singleLineHeight;
                SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
                return ComputeElementHeight(element, baseType);
            };

            list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                if (index < 0 || index >= listProperty.arraySize) return;
                SerializedProperty element = listProperty.GetArrayElementAtIndex(index);
                DrawElementInRect(rect, element, baseType, GetElementLabel(index));
            };

            list.onAddDropdownCallback = (rect, _) =>
            {
                ManagedReferenceTypeDropdown.Show(
                    rect,
                    baseType,
                    type => ManagedReferenceMutationService.AppendInstance(
                        listProperty.serializedObject,
                        listProperty.propertyPath,
                        type),
                    includeNoneEntry: false,
                    title: "Add " + (baseType != null ? ObjectNames.NicifyVariableName(baseType.Name) : "Item"));
            };

            list.onRemoveCallback = rl =>
            {
                ManagedReferenceMutationService.RemoveAt(
                    listProperty.serializedObject,
                    listProperty.propertyPath,
                    rl.index);
            };

            return list;
        }

        // ── Element drawing ──────────────────────────────────────────────────
        // Re-implements ManagedReferenceRenderer.DrawElement against a fixed Rect
        // (instead of EditorGUILayout) so it integrates cleanly with ReorderableList.

        private static float ComputeElementHeight(SerializedProperty element, Type baseType)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (element.propertyType != SerializedPropertyType.ManagedReference)
            {
                return ManagedReferencePropertyRouter.GetPropertyHeight(element) + spacing;
            }

            return ManagedReferenceRenderer.GetElementHeight(element, baseType) + spacing + 4f;
        }

        private static void DrawElementInRect(Rect rect, SerializedProperty element, Type baseType, string fallbackLabel)
        {
            Rect adjustedRect = new Rect(rect.x, rect.y + 2f, rect.width, rect.height - 2f);
            ManagedReferenceRenderer.DrawElement(adjustedRect, element, baseType, fallbackLabel, labelWidthAdjustment: 20f);
        }

    }
}
