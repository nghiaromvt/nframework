namespace NFramework.Save
{
    public interface IChecksumGenerator
    {
        byte[] ComputeHash(byte[] data);
        bool VerifyHash(byte[] data, byte[] expectedHash);
        string GenerateChecksum(byte[] data);
        bool VerifyChecksum(byte[] data, string expectedChecksum);
    }
}
