using UnityEngine;

namespace NFramework
{
    public static class MathUtils
    {
        /// <summary>
        /// Remaps a value x in interval [A,B], to the proportional value in interval [C,D]
        /// </summary>
        /// <param name="x">The value to remap.</param>
        /// <param name="A">the minimum bound of interval [A,B] that contains the x value</param>
        /// <param name="B">the maximum bound of interval [A,B] that contains the x value</param>
        /// <param name="C">the minimum bound of target interval [C,D]</param>
        /// <param name="D">the maximum bound of target interval [C,D]</param>
        public static float Remap(float x, float A, float B, float C, float D)
        {
            float remappedValue = C + (x - A) / (B - A) * (D - C);
            return remappedValue;
        }

        /// <summary>
        /// Clamps the angle in parameters between a minimum and maximum angle (all angles expressed in degrees)
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="minimumAngle"></param>
        /// <param name="maximumAngle"></param>
        public static float ClampAngle(float angle, float minimumAngle, float maximumAngle)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;

            return Mathf.Clamp(angle, minimumAngle, maximumAngle);
        }

        /// <summary>
        /// Computes and returns the angle between two vectors, on a 360° scale
        /// This is use for 2D (XY)
        /// </summary>
        public static float AngleBetween2D(Vector2 vectorA, Vector2 vectorB)
        {
            float angle = Vector2.Angle(vectorA, vectorB);
            Vector3 cross = Vector3.Cross(vectorA, vectorB);

            if (cross.z > 0)
                angle = 360 - angle;

            return angle;
        }

        /// <summary>
        /// Returns a vector3 based on the angle in parameters
        /// </summary>
        public static Vector3 DirectionFromAngle(float angle)
        {
            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Sin(angle * Mathf.Deg2Rad);
            direction.y = 0f;
            direction.z = Mathf.Cos(angle * Mathf.Deg2Rad);
            return direction;
        }

        /// <summary>
        /// Returns a vector3 based on the angle in parameters
        /// </summary>
        /// <param name="angle"></param>
        public static Vector3 DirectionFromAngle2D(float angle)
        {
            Vector3 direction = Vector3.zero;
            direction.x = Mathf.Cos(angle * Mathf.Deg2Rad);
            direction.y = Mathf.Sin(angle * Mathf.Deg2Rad);
            direction.z = 0f;
            return direction;
        }

        /// <summary>
		/// Returns the distance between a point and a line.
		/// </summary>
		/// <returns>The between point and line.</returns>
		public static float DistanceBetweenPointAndLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            return Vector3.Magnitude(ProjectPointOnLine(point, lineStart, lineEnd) - point);
        }

        /// <summary>
        /// Projects a point on a line (perpendicularly) and returns the projected point.
        /// </summary>
        /// <returns>The point on line.</returns>
        public static Vector3 ProjectPointOnLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 rhs = point - lineStart;
            Vector3 vector2 = lineEnd - lineStart;
            float magnitude = vector2.magnitude;
            Vector3 lhs = vector2;
            if (magnitude > 1E-06f)
            {
                lhs = (Vector3)(lhs / magnitude);
            }
            float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
            return (lineStart + ((Vector3)(lhs * num2)));
        }

        public static float NormalizeFloat(float value, int fractNum)
        {
            return Mathf.RoundToInt(value * Mathf.Pow(10, fractNum)) / Mathf.Pow(10, fractNum);
        }
    }
}
