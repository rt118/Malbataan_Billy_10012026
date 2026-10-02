using System.Text.Json;

namespace Malbataan_Billy_10012026.Models
{
    /// <summary>A .json file (array of objects) to filter, or a .csv file (with a header row) to aggregate.</summary>
    public class UploadRequest
    { 
     
        public string? Filters { get; set; } // JSON uploads only: JSON array of rules, e.g. [{"field":"name","op":"eq","value":"John"}].

        public string? Aggregate { get; set; } = "avg"; // CSV uploads only: avg, sum, min, max or count
    }

    /// <summary>One filter condition. Field supports dot notation (e.g. "address.city").</summary>
    public record FilterRule(string Field, string Op, JsonElement Value); 

}
