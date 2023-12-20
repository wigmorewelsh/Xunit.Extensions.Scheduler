using System;
using System.Threading;
using System.Threading.Tasks;

namespace Xunit.Extensions.Scheduler;

internal sealed class AsyncLimiter
{
    private readonly SemaphoreSlim semaphore;
    private readonly Task<IDisposable> releaser;

    public AsyncLimiter(int limit)
    {
        semaphore = new SemaphoreSlim(limit, limit);
        releaser = Task.FromResult((IDisposable)new Releaser(this));
    }

    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        var wait = semaphore.WaitAsync(cancellationToken);
        return wait.IsCompleted
            ? releaser
            : wait.ContinueWith((_, state) => (IDisposable)state!,
                releaser.Result, cancellationToken,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private sealed class Releaser : IDisposable
    {
        private readonly AsyncLimiter toRelease;

        internal Releaser(AsyncLimiter toRelease)
        {
            this.toRelease = toRelease;
        }

        public void Dispose()
        {
            toRelease.semaphore.Release();
        }
    }
}