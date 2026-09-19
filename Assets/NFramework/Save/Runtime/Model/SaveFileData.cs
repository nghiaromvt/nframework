using System;
using System.Collections.Generic;

namespace NFramework.Save
{
    [Serializable]
    public class SaveFileData
    {
        public Dictionary<string, SaveEntry> Entries = new();
    }
}
