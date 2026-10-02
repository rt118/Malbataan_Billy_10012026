using Malbataan_Billy_10012026.Filters;
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
        private static readonly JsonSerializerOptions RuleOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

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
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] UploadRequest request)
        { 
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant(); 
            var safeFileName = Path.GetFileName(file.FileName);
            try
            {
               return extension switch
                {
                    ".json" => await HandleJsonAsync(file, request, HttpContext.RequestAborted),
                    ".csv" => await HandleCSVAsync(file, request, HttpContext.RequestAborted)
                };
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

        private async Task<IActionResult> HandleJsonAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        { 
            var rule = JsonSerializer.Deserialize<FilterRule>(request.Filters, RuleOptions);
            var validateRule = _jsonProcessor.Validate(rule);
            if (!string.IsNullOrEmpty(validateRule))
            {
                return BadRequest(new
                {
                    Error = "InvalidFilterRule",
                    Message = validateRule
                });
            } 
            var jsonesult = await _jsonProcessor.ProcessJsonFile(file, rule);
            return Ok(jsonesult);
        }


        private async Task<IActionResult> HandleCSVAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        {
            var csvResult = await _csvProcessor.ProcessCSVFile(file, request.Aggregate);
            return Ok(csvResult);
        }
    }
}
