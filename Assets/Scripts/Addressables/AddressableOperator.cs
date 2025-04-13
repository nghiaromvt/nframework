using System;

namespace NFramework
{
    public class AddressableOperator
    {
        // Use that for purpose of display progress (downloadedBytes, totalBytes, downloadPercent)
        public Action<float, float, float> OnProgress;
        
        public string Key { get; protected set; }
        public EAddressableOperationStatus Status { get; protected set; }
    }
}