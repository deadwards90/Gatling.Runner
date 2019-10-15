using System.Collections;
using System.Collections.Generic;

namespace Gatling.Orchestrator.Models
{
    public class TestSettings
    {
        public string TestId { get; set; }
        public IEnumerable<(string location, int count)> Regions { get; set; }
        public string FileName { get; set; }
    }
}