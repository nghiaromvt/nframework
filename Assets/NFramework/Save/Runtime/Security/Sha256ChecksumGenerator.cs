using System;
using System.Security.Cryptography;
using System.Text;

namespace NFramework.Save
{
    public class Sha256ChecksumGenerator : IChecksumGenerator
    {
        public const int HASH_SIZE = 32;

        public byte[] ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return new byte[HASH_SIZE];
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        public bool VerifyHash(byte[] data, byte[] expectedHash)
        {
            if (expectedHash == null || expectedHash.Length != HASH_SIZE)
            {
                return false;
            }

            byte[] actualHash = ComputeHash(data);
            for (int i = 0; i < HASH_SIZE; i++)
            {
                if (actualHash[i] != expectedHash[i])
                {
                    return false;
                }
            }

            return true;
        }

        public string GenerateChecksum(byte[] data)
        {
            byte[] hash = ComputeHash(data);
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }

            return sb.ToString();
        }

        public bool VerifyChecksum(byte[] data, string expectedChecksum)
        {
            if (string.IsNullOrEmpty(expectedChecksum))
            {
                return false;
            }

            string actualChecksum = GenerateChecksum(data);
            return string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase);
        }
    }
}
