using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Gatling.Runner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnvironmentController : ControllerBase
    {
        [Route("")]
        // GET api/values
        [HttpGet]
        public ActionResult<IDictionary<string, string>> Get()
        {
            return new OkObjectResult(Environment.GetEnvironmentVariables());
        }
    }
}
