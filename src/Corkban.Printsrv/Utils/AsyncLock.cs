namespace CorkbanPrintsrv.Utils;

public class AsyncLock
{
    private readonly IDisposable _releaserHandle;
    private readonly Task<IDisposable> _releaserTask;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AsyncLock()
    {
        _releaserHandle = new Releaser(_semaphore);
        _releaserTask = Task.FromResult(_releaserHandle);
    }

    public Task<IDisposable> LockAsync()
    {
        var waitTask = _semaphore.WaitAsync();
        return waitTask.IsCompleted
            ? _releaserTask
            : waitTask.ContinueWith((_, releaser) => (IDisposable)releaser!, _releaserHandle, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}