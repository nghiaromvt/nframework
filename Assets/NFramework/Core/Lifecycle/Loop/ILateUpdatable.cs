namespace NFramework
{
    /// <summary>
    /// Contract for objects updated during the LateUpdate tick phase.
    /// </summary>
    public interface ILateUpdatable
    {
        void OnLateUpdate(float deltaTime);
    }
}
