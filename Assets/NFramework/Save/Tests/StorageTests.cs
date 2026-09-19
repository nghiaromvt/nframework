using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class StorageTests
    {
        private string _testDir;
        private FileStorageService _fileStorage;
        private PlayerPrefsStorageService _prefStorage;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Application.temporaryCachePath, "SaveTests_" + System.Guid.NewGuid().ToString("N"));
            _fileStorage = new FileStorageService(_testDir);
            _prefStorage = new PlayerPrefsStorageService();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void FileStorage_WriteAtomicAndRead_ReturnsCorrectBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Test File Content");
            _fileStorage.WriteAtomic("test.sav", data);

            Assert.IsTrue(_fileStorage.Exists("test.sav"));
            byte[] readBytes = _fileStorage.Read("test.sav");
            Assert.AreEqual(data, readBytes);
        }

        [Test]
        public void FileStorage_Overwrite_CreatesBackupFile()
        {
            byte[] v1 = Encoding.UTF8.GetBytes("Version 1");
            byte[] v2 = Encoding.UTF8.GetBytes("Version 2");

            _fileStorage.WriteAtomic("test.sav", v1);
            Assert.IsFalse(_fileStorage.HasBackup("test.sav"));

            _fileStorage.WriteAtomic("test.sav", v2);
            Assert.IsTrue(_fileStorage.HasBackup("test.sav"));

            byte[] current = _fileStorage.Read("test.sav");
            byte[] backup = _fileStorage.ReadBackup("test.sav");

            Assert.AreEqual(v2, current);
            Assert.AreEqual(v1, backup);
        }

        [Test]
        public void FileStorage_Delete_RemovesAllFiles()
        {
            _fileStorage.WriteAtomic("test.sav", new byte[] { 1 });
            _fileStorage.WriteAtomic("test.sav", new byte[] { 2 }); // Creates backup

            _fileStorage.Delete("test.sav");

            Assert.IsFalse(_fileStorage.Exists("test.sav"));
            Assert.IsFalse(_fileStorage.HasBackup("test.sav"));
        }

        [Test]
        public void PlayerPrefsStorage_WriteAndRead_ReturnsCorrectBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("PlayerPrefs Payload");
            _prefStorage.WriteAtomic("pref_test.sav", data);

            Assert.IsTrue(_prefStorage.Exists("pref_test.sav"));
            byte[] readBytes = _prefStorage.Read("pref_test.sav");
            Assert.AreEqual(data, readBytes);

            _prefStorage.Delete("pref_test.sav");
            Assert.IsFalse(_prefStorage.Exists("pref_test.sav"));
        }
    }
}
