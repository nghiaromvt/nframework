using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NFramework
{
    public interface ILoadingStep
    {
        string DisplayName { get; }
        float Weight { get; }
        UniTask ExecuteAsync(IProgress<float> progress, CancellationToken cancellationToken = default);
    }

    public abstract class LoadingStepBase : ILoadingStep
    {
        public abstract string DisplayName { get; }
        public virtual float Weight => 1f;

        public abstract UniTask ExecuteAsync(IProgress<float> progress, CancellationToken cancellationToken = default);
    }
}
