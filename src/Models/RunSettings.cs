using System;

namespace Gatling.Runner.Models
{
    public class RunSettings
    {
        public string SimulationClassName { get; set; }
        public string GatlingPath { get; set; }
        public Guid RunId { get; set; }
    }
}
