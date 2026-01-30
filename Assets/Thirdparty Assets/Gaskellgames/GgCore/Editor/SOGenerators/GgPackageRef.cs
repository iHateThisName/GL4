#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaskellgames.EditorOnly
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>

    //[CreateAssetMenu(fileName = "GgPackageRef_", menuName = "Gaskellgames/GgCore/GgPackageRef")]
    public class GgPackageRef : GgScriptableObject
    {
        #region Variables

        [SerializeField, ReadOnly]
        [Tooltip("The version reference of this package.")]
        private SemanticVersion version;

        [SerializeField, ReadOnly]
        [Tooltip("The copyright notice of this package.")]
        private CopyrightNotice copyright;

        [SerializeField, ReadOnly]
        [Tooltip("The local file path of this PackageRef.")]
        private string pathRef;
        
        private string PathRef => AssetDatabase.GetAssetPath(this).Replace($"/{this.name}.asset", "");

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region OnValidate

        private void OnValidate()
        {
            pathRef = PathRef;
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region Static Methods

        public static bool TryGetVersion(string packageRefName, out SemanticVersion version)
        {
            List<GgPackageRef> allPathRefs = EditorExtensions.GetAllAssetsByType<GgPackageRef>();
            foreach (GgPackageRef pathRefInstance in allPathRefs)
            {
                if (pathRefInstance.name != packageRefName) { continue; }
                version = pathRefInstance.version;
                return true;
            }

            version = new SemanticVersion();
            return false;
        }

        public static bool TryGetCopyright(string packageRefName, out CopyrightNotice copyright)
        {
            List<GgPackageRef> allPathRefs = EditorExtensions.GetAllAssetsByType<GgPackageRef>();
            foreach (GgPackageRef pathRefInstance in allPathRefs)
            {
                if (pathRefInstance.name != packageRefName) { continue; }
                copyright = pathRefInstance.copyright;
                return true;
            }

            copyright = new CopyrightNotice();
            return false;
        }
        
        public static bool TryGetFullFilePath(string packageRefName, string relativeFilepath, out string filePath)
        {
            List<GgPackageRef> allPathRefs = EditorExtensions.GetAllAssetsByType<GgPackageRef>();
            foreach (GgPackageRef pathRefInstance in allPathRefs)
            {
                if (pathRefInstance.name != packageRefName) { continue; }
                filePath = pathRefInstance.PathRef + relativeFilepath;
                return true;
            }

            filePath = string.Empty;
            return false;
        }

        #endregion
        
    } // class end
}
#endif