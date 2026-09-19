namespace NFramework
{
    /// <summary>
    /// Contract for objects updated during the standard Update tick phase.
    /// </summary>
    public interface IUpdatable
    {
        void OnUpdate(float deltaTime);
    }
}
