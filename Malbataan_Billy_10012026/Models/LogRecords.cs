namespace Malbataan_Billy_10012026.Models
{  
    public class FileProcessingRecord
    {
        public string Filename { get; set; }
        public string FileType { get; set; }
        public long SizeBytes { get; set; }
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public double ProcessingTimeMs { get; set; }
        public DateTimeOffset ProcessedAtUtc { get; set; }
    }

    public class ProcessingReport
    {
        public int TotalFiles { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public IReadOnlyDictionary<string, int> FilesByType { get; set; }
        public double AverageProcessingTimeMs { get; set; }
        public IReadOnlyList<FileProcessingRecord> RecentFiles { get; set; }
    }
}
