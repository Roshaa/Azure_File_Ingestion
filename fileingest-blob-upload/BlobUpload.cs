using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace fileingest_blob_upload;

public class BlobUpload(BlobContainerClient container, ILogger<BlobUpload> _logger)
{
    [Function("BlobUpload")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Processing blob upload request");

        if (!req.Headers.TryGetValues("x-file-name", out var values))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Missing x-file-name header");
            return bad;
        }

        string originalFileName = values.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(originalFileName))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid file name");
            return bad;
        }

        _logger.LogInformation("Validations ok, Uploading file {FileName}", originalFileName);

        string ext = Path.GetExtension(originalFileName);
        string blobName = $"{Guid.NewGuid():N}{ext}";

        BlobClient blob = container.GetBlobClient(blobName);

        BlobUploadOptions options = new BlobUploadOptions
        {
            Metadata = new Dictionary<string, string>
            {
                { "originalFileName", originalFileName }
            }
        };

        await blob.UploadAsync(req.Body, options);
        _logger.LogInformation("File uploaded as blob {BlobName}", blobName);

        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            blobName,
            originalFileName
        });

        return response;
    }
}