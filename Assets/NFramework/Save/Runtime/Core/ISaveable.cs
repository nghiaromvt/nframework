using System.Collections.Generic;

namespace NFramework.Save
{
    public interface ISaveable
    {
        string SaveKey { get; }
        int SchemaVersion { get; }
        bool IsDirty { get; }
        IReadOnlyList<IMigration> Migrations { get; }

        byte[] CaptureState(ISerializer serializer);
        void RestoreState(ISerializer serializer, byte[] data);
        void SetClean();
    }
}
