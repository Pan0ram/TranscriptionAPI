namespace TranscriptionAPI
{
    public class Transcription
    {
        public DateTime Date { get; set; }

        public List<TranscriptionData> TranscriptionLines { get; set; } = new List<TranscriptionData>();
    }

    public class TranscriptionData
    {
        public int StartSeconds { get; set; }
        public int EndSeconds { get; set; }
        public string Transcript { get; set; } = String.Empty;
    }
}