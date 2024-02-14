namespace TranscriptionAPI
{
    public class TranscriptionData
    {
        public int StartSeconds { get; set; }
        public int EndSeconds { get; set; }
        public string Transcript { get; set; } = String.Empty;
    }
}