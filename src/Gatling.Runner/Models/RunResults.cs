using System;
using System.Collections.Generic;

namespace Gatling.Runner.Models
{
    public class RunResults
    {
        public List<string> ConsoleOutput { get; set; }
        public Guid RunId { get; set; }
    }
}