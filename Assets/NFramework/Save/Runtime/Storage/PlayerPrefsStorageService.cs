using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace NFramework.Save
{
    public class PlayerPrefsStorageService : IStorageService
    {
        private const string BACKUP_SUFFIX = "_backup";

        public void WriteAtomic(string filename, byte[] data)
        {
            if (PlayerPrefs.HasKey(filename))
            {
                string existingData = PlayerPrefs.GetString(filename);
                PlayerPrefs.SetString(filename + BACKUP_SUFFIX, existingData);
            }

            string base64 = Convert.ToBase64String(data);
            PlayerPrefs.SetString(filename, base64);
            PlayerPrefs.Save();
        }

        public byte[] Read(string filename)
        {
            string base64 = PlayerPrefs.GetString(filename, string.Empty);
            if (string.IsNullOrEmpty(base64))
            {
                throw new InvalidOperationException($"No save data found in PlayerPrefs for key: {filename}");
            }

            return Convert.FromBase64String(base64);
        }

        public UniTask<byte[]> ReadAsync(string filename)
        {
            return UniTask.FromResult(Read(filename));
        }

        public byte[] ReadBackup(string filename)
        {
            string base64 = PlayerPrefs.GetString(filename + BACKUP_SUFFIX, string.Empty);
            if (string.IsNullOrEmpty(base64))
            {
                throw new InvalidOperationException($"No backup save data found in PlayerPrefs for key: {filename}");
            }

            return Convert.FromBase64String(base64);
        }

        public UniTask<byte[]> ReadBackupAsync(string filename)
        {
            return UniTask.FromResult(ReadBackup(filename));
        }

        public bool Exists(string filename)
        {
            return PlayerPrefs.HasKey(filename);
        }

        public bool HasBackup(string filename)
        {
            return PlayerPrefs.HasKey(filename + BACKUP_SUFFIX);
        }

        public void Delete(string filename)
        {
            PlayerPrefs.DeleteKey(filename);
            PlayerPrefs.DeleteKey(filename + BACKUP_SUFFIX);
            PlayerPrefs.Save();
        }
    }
}
