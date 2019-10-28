using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Gatling.Runner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        private readonly IApplicationLifetime _appLifetime;

        public EnvironmentController(IApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        [Route("")]
        [HttpGet]
        public ActionResult<IDictionary<string, string>> Get()
        {
            return new OkObjectResult(Environment.GetEnvironmentVariables());
        }

        [Route("stop")]
        [HttpGet]
        public IActionResult CloseApplication()
        {
            _appLifetime.StopApplication();
            return Ok();
        }
    }
}
