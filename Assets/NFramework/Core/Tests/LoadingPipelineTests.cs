using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class LoadingPipelineTests
    {
        private class FakeStep : ILoadingStep
        {
            public string DisplayName { get; }
            public float Weight { get; }
            private readonly Func<IProgress<float>, UniTask> _action;

            public FakeStep(string displayName, float weight, Func<IProgress<float>, UniTask> action = null)
            {
                DisplayName = displayName;
                Weight = weight;
                _action = action;
            }

            public async UniTask ExecuteAsync(IProgress<float> progress, CancellationToken cancellationToken = default)
            {
                if (_action != null)
                {
                    await _action(progress);
                }
            }
        }

        [Test]
        public async Task ExecuteAsync_StepsRunInOrder()
        {
            var executedSteps = new List<string>();
            var pipeline = new LoadingPipeline();

            pipeline.AddStep(new FakeStep("Step 1", 1f, p =>
            {
                executedSteps.Add("Step 1");
                return UniTask.CompletedTask;
            }));

            pipeline.AddStep(new FakeStep("Step 2", 1f, p =>
            {
                executedSteps.Add("Step 2");
                return UniTask.CompletedTask;
            }));

            await pipeline.ExecuteAsync();

            CollectionAssert.AreEqual(new[] { "Step 1", "Step 2" }, executedSteps);
            Assert.AreEqual(1f, pipeline.Progress, 0.001f);
        }

        [Test]
        public async Task ExecuteAsync_FiresStepStartedAndProgressEvents()
        {
            var stepNames = new List<string>();
            var progressValues = new List<float>();

            var pipeline = new LoadingPipeline();
            pipeline.OnStepStarted += name => stepNames.Add(name);
            pipeline.OnProgress += p => progressValues.Add(p);

            pipeline.AddStep(new FakeStep("First", 1f));
            pipeline.AddStep(new FakeStep("Second", 3f));

            await pipeline.ExecuteAsync();

            CollectionAssert.AreEqual(new[] { "First", "Second" }, stepNames);
            Assert.Contains(0f, progressValues);
            Assert.Contains(0.25f, progressValues); // After First (weight 1/4)
            Assert.Contains(1f, progressValues); // After Second (weight 4/4)
        }

        [Test]
        public void ExecuteAsync_StepThrows_PipelineRethrowsAndStops()
        {
            var executedSteps = new List<string>();
            var pipeline = new LoadingPipeline();

            pipeline.AddStep(new FakeStep("Faulty", 1f, p => throw new InvalidOperationException("Step failed!")));
            pipeline.AddStep(new FakeStep("NeverRan", 1f, p =>
            {
                executedSteps.Add("NeverRan");
                return UniTask.CompletedTask;
            }));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await pipeline.ExecuteAsync());
            CollectionAssert.IsEmpty(executedSteps);
            Assert.IsFalse(pipeline.IsRunning);
        }

        [Test]
        public void AddStep_ZeroOrNegativeWeight_ThrowsArgumentException()
        {
            var pipeline = new LoadingPipeline();
            Assert.Throws<ArgumentException>(() => pipeline.AddStep(new FakeStep("Zero", 0f)));
            Assert.Throws<ArgumentException>(() => pipeline.AddStep(new FakeStep("Negative", -1f)));
        }

        [Test]
        public async Task ExecuteAsync_EmptyPipeline_CompletesWithFullProgress()
        {
            var pipeline = new LoadingPipeline();
            await pipeline.ExecuteAsync();

            Assert.AreEqual(1f, pipeline.Progress);
        }
    }
}
