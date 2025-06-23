using DocumentAIPoC.Services;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

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

        /// <summary>
        /// Test rápido para verificar que la API funciona.
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API OK" });
        }

        [HttpGet("test-pdf")]
        public IActionResult GetTestPdf()
        {
            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 20);
            gfx.DrawString("¡PdfSharpCore instalado y funcionando!", font, XBrushes.Black, new XPoint(100, 100));

            var ms = new MemoryStream();
            pdf.Save(ms, false);
            ms.Position = 0;
            return File(ms, "application/pdf", "Test_PdfSharpCore.pdf");
        }

        /// <summary>
        /// Procesa el documento con Form Parser.
        /// </summary>
        [HttpPost("form-parser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ProcessFormParser([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var document = await _docAiService.ProcessFormParserAsync(ms.ToArray());

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


        /// <summary>
        /// Procesa el documento con Summarizer.
        /// </summary>
        [HttpPost("summarize")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Summarize([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var summaryDocument = await _docAiService.ProcessSummarizerAsync(ms.ToArray());

            return Ok(new
            {
                Summary = summaryDocument.Text
            });
        }


        [HttpPost("summarize-pdf")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SummarizePdf([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var summaryDoc = await _docAiService.ProcessSummarizerAsync(ms.ToArray());
            var summaryText = summaryDoc.Text.Replace("\n", " ").Replace("\r", " ");

            var pdf = new PdfDocument();
            var page = pdf.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Verdana", 12);
            var tf = new XTextFormatter(gfx);
            tf.Alignment = XParagraphAlignment.Left;

            var rect = new XRect(40, 40, page.Width - 80, page.Height - 80);
            tf.DrawString(summaryText, font, XBrushes.Black, rect);

            var outStream = new MemoryStream();
            pdf.Save(outStream, false);
            outStream.Position = 0;

            return File(outStream, "application/pdf", "Resumen_DocumentAI.pdf");
        }




    }

    /// <summary>
    /// Modelo para subir archivo PDF.
    /// </summary>
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
    }
}
