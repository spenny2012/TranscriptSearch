namespace YtTranscriptSearch.Shared
{
    public class TranscriptSearchProgress
    {
        public string? VideoTitle { get; set; }
        public Exception? Exception { get; set; }
        public TranscriptSearchMatch[]? Matches { get; set; }
    }
}
