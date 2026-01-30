#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gaskellgames.EditorOnly
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    //[CreateAssetMenu(fileName = "ShaderAutoUpdater", menuName = "Gaskellgames/ShaderAutoUpdater")]
    public class ShaderAutoUpdater_SO : GgScriptableObject
    {
        #region Variables

        private enum Pipeline
        {
            Unknown,
            BIRP,
            URP,
            HDRP,
            Other
        }
        
        [SerializeField, ReadOnly]
        [Tooltip("The current render pipeline target.")]
        private Pipeline targetRenderPipeline = Pipeline.Unknown;
        
        [SerializeField, Required]
        [Tooltip("Reference to all materials to be auto-updated based on render pipeline.")]
        private List<Material> materials;
    
        private const string packageRefName = "GgCore";
        private const string relativeEditorFolder = "/Editor/Materials";
        private const string relativeRuntimeFolder = "/Runtime/Materials/Shaders";
        
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region Editor Loop
        
        private void OnEnable()
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();
        }
        
        private void OnDisable()
        {
            tokenSource?.Cancel();
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region Private Methods

        private void DeleteAllShadersExcept(string folderPath, string assetName)
        {
            // cache path of asset to keep
            string assetFilePath = folderPath + "/" + assetName;
            string metaFilePath = assetFilePath + ".meta";
            
            // get all assets in folder
            string[] assetGuids = AssetDatabase.FindAssets("", new string[] { folderPath });
            
            // delete all except specified asset and the corresponding meta file
            for (int i = assetGuids.Length - 1; i >= 0; i--)
            {
                string guid = assetGuids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath == assetFilePath || assetPath == metaFilePath) { continue; }
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
        
        private async void ImportShaderForPipeline(Pipeline pipeline, bool cancelIfAlreadyExists)
        {
            tokenSource?.Cancel();
            tokenSource = new CancellationTokenSource();

            // cache file paths and asset names (packages should all be setup to import to Assets root folder!)
            if (!GetPackageNameForPipeline(pipeline, out string packageName)) { return; }
            if (!GgPackageRef.TryGetFullFilePath(packageRefName, relativeEditorFolder, out string packagesFolder)) { return; }
            if (!GetShaderNameForPipeline(pipeline, out string shaderName)) { return; }
            if (!GgPackageRef.TryGetFullFilePath(packageRefName, relativeRuntimeFolder, out string shaderFolder)) { return; }
            string importLocation = "Assets/" + shaderName;
            string desiredLocation = shaderFolder + "/" + shaderName;
            
            // check if shader already exists
            if (cancelIfAlreadyExists && System.IO.File.Exists(desiredLocation)) { return; }
            
            // import package
            string packageFilePath = packagesFolder + "/" + packageName;
            AssetDatabase.ImportPackage(packageFilePath, false);
            
            // move to runtime folder
            await GgTask.WaitUntil(() => System.IO.File.Exists(importLocation), tokenSource, 20);
            AssetDatabase.MoveAsset(importLocation, desiredLocation);
            
            // update material shaders
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(desiredLocation);
            UpdateMaterials(shader);
        }
        
        private bool GetPackageNameForPipeline(Pipeline pipeline, out string packageName)
        {
            switch (pipeline)
            {
                case Pipeline.BIRP:
                    packageName = "GgStandard_BIRP.unitypackage";
                    return true;
                
                case Pipeline.URP:
                    packageName = "GgStandard_URP.unitypackage";
                    return true;
                
                case Pipeline.HDRP:
                    packageName = "GgStandard_HDRP.unitypackage";
                    return true;
                
                case Pipeline.Unknown:
                case Pipeline.Other:
                default:
                    packageName = "";
                    return false;
            }
        }
        
        private bool GetShaderNameForPipeline(Pipeline pipeline, out string shaderName)
        {
            switch (pipeline)
            {
                case Pipeline.BIRP:
                    shaderName = "GgStandard_BIRP.shader";
                    return true;
                
                case Pipeline.URP:
                    shaderName = "GgStandard_URP.shader";
                    return true;
                
                case Pipeline.HDRP:
                    shaderName = "GgStandard_HDRP.shader";
                    return true;
                
                case Pipeline.Unknown:
                case Pipeline.Other:
                default:
                    shaderName = "";
                    return false;
            }
        }
        
        /// <summary>
        /// Update the shader used in each material.
        /// </summary>
        /// <param name="shader"></param>
        private void UpdateMaterials(Shader shader)
        {
            if (!shader)
            {
                Log(GgLogType.Warning, "Warning: Missing shader reference: ShaderAutoUpdater failed to update shaders for the current pipeline.");
                return;
            }
            
            for (int i = materials.Count - 1; i >= 0; i--)
            {
                if (!materials[i])
                {
                    materials.RemoveAt(i);
                    continue;
                }
                if (materials[i].shader == shader) { continue; }
                materials[i].shader = shader;
                Log(GgLogType.Info, "Material [{0}] updated to use shader [{1}].", materials[i].name, shader);
            }
        }
        
        /// <summary>
        /// Get the pipeline for the current target.
        /// </summary>
        /// <returns></returns>
        private Pipeline GetTargetRenderPipeline()
        {
            if (GetOverrideRenderPipeline(out Pipeline pipeline))
            {
                Log(GgLogType.Info, "Using override render pipeline: {0}", pipeline);
                return pipeline;
            }
            
            pipeline = GetDefaultRenderPipeline();
            Log(GgLogType.Info, "Using default render pipeline: {0}", pipeline);
            return pipeline;
        }

        /// <summary>
        /// Use <see cref="GraphicsSettings.defaultRenderPipeline"/> to determine the default render pipeline.
        /// </summary>
        private Pipeline GetDefaultRenderPipeline()
        {
            if (GraphicsSettings.defaultRenderPipeline == null)
            {
                return Pipeline.BIRP;
            }

            if (GraphicsSettings.defaultRenderPipeline.GetType().Name == "HDRenderPipelineAsset")
            {
                return Pipeline.HDRP;
            }

            if (GraphicsSettings.defaultRenderPipeline.GetType().Name == "UniversalRenderPipelineAsset")
            {
                return Pipeline.URP;
            }

            return Pipeline.Other;
        }

        /// <summary>
        /// Use <see cref="GraphicsSettings.currentRenderPipeline"/> to determine the current render pipeline.
        /// </summary>
        /// <returns></returns>
        private Pipeline GetCurrentRenderPipeline()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                return Pipeline.BIRP;
            }

            if (GraphicsSettings.currentRenderPipeline.GetType().Name == "HDRenderPipelineAsset")
            {
                return Pipeline.HDRP;
            }

            if (GraphicsSettings.currentRenderPipeline.GetType().Name == "UniversalRenderPipelineAsset")
            {
                return Pipeline.URP;
            }

            return Pipeline.Other;
        }

        /// <summary>
        /// Use <see cref="QualitySettings.renderPipeline"/> to determine the overriding render pipeline for the current quality level.
        /// </summary>
        /// <param name="pipeline"></param>
        /// <returns></returns>
        private bool GetOverrideRenderPipeline(out Pipeline pipeline)
        {
            if (QualitySettings.renderPipeline == null)
            {
                // False: The active render pipeline is the default render pipeline
                pipeline = Pipeline.Unknown;
                return false;
            }

            // True: The active render pipeline is the overriding render pipeline...
            if (QualitySettings.renderPipeline.GetType().Name == "HDRenderPipelineAsset")
            {
                // ... HDRP
                pipeline = Pipeline.HDRP;
                return true;
            }

            if (QualitySettings.renderPipeline.GetType().Name == "UniversalRenderPipelineAsset")
            {
                // ... URP
                pipeline = Pipeline.URP;
                return true;
            }

            // ... Other
            pipeline = Pipeline.Other;
            return true;
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region Internal Methods
        
        internal void UpdateMaterialsForCurrentTargetPipeline()
        {
            Pipeline pipeline = GetTargetRenderPipeline();
            targetRenderPipeline = pipeline;
            
            // get shader name for current pipeline
            if (!GetShaderNameForPipeline(pipeline, out string shaderName)) { return; }
        
            // delete shaders for other pipelines
            if (!GgPackageRef.TryGetFullFilePath(packageRefName, relativeRuntimeFolder, out string folderPath)) { return; }
            DeleteAllShadersExcept(folderPath, shaderName);
        
            // import shader from package if current pipeline shader does not exist
            ImportShaderForPipeline(pipeline, true);
        }

        #endregion
        
    } // class end 
}
#endif