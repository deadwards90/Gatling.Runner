using System;
using System.Threading.Tasks;
using Gatling.Runner.Models;
using Gatling.Runner.Queuing;
using Gatling.Runner.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gatling.Runner.Controllers
{
    [Route("api/[controller]")]
    public class GatlingController : Controller
    {
        private readonly IBackgroundTaskQueue _queue;
        private readonly FileService _fileService;
        private readonly GatlingService _gatlingService;
        private readonly IJobStatusService _jobStatusService;

        public GatlingController(IBackgroundTaskQueue queue, FileService fileService, GatlingService gatlingService, IJobStatusService jobStatusService)
        {
            _queue = queue;
            _fileService = fileService;
            _gatlingService = gatlingService;
            _jobStatusService = jobStatusService;
        }

        [Route("start/{runId:guid}")]
        [HttpPost]
        public async Task<IActionResult> Start(Guid runId, [FromQuery] bool returnReport)
        {
            if (ValidateQueryParameters(runId, out var badRequest)) return badRequest;

            var runSettings = await _fileService.CreateRunFolders(runId, Request.Body);

            if (returnReport)
            {
                await _gatlingService.RunSimulation(runSettings);
                return new FileStreamResult(_fileService.GetReportsStream(runId),
                    "application/zip") ;
            }

            var runResults = await _gatlingService.RunSimulation(runSettings);
            return Ok(runResults);
        }


        [Route("startasync/{runId:guid}")]
        [HttpPost]
        public async Task<IActionResult> StartAsync(Guid runId)
        {
            if (ValidateQueryParameters(runId, out var badRequest)) return badRequest;

            var runSettings = await _fileService.CreateRunFolders(runId, Request.Body);

            _queue.QueueBackgroundWorkItem(runId.ToString(), async token =>
            {
                await _gatlingService.RunSimulation(runSettings);
            });

            return new AcceptedResult($"/getresult/{runId}", null);
        }

        [Route("getresult/{runId:guid}")]
        [HttpPost]
        public IActionResult GetResults(Guid runId)
        {
            if (runId == default)
            {
                return BadRequest("Must supply runId");
            }

            switch (_jobStatusService.GetState(runId.ToString()))
            {
                case State.Finished:
                    return new FileStreamResult(_fileService.GetReportsStream(runId),
                        "application/zip");
                case State.Failed:
                    return new BadRequestObjectResult("Job Failed");
                case State.Started:
                    return new AcceptedResult();
                default:
                    throw new ArgumentOutOfRangeException("state", "Unknown state encountered");
            }
        }

        private bool ValidateQueryParameters(Guid runId, out IActionResult badRequest)
        {
            if (Request.ContentType != "application/zip")
            {
                {
                    badRequest = BadRequest("Upload must be in zip format");
                    return true;
                }
            }

            if (runId == default)
            {
                {
                    badRequest = BadRequest("Must supply runId as query parameter");
                    return true;
                }
            }

            badRequest = null;
            return false;
        }
    }
}