using NUnit.Framework;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class SaveHeaderTests
    {
        [Test]
        public void Header_ToBytesAndTryRead_ValidatesCorrectly()
        {
            byte[] checksum = new byte[32];
            checksum[0] = 0xAA;
            checksum[31] = 0xFF;

            SaveFileHeader header = new SaveFileHeader(checksum, 1);
            byte[] headerBytes = header.ToBytes();

            Assert.AreEqual(SaveFileHeader.HEADER_SIZE, headerBytes.Length);
            Assert.AreEqual((byte)'N', headerBytes[0]);
            Assert.AreEqual((byte)'F', headerBytes[1]);
            Assert.AreEqual((byte)'S', headerBytes[2]);
            Assert.AreEqual((byte)'V', headerBytes[3]);
            Assert.AreEqual(1, headerBytes[4]);
            Assert.AreEqual(0xAA, headerBytes[5]);
            Assert.AreEqual(0xFF, headerBytes[36]);

            bool success = SaveFileHeader.TryRead(headerBytes, out SaveFileHeader parsed, out int offset);

            Assert.IsTrue(success);
            Assert.AreEqual(1, parsed.FormatVersion);
            Assert.AreEqual(SaveFileHeader.HEADER_SIZE, offset);
            Assert.AreEqual(0xAA, parsed.Checksum[0]);
            Assert.AreEqual(0xFF, parsed.Checksum[31]);
        }

        [Test]
        public void Header_InvalidMagic_ReturnsFalse()
        {
            byte[] invalid = new byte[SaveFileHeader.HEADER_SIZE];
            invalid[0] = (byte)'X';
            invalid[1] = (byte)'Y';
            invalid[2] = (byte)'Z';
            invalid[3] = (byte)'W';

            bool success = SaveFileHeader.TryRead(invalid, out _, out _);
            Assert.IsFalse(success);
        }

        [Test]
        public void Header_TooShort_ReturnsFalse()
        {
            byte[] shortData = new byte[] { (byte)'N', (byte)'F', (byte)'S', (byte)'V', 1 };
            bool success = SaveFileHeader.TryRead(shortData, out _, out _);

            Assert.IsFalse(success);
        }
    }
}
