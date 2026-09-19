using System;

namespace NFramework.Save
{
    public readonly struct SaveFileHeader
    {
        public static readonly byte[] MAGIC_BYTES = { (byte)'N', (byte)'F', (byte)'S', (byte)'V' };
        public const byte CURRENT_FORMAT_VERSION = 1;
        public const int CHECKSUM_SIZE = 32;
        public const int HEADER_SIZE = 4 + 1 + CHECKSUM_SIZE;

        public byte FormatVersion { get; }
        public byte[] Checksum { get; }

        public SaveFileHeader(byte[] checksum, byte formatVersion = CURRENT_FORMAT_VERSION)
        {
            FormatVersion = formatVersion;
            Checksum = checksum ?? new byte[CHECKSUM_SIZE];
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[HEADER_SIZE];
            Array.Copy(MAGIC_BYTES, 0, bytes, 0, MAGIC_BYTES.Length);
            bytes[4] = FormatVersion;

            if (Checksum != null && Checksum.Length == CHECKSUM_SIZE)
            {
                Array.Copy(Checksum, 0, bytes, 5, CHECKSUM_SIZE);
            }

            return bytes;
        }

        public static bool TryRead(byte[] data, out SaveFileHeader header, out int payloadOffset)
        {
            header = default;
            payloadOffset = 0;

            if (data == null || data.Length < HEADER_SIZE)
            {
                return false;
            }

            for (int i = 0; i < MAGIC_BYTES.Length; i++)
            {
                if (data[i] != MAGIC_BYTES[i])
                {
                    return false;
                }
            }

            byte version = data[4];
            byte[] checksum = new byte[CHECKSUM_SIZE];
            Array.Copy(data, 5, checksum, 0, CHECKSUM_SIZE);

            header = new SaveFileHeader(checksum, version);
            payloadOffset = HEADER_SIZE;
            return true;
        }
    }
}
