using Cysharp.Threading.Tasks;

namespace NFramework.Save
{
    public interface IStorageService
    {
        void WriteAtomic(string filename, byte[] data);

        byte[] Read(string filename);
        UniTask<byte[]> ReadAsync(string filename);

        byte[] ReadBackup(string filename);
        UniTask<byte[]> ReadBackupAsync(string filename);

        bool Exists(string filename);
        bool HasBackup(string filename);
        void Delete(string filename);
    }
}
