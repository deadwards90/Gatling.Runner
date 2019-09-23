using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Gatling.Runner.Models;
using Newtonsoft.Json;

namespace Gatling.Runner.Services
{
    public class FileService
    {
        public async Task<RunSettings> CreateRunFolders(Guid runId, Stream requestStream)
        {
            var gatlingHomeFolder = Environment.GetEnvironmentVariable("GATLING_HOME");

            using (var memoryStream = new MemoryStream())
            {
                await requestStream.CopyToAsync(memoryStream);
                await System.IO.File.WriteAllBytesAsync($"/tmp/{runId}.zip", memoryStream.ToArray());
                Directory.CreateDirectory($"/tmp/{runId}");
                Directory.CreateDirectory($"/tmp/{runId}/results");
                ZipFile.ExtractToDirectory($"/tmp/{runId}.zip", $"/tmp/{runId}");
            }

            var runSettings = JsonConvert.DeserializeObject<RunSettings>(System.IO.File.ReadAllText($"/tmp/{runId}/run.json"));
            runSettings.GatlingPath = $"{gatlingHomeFolder}/bin/gatling.sh";
            runSettings.RunId = runId;
            return runSettings;
        }

        public Stream GetReportsStream(Guid runId)
        {
            var fileName = $"/tmp/{runId}.results.zip";
            if(File.Exists(fileName))
                return new FileStream(fileName, FileMode.Open, FileAccess.Read);

            ZipFile.CreateFromDirectory($"/tmp/{runId}/results", fileName);
            return new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }
    }
}
