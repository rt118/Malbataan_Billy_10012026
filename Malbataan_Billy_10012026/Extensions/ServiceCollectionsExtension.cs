using Malbataan_Billy_10012026.Services;
using Malbataan_Billy_10012026.Services.Interfaces;

namespace Malbataan_Billy_10012026.Extensions
{
    public static class ServiceCollectionsExtension
    {
        public static IServiceCollection AddProcessingServices(this IServiceCollection services)
        {
            // Register processing services for DI. Scoped is a safe default for per-request lifetime.
            services.AddScoped<ICSVProcessorService, CSVProcessorService>();
            services.AddScoped<IJSONProcessorService, JSONProcessorService>();
            services.AddSingleton<IUploadTrackingService, UploadTrackingService>();
            return services;
        }
    }
}
