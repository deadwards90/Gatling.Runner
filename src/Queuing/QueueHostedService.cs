using System;
using System.Threading;
using System.Threading.Tasks;
using Gatling.Runner.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gatling.Runner.Queuing
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly IJobStatusService _jobStatusService;
        private readonly ILogger _logger;

        public QueuedHostedService(IBackgroundTaskQueue taskQueue,
            IJobStatusService jobStatusService,
            ILogger<QueuedHostedService> logger)
        {
            TaskQueue = taskQueue;
            _jobStatusService = jobStatusService;
            _logger = logger;
        }

        public IBackgroundTaskQueue TaskQueue { get; }

        protected override async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                var (jobId, workItem) = await TaskQueue.DequeueAsync(cancellationToken);

                try
                {
                    await workItem(cancellationToken);
                    _jobStatusService.SetState(jobId, State.Finished);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        $"Error occurred executing {nameof(workItem)}.");
                    _jobStatusService.SetState(jobId, State.Failed);
                }
            }

            _logger.LogInformation("Queued Hosted Service is stopping.");
        }
    }
}
