using UnityEngine;

namespace NFramework
{
    public static class VectorHelper
    {
        #region Round vector
        /// <summary>
        /// Rounds all components of a Vector2.
        /// </summary>
        public static Vector2 RoundVector(Vector2 vector) => RoundVector((Vector3)vector);

        /// <summary>
        /// Rounds all components of a Vector3.
        /// </summary>
        public static Vector3 RoundVector(Vector3 vector)
        {
            return new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
        }
        #endregion

        #region Random vector
        /// <summary>
        /// Returns a random Vector2 from 2 defined Vector2.
        /// </summary>
        public static Vector2 RandomVector(Vector2 minimum, Vector2 maximum)
        {
            return RandomVector((Vector3)minimum, (Vector3)maximum);
        }

        /// <summary>
        /// Returns a random Vector3 from 2 defined Vector3.
        /// </summary>
        public static Vector3 RandomVector(Vector3 minimum, Vector3 maximum)
        {
            return new Vector3(UnityEngine.Random.Range(minimum.x, maximum.x),
                                             UnityEngine.Random.Range(minimum.y, maximum.y),
                                             UnityEngine.Random.Range(minimum.z, maximum.z));
        }

        /// <summary>
        /// Returns a normalized random direction (2D).
        /// </summary>
        public static Vector2 GetRandomDirVector2()
        {
            return RandomVector(Vector2.zero, Vector2.one).normalized;
        }

        /// <summary>
        /// Returns a normalized random  direction (3D).
        /// </summary>
        public static Vector3 GetRandomDirVector3()
        {
            return RandomVector(Vector3.zero, Vector3.one).normalized;
        }
        #endregion

        #region Rotate vector
        /// <summary>
        /// Rotates a point around the given pivot.
        /// </summary>
        /// <returns>The new point position.</returns>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot's position.</param>
        /// <param name="angle">The angle we want to rotate our point.</param>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, float angle)
        {
            angle = angle * (Mathf.PI / 180f);
            var rotatedX = Mathf.Cos(angle) * (point.x - pivot.x) - Mathf.Sin(angle) * (point.y - pivot.y) + pivot.x;
            var rotatedY = Mathf.Sin(angle) * (point.x - pivot.x) + Mathf.Cos(angle) * (point.y - pivot.y) + pivot.y;
            return new Vector3(rotatedX, rotatedY, 0);
        }

        /// <summary>
		/// Rotates a point around the given pivot.
		/// </summary>
		/// <returns>The new point position.</returns>
		/// <param name="point">The point to rotate.</param>
		/// <param name="pivot">The pivot's position.</param>
		/// <param name="angles">The angle as a Vector3.</param>
		public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angle)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = Quaternion.Euler(angle) * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        /// <summary>
        /// Rotates a point around the given pivot.
        /// </summary>
        /// <returns>The new point position.</returns>
        /// <param name="point">The point to rotate.</param>
        /// <param name="pivot">The pivot's position.</param>
        /// <param name="angles">The angle as a Vector3.</param>
        public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion quaternion)
        {
            // we get point direction from the point to the pivot
            Vector3 direction = point - pivot;
            // we rotate the direction
            direction = quaternion * direction;
            // we determine the rotated point's position
            point = direction + pivot;
            return point;
        }

        /// <summary>
        /// Rotates a vector2 by the angle (in degrees) specified and returns it
        /// Same as rotate with pivot Vector3.zero
        /// </summary>
        /// <returns>The rotated Vector2.</returns>
        /// <param name="vector">The vector to rotate.</param>
        /// <param name="angle">Degrees.</param>
        public static Vector2 RotateVector2(Vector2 vector, float angle)
        {
            if (angle == 0)
                return vector;

            float sinus = Mathf.Sin(angle * Mathf.Deg2Rad);
            float cosinus = Mathf.Cos(angle * Mathf.Deg2Rad);

            float oldX = vector.x;
            float oldY = vector.y;
            vector.x = (cosinus * oldX) - (sinus * oldY);
            vector.y = (sinus * oldX) + (cosinus * oldY);
            return vector;
        }
        #endregion

        #region Line
        private static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        private static bool Approximately(float a, float b, float tolerance = 1e-5f) => Mathf.Abs(a - b) <= tolerance;

        public static float CrossProduct2D(Vector2 a, Vector2 b) => a.x * b.y - b.x * a.y;

        /// <summary>
        /// Determine whether 2 lines intersect, and give the intersection point if so.
        /// </summary>
        /// <param name="p1start">Start point of the first line</param>
        /// <param name="p1end">End point of the first line</param>
        /// <param name="p2start">Start point of the second line</param>
        /// <param name="p2end">End point of the second line</param>
        /// <param name="intersection">If there is an intersection, this will be populated with the point</param>
        /// <returns>True if the lines intersect, false otherwise.</returns>
        public static bool IntersectLineSegments2D(Vector2 p1start, Vector2 p1end, Vector2 p2start, Vector2 p2end,
            out Vector2 intersection)
        {
            // Consider:
            //   p1start = p
            //   p1end = p + r
            //   p2start = q
            //   p2end = q + s
            // We want to find the intersection point where :
            //  p + t*r == q + u*s
            // So we need to solve for t and u
            var p = p1start;
            var r = p1end - p1start;
            var q = p2start;
            var s = p2end - p2start;
            var qminusp = q - p;

            float cross_rs = CrossProduct2D(r, s);

            if (Approximately(cross_rs, 0f))
            {
                // Parallel lines
                if (Approximately(CrossProduct2D(qminusp, r), 0f))
                {
                    // Co-linear lines, could overlap
                    float rdotr = Vector2.Dot(r, r);
                    float sdotr = Vector2.Dot(s, r);
                    // this means lines are co-linear
                    // they may or may not be overlapping
                    float t0 = Vector2.Dot(qminusp, r / rdotr);
                    float t1 = t0 + sdotr / rdotr;
                    if (sdotr < 0)
                    {
                        // lines were facing in different directions so t1 > t0, swap to simplify check
                        Swap(ref t0, ref t1);
                    }

                    if (t0 <= 1 && t1 >= 0)
                    {
                        // Nice half-way point intersection
                        float t = Mathf.Lerp(Mathf.Max(0, t0), Mathf.Min(1, t1), 0.5f);
                        intersection = p + t * r;
                        return true;
                    }
                    else
                    {
                        // Co-linear but disjoint
                        intersection = Vector2.zero;
                        return false;
                    }
                }
                else
                {
                    // Just parallel in different places, cannot intersect
                    intersection = Vector2.zero;
                    return false;
                }
            }
            else
            {
                // Not parallel, calculate t and u
                float t = CrossProduct2D(qminusp, s) / cross_rs;
                float u = CrossProduct2D(qminusp, r) / cross_rs;
                if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
                {
                    intersection = p + t * r;
                    return true;
                }
                else
                {
                    // Lines only cross outside segment range
                    intersection = Vector2.zero;
                    return false;
                }
            }
        }
        #endregion
    }
}

