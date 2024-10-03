using YoutubeExplode;
using YoutubeExplode.Channels;

namespace YtTranscriptSearch.Shared
{
    public class TranscriptSearchHelpers
    {

        /// <summary>
        /// Validate URI or Youtube channel name and create an instance of TranscriptSearchParameters
        /// </summary>
        /// <param name="client">YoutubeClient</param>
        /// <param name="channel">Youtube Channel Name</param>
        /// <param name="searchTerms">Search Terms</param>
        /// <returns>TranscriptSearchParameters</returns>
        public static async Task<TranscriptSearchParameters> CreateAsync(YoutubeClient client, string channel, string searchTerms)
        {
            if (client == null) throw new NullReferenceException(nameof(client));
            if (string.IsNullOrWhiteSpace(channel)) throw new NullReferenceException(nameof(channel));
            if (string.IsNullOrWhiteSpace(searchTerms)) throw new NullReferenceException(nameof(searchTerms));

            string? fullUrl;

            if (Uri.TryCreate(channel, new UriCreationOptions(), out Uri? _))
            {
                fullUrl = channel;
            }

            else
            {
                if (channel.StartsWith('@'))
                {
                    fullUrl = $"https://youtube.com/{channel}";
                }

                else
                {
                    fullUrl = $"https://youtube.com/@{channel}";
                }

                if (!Uri.TryCreate(fullUrl, new UriCreationOptions { }, out Uri? _))
                {
                    throw new Exception($"Invalid Uri '{channel}'");
                }
            }

            Channel foundChannel = await client.Channels.GetByHandleAsync(fullUrl);

            return new TranscriptSearchParameters
            {
                Channel = foundChannel,
                SearchTerms = searchTerms,
                Matches = new List<TranscriptSearchMatch>(),
                YoutubeClient = client
            };
        }
    }
}
