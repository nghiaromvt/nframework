using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;

namespace vietlabs.fr2
{
    internal static class FR2_ObjectExtensions
    {
        public static bool IsSceneObject(this UnityObject obj)
        {
            return obj != null && !EditorUtility.IsPersistent(obj) && (obj is GameObject || obj is Component);
        }

        public static bool IsAssetObject(this UnityObject obj)
        {
            return obj != null && EditorUtility.IsPersistent(obj);
        }

        internal static GameObject GetGameObject(this UnityObject target)
        {
            if (!target) return null;
            if (target is GameObject go) return go;
            if (target is Component comp && comp != null) return comp.gameObject;
            return null;
        }

        internal static bool IsValidWithProfiler<T>(this T obj, string profilerSample) where T : UnityObject
        {
            if (obj == null)
            {
                if (!string.IsNullOrEmpty(profilerSample))
                {
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                return false;
            }
            return true;
        }

        internal static bool ValidateOrCallback<T, TResult>(this T obj, Action<TResult> callback, TResult emptyResult) where T : UnityObject
        {
            if (obj == null)
            {
                callback?.Invoke(emptyResult);
                return false;
            }
            return true;
        }

        internal static IEnumerable<GameObject> GetAllChildren(this GameObject target, bool includeTarget = false)
        {
            if (target == null) yield break;
            
            if (includeTarget) yield return target;

            if (target.transform.childCount > 0)
            {
                for (var i = 0; i < target.transform.childCount; i++)
                {
                    var child = target.transform.GetChild(i).gameObject;
                    if (child == null) continue;
                    
                    yield return child;
                    
                    foreach (var grandChild in child.GetAllChildren(false))
                    {
                        yield return grandChild;
                    }
                }
            }
        }
        
        internal static IEnumerable<Transform> GetAllChildTransforms(this Transform root)
        {
            yield return root;
            
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                foreach (var descendant in child.GetAllChildTransforms())
                {
                    yield return descendant;
                }
            }
        }
        
        internal static IEnumerable<GameObject> GetAllGameObjectsInScenes()
        {
            for (var j = 0; j < SceneManager.sceneCount; j++)
            {
                var scene = SceneManager.GetSceneAt(j);
                if (!scene.isLoaded) continue;

                var rootObjects = new List<GameObject>();
                scene.GetRootGameObjects(rootObjects);

                foreach (var rootObject in rootObjects)
                {
                    foreach (var gameObject in rootObject.GetAllChildren(true))
                    {
                        yield return gameObject;
                    }
                }
            }
        }

        private static bool _isDirty = false;
        
        internal static bool IsReferenceDirty => _isDirty;
        
        internal static void ClearReferenceDirtyFlag() => _isDirty = false;
        
        internal static IEnumerable<UnityObject> GetObjectReferences(this SerializedObject serializedObject)
        {
            return serializedObject.CollectObjectReferencesInternal("FR2.GetObjectReferences");
        }
        
        internal static void GetObjectReferencesAsync(this SerializedObject serializedObject, Action<List<UnityObject>> onComplete)
        {
            var properties = CollectObjectReferenceProperties(serializedObject);
            if (properties.Count == 0)
            {
                var emptyList = FR2_CollectionExtensions.GetPooledList<UnityObject>();
                onComplete?.Invoke(emptyList);
                FR2_CollectionExtensions.ReturnPooledList(emptyList);
                return;
            }

            EditorApplication.update += () => ProcessReferencesAsync(properties, onComplete);
        }

        private static IEnumerable<UnityObject> CollectObjectReferencesInternal(this SerializedObject serializedObject, string profilerSample)
        {
            if (serializedObject?.targetObject == null) yield break;

            UnityEngine.Profiling.Profiler.BeginSample(profilerSample);
            try
            {
                var iterator = serializedObject.GetIterator();
                while (iterator.NextVisible(true))
                {
                    if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue != null)
                    {
                        yield return iterator.objectReferenceValue;
                    }
                }
            }
            finally
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private static List<SerializedProperty> CollectObjectReferenceProperties(SerializedObject serializedObject)
        {
            var propertyList = FR2_CollectionExtensions.GetPooledList<SerializedProperty>();
            if (serializedObject == null) return propertyList;

            var iterator = serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    propertyList.Add(iterator.Copy());
                }
            }
            return propertyList;
        }

        private static void ProcessReferencesAsync(List<SerializedProperty> properties, Action<List<UnityObject>> onComplete)
        {
            var resultList = FR2_CollectionExtensions.GetPooledList<UnityObject>();
            foreach (var prop in properties)
            {
                if (prop.objectReferenceValue != null)
                {
                    resultList.Add(prop.objectReferenceValue);
                }
            }
            _isDirty = true;
            onComplete?.Invoke(resultList);
            FR2_CollectionExtensions.ReturnPooledList(properties);
            FR2_CollectionExtensions.ReturnPooledList(resultList);
            EditorApplication.update -= () => ProcessReferencesAsync(properties, onComplete);
        }

        internal static bool IsValidForFR2(this FR2_Asset asset)
        {
            if (asset == null) return false;
            if (asset.IsMissing) return false;
            if (asset.IsFolder) return false;
            if (asset.inPlugins) return false;
            if (asset.inEditor) return false;
            if (!asset.assetPath.StartsWith("Assets/")) return false;
            return true;
        }
        
        internal static bool IsValidAssetPath(this string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && assetPath.StartsWith("Assets/");
        }
        
        internal static bool IsValid(this GameObject gameObject)
        {
            return gameObject != null;
        }
        
        internal static bool ValidateWithWarning(this string value, string warningMessage = null)
        {
            bool isValid = !string.IsNullOrEmpty(value);
            if (!isValid && !string.IsNullOrEmpty(warningMessage))
            {
                FR2_LOG.LogWarning(warningMessage);
            }
            return isValid;
        }
        
        internal static bool ValidateGUID(this string guid, string context = null)
        {
            if (!FR2_Asset.IsValidGUID(guid))
            {
                if (!string.IsNullOrEmpty(context))
                {
                    FR2_LOG.LogWarning($"Invalid GUID in {context}: {guid}");
                }
                return false;
            }
            return true;
        }

    }
}