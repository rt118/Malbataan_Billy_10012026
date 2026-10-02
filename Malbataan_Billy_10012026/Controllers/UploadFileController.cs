using Malbataan_Billy_10012026.Filters;
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
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
        private readonly IUploadTrackingService _tracker;

        private static readonly JsonSerializerOptions RuleOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UploadFileController(
            ICSVProcessorService csvProcessor,
            IJSONProcessorService jsonProcessor,
            IUploadTrackingService tracker,
            ILogger<UploadFileController> logger)
        {
            _csvProcessor = csvProcessor ?? throw new ArgumentNullException(nameof(csvProcessor));
            _jsonProcessor = jsonProcessor ?? throw new ArgumentNullException(nameof(jsonProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracker = tracker ?? throw new ArgumentNullException(nameof(tracker));
        }

     
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [FileValidator] 
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] UploadRequest request)
        { 
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant(); 
            var fileName = Path.GetFileName(file.FileName);
            var stopwatch = Stopwatch.StartNew();

            try
            { 
                IActionResult result = extension switch
                {
                    ".json" => await HandleJsonAsync(file, request, HttpContext.RequestAborted),
                    ".csv" => await HandleCSVAsync(file, request, HttpContext.RequestAborted)
                };
                var error = (result as ObjectResult)?.Value is ProblemDetails problem ? problem.Detail : null;
                _tracker.Record(BuildRecord(fileName, extension, file.Length, stopwatch, succeeded: result is OkObjectResult, error));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file {FileName}", fileName);
                _tracker.Record(BuildRecord(fileName, extension, file.Length, stopwatch, succeeded: false, ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "ProcessingFailed",
                    Message = ex.Message.ToString()
                });
            }
        }

        private async Task<IActionResult> HandleJsonAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        { 
            var rule = JsonSerializer.Deserialize<FilterRule>(request.Filters, RuleOptions);
            var validateRule = _jsonProcessor.Validate(rule);
            if (!string.IsNullOrEmpty(validateRule))
                throw new Exception($"Invalid filter rule: {validateRule}");

            var jsonesult = await _jsonProcessor.ProcessJsonFile(file, rule);
            return Ok(jsonesult);
        }


        private async Task<IActionResult> HandleCSVAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        {
            var csvResult = await _csvProcessor.ProcessCSVFile(file, request.Aggregate); 
            return Ok(csvResult);
        }

        private static FileProcessingRecord BuildRecord(
        string fileName, string extension, long fileLength, Stopwatch stopwatch, bool succeeded, string? error) =>
        new FileProcessingRecord
        {
            Filename = fileName,
            FileType = string.IsNullOrEmpty(extension) ? "unknown" : extension.TrimStart('.'),
            SizeBytes = fileLength,
            Succeeded = succeeded,
            Error = error,
            ProcessingTimeMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2),
            ProcessedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
