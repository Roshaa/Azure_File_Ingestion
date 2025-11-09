using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fileingest_blob_upload;

public class CosmosPersist
{
    private readonly ILogger<CosmosPersist> _logger;

    public CosmosPersist(ILogger<CosmosPersist> logger)
    {
        _logger = logger;
    }

    [Function("CosmosPersist")]
    public async Task Run(
        [BlobTrigger("uploads/{name}", Connection = "BLOB_CONNECTION_STRING")]
        Stream stream,
        string name,
        FunctionContext context)
    {

        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        _logger.LogInformation(
            "Blob trigger processed blob {Name}, size {Size} bytes",
            name,
            content.Length);

    }
}
