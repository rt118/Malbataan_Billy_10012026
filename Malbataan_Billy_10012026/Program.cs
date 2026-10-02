using Malbataan_Billy_10012026.Extensions;
using Malbataan_Billy_10012026.Validators; 
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register processing services (CSV/JSON) via your extension
builder.Services.AddProcessingServices(); 
// Register OpenAPI configuration from extension
builder.Services.AddCustomOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
} 
// Register your custom validation middleware here
app.UseMiddleware<ApiKeyValidator>();

app.UseHttpsRedirection(); 
app.UseAuthorization(); 
app.MapControllers(); 
app.Run();
