using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using OmegaBot.Application.Services.Interfaces;

namespace OmegaBot.Application.Services
{
    public class RssService : IRssService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;
        private string? _lastItemId;

        public RssService(IHttpClientFactory httpClientFactory, AppSettings appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
        }

        public async Task<(string title, string link, string? imageUrl)?> GetLatestPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var feed = await FeedReader.ReadAsync(_appSettings.RssFeed);

            var latest = feed.Items.FirstOrDefault();
            if (latest == null) return null;

            if (latest.Id == _lastItemId)
            {
                return null;
            }
            _lastItemId = latest.Id;

            var itemMediaRss = (MediaRssFeedItem)latest.SpecificItem;
            var imgUrl = itemMediaRss.Media.FirstOrDefault()?.Url;
            return (latest.Title, latest.Link, imgUrl);
        }
    }
}
