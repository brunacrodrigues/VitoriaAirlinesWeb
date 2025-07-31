using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Provides helper methods for interacting with Azure Blob Storage.
    /// This includes uploading files to specified containers.
    /// </summary>
    public class BlobHelper : IBlobHelper
    {
        private readonly BlobServiceClient _blobClient;


        /// <summary>
        /// Initializes a new instance of the BlobHelper class.
        /// </summary>
        /// <param name="configuration">The application's configuration, used to retrieve the Azure Blob Storage connection string.</param>

        public BlobHelper(IConfiguration configuration)
        {
            string keys = configuration["Blob:ConnectionString"];
            _blobClient = new BlobServiceClient(keys);
        }


        /// <summary>
        /// Uploads a file (IFormFile) to a specified Azure Blob Storage container.
        /// If the container does not exist, it will be created with public blob access.
        /// A new GUID is generated as the blob name.
        /// </summary>
        /// <param name="file">The IFormFile representing the file to upload.</param>
        /// <param name="containerName">The name of the blob container where the file will be uploaded.</param>
        /// <returns>
        /// Task: A Guid representing the unique name given to the uploaded blob.
        /// </returns>
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
