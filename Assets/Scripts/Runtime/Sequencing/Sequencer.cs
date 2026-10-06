using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Runtime
{
    /// <summary>
    /// Runs steps side by side without blocking the caller, and knows when none is left running (D69).
    /// Order and grouping come from <see cref="StepSequence"/> and <see cref="StepGroup"/>; one call to
    /// <see cref="CancelAll"/> stops every step it runs (restart, home).
    /// </summary>
    public sealed class Sequencer : IDisposable
    {
        private CancellationTokenSource cancellation = new CancellationTokenSource();
        private UniTaskCompletionSource idle;
        private int running;

        public bool IsIdle => running == 0;

        public void Run(IStep step) => RunAsync(step, cancellation.Token).Forget();

        /// <summary>Finishes when no step is running, at once if none is.</summary>
        public UniTask WhenIdle()
        {
            if (running == 0)
                return UniTask.CompletedTask;

            if (idle == null)
                idle = new UniTaskCompletionSource();

            return idle.Task;
        }

        /// <summary>Stops every running step; steps run afterwards start fresh.</summary>
        public void CancelAll()
        {
            CancellationTokenSource cancelled = cancellation;
            cancellation = new CancellationTokenSource();
            cancelled.Cancel();
            cancelled.Dispose();
        }

        public void Dispose()
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }

        private async UniTaskVoid RunAsync(IStep step, CancellationToken token)
        {
            running++;

            try
            {
                await step.Play(token);
            }
            catch (OperationCanceledException)
            {
                // Cancelled on purpose: the step stops where it is.
            }
            finally
            {
                running--;

                if (running == 0)
                    ReportIdle();
            }
        }

        private void ReportIdle()
        {
            if (idle == null)
                return;

            UniTaskCompletionSource done = idle;
            idle = null;
            done.TrySetResult();
        }
    }
}
