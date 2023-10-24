using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NFramework
{
    public static class PathUtils
    {
        public const string DOWNLOAD_FOLDER_NAME = "Download";
        public const string SAVE_FOLDER_NAME = "Save";
        public const string SCREENSHOT_FOLDER_NAME = "Screenshot";

        public static string GetDataPath()
        {
#if UNITY_EDITOR
            return Application.dataPath.Replace("Assets", "CacheData");
#else
		    return Application.persistentDataPath;
#endif
        }

        public static string GetDownloadFolderPath(bool autoCreate = true)
        {
            var path = Path.Combine(GetDataPath(), DOWNLOAD_FOLDER_NAME);
            if (!Directory.Exists(path) && autoCreate)
                Directory.CreateDirectory(path);

            return path;
        }

        public static string GetSaveFolderPath(bool autoCreate = true)
        {
            var path = Path.Combine(GetDataPath(), SAVE_FOLDER_NAME);
            if (!Directory.Exists(path) && autoCreate)
                Directory.CreateDirectory(path);

            return path;
        }

        public static string GetScreenShotFolderPath(bool autoCreate = true)
        {
            var path = Path.Combine(GetDataPath(), SCREENSHOT_FOLDER_NAME);
            if (!Directory.Exists(path) && autoCreate)
                Directory.CreateDirectory(path);

            return path;
        }

#if UNITY_EDITOR
        public static Dictionary<string, string> GetAssetsPathDictionary(string filter, string[] searchInFolder = null)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets(filter, searchInFolder);
            var pathDic = new Dictionary<string, string>();
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (pathDic.ContainsKey(fileName))
                    Logger.LogWarning($"Already have key [{fileName}] => ignore");
                else
                    pathDic.Add(fileName, path);
            }
            return pathDic;
        }
#endif
    }
}
