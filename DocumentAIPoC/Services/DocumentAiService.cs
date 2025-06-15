using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;
using System.Threading.Tasks;

namespace DocumentAIPoC.Services
{
    /// <summary>
    /// Servicio para procesar documentos PDF usando Google Document AI.
    /// </summary>
    public class DocumentAiService
    {
        private readonly string _processorName;

        public DocumentAiService(IConfiguration configuration)
        {
            _processorName = configuration["DocumentAI:ProcessorID"];

            var credentialRelativePath = configuration["DocumentAI:CredentialFile"];
            var credentialFilePath = Path.Combine(AppContext.BaseDirectory, credentialRelativePath);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);
        }


        /// <summary>
        /// Procesa el archivo PDF (como bytes) usando Document AI.
        /// Devuelve un objeto 'Google.Cloud.DocumentAI.V1.Document' estructurado.
        /// </summary>
        public async Task<Google.Cloud.DocumentAI.V1.Document> ProcessDocumentAsync(byte[] fileBytes)
        {
            // Crea el cliente
            var client = await DocumentProcessorServiceClient.CreateAsync();

            // Construye la petición
            var request = new ProcessRequest
            {
                Name = _processorName,
                RawDocument = new RawDocument
                {
                    Content = ByteString.CopyFrom(fileBytes),
                    MimeType = "application/pdf"
                }
            };

            // Llama a Document AI y devuelve el objeto Document
            var response = await client.ProcessDocumentAsync(request);
            return response.Document;
        }
    }
}
