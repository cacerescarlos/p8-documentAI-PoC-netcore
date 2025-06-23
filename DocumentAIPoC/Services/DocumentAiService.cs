using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentAIPoC.Services
{
    /// <summary>
    /// Servicio para procesar documentos PDF usando múltiples procesadores de Google Document AI.
    /// </summary>
    public class DocumentAiService
    {
        private readonly IConfiguration _config;

        public DocumentAiService(IConfiguration config)
        {
            _config = config;

            var credentialRelativePath = config["DocumentAI:CredentialFile"];
            var credentialFilePath = Path.Combine(AppContext.BaseDirectory, credentialRelativePath);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);
        }

        /// <summary>
        /// Procesa un documento usando Document OCR (básico, solo texto sin estructura).
        /// </summary>
        public async Task<Document> ProcessOcrAsync(byte[] fileBytes)
        {
            var processorId = _config["DocumentAI:OcrID"];
            return await ProcessWithProcessorIdAsync(processorId, fileBytes);
        }


        /// <summary>
        /// Procesa un documento usando el Form Parser.
        /// </summary>
        public async Task<Document> ProcessFormParserAsync(byte[] fileBytes)
        {
            var processorId = _config["DocumentAI:FormParserID"];
            return await ProcessWithProcessorIdAsync(processorId, fileBytes);
        }

        /// <summary>
        /// Procesa un documento usando el Summarizer.
        /// </summary>
        public async Task<Document> ProcessSummarizerAsync(byte[] fileBytes)
        {
            var processorId = _config["DocumentAI:SummarizerID"];
            return await ProcessWithProcessorIdAsync(processorId, fileBytes);
        }

        /// <summary>
        /// Procesa un documento usando el Custom Extractor.
        /// </summary>
        public async Task<Document> ProcessCustomExtractorAsync(byte[] fileBytes)
        {
            var processorId = _config["DocumentAI:CustomExtractorID"];
            return await ProcessWithProcessorIdAsync(processorId, fileBytes);
        }

        /// <summary>
        /// Método privado reutilizable para invocar cualquier ProcessorID.
        /// </summary>
        private async Task<Document> ProcessWithProcessorIdAsync(string processorId, byte[] fileBytes)
        {
            var client = await DocumentProcessorServiceClient.CreateAsync();

            var request = new ProcessRequest
            {
                Name = processorId,
                RawDocument = new RawDocument
                {
                    Content = ByteString.CopyFrom(fileBytes),
                    MimeType = "application/pdf"
                }
            };

            var response = await client.ProcessDocumentAsync(request);
            return response.Document;
        }
    }
}
