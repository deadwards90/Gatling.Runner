using System.Collections.Concurrent;
using Gatling.Runner.Models;
using Gatling.Runner.Queuing;

namespace Gatling.Runner.Services
{
    public interface IJobStatusService
    {
        void SetState(string jobId, State newState);
        State GetState(string jobId);
    }

    public class JobStatusService : IJobStatusService
    {
        private readonly ConcurrentDictionary<string, State> _jobStatus = new ConcurrentDictionary<string, State>();

        public void SetState(string jobId, State newState)
        {
            _jobStatus.AddOrUpdate(jobId, newState, (_, __) => newState);
        }

        public State GetState(string jobId)
        {
            return _jobStatus[jobId];
        }
    }
}
