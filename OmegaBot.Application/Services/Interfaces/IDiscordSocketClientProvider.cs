using Discord.WebSocket;

namespace OmegaBot.Services.Interfaces
{
    public interface IDiscordSocketClientProvider
    {
        DiscordSocketClient GetDiscordSocketClient();
    }
}