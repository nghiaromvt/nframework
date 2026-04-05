using UnityEngine;

namespace NFramework
{
    public static class RendererExtension
    {
        /// <summary>
        /// Returns true if a renderer is visible from a camera
        /// </summary>
        public static bool IsVisibleFromCamera(this Renderer renderer, Camera camera)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
        }

        /// <summary>
        /// Set shared material theo index trong mảng sharedMaterials
        /// </summary>
        public static void SetSharedMaterialAt(this Renderer renderer, int index, Material material)
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null");
                return;
            }

            var mats = renderer.sharedMaterials;

            if (mats == null || mats.Length == 0)
            {
                Debug.LogError("Renderer has no sharedMaterials");
                return;
            }

            if (index < 0 || index >= mats.Length)
            {
                Debug.LogError($"Index {index} is out of range (0 - {mats.Length - 1})");
                return;
            }

            mats[index] = material;
            renderer.sharedMaterials = mats;
        }
    }
}