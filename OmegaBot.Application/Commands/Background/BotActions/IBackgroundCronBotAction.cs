namespace OmegaBot.Commands.Background.BotActions
{
    public interface IBackgroundCronBotAction : IBackgroundBotAction
    {
        string Cron { get; }
    }
}
