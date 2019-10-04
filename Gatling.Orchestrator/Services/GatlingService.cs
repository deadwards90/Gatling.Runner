using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gatling.Orchestrator.Services
{
    public interface IGatlingService
    {
        Task StartTest(string gatlingUrl, string filename, string testId);
        Task<bool> CheckTestStatus(string gatlingUrl, string testId);
        Task<string> GetResult(string gatlingUrl, string testId, string containerGroupName);
        Task<string> MergeReports(string gatlingUrl, IEnumerable<string> simulationLogs, string fileName);
        Task CloseApplication(string gatlingUrl);
    }

    public class GatlingService : IGatlingService
    {
        private readonly IFileService _fileService;
        private readonly HttpClient _httpClient;

        public GatlingService(IHttpClientFactory httpClientFactory,
            IFileService fileService)
        {
            _fileService = fileService;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task StartTest(string gatlingUrl, string filename, string testId)
        {
            var zipTestBytes = await _fileService.GetFile(FileService.TestZipsContainer, filename);
            var byteArrayContent = new ByteArrayContent(zipTestBytes);

            await _httpClient.PostAsync($"http://{gatlingUrl}:80/api/starttestasync/{testId}", byteArrayContent);
        }

        public async Task<bool> CheckTestStatus(string gatlingUrl, string testId)
        {
            var testResult = await _httpClient.GetAsync($"http://{gatlingUrl}:80/api/checkresult/{testId}");
            switch (testResult.StatusCode)
            {
                case HttpStatusCode.OK:
                    return true;
                case HttpStatusCode.Accepted:
                    return false;
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.NotFound:
                    throw new GatlingTestFailureException($"Failure at {gatlingUrl}");
                default:
                    return false;
            }
        }

        public async Task<string> GetResult(string gatlingUrl, string testId, string containerGroupName)
        {
            var testResult =
                await _httpClient.GetAsync($"http://{gatlingUrl}:80/api/getresult/{testId}?simulationLogOnly=true");

            if (!testResult.IsSuccessStatusCode)
            {
                throw new GatlingTestFailureException($"Failure at {gatlingUrl}");
            }

            using (var testResultZip = await testResult.Content.ReadAsStreamAsync())
            {
                var resultsLogName = $"{containerGroupName}-result.log";
                await _fileService.SaveTestResults(resultsLogName, testResultZip);
                return resultsLogName;
            }
        }

        public async Task<string> MergeReports(string gatlingUrl, IEnumerable<string> simulationLogs, string fileName)
        {
            byte[] zipBytes;
            using (var outStream = new MemoryStream())
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            {
                foreach (var regionSetting in simulationLogs)
                {
                    var logBytes =
                        await _fileService.GetFile(FileService.TestResultsContainer,
                            regionSetting);

                    var fileInArchive = archive.CreateEntry(regionSetting, 
                        CompressionLevel.Optimal);
                    using (var entryStream = fileInArchive.Open())
                    using (var fileToCompressStream = new MemoryStream(logBytes))
                    {
                        fileToCompressStream.CopyTo(entryStream);
                    }
                }

                zipBytes = outStream.ToArray();
            }

            var byteArrayContent = new ByteArrayContent(zipBytes);

            var result = 
                await _httpClient.PostAsync($"http://{gatlingUrl}:80/api/generatereport", byteArrayContent);

            using (var fileStream = await result.Content.ReadAsStreamAsync())
            {
                await _fileService.SaveTestResults(fileName, fileStream);
            }

            return fileName;
        }

        public async Task CloseApplication(string gatlingUrl)
        {
            await _httpClient.PostAsync($"http://{gatlingUrl}:80/api/environment/stop", null);
        }
    }
}
