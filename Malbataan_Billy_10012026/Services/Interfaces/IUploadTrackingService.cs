using Malbataan_Billy_10012026.Models;

namespace Malbataan_Billy_10012026.Services.Interfaces
{
    public interface IUploadTrackingService
    {
        public void Record(FileProcessingRecord record);
        public ProcessingReport GetReport(int recentCount = 20);
    }
}
