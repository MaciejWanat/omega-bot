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
            if (action is null)
            {
                return;
            }

            // Normalize cron expressions that may omit the seconds/fields (accept both 4- and 5-field crons).
            var cronExpression = action.Cron?.Trim() ?? throw new InvalidOperationException("Cron expression is null");
            var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4)
            {
                cronExpression = cronExpression + " *";
            }
            else if (parts.Length != 5)
            {
                throw new FormatException($"Invalid cron expression: '{action.Cron}'");
            }

            var schedule = CrontabSchedule.Parse(cronExpression);
            var nextRun = schedule.GetNextOccurrence(DateTime.Now);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    var delay = nextRun - now;

                    if (delay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay, cancellationToken);
                    }
                    else if (delay < TimeSpan.Zero)
                    {
                        nextRun = schedule.GetNextOccurrence(DateTime.Now);
                        continue;
                    }

                    await _logger.ApplicationLog($"{DateTime.Now} - running background job - {action.Name}");
                    await action.ExecuteAction();

                    nextRun = schedule.GetNextOccurrence(nextRun);

                    if (nextRun <= DateTime.Now)
                    {
                        nextRun = schedule.GetNextOccurrence(DateTime.Now);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Shutdown requested, exit loop gracefully.
                    break;
                }
                catch (Exception e)
                {
                    await _logger.ApplicationLog($"Error in background job - {action.Name}. {e.Message}", LogSeverity.Error);
                }
            }
        }
    }
}