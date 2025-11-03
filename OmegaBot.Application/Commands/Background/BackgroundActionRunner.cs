using Discord;
using OmegaBot.Commands.Background.BotActions;
using OmegaBot.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using NCrontab;

namespace OmegaBot.Commands.Background
{
    public class BackgroundActionRunner<T> : BackgroundService where T : IBackgroundBotAction
    {
        private readonly IDiscordLogger _logger;

        private readonly T _backgroundAction;

        public BackgroundActionRunner(IDiscordLogger logger, T backgroundAction)
        {
            _logger = logger;
            _backgroundAction = backgroundAction;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            switch (_backgroundAction)
            {
                case IBackgroundCronBotAction:
                    await ExecuteCronAction(cancellationToken);
                    break;
            }
        }

        private async Task ExecuteCronAction(CancellationToken cancellationToken)
        {
            var action = _backgroundAction as IBackgroundCronBotAction;
            var now = DateTime.Now;
            var checkIntervalMinutes = TimeSpan.FromMinutes(1).TotalMinutes;
            var schedule = CrontabSchedule.Parse(action.Cron);
            var nextRun = schedule.GetNextOccurrence(now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    now = DateTime.Now;

                    if (now > nextRun)
                    {
                        await _logger.ApplicationLog($"{now} - running background job - {action.Name}");

                        await action.ExecuteAction();
                        nextRun = schedule.GetNextOccurrence(now);
                    }
                }
                catch (Exception e)
                {
                    await _logger.ApplicationLog($"Error in background job - {action.Name}. {e.Message}", LogSeverity.Error);
                }

                await Task.Delay(TimeSpan.FromMinutes(checkIntervalMinutes), cancellationToken);
            }
        }
    }
}
