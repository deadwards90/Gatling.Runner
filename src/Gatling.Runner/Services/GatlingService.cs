using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Gatling.Runner.Models;

namespace Gatling.Runner.Services
{
    public class GatlingService
    {
        public async Task<RunResults> RunSimulation(RunSettings runSettings)
        {
            var runId = runSettings.RunId;
            var gatlingStartInfo = new ProcessStartInfo(runSettings.GatlingPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments =
                    $"-sf /tmp/{runId}/simulations -rsf /tmp/{runId}/resources -rf /tmp/{runId}/results -rd {runId} -s {runSettings.SimulationClassName}"
            };

            using (var process = new Process {StartInfo = gatlingStartInfo})
            {
                process.Start();
                process.WaitForExit();

                var results = new RunResults
                {
                    RunId = runSettings.RunId,
                    ConsoleOutput = (await process.StandardOutput.ReadToEndAsync())
                        .Split("\n")
                        .ToList()
                };

                return results;
            }
        }

        public async Task<RunResults> GenerateReports(RunSettings runSettings)
        {
            var runId = runSettings.RunId;
            var gatlingStartInfo = new ProcessStartInfo(runSettings.GatlingPath)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Arguments =
                    $"-ro /tmp/{runId}/results"
            };

            using (var process = new Process { StartInfo = gatlingStartInfo })
            {
                process.Start();
                process.WaitForExit();

                var results = new RunResults
                {
                    RunId = runSettings.RunId,
                    ConsoleOutput = (await process.StandardOutput.ReadToEndAsync())
                        .Split("\n")
                        .ToList()
                };

                return results;
            }
        }
    }
}
