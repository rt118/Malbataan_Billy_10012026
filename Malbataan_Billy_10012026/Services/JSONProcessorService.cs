using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using System.Text.Json;

namespace Malbataan_Billy_10012026.Services
{
    public class JSONProcessorService : IJSONProcessorService
    {
        public async Task ProcessJsonFile(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();

            // 3. Configure case-insensitive options to easily match json keys to C# records
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // 4. Deserialize the stream into a List of C# objects
            var users = await JsonSerializer.DeserializeAsync<List<Employee>>(fileStream, options);


        }
    }
}
