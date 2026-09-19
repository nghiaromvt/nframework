using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NFramework.Save
{
    public class AesEncryptor : IEncryptor
    {
        private readonly byte[] _keyBytes;

        public AesEncryptor(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Encryption key cannot be null or empty.", nameof(key));
            }

            using (SHA256 sha = SHA256.Create())
            {
                _keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
        }

        public AesEncryptor(byte[] keyBytes)
        {
            _keyBytes = keyBytes ?? throw new ArgumentNullException(nameof(keyBytes));
        }

        public byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return data;
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = _keyBytes;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);

                    using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv))
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        public byte[] Decrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return data;
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = _keyBytes;
                int ivLength = aes.BlockSize / 8;

                if (data.Length < ivLength)
                {
                    throw new CryptographicException("Encrypted data is too short to contain a valid IV.");
                }

                byte[] iv = new byte[ivLength];
                Array.Copy(data, 0, iv, 0, ivLength);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv))
                    using (MemoryStream cipherStream = new MemoryStream(data, ivLength, data.Length - ivLength))
                    using (CryptoStream cs = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read))
                    {
                        cs.CopyTo(ms);
                    }

                    return ms.ToArray();
                }
            }
        }
    }
}
