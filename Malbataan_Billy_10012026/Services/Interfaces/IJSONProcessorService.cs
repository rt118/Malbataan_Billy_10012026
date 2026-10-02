using Malbataan_Billy_10012026.Models;

namespace Malbataan_Billy_10012026.Services.Interfaces
{
    public interface IJSONProcessorService
    {
        public Task<JSONResponse> ProcessJsonFile(IFormFile file, FilterRule? rule);
        public string? Validate(FilterRule rule);
    }
}
