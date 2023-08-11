using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NFramework
{
    public static class GizmosDrawer
    {
        /// <summary>
        /// Draws a debug arrow going from the origin position and along the direction Vector3
        /// </summary>
        /// <param name="origin">Origin.</param>
        /// <param name="direction">Direction.</param>
        /// <param name="color">Color.</param>
        /// <param name="arrowLength">Arrow length.</param>
        /// <param name="arrowHeadLength">Arrow head length.</param>
        [Conditional("IS_DEBUG")]
        public static void DrawArrow(Vector3 origin, Vector3 direction, float arrowLength = 1, float arrowHeadLength = 0.20f, Color color = default)
        {
            if (direction == Vector3.zero)
                return;

            direction.Normalize();

            if (color == default)
                color = Color.white;

            Debug.DrawRay(origin, direction * arrowLength, color);
            DrawArrowEnd(origin, direction * arrowLength, color, arrowHeadLength, 35f);
        }

        /// <summary>
        /// Draws a debug cross of the specified size and color at the specified point
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawCross(Vector3 spot, float crossSize = 1f, Color color = default)
        {
            if (color == default)
                color = Color.white;

            Vector3 tempOrigin = Vector3.zero;
            Vector3 tempDirection = Vector3.zero;

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y - crossSize / 2;
            tempOrigin.z = spot.z;
            tempDirection.x = 1;
            tempDirection.y = 1;
            tempDirection.z = 0;
            Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);

            tempOrigin.x = spot.x - crossSize / 2;
            tempOrigin.y = spot.y + crossSize / 2;
            tempOrigin.z = spot.z;
            tempDirection.x = 1;
            tempDirection.y = -1;
            tempDirection.z = 0;
            Debug.DrawRay(tempOrigin, tempDirection * crossSize, color);
        }

        /// <summary>
		/// Draws the arrow end for DebugDrawArrow
		/// </summary>
		/// <param name="arrowEndPosition">Arrow end position.</param>
		/// <param name="direction">Direction.</param>
		/// <param name="color">Color.</param>
		/// <param name="arrowHeadLength">Arrow head length.</param>
        /// <param name="arrowHeadAngle">Arrow head angle.</param>
        [Conditional("IS_DEBUG")]
        private static void DrawArrowEnd(Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if (direction == Vector3.zero)
                return;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
            Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
            Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;

            Debug.DrawRay(arrowEndPosition + direction, right * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, left * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, up * arrowHeadLength, color);
            Debug.DrawRay(arrowEndPosition + direction, down * arrowHeadLength, color);
        }

        /// <summary>
		/// Draws a rectangle based on a Rect and color
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawRectangle(Rect rectangle, Color color = default)
        {
            if (color == default)
                color = Color.white;

            Vector3 pos = new Vector3(rectangle.x + rectangle.width / 2, rectangle.y + rectangle.height / 2, 0.0f);
            Vector3 scale = new Vector3(rectangle.width, rectangle.height, 0.0f);

            DrawRectangle(pos, scale, color);
        }

        /// <summary>
        /// Draws a rectangle of the specified color and size at the specified position
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawRectangle(Vector3 position, Vector3 size, Color color = default)
        {
            if (color == default)
                color = Color.white;

            Vector3 halfSize = size / 2f;

            Vector3[] points = new Vector3[]
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
        }

        /// <summary>
        /// Draws a point of the specified color and size at the specified position
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawPoint(Vector3 position, float size = 1, Color color = default)
        {
            if (color == default)
                color = Color.white;

            Vector3[] points = new Vector3[]
            {
                position + (Vector3.up * size),
                position - (Vector3.up * size),
                position + (Vector3.right * size),
                position - (Vector3.right * size),
                position + (Vector3.forward * size),
                position - (Vector3.forward * size)
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[4], points[5], color);
            Debug.DrawLine(points[0], points[2], color);
            Debug.DrawLine(points[0], points[3], color);
            Debug.DrawLine(points[0], points[4], color);
            Debug.DrawLine(points[0], points[5], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[1], points[3], color);
            Debug.DrawLine(points[1], points[4], color);
            Debug.DrawLine(points[1], points[5], color);
            Debug.DrawLine(points[4], points[2], color);
            Debug.DrawLine(points[4], points[3], color);
            Debug.DrawLine(points[5], points[2], color);
            Debug.DrawLine(points[5], points[3], color);
        }

        [Conditional("IS_DEBUG")]
        public static void DrawRay(Ray ray, float length = 1f, Color color = default, float duration = 0f, bool depthTest = true)
        {
            if (color == default)
                color = Color.white;

            Debug.DrawRay(ray.origin, ray.direction * length, color, duration, depthTest);
        }

        /// <summary>
        /// Draws a gizmo sphere of the specified size and color at a position
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawGizmoPoint(Vector3 position, float size, Color color)
        {
            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, size);
        }

        /// <summary>
        /// Draws handles to materialize the bounds of an object on screen.
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawHandlesBounds(Bounds bounds, Color color)
        {
#if UNITY_EDITOR
            Vector3 boundsCenter = bounds.center;
            Vector3 boundsExtents = bounds.extents;

            Vector3 v3FrontTopLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top left corner
            Vector3 v3FrontTopRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front top right corner
            Vector3 v3FrontBottomLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom left corner
            Vector3 v3FrontBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z - boundsExtents.z);  // Front bottom right corner
            Vector3 v3BackTopLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top left corner
            Vector3 v3BackTopRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y + boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back top right corner
            Vector3 v3BackBottomLeft = new Vector3(boundsCenter.x - boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom left corner
            Vector3 v3BackBottomRight = new Vector3(boundsCenter.x + boundsExtents.x, boundsCenter.y - boundsExtents.y, boundsCenter.z + boundsExtents.z);  // Back bottom right corner


            Handles.color = color;

            Handles.DrawLine(v3FrontTopLeft, v3FrontTopRight);
            Handles.DrawLine(v3FrontTopRight, v3FrontBottomRight);
            Handles.DrawLine(v3FrontBottomRight, v3FrontBottomLeft);
            Handles.DrawLine(v3FrontBottomLeft, v3FrontTopLeft);

            Handles.DrawLine(v3BackTopLeft, v3BackTopRight);
            Handles.DrawLine(v3BackTopRight, v3BackBottomRight);
            Handles.DrawLine(v3BackBottomRight, v3BackBottomLeft);
            Handles.DrawLine(v3BackBottomLeft, v3BackTopLeft);

            Handles.DrawLine(v3FrontTopLeft, v3BackTopLeft);
            Handles.DrawLine(v3FrontTopRight, v3BackTopRight);
            Handles.DrawLine(v3FrontBottomRight, v3BackBottomRight);
            Handles.DrawLine(v3FrontBottomLeft, v3BackBottomLeft);
#endif
        }

        /// <summary>
        /// Draws a solid rectangle at the specified position and size, and of the specified colors
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawSolidRectangle(Vector3 position, Vector3 size, Color borderColor, Color solidColor)
        {
#if UNITY_EDITOR
            Vector3 halfSize = size / 2f;

            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
            verts[1] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
            verts[2] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            verts[3] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            Handles.DrawSolidRectangleWithOutline(verts, solidColor, borderColor);
#endif
        }

        /// <summary>
        /// Draws a cube at the specified position, and of the specified color and size
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawCube(Vector3 position, Color color, Vector3 size)
        {
            Vector3 halfSize = size / 2f;

            Vector3[] points = new Vector3[]
            {
                position + new Vector3(halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,halfSize.z),
                position + new Vector3(halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,halfSize.y,-halfSize.z),
                position + new Vector3(-halfSize.x,-halfSize.y,-halfSize.z),
                position + new Vector3(halfSize.x,-halfSize.y,-halfSize.z),
            };

            Debug.DrawLine(points[0], points[1], color);
            Debug.DrawLine(points[1], points[2], color);
            Debug.DrawLine(points[2], points[3], color);
            Debug.DrawLine(points[3], points[0], color);
        }

        /// <summary>
        /// Draws a cube at the specified position, offset, and of the specified size
        /// </summary>
        [Conditional("IS_DEBUG")]
        public static void DrawGizmoCube(Transform transform, Vector3 offset, Vector3 cubeSize, bool wireOnly)
        {
            Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
            Gizmos.matrix = rotationMatrix;
            if (wireOnly)
                Gizmos.DrawWireCube(offset, cubeSize);
            else
                Gizmos.DrawCube(offset, cubeSize);
        }
    }
}
