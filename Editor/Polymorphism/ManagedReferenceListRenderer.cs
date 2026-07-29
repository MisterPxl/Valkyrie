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

            if (_cache.TryGetValue(key, out CacheEntry entry) && IsEntryValid(entry, listProperty))
            {
                return entry.List;
            }

            ReorderableList list = BuildList(listProperty, baseType);
            _cache[key] = new CacheEntry
            {
                List = list,
                SerializedObjectRef = new WeakReference<SerializedObject>(listProperty.serializedObject),
                PropertyPath = listProperty.propertyPath
            };
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

        private static string BuildCacheKey(SerializedProperty property, Type baseType)
        {
            // Stable per (target instance + property path). Wrap the target lookup
            // in a try/catch in case the underlying object has been destroyed since
            // the property was created.
            try
            {
                UnityEngine.Object[] targets = property.serializedObject.targetObjects;
                int hash = 17;
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null) hash = hash * 31 + GetStableObjectKey(targets[i]);
                }
                return hash + "::" + property.propertyPath + "::" + (baseType != null ? baseType.AssemblyQualifiedName : "");
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        private static int GetStableObjectKey(UnityEngine.Object target)
        {
#if UNITY_6000_3_OR_NEWER
            return target.GetEntityId().GetHashCode();
#else
            return target.GetInstanceID();
#endif
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
                DrawElementInRect(rect, element, baseType, $"Element {index}");
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
