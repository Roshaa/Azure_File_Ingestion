using Azure_File_Ingestion.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Azure_File_Ingestion.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
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
        public IActionResult Upload([FromForm] FileUploadDto dto)
        {
            return Ok(new { dto.Name, dto.Contact, dto.File.FileName, dto.File.Length });
        }

        [HttpGet]
        public IActionResult Poke()
        {
            return Ok("Yes i am here v3");
        }

    }
}
