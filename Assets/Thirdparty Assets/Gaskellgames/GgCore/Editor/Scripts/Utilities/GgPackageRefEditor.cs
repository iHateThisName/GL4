#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Gaskellgames.EditorOnly
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    [CustomEditor(typeof(GgPackageRef)), CanEditMultipleObjects]
    public class GgPackageRefEditor : GgEditor
    {
        #region Serialized Properties / Variables

        private GgPackageRef targetAsType;
        
        private SerializedProperty version;
        private SerializedProperty copyright;
        private SerializedProperty pathRef;
        
        private Texture banner;
        
        #endregion

        //----------------------------------------------------------------------------------------------------
        
        #region OnEnable
        
        private void OnEnable()
        {
            if (!targetAsType) { targetAsType = target as GgPackageRef; }
            banner = EditorWindowUtility.LoadInspectorBanner();
            
            version = serializedObject.FindProperty("version");
            copyright = serializedObject.FindProperty("copyright");
            pathRef = serializedObject.FindProperty("pathRef");
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region OnInspectorGUI

        public override void OnInspectorGUI()
        {
            // get & update references
            serializedObject.Update();
            
            // draw banner if turned on in Gaskellgames settings
            EditorWindowUtility.TryDrawBanner(banner, nameof(GgPackageRef).NicifyName());
            
            // draw custom inspector:
            EditorGUILayout.PropertyField(version);
            EditorGUILayout.PropertyField(copyright);
            EditorGUILayout.PropertyField(pathRef);
            
            // apply reference changes
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

    } // class end
}

#endif