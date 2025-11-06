namespace OmegaBot.Application.Services.Interfaces
{
    public interface IDishOfDayRepository
    {
        Task SaveImage(string imageUrl);
    }
}