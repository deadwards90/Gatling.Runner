using System.Collections.Concurrent;

namespace Gatling.Runner.BackgroundTask
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
            _jobStatus.AddOrUpdate(jobId, newState, (s, state) => newState);
        }

        public State GetState(string jobId)
        {
            return _jobStatus[jobId];
        }
    }
}
