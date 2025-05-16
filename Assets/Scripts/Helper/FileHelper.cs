using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace NFramework
{
    public static class FileHelper
    {
        public static bool IsValidFile(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo.Length > 0;
        }

        public static string GetValidFileName(string fileName)
        {
            // remove any invalid character from the filename.
            string ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            return ret.Replace(" ", string.Empty);
        }

        /// <summary>
        /// Deletes the specified directory
        /// </summary>
        public static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        public static long GetDirectorySize(string target_dir)
        {
            if (!Directory.Exists(target_dir))
                return 0;

            long totalSize = 0;
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

            foreach (string dir in dirs)
            {
                totalSize += GetDirectorySize(dir);
            }

            return totalSize;
        }

#if UNITY_EDITOR
        public static T LoadFirstAssetWithName<T>(string assetName, string overrideFilter = null,
            params string[] searchInFolder) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            var filter = overrideFilter ?? $"t:{typeof(T).Name}";
            var paths = GetAssetPaths(filter, searchInFolder);
            foreach (var path in paths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName == assetName)
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            }

            NLogger.LogError($"Cannot find asset with name: {assetName}");
            return null;
        }
        
        public static List<T> LoadAssetsWithType<T>(string overrideFilter = null,
            params string[] searchInFolder) where T : Object
        {
            var filter = overrideFilter ?? $"t:{typeof(T).Name}";
            var paths = GetAssetPaths(filter, searchInFolder);
            var assets = new List<T>();
            paths.ForEach(path =>
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset) 
                    assets.Add(asset);
            });
            return assets;
        }

        public static List<string> GetAssetPaths(string filter, params string[] searchInFolder)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets(filter, searchInFolder);
            return guids.Select(UnityEditor.AssetDatabase.GUIDToAssetPath).ToList();
        }
        
        public static Dictionary<string, List<string>> GetAssetFileNameToPaths(string filter, params string[] searchInFolder)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets(filter, searchInFolder);
            var pathDic = new Dictionary<string, List<string>>();
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (pathDic.TryGetValue(fileName, out var pathList))
                    pathList.Add(path);
                else
                    pathDic.Add(fileName, new List<string> { path });
            }
            return pathDic;
        }
#endif
    }
}
