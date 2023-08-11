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
                    return Screen.height / Screen.width >= 2f;
                else // Landscape
                    return Screen.width / Screen.height >= 2f;
            }
        }

        public static bool IsIpad
        {
            get
            {
                if (Screen.height > Screen.width) // Portrait
                    return Screen.height / Screen.width <= 1.775f;
                else // Landscape
                    return Screen.width / Screen.height <= 1.775f;
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
    }
}

