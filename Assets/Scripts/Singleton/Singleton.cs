namespace NFramework
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly T _i = new T();

        public static T I => _i;
    }
}
