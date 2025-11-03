using Discord;
using Discord.WebSocket;
using OmegaBot.Application.Services.Interfaces;
using OmegaBot.Services.Interfaces;

namespace OmegaBot.Commands.Background.BotActions
{
    public class FetchMealOfTheDayAction : IBackgroundCronBotAction
    {
        private readonly AppSettings _appSettings;

        private readonly DiscordSocketClient _client;

        private readonly IDiscordLogger _logger;

        private readonly IRssService _rssService;

        public string Cron { get; }

        public FetchMealOfTheDayAction(
            AppSettings appSettings,
            IDiscordSocketClientProvider discordSocketClientProvider, 
            IDiscordLogger logger,
            IRssService rssService)
        {
            _appSettings = appSettings;
            _logger = logger;
            _client = discordSocketClientProvider.GetDiscordSocketClient();
            Cron = appSettings.FetchCron;
            _rssService = rssService;
        }

        public async Task ExecuteAction()
        {
            try
            {
                await _logger.ApplicationLog("Starting fetching for meal of the day");

                var latest = await _rssService.GetLatestPostAsync();
                if (latest != null)
                {
                    var channel = _client.GetChannel(_appSettings.PostChannelId) as IMessageChannel;
                    if (channel != null)
                    {
                        var embed = new EmbedBuilder()                         
                            .WithTitle("Omeguniowe danie dnia")
                            .WithUrl(latest.Value.link)
                            .WithColor(Color.Blue)
                            .WithImageUrl(latest.Value.imageUrl)
                            .WithFooter("Źródło: Facebook (Omega Poznań)")
                            .Build();

                        await channel.SendMessageAsync("Wake up babe, new Danie Dnia just dropped", embed: embed);
                    }
                }
            }
            catch (Exception e)
            {
                await _logger.ApplicationLog($"Error while trying to fetch for meal of the day. Error message: {e.Message}", LogSeverity.Error);
            }
        }
    }
}
