using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Valkyrie.Editor
{
    /// <summary>
    /// Native Unity searchable dropdown listing every concrete type compatible with
    /// a polymorphic <c>[SerializeReference]</c> slot. Used both by single managed
    /// reference fields and by the "+ Add" button on managed reference collections.
    ///
    /// <para>Items are grouped by namespace (slashes become hierarchy levels), so
    /// large type lists stay navigable. A "None" entry at the top clears the slot.</para>
    /// </summary>
    public sealed class ManagedReferenceTypeDropdown : AdvancedDropdown
    {
        private readonly Type _baseType;
        private readonly Action<Type> _onSelected;
        private readonly bool _includeNoneEntry;
        private readonly string _title;

        public ManagedReferenceTypeDropdown(
            Type baseType,
            Action<Type> onSelected,
            bool includeNoneEntry,
            string title,
            AdvancedDropdownState state)
            : base(state)
        {
            _baseType = baseType;
            _onSelected = onSelected;
            _includeNoneEntry = includeNoneEntry;
            _title = title;

            // Make the popup tall enough to feel like Odin's: ~14 rows visible.
            minimumSize = new Vector2(280f, 360f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_title);
            var byPath = new System.Collections.Generic.Dictionary<string, AdvancedDropdownItem>();

            if (_includeNoneEntry)
            {
                root.AddChild(new TypeItem("None", null));
                root.AddSeparator();
            }

            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(_baseType);
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                AdvancedDropdownItem parent = ResolveParent(root, byPath, type.Namespace);
                parent.AddChild(new TypeItem(ObjectNames.NicifyVariableName(type.Name), type));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeItem typed)
                _onSelected?.Invoke(typed.Type);
        }

        private static AdvancedDropdownItem ResolveParent(
            AdvancedDropdownItem root,
            System.Collections.Generic.Dictionary<string, AdvancedDropdownItem> byPath,
            string ns)
        {
            if (string.IsNullOrEmpty(ns)) return root;

            string[] parts = ns.Split('.');
            string currentPath = "";
            AdvancedDropdownItem currentParent = root;

            for (int i = 0; i < parts.Length; i++)
            {
                currentPath = currentPath.Length == 0 ? parts[i] : currentPath + "." + parts[i];
                if (!byPath.TryGetValue(currentPath, out AdvancedDropdownItem next))
                {
                    next = new AdvancedDropdownItem(parts[i]);
                    currentParent.AddChild(next);
                    byPath[currentPath] = next;
                }
                currentParent = next;
            }

            return currentParent;
        }

        private sealed class TypeItem : AdvancedDropdownItem
        {
            public Type Type { get; }
            public TypeItem(string name, Type type) : base(name) { Type = type; }
        }

        /// <summary>
        /// Convenience helper: opens the dropdown anchored under <paramref name="rect"/>.
        /// Each call uses a fresh <see cref="AdvancedDropdownState"/> so independent
        /// pickers don't share the same scroll / search state.
        /// </summary>
        public static void Show(Rect rect, Type baseType, Action<Type> onSelected, bool includeNoneEntry, string title)
        {
            var dropdown = new ManagedReferenceTypeDropdown(
                baseType, onSelected, includeNoneEntry, title, new AdvancedDropdownState());
            dropdown.Show(rect);
        }
    }
}
