using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Gatling.Runner.Services;

namespace Gatling.Runner.Queuing
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(string jobId, Func<CancellationToken, Task> workItem);

        Task<(string jobId, Func<CancellationToken, Task> func)> DequeueAsync(
            CancellationToken cancellationToken);
    }

    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly IJobStatusService _jobStatusService;

        public BackgroundTaskQueue(IJobStatusService jobStatusService)
        {
            _jobStatusService = jobStatusService;
        }

        private readonly ConcurrentQueue<(string jobId, Func<CancellationToken, Task> func)> _workItems =
            new ConcurrentQueue<(string jobId, Func<CancellationToken, Task> func)>();
        private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(string jobId,
            Func<CancellationToken, Task> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue((jobId, workItem));

            _jobStatusService.SetState(jobId, State.Started);

            _signal.Release();
        }

        public async Task<(string jobId, Func<CancellationToken, Task> func)> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }

    

    public enum State
    {
        Started,
        Failed,
        Finished
    }
}
