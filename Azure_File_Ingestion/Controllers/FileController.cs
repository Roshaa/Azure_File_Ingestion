using Azure_File_Ingestion.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace Azure_File_Ingestion.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FileController(IHttpClientFactory _httpClientFactory, IConfiguration _configuration) : ControllerBase
    {
        private readonly string _functionCode =
            _configuration["BLOBUPLOAD_FUNCTION_CODE"]
            ?? throw new InvalidOperationException("BLOBUPLOAD_FUNCTION_CODE missing");

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

            await using var s = dto.File.OpenReadStream();

            using var content = new StreamContent(s);

            content.Headers.ContentType = new MediaTypeHeaderValue(dto.File.ContentType ?? "application/octet-stream");
            content.Headers.ContentLength = dto.File.Length;
            content.Headers.Add("x-file-name", dto.File.FileName);
            content.Headers.Add("x-contact", dto.Contact);

            HttpResponseMessage resp = await http.PostAsync($"BlobUpload?code={_functionCode}", content, ct);

            string body = await resp.Content.ReadAsStringAsync(ct);

            if (resp.StatusCode == HttpStatusCode.BadRequest)
                return BadRequest(body);

            if ((int)resp.StatusCode == 500)
                return StatusCode(500, "Unexpected server error");

            return StatusCode((int)resp.StatusCode, new { status = resp.StatusCode.ToString(), body });
        }

        [HttpGet]
        public IActionResult Poke() => Ok("Yes i am here v6");

    }
}
