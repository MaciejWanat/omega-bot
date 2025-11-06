using OmegaBot.Application.Services.Interfaces;
using OmegaBot.Services.Interfaces;
using System.Diagnostics;

namespace OmegaBot.Infrastructure
{
    public class DishOfDayRepository : IDishOfDayRepository
    {
        private readonly AppSettings _appSettings;
        private readonly IDiscordLogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public DishOfDayRepository(AppSettings appSettings, IHttpClientFactory httpClientFactory, IDiscordLogger logger)
        {
            _appSettings = appSettings;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SaveImage(string imageUrl)
        {
            await _logger.ApplicationLog($"Saving image from url: {imageUrl}");

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (!Directory.Exists(_appSettings.ImagesDirectory))
            {
                Directory.CreateDirectory(_appSettings.ImagesDirectory);
            }

            using var httpClient = _httpClientFactory.CreateClient();

            // Send HEAD request to check size before downloading
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, imageUrl);
            using var headResponse = await httpClient.SendAsync(headRequest);
            headResponse.EnsureSuccessStatusCode();

            // Check file size
            var maxSizeBytes = _appSettings.MaxImageSizeMb * 1024 * 1024;

            var contentLength = headResponse.Content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value > maxSizeBytes)
            {
                throw new InvalidOperationException(
                    $"File size ({contentLength.Value / 1024 / 1024:F2} MB) exceeds maximum allowed size of {_appSettings.MaxImageSizeMb} MB");
            }

            // Get file extension from URL or Content-Type
            string extension = GetFileExtension(imageUrl, headResponse.Content.Headers.ContentType?.MediaType);

            // Generate filename with extension
            var date = DateTime.Now.Date.ToString("yyyy-MM-dd");
            var fileName = $"{date}-{Guid.NewGuid()}{extension}";

            if (fileName.Length > 150)
            {
                fileName = fileName[..150] + extension;
            }
            string filePath = Path.Combine(_appSettings.ImagesDirectory, fileName);

            // Download the actual file
            using var response = await httpClient.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await stream.CopyToAsync(fileStream);

            stopWatch.Stop();
            var ts = stopWatch.Elapsed;

            await _logger.ApplicationLog(
                    $"Saving image completed in {ts:g}");
        }

        private string GetFileExtension(string url, string contentType)
        {
            // Try to get extension from URL first
            string extension = Path.GetExtension(new Uri(url).LocalPath);

            if (!string.IsNullOrEmpty(extension))
            {
                return extension;
            }

            // Fallback to Content-Type mapping
            return contentType switch
            {
                "image/jpeg" => ".jpg",
                "image/jpg" => ".jpg",
                "image/png" => ".png",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                "image/bmp" => ".bmp",
                "image/svg+xml" => ".svg",
                _ => ".jpg" // Default fallback
            };
        }
    }
}
