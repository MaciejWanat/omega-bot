namespace OmegaBot.Application.Services.Interfaces
{
    public interface IRssService
    {
        Task<(string title, string link, string? imageUrl)?> GetLatestPostAsync();
    }
}