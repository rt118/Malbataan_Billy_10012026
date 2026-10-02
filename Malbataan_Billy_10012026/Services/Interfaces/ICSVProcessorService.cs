using Malbataan_Billy_10012026.Models;

namespace Malbataan_Billy_10012026.Services.Interfaces
{
    public interface ICSVProcessorService
    {
        public Task<CSVResponse> ProcessCSVFile(IFormFile file, string aggregate);
        public string? Validate(string aggregate);
    }
}
