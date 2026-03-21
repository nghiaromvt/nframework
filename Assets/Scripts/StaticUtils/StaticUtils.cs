using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NFramework
{
    public static partial class StaticUtils
    {
        #region character

        public static bool IsDigitCharacter(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool IsLowercaseAlphabetCharacter(char c)
        {
            return c >= 'a' && c <= 'z';
        }

        public static bool IsUppercaseAlphabetCharacter(char c)
        {
            return c >= 'A' && c <= 'Z';
        }

        public static bool IsAlphabetCharacter(char c)
        {
            return IsLowercaseAlphabetCharacter(c) || IsUppercaseAlphabetCharacter(c);
        }

        #endregion

        #region exception

        public static string AggregateExceptionToString(AggregateException aggE)
        {
            var flatE = aggE.Flatten();
            var sb = new StringBuilder();
            for (var i = 1; i <= flatE.InnerExceptions.Count; i++)
            {
                sb.Append($"exception {i}={flatE.InnerExceptions[i - 1].Message}");
                if (i < flatE.InnerExceptions.Count)
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        public static void RethrowException(Exception e)
        {
            ExceptionDispatchInfo.Capture(e).Throw();
        }

        #endregion

        #region json

        public static string JsonSerializeToFriendlyText(object obj)
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.Converters.Add(new StringEnumConverter());
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T CastJsonValue<T>(object jsonValue)
        {
            if (jsonValue is JObject)
            {
                var jo = (JObject)jsonValue;
                return jo.ToObject<T>();
            }
            else if (jsonValue is JArray)
            {
                var ja = (JArray)jsonValue;
                return ja.ToObject<T>();
            }
            else //string, number, boolean
            {
                return (T)jsonValue;
            }
        }

        public static string FormatJson(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }

        #endregion

        #region random

        public static string RandomAnUID(int length,
            bool hasNumber = true, bool hasLowercase = true, bool hasUppercase = true)
        {
            var listChars = new List<char>();
            if (hasNumber)
            {
                for (var c = '0'; c <= '9'; c++)
                {
                    listChars.Add(c);
                }
            }
            if (hasLowercase)
            {
                for (var c = 'a'; c <= 'z'; c++)
                {
                    listChars.Add(c);
                }
            }
            if (hasUppercase)
            {
                for (var c = 'A'; c <= 'Z'; c++)
                {
                    listChars.Add(c);
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var rNumber = UnityEngine.Random.Range(0, listChars.Count);
                sb.Append(listChars[rNumber]);
            }
            return sb.ToString();
        }

        public static byte[] RandomAByteArray(int length)
        {
            var result = new byte[length];
            for (var i = 0; i < length; i++)
            {
                result[i] = (byte)UnityEngine.Random.Range(0, byte.MaxValue + 1);
            }
            return result;
        }

        public static Color RandomColor()
        {
            var r = UnityEngine.Random.Range(0f, 1f);
            var g = UnityEngine.Random.Range(0f, 1f);
            var b = UnityEngine.Random.Range(0f, 1f);
            return new Color(r, g, b, 1);
        }

        #endregion

        #region xor encryption

        public static string XorString(string str, string key)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                sb.Append((char)(str[i] ^ key[i % key.Length]));
            }
            return sb.ToString();
        }

        public static byte[] XorByteArray(byte[] arr, byte[] key)
        {
            var result = new byte[arr.Length];
            for (var i = 0; i < arr.Length; i++)
            {
                result[i] = (byte)(arr[i] ^ key[i % key.Length]);
            }
            return result;
        }

        #endregion

        #region screen resolution

        public static float ScreenToWorldDistance(float screenDist, Camera camera)
        {
            var screenHeight = (float)Screen.height;
            var worldHeight = camera.orthographicSize * 2;
            var ratio = worldHeight / screenHeight;
            return screenDist * ratio;
        }

        public static Vector2 GetScreenSizeInWorld(Camera camera)
        {
            var worldWidth = ScreenToWorldDistance(Screen.width, camera);
            var worldHeight = ScreenToWorldDistance(Screen.height, camera);
            return new Vector2(worldWidth, worldHeight);
        }

        public static Vector2 GetSafeScreenSizeInWorld(Camera camera)
        {
            var worldWidth = ScreenToWorldDistance(Screen.safeArea.size.x, camera);
            var worldHeight = ScreenToWorldDistance(Screen.safeArea.size.y, camera);
            return new Vector2(worldWidth, worldHeight);
        }

        public static float GetSafeTopOffset()
        {
            return Screen.height - Screen.safeArea.y - Screen.safeArea.height;
        }

        public static float GetSafeBottomOffset()
        {
            return Screen.safeArea.y;
        }

        public static float GetSafeTopOffsetInWorld(Camera camera)
        {
            return ScreenToWorldDistance(GetSafeTopOffset(), camera);
        }

        public static float GetSafeBottomOffsetInWorld(Camera camera)
        {
            return ScreenToWorldDistance(GetSafeBottomOffset(), camera);
        }

        #endregion

        #region other utils

        public static void CopyToClipboard(string txt)
        {
            GUIUtility.systemCopyBuffer = txt;
        }

        public static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        public static Texture2D CreateColorTexture(Color color)
        {
            var pix = new Color[1];
            pix[0] = color;

            var result = new Texture2D(1, 1);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public static bool IsAppRunning(string appName)
        {
            return Process.GetProcessesByName(appName).Length > 0;
        }

        public static void SetFPS(int fps)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return;
#endif

            Application.targetFrameRate = fps;
        }

        public static bool IsClickOnUI(EventSystem eventSystem, Vector3 clickedPos, List<string> tagsToExclude = null)
        {
            var data = new PointerEventData(eventSystem)
            {
                position = clickedPos,
            };
            var result = new List<RaycastResult>();
            eventSystem.RaycastAll(data, result);

            var numHits = 0;
            foreach (var i in result)
            {
                if (tagsToExclude == null || !tagsToExclude.Contains(i.gameObject.tag))
                {
                    numHits++;
                }
            }

            return numHits > 0;
        }

        #endregion
    }
}