using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class TransformExtension
    {
        /// <summary>
        /// Returns the full hierarchy path of this transform from root to current node.
        /// </summary>
        /// <param name="transform">The transform to build the path for.</param>
        /// <returns>A slash-separated hierarchy path.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transform"/> is null.</exception>
        /// <example>
        /// <code>
        /// // Example output: "World/Enemies/Boss"
        /// string hierarchyPath = bossRoot.GetHierarchyPath();
        /// </code>
        /// </example>
        public static string GetHierarchyPath(this Transform transform) {
            if (!transform) throw new ArgumentNullException(nameof(transform));

            var path = transform.name;

            while (transform.parent != null) {
                transform = transform.parent;
                path = $"{transform.name}/{path}";
            }

            return path;
        }
        
        /// <summary>
        /// Check if the transform is within a certain distance and optionally within a certain angle (FOV) from the target transform.
        /// </summary>
        /// <param name="source">The transform to check.</param>
        /// <param name="target">The target transform to compare the distance and optional angle with.</param>
        /// <param name="maxDistance">The maximum distance allowed between the two transforms.</param>
        /// <param name="maxAngle">The maximum allowed angle between the transform's forward vector and the direction to the target (default is 360).</param>
        /// <returns>True if the transform is within range and angle (if provided) of the target, false otherwise.</returns>
        public static bool InRangeOf(this Transform source, Transform target, float maxDistance, float maxAngle = 360f) {
            Vector3 directionToTarget = (target.position - source.position).WithY(0);
            return directionToTarget.magnitude <= maxDistance && Vector3.Angle(source.forward, directionToTarget) <= maxAngle / 2;
        }
        
        /// <summary>
        /// Resets transform's position, scale and rotation
        /// </summary>
        /// <param name="transform">Transform to use</param>
        public static void Reset(this Transform transform) {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
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

        #region Children
        
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

        public static List<Transform> GetDeepChildren(this Transform parent, bool includeInactive = false)
        {
            var result = new List<Transform>();
            foreach (Transform child in parent)
            {
                if (includeInactive || child.gameObject.activeInHierarchy)
                {
                    result.Add(child);
                    result.AddRange(child.GetDeepChildren(includeInactive));
                }
            }
            return result;
        }
        
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
        
        /// <summary>
        /// Enables all child game objects of the given transform.
        /// </summary>
        /// <param name="parent">The Transform whose child game objects are to be enabled.</param>
        public static void EnableChildren(this Transform parent) {
            parent.ForEveryChild(child => child.gameObject.SetActive(true));
        }

        /// <summary>
        /// Disables all child game objects of the given transform.
        /// </summary>
        /// <param name="parent">The Transform whose child game objects are to be disabled.</param>
        public static void DisableChildren(this Transform parent) {
            parent.ForEveryChild(child => child.gameObject.SetActive(false));
        }

        /// <summary>
        /// Executes a specified action for each child of a given transform.
        /// </summary>
        /// <param name="parent">The parent transform.</param>
        /// <param name="action">The action to be performed on each child.</param>
        /// <remarks>
        /// This method iterates over all child transforms in reverse order and executes a given action on them.
        /// The action is a delegate that takes a Transform as parameter.
        /// </remarks>
        public static void ForEveryChild(this Transform parent, System.Action<Transform> action) {
            for (var i = parent.childCount - 1; i >= 0; i--) {
                action(parent.GetChild(i));
            }
        }
        
        /// <summary>
        /// Retrieves all the children of a given Transform.
        /// </summary>
        /// <remarks>
        /// This method can be used with LINQ to perform operations on all child Transforms. For example,
        /// you could use it to find all children with a specific tag, to disable all children, etc.
        /// Transform implements IEnumerable and the GetEnumerator method which returns an IEnumerator of all its children.
        /// </remarks>
        /// <param name="parent">The Transform to retrieve children from.</param>
        /// <returns>An IEnumerable&lt;Transform&gt; containing all the child Transforms of the parent.</returns>    
        public static IEnumerable<Transform> Children(this Transform parent) {
            foreach (Transform child in parent) {
                yield return child;
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

        #region Pose

        /// <summary>
        /// Sets a transform position and rotation from a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform to modify.</param>
        /// <param name="pose">The pose containing world position and world rotation.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transform"/> is null.</exception>
        /// <example>
        /// <code>
        /// Pose savedPose = savePoint.GetPose();
        /// cameraRig.SetPose(savedPose);
        /// </code>
        /// </example>
        public static void SetPose(this Transform transform, in Pose pose) {
            if (!transform) throw new ArgumentNullException(nameof(transform));

            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        /// <summary>
        /// Gets a transform position and rotation as a <see cref="Pose"/>.
        /// </summary>
        /// <param name="transform">The transform to read.</param>
        /// <returns>A pose containing world position and world rotation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transform"/> is null.</exception>
        /// <example>
        /// <code>
        /// Pose handPose = handTransform.GetPose();
        /// </code>
        /// </example>
        public static Pose GetPose(this Transform transform) {
            if (!transform) throw new ArgumentNullException(nameof(transform));

            transform.GetPositionAndRotation(out var position, out var rotation);
            return new Pose(position, rotation);
        }

        #endregion
    }
}