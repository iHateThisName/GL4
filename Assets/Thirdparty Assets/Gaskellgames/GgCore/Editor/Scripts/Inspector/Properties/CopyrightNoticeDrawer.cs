#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gaskellgames.EditorOnly
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    [CustomPropertyDrawer(typeof(CopyrightNotice))]
    public class CopyrightNoticeDrawer : PropertyDrawer
    {
        #region variables

        private SerializedProperty firstPublished;
        private SerializedProperty ownerName;
        
        private float singleLine = EditorGUIUtility.singleLineHeight;
        
        #endregion

        //----------------------------------------------------------------------------------------------------
        
        #region Property Height
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) - (singleLine * 1.1f);
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------

        #region OnGUI

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // open property and get reference to instance
            EditorGUI.BeginProperty(position, label, property);

            // get reference to SerializeFields
            firstPublished = property.FindPropertyRelative("firstPublished");
            ownerName = property.FindPropertyRelative("ownerName");
            
            // draw property
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(label.text, label.tooltip));
            GUILayout.Space(2);
            EditorGUILayout.LabelField("\u00a9", GUILayout.Width(14));
            firstPublished.intValue = EditorGUILayout.IntField(new GUIContent(""), firstPublished.intValue, GUILayout.Width(35), GUILayout.ExpandWidth(true));
            ownerName.stringValue = EditorGUILayout.TextField(new GUIContent(""), ownerName.stringValue, GUILayout.Width(140), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            // close property
            EditorGUI.EndProperty();
        }

        #endregion

    } // class end
}
#endif
