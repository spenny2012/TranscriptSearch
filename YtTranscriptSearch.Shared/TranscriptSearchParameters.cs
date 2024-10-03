using YoutubeExplode;
using YoutubeExplode.Channels;

namespace YtTranscriptSearch.Shared
{
    public class TranscriptSearchParameters
    {
        public string? SearchTerms { get; set; }
        public YoutubeClient? YoutubeClient { get; set; }
        public Channel? Channel { get; set; }
        public IList<TranscriptSearchMatch>? Matches { get; set; } 
        public IProgress<TranscriptSearchProgress>? Progress { get; set; }
    }
}
