using System.Collections.Generic;
using UnityEngine;

namespace Gaskellgames
{
    /// <remarks>
    /// Code created by Gaskellgames: https://gaskellgames.com
    /// </remarks>
    
    public static class BoundsExtensions
    {
        #region TryGetBounds
        
        /// <summary>
        /// Try to calculate the world space bounding volume of an object and it's children.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <param name="useRenderers"></param>
        /// <param name="useColliders"></param>
        /// <param name="usePivots"></param>
        /// <returns></returns>
        private static bool TryGetBounds(Transform root, out Bounds bounds, bool useRenderers, bool useColliders, bool usePivots)
        {
            // quick return
            if (!root)
            {
                bounds = new Bounds();
                return false;
            }

            if (useRenderers && TryGetRendererBounds(root, out Bounds rendererBounds))
            {
                bounds = rendererBounds;
                if (useColliders && TryGetColliderBounds(root, out Bounds colliderBounds))
                {
                    bounds.Encapsulate(colliderBounds);
                }
                if (usePivots && TryGetPivotBounds(root, out Bounds pivotBounds))
                {
                    bounds.Encapsulate(pivotBounds);
                }
                return true;
            }
            else
            {
                if (useColliders && TryGetColliderBounds(root, out Bounds colliderBounds))
                {
                    bounds = colliderBounds;
                    if (usePivots && TryGetPivotBounds(root, out Bounds pivotBounds))
                    {
                        bounds.Encapsulate(pivotBounds);
                    }
                    return true;
                }
                else
                {
                    if (usePivots && TryGetPivotBounds(root, out Bounds pivotBounds))
                    {
                        bounds = pivotBounds;
                        return true;
                    }
                    bounds = new Bounds();
                    return false;
                }
            }
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region TryGetRendererBounds
        
        /// <summary>
        /// Try to calculate the world space bounding volume of an object and it's children, using renderer.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            // quick return
            if (!root)
            {
                bounds = new Bounds();
                return false;
            }
            
            // calculate bounds of renderers
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                Renderer instance = renderers[i];
                bounds.Encapsulate(instance.bounds);
            }
            return 0 < renderers.Length;
        }
        
        #endregion
        
        //----------------------------------------------------------------------------------------------------

        #region TryGetColliderBounds
        
        /// <summary>
        /// Try to calculate the world space bounding volume of an object and it's children, using collider.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private static bool TryGetColliderBounds(Transform root, out Bounds bounds)
        {
            // quick return
            if (!root)
            {
                bounds = new Bounds();
                return false;
            }
            
            // calculate bounds of colliders
            Collider[] colliders = root.GetComponentsInChildren<Collider>();
            bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
            {
                Collider instance = colliders[i];
                bounds.Encapsulate(instance.bounds);
            }
            return 0 < colliders.Length;
        }

        #endregion
        
        //----------------------------------------------------------------------------------------------------
        
        #region TryGetPivotBounds
        
        /// <summary>
        /// Try to calculate the world space bounding volume of an object and it's children, using pivot points.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        private static bool TryGetPivotBounds(Transform root, out Bounds bounds)
        {
            // quick return
            if (!root)
            {
                bounds = new Bounds();
                return false;
            }
            
            // calculate bounds of pivots
            List<Vector3> positions = new List<Vector3>();
            Transform[] pivots = root.GetComponentsInChildren<Transform>();
            for (int i = 1; i < pivots.Length; i++)
            {
                Transform instance = pivots[i];
                positions.Add(instance.position);
            }
            bounds = GeometryUtility.CalculateBounds(positions.ToArray(), root.worldToLocalMatrix);
            bounds.center += root.position;
            return 0 < positions.Count;
        }

        #endregion

    } // class end
}