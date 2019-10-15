using System;
using System.Linq;
using System.Threading.Tasks;
using Gatling.Orchestrator.Models;
using Gatling.Orchestrator.Orchestrators;
using Gatling.Orchestrator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Gatling.Orchestrator
{
    public class StartTestHttp
    {
        private readonly IFileService _fileService;

        public StartTestHttp(IFileService fileService)
        {
            _fileService = fileService;
        }

        [FunctionName(nameof(RunTest))]
        public async Task<IActionResult> RunTest(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "starttest")] 
            HttpRequest httpRequest,
            [OrchestrationClient] DurableOrchestrationClient client
        )
        {
            var requestForm = await httpRequest.ReadFormAsync();
            var testZipFile = requestForm.Files.GetFile("test");
            var regions = requestForm["regions"].ToString().Split(",");
            var testId = Guid.Parse(requestForm["testId"].Single());

            await _fileService.SaveTestZipFile(testZipFile);

            var instanceId = await client.StartNewAsync(nameof(TestOrchestrator.StartTestOrchestrators), 
                testId.ToString(), 
                new TestSettings
                    {
                        FileName = testZipFile.FileName,
                        Regions = regions.Select(s =>
                        {
                            var regionAndCount = s.Split("-");
                            return (regionAndCount[0], int.Parse(regionAndCount[1]));
                        }),
                        TestId = testId.ToString()
                    });

            return new AcceptedResult($"status/{instanceId}", instanceId);
        }

        [FunctionName(nameof(GetStatus))]
        public async Task<IActionResult> GetStatus(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "status/{id}")]
            HttpRequest httpRequest,
            string id,
            [OrchestrationClient] DurableOrchestrationClient client
        )
        {
            var status = await client.GetStatusAsync(id);
            return new OkObjectResult(status);
        }
    }
}
