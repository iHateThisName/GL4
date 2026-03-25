using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    [CustomPropertyDrawer(typeof(TransitionCondition), true)]
    public class TransitionConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.DrawGUI(position, property, label, typeof(TransitionCondition));

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => SerializeReferenceDrawerHelper.GetHeight(property);
    }
}
