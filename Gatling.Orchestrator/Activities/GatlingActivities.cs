using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gatling.Orchestrator.Models;
using Gatling.Orchestrator.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Gatling.Orchestrator.Activities
{
    public class GatlingActivities
    {
        private readonly IFileService _fileService;
        private readonly IGatlingService _gatlingService;

        public GatlingActivities(IFileService fileService, IGatlingService gatlingService)
        {
            _fileService = fileService;
            _gatlingService = gatlingService;
        }

        [FunctionName(nameof(StartTest))]
        public async Task StartTest([ActivityTrigger] DurableActivityContextBase context, ILogger logger)
        {
            var regionSettings = context.GetInput<RegionSettings>();

            logger.LogInformation("Starting test");
            await _gatlingService.StartTest(regionSettings.Url, regionSettings.FileName, regionSettings.TestId);
        }

        [FunctionName(nameof(CheckTestStatus))]
        public async Task<bool> CheckTestStatus([ActivityTrigger] DurableActivityContextBase context, ILogger logger)
        {
            var regionSettings = context.GetInput<RegionSettings>();

            logger.LogInformation("Checking test status for container {name}", regionSettings.ContainerName);
            return await _gatlingService.CheckTestStatus(regionSettings.Url, regionSettings.TestId);
        }

        [FunctionName(nameof(GetResult))]
        public async Task<string> GetResult([ActivityTrigger] DurableActivityContextBase context, ILogger logger)
        {
            var regionSettings = context.GetInput<RegionSettings>();

            logger.LogInformation("Getting test result for {name}", regionSettings.ContainerName);

            return await _gatlingService.GetResult(regionSettings.Url, 
                regionSettings.TestId, regionSettings.ContainerName);
        }

        [FunctionName(nameof(GenerateMergedReport))]
        public async Task<string> GenerateMergedReport([ActivityTrigger] DurableActivityContextBase context, ILogger logger)
        {
            var regionSettings = context.GetInput<List<RegionSettings>>();

            logger.LogInformation("Generating merge report for {testId}",
                regionSettings.First().TestId);

            var gatlingUrl = regionSettings.First().Url;

            var mergeReportZip = $"{regionSettings.First().TestId}-merge.zip";
            await _gatlingService.MergeReports(gatlingUrl,
                regionSettings.Select(s => s.SimulationLogName),
                mergeReportZip);

            return _fileService.GetFileUrl(FileService.TestResultsContainer, mergeReportZip);
        }
    }
}
