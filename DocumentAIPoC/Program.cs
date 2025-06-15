using DocumentAIPoC.Services;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar DocumentAiService para DI
builder.Services.AddScoped<DocumentAiService>();

// Configura credencial de google embebida
var credentialRelativePath = builder.Configuration["DocumentAI:CredentialFile"];
var credentialFilePath = Path.Combine(AppContext.BaseDirectory, credentialRelativePath);
Console.WriteLine($"[INFO] GOOGLE_APPLICATION_CREDENTIALS resolved to: {credentialFilePath}");
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialFilePath);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
