namespace NFramework.Save
{
    public interface IMigration
    {
        int FromVersion { get; }
        int ToVersion { get; }
        byte[] Migrate(byte[] oldData, ISerializer serializer);
    }
}
