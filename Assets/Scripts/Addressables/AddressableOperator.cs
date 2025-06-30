#if ADDRESSABLES
using System;

namespace NFramework
{
    public class AddressableOperator
    {
        // Use that for purpose of display progress (downloadedBytes, totalBytes, downloadPercent)
        public Action<float, float, float> OnProgress { get; set; }
        public string Key { get; protected set; }
        public AddressableOperationStatus Status { get; protected set; }
    }
}
#endif