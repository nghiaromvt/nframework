using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NFramework
{
    public static class GoogleSheetHelper
    {
        public static void GetConfig<T>(string sheetId, string gridId, Action<List<T>> callback, string tsvCachePath = null, string jsonCachePath = null)
        {
            GetSheetTSVText(sheetId, gridId, tsvText =>
            {
                TryWriteTextToFile(tsvCachePath, tsvText);
                var json = ConvertTSVTextToJsonListObject(tsvText);
                TryWriteTextToFile(jsonCachePath, json);
                callback?.Invoke(JsonConvert.DeserializeObject<List<T>>(json));
            });
        }

        public static void GetSheetTSVText(string sheetId, string gridId, Action<string> callback)
        {
            string url = $@"https://docs.google.com/spreadsheets/d/{sheetId}/export?gid={gridId}&format=tsv";
            LoadTextFromWeb(url, callback);
        }

        private static void LoadTextFromWeb(string url, Action<string> callBack)
        {
            WWW request = new WWW(url);
            while (!request.isDone)
            {
                request.MoveNext();
            }
            callBack?.Invoke(request.text);
        }

        public static string ConvertTSVTextToJsonListObject(string tsvText)
        {
            var tsv = new List<string[]>();
            var lines = tsvText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (string line in lines)
                tsv.Add(line.Split('\t'));

            var properties = lines[0].Split('\t');
            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                {
                    var cellText = tsv[i][j];
                    if (!string.IsNullOrEmpty(cellText))
                        objResult.Add(properties[j], tsv[i][j]);
                }

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }

        private static void TryWriteTextToFile(string path, string text)
        {
            if (string.IsNullOrEmpty(path))
            {
                Logger.Log(text);
                return;
            }

            if (!File.Exists(path))
            {
                var fs = new FileStream(path, FileMode.Create);
                fs.Dispose();
            }

            File.WriteAllText(path, text);
        }
    }
}
