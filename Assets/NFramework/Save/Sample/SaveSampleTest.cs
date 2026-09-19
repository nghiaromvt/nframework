using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Sample
{
    public class SaveSampleTest : MonoBehaviour, ISaveable
    {
        [Serializable]
        public class Data
        {
            public string PlayerName = "Player";
            public int Level = 1;
            public int Coins = 0;
        }

        [SerializeField] private Data _data = new();
        private LocalSaveManager _saveManager;

        public string SaveKey => "PlayerProfile";
        public int SchemaVersion => 1;
        public bool IsDirty { get; private set; }
        public IReadOnlyList<IMigration> Migrations => null;

        private void Awake()
        {
            if (!ServiceLocator.TryGet(out ISaveService _))
            {
                _saveManager = new LocalSaveManager(autoLoad: true);
                ServiceLocator.Register<ISaveService>(_saveManager);
            }
        }

        private void Start()
        {
            // Register to the save service
            if (ServiceLocator.TryGet(out ISaveService saveService))
            {
                saveService.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out ISaveService saveService))
            {
                saveService.UnRegister(this);
            }

            _saveManager?.Dispose();
        }

        [Button]
        public void AddCoins(int amount)
        {
            _data.Coins += amount;
            IsDirty = true; // Mark dirty so save will write to disk
        }

        public byte[] CaptureState(ISerializer serializer)
        {
            return serializer.Serialize(_data);
        }

        public void RestoreState(ISerializer serializer, byte[] data)
        {
            _data = serializer.Deserialize<Data>(data) ?? new Data();
        }

        public void SetClean()
        {
            IsDirty = false;
        }
    }
}
