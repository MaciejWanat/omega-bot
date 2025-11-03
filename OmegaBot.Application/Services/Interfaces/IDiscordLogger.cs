using System;
using System.Threading.Tasks;
using Discord;

namespace OmegaBot.Services.Interfaces
{
    public interface IDiscordLogger
    {
        Task LogDiscordMessage(LogMessage msg);
        Task ApplicationLog(string message, LogSeverity logSeverity = LogSeverity.Info, Exception e = null);
    }
}