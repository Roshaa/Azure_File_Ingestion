using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace fileingest_blob_upload;

public class CosmosPersist
{
    private readonly ILogger<CosmosPersist> _logger;
    private readonly BlobServiceClient _blob;

    public CosmosPersist(ILogger<CosmosPersist> logger)
    {
        _logger = logger;
        _blob = new BlobServiceClient(Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING"));
    }

    [Function("CosmosPersist")]
    public async Task Run([QueueTrigger("blob-events", Connection = "BLOB_CONNECTION_STRING")] string eventJson)
    {
        using var doc = JsonDocument.Parse(eventJson);
        var subject = doc.RootElement.GetProperty("subject").GetString();
        var parts = subject.Split("/blobs/");
        var container = parts[0].Split("/containers/")[1];
        var name = parts[1];

        var blob = _blob.GetBlobContainerClient(container).GetBlobClient(name);
        using var stream = await blob.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        _logger.LogInformation("Processed blob {Name}, {Size} bytes", name, content.Length);

        // TODO: write to Cosmos here
    }
}
