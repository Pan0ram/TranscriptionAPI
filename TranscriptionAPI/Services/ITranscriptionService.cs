namespace TranscriptionAPI
{
    public interface ITranscriptionService
    {
        Task<Transcription> GetTranscriptionFromYoutubeURL(string youtubeURL);
    }
}