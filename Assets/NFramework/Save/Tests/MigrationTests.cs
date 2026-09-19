using System;
using System.Collections.Generic;
using NUnit.Framework;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class MigrationTests
    {
        [Serializable]
        private class PlayerDataV1
        {
            public string Name;
        }

        [Serializable]
        private class PlayerDataV2
        {
            public string Name;
            public int Level;
        }

        [Serializable]
        private class PlayerDataV3
        {
            public string FullName;
            public int Level;
            public int Gold;
        }

        private class MigrationV1ToV2 : IMigration
        {
            public int FromVersion => 1;
            public int ToVersion => 2;

            public byte[] Migrate(byte[] oldData, ISerializer serializer)
            {
                PlayerDataV1 v1 = serializer.Deserialize<PlayerDataV1>(oldData);
                PlayerDataV2 v2 = new PlayerDataV2
                {
                    Name = v1.Name,
                    Level = 1
                };
                return serializer.Serialize(v2);
            }
        }

        private class MigrationV2ToV3 : IMigration
        {
            public int FromVersion => 2;
            public int ToVersion => 3;

            public byte[] Migrate(byte[] oldData, ISerializer serializer)
            {
                PlayerDataV2 v2 = serializer.Deserialize<PlayerDataV2>(oldData);
                PlayerDataV3 v3 = new PlayerDataV3
                {
                    FullName = v2.Name,
                    Level = v2.Level,
                    Gold = 100
                };
                return serializer.Serialize(v3);
            }
        }

        private class MockSaveable : ISaveable
        {
            public string SaveKey => "PlayerProfile";
            public int SchemaVersion { get; set; } = 3;
            public bool IsDirty { get; private set; }
            public IReadOnlyList<IMigration> Migrations { get; set; } = new List<IMigration>
            {
                new MigrationV1ToV2(),
                new MigrationV2ToV3()
            };

            public PlayerDataV3 State;

            public byte[] CaptureState(ISerializer serializer)
            {
                return serializer.Serialize(State);
            }

            public void RestoreState(ISerializer serializer, byte[] data)
            {
                State = serializer.Deserialize<PlayerDataV3>(data);
            }

            public void SetClean()
            {
                IsDirty = false;
            }
        }

        private ISerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new OdinSerializerHandler();
        }

        [Test]
        public void Migrate_FromV1ToV3_ChainsMigrationsSuccessfully()
        {
            PlayerDataV1 v1Data = new PlayerDataV1 { Name = "Warrior" };
            byte[] v1Bytes = _serializer.Serialize(v1Data);
            SaveEntry entry = new SaveEntry("PlayerProfile", 1, v1Bytes);

            MockSaveable saveable = new MockSaveable();

            byte[] migratedBytes = SchemaMigrationRunner.Migrate(saveable, entry, _serializer);
            saveable.RestoreState(_serializer, migratedBytes);

            Assert.IsNotNull(saveable.State);
            Assert.AreEqual("Warrior", saveable.State.FullName);
            Assert.AreEqual(1, saveable.State.Level);
            Assert.AreEqual(100, saveable.State.Gold);
        }

        [Test]
        public void Migrate_NewerVersion_ThrowsException()
        {
            SaveEntry entry = new SaveEntry("PlayerProfile", 99, new byte[4]);
            MockSaveable saveable = new MockSaveable { SchemaVersion = 3 };

            Assert.Throws<InvalidOperationException>(() =>
                SchemaMigrationRunner.Migrate(saveable, entry, _serializer));
        }

        [Test]
        public void Migrate_MissingStep_ThrowsException()
        {
            SaveEntry entry = new SaveEntry("PlayerProfile", 1, new byte[4]);
            MockSaveable saveable = new MockSaveable
            {
                SchemaVersion = 3,
                Migrations = new List<IMigration> { new MigrationV1ToV2() } // Missing V2ToV3
            };

            Assert.Throws<InvalidOperationException>(() =>
                SchemaMigrationRunner.Migrate(saveable, entry, _serializer));
        }
    }
}
