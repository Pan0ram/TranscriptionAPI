namespace TranscriptionAPI
{
    public interface ITranscriptionService
    {
        Transcription GetTranscriptionFromYoutubeURL(string youtubeURL);
    }
}