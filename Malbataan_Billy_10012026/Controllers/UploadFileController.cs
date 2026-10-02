using Malbataan_Billy_10012026.Filters;
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Malbataan_Billy_10012026.Controllers
{
    /// <summary>
    /// Controller responsible for handling file uploads (CSV and JSON), delegating processing
    /// to the configured processor services and recording processing metrics.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileController"/> class.
        /// </summary>
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

        /// <summary>
        /// Receives a multipart/form-data file upload and routes processing based on file extension.
        /// Supports JSON and CSV files. Records processing results via the tracking service.
        /// </summary>
        /// <param name="File">The uploaded file (required).</param>
        /// <param name="request">Form data containing optional parameters (filters for JSON, aggregate for CSV).</param> 
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [FileValidator] 
        public async Task<IActionResult> UploadFile(IFormFile File, [FromForm] UploadRequest request)
        { 
            var extension = Path.GetExtension(File.FileName)?.ToLowerInvariant(); 
            var fileName = Path.GetFileName(File.FileName);
            var stopwatch = Stopwatch.StartNew();

            try
            { 
                IActionResult result = extension switch
                {
                    ".json" => await HandleJsonAsync(File, request, HttpContext.RequestAborted),
                    ".csv" => await HandleCSVAsync(File, request, HttpContext.RequestAborted)
                };
                var error = (result as ObjectResult)?.Value is ProblemDetails problem ? problem.Detail : null;
                _tracker.Record(BuildRecord(fileName, extension, File.Length, stopwatch, succeeded: result is OkObjectResult, error));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing uploaded file {FileName}", fileName);
                _tracker.Record(BuildRecord(fileName, extension, File.Length, stopwatch, succeeded: false, ex.Message));
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Error = "ProcessingFailed",
                    Message = ex.Message.ToString()
                });
            }
        }

        /// <summary>
        /// Handles processing of a JSON upload: deserializes provided filter rules, validates them,
        /// invokes the JSON processor, and returns the processor result.
        /// </summary>
        private async Task<IActionResult> HandleJsonAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        { 
            var rule = JsonSerializer.Deserialize<FilterRule>(request.Filters, RuleOptions);
            var validateRule = _jsonProcessor.Validate(rule);
            if (!string.IsNullOrEmpty(validateRule))
                throw new Exception($"Invalid filter rule: {validateRule}");

            var jsonesult = await _jsonProcessor.ProcessJsonFile(file, rule);
            return Ok(jsonesult);
        }

        /// <summary>
        /// Handles processing of a CSV upload by invoking the CSV processor with the requested aggregate.
        /// </summary>
        private async Task<IActionResult> HandleCSVAsync(IFormFile file, UploadRequest request, CancellationToken ct)
        {
            var validateRule = _csvProcessor.Validate(request.Aggregate);
            if (!string.IsNullOrEmpty(validateRule))
                throw new Exception($"Invalid aggregate value: {validateRule}");
            var csvResult = await _csvProcessor.ProcessCSVFile(file, request.Aggregate); 
            return Ok(csvResult);
        }

        /// <summary>
        /// Builds a <see cref="FileProcessingRecord"/> from processing metadata.
        /// </summary>
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
