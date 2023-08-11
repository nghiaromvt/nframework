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
    }
}
