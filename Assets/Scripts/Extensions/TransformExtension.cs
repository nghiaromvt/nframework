using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class TransformExtension
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
            StringMatchType matchType = StringMatchType.Exactly)
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
            StringMatchType matchType = StringMatchType.Exactly)
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
            StringMatchType matchType = StringMatchType.Exactly)
        {
            for (int t = transform.childCount - 1; t >= 0; t--)
            {
                Transform child = transform.GetChild(t);

                if (exceptChildName != null)
                {
                    if (child.name.IsMatchWith(exceptChildName, matchType))
                        continue;
                }

                if (exceptChildType != null && child.TryGetComponent(exceptChildType, out _))
                    continue;

                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(child.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
        
        #endregion

        #region Set Position
        
        public static void SetPosX(this Transform tf, float x) => tf.position = tf.position.WithX(x);

        public static void SetPosY(this Transform tf, float y) => tf.position = tf.position.WithY(y);

        public static void SetPosZ(this Transform tf, float z) => tf.position = tf.position.WithZ(z);

        public static void SetPosXY(this Transform tf, float x, float y) 
            => tf.position = tf.position.WithXY(x, y);
        
        public static void SetPosXZ(this Transform tf, float x, float z) 
            => tf.position = tf.position.WithXZ(x, z);
        
        public static void SetPosYZ(this Transform tf, float y, float z) 
            => tf.position = tf.position.WithYZ(y, z);

        public static void SetLocalPosX(this Transform tf, float x) => tf.localPosition = tf.localPosition.WithX(x);

        public static void SetLocalPosY(this Transform tf, float y) => tf.localPosition = tf.localPosition.WithY(y);

        public static void SetLocalPosZ(this Transform tf, float z) => tf.localPosition = tf.localPosition.WithZ(z);

        public static void SetLocalPosXY(this Transform tf, float x, float y) 
            => tf.localPosition = tf.localPosition.WithXY(x, y);
        
        public static void SetLocalPosXZ(this Transform tf, float x, float z) 
            => tf.localPosition = tf.localPosition.WithXZ(x, z);
        
        public static void SetLocalPosYZ(this Transform tf, float y, float z) 
            => tf.localPosition = tf.localPosition.WithYZ(y, z);
        
        #endregion

        #region Set Euler Angles
        
        public static void SetEulerAnglesX(this Transform tf, float x) => tf.eulerAngles = tf.eulerAngles.WithX(x);

        public static void SetEulerAnglesY(this Transform tf, float y) => tf.eulerAngles = tf.eulerAngles.WithY(y);

        public static void SetEulerAnglesZ(this Transform tf, float z) => tf.eulerAngles = tf.eulerAngles.WithZ(z);

        public static void SetEulerAnglesXY(this Transform tf, float x, float y) 
            => tf.eulerAngles = tf.eulerAngles.WithXY(x, y);
        
        public static void SetEulerAnglesXZ(this Transform tf, float x, float z) 
            => tf.eulerAngles = tf.eulerAngles.WithXZ(x, z);
        
        public static void SetEulerAnglesYZ(this Transform tf, float y, float z) 
            => tf.eulerAngles = tf.eulerAngles.WithYZ(y, z);

        public static void SetLocalEulerAnglesX(this Transform tf, float x) => tf.localEulerAngles = tf.localEulerAngles.WithX(x);

        public static void SetLocalEulerAnglesY(this Transform tf, float y) => tf.localEulerAngles = tf.localEulerAngles.WithY(y);

        public static void SetLocalEulerAnglesZ(this Transform tf, float z) => tf.localEulerAngles = tf.localEulerAngles.WithZ(z);

        public static void SetLocalEulerAnglesXY(this Transform tf, float x, float y) 
            => tf.localEulerAngles = tf.localEulerAngles.WithXY(x, y);
        
        public static void SetLocalEulerAnglesXZ(this Transform tf, float x, float z) 
            => tf.localEulerAngles = tf.localEulerAngles.WithXZ(x, z);
        
        public static void SetLocalEulerAnglesYZ(this Transform tf, float y, float z) 
            => tf.localEulerAngles = tf.localEulerAngles.WithYZ(y, z);
        
        #endregion

        #region Set Local Scale
        
        public static void SetLocalScaleX(this Transform self, float value) => 
            self.localScale = self.localScale.WithX(value);

        public static void SetLocalScaleY(this Transform self, float value) => 
            self.localScale = self.localScale.WithY(value);

        public static void SetLocalScaleZ(this Transform self, float value) => 
            self.localScale = self.localScale.WithZ(value);
        
        public static void SetLocalScaleXY(this Transform tf, float x, float y) 
            => tf.localScale = tf.localScale.WithXY(x, y);
        
        public static void SetLocalScaleXZ(this Transform tf, float x, float z) 
            => tf.localScale = tf.localScale.WithXZ(x, z);
        
        public static void SetLocalScaleYZ(this Transform tf, float y, float z) 
            => tf.localScale = tf.localScale.WithYZ(y, z);
        
        #endregion

        #region Add Position
        
        public static void AddPosX(this Transform self, float x) => self.SetPosX(self.position.x + x);

        public static void AddPosY(this Transform self, float y) => self.SetPosY(self.position.y + y);

        public static void AddPosZ(this Transform self, float z) => self.SetPosZ(self.position.z + z);

        public static void AddLocalPosX(this Transform self, float x) => self.SetLocalPosX(self.localPosition.x + x);

        public static void AddLocalPosY(this Transform self, float y) => self.SetLocalPosY(self.localPosition.y + y);

        public static void AddLocalPosZ(this Transform self, float z) => self.SetLocalPosZ(self.localPosition.z + z);

        #endregion

        #region Add Euler Angle
        
        public static void AddEulerAnglesX(this Transform self, float x) => self.SetEulerAnglesX(self.eulerAngles.x + x);

        public static void AddEulerAnglesY(this Transform self, float y) => self.SetEulerAnglesY(self.eulerAngles.y + y);

        public static void AddEulerAnglesZ(this Transform self, float z) => self.SetEulerAnglesZ(self.eulerAngles.z + z);

        public static void AddLocalEulerAnglesX(this Transform self, float x) => self.SetLocalEulerAnglesX(self.localEulerAngles.x + x);

        public static void AddLocalEulerAnglesY(this Transform self, float y) => self.SetLocalEulerAnglesY(self.localEulerAngles.y + y);

        public static void AddLocalEulerAnglesZ(this Transform self, float z) => self.SetLocalEulerAnglesZ(self.localEulerAngles.z + z);

        #endregion

        #region Add Local Scale
        
        public static void AddLocalScaleX(this Transform self, float value) => self.SetLocalScaleX(self.localScale.x + value);

        public static void AddLocalScaleY(this Transform self, float value) => self.SetLocalScaleY(self.localScale.y + value);

        public static void AddLocalScaleZ(this Transform self, float value) => self.SetLocalScaleZ(self.localScale.z + value);

        #endregion
    }
}