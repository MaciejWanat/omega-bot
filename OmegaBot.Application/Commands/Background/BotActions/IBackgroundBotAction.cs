using System.Threading.Tasks;

namespace OmegaBot.Commands.Background.BotActions
{
    public interface IBackgroundBotAction
    {
        string Name => GetType().Name;

        Task ExecuteAction();
    }
}
