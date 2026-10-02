using Malbataan_Billy_10012026.Models;
using Malbataan_Billy_10012026.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Malbataan_Billy_10012026.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : Controller
    {
        private readonly IUploadTrackingService _tracker;

        public ReportController(IUploadTrackingService tracker)
        {
            _tracker = tracker;
        }
        /// <summary>
        /// File counters and the most recent processing log entries (newest first).
        /// Held in memory, so it resets when the app restarts.
        /// </summary>
        /// <param name="recent">How many recent entries to include (1 to 100, default 20).</param>
        [HttpGet]
        [ProducesResponseType<ProcessingReport>(StatusCodes.Status200OK)]
        public ActionResult<ProcessingReport> Get([FromQuery] int recent = 20) =>
            _tracker.GetReport(recent);
    }
}
