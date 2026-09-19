using System.Collections.Generic;
using UnityEditor;

namespace vietlabs.fr2
{
    internal static class FR2_AssetDatabaseExtensions
    {
        internal static string ToAssetPath(this string guid)
        {
            if (string.IsNullOrEmpty(guid)) return string.Empty;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? string.Empty : path;
        }
        
        internal static string ToGUID(this string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return string.Empty;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return string.IsNullOrEmpty(guid) ? string.Empty : guid;
        }
        
        internal static bool IsValidGUID(this string guid)
        {
            if (string.IsNullOrEmpty(guid)) return false;
            return !string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid));
        }
        
        internal static bool IsValidAssetPath(this string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return false;
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(assetPath));
        }
        
        internal static string ToValidGUID(this string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return null;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return string.IsNullOrEmpty(guid) ? null : guid;
        }
        
        internal static string ToValidPath(this string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            string path = AssetDatabase.GUIDToAssetPath(guid);
            return string.IsNullOrEmpty(path) ? null : path;
        }
        
        internal static bool IsUserAsset(this string assetPath)
        {
            return !string.IsNullOrEmpty(assetPath) && assetPath.StartsWith("Assets/");
        }
        
        internal static string ToUserAssetGUID(this string assetPath)
        {
            return assetPath.IsUserAsset() ? assetPath.ToValidGUID() : null;
        }
        
        internal static string[] ToGUIDs(this string[] assetPaths, bool skipInvalid = true)
        {
            if (assetPaths == null) return new string[0];
            var results = new List<string>();
            for (int i = 0; i < assetPaths.Length; i++)
            {
                string guid = assetPaths[i].ToGUID();
                if (!skipInvalid || !string.IsNullOrEmpty(guid))
                {
                    results.Add(guid);
                }
            }
            return results.ToArray();
        }
        
        internal static string[] ToAssetPaths(this string[] guids, bool skipInvalid = true)
        {
            if (guids == null) return new string[0];
            var results = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = guids[i].ToAssetPath();
                if (!skipInvalid || !string.IsNullOrEmpty(path))
                {
                    results.Add(path);
                }
            }
            return results.ToArray();
        }
    }
    
    internal static class FR2_UnityObjectExtensions
    {
        internal static string GetAssetPath(this UnityEngine.Object obj)
        {
            return obj == null ? string.Empty : AssetDatabase.GetAssetPath(obj);
        }
        
        internal static bool IsAssetValid(this UnityEngine.Object obj)
        {
            return obj != null && AssetDatabase.Contains(obj);
        }
    }
}
