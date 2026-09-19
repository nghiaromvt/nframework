using UnityEngine;

namespace NFramework
{
    public static class CameraExtension
    {
        public static bool IsWorldPointInViewport(this Camera camera, Vector3 point)
        {
            var position = camera.WorldToViewportPoint(point);
            return position is { x: > 0, x: < 1, y: > 0, y: < 1 };
        }
        
        /// <summary>
        /// Gets a point with the same screen point as the source point,
        /// but at the specified distance from camera.
        /// </summary>
        public static Vector3 WorldPointOffsetByDepth(this Camera camera,
            Vector3 source,
            float distanceFromCamera,
            Camera.MonoOrStereoscopicEye eye = Camera.MonoOrStereoscopicEye.Mono)
        {
            var screenPoint = camera.WorldToScreenPoint(source, eye);
            return camera.ScreenToWorldPoint(screenPoint.WithZ(distanceFromCamera), eye);
        }
        
        public static Bounds GetOrthoCameraBounds(this Camera cam)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Vector3 center = cam.transform.position;
            Vector3 size = new Vector3(halfWidth * 2f, halfHeight * 2f, 0f);

            return new Bounds(center, size);
        }
        
        public static Vector3 RemapWorldPositionBetweenCameras(this Camera sourceCam,
            Vector3 sourceWorldPos, Camera targetCam, float targetDepth)
        {
            Vector3 viewportPos = sourceCam.WorldToViewportPoint(sourceWorldPos);
            return targetCam.ViewportToWorldPoint(
                new Vector3(viewportPos.x, viewportPos.y, targetDepth)
            );
        }
    }
}

