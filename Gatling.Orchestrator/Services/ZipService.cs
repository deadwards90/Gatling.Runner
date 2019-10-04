using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage.Blob;

namespace Gatling.Orchestrator.Services
{
    public interface IFileService
    {
        Task SaveTestZipFile(IFormFile testFormFile);
        Task SaveTestResults(string filename, Stream fileStream);
        Task<byte[]> GetFile(string container, string filename);
        string GetFileUrl(string container, string filename);
    }

    public class FileService : IFileService
    {
        public const string TestZipsContainer = "test-zips";
        public const string TestResultsContainer = "results";
        private readonly CloudBlobClient _blobClient;

        public FileService(CloudBlobClient blobClient)
        {
            _blobClient = blobClient;
        }

        public async Task SaveTestZipFile(IFormFile testFormFile)
        {
            var zipsContainer = _blobClient.GetContainerReference(TestZipsContainer);

            await zipsContainer.CreateIfNotExistsAsync();

            var testBlob = zipsContainer.GetBlockBlobReference(testFormFile.FileName);

            await testBlob.DeleteIfExistsAsync();

            using (var fileStream = testFormFile.OpenReadStream())
            {
                await testBlob.UploadFromStreamAsync(fileStream);
            }
        }

        public async Task SaveTestResults(string filename, Stream fileStream)
        {
            var resultsContainer = _blobClient.GetContainerReference(TestResultsContainer);

            await resultsContainer.CreateIfNotExistsAsync();

            var testBlob = resultsContainer.GetBlockBlobReference(filename);

            await testBlob.DeleteIfExistsAsync();
            await testBlob.UploadFromStreamAsync(fileStream);
        }

        public async Task<byte[]> GetFile(string container, string filename)
        {
            using (var stream = await _blobClient.GetContainerReference(container)
                .GetBlobReference(filename)
                .OpenReadAsync())
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public string GetFileUrl(string container, string filename)
        {
            var blobReference = _blobClient.GetContainerReference(container)
                .GetBlobReference(filename);

            var signature = blobReference
                .GetSharedAccessSignature(new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddDays(1)
                });

            return blobReference.Uri + signature;
        }
    }
}
