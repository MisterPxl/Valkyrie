using System;
using System.Collections.Generic;
using System.Linq;
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
            Dictionary<string, AdvancedDropdownItem> byPath = new Dictionary<string, AdvancedDropdownItem>();

            if (_includeNoneEntry)
            {
                root.AddChild(new TypeItem("None", null));
                root.AddSeparator();
            }

            Type[] types = ManagedReferenceTypeCache.GetCompatibleTypes(_baseType);
            TypeMenuItem[] menuItems = types
                .Select(TypeMenuItem.FromType)
                .OrderBy(item => item.Order)
                .ThenBy(item => item.Path, StringComparer.Ordinal)
                .ThenBy(item => item.Label, StringComparer.Ordinal)
                .ToArray();

            for (int i = 0; i < menuItems.Length; i++)
            {
                TypeMenuItem item = menuItems[i];
                AdvancedDropdownItem parent = ResolveParent(root, byPath, item.Path, item.UsesCategoryPath);
                parent.AddChild(new TypeItem(item.Label, item.Type));
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
            Dictionary<string, AdvancedDropdownItem> byPath,
            string path,
            bool slashSeparated)
        {
            if (string.IsNullOrEmpty(path)) return root;

            char separator = slashSeparated ? '/' : '.';
            string[] parts = path.Split(separator);
            string currentPath = "";
            AdvancedDropdownItem currentParent = root;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (part.Length == 0) continue;

                currentPath = currentPath.Length == 0 ? part : currentPath + "/" + part;
                if (!byPath.TryGetValue(currentPath, out AdvancedDropdownItem next))
                {
                    next = new AdvancedDropdownItem(part);
                    currentParent.AddChild(next);
                    byPath[currentPath] = next;
                }
                currentParent = next;
            }

            return currentParent;
        }

        private readonly struct TypeMenuItem
        {
            public Type Type { get; }
            public string Path { get; }
            public string Label { get; }
            public int Order { get; }
            public bool UsesCategoryPath { get; }

            private TypeMenuItem(Type type, string path, string label, int order, bool usesCategoryPath)
            {
                Type = type;
                Path = path;
                Label = label;
                Order = order;
                UsesCategoryPath = usesCategoryPath;
            }

            public static TypeMenuItem FromType(Type type)
            {
                ManagedReferenceCategoryAttribute category = Attribute.GetCustomAttribute(
                    type,
                    typeof(ManagedReferenceCategoryAttribute),
                    inherit: false) as ManagedReferenceCategoryAttribute;

                if (category != null)
                {
                    string label = string.IsNullOrWhiteSpace(category.Label)
                        ? ObjectNames.NicifyVariableName(type.Name)
                        : category.Label.Trim();
                    return new TypeMenuItem(type, category.Path ?? string.Empty, label, category.Order, true);
                }

                return new TypeMenuItem(
                    type,
                    type.Namespace ?? string.Empty,
                    ObjectNames.NicifyVariableName(type.Name),
                    0,
                    false);
            }
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
