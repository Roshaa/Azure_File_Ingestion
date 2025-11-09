using Azure.Storage.Blobs;
using fileingest_blob_upload.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace fileingest_blob_upload
{

    public class CosmosPersist(BlobServiceClient _blob, ILogger<CosmosPersist> _logger, Container _cosmosContainer)
    {

        [Function("CosmosPersist")]
        public async Task Run([QueueTrigger("blob-events", Connection = "BLOB_CONNECTION_STRING")] string eventJson)
        {
            using var doc = JsonDocument.Parse(eventJson);

            string url = doc.RootElement.GetProperty("data").GetProperty("url").GetString()!;
            long size = doc.RootElement.GetProperty("data").GetProperty("contentLength").GetInt64();
            var uri = new Uri(url);
            string container = uri.Segments[1].TrimEnd('/');
            string name = string.Join("", uri.Segments.Skip(2));

            var blob = _blob.GetBlobContainerClient(container).GetBlobClient(name);
            var props = (await blob.GetPropertiesAsync()).Value;

            props.Metadata.TryGetValue("originalFileName", out var originalFileName);
            props.Metadata.TryGetValue("contact", out var contact);

            var item = new IngestedFile
            {
                id = Guid.NewGuid().ToString(),
                originalFileName = originalFileName ?? "",
                blobName = name,
                created = props.CreatedOn,
                size = size,
                type = props.ContentType ?? "",
                contact = contact
            };

            await _cosmosContainer.UpsertItemAsync(item, new PartitionKey(item.contact));
            _logger.LogInformation("saved {Name} {Size}", name, item.size);
        }
    }
}