namespace NFramework
{
    public class Singleton<T> where T : class, new()
    {
        public static T I { get; } = new T();
    }
}
