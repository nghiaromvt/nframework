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
                if (!string.IsNullOrEmpty(tsvCachePath))
                    File.WriteAllText(tsvCachePath, tsvText);

                var json = ConvertTSVTextToJsonListObject(tsvText);

                if (!string.IsNullOrEmpty(jsonCachePath))
                    File.WriteAllText(tsvCachePath, json);

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
                request.MoveNext();

            Logger.Log(request.text);
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
                    objResult.Add(properties[j], tsv[i][j]);

                listObjResult.Add(objResult);
            }

            return JsonConvert.SerializeObject(listObjResult);
        }
    }
}