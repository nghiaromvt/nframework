using Sirenix.Serialization;

namespace NFramework.Save
{
    public class OdinSerializerHandler : ISerializer
    {
        private readonly DataFormat _format;

        public OdinSerializerHandler(DataFormat format = DataFormat.Binary)
        {
            _format = format;
        }

        public byte[] Serialize<T>(T value)
        {
            return SerializationUtility.SerializeValue(value, _format);
        }

        public T Deserialize<T>(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return default;
            }

            return SerializationUtility.DeserializeValue<T>(bytes, _format);
        }
    }
}
