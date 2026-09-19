using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class LocalSaveManagerTests
    {
        [Serializable]
        private class TestGameData
        {
            public int Coins;
            public string Name;
        }

        private class MockSaveableEntity : ISaveable
        {
            public string SaveKey => "TestEntity";
            public int SchemaVersion => 1;
            public bool IsDirty { get; set; }
            public IReadOnlyList<IMigration> Migrations => null;

            public TestGameData Data = new TestGameData();

            public byte[] CaptureState(ISerializer serializer)
            {
                return serializer.Serialize(Data);
            }

            public void RestoreState(ISerializer serializer, byte[] data)
            {
                Data = serializer.Deserialize<TestGameData>(data) ?? new TestGameData();
            }

            public void SetClean()
            {
                IsDirty = false;
            }
        }

        private GameObject _lifecycleObject;
        private LocalSaveManager _saveManager;
        private string _testDir;
        private IStorageService _storageService;
        private ISerializer _serializer;
        private IEncryptor _encryptor;
        private IChecksumGenerator _checksumGenerator;

        [SetUp]
        public void SetUp()
        {
            if (!ApplicationLifecycle.HasInstance)
            {
                _lifecycleObject = new GameObject("[ApplicationLifecycle]");
                _lifecycleObject.AddComponent<ApplicationLifecycle>();
            }

            _testDir = Path.Combine(Application.temporaryCachePath, "ManagerTests_" + Guid.NewGuid().ToString("N"));
            _storageService = new FileStorageService(_testDir);
            _serializer = new OdinSerializerHandler();
            _encryptor = new AesEncryptor("TestSecretKey2026!");
            _checksumGenerator = new Sha256ChecksumGenerator();

            _saveManager = new LocalSaveManager(
                autoLoad: false,
                autoSaveOnQuit: true,
                autoSaveOnInterrupt: true,
                serializer: _serializer,
                encryptor: _encryptor,
                checksumGenerator: _checksumGenerator,
                storageService: _storageService);
        }

        [TearDown]
        public void TearDown()
        {
            _saveManager?.Dispose();

            if (_lifecycleObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_lifecycleObject);
                _lifecycleObject = null;
            }

            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void SaveAndLoad_RestoresDataCorrectly()
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 500, Name = "Alice" };
            entity.IsDirty = true;

            _saveManager.Save();

            Assert.IsFalse(entity.IsDirty);
            Assert.IsTrue(_storageService.Exists("gamedata.sav"));

            MockSaveableEntity restoredEntity = new MockSaveableEntity();
            _saveManager.Register(restoredEntity);

            Assert.AreEqual(500, restoredEntity.Data.Coins);
            Assert.AreEqual("Alice", restoredEntity.Data.Name);
        }

        [UnityTest]
        public System.Collections.IEnumerator SaveAsync_And_LoadAsync_RestoresDataCorrectly() => UniTask.ToCoroutine(async () =>
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 999, Name = "Bob" };
            entity.IsDirty = true;

            await _saveManager.SaveAsync();

            Assert.IsFalse(entity.IsDirty);
            Assert.IsTrue(_storageService.Exists("gamedata.sav"));

            MockSaveableEntity restoredEntity = new MockSaveableEntity();
            _saveManager.Register(restoredEntity);

            Assert.AreEqual(999, restoredEntity.Data.Coins);
            Assert.AreEqual("Bob", restoredEntity.Data.Name);
        });

        [Test]
        public void Save_WhenNotDirty_DoesNotWriteFile()
        {
            MockSaveableEntity entity = new MockSaveableEntity
            {
                IsDirty = false,
                Data = new TestGameData { Coins = 100, Name = "NotDirty" }
            };

            _saveManager.Register(entity);
            _saveManager.Save();

            Assert.IsFalse(_storageService.Exists("gamedata.sav"));
        }

        [Test]
        public void ServiceLocator_RegistersAndResolvesSaveService()
        {
            ServiceLocator.Register<ISaveService>(_saveManager);
            bool found = ServiceLocator.TryGet(out ISaveService saveService);
            Assert.IsTrue(found);
            Assert.AreSame(_saveManager, saveService);
        }

        [Test]
        public void CorruptedSaveFile_FallsBackToBackup()
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 100, Name = "SaveV1" };
            entity.IsDirty = true;
            _saveManager.Save(); // Save 1 (creates main file)

            entity.Data = new TestGameData { Coins = 200, Name = "SaveV2" };
            entity.IsDirty = true;
            _saveManager.Save(); // Save 2 (moves Save 1 to backup, writes Save 2 to main)

            Assert.IsTrue(_storageService.HasBackup("gamedata.sav"));

            // Tamper main file bytes directly without rotating backup
            string mainPath = Path.Combine(_testDir, "gamedata.sav");
            byte[] mainBytes = File.ReadAllBytes(mainPath);
            mainBytes[mainBytes.Length - 1] ^= 0xFF; // Corrupt payload byte
            File.WriteAllBytes(mainPath, mainBytes);

            LogAssert.Expect(LogType.Error, "Failed to unpack save data: Save data checksum verification failed (corrupted file).");

            // Create new manager to simulate restart and loading corrupted save
            using (LocalSaveManager newManager = new LocalSaveManager(
                autoLoad: true,
                autoSaveOnQuit: false,
                autoSaveOnInterrupt: false,
                serializer: _serializer,
                encryptor: _encryptor,
                checksumGenerator: _checksumGenerator,
                storageService: _storageService))
            {
                MockSaveableEntity restoredEntity = new MockSaveableEntity();
                newManager.Register(restoredEntity);

                // Should fallback to SaveV1 from backup!
                Assert.AreEqual(100, restoredEntity.Data.Coins);
                Assert.AreEqual("SaveV1", restoredEntity.Data.Name);
            }
        }

        [Test]
        public void SaveAndLoad_TriggersBeforeAndAfterEvents()
        {
            int beforeSaveCount = 0;
            int afterSaveCount = 0;
            int beforeLoadCount = 0;
            int afterLoadCount = 0;

            _saveManager.OnBeforeSave += () => beforeSaveCount++;
            _saveManager.OnAfterSave += () => afterSaveCount++;
            _saveManager.OnBeforeLoad += () => beforeLoadCount++;
            _saveManager.OnAfterLoad += () => afterLoadCount++;

            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 50, Name = "EventTester" };
            entity.IsDirty = true;
            _saveManager.Save();

            Assert.AreEqual(1, beforeSaveCount);
            Assert.AreEqual(1, afterSaveCount);

            _saveManager.Load();

            Assert.AreEqual(1, beforeLoadCount);
            Assert.AreEqual(1, afterLoadCount);
        }

        [Test]
        public void Load_WhenNoSaveFileExists_RestoresDefaultStateAndInvokesOnAfterLoad()
        {
            int beforeLoadCount = 0;
            int afterLoadCount = 0;
            _saveManager.OnBeforeLoad += () => beforeLoadCount++;
            _saveManager.OnAfterLoad += () => afterLoadCount++;

            MockSaveableEntity entity = new MockSaveableEntity
            {
                Data = new TestGameData { Coins = 123, Name = "ModifiedBeforeLoad" }
            };

            _saveManager.Register(entity);
            // Re-modify to simulate changed state
            entity.Data = new TestGameData { Coins = 999, Name = "Changed" };

            _saveManager.Load();

            Assert.AreEqual(1, beforeLoadCount);
            Assert.AreEqual(1, afterLoadCount);
            Assert.AreEqual(0, entity.Data.Coins);
            Assert.IsNull(entity.Data.Name);
        }

        [UnityTest]
        public System.Collections.IEnumerator LoadAsync_WhenNoSaveFileExists_RestoresDefaultStateAndInvokesOnAfterLoad() => UniTask.ToCoroutine(async () =>
        {
            int beforeLoadCount = 0;
            int afterLoadCount = 0;
            _saveManager.OnBeforeLoad += () => beforeLoadCount++;
            _saveManager.OnAfterLoad += () => afterLoadCount++;

            MockSaveableEntity entity = new MockSaveableEntity
            {
                Data = new TestGameData { Coins = 999, Name = "Changed" }
            };

            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 999, Name = "Changed" };

            await _saveManager.LoadAsync();

            Assert.AreEqual(1, beforeLoadCount);
            Assert.AreEqual(1, afterLoadCount);
            Assert.AreEqual(0, entity.Data.Coins);
            Assert.IsNull(entity.Data.Name);
        });

        [Test]
        public void ApplicationLifecycle_Pause_TriggersAutoSave()
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 777, Name = "PauseSave" };
            entity.IsDirty = true;

            _saveManager.OnApplicationPause(true);

            Assert.IsFalse(entity.IsDirty);
            Assert.IsTrue(_storageService.Exists("gamedata.sav"));
        }

        [Test]
        public void ApplicationLifecycle_FocusLost_TriggersAutoSave()
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 888, Name = "FocusSave" };
            entity.IsDirty = true;

            _saveManager.OnApplicationFocus(false);

            Assert.IsFalse(entity.IsDirty);
            Assert.IsTrue(_storageService.Exists("gamedata.sav"));
        }

        [Test]
        public void ApplicationLifecycle_Quit_TriggersAutoSave()
        {
            MockSaveableEntity entity = new MockSaveableEntity();
            _saveManager.Register(entity);
            entity.Data = new TestGameData { Coins = 999, Name = "QuitSave" };
            entity.IsDirty = true;

            _saveManager.OnApplicationQuit();

            Assert.IsFalse(entity.IsDirty);
            Assert.IsTrue(_storageService.Exists("gamedata.sav"));
        }
    }
}
