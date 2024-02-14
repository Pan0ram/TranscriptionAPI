namespace TranscriptionAPI
{
    public class Transcription
    {
        public DateTime Date { get; set; }

        public List<TranscriptionData> TranscriptionLines { get; set; } = new List<TranscriptionData>();
    }
}