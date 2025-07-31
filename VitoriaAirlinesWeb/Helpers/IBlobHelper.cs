namespace VitoriaAirlinesWeb.Helpers
{
    /// <summary>
    /// Defines the contract for a helper class that interacts with Azure Blob Storage.
    /// </summary>
    public interface IBlobHelper
    {
        /// <summary>
        /// Uploads a file to a specified Azure Blob Storage container.
        /// </summary>
        /// <param name="file">The IFormFile representing the file to upload.</param>
        /// <param name="containerName">The name of the blob container.</param>
        /// <returns>
        /// Task: A Guid representing the unique name of the uploaded blob.
        /// </returns>
        Task<Guid> UploadBlobAsync(IFormFile file, string containerName);
    }
}
