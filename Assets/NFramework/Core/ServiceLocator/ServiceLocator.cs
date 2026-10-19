using System;
using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                throw new InvalidOperationException($"Service of type {type.Name} is already registered in {nameof(ServiceLocator)}!");
            }

            _services.Add(type, service);
        }

        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);
            if (!_services.Remove(type))
            {
                throw new InvalidOperationException($"Cannot unregister service of type {type.Name}: not registered in {nameof(ServiceLocator)}!");
            }
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service of type {type.Name} is not registered in {nameof(ServiceLocator)}!");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);
            if (_services.TryGetValue(type, out object instance))
            {
                service = (T)instance;
                return true;
            }

            service = null;
            return false;
        }

        public static void ClearAll() => _services.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _services.Clear();
        }
    }
}
