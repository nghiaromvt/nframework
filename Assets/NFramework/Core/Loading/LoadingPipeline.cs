using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NFramework
{
    public class LoadingPipeline
    {
        private readonly List<ILoadingStep> _steps = new();

        public event Action<string> OnStepStarted;
        public event Action<float> OnProgress;

        public float Progress { get; private set; }
        public bool IsRunning { get; private set; }

        public IReadOnlyList<ILoadingStep> Steps => _steps;

        public LoadingPipeline AddStep(ILoadingStep step)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }

            if (step.Weight <= 0f)
            {
                throw new ArgumentException($"Step weight must be positive. Step '{step.DisplayName}' has weight {step.Weight}.", nameof(step));
            }

            _steps.Add(step);
            return this;
        }

        public async UniTask ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("LoadingPipeline is already running.");
            }

            if (_steps.Count == 0)
            {
                Progress = 1f;
                OnProgress?.Invoke(1f);
                return;
            }

            float totalWeight = 0f;
            for (int i = 0; i < _steps.Count; i++)
            {
                totalWeight += _steps[i].Weight;
            }

            IsRunning = true;
            Progress = 0f;
            OnProgress?.Invoke(0f);

            float completedWeight = 0f;

            try
            {
                for (int i = 0; i < _steps.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var step = _steps[i];
                    OnStepStarted?.Invoke(step.DisplayName);

                    float currentStepWeight = step.Weight;
                    var stepProgress = new ActionProgress(stepPercent =>
                    {
                        float clampedPercent = Math.Clamp(stepPercent, 0f, 1f);
                        float overallProgress = (completedWeight + currentStepWeight * clampedPercent) / totalWeight;
                        Progress = overallProgress;
                        OnProgress?.Invoke(overallProgress);
                    });

                    await step.ExecuteAsync(stepProgress, cancellationToken);

                    completedWeight += currentStepWeight;
                    Progress = completedWeight / totalWeight;
                    OnProgress?.Invoke(Progress);
                }

                Progress = 1f;
                OnProgress?.Invoke(1f);
            }
            finally
            {
                IsRunning = false;
            }
        }

        private sealed class ActionProgress : IProgress<float>
        {
            private readonly Action<float> _action;

            public ActionProgress(Action<float> action)
            {
                _action = action;
            }

            public void Report(float value)
            {
                _action?.Invoke(value);
            }
        }
    }
}
