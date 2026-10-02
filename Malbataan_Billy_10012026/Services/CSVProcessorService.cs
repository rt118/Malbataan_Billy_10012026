using CsvHelper; // Ensure this using directive is present
using CsvHelper.Configuration; // Ensure this using directive is present
using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using System;
using System.Globalization;

namespace Malbataan_Billy_10012026.Services
{
    public class CSVProcessorService : ICSVProcessorService
    {
        public async Task<CSVResponse> ProcessCSVFile(IFormFile file, string aggregate)
        {
            var op = aggregate.Trim().ToLowerInvariant();
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            // 3. Configure CsvHelper with standard invariant culture defaults
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null, // Prevents errors if columns are out of order
                MissingFieldFound = null
            };
            try
            {
                using var csv = new CsvReader(reader, config);
                var records = await csv.GetRecordsAsync<Employee>().ToListAsync();

                int totalEmployees = records.Count; 

                double result = op switch
                {
                    "avg" => records.Average(emp => emp.Salary),
                    "sum" => records.Sum(emp => emp.Salary),
                    "min" => records.Min(emp => emp.Salary),
                    "max" => records.Max(emp => emp.Salary),
                    _ => totalEmployees
                };

                return new CSVResponse
                {
                    Filename = file.FileName,
                    TotalRecords = totalEmployees,
                    Column = "Salary",
                    Aggregate = aggregate.Trim().ToLowerInvariant(),
                    Value = Math.Round(result, 6)
                };
            }
            catch (Exception ex)
            { 
                throw;
            }
        }
    }
}
