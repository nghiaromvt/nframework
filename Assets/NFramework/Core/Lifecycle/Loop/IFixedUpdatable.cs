namespace NFramework
{
    /// <summary>
    /// Contract for objects updated during the physics FixedUpdate tick phase.
    /// </summary>
    public interface IFixedUpdatable
    {
        void OnFixedUpdate(float fixedDeltaTime);
    }
}
