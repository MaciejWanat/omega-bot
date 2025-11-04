using OmegaBot.Application.Services.Models;

namespace OmegaBot.Application.Services.Interfaces
{
    public interface IRssService
    {
        public Task<LatestPostResponse> GetLatestPostAsync();
    }
}