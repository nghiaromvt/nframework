using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class DeviceInfo
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

        public static bool IsDevelopment
        {
            get
            {
#if DEVELOPMENT
                return true;
#endif
                return false;
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

        public static bool HasInternet() => Application.internetReachability != NetworkReachability.NotReachable;

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

        private static string _deviceUDID = "";
        public static string GetUDID()
        {
            if (_deviceUDID.Length <= 0)
            {
                _deviceUDID = PlayerPrefs.GetString("didu", string.Empty);
                if (string.IsNullOrEmpty(_deviceUDID))
                {
                    _deviceUDID = SystemInfo.deviceUniqueIdentifier;
                    PlayerPrefs.SetString("didu", _deviceUDID);
                }
            }
            return _deviceUDID;
        }
    }
}

