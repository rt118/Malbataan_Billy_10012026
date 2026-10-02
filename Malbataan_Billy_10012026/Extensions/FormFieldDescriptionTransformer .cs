using Malbataan_Billy_10012026.Models;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.ComponentModel;
using System.Reflection;

namespace Malbataan_Billy_10012026.Extensions
{

    /// <summary>
    /// Workaround for the .NET 10 XML-comment generator, which copies the LAST form property's
    /// summary onto the whole request body and leaves the individual fields undocumented.
    /// Document transformers run after every operation is built, so this has the final say:
    /// it sets each form field's description from its [Description] attribute and replaces
    /// the request body description.
    /// </summary>
    internal sealed class FormFieldDescriptionTransformer : IOpenApiDocumentTransformer
    {
        private const string FormContentType = "multipart/form-data";
        private const string BodyDescription = "Upload a .json file to filter, or a .csv file to aggregate.";

        // Read once: property name -> [Description] text (matched case-insensitively,
        // because form field names can be PascalCase or camelCase in the document).
        private static readonly Dictionary<string, string> Descriptions =
            typeof(UploadRequest)
                .GetProperties()
                .Select(p => (p.Name, Text: p.GetCustomAttribute<DescriptionAttribute>()?.Description))
                .Where(x => x.Text is not null)
                .ToDictionary(x => x.Name, x => x.Text!, StringComparer.OrdinalIgnoreCase);

        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            foreach (var pathItem in document.Paths.Values)
            {
                if (pathItem.Operations is null) continue;

                foreach (var operation in pathItem.Operations.Values)
                {
                    if (operation.RequestBody is not OpenApiRequestBody body ||
                        body.Content is null ||
                        !body.Content.TryGetValue(FormContentType, out var media) ||
                        media.Schema?.Properties is not { } properties)
                        continue;

                    var matched = false;
                    foreach (var (name, schema) in properties)
                    {
                        if (schema is OpenApiSchema fieldSchema &&
                            Descriptions.TryGetValue(name, out var text))
                        {
                            fieldSchema.Description = text;
                            matched = true;
                        }
                    }

                    // Replace the description .NET copied from the last property.
                    if (matched) body.Description = BodyDescription;
                }
            }

            return Task.CompletedTask;
        }
    }
}
