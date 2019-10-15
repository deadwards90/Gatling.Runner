using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Gatling.Orchestrator.Activities;
using Gatling.Orchestrator.Models;
using Microsoft.Azure.WebJobs;

namespace Gatling.Orchestrator.Orchestrators
{
    public class TestOrchestrator
    {
        [FunctionName(nameof(StartTestOrchestrators))]
        public async Task<string> StartTestOrchestrators([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var testSettings = context.GetInput<TestSettings>();

            var regionTasks = new List<Task<RegionSettings>>();

            foreach(var (location, count) in testSettings.Regions)
            {
                for (var i = 0; i < count; i++)
                {
                    regionTasks.Add(
                        context.CallSubOrchestratorAsync<RegionSettings>(nameof(RunTestInRegionOrchestrator),
                            new RegionSettings
                            {
                                TestId = testSettings.TestId,
                                Region = location,
                                FileName = testSettings.FileName
                            }));
                }
            }

            var results = await Task.WhenAll(regionTasks);
            var mergeReportUrl = await context.CallActivityAsync<string>(nameof(GatlingActivities.GenerateMergedReport), 
                results);

            var deleteTasks = results.Select(result => 
                context.CallActivityAsync(nameof(AciActivities.DeleteAciInRegion), result.ContainerName)).ToList();

            await Task.WhenAll(deleteTasks);

            return mergeReportUrl;
        }

        [FunctionName(nameof(RunTestInRegionOrchestrator))]
        public async Task<RegionSettings> RunTestInRegionOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var regionSettings = context.GetInput<RegionSettings>();

            var (containerUrl, containerName) = await context.CallActivityAsync<(string, string)>(
                nameof(AciActivities.CreateAciInRegion),
                regionSettings.Region);

            regionSettings.Url = containerUrl;
            regionSettings.ContainerName = containerName;

            await context.CallActivityAsync<string>(nameof(GatlingActivities.StartTest), regionSettings);

            var done = false;
            while (!done)
            {
                done = await context.CallActivityAsync<bool>(nameof(GatlingActivities.CheckTestStatus), regionSettings);
                if (!done)
                {
                    await context.CreateTimer(context.CurrentUtcDateTime.Add(TimeSpan.FromMinutes(1)),
                        CancellationToken.None);
                }
            }

            regionSettings.SimulationLogName = 
                await context.CallActivityAsync<string>(nameof(GatlingActivities.GetResult), regionSettings);

            return regionSettings;
        }
    }
}
