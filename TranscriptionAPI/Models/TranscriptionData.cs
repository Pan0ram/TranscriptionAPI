namespace TranscriptionAPI
{
    public class TranscriptionData
    {
        public TimeSpan StartSeconds { get; set; }
        public TimeSpan EndSeconds { get; set; }
        public string Transcript { get; set; } = String.Empty;
    }
}