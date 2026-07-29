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
            if (listProperty == null) return;
            if (!listProperty.isArray)
            {
                EditorGUILayout.PropertyField(listProperty, true);
                return;
            }

            ReorderableList list = GetOrBuildList(listProperty, field);
            list.DoLayoutList();
        }

        private static ReorderableList GetOrBuildList(SerializedProperty listProperty, InspectedField field)
        {
            string key = BuildCacheKey(listProperty);

            if (_cache.TryGetValue(key, out CacheEntry entry) && IsEntryValid(entry, listProperty))
            {
                return entry.List;
            }

            ReorderableList list = BuildList(listProperty, field);
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

        private static string BuildCacheKey(SerializedProperty property)
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
                return hash + "::" + property.propertyPath;
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

        private static ReorderableList BuildList(SerializedProperty listProperty, InspectedField field)
        {
            Type baseType = field.ManagedReferenceBaseType;
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
                return ComputeElementHeight(element);
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
                    type => AppendInstance(listProperty, type),
                    includeNoneEntry: false,
                    title: "Add " + (baseType != null ? ObjectNames.NicifyVariableName(baseType.Name) : "Item"));
            };

            list.onRemoveCallback = rl =>
            {
                int idx = rl.index;
                if (idx < 0 || idx >= listProperty.arraySize) idx = listProperty.arraySize - 1;
                if (idx < 0) return;
                listProperty.DeleteArrayElementAtIndex(idx);
                listProperty.serializedObject.ApplyModifiedProperties();
            };

            return list;
        }

        // ── Element drawing ──────────────────────────────────────────────────
        // Re-implements ManagedReferenceRenderer.DrawElement against a fixed Rect
        // (instead of EditorGUILayout) so it integrates cleanly with ReorderableList.

        private static float ComputeElementHeight(SerializedProperty element)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            if (element.propertyType != SerializedPropertyType.ManagedReference)
            {
                return EditorGUI.GetPropertyHeight(element, true) + spacing;
            }

            float height = lineHeight + spacing;
            bool hasValue = !string.IsNullOrEmpty(element.managedReferenceFullTypename);
            if (!hasValue || !element.isExpanded) return height + 4f;

            var iterator = element.Copy();
            var endProperty = iterator.GetEndProperty();
            if (!iterator.NextVisible(true)) return height + 4f;

            while (!SerializedProperty.EqualContents(iterator, endProperty))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + spacing;
                if (!iterator.NextVisible(false)) break;
            }

            return height + 4f;
        }

        private static void DrawElementInRect(Rect rect, SerializedProperty element, Type baseType, string fallbackLabel)
        {
            if (element.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(rect, element, true);
                return;
            }

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect headerRect = new Rect(rect.x, rect.y + 2f, rect.width, lineHeight);
            DrawElementHeader(headerRect, element, baseType, fallbackLabel);

            bool hasValue = !string.IsNullOrEmpty(element.managedReferenceFullTypename);
            if (!hasValue || !element.isExpanded) return;

            float y = headerRect.yMax + spacing;
            EditorGUI.indentLevel++;

            var iterator = element.Copy();
            var endProperty = iterator.GetEndProperty();
            if (iterator.NextVisible(true))
            {
                while (!SerializedProperty.EqualContents(iterator, endProperty))
                {
                    float h = EditorGUI.GetPropertyHeight(iterator, true);
                    Rect childRect = new Rect(rect.x, y, rect.width, h);
                    EditorGUI.PropertyField(childRect, iterator, true);
                    y += h + spacing;
                    if (!iterator.NextVisible(false)) break;
                }
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawElementHeader(Rect rect, SerializedProperty element, Type baseType, string fallbackLabel)
        {
            bool hasValue = !string.IsNullOrEmpty(element.managedReferenceFullTypename);
            float labelWidth = EditorGUIUtility.labelWidth - 20f; // compensate ReorderableList drag handle

            Rect labelRect = new Rect(rect.x, rect.y, labelWidth, rect.height);
            Rect dropdownRect = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height);

            string elementLabel = hasValue
                ? FormatValueLabel(element)
                : fallbackLabel;

            if (hasValue)
            {
                element.isExpanded = EditorGUI.Foldout(labelRect, element.isExpanded, elementLabel, true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, elementLabel);
            }

            string dropdownText = hasValue
                ? FormatValueLabel(element)
                : $"None ({FormatBaseType(baseType)})";

            GUIContent content = new GUIContent(
                dropdownText,
                hasValue ? EditorGUIUtility.IconContent("cs Script Icon").image : null);

            if (EditorGUI.DropdownButton(dropdownRect, content, FocusType.Keyboard, EditorStyles.objectField))
            {
                SerializedObject serializedObject = element.serializedObject;
                string propertyPath = element.propertyPath;

                ManagedReferenceTypeDropdown.Show(
                    dropdownRect,
                    baseType,
                    type => AssignType(serializedObject, propertyPath, type),
                    includeNoneEntry: hasValue,
                    title: "Select " + FormatBaseType(baseType));
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static void AppendInstance(SerializedProperty listProperty, Type type)
        {
            if (listProperty == null || !listProperty.isArray) return;
            if (type == null) return;

            int newIndex = listProperty.arraySize;
            listProperty.arraySize = newIndex + 1;

            SerializedProperty element = listProperty.GetArrayElementAtIndex(newIndex);
            try
            {
                element.managedReferenceValue = Activator.CreateInstance(type);
                element.isExpanded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Valkyrie: failed to create instance of {type.Name} — {e.Message}");
                listProperty.arraySize = newIndex;
            }

            listProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void AssignType(SerializedObject serializedObject, string propertyPath, Type type)
        {
            var prop = serializedObject.FindProperty(propertyPath);
            if (prop == null) return;

            if (type == null)
            {
                prop.managedReferenceValue = null;
            }
            else
            {
                try
                {
                    prop.managedReferenceValue = Activator.CreateInstance(type);
                    prop.isExpanded = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Valkyrie: failed to create instance of {type.Name} — {e.Message}");
                    return;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private static string FormatValueLabel(SerializedProperty property)
        {
            string fullTypename = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(fullTypename)) return "None";

            int spaceIdx = fullTypename.IndexOf(' ');
            string typeFullName = spaceIdx >= 0 ? fullTypename.Substring(spaceIdx + 1) : fullTypename;
            int lastDot = typeFullName.LastIndexOf('.');
            string shortName = lastDot >= 0 ? typeFullName.Substring(lastDot + 1) : typeFullName;
            return ObjectNames.NicifyVariableName(shortName);
        }

        private static string FormatBaseType(Type baseType)
        {
            if (baseType == null) return "Object";
            return ObjectNames.NicifyVariableName(baseType.Name);
        }
    }
}
