using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace TranscriptionAPI.Controllers
{
    public class ErrorController : ControllerBase
    {
        protected readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        protected IActionResult HandleError(Exception ex)
        {
            _logger.LogError(ex, "Ein Fehler ist aufgetreten");
            return StatusCode(500, "Ein interner Serverfehler ist aufgetreten");
        }
    }
}