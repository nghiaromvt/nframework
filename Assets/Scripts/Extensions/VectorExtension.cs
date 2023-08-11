using UnityEngine;

namespace NFramework
{
    public static class VectorExtension
    {
        #region Vecter2 Extentions

        public static float GetAngleInRadian(this Vector2 v1, Vector2 v2)
        {
            return Mathf.Atan2(v2.y - v1.y, v2.x - v1.x);
        }

        public static float GetAngleInDegree(this Vector2 v1, Vector2 v2)
        {
            return Mathf.Atan2(v2.y - v1.y, v2.x - v1.x) * Mathf.Rad2Deg;
        }

        public static Vector2 Rotate(this Vector2 v, float angle)
        {
            return Quaternion.AngleAxis(angle, Vector3.back) * v;
        }

        public static float GetAngleInDegree(this Vector2 v1)
        {
            return Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
        }

        #endregion

        #region Vecter3 Extentions

        public static float GetAngleInRadian(this Vector3 v1, Vector3 v2)
        {
            return Mathf.Atan2(v2.y - v1.y, v2.x - v1.x);
        }

        public static float GetAngleInDegree(this Vector3 v1, Vector3 v2)
        {
            return Mathf.Atan2(v2.y - v1.y, v2.x - v1.x) * Mathf.Rad2Deg;
        }

        public static float GetAngleInDegree(this Vector3 v1)
        {
            return Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;
        }


        public static Vector3 Rotate(this Vector3 v, float angle)
        {
            v = Quaternion.AngleAxis(angle, Vector3.back) * v;
            return v;
        }

        #endregion
    }
}