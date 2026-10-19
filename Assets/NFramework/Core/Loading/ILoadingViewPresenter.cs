namespace NFramework
{
    public interface ILoadingViewPresenter
    {
        float CurrentProgress { get; }
        void SetStepName(string stepName);
        void SetProgress(float progress);
    }
}
