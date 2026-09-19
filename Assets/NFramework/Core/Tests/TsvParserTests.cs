using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace NFramework.Core.Tests
{
    public class TsvParserTests
    {
        public enum TestCategory
        {
            None,
            Armor,
            Weapon,
            Consumable
        }

        public class TestItem
        {
            [TSVColumn("item_id")]
            public int Id;

            public string Name { get; set; }

            [SerializeField]
            private float _price;

            public float Price => _price;

            public bool IsActive;

            public TestCategory Category { get; set; }

            public int[] Tags;

            public List<string> Aliases { get; set; }
        }

        [Test]
        public void Parse_ValidTsvWithCommentsAndEmptyLines_ParsesCorrectly()
        {
            var tsv =
                "# This is a comment\n" +
                "\n" +
                "item_id\tname\tprice\tisActive\tcategory\ttags\taliases\n" +
                "# Another comment before data\n" +
                "101\tIron Sword\t19.99\ttrue\tWeapon\t1,2,3\tsword;blade\n" +
                "102\tWooden Shield\t5.5\t0\tArmor\t4\tshield\n";

            var items = TsvParser.Parse<TestItem>(tsv);

            Assert.AreEqual(2, items.Count);

            Assert.AreEqual(101, items[0].Id);
            Assert.AreEqual("Iron Sword", items[0].Name);
            Assert.AreEqual(19.99f, items[0].Price, 0.001f);
            Assert.IsTrue(items[0].IsActive);
            Assert.AreEqual(TestCategory.Weapon, items[0].Category);
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, items[0].Tags);
            CollectionAssert.AreEqual(new[] { "sword", "blade" }, items[0].Aliases);

            Assert.AreEqual(102, items[1].Id);
            Assert.AreEqual("Wooden Shield", items[1].Name);
            Assert.AreEqual(5.5f, items[1].Price, 0.001f);
            Assert.IsFalse(items[1].IsActive);
            Assert.AreEqual(TestCategory.Armor, items[1].Category);
            CollectionAssert.AreEqual(new[] { 4 }, items[1].Tags);
            CollectionAssert.AreEqual(new[] { "shield" }, items[1].Aliases);
        }

        [Test]
        public void Parse_InvalidNumberFormat_ThrowsException()
        {
            var tsv =
                "item_id\tname\tprice\n" +
                "invalid_number\tSword\t10\n";

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(".*failed to parse 'invalid_number'.*"));
            Assert.Throws<FormatException>(() => TsvParser.Parse<TestItem>(tsv));
        }

        [Test]
        public void NormalizeGoogleSheetUrl_StandardEditUrlWithGid_NormalizesCorrectly()
        {
            var input = "https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit#gid=123456789";
            var expected = "https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/export?format=tsv&gid=123456789";

            var actual = TsvParser.NormalizeGoogleSheetUrl(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NormalizeGoogleSheetUrl_EditUrlWithoutGid_DefaultsGidToZero()
        {
            var input = "https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit";
            var expected = "https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/export?format=tsv&gid=0";

            var actual = TsvParser.NormalizeGoogleSheetUrl(input);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NormalizeGoogleSheetUrl_NonGoogleSheetUrl_ReturnsOriginal()
        {
            var input = "https://example.com/data.tsv";
            var actual = TsvParser.NormalizeGoogleSheetUrl(input);

            Assert.AreEqual(input, actual);
        }

        [Test]
        public void IsWebUrl_DetectsHttpAndHttps()
        {
            Assert.IsTrue(TsvParser.IsWebUrl("http://example.com/test.tsv"));
            Assert.IsTrue(TsvParser.IsWebUrl("https://example.com/test.tsv"));
            Assert.IsFalse(TsvParser.IsWebUrl("Assets/Data/test.tsv"));
            Assert.IsFalse(TsvParser.IsWebUrl(string.Empty));
        }
    }
}
