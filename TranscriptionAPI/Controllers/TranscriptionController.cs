using Microsoft.AspNetCore.Mvc;

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
        public async Task<Transcription> Get(string youtubeURL)
        {
            var result = await _transcriptionService.GetTranscriptionFromYoutubeURL(youtubeURL);
            return result;
        }
    }
}
