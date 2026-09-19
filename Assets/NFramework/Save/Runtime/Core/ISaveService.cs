using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace NFramework.Save
{
    public interface ISaveService
    {
        event Action OnBeforeSave;
        event Action OnAfterSave;
        event Action OnBeforeLoad;
        event Action OnAfterLoad;

        IReadOnlyList<ISaveable> RegisteredSaveables { get; }

        void Register(ISaveable saveable);
        void UnRegister(ISaveable saveable);

        void Save();
        UniTask SaveAsync();

        void Load();
        UniTask LoadAsync();

        void DeleteSave();
    }
}
