using Malbataan_Billy_10012026.Validators; 
using Microsoft.OpenApi; 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
 builder.Services.AddOpenApi(options =>
{
   //Define the API Key scheme safely
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Fix the parent components container if null
        document.Components ??= new OpenApiComponents();

        // FIX: Explicitly initialize the SecuritySchemes dictionary if null
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
     
        if (!document.Components.SecuritySchemes.ContainsKey("ApiKeyScheme"))
        {
            document.Components.SecuritySchemes.Add("ApiKeyScheme", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Description = "Enter your secret API Key to execute this endpoint."
            });

            foreach (var path in document.Paths.Values)
            {
                foreach (var operation in path.Operations.Values)
                {
                    operation.Security ??= new List<OpenApiSecurityRequirement>(); 
                    operation.Security.Add(new OpenApiSecurityRequirement {
                            [ new OpenApiSecuritySchemeReference("ApiKeyScheme", document) ] = []
                        });
                }
            }
        } 
        return Task.CompletedTask;
    }); 
});

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
