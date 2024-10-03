using System.Web;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.ClosedCaptions;
using System.Collections.Specialized;

namespace YtTranscriptSearch.Shared
{
    public class TranscriptSearch
    {
        public async Task SearchAsync(TranscriptSearchParameters searchParams, CancellationToken cancellationToken)
        {
            ValidateParams(searchParams);

            TranscriptSearchProgress currentProgress = new();

            try
            {
                await foreach (PlaylistVideo videoUpload in searchParams.YoutubeClient.Channels.GetUploadsAsync(searchParams.Channel.Id, cancellationToken))
                {
                    try
                    {
                        currentProgress = new TranscriptSearchProgress
                        {
                            VideoTitle = videoUpload.Title
                        };

                        NameValueCollection queryStringBuilder = HttpUtility.ParseQueryString(videoUpload.Url);
                        queryStringBuilder.Remove("list"); // remove parameter

                        string? clipTimestampUrl = queryStringBuilder.ToString();

                        ClosedCaptionManifest manifest = await searchParams.YoutubeClient.Videos.ClosedCaptions.GetManifestAsync(videoUpload.Id, cancellationToken);
                        ClosedCaptionTrackInfo trackInfo = manifest.GetByLanguage("en");
                        ClosedCaptionTrack track = await searchParams.YoutubeClient.Videos.ClosedCaptions.GetAsync(trackInfo, cancellationToken);

                        TranscriptSearchMatch[] foundMatches = track.Captions
                            .Where(caption => caption.Text.Contains(searchParams.SearchTerms, StringComparison.InvariantCultureIgnoreCase))
                            .Select(x => new TranscriptSearchMatch(videoUpload.Title, x.Text, $"{x.Offset.Hours}:{x.Offset.Minutes}:{x.Offset.Seconds}", $"{clipTimestampUrl}&t={(int)x.Offset.TotalSeconds}s"))
                            .ToArray();

                        if (foundMatches.Length > 0)
                        {
                            for (int i = 0; i < foundMatches.Length; i++)
                            {
                                TranscriptSearchMatch foundMatch = foundMatches[i];
                                searchParams.Matches.Add(foundMatch);
                            }
                        }

                        currentProgress.Matches = foundMatches;
                    }

                    catch (TaskCanceledException)
                    {
                        return;
                    }

                    catch (Exception ex)
                    {
                        currentProgress.Exception = ex;
                    }

                    searchParams.Progress.Report(currentProgress);
                }
            }

            catch (TaskCanceledException)
            {
                return;
            }
        }

        static void ValidateParams(TranscriptSearchParameters searchParams)
        {
            if (searchParams.YoutubeClient == null || searchParams.Channel == null || searchParams.Matches == null || searchParams.Progress == null || string.IsNullOrWhiteSpace(searchParams.SearchTerms))
                throw new Exception(nameof(searchParams));
        }
    }
}