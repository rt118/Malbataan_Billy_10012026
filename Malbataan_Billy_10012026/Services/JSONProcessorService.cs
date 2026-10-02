using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Reflection;
using System.Text.Json;

namespace Malbataan_Billy_10012026.Services
{
    public class JSONProcessorService : IJSONProcessorService
    {
        private static readonly HashSet<string> Operators = new(StringComparer.OrdinalIgnoreCase)
        {
            "eq", "ne", "gt", "gte", "lt", "lte", "in", "contains", "icontains"
        };

        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true // accepts "salary" and "Salary"
        };

        private static readonly Dictionary<string, PropertyInfo> Properties =
      typeof(Employee)
          .GetProperties(BindingFlags.Public | BindingFlags.Instance)
          .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        public string? Validate(FilterRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.Field)) return "'field' is required.";
            if (string.IsNullOrWhiteSpace(rule.Op)) return "'op' is required.";
            if (!Operators.Contains(rule.Op))return $"unknown op '{rule.Op}'. Allowed: {string.Join(", ", Operators.Order())}.";
            if (rule.Value.ValueKind == JsonValueKind.Undefined) return "'value' is required.";
            if (rule.Op.Equals("in", StringComparison.OrdinalIgnoreCase) &&rule.Value.ValueKind != JsonValueKind.Array) return "'in' requires an array value.";
            return null;
        }

        public async Task<JSONResponse> ProcessJsonFile(IFormFile file, FilterRule? rule)
        {
            using var fileStream = file.OpenReadStream();    
            var predicate = rule is null ? null : BuildPredicate(rule);
            var users = await JsonSerializer.DeserializeAsync<List<Employee>>(fileStream, options); 
            var result = predicate is null ? users : users.Where(predicate).ToList();  

            return new JSONResponse
            {
                Filename = file.FileName,
                TotalRecords = users?.Count ?? 0,
                MatchedRecords = result?.Count ?? 0,
                FiltersApplied = rule is null ? Array.Empty<FilterRule>() : new[] { rule }, 
                Data = result.Select(e => JsonSerializer.SerializeToElement(e, options)).ToList()
            };
        }
        

        private static Func<Employee, bool> BuildPredicate(FilterRule rule)
        {  
            if (!Properties.TryGetValue(rule.Field.Trim(), out var prop))
                throw new Exception(  $"Unknown field '{rule.Field}'. Allowed: {string.Join(", ", Properties.Keys)}."); 

            var op = (rule.Op ?? string.Empty).Trim().ToLowerInvariant();
            var type = prop.PropertyType;

            // Null values never match, whatever the operator.
            Func<Employee, bool> Single(Func<object, object, bool> test)
            {
                var expected = ConvertValue(rule.Value, prop);
                return e => prop.GetValue(e) is { } actual && test(actual, expected);
            }

            Func<Employee, bool> Ordered(Func<int, bool> test)
            {
                if (!typeof(IComparable).IsAssignableFrom(Nullable.GetUnderlyingType(type) ?? type))
                    throw new Exception($"'{op}' can't be used on '{prop.Name}' ({type.Name}).");

                return Single((actual, expected) => test(Compare(actual, expected)));
            }

            switch (op)
            {
                case "eq": return Single((a, v) => Equals(a, v));
                case "ne": return Single((a, v) => !Equals(a, v));

                case "gt": return Ordered(c => c > 0);
                case "gte": return Ordered(c => c >= 0);
                case "lt": return Ordered(c => c < 0);
                case "lte": return Ordered(c => c <= 0);

                case "in":
                    {
                        if (rule.Value.ValueKind != JsonValueKind.Array)
                            throw new Exception("'in' requires an array value.");

                        var allowed = rule.Value.EnumerateArray()
                            .Select(el => ConvertValue(el, prop))
                            .ToList();

                        return e => prop.GetValue(e) is { } actual && allowed.Any(x => Equals(actual, x));
                    }

                case "contains":
                case "icontains":
                    {
                        if (type != typeof(string))
                            throw new Exception(
                                $"'{op}' only works on text fields; '{prop.Name}' is {type.Name}.");

                        if (rule.Value.ValueKind != JsonValueKind.String)
                            throw new Exception($"'{op}' requires a string value.");

                        var needle = rule.Value.GetString() ?? string.Empty;
                        var comparison = op == "icontains"
                            ? StringComparison.OrdinalIgnoreCase
                            : StringComparison.Ordinal;

                        return e => prop.GetValue(e) is string s && s.Contains(needle, comparison);
                    }

                default:
                    throw new Exception(
                        $"Unknown op '{rule.Op}'. Allowed: eq, ne, gt, gte, lt, lte, in, contains, icontains.");
            }
        }


        private static object ConvertValue(JsonElement value, PropertyInfo prop)
        {
            try
            {
                var converted = value.Deserialize(prop.PropertyType, options);
                if (converted is not null) return converted;
            }
            catch (JsonException)
            {
                // fall through to the error below
            } 
            throw new Exception(
                $"Value {value.GetRawText()} isn't valid for '{prop.Name}' ({prop.PropertyType.Name}).");
        }
        

        private static int Compare(object actual, object expected) =>
            actual is string s
                ? string.CompareOrdinal(s, (string)expected) // culture-independent
                : ((IComparable)actual).CompareTo(expected); 
        }
    }
