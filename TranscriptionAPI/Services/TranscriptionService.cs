using TranscriptionAPI.Modul;

namespace TranscriptionAPI.Services
{
    public class TranscriptionService : ITranscriptionService
    {
        public Transcription GetTranscriptionFromYoutubeURL(string youtubeURL)
        {
            // 1. Take YoutubeURL and extract audio from Youtube with OpenCV

            // 2. Use OpenAI's Whisper language model to transcript the video (async)

            // 2.5 Optional Translate

            // 3. Return Transcription

            Transcription model = new Transcription()
            {
                Date = DateTime.Now,
                TranscriptionLines = new List<TranscriptionData>
                {
                    new TranscriptionData() { StartSeconds = 0, EndSeconds = 2, Transcript = "Lorem ipsum dolor sit amet, consetetur sad" },
                    new TranscriptionData() { StartSeconds = 2, EndSeconds = 5, Transcript = "cusam et justo duo dolores et ea rebum. Stet clita kasd" },
                    new TranscriptionData() { StartSeconds = 5, EndSeconds = 2, Transcript = "dolor sit amet. Lorem ipsum dolor sit am" },
                    new TranscriptionData() { StartSeconds = 0, EndSeconds = 8, Transcript = " erat, sed diam voluptua. At vero e" }
                }
            };

            return model;
        }
    }

    public interface ITranscriptionService
    {
        Transcription GetTranscriptionFromYoutubeURL(string youtubeURL);
    }
}
