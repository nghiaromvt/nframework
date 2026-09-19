using System;
using System.IO;
using Cysharp.Threading.Tasks;

namespace NFramework.Save
{
    public class FileStorageService : IStorageService
    {
        private const string TEMP_EXTENSION = ".tmp";
        private const string BACKUP_EXTENSION = ".backup";

        private readonly string _baseDirectory;

        public FileStorageService(string baseDirectory)
        {
            if (string.IsNullOrEmpty(baseDirectory))
            {
                throw new ArgumentException("Base directory cannot be null or empty.", nameof(baseDirectory));
            }

            _baseDirectory = baseDirectory;
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
            }
        }

        public void WriteAtomic(string filename, byte[] data)
        {
            string mainPath = GetFilePath(filename);
            string tempPath = mainPath + TEMP_EXTENSION;
            string backupPath = mainPath + BACKUP_EXTENSION;

            File.WriteAllBytes(tempPath, data);

            if (File.Exists(mainPath))
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                File.Move(mainPath, backupPath);
            }

            File.Move(tempPath, mainPath);
        }

        public byte[] Read(string filename)
        {
            string mainPath = GetFilePath(filename);
            return File.ReadAllBytes(mainPath);
        }

        public UniTask<byte[]> ReadAsync(string filename)
        {
            return UniTask.RunOnThreadPool(() => Read(filename));
        }

        public byte[] ReadBackup(string filename)
        {
            string backupPath = GetFilePath(filename) + BACKUP_EXTENSION;
            return File.ReadAllBytes(backupPath);
        }

        public UniTask<byte[]> ReadBackupAsync(string filename)
        {
            return UniTask.RunOnThreadPool(() => ReadBackup(filename));
        }

        public bool Exists(string filename)
        {
            return File.Exists(GetFilePath(filename));
        }

        public bool HasBackup(string filename)
        {
            return File.Exists(GetFilePath(filename) + BACKUP_EXTENSION);
        }

        public void Delete(string filename)
        {
            string mainPath = GetFilePath(filename);
            string tempPath = mainPath + TEMP_EXTENSION;
            string backupPath = mainPath + BACKUP_EXTENSION;

            if (File.Exists(mainPath))
            {
                File.Delete(mainPath);
            }

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }

        private string GetFilePath(string filename)
        {
            return Path.Combine(_baseDirectory, filename);
        }
    }
}
