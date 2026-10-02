using Malbataan_Billy_10012026.Models;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Malbataan_Billy_10012026.Extensions
{
    public static class OpenApiConfigurationExtension
    {
        private const string SchemeId = "ApiKeyScheme";

        public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                // ensure components and the scheme exist
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                    if (!document.Components.SecuritySchemes.ContainsKey(SchemeId))
                    {
                        document.Components.SecuritySchemes.Add(SchemeId, new OpenApiSecurityScheme
                        {
                            Type = SecuritySchemeType.ApiKey,
                            Name = "X-API-KEY",
                            In = ParameterLocation.Header,
                            Description = "Enter your secret API Key to execute this endpoint."
                        });
                    }

                    foreach (var path in document.Paths.Values)
                    {
                        foreach (var operation in path.Operations.Values)
                        {
                            operation.Security ??= new List<OpenApiSecurityRequirement>();
                            operation.Security.Add(new OpenApiSecurityRequirement
                            {
                                [new OpenApiSecuritySchemeReference(SchemeId, document)] = []
                            });
                        }
                    }

                    return Task.CompletedTask;
                }); 
            }); 
            return services;
        }
    }
}
