using UnityEngine;

namespace NFramework
{
    public class Singleton<T> where T : class, new()
    {
        private static T _i;

        public static T I => _i ??= new T();
        public static bool HasInstance => _i != null;

        static Singleton()
        {
            SingletonResetRegistry.Register(ResetStatic);
        }

        private static void ResetStatic()
        {
            _i = null;
        }
    }
}
