using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace NFramework
{
    public static class TsvParser
    {
        private const int WEB_TIMEOUT_SECONDS = 30;
        private static readonly Regex GoogleSheetIdRegex = new(@"/spreadsheets/d/([a-zA-Z0-9-_]+)", RegexOptions.Compiled);
        private static readonly Regex GoogleSheetGidRegex = new(@"[#&?]gid=([0-9]+)", RegexOptions.Compiled);

        public static List<T> Parse<T>(string tsvContent) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(tsvContent))
            {
                return new List<T>();
            }

            var lines = tsvContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int headerLineIndex = -1;
            string[] headers = null;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                headers = lines[i].Split('\t');
                headerLineIndex = i;
                break;
            }

            if (headers == null || headers.Length == 0)
            {
                return new List<T>();
            }

            for (int i = 0; i < headers.Length; i++)
            {
                headers[i] = headers[i].Trim();
            }

            var memberSetters = BuildMemberSetters<T>(headers);
            var result = new List<T>();

            for (int i = headerLineIndex + 1; i < lines.Length; i++)
            {
                var rawLine = lines[i];
                var trimmedLine = rawLine.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                var cells = rawLine.Split('\t');
                var item = new T();

                for (int col = 0; col < headers.Length; col++)
                {
                    var setter = memberSetters[col];
                    if (setter == null)
                    {
                        continue;
                    }

                    var cellValue = col < cells.Length ? cells[col].Trim() : string.Empty;
                    try
                    {
                        setter(item, cellValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TsvParser] Error at row {i + 1}, column '{headers[col]}': failed to parse '{cellValue}'. Details: {ex.Message}");
                        throw;
                    }
                }

                result.Add(item);
            }

            return result;
        }

        public static string NormalizeGoogleSheetUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            if (!url.Contains("docs.google.com/spreadsheets"))
            {
                return url;
            }

            if (url.Contains("output=tsv") || url.Contains("format=tsv"))
            {
                return url;
            }

            if (url.Contains("/d/e/"))
            {
                var pubMatch = Regex.Match(url, @"/spreadsheets/d/e/([a-zA-Z0-9-_]+)");
                if (pubMatch.Success)
                {
                    var token = pubMatch.Groups[1].Value;
                    return $"https://docs.google.com/spreadsheets/d/e/{token}/pub?output=tsv";
                }
            }

            var idMatch = GoogleSheetIdRegex.Match(url);
            if (!idMatch.Success || idMatch.Groups[1].Value == "e")
            {
                return url;
            }

            var sheetId = idMatch.Groups[1].Value;
            var gidMatch = GoogleSheetGidRegex.Match(url);
            var gid = gidMatch.Success ? gidMatch.Groups[1].Value : "0";

            return $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
        }

        public static bool IsWebUrl(string pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
            {
                return false;
            }

            return pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        public static string FetchContent(string pathOrUrl)
        {
            if (IsWebUrl(pathOrUrl))
            {
                var normalizedUrl = NormalizeGoogleSheetUrl(pathOrUrl);
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(WEB_TIMEOUT_SECONDS);
                var response = httpClient.GetAsync(normalizedUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            if (!File.Exists(pathOrUrl))
            {
                Debug.LogError($"[TsvParser] File not found: {pathOrUrl}");
                throw new FileNotFoundException($"File not found: {pathOrUrl}");
            }

            return File.ReadAllText(pathOrUrl);
        }

        public static async Task<string> FetchContentAsync(string pathOrUrl)
        {
            if (IsWebUrl(pathOrUrl))
            {
                var normalizedUrl = NormalizeGoogleSheetUrl(pathOrUrl);
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(WEB_TIMEOUT_SECONDS);
                var response = await httpClient.GetAsync(normalizedUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }

            if (!File.Exists(pathOrUrl))
            {
                Debug.LogError($"[TsvParser] File not found: {pathOrUrl}");
                throw new FileNotFoundException($"File not found: {pathOrUrl}");
            }

            return await Task.Run(() => File.ReadAllText(pathOrUrl));
        }

        public static void SaveToCache(string cachePath, string content)
        {
            if (string.IsNullOrWhiteSpace(cachePath))
            {
                return;
            }

            var directory = Path.GetDirectoryName(cachePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(cachePath, content);
        }

        private static Action<T, string>[] BuildMemberSetters<T>(string[] headers) where T : class
        {
            var targetType = typeof(T);
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var fields = targetType.GetFields(bindingFlags);
            var properties = targetType.GetProperties(bindingFlags);

            var setters = new Action<T, string>[headers.Length];

            for (int i = 0; i < headers.Length; i++)
            {
                var header = headers[i];
                if (string.IsNullOrEmpty(header))
                {
                    continue;
                }

                FieldInfo matchedField = null;
                PropertyInfo matchedProperty = null;

                foreach (var field in fields)
                {
                    if (!field.IsPublic && !field.IsDefined(typeof(SerializeField), true))
                    {
                        continue;
                    }

                    var columnAttr = field.GetCustomAttribute<TSVColumnAttribute>(true);
                    if (columnAttr != null && columnAttr.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedField = field;
                        break;
                    }

                    if (field.Name.Equals(header, StringComparison.OrdinalIgnoreCase) ||
                        field.Name.TrimStart('_').Equals(header, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedField = field;
                    }
                }

                if (matchedField == null)
                {
                    foreach (var prop in properties)
                    {
                        if (!prop.CanWrite)
                        {
                            continue;
                        }

                        var columnAttr = prop.GetCustomAttribute<TSVColumnAttribute>(true);
                        if (columnAttr != null && columnAttr.Name.Equals(header, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedProperty = prop;
                            break;
                        }

                        if (prop.Name.Equals(header, StringComparison.OrdinalIgnoreCase) ||
                            prop.Name.TrimStart('_').Equals(header, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedProperty = prop;
                        }
                    }
                }

                if (matchedField != null)
                {
                    var field = matchedField;
                    setters[i] = (instance, rawValue) =>
                    {
                        var converted = ConvertValue(rawValue, field.FieldType);
                        field.SetValue(instance, converted);
                    };
                }
                else if (matchedProperty != null)
                {
                    var prop = matchedProperty;
                    setters[i] = (instance, rawValue) =>
                    {
                        var converted = ConvertValue(rawValue, prop.PropertyType);
                        prop.SetValue(instance, converted);
                    };
                }
            }

            return setters;
        }

        private static object ConvertValue(string rawValue, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return rawValue;
            }

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                if (targetType.IsValueType)
                {
                    return Activator.CreateInstance(targetType);
                }

                return null;
            }

            if (targetType == typeof(int))
            {
                return int.Parse(rawValue, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(long))
            {
                return long.Parse(rawValue, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(float))
            {
                return float.Parse(rawValue, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(double))
            {
                return double.Parse(rawValue, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(decimal))
            {
                return decimal.Parse(rawValue, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool))
            {
                var lower = rawValue.ToLowerInvariant();
                return lower switch
                {
                    "true" or "1" or "yes" => true,
                    "false" or "0" or "no" => false,
                    _ => throw new FormatException($"Cannot convert '{rawValue}' to boolean.")
                };
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, rawValue, true);
            }

            if (targetType.IsArray)
            {
                var elementType = targetType.GetElementType();
                var items = SplitDelimited(rawValue);
                var array = Array.CreateInstance(elementType, items.Length);

                for (int i = 0; i < items.Length; i++)
                {
                    array.SetValue(ConvertValue(items[i], elementType), i);
                }

                return array;
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = targetType.GetGenericArguments()[0];
                var items = SplitDelimited(rawValue);
                var list = (IList)Activator.CreateInstance(targetType);

                for (int i = 0; i < items.Length; i++)
                {
                    list.Add(ConvertValue(items[i], elementType));
                }

                return list;
            }

            throw new NotSupportedException($"Type '{targetType.FullName}' is not supported for TSV parsing.");
        }

        private static string[] SplitDelimited(string rawValue)
        {
            char delimiter = rawValue.Contains(';') ? ';' : ',';
            var split = rawValue.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                split[i] = split[i].Trim();
            }

            return split;
        }
    }
}
