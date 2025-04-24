using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace NFramework
{
    public enum BuildEnvironment
    {
        Development = 0,
        Staging = 1,
        Production = 2,
    }

    public enum DeviceHardwareLevel
    {
        Low = 0,
        Medium = 1,
        High = 2,
    }
    
    public static class DeviceHelper
    {
        public static bool IsTallPhone
        {
            get
            {
                if (Screen.height > Screen.width) // Portrait
                    return (float)Screen.height / Screen.width >= 2f;
                else // Landscape
                    return (float)Screen.width / Screen.height >= 2f;
            }
        }

        public static bool IsIpad
        {
            get
            {
                if (Screen.height > Screen.width) // Portrait
                    return (float)Screen.height / Screen.width <= 1.775f;
                else // Landscape
                    return (float)Screen.width / Screen.height <= 1.775f;
            }
        }

        public static bool IsAndroid
        {
            get
            {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsIOS
        {
            get
            {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsWebGL
        {
            get
            {
#if UNITY_WEBGL
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsDevelopment => BuildEnvironment == BuildEnvironment.Development;

        public static BuildEnvironment BuildEnvironment
        {
            get
            {
#if DEVELOPMENT
                return BuildEnvironment.Development;
#elif STAGING
                return BuildEnvironment.Staging;
#else
                return BuildEnvironment.Production;
#endif
            }
        }

        public static bool IsNoAds
        {
            get
            {
#if NO_ADS
                return true;
#endif
                return false;
            }
        }

        public static bool IsTestIAP
        {
            get
            {
#if TEST_IAP
                return true;
#endif
                return false;
            }
        }

        public static bool IsNoTracking
        {
            get
            {
#if NO_TRACKING
                return true;
#endif
                return false;
            }
        }

        public static bool NetworkReachabilityStatus() => Application.internetReachability != NetworkReachability.NotReachable;

        public static List<string> GetLocalIPAddress()
        {
            List<string> localIPs = new List<string>();
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    localIPs.Add(ip.ToString());
            }
            return localIPs;
        }

        public static void OpenDeviceWifiSetting()
        {
            try
            {
#if UNITY_ANDROID
                using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.WIFI_SETTINGS"))
                    {
                        currentActivityObject.Call("startActivity", intentObject);
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        
        public static int GetAndroidSdkLevel() {
#if UNITY_ANDROID && !UNITY_EDITOR
            using (var version = new AndroidJavaClass("android.os.Build$VERSION")) 
            {
                return version.GetStatic<int>("SDK_INT");
            }
#else
            return 0;
#endif
        }
        
#if UNITY_EDITOR
        public static List<string> GetScriptingDefinesStringList()
        {
            var scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
            return scriptingDefinesStringList;
        }
#endif

        public static DeviceHardwareLevel GetDeviceHardwareLevel()
        {
            var ramGB = SystemInfo.systemMemorySize / 1024;
            
            if (IsAndroid)
            {
                if (ramGB <= 4)
                    return DeviceHardwareLevel.Low;
                if (ramGB <= 6)
                    return DeviceHardwareLevel.Medium;
            }
            else if (IsIOS)
            {
                if (ramGB <= 2)
                    return DeviceHardwareLevel.Low;
                if (ramGB <= 3)
                    return DeviceHardwareLevel.Medium;
            }
            
            return DeviceHardwareLevel.High;
        }
    }
}

