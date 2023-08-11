using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class TranformExtension
    {
        public static int GetChildCount(this Transform trans, bool includeInactive)
        {
            if (includeInactive)
            {
                return trans.childCount;
            }
            else
            {
                int count = 0;
                for (int i = 0; i < trans.childCount; ++i)
                {
                    if (trans.GetChild(i).gameObject.activeSelf)
                    {
                        ++count;
                    }
                }
                return count;
            }
        }

        #region Find Deep
        /// <summary>
        /// Finds (first) child by name, breadth first
        /// </summary>
        public static Transform FindDeepChildBFS(this Transform parent, string childName,
            EStringMatchType matchType = EStringMatchType.Exactly)
        {
            if (childName == null)
                return null;

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                var child = queue.Dequeue();
                if (child.name.IsMatchWith(childName, matchType) && child != parent)
                    return child;

                foreach (Transform t in child)
                {
                    queue.Enqueue(t);
                }
            }
            return null;
        }

        /// <summary>
        /// Finds children by name, depth first
        /// </summary>
        public static Transform FindDeepChildDFS(this Transform parent, string childName,
            EStringMatchType matchType = EStringMatchType.Exactly)
        {
            if (childName == null)
                return null;

            foreach (Transform child in parent)
            {
                if (child.name.IsMatchWith(childName, matchType))
                    return child;

                var result = child.FindDeepChildDFS(childName);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion

        #region Destroy all children
        /// <summary>
        /// Destroys a transform's children.
        /// Note: children is just under 1 level.
        /// </summary>
        public static void DestroyAllChildren(this Transform transform, Type exceptChildType = null, string exceptChildName = null,
            EStringMatchType matchType = EStringMatchType.Exactly)
        {
            for (int t = transform.childCount - 1; t >= 0; t--)
            {
                Transform child = transform.GetChild(t);

                if (exceptChildName != null)
                {
                    if (child.name.IsMatchWith(exceptChildName, matchType))
                        continue;
                }

                if (exceptChildType != null && child.TryGetComponent(exceptChildType, out var component))
                    continue;

                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(child.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
        #endregion

        #region Set Position
        public static void SetPosX(this Transform tf, float x)
        {
            tf.position = new Vector3(x, tf.position.y, tf.position.z);
        }

        public static void SetPosY(this Transform tf, float y)
        {
            tf.position = new Vector3(tf.position.x, y, tf.position.z);
        }

        public static void SetPosZ(this Transform tf, float z)
        {
            tf.position = new Vector3(tf.position.x, tf.position.y, z);
        }

        public static void SetLocalPosX(this Transform tf, float x)
        {
            tf.localPosition = new Vector3(x, tf.localPosition.y, tf.localPosition.z);
        }

        public static void SetLocalPosY(this Transform tf, float y)
        {
            tf.localPosition = new Vector3(tf.localPosition.x, y, tf.localPosition.z);
        }

        public static void SetLocalPosZ(this Transform self, float z)
        {
            self.localPosition = new Vector3(self.localPosition.x, self.localPosition.y, z);
        }
        #endregion

        #region Set Euler Angle
        public static void SetEulerAngleX(this Transform self, float x)
        {
            self.eulerAngles = new Vector3(x, self.eulerAngles.y, self.eulerAngles.z);
        }

        public static void SetEulerAngleY(this Transform self, float y)
        {
            self.eulerAngles = new Vector3(self.eulerAngles.x, y, self.eulerAngles.z);
        }

        public static void SetEulerAngleZ(this Transform self, float z)
        {
            self.eulerAngles = new Vector3(self.eulerAngles.x, self.eulerAngles.y, z);
        }

        public static void SetLocalEulerAngleX(this Transform self, float x)
        {
            self.localEulerAngles = new Vector3(x, self.localEulerAngles.y, self.localEulerAngles.z);
        }

        public static void SetLocalEulerAngleY(this Transform self, float y)
        {
            self.localEulerAngles = new Vector3(self.localEulerAngles.x, y, self.localEulerAngles.z);
        }

        public static void SetLocalEulerAngleZ(this Transform self, float z)
        {
            self.localEulerAngles = new Vector3(self.localEulerAngles.x, self.localEulerAngles.y, z);
        }
        #endregion

        #region Set Local Scale
        public static void SetLocalScaleX(this Transform self, float value)
        {
            self.localScale = new Vector3(value, self.localScale.y, self.localScale.z);
        }

        public static void SetLocalScaleY(this Transform self, float value)
        {
            self.localScale = new Vector3(self.localScale.x, value, self.localScale.z);
        }

        public static void SetLocalScaleZ(this Transform self, float value)
        {
            self.localScale = new Vector3(self.localScale.x, self.localScale.y, value);
        }
        #endregion

        #region Add Position
        public static void AddPosX(this Transform self, float x)
        {
            self.SetPosX(self.position.x + x);
        }

        public static void AddPosY(this Transform self, float y)
        {
            self.SetPosY(self.position.y + y);
        }

        public static void AddPosZ(this Transform self, float z)
        {
            self.SetPosZ(self.position.z + z);
        }

        public static void AddLocalPosX(this Transform self, float x)
        {
            self.SetLocalPosX(self.localPosition.x + x);
        }

        public static void AddLocalPosY(this Transform self, float y)
        {
            self.SetLocalPosY(self.localPosition.y + y);
        }

        public static void AddLocalPosZ(this Transform self, float z)
        {
            self.SetLocalPosZ(self.localPosition.z + z);
        }
        #endregion

        #region Add Euler Angle
        public static void AddEulerAngleX(this Transform self, float x)
        {
            self.SetEulerAngleX(self.eulerAngles.x + x);
        }

        public static void AddEulerAngleY(this Transform self, float y)
        {
            self.SetEulerAngleY(self.eulerAngles.y + y);
        }

        public static void AddEulerAngleZ(this Transform self, float z)
        {
            self.SetEulerAngleZ(self.eulerAngles.z + z);
        }

        public static void AddLocalEulerAngleX(this Transform self, float x)
        {
            self.SetLocalEulerAngleX(self.localEulerAngles.x + x);
        }

        public static void AddLocalEulerAngleY(this Transform self, float y)
        {
            self.SetLocalEulerAngleY(self.localEulerAngles.y + y);
        }

        public static void AddLocalEulerAngleZ(this Transform self, float z)
        {
            self.SetLocalEulerAngleZ(self.localEulerAngles.z + z);
        }
        #endregion

        #region Add Local Scale
        public static void AddLocalScaleX(this Transform self, float value)
        {
            self.SetLocalScaleX(self.localScale.x + value);
        }

        public static void AddLocalScaleY(this Transform self, float value)
        {
            self.SetLocalScaleY(self.localScale.y + value);
        }

        public static void AddLocalScaleZ(this Transform self, float value)
        {
            self.SetLocalScaleZ(self.localScale.z + value);
        }
        #endregion
    }
}