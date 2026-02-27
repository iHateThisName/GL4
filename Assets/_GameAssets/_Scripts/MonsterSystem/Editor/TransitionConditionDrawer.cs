using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    [CustomPropertyDrawer(typeof(TransitionCondition), true)]
    public class TransitionConditionDrawer : PropertyDrawer
    {
        private static Type[] _concreteTypes;
        private static string[] _displayNames;

        private static void CacheTypes()
        {
            if (_concreteTypes != null) return;

            var types = TypeCache.GetTypesDerivedFrom<TransitionCondition>()
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToArray();

            _concreteTypes = types;
            _displayNames = new string[types.Length + 1];
            _displayNames[0] = "(None)";
            for (int i = 0; i < types.Length; i++)
                _displayNames[i + 1] = ObjectNames.NicifyVariableName(types[i].Name);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheTypes();

            EditorGUI.BeginProperty(position, label, property);

            // Current type index
            Type currentType = property.managedReferenceValue?.GetType();
            int currentIndex = 0;
            if (currentType != null)
            {
                currentIndex = Array.IndexOf(_concreteTypes, currentType) + 1;
                if (currentIndex <= 0) currentIndex = 0; // type not found
            }

            // Type dropdown
            var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, _displayNames);

            if (newIndex != currentIndex)
            {
                if (newIndex == 0)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    property.managedReferenceValue = Activator.CreateInstance(_concreteTypes[newIndex - 1]);
                }
            }

            // Draw child properties inline
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheTypes();

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
}
