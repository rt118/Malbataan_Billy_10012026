using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Malbataan_Billy_10012026.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FileValidatorAttribute : ActionFilterAttribute
    {
        private static readonly string[] _allowedExtensions = [".csv", ".json"]; 
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var files = context.ActionArguments.Values
             .OfType<IFormFile>()
             .Concat(context.ActionArguments.Values.OfType<IFormFileCollection>().SelectMany(x => x))
             .ToList();

            if (!files.Any())
            {
                context.Result = new BadRequestObjectResult(new { Error = "No file uploaded." });
                return;
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!_allowedExtensions.Contains(extension))
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        Error = "Invalid file type.",
                        Details = $"The file '{file.FileName}' is invalid. Only .csv and .json data documents are accepted."
                    });
                    return;
                }
            } 
            base.OnActionExecuting(context);
        }
    }
}
