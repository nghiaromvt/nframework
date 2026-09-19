using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NFramework.Core.Tests
{
    public class TsvIntegrationTests
    {
        private string _tempTsvFile;
        private string _tempCacheFile;

        [SetUp]
        public void SetUp()
        {
            _tempTsvFile = Path.Combine(Application.temporaryCachePath, "test_items.tsv");
            _tempCacheFile = Path.Combine(Application.temporaryCachePath, "test_cache.tsv");

            var tsvContent =
                "# Sample Items TSV\n" +
                "Item ID\tItem Name\tprice\tIsAvailable\tRarity\tAttributes\n" +
                "# First row\n" +
                "1\tExcalibur\t1000.5\ttrue\tLegendary\tHoly,Slashing\n" +
                "2\tHealing Potion\t25.0\t1\tCommon\tRestoresHP;Drinkable\n";

            File.WriteAllText(_tempTsvFile, tsvContent);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_tempTsvFile))
            {
                File.Delete(_tempTsvFile);
            }

            if (File.Exists(_tempCacheFile))
            {
                File.Delete(_tempCacheFile);
            }
        }

        [Test]
        public void Fetch_LocalFile_PopulatesItemsAndUpdatesMetadata()
        {
            var so = ScriptableObject.CreateInstance<SampleItemTableSO>();
            so.TsvPath = _tempTsvFile;
            so.Category = "Equipment";

            so.Fetch();

            Assert.AreEqual(2, so.Items.Count);

            var item1 = so.Items[0];
            Assert.AreEqual(1, item1.Id);
            Assert.AreEqual("Excalibur", item1.Name);
            Assert.AreEqual(1000.5f, item1.Price, 0.001f);
            Assert.IsTrue(item1.IsAvailable);
            Assert.AreEqual(SampleItemRarity.Legendary, item1.Rarity);
            CollectionAssert.AreEqual(new[] { "Holy", "Slashing" }, item1.Attributes);

            var item2 = so.Items[1];
            Assert.AreEqual(2, item2.Id);
            Assert.AreEqual("Healing Potion", item2.Name);
            Assert.AreEqual(25.0f, item2.Price, 0.001f);
            Assert.IsTrue(item2.IsAvailable);
            Assert.AreEqual(SampleItemRarity.Common, item2.Rarity);
            CollectionAssert.AreEqual(new[] { "RestoresHP", "Drinkable" }, item2.Attributes);

            Assert.IsTrue(so.AfterFetchedCalled);
            Assert.IsFalse(string.IsNullOrEmpty(so.LastSyncedTime));

            Object.DestroyImmediate(so);
        }

        [Test]
        public void Fetch_WithCacheEnabled_WritesCacheFile()
        {
            var so = ScriptableObject.CreateInstance<SampleItemTableSO>();
            so.TsvPath = _tempTsvFile;
            so.EnableCache = true;
            so.CachePath = _tempCacheFile;

            so.Fetch();

            Assert.IsTrue(File.Exists(_tempCacheFile));
            var cachedText = File.ReadAllText(_tempCacheFile);
            Assert.IsTrue(cachedText.Contains("Excalibur"));

            Object.DestroyImmediate(so);
        }

        [Test]
        public async System.Threading.Tasks.Task FetchAsync_LocalFile_PopulatesItemsAndUpdatesMetadata()
        {
            var so = ScriptableObject.CreateInstance<SampleItemTableSO>();
            so.TsvPath = _tempTsvFile;
            so.Category = "Equipment";

            await so.FetchAsync();

            Assert.AreEqual(2, so.Items.Count);
            Assert.AreEqual("Excalibur", so.Items[0].Name);
            Assert.IsTrue(so.AfterFetchedCalled);
            Assert.IsFalse(string.IsNullOrEmpty(so.LastSyncedTime));

            Object.DestroyImmediate(so);
        }

        [Test]
        public void Fetch_InvalidLocalFile_LogsErrorAndThrowsFileNotFound()
        {
            var so = ScriptableObject.CreateInstance<SampleItemTableSO>();
            so.TsvPath = Path.Combine(Application.temporaryCachePath, "non_existent_file.tsv");

            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*File not found.*"));
            Assert.Throws<FileNotFoundException>(() => so.Fetch());

            Object.DestroyImmediate(so);
        }
    }
}
