using DocumentAIPoC.Services;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;

namespace DocumentAIPoC.Controllers
{
    [ApiController]
    [Route("api/document")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentAiService _docAiService;

        public DocumentController(DocumentAiService docAiService)
        {
            _docAiService = docAiService;
        }

        /// <summary>
        /// Procesa el documento usando el procesador Document OCR de Google Document AI.
        /// Devuelve:
        /// - <b>Text</b>: Texto plano detectado mediante reconocimiento óptico de caracteres (OCR).
        /// No extrae campos clave ni estructura de tablas.
        /// Ideal para digitalizar escaneos simples, cartas o imágenes sin estructura.
        /// </summary>
        [HttpPost("ocr")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Ocr([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var document = await _docAiService.ProcessOcrAsync(ms.ToArray());

            return Ok(new
            {
                Text = document.Text
            });
        }

        /// <summary>
        /// Procesa el documento usando el Form Parser de Document AI.
        /// Devuelve:
        /// - <b>Text</b>: Texto crudo detectado mediante OCR.
        /// - <b>Fields</b>: Campos clave tipo key-value extraídos del formulario.
        /// - <b>Pages</b>: Información de cada página (número, dimensiones).
        /// - <b>Tables</b>: Estructura de tablas detectadas (cabeceras y filas).
        /// Ideal para formularios semi-estructurados como facturas, recibos, contratos y reportes.
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

            var Fields = document.Pages
           .SelectMany(p => p.FormFields)
           .Select(f => new
           {
               FieldName = f.FieldName?.TextAnchor?.Content,
               FieldValue = f.FieldValue?.TextAnchor?.Content
           });

            var Tables = document.Pages
                .SelectMany(p => p.Tables)
                .Select(t => new
                {
                    Headers = t.HeaderRows.Select(row => row.Cells.Select(c => c.Layout.TextAnchor.Content).ToList()).ToList(),
                    Body = t.BodyRows.Select(row => row.Cells.Select(c => c.Layout.TextAnchor.Content).ToList()).ToList()
                });

            return Ok(new
            {
                Text = document.Text,

                // Entidades semánticas, como organizaciones, nombres, etc.
                Entities = document.Entities.Select(e => new
                {
                    e.Type,
                    e.MentionText,
                    e.Confidence
                }),

                // Campos clave-valor detectados visualmente.
                Fields = Fields,

                // Tablas:
                Tables = Tables

            });
        }



        /// <summary>
        /// Procesa el documento usando el procesador Summarizer de Google Document AI.
        /// Devuelve:
        /// - <b>Text</b>: Texto resumido generado automáticamente.
        /// Diseñado para condensar información redundante y producir una versión breve.
        /// Ideal para documentos largos con contenido repetitivo o denso.
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

        /// <summary>
        /// Procesa el documento usando el procesador Custom Extractor de Google Document AI.
        /// Devuelve:
        /// - <b>Text</b>: Texto plano detectado mediante OCR.
        /// - <b>Fields</b>: Campos clave personalizados definidos y entrenados por el usuario.
        /// Extrae información específica según el modelo entrenado.
        /// Ideal para documentos con estructura propia, como recibos de pago, contratos o licencias.
        /// </summary>
        [HttpPost("custom-extractor")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CustomExtractor([FromForm] UploadDocumentRequest request)
        {
            var file = request.File;
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no proporcionado.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var document = await _docAiService.ProcessCustomExtractorAsync(ms.ToArray());

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




    }

    /// <summary>
    /// Modelo para subir archivo PDF.
    /// </summary>
    public class UploadDocumentRequest
    {
        public IFormFile File { get; set; }
    }
}
