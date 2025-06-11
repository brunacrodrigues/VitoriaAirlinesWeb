using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VitoriaAirlinesWeb.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient _blobClient;

        public BlobHelper(IConfiguration configuration)
        {
            string keys = configuration["Blob:ConnectionString"];
            _blobClient = new BlobServiceClient(keys);
        }

        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            var containerClient = _blobClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            Guid name = Guid.NewGuid();
            var blobClient = containerClient.GetBlobClient(name.ToString());

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return name;
        }
    }
}
