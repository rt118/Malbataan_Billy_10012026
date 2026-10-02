using System.Text.Json;

namespace Malbataan_Billy_10012026.Models
{
    public class CSVResponse
    {
        public string Filename { get; set; }
        public int TotalRecords { get; set; }
        public string Column { get; set; }
        public string Aggregate { get; set; }
        public double Value { get; set; }
    }

    public class JSONResponse
    {
        public string Filename { get; set; }
        public int TotalRecords { get; set; }
        public int MatchedRecords { get; set; }
        public IReadOnlyList<FilterRule> FiltersApplied { get; set; }
        public string MatchMode { get; set; }
        public IReadOnlyList<JsonElement> Data { get; set; }
    }
}
