using DocumentAIPoC.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAIPoC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentAiService _docAiService;

        public DocumentController(DocumentAiService docAiService)
        {
            _docAiService = docAiService;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API OK ✔️" });
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var document = await _docAiService.ProcessDocumentAsync(ms.ToArray());

            return Ok(new
            {
                Text = document.Text,
                Fields = document.Entities.Select(e => new
                {
                    e.Type,
                    e.MentionText,
                    e.Confidence
                })
            });
        }

    }


    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
    }
}
