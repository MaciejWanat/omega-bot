using System;
using System.Threading.Tasks;
using Discord;
using OmegaBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace OmegaBot.Services
{
    public class DiscordLogger : IDiscordLogger
    {
        private readonly ILogger<DiscordLogger> _logger;

        public DiscordLogger(ILogger<DiscordLogger> logger)
        {
            _logger = logger;
        }

        public Task LogDiscordMessage(LogMessage msg)
        {
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(msg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(msg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(msg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(msg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(msg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(msg.Message);
                    break;
            }
            return Task.CompletedTask;
        }

        public Task ApplicationLog(string message, LogSeverity logSeverity = LogSeverity.Info, Exception e = null)
            => LogDiscordMessage(new LogMessage(logSeverity, "Application", message, e));
    }
}
