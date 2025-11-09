using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace fileingest_blob_upload;

public class BlobUpload(ILogger<BlobUpload> _logger)
{
    [Function("BlobUpload")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        var res = req.CreateResponse(HttpStatusCode.OK);

        await res.WriteStringAsync("TO IMPLEMENT!");

        return res;
    }
}