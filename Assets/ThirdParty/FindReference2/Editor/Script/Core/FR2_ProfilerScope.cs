using System;

namespace vietlabs.fr2
{
    internal struct FR2_ProfilerScope : IDisposable
    {
        private readonly bool _isActive;
        
        public FR2_ProfilerScope(string sampleName)
        {
            _isActive = !string.IsNullOrEmpty(sampleName);
            if (_isActive)
            {
                UnityEngine.Profiling.Profiler.BeginSample(sampleName);
            }
        }
        
        public void Dispose()
        {
            if (_isActive)
            {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
    }
    
    internal static class FR2_ProfilerExtensions
    {
        internal static void WithProfiler(this Action action, string sampleName)
        {
            if (action == null) return;
            using (new FR2_ProfilerScope(sampleName))
            {
                action.Invoke();
            }
        }
        
        internal static T WithProfiler<T>(this Func<T> func, string sampleName)
        {
            if (func == null) return default(T);
            using (new FR2_ProfilerScope(sampleName))
            {
                return func.Invoke();
            }
        }
        
        internal static string CreateSampleName(this string operation, string context = null)
        {
            if (string.IsNullOrEmpty(operation)) return null;
            string baseName = $"FR2.{operation}";
            return string.IsNullOrEmpty(context) ? baseName : $"{baseName}.{context}";
        }
    }
    
    internal static class FR2_Samples
    {
        internal const string AssetLoadContent = "FR2.Asset.LoadContent";
        internal const string AssetLoadFileInfo = "FR2.Asset.LoadFileInfo";
        internal const string CacheAsyncLoadContent = "FR2.Cache.AsyncLoadContent";
        internal const string GetAllObjectReferencesSerializedObject = "FR2.GetAllObjectReferences.SerializedObject";
        internal const string GetAllObjectReferencesComponent = "FR2.GetAllObjectReferences.Component";
        internal const string GetAllObjectReferencesGameObject = "FR2.GetAllObjectReferences.GameObject";
        internal const string CollectObjectReferenceProperties = "FR2.CollectObjectReferenceProperties";
        internal const string SceneCacheScanComponents = "FR2.SceneCache.ScanComponents";
        internal const string SceneCacheScanComponent = "FR2.SceneCache.ScanComponent";
        internal const string FileCompareReadChunk = "FR2.FileCompare.ReadChunk";
    }
}
