using Microsoft.AspNetCore.Mvc;
using TranscriptionAPI.Model;
using TranscriptionAPI.Services;

namespace TranscriptionAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILogger<TranscriptionController> _logger;

        public TranscriptionController(ILogger<TranscriptionController> logger,
            ITranscriptionService transcriptionService)
        {
            _logger = logger;
            _transcriptionService = transcriptionService;
        }

        [HttpGet(Name = "GetTranscription")]
        public IActionResult Get(string youtubeURL)
        {
            try
            {
                var result = _transcriptionService.GetTranscriptionFromYoutubeURL(youtubeURL);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return HandleError(ex);
            }
        }

        private IActionResult HandleError(Exception ex)
        {
            throw new NotImplementedException();
        }

        [HttpGet(Name = "GetTranscription")]
        public Transcription Get(string youtubeURL)
        {
            var result = _transcriptionService.GetTranscriptionFromYoutubeURL(youtubeURL);
            return result;
        }
    }
}
