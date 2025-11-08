using Azure_File_Ingestion.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Azure_File_Ingestion.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController(IHttpClientFactory _httpClientFactory) : ControllerBase
    {
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(5_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 5_000_000)]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto, CancellationToken ct = default)
        {
            HttpClient http = _httpClientFactory.CreateClient("blobUploadFunction");
            HttpResponseMessage response = await http.GetAsync("BlobUpload", ct);

            string requestUri = response.RequestMessage?.RequestUri?.ToString();
            string body = await response.Content.ReadAsStringAsync(ct);

            return StatusCode((int)response.StatusCode, new
            {
                called = requestUri,
                statusCode = (int)response.StatusCode,
                status = response.StatusCode.ToString(),
                body
            });
        }

        [HttpGet]
        public IActionResult Poke()
        {
            return Ok("Yes i am here v5");
        }

    }
}
