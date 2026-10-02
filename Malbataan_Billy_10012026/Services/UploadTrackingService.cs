using Malbataan_Billy_10012026.Controllers;
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Malbataan_Billy_10012026.Services
{
    public class UploadTrackingService : IUploadTrackingService 
    {
        private readonly ILogger<UploadTrackingService> _logger;
        private const int MaxHistory = 100;
        private readonly Lock _lock = new();
        private readonly Queue<FileProcessingRecord> _recent = new();
        private readonly Dictionary<string, int> _byType = new(StringComparer.OrdinalIgnoreCase);
        private int _succeeded;
        private int _failed;
        private double _totalMs;


        public UploadTrackingService(ILogger<UploadTrackingService> logger)
        {
            _logger = logger;
        } 

        public void Record(FileProcessingRecord record)
        {
            lock (_lock)
            {
                if (record.Succeeded) _succeeded++; else _failed++;
                _totalMs += record.ProcessingTimeMs;
                _byType[record.FileType] = _byType.GetValueOrDefault(record.FileType) + 1;

                _recent.Enqueue(record);
                while (_recent.Count > MaxHistory) _recent.Dequeue();
            }

            if (record.Succeeded)
            {
                _logger.LogInformation(
                    "File processed: {Filename} | type={FileType} | size={SizeBytes} B | time={ElapsedMs} ms | status=Succeeded",
                    record.Filename, record.FileType, record.SizeBytes, record.ProcessingTimeMs);
            }
            else
            {
                _logger.LogWarning(
                    "File failed: {Filename} | type={FileType} | size={SizeBytes} B | time={ElapsedMs} ms | error={Error}",
                    record.Filename, record.FileType, record.SizeBytes, record.ProcessingTimeMs, record.Error);
            }
        }

        public ProcessingReport GetReport(int recentCount = 20)
        {
            lock (_lock)
            {
                var total = _succeeded + _failed;
                return new ProcessingReport
                {
                    TotalFiles = total,
                    Succeeded = _succeeded,
                    Failed = _failed,
                    FilesByType = new Dictionary<string, int>(_byType),
                    AverageProcessingTimeMs = (total == 0 ? 0 : Math.Round(_totalMs / total, 2)),
                    RecentFiles = _recent.Reverse().Take(Math.Clamp(recentCount, 1, MaxHistory)).ToList()
                };
            } 
        }
    }
}
