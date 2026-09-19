using System;

namespace NFramework.Save
{
    [Serializable]
    public class SaveEntry
    {
        public string Key;
        public int SchemaVersion;
        public byte[] Data;

        public SaveEntry()
        {
        }

        public SaveEntry(string key, int schemaVersion, byte[] data)
        {
            Key = key;
            SchemaVersion = schemaVersion;
            Data = data;
        }
    }
}
