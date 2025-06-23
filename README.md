# Document AI: Revolucionando la Gestión de Documentos con Inteligencia Artificial
Montando un Proof of Concept (PoC) para procesar PDFs con IA usando Google Document AI + .NET Core.  El objetivo: recibir documentos, extraer campos clave como NIT, devolver resultados limpios vía API y dejar todo documentado
<div align="center">
<img src="https://servinformacion.com/wp-content/uploads/2024/04/Document-AI.png.webp" align="center" style="width: 100%" />
</div>  

## 📄 DocumentAIPoC — Prueba de Concepto .NET Core + Google Document AI

Este README explica cómo configurar, ejecutar y mantener un PoC que conecta una API ASP.NET Core con Google Document AI.

---

## ✅ Requisitos previos
## ✅ Configuración paso a paso en Google Cloud Document AI

### 1️⃣ Crear proyecto en Google Cloud Console

* Accede a [https://console.cloud.google.com/](https://console.cloud.google.com/)
* Crea un proyecto nuevo o usa uno existente.

### 2️⃣ Habilitar facturación

* Ve a **Facturación** en el menú lateral.
* Asocia tu proyecto a una cuenta de facturación activa (tarjeta o crédito gratuito).

### 3️⃣ Habilitar la API Document AI

* Ve a **API y servicios → Biblioteca**.
* Busca `Document AI API` y haz clic en **Habilitar**.

### 4️⃣ Crear Processor (Form Parser)

* Ve a **Document AI → Processors**.
* Haz clic en **Explorar procesadores**.
* Elige **Form Parser** (ideal para formularios estructurados).
* Asigna un nombre y región (`us` o `us-central1`).
* Guarda el **Processor ID** completo: `projects/PROJECT_ID/locations/LOCATION/processors/PROCESSOR_ID`.

### 5️⃣ Crear Service Account

* Ve a **IAM y administración → Cuentas de servicio**.
* Crea una nueva Service Account (ej. `documentai-poc-user`).
* Asigna roles:

  * **Usuario de la API de Document AI** (obligatorio)
  * **Editor de Document AI** (opcional, recomendado para PoC)

### 6️⃣ Generar clave JSON

* Dentro de la Service Account, ve a la pestaña **Claves**.
* Haz clic en **Agregar clave → Crear clave nueva → JSON**.
* Descarga el archivo `service-account.json` y guárdalo en `/credentials/`.

### 7️⃣ Verificar permisos

* Asegúrate de que la cuenta de servicio tenga los roles correctos.
* Si usas IAM Conditions, no agregues restricciones para este PoC.

### 8️⃣ Probar Processor desde la consola

* Ve a **Document AI → Mis procesadores**.
* Sube un PDF de ejemplo y confirma que se procesa correctamente.

Con estos pasos, tu proyecto está listo para integrarse con la API desde tu backend .NET Core. ✅


## Previo a Implementación

* .NET 8 o superior
* API Document AI habilitada en Google Cloud
* Processor configurado (Form Parser o Custom Extractor)
* Service Account con roles:

  * Usuario de la API de Document AI
  * (Opcional) Editor de Document AI


## 📦 Instalación de paquete necesario

Antes de compilar el proyecto, instala el cliente oficial de Google Document AI:

```bash
dotnet add package Google.Cloud.DocumentAI.V1
```

Este paquete incluye:

* `DocumentProcessorServiceClient`
* Tipos `Document`, `ProcessRequest`, `RawDocument`

---

## ✅ Estructura del proyecto

```plaintext
DocumentAIPoC/
 ├─ Controllers/
 │   └─ DocumentController.cs
 ├─ Services/
 │   └─ DocumentAiService.cs
 ├─ credentials/
 │   ├─ service-account.json
 │   └─ service-account-example.json
 ├─ appsettings.json
 ├─ Program.cs
 ├─ .gitignore
 └─ README.md
```

---

## ✅ Configuración de credenciales

1. Crear Service Account en Google Cloud Console
2. Descargar clave JSON y guardarla en `/credentials/`
3. Excluir la carpeta `credentials/` en `.gitignore`

---

## ✅ Asignar roles

En IAM & Admin → Service Accounts:

* Agregar roles:

  * **Usuario de la API de Document AI** (obligatorio)
  * **Editor de Document AI** (opcional, recomendado para PoC)

---

## ✅ appsettings.json

```json
{
  "DocumentAI": {
    "ProcessorID": "projects/PROJECT_ID/locations/LOCATION/processors/PROCESSOR_ID",
    "CredentialFile": "credentials/service-account.json"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## ✅ .gitignore

```plaintext
credentials/
*.json
```

---

## ✅ Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar servicio custom con DI
builder.Services.AddScoped<DocumentAiService>();

// Configurar variable GOOGLE_APPLICATION_CREDENTIALS embebida
var credentialRelativePath = builder.Configuration["DocumentAI:CredentialFile"];
var credentialFilePath = Path.Combine(AppContext.BaseDirectory, credentialRelativePath);
Console.WriteLine($"[INFO] GOOGLE_APPLICATION_CREDENTIALS resolved to: {credentialFilePath}");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## ✅ DocumentAiService.cs

```csharp
using Google.Cloud.DocumentAI.V1;
using Google.Protobuf;

namespace DocumentAIPoC.Services;

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

    public async Task<Document> ProcessDocumentAsync(byte[] fileBytes)
    {
        var client = await DocumentProcessorServiceClient.CreateAsync();

        var request = new ProcessRequest
        {
            Name = _processorName,
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
```

---

## ✅ UploadDocumentRequest.cs

```csharp
public class UploadDocumentRequest
{
    public IFormFile File { get; set; }
}
```

---

## ✅ DocumentController.cs

```csharp
using DocumentAIPoC.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAIPoC.Controllers;

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
```

---

## ✅ Pruebas en Swagger

1. Ejecuta `dotnet run`
2. Abre `/swagger`
3. Testea `GET /api/document/ping` → "API OK ✔️"
4. Testea `POST /api/document/upload` → Subir PDF → Recibir JSON con `Text` + `Fields`.

---

## ✅ Buenas prácticas finales

* Mantén `credentials/` fuera del repositorio.
* Usa rutas relativas y `AppContext.BaseDirectory` para portabilidad.
* Controla roles IAM con mínimo privilegio necesario.
* Documenta tu ProcessorID en `appsettings`.

---

## 🚀 ¡Listo para producción, staging o demos!


