using Malbataan_Billy_10012026.Filters;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Malbataan_Billy_10012026.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
       
 
        // POST api/<UploadFileController>
        [HttpPost("file")]
        [FileValidator] 
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file was uploaded or the file is empty.");
            }

            var safeFileName = Path.GetFileName(file.FileName);

            // 3. Process the file data stream
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            // TODO: Process or save the 'content' string here

            return Ok(new
            {
                Message = "File uploaded successfully via Controller!",
                FileName = safeFileName,
                SizeInBytes = file.Length
            });
        } 
    }
}
