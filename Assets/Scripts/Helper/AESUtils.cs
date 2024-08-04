using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NFramework
{
    public class AESUtils
    {
        private static readonly byte[] _iv = new byte[16]; // Initialization vector
#if UNITY_EDITOR
        public const string EDITOR_GLOBAL_AES_KEY_STRING = "EditorGlobalAESKey";
        private const string DEFAULT_EDITOR_GLOBAL_AES_KEY = "123456";

        public static string GetGlobalEditorAESKey()
        {
            var result = UnityEditor.EditorPrefs.GetString(EDITOR_GLOBAL_AES_KEY_STRING);
            if (string.IsNullOrEmpty(result))
            {
                result = DEFAULT_EDITOR_GLOBAL_AES_KEY;
                UnityEditor.EditorPrefs.SetString(EDITOR_GLOBAL_AES_KEY_STRING, DEFAULT_EDITOR_GLOBAL_AES_KEY);
            }
            return result;
        }
#endif

        public static string EncryptAES(string data, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be empty.");
            }

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            // Truncate or pad the key to 128 bits (16 bytes)
            keyBytes = TruncateOrPadKey(keyBytes, 16);

            try
            {
                byte[] encryptedBytes = EncryptAES(dataBytes, keyBytes);
                return Convert.ToBase64String(encryptedBytes);
            }
            catch (CryptographicException ex)
            {
                Debug.LogError("Encryption failed: " + ex.Message);
                throw; // Re-throw the exception for handling at a higher level
            }
        }

        public static string DecryptAES(string encryptedData, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be empty.");
            }

            try
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                keyBytes = TruncateOrPadKey(keyBytes, 16);

                byte[] decryptedBytes = DecryptAES(encryptedBytes, keyBytes);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (FormatException ex)
            {
                Debug.LogError("Invalid Base64 string: " + ex.Message);
                throw;
            }
            catch (CryptographicException ex)
            {
                Debug.LogError("Decryption failed: " + ex.Message);
                throw;
            }
        }

        public static byte[] EncryptAES(byte[] data, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                byte[] encryptedData = encryptor.TransformFinalBlock(data, 0, data.Length);

                return encryptedData;
            }
        }

        public static byte[] DecryptAES(byte[] encryptedData, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = _iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                return decryptedData;
            }
        }

        private static byte[] TruncateOrPadKey(byte[] keyBytes, int desiredLength)
        {
            if (keyBytes.Length > desiredLength)
            {
                return keyBytes.Take(desiredLength).ToArray(); // Truncate
            }
            else
            {
                byte[] paddedKey = new byte[desiredLength];
                Array.Copy(keyBytes, paddedKey, keyBytes.Length);
                return paddedKey; // Pad with zeros
            }
        }
    }
}
