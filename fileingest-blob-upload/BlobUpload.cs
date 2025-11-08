using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fileingest_blob_upload;

public class BlobUpload
{
    private readonly ILogger<BlobUpload> _logger;

    public BlobUpload(ILogger<BlobUpload> logger)
    {
        _logger = logger;
    }

    [Function("BlobUpload")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}