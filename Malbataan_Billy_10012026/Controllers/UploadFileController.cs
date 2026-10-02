using Malbataan_Billy_10012026.Filters;
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Malbataan_Billy_10012026.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly ICSVProcessorService _csvProcessor;
        private readonly IJSONProcessorService _jsonProcessor;
        private readonly ILogger<UploadFileController> _logger;

        public UploadFileController(
            ICSVProcessorService csvProcessor,
            IJSONProcessorService jsonProcessor,
            ILogger<UploadFileController> logger)
        {
            _csvProcessor = csvProcessor ?? throw new ArgumentNullException(nameof(csvProcessor));
            _jsonProcessor = jsonProcessor ?? throw new ArgumentNullException(nameof(jsonProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

     
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [FileValidator] 
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] FilterRequest Filter)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded or the file is empty.");
            }

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant(); 
            var safeFileName = Path.GetFileName(file.FileName);

            try
            {
                if (extension == ".csv")
                {
                    var csvResult = await _csvProcessor.ProcessCSVFile(file, Filter.Aggregate);
                    return Ok(csvResult);
                }
                else
                {
                    await _jsonProcessor.ProcessJsonFile(file);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file {FileName}", safeFileName);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "ProcessingFailed",
                    Message = "An error occurred while processing the uploaded file."
                });
            }
        } 
    }
}
