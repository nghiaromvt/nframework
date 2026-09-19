using System;
using System.Collections.Generic;
using NUnit.Framework;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class SerializerTests
    {
        [Serializable]
        private class TestPlayerData
        {
            public string Name;
            public int Level;
            public List<int> Scores;
        }

        private ISerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new OdinSerializerHandler();
        }

        [Test]
        public void SerializeAndDeserialize_PrimitiveValue_MatchesOriginal()
        {
            int original = 1337;
            byte[] bytes = _serializer.Serialize(original);
            int result = _serializer.Deserialize<int>(bytes);

            Assert.AreEqual(original, result);
        }

        [Test]
        public void SerializeAndDeserialize_ComplexObject_MatchesOriginal()
        {
            TestPlayerData original = new TestPlayerData
            {
                Name = "Hero",
                Level = 42,
                Scores = new List<int> { 100, 200, 300 }
            };

            byte[] bytes = _serializer.Serialize(original);
            TestPlayerData result = _serializer.Deserialize<TestPlayerData>(bytes);

            Assert.IsNotNull(result);
            Assert.AreEqual(original.Name, result.Name);
            Assert.AreEqual(original.Level, result.Level);
            Assert.AreEqual(original.Scores.Count, result.Scores.Count);
            Assert.AreEqual(original.Scores[1], result.Scores[1]);
        }

        [Test]
        public void Deserialize_NullOrEmptyBytes_ReturnsDefault()
        {
            TestPlayerData nullResult = _serializer.Deserialize<TestPlayerData>(null);
            TestPlayerData emptyResult = _serializer.Deserialize<TestPlayerData>(new byte[0]);

            Assert.IsNull(nullResult);
            Assert.IsNull(emptyResult);
        }
    }
}
