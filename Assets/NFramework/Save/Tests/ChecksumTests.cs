using System.Text;
using NUnit.Framework;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class ChecksumTests
    {
        private IChecksumGenerator _checksumGenerator;

        [SetUp]
        public void SetUp()
        {
            _checksumGenerator = new Sha256ChecksumGenerator();
        }

        [Test]
        public void GenerateChecksum_DeterministicAndNonEmpty()
        {
            byte[] data = Encoding.UTF8.GetBytes("Game Save File Bytes");
            string hash1 = _checksumGenerator.GenerateChecksum(data);
            string hash2 = _checksumGenerator.GenerateChecksum(data);

            Assert.IsNotEmpty(hash1);
            Assert.AreEqual(hash1, hash2);
        }

        [Test]
        public void VerifyChecksum_TamperedData_ReturnsFalse()
        {
            byte[] data = Encoding.UTF8.GetBytes("Original Save Data");
            string checksum = _checksumGenerator.GenerateChecksum(data);

            byte[] tamperedData = Encoding.UTF8.GetBytes("Tampered Save Data");

            Assert.IsTrue(_checksumGenerator.VerifyChecksum(data, checksum));
            Assert.IsFalse(_checksumGenerator.VerifyChecksum(tamperedData, checksum));
        }

        [Test]
        public void ComputeHashAndVerifyHash_ValidatesRawBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Binary Save Payload");
            byte[] hash = _checksumGenerator.ComputeHash(data);

            Assert.AreEqual(32, hash.Length);
            Assert.IsTrue(_checksumGenerator.VerifyHash(data, hash));

            byte[] tamperedData = Encoding.UTF8.GetBytes("Corrupted Payload");
            Assert.IsFalse(_checksumGenerator.VerifyHash(tamperedData, hash));
        }
    }
}
