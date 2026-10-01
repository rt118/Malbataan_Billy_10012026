using Microsoft.Extensions.Logging;
using System;
namespace Malbataan_Billy_10012026.Validators
{
    public class ApiKeyValidator
    {
        private readonly RequestDelegate _next;
        private const string ApiKeyHeader = "X-API-KEY";
        private readonly string _expectedApiKey;
        private readonly ILogger<ApiKeyValidator> _logger;


        public ApiKeyValidator(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyValidator> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _expectedApiKey = configuration?["Authentication:ApiKey"]
                ?? throw new InvalidOperationException("Missing Authentication:ApiKey in configuration.");

        }

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            // 2. Target only your backend API routes (ignores requests for static files, etc.)
            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
                {
                    _logger.LogWarning("API key header '{Header}' not present. Request headers: {Headers}", ApiKeyHeader, string.Join(", ", context.Request.Headers.Keys));
                }

                if (string.IsNullOrEmpty(extractedApiKey) || !string.Equals(extractedApiKey, _expectedApiKey, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Unauthorized request to {Path}.", context.Request.Path);
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        Error = "Unauthorized Access",
                        Message = "The provided API Key is invalid or missing from the headers."
                    });

                    return;
                }
            }

            // 5. If valid, pass the request to the next step in the pipeline
            await _next(context);
        }
    }
}
