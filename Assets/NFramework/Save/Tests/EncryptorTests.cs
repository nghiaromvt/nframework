using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using NFramework;
using NFramework.Save;

namespace NFramework.Save.Tests
{
    [TestFixture]
    public class EncryptorTests
    {
        private const string TEST_KEY = "SecretKey12345678";
        private IEncryptor _encryptor;

        [SetUp]
        public void SetUp()
        {
            _encryptor = new AesEncryptor(TEST_KEY);
        }

        [Test]
        public void EncryptAndDecrypt_MatchesOriginalData()
        {
            byte[] original = Encoding.UTF8.GetBytes("Super secret save game payload!");
            byte[] encrypted = _encryptor.Encrypt(original);

            Assert.AreNotEqual(original, encrypted);

            byte[] decrypted = _encryptor.Decrypt(encrypted);
            Assert.AreEqual(original, decrypted);
        }

        [Test]
        public void Decrypt_WithWrongKey_ThrowsException()
        {
            byte[] original = Encoding.UTF8.GetBytes("Data to encrypt with a sufficiently long payload to ensure cryptographic padding failure on wrong key.");
            byte[] encrypted = _encryptor.Encrypt(original);

            IEncryptor wrongEncryptor = new AesEncryptor("DifferentKey87654321");
            Assert.Throws<CryptographicException>(() => wrongEncryptor.Decrypt(encrypted));
        }

        [Test]
        public void Encrypt_EmptyOrNullData_ReturnsEmptyOrNull()
        {
            Assert.IsNull(_encryptor.Encrypt(null));
            Assert.IsEmpty(_encryptor.Encrypt(new byte[0]));
        }
    }
}
