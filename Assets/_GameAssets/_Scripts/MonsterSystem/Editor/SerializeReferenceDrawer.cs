using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Generic property drawer for [SerializeReference] fields.
    /// Renders a type-selection dropdown followed by the selected type's serialized fields.
    /// Reusable across DestinationStrategy, PointSelector, and any future [SerializeReference] types.
    /// </summary>
    public static class SerializeReferenceDrawerHelper
    {
        private static readonly Dictionary<Type, (Type[] types, string[] names)> TypeCaches = new();

        public static (Type[] types, string[] names) GetCachedTypes(Type baseType)
        {
            if (TypeCaches.TryGetValue(baseType, out var cached))
                return cached;

            var types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();

            var names = new string[types.Length + 1];
            names[0] = "(None)";
            for (int i = 0; i < types.Length; i++)
                names[i + 1] = ObjectNames.NicifyVariableName(types[i].Name);

            TypeCaches[baseType] = (types, names);
            return (types, names);
        }

        public static void DrawGUI(Rect position, SerializedProperty property, GUIContent label, Type baseType)
        {
            var (concreteTypes, displayNames) = GetCachedTypes(baseType);

            EditorGUI.BeginProperty(position, label, property);

            Type currentType = property.managedReferenceValue?.GetType();
            int currentIndex = 0;
            if (currentType != null)
            {
                currentIndex = Array.IndexOf(concreteTypes, currentType) + 1;
                if (currentIndex <= 0) currentIndex = 0;
            }

            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, displayNames);

            if (newIndex != currentIndex)
            {
                property.managedReferenceValue = newIndex == 0
                    ? null
                    : Activator.CreateInstance(concreteTypes[newIndex - 1]);
            }

            if (property.managedReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                var iterator = property.Copy();
                int depth = iterator.depth;
                if (iterator.NextVisible(true))
                {
                    float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    do
                    {
                        if (iterator.depth <= depth) break;
                        float h = EditorGUI.GetPropertyHeight(iterator, true);
                        var childRect = new Rect(position.x, y, position.width, h);
                        EditorGUI.PropertyField(childRect, iterator, true);
                        y += h + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (iterator.NextVisible(false));
                }
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public static float GetHeight(SerializedProperty property)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue != null)
            {
                var iterator = property.Copy();
                int depth = iterator.depth;
                if (iterator.NextVisible(true))
                {
                    do
                    {
                        if (iterator.depth <= depth) break;
                        height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                    }
                    while (iterator.NextVisible(false));
                }
            }

            return height;
        }
    }

    [CustomPropertyDrawer(typeof(DestinationStrategy), true)]
    public class DestinationStrategyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.DrawGUI(position, property, label, typeof(DestinationStrategy));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.GetHeight(property);
    }

    [CustomPropertyDrawer(typeof(PointSelector), true)]
    public class PointSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.DrawGUI(position, property, label, typeof(PointSelector));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.GetHeight(property);
    }
}
