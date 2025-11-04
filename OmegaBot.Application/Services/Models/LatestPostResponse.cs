namespace OmegaBot.Application.Services.Models
{
    public record LatestPostResponse
    {
        private LatestPostResponse()
        {
        }

        public static LatestPostResponse GetSuccessfulResponse(string title, string link, string imageUrl)
        {
            return new LatestPostResponse
            {
                FetchSuccessful = true,
                Title = title,
                Link = link,
                ImageUrl = imageUrl
            };
        }

        public static LatestPostResponse GetFailedResponse(string errorMessage)
        {
            return new LatestPostResponse
            {
                FetchSuccessful = false,
                ErrorMessage = errorMessage
            };
        }

        public bool FetchSuccessful { get; init; }
        public string ErrorMessage { get; init; }
        public string Title { get; init; }
        public string Link { get; init; }
        public string ImageUrl { get; init; }
    }
}
