using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace fileingest_blob_upload;

public class BlobUpload(ILogger<BlobUpload> _logger)
{
    [Function("BlobUpload")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        string filename = req.Headers.GetValues("x-file-name").FirstOrDefault() ?? "unnamed";

        if (string.IsNullOrEmpty(filename))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Missing x-file-name header");
            return badRequest;
        }

        //TODO SAVE TO BLOB STORAGE
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"OK, file received: {filename} ");
        return response;
    }
}