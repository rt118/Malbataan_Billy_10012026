using System.Text.Json;

namespace Malbataan_Billy_10012026.Models
{
    /// <summary>A .json file (array of objects) to filter, or a .csv file (with a header row) to aggregate.</summary>
    public class UploadRequest
    { 
        /// <summary>
        /// JSON uploads only: JSON array of rules, e.g. [{"field":"name","op":"eq","value":"John"}]. 
        /// </summary>
        public string? Filters { get; set; }

        /// <summary>CSV uploads only: avg, sum, min, max or count. Defaults to "avg".</summary>
        public string? Aggregate { get; set; } = "avg";
    }

    /// <summary>One filter condition. Field supports dot notation (e.g. "address.city").</summary>
    public record FilterRule(string Field, string Op, JsonElement Value); 

}
